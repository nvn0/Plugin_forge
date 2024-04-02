using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Xml.Linq;
using System.Runtime.InteropServices.JavaScript;
using System.Net.Sockets;


namespace PortController
{

    internal class SocketData
    {


        private string cont_name;
        private string cont_type;
        private string cont_action;
        private string cont_port;

        public string Name { get { return cont_name; } /*private*/ set { cont_name = Name; } }
        public string Type { get { return cont_type; } /*private*/ set { cont_type = Type; } }
        public string Action { get { return cont_action; } /*private*/ set { cont_action = Action; } }
        public string Port { get { return cont_port; } /*private*/ set { cont_port = Port; } }


        public SocketData()
        { 
        
        }

        

        
        public void ReceberJson(Socket clientSocket)
        {
            Console.WriteLine("teste func receber json");
            // Ler os dados recebidos do cliente
            byte[] buffer = new byte[1024];
            int bytesRead = clientSocket.Receive(buffer);
            string jsonData = Encoding.UTF8.GetString(buffer, 0, bytesRead);
            dynamic receivedData = JsonConvert.DeserializeObject(jsonData);

            // Imprime os dados recebidos
            Console.WriteLine("Dados recebidos do client:");
            Console.WriteLine($"Nome: {receivedData.Container}");
            Console.WriteLine($"Tipo: {receivedData.Type}");
            Console.WriteLine($"Ação: {receivedData.Action}");
            Console.WriteLine($"Porta: {receivedData.Port}");

           Console.WriteLine(receivedData.Type);



            if (receivedData.Action == "GetInfo")
            {
                Console.WriteLine("receber json - getinfo");
                string[] ports;
                ports = Program.GetInfoPortsContainer(receivedData.Container, receivedData.Type);
                //ports = Program.GetInfoPortsContainer("merda", "caralho");

                dynamic responseData = new
                {
                    Status = "Sucesso",
                    Ports = ports
                };

                string responseJson = JsonConvert.SerializeObject(responseData);
                byte[] responseBytes = Encoding.UTF8.GetBytes(responseJson);
                clientSocket.Send(responseBytes);
            }
            else
            {
                clientSocket.Shutdown(SocketShutdown.Both);
                clientSocket.Close();

                Program.execRegraContainer(receivedData.Container, receivedData.Type, receivedData.Action, receivedData.Port);
            }

            clientSocket.Close();
        }
        



    }
}
