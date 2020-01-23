using Shared.Hosting.Abstractions;
using Shared.Web.Bootstrapping;

namespace Sample.AllInOne.Service
{
    internal static class Program
    {
        private static int Main(string[] args)
        {
            return ServiceHost.Run(args, "All-In-One Service", new WebService<Startup>(args));
        }
    }
}