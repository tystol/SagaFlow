using System;
using System.IO;
using System.Reflection;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using SagaFlow;

namespace Microsoft.AspNetCore.Builder
{
    public static class SagaFlowBuilderExtensions
    {
        // The placeholder used by SagaFlow UI for the default path, we will replace this text with the 
        // default or custom route supplied with SagaFlow was added to the Services Collection.
        private const string sagaFlowUiDefaultApiPathPlaceHolder = "__default_saga_flow_route_placeholder__";
        
        /// <summary>
        /// Register the Workflow middleware
        /// </summary>
        public static IApplicationBuilder UseSagaFlow(
            this IApplicationBuilder app)
        {
            var module = app.ApplicationServices.GetRequiredService<SagaFlowModule>();
            var provider =
                new ManifestEmbeddedFileProvider(assembly: Assembly.GetAssembly(typeof(SagaFlowModule)), "UI");
            var path = new PathString("/" + module.ApiBasePath);
            var path2 = new PathString("/" + module.ApiBasePath + "/");

            var webComponentsPath = new PathString($"/{module.ApiBasePath}/web-components.js");

            //return app.UseMiddleware<SagaFlowMiddleware>();
            return app.Use(async (context, next) =>
            {
                if (context.Request.Method == "GET" && (context.Request.Path == path || context.Request.Path == path2))
                {
                    await context.Response.SendFileAsync(ReplaceSagaFlowDefaultApi(provider.GetFileInfo("index.html"), module));
                    return;
                }

                if (context.Request.Method == "GET" && (context.Request.Path == webComponentsPath))
                {
                    context.Response.ContentType = "text/javascript";
                    await context.Response.SendFileAsync(ReplaceSagaFlowDefaultApi(provider.GetFileInfo("web-components.js"), module));
                    return;
                }

                // Call the next delegate/middleware in the pipeline.
                await next(context);
            });
        }

        /// <summary>
        /// Replaces the SagaFlow.UI placeholder for the SagaFlow route with the route defined when ServiceCollection.AddSagaFlow(..)
        /// was called.
        /// </summary>
        /// <param name="fileInfo"></param>
        /// <param name="sagaFlowModule"></param>
        /// <returns></returns>
        private static IFileInfo ReplaceSagaFlowDefaultApi(IFileInfo fileInfo, SagaFlowModule sagaFlowModule) {
            using var stream = fileInfo.CreateReadStream();
            using var streamReader = new StreamReader(stream);

            var content = streamReader.ReadToEnd();
            
            return new SagaFlowFileInfo(fileInfo.Name, content.Replace(sagaFlowUiDefaultApiPathPlaceHolder, sagaFlowModule.ApiBasePath));
        }

        private class SagaFlowFileInfo : IFileInfo
        {
            private readonly string _name;
            private readonly string _context;

            public SagaFlowFileInfo(string name, string context)
            {
                _name = name;
                _context = context;
            }
            
            public Stream CreateReadStream()
            {
                return new MemoryStream(Encoding.UTF8.GetBytes(_context));
            }

            public bool Exists => true;
            public bool IsDirectory => false;
            public DateTimeOffset LastModified => DateTimeOffset.Now;
            public long Length => _context.Length;
            public string Name => _name;
            public string PhysicalPath => null;
        }
    }
}
