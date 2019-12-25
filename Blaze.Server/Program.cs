using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Blaze.Server
{
    class Program
    {
        static void Main(string[] args)
        {
            Log.Initialize("Blaze.Server.log");
            Log.Info("Starting Blaze.Server...");

            Time.Initialize();


            BlazeHubServer.Start();
            BlazeServer.Start();
            MagmaServer.Start();
            Console.ReadLine();
        }
    }
}
