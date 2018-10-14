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
      var records = new List<GovQARecord>();
      records.Add(new GovQARecord() { issue_id = 129, note = "this is a new note" });
      var closedIssues = new List<int>();
      if (Token.valid_token)
      {
        foreach (var r in records)
        {
          if(!IsValidIssue(r)) continue;

          string json = Program.GetJSON(Program.CreateWebRequest(BaseStatusUpdateURI + r.issue_id, Token.Headers, "GET")).ToString();
          if (json != null)
          {
            var tokenObject = JsonConvert.DeserializeObject<string>(json);
            if (tokenObject == "Success")
            {
              closedIssues.Add(r.issue_id);
            }

          }

          if (r.note.Length > 0)
          {
            json = Program.GetJSON(Program.CreateWebRequest($@"{BaseNoteUpdateURI}{r.issue_id}&note={r.note}", Token.Headers, "GET")).ToString();
            if (json != null)
            {
              var tokenObject = JsonConvert.DeserializeObject<string>(json);
              if (tokenObject == "Success")
              {
                closedIssues.Add(r.issue_id * -1);
              }

            }
          }

        }
      }
      return closedIssues;
    }

    private void RenewToken()
    {
      Token = new AccessToken().Login();
      Console.Write("Completed Login");
    }

    private bool IsValidIssue(GovQARecord r)
    {
      var uri = BaseValidateIssueURI + "&r.reference_no=" + r.reference_no;
      var json = Program.GetJSON(Program.CreateWebRequest(uri, Token.Headers, "GET")).ToString();
      if (json != null)
      {
        var tokenObject = JsonConvert.DeserializeObject<GovQARecord>(json);
        if (tokenObject != null)
        {
          return true;
        }

      }
      return false;
    }
  }
}
