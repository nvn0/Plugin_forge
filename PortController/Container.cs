﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PortController
{
    internal class Container
    {

        private string? name;
        private string? tipo;
        private bool estado;
        private List<int> portas;

        public string Name { get { return name; } /*private*/ set { name = Name; } }

        public string Tipo { get { return tipo; } /*private*/ set { tipo = Tipo; } } // lxc ou incus etc

        public bool Estado { get { return estado; } set { estado = Estado; } } // ligado ou desligado

        public List<int> Portas { get { return portas; } set { portas = value; } }

        




        protected string ExecuteCommand(string command)
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
