using Microsoft.AspNetCore.Mvc;
using BLL.Models;
using BLL;
using BLL.Operations;

namespace React.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TrainingsController : ControllerBase
    {
        private TrainingDbOperations _db;

        public TrainingsController(MyOptions options)
        {
            _db = new TrainingDbOperations(options);
        }

        [HttpGet]
        public List<TrainingModel> Get()
        {
            return _db.SelectAllTrainings();
        }

        [HttpGet("{id}")]
        public TrainingModel Get(int id)
        {
            return _db.SelectTrainingById(id);
        }

        //// POST api/<PurchasesController>
        //[HttpPost]
        //public int Post([FromBody] Purchase value)
        //{
        //    return value.add();
        //}

        //// PUT api/<PurchasesController>/5
        //[HttpPut("{id}")]
        //public void Put(int id, [FromBody] string value)
        //{
        //}

        //// DELETE api/<PurchasesController>/5
        //[HttpDelete("{id}")]
        //public void Delete(int id)
        //{
        //}
    }
}
