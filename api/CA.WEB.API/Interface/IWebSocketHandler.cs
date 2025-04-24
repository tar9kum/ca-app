
using System.Net.WebSockets;

namespace CA.WEB.API.Interface
{
    public interface IWebSocketManager
    {
        Task HandleWebSocketAsync(HttpContext context, WebSocket webSocket);
    }
}