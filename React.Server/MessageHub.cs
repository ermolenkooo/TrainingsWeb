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

        public MessageHub(MyOptions options, MessageManager messageManager)
        {
            _options = options;
            _messageManager = messageManager;
        }

        public async Task Start(string message)
        {
            await _options.scadaVConnection1.CreateArchiveHost(_options.Settings.ArchiveIp);
            await _options.scadaVConnection2.CreateArchiveHost(_options.Settings.Archive2Ip);
            await _options.scadaVConnection3.CreateArchiveHost(_options.Settings.Archive3Ip);

            await _options.scadaVConnection1.CreateServerHost(_options.Settings.ArchiveIp);
            await _options.scadaVConnection2.CreateServerHost(_options.Settings.Archive2Ip);
            await _options.scadaVConnection3.CreateServerHost(_options.Settings.Archive3Ip);

            await _messageManager.StartConnection(Convert.ToInt32(message), this, _options);
        }

        public async Task End(string message)
        {
            _messageManager.HandlerEndTrainingAsync();
        }
    }
}

