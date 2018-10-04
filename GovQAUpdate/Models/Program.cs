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

namespace GovQAUpdate
{
  class Program
  {
    public const int AppId = 00000;
    public const int GovQAUpdate_application_staff_id = 1;

    public static void Main()
    {
      var token = new AccessToken();
      var i =  GetJSONAsync(token.GetLoginURL(), token.Headers);
      var records = new GovQARecord();


      // TODO: write program

      // While (ProgramShouldBeRunning)
      // {


        // try
        // {

          // initialize GovQA object

          // get list of completed PublicWorks && AnimalControl GovQA ids
          // reduce list by checking against already completed ids need criteria to filter by

          // update GovQA

          // update ProcessedGovQAIds set GovQAClosedDate = current datetime

          // wait     

        // }
        // catch(Exception ex)
        // {
            // write error 
        // }
      // }
    }

    public static Task<string> GetJSONAsync(string url, WebHeaderCollection hc = null)
    {


      //ServicePointManager.ReusePort = true;
      //ServicePointManager.Expect100Continue = true;
      //ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
      //var wr = HttpWebRequest.Create(url);
      //wr.Timeout = 40000;
      //wr.Proxy = null;
      //wr.ContentType = "application/json";
      //if (hc != null) // Added this bit for the Fleet Complete Headers that are derived from the Authentication information.
      //{
      //  foreach (string key in hc.AllKeys)
      //  {
      //    wr.Headers.Add(key, hc[key]);
      //  }
      //}

      // TODO: get content-type application/json and accept application/json headers get added
      string json = "";

      try
      {

        using (var client = new HttpClient())
        {
          var request = new HttpRequestMessage()
          {
            RequestUri = new Uri(url),
            Method = HttpMethod.Post,
          };
          if (hc != null) // Added this bit for the Fleet Complete Headers that are derived from the Authentication information.
          {
            
            foreach (string key in hc.AllKeys)
            {
              request.Headers.Add(key, hc[key] + ",");
            }
          }

          var task = client.SendAsync(request)
              .ContinueWith((taskwithmsg) =>
              {
                var response = taskwithmsg.Result;

                var jsonTask = response.Content.ReadAsStreamAsync();
                jsonTask.Wait();
                var jsonObject = jsonTask.Result;
              });
        };
        //using (HttpClient client = new HttpClient())
        //{
        //  if (hc != null) // Added this bit for the Fleet Complete Headers that are derived from the Authentication information.
        //  {
        //    foreach (string key in hc.AllKeys)
        //    {
        //      wr.Headers.Add(key, hc[key]);
        //    }
        //  };
        //  // TODO: add headers to HttpClient without using HttpClient.DefaultRequestHeaders
        // HttpResponseMessage response = await client.SendAsync(wr);
        // HttpContent content = response.Content;
        //  {
        //    if (response != null)
        //    {
        //      using (StreamReader sr = new StreamReader(await content.ReadAsStringAsync()))
        //      {
        //        json = sr.ReadToEnd();
        //        return json;
        //      }
        //    }
        //  }

        return null;
      }
      catch (Exception ex)
      {
        new ErrorLog(ex, url + '\n' + json);
        return null;
      }
    }

    //public enum CS_Type : int
    //{
    //  GovQA_ActivationKey = 1,
    //  GovQA_AuthKey = 2,
    //  GovQA_User = 3,
    //  GovQA_Password = 4,
    //  GovQA_Login = 5,
    //  GovQA_Client_Secret = 6,
    //  LOG = 7

    //}

    //public static string GetCS(CS_Type cs)
    //{
    //  return ConfigurationManager.ConnectionStrings[cs.ToString()].ConnectionString;
    //}
  }
}