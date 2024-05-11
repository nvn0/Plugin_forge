using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Xml.Linq;
using System.Runtime.InteropServices.JavaScript;
using System.Net.Sockets;
using System.ComponentModel.Design;


namespace PortController
{

    internal class SocketData
    {


        private string cont_name;
        private string cont_type;
        private string cont_action;
        private string cont_fw;
        private string cont_port;

        public string Name { get { return cont_name; } /*private*/ set { cont_name = Name; } }
        public string Type { get { return cont_type; } /*private*/ set { cont_type = Type; } }
        public string Action { get { return cont_action; } /*private*/ set { cont_action = Action; } }
        public string Fwtype { get { return cont_fw; } /*private*/ set { cont_fw = Fwtype; } }
        public string Port { get { return cont_port; } /*private*/ set { cont_port = Port; } }


        public SocketData()
        { 
        
        }

        

        
        public void ReceberJson(Socket clientSocket)
        {
            //Console.WriteLine("teste func receber json");

            // Ler os dados recebidos do cliente

            
            byte[] buffer = new byte[1024];
            int bytesRead = clientSocket.Receive(buffer);
            string jsonData = Encoding.UTF8.GetString(buffer, 0, bytesRead);
            dynamic receivedData = JsonConvert.DeserializeObject(jsonData);

            // Imprime os dados recebidos
            Console.WriteLine("Dados recebidos do client:");
            Console.WriteLine($"Nome: {receivedData.Container}");
            Console.WriteLine($"Tipo: {receivedData.Type}");
            Console.WriteLine($"Firewall: {receivedData.Fw}");
            Console.WriteLine($"Ação: {receivedData.Action}");
            Console.WriteLine($"Protocol: {receivedData.Protocol}");
            Console.WriteLine($"Porta: {receivedData.Port}");
            Console.WriteLine($"Container_internal_ip: {receivedData.Container_internal_ip}");
            Console.WriteLine($"Container_internal_port: {receivedData.Container_internal_port}");
            Console.WriteLine($"Rule: {receivedData.Rule}");

           



            if (receivedData.Action == "GetInfo")
            {
                Console.WriteLine("receber json - getinfo");

                string[] ports;
                string estado;

                ports = GetInfoPortsContainer(receivedData.Container, receivedData.Type);
                estado = GetStateCont(receivedData.Container, receivedData.Type);

                dynamic responseData = new
                {
                    Status = "Sucesso",
                    Estado = estado,
                    Ports = ports
                };

                string responseJson = JsonConvert.SerializeObject(responseData);
                byte[] responseBytes = Encoding.UTF8.GetBytes(responseJson);
                clientSocket.Send(responseBytes);
            }
            if (receivedData.Type == "host" && receivedData.Action != "AddNat")
            {

                clientSocket.Shutdown(SocketShutdown.Both);
                clientSocket.Close();

                execRegra(receivedData.Action, receivedData.Fw, receivedData.Protocol, receivedData.Port, receivedData.Rule);

            } 
            if (receivedData.Type == "host" && receivedData.Action == "AddNat" || receivedData.Type == "host" && receivedData.Action == "RemoveNat")
            {

                clientSocket.Shutdown(SocketShutdown.Both);
                clientSocket.Close();

                execRegraNat(receivedData.Action, receivedData.Fw, receivedData.Protocol, receivedData.Port, receivedData.Container_internal_ip, receivedData.Container_internal_port, receivedData.Rule);

            }
            else
            {
                clientSocket.Shutdown(SocketShutdown.Both);
                clientSocket.Close();

                execRegraContainer(receivedData.Container, receivedData.Type, receivedData.Action, receivedData.Fw, receivedData.Protocol, receivedData.Port, receivedData.Rule);
            }

            clientSocket.Close();
        }


        // -------------------------------------------------------- host --------------------------------------------------------



        private static void execRegra(dynamic action, dynamic firewall, dynamic protocol, dynamic port, dynamic rule)
        {
            string saction = action;
            string sfw = firewall;
            string sprotocol = protocol;      
            string sport = port;
            string srule = rule;


            Firewall fw = new Firewall();

            fw.AddRule(saction, sfw, sprotocol, sport, srule);

        }



        private static void execRegraNat(dynamic action, dynamic firewall, dynamic protocol, dynamic port, dynamic cont_internal_ip, dynamic container_internal_port, dynamic rule)
        {
            string saction = action;
            string sfw = firewall;
            string sprotocol = protocol;
            string sport = port;
            string scont_internal_ip = cont_internal_ip;
            string scont_internal_port = container_internal_port;
            string srule = rule;


            Firewall fw = new Firewall();

            fw.AddRuleNat(sfw, sprotocol, sport, scont_internal_ip, scont_internal_port, srule);

        }
        // ------------------------------------------------------ Containers -----------------------------------------------------------

        private static void execRegraContainer(dynamic name, dynamic type, dynamic action, dynamic firewall, dynamic protocol, dynamic port, dynamic rule)
        {
            string sname = name;
            string stype = type;
            string saction = action;
            string sfw = firewall;
            string sprotocol = protocol;
            string sport = port;
            string srule = rule;

            switch (stype)
            {
                case "lxc":
                    Console.WriteLine("Opção 1 selecionada.");

                    Lxc lxc = new Lxc();

                    /*
                    if (fwt == "ipt" && action)
                    {
                        lxc.OpenPort(name, port);
                    }   
                    else if (fwt == "nft")
                    {
                        lxc.NFOpenPort(name, port);
                    }
                    */

                    lxc.AddRule(sname, action, sfw, sprotocol, sport, srule);


                    /*
                    string nomec = name;
                    Console.WriteLine(nomec);
                    lxc.ApiCommand(nomec);
                    */

                    break;
                case "incus":
                    Console.WriteLine("Opção 2 selecionada.");
                    break;              
                default:
                    Console.WriteLine("Opção inválida. (execRegraContainer)");
                    break;
            }


        }

        private static string GetStateCont(dynamic name, dynamic type)
        {
            string sname = name;
            string stype = type;

            switch (stype)
            {
                case "lxc":
                    Console.WriteLine("Opção 3 selecionada.");
                       
                    Lxc lxc = new Lxc();

                    bool state = lxc.GetState(sname);

                    if (state == true)
                    {
                        return $"{sname} is running";
                    }
                    else
                    {
                        return $"{sname} is down";
                    }

                    break;
                case "incus":
                    return " ";

                    break;
                default:
                    Console.WriteLine("Opção inválida. (execRegraContainer)");
                    return " ";
                    break;
            }



        }

        private static string[] GetInfoPortsContainer(dynamic name, dynamic type)
        {

            string opcao = type;

            switch (opcao)
            {
                case "lxc":
                    Console.WriteLine("Opção 1 selecionada. (getInfo)");

                    /*
                    string nomec = name;
                    string typec = type;

                    Lxc lxc = new Lxc(nomec, typec);
                    */

                    Lxc lxc = new Lxc { Nome = name, Tipo = type };
                    

                   

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
