using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {
            string ck = Console.ReadLine();
            while (!string.IsNullOrEmpty(ck))
            {
                if (ck == "1")
                {
                    Clyconf.Net.Core.ConfigManager.Instance.InitClient();
                }
                else
                {
                    Clyconf.Net.Core.ConfigManager.Instance.InitServer();
                }

                ck = Console.ReadLine();
            }
        }
    }
}
