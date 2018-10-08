using Newtonsoft.Json;
using System;
using System.Text;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;

namespace GovQAUpdate
{
  public class AccessToken
  {
    //public string Domain { get; set; }
    //public bool IsMustChangePassword { get; set; }
    //public int LicenseExpiryDays { get; set; }
    //public string Name { get; set; }
    public string current_session_id { get; set; } = "";
    public int staff_id { get; set; } = -1;
    public string Version { get; set; } = "";

    public string APIVersion
    {
      get
      {
        if (Version.Length == 0)
        {
          return "";
        }
        else
        {
          return Version.Substring(0, 5).Replace(".", "_");
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
          // TODO: code to set headers if already logged in
        }
        return whc;
      }
    }
    public string URL
    {
      get
      {
        var url = new StringBuilder("http://claycountyfl.webqaservices.com/PROD/api/");
        if (current_session_id.Length == 0)
        {
          url.Append("MobileUserLoginAdmin?");
        }
        else
        {
          // TODO: switch statement to set api call to update or get GovQA data and set session id
        }
        return url.ToString();
      }
      set
      {

      }
    }

    // TODO: figure out a way to add the header from here

    public AccessToken()
    {
      Authenticate();
    }


    public string GetLoginURL(AccessToken token = null)
    {
      string activationKey = Properties.Resources.ActivationKey;
      string authKey = Properties.Resources.AuthKey;

      return $"http://claycountyfl.webqaservices.com/PROD/api/MobileUserLoginAdmin?AuthKey=" + Properties.Resources.AuthKey + "&ActivationKey=" + Properties.Resources.ActivationKey;
    }


    public AccessToken Authenticate()
    {
      string json = Program.GetJSON(GetLoginURL(), Headers, (current_session_id.Length == 0 ? "POST" : "GET")).ToString();
      if (json != null)
      {
        return JsonConvert.DeserializeObject<AccessToken>(json);
      }
      else
      {
        return null;
      }
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