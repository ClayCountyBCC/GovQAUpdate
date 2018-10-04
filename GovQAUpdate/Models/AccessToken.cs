using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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
        if(current_session_id.Length == 0)
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
      

    }


    public string GetLoginURL(AccessToken token = null)
    {
      string activationKey = Properties.Resources.ActivationKey;
      string authKey = Properties.Resources.AuthKey;

      return $"http://claycountyfl.webqaservices.com/PROD/api/MobileUserLoginAdmin?authKey={authKey}&activationKey={activationKey}";
    }


    public static AccessToken Authenticate()
    {
      var token = new AccessToken();
      
      string json = Program.GetJSONAsync(token.URL, token.Headers).ToString();
      if (json != null)
      {
        return JsonConvert.DeserializeObject<AccessToken>(json);
      }
      else
      {
        return null;
      }
    }
    
  }

}