using System.Reflection;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using SagaFlow;
using SagaFlow.Schema;

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
        public static IApplicationBuilder UseSagaFlow(this IApplicationBuilder app, Action<SagaFlowUiOptionsBuilder>? optionsBuilder = null)
        {
            var module = app.ApplicationServices.GetRequiredService<SagaFlowModule>();
            module.ServiceProvider = app.ApplicationServices;
            var options = new SagaFlowUiOptionsBuilder(module);
            optionsBuilder?.Invoke(options);

            foreach (var startupAction in module.SageFlowStartup)
            {
                startupAction.Invoke(app);
            }
            
            var provider = new ManifestEmbeddedFileProvider(Assembly.GetAssembly(typeof(SagaFlowModule))! , "UI");
            var sagaflowUiIndexPage = ReplaceSagaFlowDefaultApi(provider.GetFileInfo("index.html"), module);
            var path = new PathString("/" + module.ApiBasePath);
            var path2 = new PathString("/" + module.ApiBasePath + "/");

            foreach (var frontEndPlugin in options.EmbeddedResourcePlugins)
            {
                app.UseStaticFiles(new StaticFileOptions
                {
                    RequestPath = frontEndPlugin.PluginFolderPath,
                    FileProvider = frontEndPlugin.PluginFileProvider
                });
            }
            //return app.UseMiddleware<SagaFlowMiddleware>();
            return app.Use(async (context, next) =>
            {
                if (context.Request.Method == "GET")
                {
                    if (context.Request.Path == path || context.Request.Path == path2)
                    {
                        await context.Response.SendFileAsync(sagaflowUiIndexPage);
                        return;
                    }
                    
                    /*
                    foreach (var frontEndPlugin in options.EmbeddedResourcePlugins)
                    {
                        if (context.Request.Path.StartsWithSegments(frontEndPlugin.PluginFolderPath, out var remainingPath))
                        {
                            await context.Response.SendFileAsync(frontEndPlugin.GetFile(remainingPath));
                            return;
                        }
                    }
                    */
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

    public class SagaFlowUiOptionsBuilder
    {
        private readonly SagaFlowModule sagaFlowModule;

        internal SagaFlowUiOptionsBuilder(SagaFlowModule sagaFlowModule)
        {
            this.sagaFlowModule = sagaFlowModule;
        }
        
        internal IList<EmbeddedResourcePlugin> EmbeddedResourcePlugins { get; } = new List<EmbeddedResourcePlugin>();

        public SagaFlowUiOptionsBuilder AddEmbeddedResourcePlugin(string pluginId, Assembly assembly, string rootPath, string entryPointJsfile)
        {
            var provider = new ManifestEmbeddedFileProvider(assembly, rootPath);
            var pluginFile = provider.GetFileInfo(entryPointJsfile);
            if (!pluginFile.Exists)
                throw new InvalidOperationException($"The embedded resource plugin {pluginId} does not contain the entry point file {entryPointJsfile}");
            
            var pluginFolderHostPath = new PathString($"/{sagaFlowModule.ApiBasePath}/plugins/{pluginId}");
            EmbeddedResourcePlugins.Add(new EmbeddedResourcePlugin(pluginFolderHostPath, provider));
            sagaFlowModule.RegisterFrontEndPlugin(new FrontEndPlugin(pluginId, pluginFolderHostPath + $"/{entryPointJsfile}"));
            return this;
        }

        public SagaFlowUiOptionsBuilder AddHostedPlugin(string pluginId, string entryPointUrl)
        {
            sagaFlowModule.RegisterFrontEndPlugin(new FrontEndPlugin(pluginId, entryPointUrl));
            return this;
        }

        public SagaFlowUiOptionsBuilder AddSidebarComponent(string pluginId, string label, string icon, string pluginComponentId)
        {
            var plugin = sagaFlowModule.FrontEndPlugins.GetValueOrDefault(pluginId) 
                         ?? throw new ArgumentOutOfRangeException(nameof(pluginId), $"The plugin {pluginId} is not registered");
            plugin.RegisterSidebarComponent(label, icon, pluginComponentId);
            return this;
        }

        internal class EmbeddedResourcePlugin
        {
            private readonly ManifestEmbeddedFileProvider pluginFileProvider;

            public EmbeddedResourcePlugin(PathString pluginFolderPath, ManifestEmbeddedFileProvider pluginFileProvider)
            {
                this.pluginFileProvider = pluginFileProvider;
                PluginFolderPath = pluginFolderPath;
            }

            public PathString PluginFolderPath { get; }
            
            public ManifestEmbeddedFileProvider PluginFileProvider => pluginFileProvider;

            public IFileInfo GetFile(PathString pluginPath)
            {
                return pluginFileProvider.GetFileInfo(pluginPath);
            }
        }
    }
}
