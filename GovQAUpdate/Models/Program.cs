using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;

namespace GovQAUpdate
{
  class Program
  {
    public const int AppId = 00000;
    public const int GovQAUpdate_application_staff_id = 32;
    public static bool is_debug = System.Diagnostics.Debugger.IsAttached;

    public static void Main()
    {

      GovQAControl controller = new GovQAControl();

      controller.Update();

    }

    public static string GetJSON(HttpWebRequest wr)
    {
      
      ServicePointManager.ReusePort = true;
      ServicePointManager.Expect100Continue = true;
      ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

      //var wr = CreateWebRequest(uri, hc, apiMethod);

      string json = "";
      try
      {
        using (var response = wr.GetResponse())
        {
          if (response != null)
          {
            using (StreamReader sr = new StreamReader(response.GetResponseStream()))
            {
              json = sr.ReadToEnd();
              return json;
            }
          }
        }
        return null;
      }
      catch (Exception ex)
      {
        new ErrorLog(ex, wr.RequestUri.ToString() + '\n' + json);
        return null;
      }
    }

    public static HttpWebRequest CreateWebRequest(string uri, WebHeaderCollection hc, string apiMethod)
    {

      var wr = (HttpWebRequest)WebRequest.Create(uri);

      wr.ContentType = "application/json";

      wr.Accept = "application/jason";
      wr.Timeout = 40000;
      wr.Method = apiMethod;
      wr.Proxy = null;

      var postArray = new List<byte>().ToArray();


      if (hc != null && hc.AllKeys.Contains("login"))
      {
        postArray = Encoding.ASCII.GetBytes("{ \"login\": \"" + Properties.Resources.Prod_User + 
                                            "\", \"password\": \"" + Properties.Resources.Password + "\", }");

        wr.ContentLength = postArray.Length;
        var reqStream = wr.GetRequestStream();
        reqStream.Write(postArray, 0, postArray.Length);
        reqStream.Close();
      }


      return wr;
    }

  }

}