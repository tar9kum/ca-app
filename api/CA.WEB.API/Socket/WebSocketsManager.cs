using System;
using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;
using System.Collections.Generic;
using System.Linq;
using CA.WEB.API.Interface;
using CA.WEB.API.Model;

namespace CA.WEB.API.Socket
{
    
    public class WebSocketsManager : IWebSocketManager, IDisposable
    {
        private static readonly ConcurrentDictionary<string, WebSocket> _connections = new ConcurrentDictionary<string, WebSocket>();
        private static readonly ConcurrentDictionary<string, ConcurrentBag<int>> _subscriptions = new ConcurrentDictionary<string, ConcurrentBag<int>>(); // Track subscriptions
        private readonly ILogger<WebSocketManager> _logger;
        private readonly IStockPriceMonitor _stockPriceMonitor;
        private bool _disposed = false;

        public WebSocketsManager(ILogger<WebSocketManager> logger, IStockPriceMonitor stockPriceMonitor)
        {
            _logger = logger;
            _stockPriceMonitor = stockPriceMonitor;
            _stockPriceMonitor.StockUpdated += OnStockUpdated;
        }


        /// <summary>
        /// For this application this will work well, however on for very large number of connection, we need to refactor this or may look for an 
        /// alternative solution. May be reverse lookup structure: Instead of storing subscriptions per connection, store connections per stock.
        /// to achieve high scalability may be decoupling the subscription from the connection and use distribute cache, also we can 
        /// decouple WebSocketManager with StockPriceMonitor, and use a message broker or a pub/sub system.
        /// </summary>
        private async void OnStockUpdated(object? sender, Stock updatedStock)
        {            
            var priceUpdate = new Stock { Id= updatedStock.Id, Price = updatedStock.Price, UpdatedAt = updatedStock.UpdatedAt };
            var jsonPriceUpdate = JsonSerializer.Serialize(priceUpdate);
            var bytes = Encoding.UTF8.GetBytes(jsonPriceUpdate);

            foreach (var (connectionId, socket) in _connections)
            {
                if (_subscriptions.TryGetValue(connectionId, out var subscribedStocks) &&
                    subscribedStocks.Contains(updatedStock.Id) &&
                    socket.State == WebSocketState.Open)
                {
                    try
                    {
                        await socket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, CancellationToken.None);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Error broadcasting stock update to the client {connectionId}: {ex}");
                    }
                }
            }
        }


