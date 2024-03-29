﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PortController
{
    internal class Lxc:Container
    {



        public void AddRule(string container_name, string rule)
        {
            ExecuteCommand($"lxc exec {container_name} -- sudo iptables -A {rule}");
            ExecuteCommand($"lxc exec {container_name} -- sudo /sbin/iptables-save");
        }

        public void OpenPort(string container_name, string port)
        {
            ExecuteCommand($"lxc exec {container_name} -- sudo iptables -A  INPUT -p tcp --dport {port} -j ACCEPT");
            ExecuteCommand($"lxc exec {container_name} -- sudo /sbin/iptables-save");
        }


        // Esta função define a regra para aceitar conexoes ssh de entrada de qualquer ip
        public void AllowSshIn(string container_name)
        {
            ExecuteCommand($"lxc exec {container_name} -- sudo iptables -I INPUT -p tcp --dport 22 -j ACCEPT");
            ExecuteCommand($"lxc exec {container_name} -- sudo /sbin/iptables-save");
        }



    }
}
