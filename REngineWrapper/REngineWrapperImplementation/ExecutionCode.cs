using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using RManaged.Core;

namespace RManaged.RExecutionService
{
    class Program
    {
        static void Main(string[] args)
        {
            var REngineThreadExecutor = new RExecutionThread(args[0]);
            Console.ReadLine();
        }
    }
}
