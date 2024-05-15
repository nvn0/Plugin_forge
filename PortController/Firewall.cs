using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Net.Sockets;
using System.Runtime.InteropServices.JavaScript;
using System.Text.Json;

namespace PortController
{
    internal class Firewall
    {

        // Esta class interage diretamente no sistema host

       

        // Constantes
        private const string host_ip = "192.168.1.80";
        private const string bridge_interface = "lxdbr0";




        // função para de defenir uma regra personalizada 
        public void iptCustomRule(string rule)
        {
            ExecuteCommand($"iptables -A {rule} && /sbin/iptables-save"); // -A para adicionar no fundo da lista ou -I para adicionar ao topo da lista
			//ExecuteCommand($"/sbin/iptables-save");
        }
        

        public void OpenPort(string protocol, string port)
        {
            ExecuteCommand($"iptables -A  INPUT -p {protocol} --dport {port} -j ACCEPT && /sbin/iptables-save");
        }

        public void ClosePort( string protocol, string port)
        {
            ExecuteCommand($"iptables -A  INPUT -p {protocol} --dport {port} -j DROP && /sbin/iptables-save");
        }



        // Esta função define a regra para aceitar conexoes ssh de entrada de qualquer ip
        public void AllowSshIn()
		{
			ExecuteCommand($"iptables -I INPUT -p tcp --dport 22 -j ACCEPT && /sbin/iptables-save");
			//ExecuteCommand($"/sbin/iptables-save");
		}
		

		//função para defenir o comportamento default para negar o trafego que entra
		public void DenyAllIn()
		{
			ExecuteCommand($"iptables --policy INPUT DROP && /sbin/iptables-save");
			//ExecuteCommand($"/sbin/iptables-save");
		}



        /*
		função para apagar regra especifica com base o numero
		(NOTA: No iptables as regras são numeradas, se uma regras for apagada todas as
		outras na sequencia baixam uma posição na lista)		
		*/
        public void DeleteRuleNumber(string rule_number)
        {
            ExecuteCommand($"iptables -D INPUT {rule_number} && /sbin/iptables-save");
            //ExecuteCommand($"/sbin/iptables-save");
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
            ExecuteCommand($"lxc network forward port add {bridge_interface} {host_ip} {protocol} {porta_exterior} {ip_container} {porta_container}");

        }













        //---------------------------------------------------------------------------------------------------------
        //                                       IPTABLES - criar comandos na tabela NAT
        //---------------------------------------------------------------------------------------------------------





        public void criar_ligação(string porta_exterior, string porta_container, string ip_container, string protocol)
        {
            ExecuteCommand($"iptables -t nat -I PREROUTING -p {protocol} --dport {porta_exterior} -j DNAT --to-destination {ip_container}:{porta_container} && doas /sbin/iptables-save");

        }













        //---------------------------------------------------------------------------------------------------------
        //                                                      API - LXC 
        //---------------------------------------------------------------------------------------------------------


        // Caminho -> /var/lib/lxd/unix.socket


