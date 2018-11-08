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

      // Step 6: Update
      if (controller.Update())
      {
        return;
      }
      else
      {
        // TODO: set up email to inform there is an issue updating the status on some reference numbers
        Console.WriteLine("Pause Statement");
        }



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

    public static string GetValidateJSON(HttpWebRequest wr)
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
        var errorString = "an unhandled exception was thrown by web api controller.";
        if (errorString.ToLower() != "an unhandled exception was thrown by web api controller.")
        {
          new ErrorLog(ex, wr.RequestUri.ToString() + '\n' + json);
        }

        return null;
      }
    }

    public static HttpWebRequest CreateWebRequest(string uri, WebHeaderCollection hc, string apiMethod)
    {
      try
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

      catch (Exception ex)
      {
        Console.WriteLine(ex.ToString());
        return null;
      }
    }

    public static List<T> Get_Data<T>(string query, string cs)
    {
      try
      {
        using (IDbConnection db = new SqlConnection(GetCS(cs)))
        {
          return (List<T>)db.Query<T>(query);
        }
      }
      catch (Exception ex)
      {
        new ErrorLog(ex, query);
        return null;
      }
    }

    public static List<T> Get_Data<T>(string query, DynamicParameters dbA, string cs)
    {
      try
      {
        using (IDbConnection db = new SqlConnection(cs))
        {
          return (List<T>)db.Query<T>(query, dbA);
        }
      }
      catch (Exception ex)
      {
        new ErrorLog(ex, query);
        return null;
      }
    }

    public static int Exec_Query(string query, DynamicParameters dbA, string cs)
    {
      try
      {
        using (IDbConnection db = new SqlConnection(cs))
        {
          return db.ExecuteScalar<int>(query, dbA);
        }
      }
      catch (Exception ex)
      {
        new ErrorLog(ex, query);
        return -1;
      }
    }

    public static int Exec_Query(string query, string cs)
    {
      try
      {
        using (IDbConnection db = new SqlConnection(cs))
        {
          return db.Execute(query);
        }
      }
      catch (Exception ex)
      {
        new ErrorLog(ex, query);
        return -1;
      }
    }

    public static int Exec_Scalar(string query, DynamicParameters dbA, string cs)
    {
      try
      {
        using (IDbConnection db = new SqlConnection(cs))
        {
          return db.ExecuteScalar<int>(query, dbA);
        }
      }
      catch (Exception ex)
      {
        new ErrorLog(ex, query);
        return -1;
      }
    }

    public static string GetCS(string cs)
    {
      return cs;
    }

  }

}