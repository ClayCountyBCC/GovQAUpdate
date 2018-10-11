using System;
using Newtonsoft.Json;

using System.Net.Http;
using System.Collections.Generic;

namespace GovQAUpdate
{ 
  public class GovQAControl
  {

    private AccessToken Token { get; set; }
    private DateTime DataTimeOut { get; set; }
    private List<GovQARecord> Records { get; set; }
    public string LoginURI { get; set; }
    public string BaseStatusUpdateURI { get; set; }


    public GovQAControl()
    {
      if(Token == null)
      {
        RenewToken();
      }
    }


    public List<int> Update(string note = "")
    {
      var records = new List<GovQARecord>();
      records.Add(new GovQARecord() { issue_id = 129});
      var UpdatedIssueIds = new List<int>();
      var valid = Token.IsTokenValid();

      foreach (var r in records)
      {
        var uri = BaseStatusUpdateURI + r.issue_id;
        string json = Program.GetJSON(Program.CreateWebRequest(uri, Token.Headers, "GET")).ToString();
        if (json != null)
        {
          var tokenObject = JsonConvert.DeserializeObject<string>(json);
          if (tokenObject == "Success")
          {
            UpdatedIssueIds.Add(r.issue_id);
          }

        }
      }

      return UpdatedIssueIds;
    }

    public bool IsTokenValid()
    {
      return Token.IsTokenValid();
    }

    private void RenewToken()
    {
      Token = new AccessToken().Login();
      LoginURI = Token.GetLoginURL();
      BaseStatusUpdateURI = Token.GetSetCloseStatusURI();
      Console.Write("Completed Login");
    }
  }
}
