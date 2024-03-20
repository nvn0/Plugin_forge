using System;
using System.Threading;
using System.Net.Sockets;
using System.Text;
using System.IO;

namespace Plugin_forge
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

                    // Lendo os dados recebidos do cliente
                    byte[] buffer = new byte[1024];
                    int bytesRead = clientSocket.Receive(buffer);
                    string dataReceived = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                    Console.WriteLine($"Mensagem recebida do cliente: {dataReceived}");

                    // Enviar uma resposta de volta para o cliente
                    /*
                    string responseMessage = "Mensagem recebida com sucesso!";
                    byte[] responseData = Encoding.UTF8.GetBytes(responseMessage);
                    clientSocket.Send(responseData);
                    */


                    // Fechar o socket do cliente
                    clientSocket.Shutdown(SocketShutdown.Both);
                    clientSocket.Close();

                    execRegra();
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


    }
}
