using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Rebus.Activation;
using Rebus.Config;
using Rebus.ServiceProvider;
using SagaFlow;

namespace Microsoft.AspNetCore.Builder
{
    public static class SagaFlowBuilderExtensions
    {
        /// <summary>
        /// Register the Workflow middleware
        /// </summary>
        public static IApplicationBuilder UseSagaFlow(
            this IApplicationBuilder app,
            string apiBasePath = "sagaflow")
        {
            app.ApplicationServices.UseRebus(bus =>
            {
                var sfm = app.ApplicationServices.GetService<SagaFlowModule>();
                return Task.WhenAll(sfm.Commands.Select(c => bus.Subscribe(c.CommandType)));
            });

            return app; //.UseMiddleware<SagaFlowMiddleware>();
        }
    }
}
