﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Xml.Linq;
using System.Runtime.InteropServices.JavaScript;
using System.Net.Sockets;
using System.ComponentModel.Design;
using static System.Collections.Specialized.BitVector32;
using System.Data;
using static System.Formats.Asn1.AsnWriter;
using JsonSerializer = System.Text.Json.JsonSerializer; // Alias para o JsonSerializer



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
            

            // Ler os dados recebidos do cliente(socket)
            byte[] buffer = new byte[1024];
            int bytesRead = clientSocket.Receive(buffer);
            string jsonData = Encoding.UTF8.GetString(buffer, 0, bytesRead);
            dynamic receivedData = JsonConvert.DeserializeObject(jsonData);

            // Imprime os dados recebidos
            Console.WriteLine("Dados recebidos do client:");
            Console.WriteLine($"Nome: {receivedData.Container}");
            Console.WriteLine($"Type: {receivedData.Type}");
            Console.WriteLine($"Action: {receivedData.Action}");
            Console.WriteLine($"Firewall: {receivedData.Fw}");
            Console.WriteLine($"Protocol: {receivedData.Protocol}");
            Console.WriteLine($"Port: {receivedData.Port}");
            Console.WriteLine($"External_ip: {receivedData.External_ip}");
            Console.WriteLine($"Container_internal_ip: {receivedData.Container_internal_ip}");
            Console.WriteLine($"Container_internal_port: {receivedData.Container_internal_port}");
            Console.WriteLine($"Rule: {receivedData.Rule}");

            // conversao das variaveis para string
            string scont_name = receivedData.Container;
            string stype = receivedData.Type;
            string saction = receivedData.Action;
            string sfw = receivedData.Fw;
            string sprotocol = receivedData.Protocol;
            string sport = receivedData.Port;
            string sexternal_ip = receivedData.External_ip;
            string scont_internal_ip = receivedData.Container_internal_ip;
            string scont_internal_port = receivedData.Container_internal_port;
            string srule = receivedData.Rule;



            if (receivedData.Action == "GetInfo")
            {
                /*
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
                */
            }
            if (receivedData.Type == "host" && receivedData.Action == "Getipports") // ver as portas de um ip externo associado a um container
            {
                List<dynamic> portsList;

                Firewall fwi = new Firewall();
                portsList = fwi.Lxd_api_nat_ip_ports(sexternal_ip);
                Console.WriteLine(portsList);

                string requestBody = JsonSerializer.Serialize(portsList);
                byte[] requestBytes = Encoding.UTF8.GetBytes(requestBody);


                Console.WriteLine(requestBytes);
                clientSocket.Send(requestBytes);


            }
            if (receivedData.Type == "host" && receivedData.Action == "ClosePort" || receivedData.Type == "host" && receivedData.Action == "OpenPort" || receivedData.Type == "host" && receivedData.Action == "ExecCmd")
            {

                //clientSocket.Shutdown(SocketShutdown.Both);
                clientSocket.Close();
                

                Firewall fwh = new Firewall();

                fwh.AddRule(saction, sfw, sprotocol, sport, srule);


                //execRegra(receivedData.Action, receivedData.Fw, receivedData.Protocol, receivedData.Port, receivedData.Rule);

            }
            //if (receivedData.Type == "host" && receivedData.Action == "AddNat" || receivedData.Type == "host" && receivedData.Action == "RemoveNat" || receivedData.Type == "host" && receivedData.Action == "ResetNat" || receivedData.Type == "host" && receivedData.Action == "Listports")
            if (receivedData.Type == "host" && receivedData.Action == "AddNat" || receivedData.Type == "host" && receivedData.Action == "RemoveNat" || receivedData.Type == "host" && receivedData.Action == "ResetNat")
            {
                
                //clientSocket.Shutdown(SocketShutdown.Both);
                clientSocket.Close();

                

                Firewall fwn = new Firewall();

                fwn.AddRuleNat(saction, sfw, sprotocol, sport, sexternal_ip, scont_internal_ip, scont_internal_port, srule);


                //execRegraNat(receivedData.Action, receivedData.Fw, receivedData.Protocol, receivedData.Port, receivedData.External_ip, receivedData.Container_internal_ip, receivedData.Container_internal_port, receivedData.Rule);

            }
            if (receivedData.Type == "lxc" || receivedData.Type == "incus") // executar regras dentro dos containers
            {
                //clientSocket.Shutdown(SocketShutdown.Both);
                clientSocket.Close();

                switch (stype)
                {
                    case "lxc":
                        Console.WriteLine("Opcao 1 selecionada.");

                        Lxc lxc = new Lxc();

                        lxc.AddRule(scont_name, saction, sfw, sprotocol, sport, srule);

                        break;
                    case "incus":
                        Console.WriteLine("Opcao 2 selecionada.");

                        Incus incus = new Incus();

                        incus.AddRule(scont_name, saction, sfw, sprotocol, sport, srule);

                        break;
                    default:
                        Console.WriteLine("Opcao invalida. (execRegraContainer)");
                        break;
                }

                //execRegraContainer(receivedData.Container, receivedData.Type, receivedData.Action, receivedData.Fw, receivedData.Protocol, receivedData.Port, receivedData.Rule);
            }

            clientSocket.Close();
        }

        // ########################################################################################################################
        // -------------------------------------------------------- host --------------------------------------------------------
        // ########################################################################################################################

        /*
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
        */

        /*
        private static void execRegraNat(dynamic action, dynamic firewall, dynamic protocol, dynamic port, dynamic external_ip, dynamic cont_internal_ip, dynamic container_internal_port, dynamic rule)
        {
            string saction = action;
            string sfw = firewall;
            string sprotocol = protocol;
            string sport = port;
            string sexternal_ip = external_ip;
            string scont_internal_ip = cont_internal_ip;
            string scont_internal_port = container_internal_port;
            string srule = rule;


            Firewall fw = new Firewall();

            fw.AddRuleNat(saction, sfw, sprotocol, sport, sexternal_ip, scont_internal_ip, scont_internal_port, srule);

        }
        */

        // ###################################################################################################################################################
        // ------------------------------------------------------      Containers    -------------------------------------------------------------------------
        // ###################################################################################################################################################


        
        //private static void execRegraContainer(dynamic name, dynamic type, dynamic action, dynamic firewall, dynamic protocol, dynamic port, dynamic rule)
        //{
        //    string sname = name;
        //    string stype = type;
        //    string saction = action;
        //    string sfw = firewall;
        //    string sprotocol = protocol;
        //    string sport = port;
        //    string srule = rule;

        //    switch (stype)
        //    {
        //        case "lxc":
        //            Console.WriteLine("Opção 1 selecionada.");

        //            Lxc lxc = new Lxc();

        //            /*
        //            if (fwt == "ipt" && action)
        //            {
        //                lxc.OpenPort(name, port);
        //            }   
        //            else if (fwt == "nft")
        //            {
        //                lxc.NFOpenPort(name, port);
        //            }
        //            */

        //            lxc.AddRule(sname, saction, sfw, sprotocol, sport, srule);


        //            /*
        //            string nomec = name;
        //            Console.WriteLine(nomec);
        //            lxc.ApiCommand(nomec);
        //            */

        //            break;
        //        case "incus":
        //            Console.WriteLine("Opção 2 selecionada.");
        //            break;
        //        default:
        //            Console.WriteLine("Opção inválida. (execRegraContainer)");
        //            break;
        //    }


        //}
        

        /*
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



        }*/
        
        //private static string[] GetInfoPortsContainer(dynamic name, dynamic type)
        //{

        //    string opcao = type;

        //    switch (opcao)
        //    {
        //        case "lxc":
        //            Console.WriteLine("Opção 1 selecionada. (getInfo)");

        //            /*
        //            string nomec = name;
        //            string typec = type;

        //            Lxc lxc = new Lxc(nomec, typec);
        //            */

        //            Lxc lxc = new Lxc { Nome = name, Tipo = type };




        //            //Console.WriteLine(lxc.GetInfo(name));

        //            //Console.WriteLine(lxc.GetPorts());
        //            Console.WriteLine(lxc.GetPorts2());
        //            string[] array = lxc.GetPorts2().ToArray();

        //            return array;



        //            break;

        //        case "incus":
        //            Console.WriteLine("Opção 2 selecionada.");
        //            string[] array2 = { "1" };
        //            return array2;

        //            break;
        //        default:
        //            Console.WriteLine("Opção inválida. (GetInfoPortsContainer)");
        //            string[] array4 = { "1" };
        //            return array4;
        //            break;

        //    }


        //}
            
    }
}