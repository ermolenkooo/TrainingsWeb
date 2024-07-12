using BLL;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace React.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RemotedController : ControllerBase
    {
        private readonly IHubContext<MessageHub> _hubContext;
        private MyOptions _options;
        MessageManager _messageManager;

        public RemotedController(IHubContext<MessageHub> hubContext, MyOptions options, MessageManager messageManager)
        {
            _hubContext = hubContext;
            _options = options;
            _messageManager = messageManager;
        }

        [HttpGet("{taskId}/true/changetaskshedulers")]
        public async Task<IActionResult> StartTaskShedulers(int taskId)
        {
            _messageManager.SetSettings(_hubContext, _options, true);
            if (_messageManager.CheckTraining(taskId))
            {

                await _options.scadaVConnection1.CreateArchiveHost(_options.Settings.ArchiveIp);
                await _options.scadaVConnection2.CreateArchiveHost(_options.Settings.Archive2Ip);
                await _options.scadaVConnection3.CreateArchiveHost(_options.Settings.Archive3Ip);

                await _options.scadaVConnection1.CreateServerHost(_options.Settings.ArchiveIp);
                await _options.scadaVConnection2.CreateServerHost(_options.Settings.Archive2Ip);
                await _options.scadaVConnection3.CreateServerHost(_options.Settings.Archive3Ip);

                await _hubContext.Clients.All.SendAsync("RemovedStart");

                await _messageManager.StartConnection(taskId);
                return Ok();
            }
            else
                return NotFound();
        }

        [HttpGet("{taskId}/false/changetaskshedulers")]
        public IActionResult StopTaskShedulers()
        {
            if (_messageManager.StatusTraining != 2)
            {
                _messageManager.HandlerEndTrainingAsync();
                return Ok();
            }
            else
                return NotFound();
        }
    }
}