        /// <summary>
        /// Handle incoming WebSocket connections
        /// This method is called when a new WebSocket connection is established.
        /// It accepts the WebSocket connection and starts processing messages.
        /// </summary>
        public async Task HandleWebSocketAsync(HttpContext context, WebSocket webSocket)
        {
            var socketId = Guid.NewGuid().ToString();
            _connections.TryAdd(socketId, webSocket);
            _subscriptions.TryAdd(socketId, new ConcurrentBag<int>()); 
            _logger.LogInformation($"Client with Id: {socketId} connected with {_connections.Count} no of connections");

            try
            {
                await ProcessRequest(webSocket, socketId);
            }
            catch (WebSocketException ex)
            {
                if (ex.Message.Contains("Connection closed", StringComparison.OrdinalIgnoreCase) ||
                    ex.Message.Contains("Endpoint not available", StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogInformation($"Client {socketId} disconnected gracefully.");
                }
                else
                {
                    _logger.LogError($"WebSocketException for client {socketId}: {ex}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception for client {socketId}: {ex}");
            }
            finally
            {
                _connections.TryRemove(socketId, out _);
                _subscriptions.TryRemove(socketId, out _); // Clean up subscriptions
                await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Connection closed", CancellationToken.None);
                _logger.LogInformation($"Client {socketId} disconnected. Total connections: {_connections.Count}");
            }
        }


        /// <summary>
        // Start the stock price update monitoring
        // Effecicny of this method is very crucial, it processes message for each connection.
        // we could look into a throttling mechanism to limit the rate of updates sent to clients.
        // Also could implement a backoff strategy to reduce the frequency of updates when the server is under heavy load.
        // Implement a message queue to decouple the WebSocket communication from the stock price updates.
        // look into horizontal scaling
        /// </summary>
        private async Task ProcessRequest(WebSocket webSocket, string socketId)
        {
            var buffer = new byte[1024 * 4];
            WebSocketReceiveResult wsResult;

            // Loop to receive messages from the client
            while (webSocket.State == WebSocketState.Open)
            {
                // Receive the message
                wsResult = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                //we could support more types of messages, for this app this is will do.
                if (wsResult.MessageType == WebSocketMessageType.Text)
                {
                    var message = Encoding.UTF8.GetString(buffer, 0, wsResult.Count);
                    _logger.LogInformation($"Message received from {socketId}: {message}");

                    // For this application, we only expect "subscribe" and "unsubscribe" messages.
                    // for example: "subscribe:1" or "unsubscribe:1"
                    // The stock ID should be an integer.
                    // we can extend this to support more commands in the future.
                    // we can support more types of messages in the future.
                    if (message.StartsWith("subscribe:"))
                    {
                        var stockIdString = message.Substring("subscribe:".Length);
                        if (int.TryParse(stockIdString, out var stockId))
                        {
                            if (_subscriptions.TryGetValue(socketId, out var subscribedStocks))
                            {
                                subscribedStocks.Add(stockId);
                                _logger.LogInformation($"This {socketId} has been subscribed to the stock {stockId}");
                                // Send the current stock price immediately upon subscription
                                var stock = _stockPriceMonitor.GetStock(stockId); // Get the stock.
                                //var jsonStock = JsonSerializer.Serialize(stock);
                                var bytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(stock));
                                await webSocket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, CancellationToken.None);
                            }
                        }
                        else
                        {
                            // Log the error and send a message to the client
                            _logger.LogWarning($" Format of the stock ID : {stockIdString} is invalid, We will support more format in upcoming release.");
                            await webSocket.SendAsync(Encoding.UTF8.GetBytes("Format of the stock ID is invalid."), WebSocketMessageType.Text, true, CancellationToken.None);
                        }
                    }
                    // Unsubscribe message
                    else if (message.StartsWith("unsubscribe:"))
                    {
                        // Unsubscribe from a stock
                        var stockIdString = message.Substring("unsubscribe:".Length);
                        if (int.TryParse(stockIdString, out var stockId))
                        {
                            if (_subscriptions.TryGetValue(socketId, out var subscribedStocks))
                            {
                                if (subscribedStocks.Contains(stockId))
                                {
                                    //  Remove the stock ID from the subscription list
                                    var newList = subscribedStocks.Where(id => id != stockId).ToList();
                                    _subscriptions[socketId] = new ConcurrentBag<int>(newList);

                                    _logger.LogInformation($"This {socketId} unsubscribed from stock {stockId}");
                                }
                                else
                                {
                                    _logger.LogWarning($"This {socketId} was not subscribed to stock {stockId}");
                                }
                            }
                        }
                        else
                        {                           
                            _logger.LogWarning($" Format of the stock ID : {stockIdString} is invalid, We will support more format in upcoming release.");
                            await webSocket.SendAsync(Encoding.UTF8.GetBytes("Invalid stock ID format."), WebSocketMessageType.Text, true, CancellationToken.None);
                        }
                    }
                    else
                    {
                        _logger.LogWarning($"Message from {socketId}: {message} is not known.");
                        await webSocket.SendAsync(Encoding.UTF8.GetBytes($"Message unknown: {message}"), WebSocketMessageType.Text, true, CancellationToken.None);
                    }
                }
                else if (wsResult.MessageType == WebSocketMessageType.Close)
                {
                    await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Initiated client close", CancellationToken.None);
                }
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            // Dispose of managed resources
            if (!_disposed)
            {
                if (disposing)
                {
                    _stockPriceMonitor.StockUpdated -= OnStockUpdated;
                }
                _disposed = true;
            }
        }

        public void Dispose()
        {
            //  Dispose of unmanaged resources
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}