using System;
using System.Threading;
using System.Net.Sockets;
using System.Text;
using System.IO;
using Newtonsoft.Json;
using System.Runtime.InteropServices.JavaScript;

namespace PortController
{
    internal class Program
    {

        static void Main(string[] args)
        {
            /*
            Console.WriteLine("Hello World!!!!!");
            string minha_regra = "INPUT -p tcp --dport 22 -j DROP";

            Firewall firewall = new Firewall();
            //firewall.AddRule(minha_regra);

            Thread thread1 = new Thread(firewall.AllowSshIn);

            thread1.Start();
            

			//firewall.AllowSshIn();
            */


            //-------------------------------------------------------------------------------

            string cont_name;
            string cont_type;
            string cont_port;



            // Caminho do ficheiro do socket
            string socketPath = "/tmp/meu_socket";

            if (File.Exists(socketPath))
            {
                Console.WriteLine("O arquivo do socket já existe. Criando um novo...");
                //File.Delete(socketPath);
            }

            // Criando o socket Unix
            Socket serverSocket = new Socket(AddressFamily.Unix, SocketType.Stream, ProtocolType.Unspecified);

            try
            {
                // ligar o socket à unix socket
                serverSocket.Bind(new UnixDomainSocketEndPoint(socketPath));

                // Definindo o tamanho máximo da fila de conexão pendentes
                serverSocket.Listen(5);

                Console.WriteLine("À espera de pedidos...");

                // Loop para aceitar conexões
                while (true)
                {
                    // Aceitar a conexão do cliente
                    Socket clientSocket = serverSocket.Accept();

                    // Ler os dados recebidos do cliente
                    byte[] buffer = new byte[1024];
                    int bytesRead = clientSocket.Receive(buffer);

                    string jsonData = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                    dynamic receivedData = JsonConvert.DeserializeObject(jsonData);

                    // Imprime os dados recebidos
                    Console.WriteLine("Dados recebidos do client:");
                    Console.WriteLine($"Nome: {receivedData.Container}");
                    Console.WriteLine($"Tipo: {receivedData.Type}");
                    Console.WriteLine($"Porta: {receivedData.Port}");




                    cont_name = receivedData.Container;
                    cont_type = receivedData.Type;
                    cont_port = receivedData.Port;





                    clientSocket.Shutdown(SocketShutdown.Both);
                    clientSocket.Close();

                    execRegraContainer(cont_name, cont_type, cont_port);
                    //execRegraContainer(receivedData.Container, receivedData.Type, receivedData.Port);
                }


            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ocorreu um erro: {ex.Message}");
            }
            finally
            {
                // Fechar o socket do servidor
                serverSocket.Close();
            }










        }


        static void execRegra()
        {

            Firewall firewall = new Firewall();

            Thread thread1 = new Thread(firewall.AllowSshIn);

            thread1.Start();



        }


        static void execRegraContainer(string name, string type, string port)
        {

            string opcao = type;

            switch (opcao)
            {
                case "lxc":
                    Console.WriteLine("Opção 1 selecionada.");

                    Lxc lxc = new Lxc();

                    Thread thread1 = new Thread(() => lxc.OpenPort(name, port));

                    break;
                case "incus":
                    Console.WriteLine("Opção 2 selecionada.");
                    break;
                case "docker":
                    Console.WriteLine("Opção 3 selecionada.");
                    break;
                default:
                    Console.WriteLine("Opção inválida.");
                    break;
            }


        }

    }
}
