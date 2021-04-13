using Newtonsoft.Json;

namespace c2interop{
	public partial class ConfigRoot
	{
		[JsonProperty("Config")]
		public Config Config { get; set; }
	}

	public partial class Config
	{
		[JsonProperty("EnableProxy")]
		public string EnableProxy { get; set; }

		[JsonProperty("ProxyServer")]
		public string ProxyServer { get; set; }

		[JsonProperty("EmpireAPIEndpoint")]
		public string EmpireAPIEndpoint { get; set; }
		
		[JsonProperty("EmpireAPIUsername")]
		public string EmpireAPIUsername { get; set; }

		[JsonProperty("EmpireAPIPassword")]
		public string EmpireAPIPassword {get;set;}
	}

	public partial class ConfigRoot
	{
		public static ConfigRoot FromJson(string json) => JsonConvert.DeserializeObject<ConfigRoot>(json, Converter.Settings);
	}
	public class Converter
	{
		public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
		{
			DateParseHandling = DateParseHandling.None
		};
	}
}