using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

namespace Grand.Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = WebHost.CreateDefaultBuilder(args)
                .UseKestrel(options => options.AddServerHeader = false)
                .CaptureStartupErrors(true)
                .UseSetting(WebHostDefaults.PreventHostingStartupKey, "true")
                .UseStartup<Startup>()
                .Build();

            host.Run();
        }
    }
}