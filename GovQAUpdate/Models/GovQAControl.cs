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
      }

      if (Token.valid_token && (Records == null || !Records.Any()))
      {
        // Step 1: Get Records to update
        Records = GovQARecord.GetRecordsToUpdate();

        // if there are no records, quit application
        if (Records.Count() == 0) return;


        // Step 2: Process Records
        foreach (var r in Records)
        {
          r.SetReference_and_IssueId();

          if (r.note_id != -1)
          {
            // r.GetNote();
          }

        }

        var rs = GovQARecord.InsertRecordsIntoReference_Table(Records);
        // Step 3: Insert Records into Reference number table
        if (rs.Count() > 0)
        {
          string failedInserts = "";
          foreach (var r in rs)
          {
            failedInserts += r + "\n";
          }


          new ErrorLog(
          "There are reference numbers not inserted into reference_number_table",
          "The reference numbers not correctly inserted are:\n" + failedInserts,
          "GovQARecord.InsertRecordsIntoReference_Table()", "", "");

        }

        // Step 4: Validate Records 
        foreach (var r in Records)
        {
          if (!rs.Contains(r.reference_no))
          {
            r.SetIsValidReferenceNumber(IsValidIssue(r));


          }

        }





        Console.WriteLine("pause spot");


      }


    }

    public bool Update()
    {
      GovQARecord.UpdateClosed_Reference_Numbers_Table();
      var issueIdsToClose = new List<int>();
      var closedIssues = new List<int>();

      // pull issues to close
      issueIdsToClose.AddRange(GovQARecord.GetIssueIdsToUpdate());
      // GET ISSUES TO UPDATE
      //
      if (Token.valid_token)
      {
        foreach (var id in issueIdsToClose)
        {

          string json = Program.GetJSON(Program.CreateWebRequest(BaseStatusUpdateURI + id, Token.Headers, "GET"));
          if (json != null)
          {
            var tokenObject = JsonConvert.DeserializeObject<string>(json);
            if (tokenObject == "Success")
            {
              closedIssues.Add(id);
            }

          }
          else
          {
            new ErrorLog("GovQA reference number " + id + "status not updated in GovQA", id + " status was not updated", "", "", "");

          }

        }
      }

      return UpdateAllClosedStatus(closedIssues) == closedIssues.Count();


    }

    private void RenewToken()
    {
      Token = new AccessToken().Login();
    }

    private bool IsValidIssue(GovQARecord r)
    {
      var uri = BaseValidateIssueURI + "&referenceNo=" + r.reference_no;

      try
      {
        string json = Program.GetValidateJSON(Program.CreateWebRequest(uri, Token.Headers, "GET"));

        if (json != null)
        {
          var tokenObject = JsonConvert.DeserializeObject<GovQARecord>(json);
          if (tokenObject.reference_no == r.reference_no)
          {
            return true;
          }
          else
          {
            return false;
          }

        }
      }
      catch (Exception ex)
      {
        Token.valid_token = false;

        // TODO: here we call function to email notification of invalid ref #

      }

      return false;
    }

    public int UpdateAllClosedStatus(List<int> issue_id)
    {

      var numberOfIssues = 0;

      var query = @"
      
        USE ClayGovQA;
        DECLARE @date_closed DATETIME = GETDATE();

        WITH reference_numbers AS (
        SELECT reference_number FROM GovQA_Reference_Number_Table
        WHERE issue_id IN (@issue_id))


        UPDATE GovQA_Closed_Reference_Numbers
        SET [status] = 'CLOSED', date_closed = @date_closed
        WHERE reference_number IN (SELECT reference_number FROM reference_numbers)


      ";

      foreach (var id in issue_id)
      {
        var param = new DynamicParameters();
        param.Add("@issue_id", id);
        try
        {
          var i = Program.Exec_Query(query, param, Properties.Resources.Prod_ClayGovQA_Connection_String);
          numberOfIssues++;
          Console.Write(i);
        }
        catch (Exception ex)
        {
          new ErrorLog("GovQA reference number " + id + " did not correctly update in GovQA_Closed_Reference_Numbers table", id + " status was not updated", "", "", "");
        }

      }


      return numberOfIssues;


    }

  }
}
