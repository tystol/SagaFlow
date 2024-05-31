using System.Reflection;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
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

            var webComponentsPath = new PathString($"/{module.ApiBasePath}/web-components.js");
            
            //return app.UseMiddleware<SagaFlowMiddleware>();
            return app.Use(async (context, next) =>
            {
                if (context.Request.Method == "GET" && (context.Request.Path == path || context.Request.Path == path2))
                {
                    await context.Response.SendFileAsync(provider.GetFileInfo("index.html"));
                    return;
                }
                
                if (context.Request.Method == "GET" && (context.Request.Path == webComponentsPath))
                {
                    context.Response.ContentType = "text/javascript";
                    await context.Response.SendFileAsync(provider.GetFileInfo("web-components.js"));
                    return;
                }
                // Call the next delegate/middleware in the pipeline.
                await next(context);
            });
        }
    }
}
