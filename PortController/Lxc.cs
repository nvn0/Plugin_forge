using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace PortController
{
    internal class Lxc:Container
    {

        //---------------------------------------------------------------------------------------------------------
        //                                                      IPTABLES
        //---------------------------------------------------------------------------------------------------------

        public void AddRule(string container_name, string rule)
        {
            ExecuteCommand($"lxc exec {container_name} -- sudo iptables -A {rule}");
            ExecuteCommand($"lxc exec {container_name} -- sudo /sbin/iptables-save");
        }

        public void OpenPort(string container_name, string port)
        {
            ExecuteCommand($"lxc exec {container_name} -- sudo iptables -A  INPUT -p tcp --dport {port} -j ACCEPT");
            ExecuteCommand($"lxc exec {container_name} -- sudo /sbin/iptables-save");
        }

        public void ClosePort(string container_name, string port)
        {
            ExecuteCommand($"lxc exec {container_name} -- sudo iptables -A  INPUT -p tcp --dport {port} -j DROP");
            ExecuteCommand($"lxc exec {container_name} -- sudo /sbin/iptables-save");
        }


        // Esta função define a regra para aceitar conexoes ssh de entrada de qualquer ip
        public void AllowSshIn(string container_name)
        {
            ExecuteCommand($"lxc exec {container_name} -- sudo iptables -I INPUT -p tcp --dport 22 -j ACCEPT");
            ExecuteCommand($"lxc exec {container_name} -- sudo /sbin/iptables-save");
        }

        


        //---------------------------------------------------------------------------------------------------------
        //                                                      NFTABLES
        //---------------------------------------------------------------------------------------------------------



        public void NFOpenPort(string container_name, string port) 
        {
            ExecuteCommand($"lxc exec {container_name} --sudo add rule inet filter input tcp dport {port} accept");


        }


        public void NFClosePort(string container_name, string port)
        {
            ExecuteCommand($"lxc exec {container_name} --sudo add rule inet filter input tcp dport {port} drop");

        }









        //---------------------------------------------------------------------------------------------------------
        //                                                      Get INFO commands
        //---------------------------------------------------------------------------------------------------------

        public string GetInfo(string container_name)
        {
            string output = ExecuteCommand($"lxc exec {container_name} -- sudo iptables -L");
            return output;

        }


        /*
        public string GetInfo(Lxc c)
        {
            string output = ExecuteCommand($"lxc exec {c.Name} -- sudo iptables -L");
            return output;

        }
        */

        public List<string> GetPorts()
        {
            //string output = ExecuteCommand($"lxc exec {this.Name} -- sudo iptables -L");
            string output = ExecuteCommand($"lxc exec {this.Nome} -- sudo netstat -tulpn | grep LISTEN | awk '{{print $4}}' | cut -f2 -d':' ");
           
            string[] linhasArray = output.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

            this.Portas = new List<string>(linhasArray);

            return Portas;

        }


        public List<string> GetPorts2() // apenas pra testar
        {
            //string output = ExecuteCommand($"sudo iptables -L");
            string output = ExecuteCommand($"sudo netstat -tulpn | grep LISTEN | awk '{{print $4}}' | cut -f2 -d':' ");
            string[] linhasArray = output.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);


            this.Portas = new List<string>(linhasArray);

            foreach (string line in Portas)
            {

                Console.WriteLine(line);
            }

            return Portas;

        }

        public string[] GetPorts3() // apenas pra testar
        {
            //string output = ExecuteCommand($"sudo iptables -L");
            string output = ExecuteCommand($"sudo netstat -tulpn | grep LISTEN | awk '{{print $4}}' | cut -f2 -d':' ");
            string[] linhasArray = output.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);


            return linhasArray;

        }


        //---------------------------------------------------------------------------------------------------------
        //                                                      API - LXC
        //---------------------------------------------------------------------------------------------------------



        // Importa a função lxc_attach_run_command da biblioteca compartilhada do LXC
        [DllImport("liblxc", CallingConvention = CallingConvention.Cdecl)]
        public static extern int lxc_attach_run_command(string name, string[] argv);



        // Executar comandos no container
        public void ApiCommand(string container_name)
        {

            string[] command = { "/bin/bash", "-c", "sudo iptables -L" };

            int result = lxc_attach_run_command(container_name, command);
            if (result != 0)
            {
                Console.WriteLine("Erro ao executar o comando dentro do contêiner");
                return;
            }

            Console.WriteLine("Comando executado dentro do contêiner com sucesso");







        }











    }
}
