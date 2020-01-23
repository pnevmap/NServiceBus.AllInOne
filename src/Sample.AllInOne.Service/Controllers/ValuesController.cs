using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using NServiceBus;
using Sample.AllInOne.Service.Commands;
using Sample.AllInOne.Service.DataAccess;

namespace Sample.AllInOne.Service.Controllers
{
    [ApiController, Route("[controller]")]
    public class ValuesController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly ApplicationDataContext _repository;
        private readonly IMessageSession _bus;

        public ValuesController(IMapper mapper, ApplicationDataContext repository, IMessageSession bus)
        {
            _mapper = mapper;
            _repository = repository;
            _bus = bus;
        }


        [HttpGet("{id}")]
        [ProducesResponseType(200, Type = typeof(string))]
        [ProducesResponseType(404)]
        public async Task<ActionResult<string>> Get(int id)
        {
            var value = await _repository.Get(id);

            if (value != null)
                return value.Value;

            return new NotFoundResult();
        }

        [HttpPost]
        [Consumes("text/plain")]
        [ProducesResponseType(202)]
        public async Task<ActionResult> Post([FromBody] string value)
        {
            await _bus.Send(new AddValue
            {
                Value = value
            });
            
            return new AcceptedResult();
        }

        [HttpPut("{id}")]
        [Consumes("text/plain")]
        [ProducesResponseType(202)]
        public async Task<ActionResult> Put(int id, [FromBody] string value)
        {
            await _bus.Send(new UpdateValue
            {
                Id = id,
                Value = value
            });
            
            return new AcceptedResult();
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(202)]
        public async Task<ActionResult> Delete(int id)
        {
            await _bus.Send(new DeleteValue
            {
                Id = id
            });
            
            return new AcceptedResult();
        }
    }
}