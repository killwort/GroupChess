using Autofac;
using FB.Web.Utils.App;

namespace GroupChess
{
    class Program:SimpleApp
    {
        static void Main(string[] args) => AppHost<Program>(args, "office-chess");
        protected override void RegisterServices(ContainerBuilder builder)
        {
            base.RegisterServices(builder);
            builder.RegisterType<GameStore>().SingleInstance().AsSelf();
        }
    }
}