using Nancy.Hosting.Self;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace retaildata
{
    class Program
    {

        static void Main(string[] args)
        {
            var dataManager = new DataManager();
            var workers = new Workers(dataManager);
            Web.workers = workers;

            var config = new HostConfiguration
            {
                UrlReservations = new UrlReservations { CreateAutomatically = true }
            };
            using (var host = new NancyHost(config, new Uri("http://localhost:1234")))
            {
                host.Start();
                Console.WriteLine("Running on http://localhost:1234");
                Console.ReadLine();
            }
        }
    }
}
