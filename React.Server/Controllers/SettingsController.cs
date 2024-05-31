using Microsoft.AspNetCore.Mvc;
using BLL.Models;
using BLL;
using BLL.Operations;

namespace React.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SettingsController : ControllerBase
    {
        private Settings settings;

        public SettingsController()
        {
            settings = new Settings();
        }

        [HttpGet]
        public Settings Get()
        {
            settings.ReadSettingsFromFile();
            return settings;
        }

        [HttpPost]
        public void Post([FromBody] Settings value)
        {
            settings.SaveSettings(value);
        }
    }
}
