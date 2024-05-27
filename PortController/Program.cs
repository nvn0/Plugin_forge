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


            Console.WriteLine("Port Controller v1.0");

            // Caminho do ficheiro do socket
            string socketPath = "/tmp/socket_proj";

            if (File.Exists(socketPath))
            {
                Console.WriteLine("O ficheiro do socket já existe. A criar um novo...");
                File.Delete(socketPath);
            }
            
            // Criar a unix socket
            Socket serverSocket = new Socket(AddressFamily.Unix, SocketType.Stream, ProtocolType.Unspecified);

            try
            {
                // ligar o socket a unix socket
                serverSocket.Bind(new UnixDomainSocketEndPoint(socketPath));

                // Definir o tamanho máximo da fila de conexoes pendentes
                serverSocket.Listen(5);

                Console.WriteLine("A espera de pedidos...");

                // Loop para aceitar conexoes
                while (true)
                {
                    // Aceitar a conexao do cliente
                    Socket clientSocket = serverSocket.Accept();

                    SocketData data = new SocketData();

                    Thread thread = new Thread(() => data.ReceberJson(clientSocket));
                    thread.Start();

                    

                }

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ocorreu um erro: {ex.Message}");
            }
            finally
            {
                // Fechar o socket
                serverSocket.Close();
            }


        }

       
    }
}
