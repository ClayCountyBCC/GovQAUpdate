using System;
using System.Net.Http;
using System.Collections.Generic;

namespace GovQAUpdate.Models
{
  public class GovQAControl
  {
    private AccessToken Token { get; set; }
    private DateTime DataTimeOut { get; set; }
    private bool isExpired { get; set; }

    public GovQAControl()
    {
      Token = AccessToken.Authenticate();
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
  }
}
