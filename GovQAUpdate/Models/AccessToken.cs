using Newtonsoft.Json;
using System;
using System.Text;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Collections.Generic;

namespace GovQAUpdate
{
  public class AccessToken
  {
    //public string Domain { get; set; }
    //public bool IsMustChangePassword { get; set; }
    //public int LicenseExpiryDays { get; set; }
    //public string Name { get; set; }
    public string BaseProductionURL => "https://claycountyfl.webqaservices.com/PROD/api/";
    private string LoginURL => GetUri("login");

    public bool valid_token { get; set; } = false;
    public string current_session_id { get; set; } = "";
    public int staff_id { get; set; } = -1;
    public string version { get; set; } = "";
    public string api_version
    {
      get
      {
        if (version.Length == 0)
        {
          return "";
        }
        else
        {
          return version.Substring(0, 5).Replace(".", "_");
        }

      }
    }


    public System.Net.WebHeaderCollection Headers
    {
      get
      {
        var whc = new System.Net.WebHeaderCollection();
        if (current_session_id.Length == 0)
        {
          whc.Add("login", Properties.Resources.Prod_User);
          whc.Add("password", Properties.Resources.Password);
          return whc;
        }
        else
        {
          return new WebHeaderCollection();
        }
      }
    }

    // TODO: figure out a way to add the header from here

    public AccessToken()
    {

    }


    public string GetURIWithEndpoint(string endPoint)
    {
      return BaseProductionURL + endPoint + "?activationKey=" + Properties.Resources.ActivationKey;
    }

    public string GetUri(string reason)
    {
      string endPoint = "";
      string parameters = "";
      switch(reason)
      {
        case "login":
          endPoint = "MobileUserLoginAdmin";
          parameters += GetLoginParameters();
          break;
        case "update_note":
          endPoint = "UploadRequestNoteAdmin";
          parameters += GetSessionarameters();
          parameters += GetUpdateNoteParameters();
          break;
        case "update_status":
          endPoint = "UploadRequestStatusAdmin";
          parameters += GetSessionarameters();
          parameters += GetUpdateStatusParameters();
          break;
        case "validate_issue": // use to validate GovQARecord
          endPoint = "MobileServiceRequestsAdmin";
          parameters += GetSessionarameters();
          break;
      }

      var uri = GetURIWithEndpoint(endPoint) + parameters;

      return uri;
    }

    public string GetLoginParameters()
    {
      return "&authKey=" + Properties.Resources.AuthKey;
    }

    public string GetUpdateStatusParameters()
    {
      return "&updated_by_staff_id=32&new_service_request_status_id=44&issue_id=";
    }
    public string GetUpdateNoteParameters()
    {
      return "&updated_by_staff_id=32&issue_id=";
    }
    public string GetSessionarameters()
    {
      return "&sessionId=" + current_session_id;
    }

    public AccessToken Login()
    {
      string json = Program.GetJSON(Program.CreateWebRequest(LoginURL, Headers, "POST")).ToString();
      if (json != null)
      {
        var tokenObject = JsonConvert.DeserializeObject<AccessToken>(json);
        tokenObject.valid_token = tokenObject.current_session_id.Length > 0;
        return tokenObject;
      }
      else
      {
        return null;
      }
    }

    public GovQARecord ValidateGoveQARecord(GovQARecord record)
    {
      
      return new GovQARecord();
    }



    //public HttpWebRequest CreateWebRequest(string uri, REQMETHOD reqMethod)
    //{
    //  var request = (HttpWebRequest)WebRequest.Create(uri);
    //  request.Timeout = 60000;
    //  if (reqMethod == REQMETHOD.POST)
    //  {
    //    request.Method = "POST";
    //    request.ContentType = "application/json";
    //  }
    //  else
    //  {
    //    request.Method = "GET";
    //  }
    //  request.Headers.Add("Accept-Language", CultureInfo.CurrentCulture.Name);

    //  request.CookieContainer = CookieContainer;

    //  return request;
    //}

    //public string TokenRequest(string idmIp, string username, string password)
    //{
    //  string token = null;
    //  string idmUri = string.Format("https://{0}:9031/as/authorization.oauth2?client_id=ssoclient&redirect_uri=napps://localhost/&response_type=code&scope=msi_unsapi_presence.watch msi_unsapi_location.watch msi_unsapi_groupmgt.read", idmIp);
    //  try
    //  {
    //    var idmCodeRequest = CreateWebRequest(idmUri, reqMethod.POST);
    //    idmCodeRequest.AllowAutoRedirect = false;
    //    AddPostData(idmCodeRequest, "pf.username=" + username + "&pf.pass=" + password);

    //    //Log("Request 1 to URL: " + request_1.RequestUri);
    //    var idmCodeResponse = idmCodeRequest.GetResponse();
    //    //string location = idmCodeResponse.Headers.Get("Location");
    //    //if (location != null)
    //    //{
    //    //  Log("Location: " + location);
    //    //  string code = location.Substring("napps://localhost/?code=".Length);
    //    //  string tokenpath =
    //    //    string.Format(
    //    //      "https://{0}:9031/as/token.oauth2?client_id=ssoclient&redirect_uri=napps://localhost/&response_type=code&state=1234&code={1}&grant_type=authorization_code",
    //    //      idmIp, code);

    //    //  var tokenResponse = CreateWebRequest(tokenpath, REQMETHOD.POST).GetResponse();
    //    //  token = GetToken(tokenResponse);
    //    //}
    //  }
    //  catch (Exception ex)
    //  {
    //    Error("Problem with taking token. Exception: " + ex.Message);
    //  }

    //  return token;
    //}

    //public void AddPostData(HttpWebRequest request, string postData)
    //{
    //  byte[] postArray = Encoding.ASCII.GetBytes(postData);
    //  request.ContentLength = postArray.Length;
    //  Stream reqStream = request.GetRequestStream();
    //  reqStream.Write(postArray, 0, postArray.Length);
    //  reqStream.Close();
    //}


  }

}