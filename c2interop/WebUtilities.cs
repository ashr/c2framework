using System;
using System.IO;

using System.Net;
using System.Net.Security;

using System.Linq;
using System.Text;
using System.Web;
using System.Security.Cryptography;
using System.Collections.Generic;

namespace c2interop
{
	public class WebUtilities
	{
		private bool EnableProxy = false;
        private string HTTP_PROXY_ADDR = "http://192.168.101.109:8080";
        //TLS goodies:
        //System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;

        private CookieContainer cookies = new CookieContainer();

		public WebUtilities(CookieContainer Cookies,bool EnableProxy = false) : this (EnableProxy)
		{
			this.cookies = Cookies;
            this.EnableProxy = EnableProxy;
		}

		public WebUtilities(bool EnableProxy = false, string ProxyServer= "http://localhost:8080")
		{
			this.EnableProxy = EnableProxy;
			this.HTTP_PROXY_ADDR = ProxyServer;
		}

		public string Base64Encode(string data)
		{
			try
			{
				byte[] bytesToEncode = Encoding.UTF8.GetBytes(data);
				return Convert.ToBase64String(bytesToEncode);
			}
			catch (Exception e)
			{
				Console.Write (e.Message);
			}
			return "";
		}

		public string Base64Decode(string data)
		{
			try
			{
				byte[] decodedBytes = Convert.FromBase64String(data);
				string decodedText = Encoding.UTF8.GetString(decodedBytes);
				return decodedText;
			}
			catch (Exception e)
			{
				Console.Write (e.Message);
			}
			return "";
		}

		public string URLEncode(string URL)
		{
			return System.Web.HttpUtility.UrlEncode(URL);
		}

		public string URLDecode(string encodedURL)
		{
			return System.Web.HttpUtility.UrlDecode(encodedURL);
		}

		public string HTMLEncode(string HTML)
		{
			return System.Web.HttpUtility.HtmlEncode(HTML);
		}

		public string HTMLDecode(string encodedHTML)
		{
			return System.Web.HttpUtility.HtmlDecode(encodedHTML);
		}

		public string GenerateMD5(string data)
		{
			MD5 hasher = MD5.Create();
			char[] charData = data.ToCharArray();
			byte[] byteCharData = new byte[charData.Length];
			for (int i = 0; i < charData.Length; i++)
			{
				byteCharData[i] = (byte)charData[i];
			}
			byte[] hashed = hasher.ComputeHash(byteCharData);
			StringBuilder sb = new StringBuilder();
			foreach (byte b in hashed) sb.Append(b.ToString());
			return sb.ToString();
		}


		public string GetResponse(string url,bool autoredirect=true, Dictionary<string,string> headers = null)
		{
			WebProxy proxyObject = new WebProxy(HTTP_PROXY_ADDR);
            System.Net.ServicePointManager.Expect100Continue = false;
            System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            ServicePointManager.ServerCertificateValidationCallback +=
				delegate(
					object sender, 
					System.Security.Cryptography.X509Certificates.X509Certificate certificate, 
					System.Security.Cryptography.X509Certificates.X509Chain
					chain, SslPolicyErrors sslPolicyErrors) {
				return true;
			};
				
			HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(url);
			request.CookieContainer = cookies;
			request.AllowAutoRedirect = autoredirect;
			//request.UserAgent = "Mozilla/4.0 (compatible; MSIE 7.0; Windows NT 5.1; .NET CLR 2.0.50727; .NET4.0C; .NET4.0E)";
			request.UserAgent = "Mozilla/5.0 (Windows NT 6.2; WOW64; rv:18.0) Gecko/20100101 Firefox/18.0";
			//Ignore shitty certificates

			request.ServerCertificateValidationCallback = delegate(object sender, System.Security.Cryptography.X509Certificates.X509Certificate certificate,
				System.Security.Cryptography.X509Certificates.X509Chain chain,
				System.Net.Security.SslPolicyErrors sslPolicyErrors)
			{
				return true; // **** Always accept
			};

			if (EnableProxy) 
				request.Proxy = proxyObject;

			try
			{
				if (headers != null){
					foreach(string key in headers.Keys){
						request.Headers.Add(key, headers[key]);
					}
				}

				HttpWebResponse response = (HttpWebResponse)request.GetResponse();

				if (response.StatusCode == HttpStatusCode.Redirect || response.StatusCode == HttpStatusCode.OK)
				{
					using (StreamReader streamReader = new StreamReader(response.GetResponseStream()))
					{
						string data = streamReader.ReadToEnd();
						return data;
					}
				}
				else
				{
				}
			}
			catch (Exception e)
			{
				Console.Write (e.Message);
			}

			return null;
		}

