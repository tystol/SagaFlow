using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Rebus.Bus;
using Rebus.Messages;

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
            var commandId = Guid.NewGuid();
            // Offer config over whether to send commands as a direct message? or always publish as event?
            var headers = new Dictionary<string, string> {{Headers.CorrelationId, Guid.NewGuid().ToString()}};
            await bus.Send(value, headers);
//            await bus.Publish(value);
            return Ok();
        }
    }
}
