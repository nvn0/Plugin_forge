using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace PortController
{
    internal class Firewall
    {


		/*
			IPTables é constituido por três tabelas (Filter table, Nat Table e Mangle Table)



			Filter table tem ainda três chains (INPUT chain, OUTPUT Chain e FORWARD chain)

			INPUT chain -> trafego que entra

			Output chaint -> trafego que sai

			FORWARD chain -> reencaminhar pacotes para outros dispositivos (ex. é como se fosse um router)


			Esta classe foca apenas na Filter Table para filtrar trafego inbound e outbound.



		 */






		// função para de defenir uma regra personalizada
        /*
        public void AddRule(string rule)
        {
            ExecuteCommand($"sudo iptables -A {rule} && \"sudo /sbin/iptables-save"); // -A para adicionar no fundo da lista ou -I para adicionar ao topo da lista
			//ExecuteCommand($"sudo /sbin/iptables-save");
        }
        */

        public void OpenPort(string protocol, string port)
        {
            ExecuteCommand($"sudo iptables -A  INPUT -p {protocol} --dport {port} -j ACCEPT && sudo /sbin/iptables-save");
        }

        public void ClosePort( string protocol, string port)
        {
            ExecuteCommand($"sudo iptables -A  INPUT -p {protocol} --dport {port} -j DROP && sudo /sbin/iptables-save");
        }



        // Esta função define a regra para aceitar conexoes ssh de entrada de qualquer ip
        public void AllowSshIn()
		{
			ExecuteCommand($"sudo iptables -I INPUT -p tcp --dport 22 -j ACCEPT");
			ExecuteCommand($"sudo /sbin/iptables-save");
		}
		

		//função para defenir o comportamento default para negar o trafego que entra
		public void DenyAllIn()
		{
			ExecuteCommand($"sudo iptables --policy INPUT DROP");
			ExecuteCommand($"sudo /sbin/iptables-save");
		}



        /*
		função para apagar regra especifica com base o numero
		(NOTA: No iptables as regras são numeradas, se uma regras for apagada todas as
		outras na sequencia baixam uma posição na lista)		
		*/
        public void DeleteRuleNumber(string rule_number)
        {
            ExecuteCommand($"sudo iptables -D INPUT {rule_number}");
            ExecuteCommand($"sudo /sbin/iptables-save");
        }


        //---------------------------------------------------------------------------------------------------------
        //                                                      NFTABLES
        //---------------------------------------------------------------------------------------------------------



        public void NFOpenPort(string protocol, string port)
        {
            ExecuteCommand($"sudo add rule inet filter input {protocol} dport {port} accept");


        }


        public void NFClosePort(string protocol, string port)
        {
            ExecuteCommand($"sudo add rule inet filter input {protocol} dport {port} drop");

        }

        //-----------------------------------------------------------------------------------------------------------------------------


        public void AddRule(string action, string firewall, string protocol, string port, string rule = "")
        {




            if (firewall == "ipt" && action == "OpenPort")
            {
                OpenPort(protocol, port);
            }
            else if (firewall == "ipt" && action == "ClosePort")
            {
                ClosePort(protocol, port);
            }
            else if (firewall == "nft" && action == "OpenPort")
            {
                NFOpenPort(protocol, port);
            }
            else if (firewall == "nft" && action == "ClosePort")
            {
                NFClosePort(protocol, port);
            }
            else if (firewall == "ipt" && action == "ExecCmd" && rule != "")
            {
                ExecuteCommand($"sudo iptables -A {rule} && sudo /sbin/iptables-save");
                //ExecuteCommand($"lxc exec {container_name} -- sudo /sbin/iptables-save");
            }
            else if (firewall == "nft" && action == "ExecCmd" && rule != "")
            {

            }

        }



        // função para criar um processo e executar comandos
        private string ExecuteCommand(string command)
        {
            try
            {
                var processInfo = new ProcessStartInfo("bash", $"-c \"{command}\"")
                {
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                var process = Process.Start(processInfo);
                if (process != null)
                {
                    string output = process.StandardOutput.ReadToEnd();
                    string error = process.StandardError.ReadToEnd();
                    process.WaitForExit();
                    if (!string.IsNullOrWhiteSpace(error))
                    {
                        throw new Exception(error);
                    }
                    return output;
                }
                else
                {
                    throw new Exception("Failed to start process.");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to execute command: {command}. {ex.Message}");
            }


        }
    }
}
