using BLL;
using BLL.Models;
using BLL.Operations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace React.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReportsController : ControllerBase
    {
        private ReportOperations _reportOperations;
        public ReportsController()
        {
            _reportOperations = new ReportOperations();
        }

        [HttpPost("{type}")]
        public async Task<IActionResult> CreateReport1(int type, [FromBody] Report report)
        {
            string filePath;
            switch(type)
            {
                case 1:
                    {
                        filePath = Path.Combine(Directory.GetCurrentDirectory(), "Files", "Отчёт по противоаварийной тренировке.txt");
                        break;
                    }
                case 2:
                    {
                        filePath = Path.Combine(Directory.GetCurrentDirectory(), "Files", "Отчёт по анализу тренировки пуска и останова.txt");
                        break;
                    }
                default: return NotFound();
            }
            Directory.CreateDirectory(Path.GetDirectoryName(filePath));
            string content = _reportOperations.CreateReport(type, report);
            await System.IO.File.WriteAllTextAsync(filePath, content);
            return Ok($"Файл создан: {filePath}");
        }
    }
}
