using System;
using System.Net.Http;
using System.Collections.Generic;

namespace GovQAUpdate
{ 
  public class GovQAControl
  {
    private AccessToken Token { get; set; }
    private DateTime DataTimeOut { get; set; }
    private bool isExpired { get; set; }

    public GovQAControl()
    {
      Token = new AccessToken();
    }

    public static HttpClient GetClientRequest()
    {
      var hcr = new HttpClient
      {
        BaseAddress = new Uri("http://claycountyfl.webqaservices.com/PROD/api/")
      };

      return new HttpClient();
    }

    public List<GovQARecord> Update()
    {

      return new List<GovQARecord>();
    }

    public string GetTokenLoginURL()
    {
      return Token.GetLoginURL();
    }
    public System.Net.WebHeaderCollection GetTokenHeaders()
    {
      return Token.Headers;
    }
  }
}
