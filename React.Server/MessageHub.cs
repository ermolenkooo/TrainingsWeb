using BLL.Operations;
using BLL;
using Microsoft.AspNetCore.SignalR;
using BLL.Models;
using System.Collections.Concurrent;
using DAL.Entities;
using Microsoft.AspNetCore.Http;

namespace React.Server
{
    public class MessageHub : Hub
    {
        private MyOptions _options;
        MessageManager _messageManager;
        private IHubContext<MessageHub> _hubContext;

        public MessageHub(MyOptions options, MessageManager messageManager, IHubContext<MessageHub> hubContext)
        {
            _options = options;
            _messageManager = messageManager;
            _hubContext = hubContext;
        }

        public async Task Start(string message)
        {
            await _options.scadaVConnection1.CreateArchiveHost(_options.Settings.ArchiveIp);
            await _options.scadaVConnection2.CreateArchiveHost(_options.Settings.Archive2Ip);
            await _options.scadaVConnection3.CreateArchiveHost(_options.Settings.Archive3Ip);

            await _options.scadaVConnection1.CreateServerHost(_options.Settings.ArchiveIp);
            await _options.scadaVConnection2.CreateServerHost(_options.Settings.Archive2Ip);
            await _options.scadaVConnection3.CreateServerHost(_options.Settings.Archive3Ip);

            _messageManager.SetSettings(_hubContext, _options, false);
            await Task.Delay(5000);
            await _messageManager.StartConnection(Convert.ToInt32(message));
        }

        public async Task End()
        {
            _messageManager.HandlerEndTrainingAsync();
        }
    }
}

