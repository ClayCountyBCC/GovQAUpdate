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
    public const int GovQAUpdate_application_staff_id = 1;

    public static void Main()
    {
      var controller = new GovQAControl();

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

   
    public static string GetJSON(string url, WebHeaderCollection hc = null, string apiMethod = "GET")
    {
      
      ServicePointManager.ReusePort = true;
      ServicePointManager.Expect100Continue = true;
      ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

      var wr = WebRequest.Create(url);
      wr.Timeout = 40000;
      wr.Method = apiMethod;
      wr.Proxy = null;
      wr.ContentType = "application/json";
      if (hc != null) // Added this bit for the Fleet Complete Headers that are derived from the Authentication information.
      {
        foreach (string key in hc.AllKeys)
        {
          wr.Headers.Add(key, hc[key]);
        }
      }
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
        new ErrorLog(ex, url + '\n' + json);
        return null;
      }
    }

  }
}