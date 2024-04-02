using System;
using System.Threading;
using System.Net.Sockets;
using System.Text;
using System.IO;
using Newtonsoft.Json;
using System.Runtime.InteropServices.JavaScript;

namespace PortController
{
    public class Program
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


            // Caminho do ficheiro do socket
            string socketPath = "/tmp/meu_socket";

            if (File.Exists(socketPath))
            {
                Console.WriteLine("O arquivo do socket já existe. Criando um novo...");
                //File.Delete(socketPath);
            }

            // Criar o socket Unix
            Socket serverSocket = new Socket(AddressFamily.Unix, SocketType.Stream, ProtocolType.Unspecified);

            try
            {
                // ligar o socket à unix socket
                serverSocket.Bind(new UnixDomainSocketEndPoint(socketPath));

                // Definir o tamanho máximo da fila de conexão pendentes
                serverSocket.Listen(5);

                Console.WriteLine("À espera de pedidos...");

                // Loop para aceitar conexões
                while (true)
                {
                    // Aceitar a conexão do cliente
                    Socket clientSocket = serverSocket.Accept();

                    SocketData data = new SocketData();

                    //Thread thread = new Thread(() => data.ReceberJson(clientSocket));

                    data.ReceberJson(clientSocket);



                 
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


        public static void execRegraContainer(string name, string type, string action, string port)
        {

            string opcao = type;

            switch (opcao)
            {
                case "lxc":
                    Console.WriteLine("Opção 1 selecionada.");

                    Lxc lxc = new Lxc();

                    lxc.OpenPort(name, port);

                    break;
                case "incus":
                    Console.WriteLine("Opção 2 selecionada.");
                    break;
                case "docker":
                    Console.WriteLine("Opção 3 selecionada.");
                    break;
                default:
                    Console.WriteLine("Opção inválida. (execRegraContainer)");
                    break;
            }


        }


        public static string[] GetInfoPortsContainer(dynamic name, dynamic type)
        {

            string opcao = type;

            switch (opcao)
            {
                case "lxc":
                    Console.WriteLine("Opção 1 selecionada. (getInfo)");

                    Lxc lxc = new Lxc{ Nome = name, Tipo = type };

                    //Thread thread1 = new Thread(() => lxc.GetInfo(name));

                    //return 

                    //Console.WriteLine(lxc.GetInfo(name));

                    //Console.WriteLine(lxc.GetPorts());
                    Console.WriteLine(lxc.GetPorts2());
                    string[] array = lxc.GetPorts2().ToArray();

                    return array;

                    

                    break;
                    
                case "incus":
                    Console.WriteLine("Opção 2 selecionada.");
                    string[] array2 = { "1" };
                    return array2;

                    break;
                case "docker":
                    Console.WriteLine("Opção 3 selecionada.");

                    string[] array3 = { "1" };
                    return array3;
                    break;
                default:
                    Console.WriteLine("Opção inválida. (GetInfoPortsContainer)");
                    string[] array4 = { "1" };
                    return array4;
                    break;
                    
            }


        }

    }
}