        private void Lxd_api_forward(string bridge_interface, string host_ip, string sprotocol,string port, string cont_internal_ip, string cont_internal_port) //criar comandos de forward
        {

            // GET /1.0/networks/{networkName}/forwards
            // GET /1.0/networks/{networkName}/forwards?recursion=1
            // GET / 1.0 / networks /{ networkName}/ forwards /{ listenAddress}
            // POST / 1.0 / networks /{ networkName}/ forwards
            //PATCH /1.0/networks/{networkName}/forwards/{listenAddress}
            string portsJson = string.Empty;
            List<dynamic> portsList = new List<dynamic>();

            try
            {
                // caminho do Unix socket
                string socketPath = "/var/lib/lxd/unix.socket";

                // Criar unix socket
                using (var socket = new Socket(AddressFamily.Unix, SocketType.Stream, ProtocolType.Unspecified))
                {
                    // Conectar ao socket
                    ConnectToUnixSocket(socket, socketPath);

                    // Enviar uma solicitação HTTP GET
                    string requestPath1 = $"/1.0/networks/{bridge_interface}/forwards/{host_ip}";
                    string request1 = $"GET {requestPath1} HTTP/1.1\r\nHost: dummy\r\n\r\n";

                    // Enviar a solicitação
                    byte[] requestBytes1 = Encoding.UTF8.GetBytes(request1);
                    socket.Send(requestBytes1);

                    // Receber resposta do socket
                    byte[] receiveBuffer = new byte[1024];
                    int receivedBytes = socket.Receive(receiveBuffer);
                    string responseData = Encoding.UTF8.GetString(receiveBuffer, 0, receivedBytes);
                    Console.WriteLine("Response from server: " + responseData);

                    //------ Edição do JSON ------------------------

                    // Encontra o índice do final dos cabeçalhos na resposta
                    int headersEndIndex = responseData.IndexOf("\r\n\r\n");

                    // Extrai a parte do corpo da resposta (após o final dos cabeçalhos)
                    string responseBody = responseData.Substring(headersEndIndex + 4);

                    // Analisa o JSON do corpo da resposta
                    dynamic jsonResponseObject = JsonSerializer.Deserialize<dynamic>(responseBody);
                    Console.WriteLine("OBJETO JSON: " + jsonResponseObject);

                    // Inicializa metadataElement com um valor padrão
                    JsonElement metadataElement = default;


                    

                    // Verifica se o objeto contém a propriedade "metadata"
                    if (jsonResponseObject is not null && jsonResponseObject.TryGetProperty("metadata", out metadataElement))
                    {
                        // Obtém o objeto "metadata"
                        dynamic metadataObject = metadataElement;

                        // Verifica se "metadata" contém a propriedade "ports"
                        if (metadataObject.TryGetProperty("ports", out JsonElement portsElement))
                        {
                           

                            // Verifica se "ports" é de fato um array
                            if (portsElement.ValueKind == JsonValueKind.Array)
                            {
                                // Converte o elemento "ports" para uma lista de portas
                                foreach (var portas in portsElement.EnumerateArray())
                                {
                                    portsList.Add(portas);
                                }

                                // Cria um novo objeto para adicionar à lista de portas
                                dynamic newPort = new
                                {
                                    description = "",
                                    listen_port = port,
                                    protocol = sprotocol,
                                    target_address = cont_internal_ip,
                                    target_port = cont_internal_port
                                };

                                // Adiciona o novo objeto à lista de portas
                                portsList.Add(newPort);
                                portsJson = JsonSerializer.Serialize(portsList);

                                Console.WriteLine("PORTAS: " + portsJson);




                            }
                            else
                            {
                                Console.WriteLine("A propriedade 'ports' em 'metadata' não é um array.");
                            }
                        }
                        else
                        {
                            Console.WriteLine("O objeto 'metadata' não possui a propriedade 'ports'.");
                        }
                    }
                    else
                    {
                        Console.WriteLine("O objeto JSON não possui a propriedade 'metadata' ou é nulo/vazio.");
                    }
                


                    
                    // ----------------------------------------------- PUT --------------------------------------------

                   

                    dynamic requestBodyObject = new
                    {
                        config = new { },
                        description = "",
                        listen_address = host_ip,
                        ports = portsList
                    };

                    // Converte o objeto dinâmico para uma string JSON
                    string requestBody = JsonSerializer.Serialize(requestBodyObject);
                    Console.WriteLine("enviar: " + requestBody);

                    // Construir a solicitação PUT
                    string requestPath2 = $"/1.0/networks/lxdbr0/forwards/{host_ip}";
                    string request2 = $"PUT {requestPath2} HTTP/1.1\r\nHost: dummy\r\nContent-Length: {Encoding.UTF8.GetBytes(requestBody).Length}\r\n\r\n{requestBody}";

                    // Enviar a solicitação
                    byte[] requestBytes2 = Encoding.UTF8.GetBytes(request2);
                    socket.Send(requestBytes2);



                    // Receber resposta do socket
                    byte[] receiveBuffer2 = new byte[1024];
                    int receivedBytes2 = socket.Receive(receiveBuffer2);
                    string responseData2 = Encoding.UTF8.GetString(receiveBuffer2, 0, receivedBytes2);
                    Console.WriteLine("Response from server: " + responseData2);

                 
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
        }

        


        private void Lxd_api_forward_remove(string bridge_interface, string host_ip, string protocol, string port, string cont_internal_ip, string cont_internal_port) //criar comandos de forward
        {

            // GET /1.0/networks/{networkName}/forwards
            // GET /1.0/networks/{networkName}/forwards?recursion=1
            // GET / 1.0 / networks /{ networkName}/ forwards /{ listenAddress}
            // POST / 1.0 / networks /{ networkName}/ forwards
            //PATCH /1.0/networks/{networkName}/forwards/{listenAddress}

            List<JsonElement> portsList = new List<JsonElement>();

            try
            {
                // caminho do Unix socket
                string socketPath = "/var/lib/lxd/unix.socket";

                // Criar unix socket
                using (var socket = new Socket(AddressFamily.Unix, SocketType.Stream, ProtocolType.Unspecified))
                {
                    // Conectar ao socket
                    ConnectToUnixSocket(socket, socketPath);


                    // Enviar uma solicitação HTTP GET
                    string requestPath1 = $"/1.0/networks/{bridge_interface}/forwards/{host_ip}";
                    string request1 = $"GET {requestPath1} HTTP/1.1\r\nHost: dummy\r\n\r\n";

                    // Enviar a solicitação
                    byte[] requestBytes1 = Encoding.UTF8.GetBytes(request1);
                    socket.Send(requestBytes1);

                    // Receber resposta do socket
                    byte[] receiveBuffer = new byte[1024];
                    int receivedBytes = socket.Receive(receiveBuffer);
                    string responseData = Encoding.UTF8.GetString(receiveBuffer, 0, receivedBytes);
                    Console.WriteLine("Response from server: " + responseData);

                    //------ Edição do JSON ------------------------

                    // Encontra o índice do final dos cabeçalhos na resposta
                    int headersEndIndex = responseData.IndexOf("\r\n\r\n");

                    // Extrai a parte do corpo da resposta (após o final dos cabeçalhos)
                    string responseBody = responseData.Substring(headersEndIndex + 4);

                    // Analisa o JSON do corpo da resposta
                    dynamic jsonResponseObject = JsonSerializer.Deserialize<dynamic>(responseBody);
                    Console.WriteLine("OBJETO JSON: " + jsonResponseObject);

                   

                    // Inicializa metadataElement com um valor padrão
                    JsonElement metadataElement = default;

                    // Verifica se o objeto contém a propriedade "metadata"
                    if (jsonResponseObject is not null && jsonResponseObject.TryGetProperty("metadata", out metadataElement))
                    {
                        // Verifica se "metadata" contém a propriedade "ports"
                        if (metadataElement.TryGetProperty("ports", out JsonElement portsElement))
                        {


                            if (portsElement.ValueKind == JsonValueKind.Array)
                            {
                               

                                // Especifica o "target_address" e o "target_port" a serem removidos
                                string targetAddressToRemove = cont_internal_ip;
                                string targetPortToRemove = cont_internal_port;
                                string listenPortToRemove = port;

                                // Converte o elemento "ports" para uma lista de portas
                                foreach (JsonElement portas in portsElement.EnumerateArray())
                                {
                                    // Verifica se o objeto contém ambas as propriedades "target_address" e "target_port"
                                    if (portas.TryGetProperty("target_address", out JsonElement targetAddressElement) &&
                                        portas.TryGetProperty("target_port", out JsonElement targetPortElement) &&
                                        portas.TryGetProperty("listen_port", out JsonElement listenPortElement))
                                    {
                                        // Verifica se o "target_address" e o "target_port" correspondem aos especificados
                                        if (targetAddressElement.GetString() == targetAddressToRemove && targetPortElement.GetString() == targetPortToRemove && listenPortElement.GetString() == listenPortToRemove)
                                        {
                                            // Se corresponderem, não adicionamos esse objeto à lista de portas
                                            continue;
                                        }
                                    }

                                    // Adiciona o objeto à lista de portas
                                    portsList.Add(portas);
                                }


                                // Verifica se algum objeto foi adicionado à lista de portas
                                if (portsList.Count > 0)
                                {
                                    // Serializa a lista de portas de volta para uma string JSON
                                    string portsJson = JsonSerializer.Serialize(portsList);
                                    Console.WriteLine("Lista de portas em formato JSON após remoção: " + portsJson);

                                    // Agora você pode usar a string JSON da lista de portas conforme necessário
                                }
                                else
                                {
                                    Console.WriteLine("Nenhum objeto adicionado à lista de portas após a remoção.");
                                }

                                // Agora você pode usar a string JSON da lista de portas conforme necessário
                            }

                            else
                            {
                                Console.WriteLine("A propriedade 'ports' em 'metadata' não é um array.");
                            }
                        }
                        else
                        {
                            Console.WriteLine("O objeto 'metadata' não possui a propriedade 'ports'.");
                        }
                    }
                    else
                    {
                        Console.WriteLine("O objeto JSON não possui a propriedade 'metadata' ou é nulo/vazio.");
                    }


                    // ----------------------------------------------- PUT --------------------------------------------



                    dynamic requestBodyObject = new
                    {
                        config = new { },
                        description = "",
                        listen_address = host_ip,
                        ports = portsList
                    };


                    // Converte o objeto dinâmico para uma string JSON
                    string requestBody = JsonSerializer.Serialize(requestBodyObject);
                    Console.WriteLine("A ENVIAR: " + requestBody);

                    // Construir a solicitação PUT
                    string requestPath2 = $"/1.0/networks/lxdbr0/forwards/{host_ip}";
                    string request2 = $"PUT {requestPath2} HTTP/1.1\r\nHost: dummy\r\nContent-Length: {Encoding.UTF8.GetBytes(requestBody).Length}\r\n\r\n{requestBody}";

                    // Enviar a solicitação
                    byte[] requestBytes2 = Encoding.UTF8.GetBytes(request2);
                    socket.Send(requestBytes2);

                    

                    // Receber resposta do socket
                    byte[] receiveBuffer2 = new byte[1024];
                    int receivedBytes2 = socket.Receive(receiveBuffer2);
                    string responseData2 = Encoding.UTF8.GetString(receiveBuffer2, 0, receivedBytes2);
                    Console.WriteLine("Response from server: " + responseData2);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
        }



        private void Lxd_api_forward_reset(string bridge_interface, string host_ip) //criar comandos de forward
        {

            // GET /1.0/networks/{networkName}/forwards
            // GET /1.0/networks/{networkName}/forwards?recursion=1
            // GET / 1.0 / networks /{ networkName}/ forwards /{ listenAddress}
            // POST / 1.0 / networks /{ networkName}/ forwards
            //PATCH /1.0/networks/{networkName}/forwards/{listenAddress}


            try
            {
                // caminho do Unix socket
                string socketPath = "/var/lib/lxd/unix.socket";

                // Criar unix socket
                using (var socket = new Socket(AddressFamily.Unix, SocketType.Stream, ProtocolType.Unspecified))
                {
                    // Conectar ao socket
                    ConnectToUnixSocket(socket, socketPath);


                   

                    // ----------------------------------------------- PUT --------------------------------------------

                    List<JsonElement> portsList = new List<JsonElement>(); // lista vazia


                    dynamic requestBodyObject = new
                    {
                        config = new { },
                        description = "",
                        listen_address = host_ip,
                        ports = portsList       
                    };


                    // Converte o objeto dinâmico para uma string JSON
                    string requestBody = JsonSerializer.Serialize(requestBodyObject);
                    Console.WriteLine("A ENVIAR: " + requestBody);

                    // Construir a solicitação PUT
                    string requestPath2 = $"/1.0/networks/lxdbr0/forwards/{host_ip}";
                    string request2 = $"PUT {requestPath2} HTTP/1.1\r\nHost: dummy\r\nContent-Length: {Encoding.UTF8.GetBytes(requestBody).Length}\r\n\r\n{requestBody}";

                    // Enviar a solicitação
                    byte[] requestBytes2 = Encoding.UTF8.GetBytes(request2);
                    socket.Send(requestBytes2);



                    // Receber resposta do socket
                    byte[] receiveBuffer2 = new byte[1024];
                    int receivedBytes2 = socket.Receive(receiveBuffer2);
                    string responseData2 = Encoding.UTF8.GetString(receiveBuffer2, 0, receivedBytes2);
                    Console.WriteLine("Response from server: " + responseData2);
                }
                
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
        }




       


        private static void ConnectToUnixSocket(Socket socket, string socketPath)
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

        public void AddRuleNat(string action, string firewall, string protocol, string port, string cont_internal_ip, string cont_internal_port, string rule = "")
        {

            string shost_ip = host_ip;
            string sbridge_interface = bridge_interface;

            if (firewall == "ipt" && action == "AddNat")
            {
                Console.WriteLine("opc 1");
                criar_ligação(port, cont_internal_port, cont_internal_ip, protocol);
            }
            else if (firewall == "lxdforward" && action == "AddNat")
            {
                Console.WriteLine("opc 2");
                Lxc_forward(sbridge_interface, shost_ip, port, cont_internal_port, cont_internal_ip, protocol);
            }
            else if (firewall == "lxdapi" && action == "AddNat")
            {
                Console.WriteLine("opc 3");
                Lxd_api_forward(sbridge_interface, shost_ip, protocol, port, cont_internal_ip, cont_internal_port);

            }
            else if (firewall == "lxdapi" && action == "RemoveNat")
            {
                Console.WriteLine("opc 4");
                Lxd_api_forward_remove(sbridge_interface, shost_ip, protocol, port, cont_internal_ip, cont_internal_port);

            }
            else if (firewall == "lxdapi" && action == "ResetNat")
            {
                Console.WriteLine("reset nat");
                Lxd_api_forward_reset(sbridge_interface, shost_ip);

            }

        }

        //------------------------------------------- Executar comandos -----------------------------------------------------


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
