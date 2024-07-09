using System;
using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BLL;
using BLL.Models;
using BLL.Operations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace React.Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WebSocketController : ControllerBase
    {
        private MyOptions _options;
        React.Server.WebSocketManager _webSocketManager;

        public WebSocketController(MyOptions options, React.Server.WebSocketManager webSocketManager)
        {
            _options = options;
            _webSocketManager = webSocketManager;
        }

        [HttpGet("/ws/{id}")]
        public async Task Get(int id)
        { 
            if (HttpContext.WebSockets.IsWebSocketRequest)
            {
                using var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
                //webSocketManager = new React.Server.WebSocketManager(webSocket, _options);
                await _webSocketManager.HandleWebSocketConnection(id, webSocket, _options);
            }
            else
            {
                HttpContext.Response.StatusCode = 400;
            }
        }

        [HttpGet("/api/stop/{id}")]
        public async Task Stop(int id)
        {
            _webSocketManager.HandlerEndTrainingAsync();
        }
    }
}