using System;
using Newtonsoft.Json;
using System.Linq;
using System.Net.Http;
using System.Collections.Generic;
using Dapper;

namespace GovQAUpdate
{
  public class GovQAControl
  {

    private AccessToken Token { get; set; }
    private DateTime DataTimeOut { get; set; }
    private List<GovQARecord> Records { get; set; }
    public string BaseStatusUpdateURI => Token.GetUri("update_status");
    public string BaseNoteUpdateURI => Token.GetUri("update_note");
    public string BaseValidateIssueURI => Token.GetUri("validate_issue");

    public GovQAControl()
    {
      if (Token == null)
      {
        RenewToken();

        if (Token.valid_token)
        {
          Records = GovQARecord.GetRecordsToUpdate();
        }

      }
    }

    public List<int> Update()
    {
      var updatedIssues = new List<int>();
      if (Token.valid_token)
      {
        foreach (var r in Records)
        {
          if (IsValidIssue(r))

          {
            Console.WriteLine("No Error");

            //string json = Program.GetJSON(Program.CreateWebRequest(BaseStatusUpdateURI + r.issue_id, Token.Headers, "GET")).ToString();
            //if (json != null)
            //{
            //  var tokenObject = JsonConvert.DeserializeObject<string>(json);
            //  if (tokenObject == "Success")
            //  {
            //    updatedIssues.Add(r.issue_id);
            //  }

            //}
          }
          else
          {
            Console.WriteLine("Error, most likely invalid reference_no");
          }
        }
      }
      return updatedIssues;
    }

    private void RenewToken()
    {
      Token = new AccessToken().Login();
      Console.WriteLine("Completed Login");
    }

    private bool IsValidIssue(GovQARecord r)
    {
      var uri = BaseValidateIssueURI + "&referenceNo=" + r.reference_no;
      try
      {
        var json = Program.GetJSON(Program.CreateWebRequest(uri, Token.Headers, "GET")).ToString();
        if (json != null)
        {
          var tokenObject = JsonConvert.DeserializeObject<GovQARecord>(json);
          if (tokenObject.reference_no == r.reference_no)
          {
            return true;
          }

        }
      }
      catch (Exception ex)
      {

        // TODO: This is where we would set valid flag in DB to false;

      }

      return false;
    }

  }
}
