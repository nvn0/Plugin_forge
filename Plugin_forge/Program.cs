using System;
using System.Threading;

namespace Plugin_forge
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!!!!!");
            string minha = "INPUT -p tcp --dport 22 -j DROP";

            Firewall firewall = new Firewall();
            //firewall.AddRule(minha);
			firewall.AllowSshIn();
        }
    }
}
