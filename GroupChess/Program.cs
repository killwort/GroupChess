using System;
using System.IO;
using System.IO.Compression;
using Autofac;
using FB.Discovery;
using FB.Settings;
using FB.Web.Utils.App;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;

namespace GroupChess
{
    class Program:SimpleApp {
        public static MarkovChain NamingChain;
        static void Main(string[] args) => AppHost<Program>(args, "office-chess");
        protected override void RegisterServices(ContainerBuilder builder)
        {
            base.RegisterServices(builder);
            builder.RegisterType<GameStore>().SingleInstance().AsSelf();
            using(var fs=File.OpenRead(Path.Combine(Directory.GetCurrentDirectory(),"Content","voina-i-mir.bin")))
            using (var gs = new GZipStream(fs, CompressionMode.Decompress))
                builder.RegisterInstance(NamingChain = new MarkovChain(gs));
        }

        public override void Configure(IApplicationBuilder app,
                                       IHostingEnvironment env,
                                       ILoggerFactory loggerFactory,
                                       IApplicationLifetime lifetime,
                                       IDiscoveryService discovery,
                                       Settings<DeploymentSettings> deploymentSettings,
                                       Settings<CorsSettings> corsSettings) {
            app.Use(
                async (context, next) => {
                    Console.WriteLine(context.Request.Path);
                    await next.Invoke();
                });
            base.Configure(app, env, loggerFactory, lifetime, discovery, deploymentSettings, corsSettings);
        }

    }
}