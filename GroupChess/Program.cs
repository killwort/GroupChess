using System;
using System.IO;
using System.IO.Compression;
using Autofac;
using FB.Web.Utils.App;

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
    }
}