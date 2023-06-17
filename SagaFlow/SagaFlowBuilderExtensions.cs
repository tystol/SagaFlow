using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
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
            this IApplicationBuilder app)
        {
            var module = app.ApplicationServices.GetRequiredService<SagaFlowModule>();
            var provider = new ManifestEmbeddedFileProvider(assembly: Assembly.GetAssembly(typeof(SagaFlowModule)), "UI");
            var path = new PathString("/" + module.ApiBasePath);
            var path2 = new PathString("/" + module.ApiBasePath + "/");
            
            app.ApplicationServices.UseRebus(bus =>
            {
                var sfm = app.ApplicationServices.GetService<SagaFlowModule>();
                return Task.WhenAll(sfm.Commands.Select(c => bus.Subscribe(c.CommandType)));
            });
            //return app.UseMiddleware<SagaFlowMiddleware>();
            return app.Use(async (context, next) =>
            {
                if (context.Request.Method == "GET" && (context.Request.Path == path || context.Request.Path == path2))
                {
                    await context.Response.SendFileAsync(provider.GetFileInfo("index.html"));
                    return;
                }
                // Call the next delegate/middleware in the pipeline.
                await next(context);
            });

        }
    }
}
