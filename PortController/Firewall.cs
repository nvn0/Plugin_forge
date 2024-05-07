using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Net.Sockets;
using System.Runtime.InteropServices.JavaScript;

namespace PortController
{
    internal class Firewall
    {

        // Esta class interage diretamente no sistema host

        /*
			IPTables é constituido por três tabelas (Filter table, Nat Table e Mangle Table)



			Filter table tem ainda três chains (INPUT chain, OUTPUT Chain e FORWARD chain)

			INPUT chain -> trafego que entra

			Output chaint -> trafego que sai

			FORWARD chain -> reencaminhar pacotes para outros dispositivos (ex. é como se fosse um router)


			Esta classe foca apenas na Filter Table para filtrar trafego inbound e outbound.



		 */

        // Constantes
        private const string host_ip = "192.168.1.80";
        private const string bridge_interface = "lxdbr0";




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
            ExecuteCommand($"doas iptables -A  INPUT -p {protocol} --dport {port} -j ACCEPT && doas /sbin/iptables-save");
        }

        public void ClosePort( string protocol, string port)
        {
            ExecuteCommand($"doas iptables -A  INPUT -p {protocol} --dport {port} -j DROP && doas /sbin/iptables-save");
        }



        // Esta função define a regra para aceitar conexoes ssh de entrada de qualquer ip
        public void AllowSshIn()
		{
			ExecuteCommand($"doas iptables -I INPUT -p tcp --dport 22 -j ACCEPT");
			ExecuteCommand($"doas /sbin/iptables-save");
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
            ExecuteCommand($"doas add rule inet filter input {protocol} dport {port} accept");


        }


        public void NFClosePort(string protocol, string port)
        {
            ExecuteCommand($"doas add rule inet filter input {protocol} dport {port} drop");

        }



        //---------------------------------------------------------------------------------------------------------
        //                                      LXC Netwok - criar comandos de forward
        //---------------------------------------------------------------------------------------------------------



        public void Lxc_forward(string bridge_interface, string host_ip, string porta_exterior, string porta_container, string ip_container, string protocol)
        {
            //ExecuteCommand($"doas lxc network forward create {bridge_interface} {host_ip}"); // -> executar apena uma vez
            ExecuteCommand($"doas lxc network forward port add {bridge_interface} {host_ip} {protocol} {porta_exterior} {ip_container} {porta_container}");

        }













        //---------------------------------------------------------------------------------------------------------
        //                                       IPTABLES - criar comandos na tabela NAT
        //---------------------------------------------------------------------------------------------------------





        public void criar_ligação(string porta_exterior, string porta_container, string ip_container, string protocol)
        {
            ExecuteCommand($"doas iptables -t nat -I PREROUTING -p {protocol} --dport {porta_exterior} -j DNAT --to-destination {ip_container}:{porta_container} && doas /sbin/iptables-save");

        }













        //---------------------------------------------------------------------------------------------------------
        //                                                      API - LXC 
        //---------------------------------------------------------------------------------------------------------


        // Caminho -> /var/lib/lxd/unix.socket


        public void Lxd_api_forward(string req_type, string bridge_interface) //criar comandos de forward
        {

            // GET /1.0/networks/{networkName}/forwards
            // GET /1.0/networks/{networkName}/forwards?recursion=1
            // GET / 1.0 / networks /{ networkName}/ forwards /{ listenAddress}
            // POST / 1.0 / networks /{ networkName}/ forwards
            //PATCH /1.0/networks/{networkName}/forwards/{listenAddress}


            try
            {
                // Endereço do Unix socket
                string socketPath = "/var/lib/lxd/unix.socket";

                // Criar um novo socket Unix
                using (var socket = new Socket(AddressFamily.Unix, SocketType.Stream, ProtocolType.Unspecified))
                {
                    // Conectar ao Unix socket
                    ConnectToUnixSocket(socket, socketPath);

                    if (req_type == "GET")
                    {
                        // Enviar uma solicitação HTTP GET
                        string requestPath = $"/1.0/networks/{bridge_interface}/forwards";
                        string request = $"GET /1.0/networks/{bridge_interface}/forwards HTTP/1.1\r\nHost: dummy\r\n\r\n";

                        // Enviar a solicitação
                        byte[] requestBytes = Encoding.UTF8.GetBytes(request);
                        socket.Send(requestBytes);

                    }
                    else if (req_type == "POST")
                    {
                        // Enviar uma solicitação HTTP POST

                        // Conteúdo que deseja enviar no corpo do POST
                        string postData = "param1=value1&param2=value2";

                        // Construir a solicitação POST
                        string networkName = "nome_da_rede";
                        string requestPath = $"/1.0/networks/{networkName}/forwards";
                        string request = $"POST {requestPath} HTTP/1.1\r\nHost: dummy\r\nContent-Length: {postData.Length}\r\n\r\n{postData}";

                        byte[] requestBytes = Encoding.UTF8.GetBytes(request);
                        socket.Send(requestBytes);

                    }

                    // Receber resposta do socket
                    byte[] receiveBuffer = new byte[1024];
                    int receivedBytes = socket.Receive(receiveBuffer);
                    string responseData = Encoding.UTF8.GetString(receiveBuffer, 0, receivedBytes);
                    Console.WriteLine("Response from server: " + responseData);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
        }

        static void ConnectToUnixSocket(Socket socket, string socketPath)
        {
            // Criar um objeto UnixDomainSocketEndPoint
            var endPoint = new UnixDomainSocketEndPoint(socketPath);

            // Conectar ao Unix socket
            socket.Connect(endPoint);
        }












        //-----------------------------------------------------------------------------------------------------------------------------
        //                                                   Decide a regra a executar
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

        public void AddRuleNat(string firewall, string protocol, string port, string cont_internal_ip, string cont_internal_port, string rule = "")
        {

            string shost_ip = host_ip;
            string sbridge_interface = bridge_interface;

            if (firewall == "ipt")
            {

                criar_ligação(port, cont_internal_port, cont_internal_ip, protocol);
            }
            else if (firewall == "lxdforward")
            {

                Lxc_forward(sbridge_interface, shost_ip, port, cont_internal_port, cont_internal_ip, protocol);
            }
            else if (firewall == "lxdapi")
            {
                Lxd_api_forward("POST", sbridge_interface);

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
