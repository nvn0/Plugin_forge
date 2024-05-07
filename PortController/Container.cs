using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PortController
{
    internal class Container
    {

        private string? nome;
        private string? tipo;
        private string? internal_ip;
        private bool estado;
        private List<string>? portas;
        //private int[] portas;

        public string Nome { get { return nome; } /*private*/ set { nome = Nome; } }

        public string Tipo { get { return tipo; } /*private*/ set { tipo = Tipo; } } // lxc ou incus etc

        public string Internal_ip { get { return internal_ip; } /*private*/ set { internal_ip = Internal_ip; } }

        public bool Estado { get { return estado; } set { estado = Estado; } } // ligado ou desligado

        public List<string> Portas { get { return portas; } set { portas = value; } }
        //public int[] Portas { get { return portas; } set { portas = value; } }

        




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
