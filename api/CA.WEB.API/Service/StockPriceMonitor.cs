using System;
using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using CA.WEB.API.Interface;
using CA.WEB.API.Model;

namespace CA.WEB.API.Service
{
    /// <summary>
    /// This class simulates a stock price monitor that updates stock prices at regular intervals.
    /// </summary>
    public class StockPriceMonitor : IStockPriceMonitor, IDisposable
    {
        private readonly ConcurrentDictionary<int, Stock> _stocks = new ConcurrentDictionary<int, Stock>();
        private Timer? _priceUpdateTimer;
        private readonly Random _random = new Random();
        private bool _disposed = false;
        private bool _isUpdating = false;

        public event EventHandler<Stock>? StockUpdated;

        public StockPriceMonitor()
        {
            // Initialize sample stocks
            _stocks.TryAdd(1, new Stock { Id = 1, Name = "Tesla", Price = 122.00m, UpdatedAt = DateTime.UtcNow });
            _stocks.TryAdd(2, new Stock { Id = 2, Name = "Amazon", Price = 150.00m, UpdatedAt = DateTime.UtcNow });
            _stocks.TryAdd(3, new Stock { Id = 3, Name = "Palantir", Price = 90.00m, UpdatedAt = DateTime.UtcNow });
            _stocks.TryAdd(4, new Stock { Id = 4, Name = "Rocket Lab", Price = 20.00m, UpdatedAt = DateTime.UtcNow });
            _stocks.TryAdd(5, new Stock { Id = 5, Name = "Meta", Price = 200.00m, UpdatedAt = DateTime.UtcNow });
            _stocks.TryAdd(6, new Stock { Id = 6, Name = "Google", Price = 220.00m, UpdatedAt = DateTime.UtcNow });
            _stocks.TryAdd(7, new Stock { Id = 7, Name = "Soundhound", Price = 10.00m, UpdatedAt = DateTime.UtcNow });
            _stocks.TryAdd(8, new Stock { Id = 8, Name = "Archer Aviation", Price = 8.00m, UpdatedAt = DateTime.UtcNow });
            _stocks.TryAdd(9, new Stock { Id = 9, Name = "Robinhood", Price = 90.00m, UpdatedAt = DateTime.UtcNow });
            _stocks.TryAdd(10, new Stock { Id = 10, Name = "Sofi", Price = 20.00m, UpdatedAt = DateTime.UtcNow });
        }

        public Stock GetStock(int id)
        {
            _stocks.TryGetValue(id, out var stock);
            return stock ?? throw new ArgumentException($"Stock Id {id} not found.");
        }

        public void StartUpdatingPrices()
        {
            if (!_isUpdating)
            {
                _priceUpdateTimer = new Timer(UpdatePrices, null, TimeSpan.Zero, TimeSpan.FromSeconds(1));
                _isUpdating = true;
            }
        }

        public void StopUpdatingPrices()
        {
            if (_isUpdating && _priceUpdateTimer != null)
            {
                _priceUpdateTimer.Dispose();
                _priceUpdateTimer = null;
                _isUpdating = false;
            }
        }


        private void UpdatePrices(object? state)
        {
            if (_disposed) return;

            //can impleneted the Throttling/debouncing here, if/when needed.

            foreach (var key in _stocks.Keys)
            {
                if (_stocks.TryGetValue(key, out var stock))
                {
                    var change = (decimal)(_random.Next(-10, 11) / 100.0); // +/- 0.10
                    stock.Price += change;
                    stock.UpdatedAt = DateTime.UtcNow;
                    _stocks.TryUpdate(key, stock, _stocks[key]); 
                    OnStockUpdated(stock);
                }
            }
        }

        /// This method is called when a stock is updated.
        protected virtual void OnStockUpdated(Stock updatedStock)
        {
            StockUpdated?.Invoke(this, updatedStock);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing && _priceUpdateTimer != null)
                {
                    _priceUpdateTimer.Dispose();
                    _priceUpdateTimer = null;
                }
                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}