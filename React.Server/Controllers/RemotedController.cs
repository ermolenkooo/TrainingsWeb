using Microsoft.AspNetCore.Mvc;

namespace React.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RemotedController : ControllerBase
    {
        private MessageHub _hub;

        //public RemotedController(MessageHub hub)
        //{
        //    _hub = hub;
        //}

        [HttpGet("{taskId}/true/changetaskshedulers")]
        public async Task StartTaskShedulers(int taskId)
        {
            
        }

        [HttpGet("{taskId}/false/changetaskshedulers")]
        public async Task StopTaskShedulers()
        {

        }
    }
}
