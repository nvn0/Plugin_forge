using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PortController
{
    internal class Incus:Container
    {


        public Incus()
        {
        
        }

        //---------------------------------------------------------------------------------------------------------
        //                                                      IPTABLES
        //---------------------------------------------------------------------------------------------------------



        public void OpenPort(string container_name, string protocol, string port)
        {
            ExecuteCommand($"lxc exec {container_name} -- doas iptables -A  INPUT -p {protocol} --dport {port} -j ACCEPT && doas /sbin/iptables-save");
            //ExecuteCommand($"lxc exec {container_name} -- sudo /sbin/iptables-save");
        }

        public void ClosePort(string container_name, string protocol, string port)
        {
            ExecuteCommand($"lxc exec {container_name} -- doas iptables -A  INPUT -p {protocol} --dport {port} -j DROP && doas /sbin/iptables-save");
            //ExecuteCommand($"lxc exec {container_name} -- sudo /sbin/iptables-save");
        }


        // Esta função define a regra para aceitar conexoes ssh de entrada de qualquer ip
        public void AllowSshIn(string container_name)
        {
            ExecuteCommand($"lxc exec {container_name} -- doas iptables -I INPUT -p tcp --dport 22 -j ACCEPT && doas /sbin/iptables-save");
            //ExecuteCommand($"lxc exec {container_name} -- sudo /sbin/iptables-save");
        }



        //---------------------------------------------------------------------------------------------------------
        //                                                      NFTABLES
        //---------------------------------------------------------------------------------------------------------



        public void NFOpenPort(string container_name, string protocol, string port)
        {
            ExecuteCommand($"doas lxc exec {container_name} --doas add rule inet filter input {protocol} dport {port} accept");


        }


        public void NFClosePort(string container_name, string protocol, string port)
        {
            ExecuteCommand($"doas lxc exec {container_name} --doas add rule inet filter input {protocol} dport {port} drop");

        }







        //---------------------------------------------------------------------------------------------------------
        //                                                      Decide o q executar
        //---------------------------------------------------------------------------------------------------------


        public void AddRule(string container_name, string action, string firewall, string protocol, string port, string rule = "")
        {




            if (firewall == "ipt" && action == "OpenPort")
            {
                OpenPort(container_name, protocol, port);
            }
            else if (firewall == "ipt" && action == "ClosePort")
            {
                ClosePort(container_name, protocol, port);
            }
            else if (firewall == "nft" && action == "OpenPort")
            {
                NFOpenPort(container_name, protocol, port);
            }
            else if (firewall == "nft" && action == "ClosePort")
            {
                NFClosePort(container_name, protocol, port);
            }
            else if (firewall == "ipt" && action == "ExecCmd" && rule != "")
            {
                ExecuteCommand($"lxc exec {container_name} -- iptables -A {rule} && /sbin/iptables-save");
                //ExecuteCommand($"lxc exec {container_name} -- sudo /sbin/iptables-save");
            }
            else if (firewall == "nft" && action == "ExecCmd" && rule != "")
            {

            }

        }




    }
}
