using Microsoft.AspNetCore.Mvc;
using Rebus.Bus;

namespace SagaFlow.MvcProvider
{
    [ApiController]
    // Route attribute is required for ApiController's, but replaced at runtime
    // via MvcEndpointRouteConvention to resolve custom sagaflow parameters.
    [Route("[sagaflow-base-path]/commands/[command-type]")]
    public class CommandController<T> : ControllerBase where T : class, new()
    {
        private readonly SagaFlowModule module;
        private readonly IBus bus;

        public CommandController(SagaFlowModule module, IBus bus)
        {
            this.module = module;
            this.bus = bus;
        }

        [HttpGet]
        public Task<IActionResult> Get()
        {
            return Post(new T());
        }

        [HttpGet("{id}")]
        public Task<IActionResult> Get(Guid id)
        {
            return Post(new T());
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] T value)
        {
            // Offer config over whether to send commands as a direct message? or always publish as event?
            await bus.Send(value);
//            await bus.Publish(value);
            return Ok();
        }
    }
}
