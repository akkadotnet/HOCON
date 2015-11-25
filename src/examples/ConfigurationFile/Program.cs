using Akka.Configuration;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConfigurationFile
{
    class Program
    {
        static void Main(string[] args)
        {
            var section = ConfigurationManager.GetSection("testsection") as TestConfigurationSection;
            var config = ConfigurationFactory.FromConfigurationElementCollection(section.HoconList);

            string resulta = config.GetString("root.simple-string-a");
            string resultb = config.GetString("root.simple-string-b");
            string resultc = config.GetString("root.simple-string-c");

            Console.WriteLine("root.simple-string-a: " + resulta);
            Console.WriteLine("root.simple-string-b: " + resultb);
            Console.WriteLine("root.simple-string-c: " + resultc);

            Console.ReadKey();
        }
    }
}