		public string PostJSONData(string data, string URL,bool autoredirect = true, Dictionary<string,string> headers = null)
		{
            HttpWebResponse response = null;
            HttpWebRequest request = null;

            try
			{
				WebProxy proxyObject = new WebProxy(HTTP_PROXY_ADDR);
				System.Net.ServicePointManager.Expect100Continue = false;
                System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

				ServicePointManager.ServerCertificateValidationCallback +=
					delegate(
						object sender, 
						System.Security.Cryptography.X509Certificates.X509Certificate certificate, 
						System.Security.Cryptography.X509Certificates.X509Chain
						chain, SslPolicyErrors sslPolicyErrors) {
					return true;
				};


				request = (HttpWebRequest)HttpWebRequest.Create(URL);
				request.Method = "POST";
                //request.ContentType = "application/json; charset=UTF-8";
                request.ContentType = "application/json; charset=ASCII";
                request.UserAgent = "Mozilla/4.0 (compatible; MSIE 7.0; Windows NT 5.1; .NET CLR 2.0.50727; .NET4.0C; .NET4.0E)";
				request.CookieContainer = cookies;
				request.AllowAutoRedirect = true;
				request.Accept = "application/json,text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8";
				//request.Headers.Add("Accept-Language","en-US,en;q=0.5");
				//request.Headers.Add("Accept-Encoding","gzip, deflate");
				request.Headers.Add("X-Requested-With","XMLHttpRequest");

				if (headers != null){
					foreach(string header in headers.Keys){
						request.Headers.Add(header,headers[header]);
					}
				}

                request.ServicePoint.Expect100Continue = false;

				//Ignore shitty certificates
				request.ServerCertificateValidationCallback = delegate(object sender, System.Security.Cryptography.X509Certificates.X509Certificate certificate,
					System.Security.Cryptography.X509Certificates.X509Chain chain,
					System.Net.Security.SslPolicyErrors sslPolicyErrors)
				{
					return true; // **** Always accept
				};

				if (EnableProxy)
					request.Proxy = proxyObject;

				StringBuilder postData = new StringBuilder();
				postData.Append(data);

                //byte[] byteArray = Encoding.UTF8.GetBytes(postData.ToString());
                byte[] byteArray = Encoding.ASCII.GetBytes(postData.ToString());
                request.ContentLength = byteArray.Length;

				Stream dataStream = request.GetRequestStream();
				dataStream.Write(byteArray, 0, byteArray.Length);
				dataStream.Close();

				response = (HttpWebResponse)request.GetResponse();

				if (response.StatusCode == HttpStatusCode.OK)
				{
                    //using (StreamReader streamReader = new StreamReader(request.GetResponse().GetResponseStream(),Encoding.UTF8))
                    using (StreamReader streamReader = new StreamReader(response.GetResponseStream(), Encoding.ASCII))
                    {
						string returnData = streamReader.ReadToEnd();
						return returnData;
					}
				}
			}
			catch (WebException e)
			{
				Console.WriteLine(e);
                if (e.Response != null)
                {
                    using (StreamReader streamReader = new StreamReader(e.Response.GetResponseStream(), Encoding.ASCII))
                    {
                        string returnData = streamReader.ReadToEnd();
                        return returnData;
                    }
                }
			}

			return null;
		}

		public string PostData(Dictionary<string, string> data, string URL, bool autoredirect = true)
		{
			try
			{
				WebProxy proxyObject = new WebProxy(HTTP_PROXY_ADDR);
                System.Net.ServicePointManager.Expect100Continue = false;
                System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                ServicePointManager.ServerCertificateValidationCallback +=
					delegate(
						object sender, 
						System.Security.Cryptography.X509Certificates.X509Certificate certificate, 
						System.Security.Cryptography.X509Certificates.X509Chain
						chain, SslPolicyErrors sslPolicyErrors) {
					return true;
				};


				HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(URL);
				request.Method = "POST";
				request.ContentType = "application/x-www-form-urlencoded";
				request.UserAgent = "Mozilla/4.0 (compatible; MSIE 7.0; Windows NT 5.1; .NET CLR 2.0.50727; .NET4.0C; .NET4.0E)";
				request.CookieContainer = cookies;
				request.AllowAutoRedirect = autoredirect;

				request.ServerCertificateValidationCallback = delegate(object sender, System.Security.Cryptography.X509Certificates.X509Certificate certificate,
					System.Security.Cryptography.X509Certificates.X509Chain chain,
					System.Net.Security.SslPolicyErrors sslPolicyErrors)
				{
					return true; // **** Always accept
				};

				if (EnableProxy)
					request.Proxy = proxyObject;
				
				StringBuilder postData = new StringBuilder();
				foreach (KeyValuePair<string, string> kv in data)
				{
					if (postData.Length == 0)
						postData.Append(kv.Key + "=" + kv.Value);
					else
						postData.Append("&" + kv.Key + "=" + kv.Value);
				}

				byte[] byteArray = Encoding.UTF8.GetBytes(postData.ToString());
				request.ContentLength = byteArray.Length;

				Stream dataStream = request.GetRequestStream();
				dataStream.Write(byteArray, 0, byteArray.Length);
				dataStream.Close();

				HttpWebResponse response = (HttpWebResponse)request.GetResponse();

				if (response.StatusCode == HttpStatusCode.OK)
				{
					using (StreamReader streamReader = new StreamReader(response.GetResponseStream()))
					{
						string returnData = streamReader.ReadToEnd();
						return returnData;
					}
				}
				else
				{
				}
			}
			catch (Exception e)
			{
				Console.Write (e.Message);
			}

			return null;
		}

		public CookieContainer GetCookies { get { return cookies; } set { cookies = value; } }

		public dynamic ParseHTTPNameValuePairResponseIntoDynamic(string webresponse)
		{
			throw new NotImplementedException();
		}

		public static string StringToBinary(string data)
		{
			StringBuilder sb = new StringBuilder();

			foreach (char c in data.ToCharArray())
			{
				sb.Append(Convert.ToString(c, 2).PadLeft(8,'0'));
			}
			return sb.ToString();
		}

		public static string BinaryToString(string data)
		{
			List<Byte> byteList = new List<Byte>();

			for (int i = 0; i < data.Length; i += 8)
			{
				byteList.Add(Convert.ToByte(data.Substring(i, 8), 2));
			}

			return Encoding.ASCII.GetString(byteList.ToArray());
		}
	}
}