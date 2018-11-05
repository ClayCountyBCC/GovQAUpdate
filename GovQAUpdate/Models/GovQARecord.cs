using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using Newtonsoft.Json;
using Dapper;
using System.Data.SqlClient;
using System.Data;
using System.Configuration;

using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;

namespace GovQAUpdate
{
  [JsonObject("Data")]
  public class GovQARecord
  {
    // Single beginning alpha, followed by six digits then the date format: MMDDYY (ex. W000123-042818))
    public string reference_no { get; set; } = "";

    // numerical data after initial alpha in reference number this is used as the actual data GovQA expects for the update
    public int issue_id { get; set; } = -1;
    public string animal_services_id { get; set; } = "";
    public string public_works_id { get; set; } = "";
    public DateTime activity_date { get; set; } = DateTime.MinValue;
    public DateTime create_date { get; set; } = DateTime.MinValue;

    public int service_request_status_id { get; set; } = -1; // 24 = "open"; 34 = "in progress"; 44 = "complete"; 45 = "Reopened"
    public string username { get; set; } = ""; // public Note notes { get; set; } = new Note();
    public int note_id { get; set; } = -1;
    public string note_text { get; set; } = "";
    public List<Note> notes { get; set; } = new List<Note>();

    public GovQARecord()
    {

    }

    public static List<GovQARecord> GetRecordsToUpdate()
    {
      var badRecords = new List<GovQARecord>();
      var goodRecords = new List<GovQARecord>();


      goodRecords.AddRange(GetPubWorksRecords());
      goodRecords.AddRange(GetAnimalServicesRecords());

      foreach (var r in goodRecords)
      {
        if (r.reference_no.Length == 0)
        {
          // TODO: set reference_no 
          if (r.animal_services_id.Length > 0)
          {
            // Just in case they add something not expected
            // r.reference_no = Regex.Match(r.animal_services_id, reference_number_pattern).Value;
          }

          else
          {
            badRecords.Add(r);
            goodRecords.Remove(r);
          }
        }

      }

      // TODO: email bad records to dev team


      return goodRecords;
    }

    public static List<GovQARecord> GetPubWorksRecords()
    {

      var records = new List<GovQARecord>();

      var query = @"
        USE PubWorks;
        WITH PubWorksReferenceNums AS (
        SELECT TOP 5000 
          N.ID note_id,
          N.cscID  public_works_id, 
          U.[name] username,
          N.note note_text, 
          cast(Cast(C.CallDate AS DATE) as varchar(14)) + ' ' + cast(Cast(C.CallTime AS TIME) as varchar(12)) create_date,
          CASE WHEN C.CompletionDate IS NOT NULL THEN 1 ELSE 0 END completed
        FROM cscAction N
        INNER JOIN csc C ON N.cscID = C.ID
        INNER JOIN usr U ON N.UserID = U.ID
        WHERE 
          N.Note LIKE
          '[WUP][0123456789][0123456789][0123456789][0123456789][0123456789][0123456789]-[0123456789][0123456789][0123456789][0123456789][0123456789][0123456789]'
        ORDER BY N.ID DESC)

        -- A.note_id, A.animal_services_id, null public_works_id, A.activity_seq, A.reference_no, A.activity_date
        SELECT NULL reference_no, NULL animal_services_id, public_works_id, create_date activity_date, note_id, note_text, username FROM PubWorksReferenceNums
        WHERE Completed = 1
        ORDER BY public_works_id
      
      ";
      try
      {

        records.AddRange(Program.Get_Data<GovQARecord>(query, Properties.Resources.Prod_PubWorks_Connection_String));

        foreach (var r in records)
        {
          r.SetReference_and_IssueId();

          if (r.note_id != -1)
          {
            // r.GetNote();
          }
        }


        return records;

      }

      catch (Exception ex)
      {
        new ErrorLog(ex, query);
        return new List<GovQARecord>();

      }
    }

    public static List<GovQARecord> GetAnimalServicesRecords()
    {
      var records = new List<GovQARecord>();

      var query = @"
          USE ANIMAL;

          WITH AllReferenceNumbers AS (
          SELECT 
            activity_identity note_id
            ,[activity_no] [animal_services_id]
            ,[activity_seq]
            ,[extra5] [reference_no]
            ,activity_date
            ,userid
          FROM [Animal].[SYSADM].[activity]
          WHERE extra5 LIKE 
          '[WUP][0123456789][0123456789][0123456789][0123456789][0123456789][0123456789]-[0123456789][0123456789][0123456789][0123456789][0123456789][0123456789]'
          --AND activity_stat NOT IN ('COMPLETED')
          --ORDER BY activity_no, activity_seq
          ), IncompleteReferenceNumbers AS (
          SELECT 
            activity_identity note_id,
            [activity_no] [animal_services_id]
            ,[activity_seq]
            ,[extra5] [reference_no]
            ,activity_date
            ,userid
          FROM [Animal].[SYSADM].[activity] A
          WHERE extra5 LIKE 
          '[WUP][0123456789][0123456789][0123456789][0123456789][0123456789][0123456789]-[0123456789][0123456789][0123456789][0123456789][0123456789][0123456789]'
          AND activity_stat NOT IN ('COMPLETED'))

          SELECT * -- A.reference_no, A.animal_services_id, null public_works_id, A.activity_date, A.note_id, A.userid username
          FROM AllReferenceNumbers A
          WHERE A.reference_no NOT IN (SELECT reference_no FROM IncompleteReferenceNumbers)
      
      ";
      try
      {

        records.AddRange(Program.Get_Data<GovQARecord>(query, Properties.Resources.Prod_AnimalServices_Connection_String));

        foreach (var r in records)
        {
          r.SetReference_and_IssueId();

          if (r.note_id != -1)
          {
            // r.GetNote();
          }
        }


        return records;

      }

      catch (Exception ex)
      {
        new ErrorLog(ex, query);
        return new List<GovQARecord>();

      }
    }

    public static int InsertRecordsIntoReference_Table()
    {
      // TODO: write code to lock the GovQA ids in table. return number of ids inserted into lock Table

      return -1;
    }

    public static int UpdateClosed_Reference_Numbers_Table()
    {
      // TODO: write code to update the date and time GovQA ticket is closed successfully
      return -1;
    }

    public void SetReference_and_IssueId()
    {
      var r = this;
      var reference_number_pattern = @"[WUP][0-9]{6}[-][0-9]{6}";

      if (public_works_id.Length > 0 || (animal_services_id.Length > 0 && reference_no.Length == 0))
      {
        reference_no = Regex.Match(note_text, reference_number_pattern).Value;
      }

      if (reference_no.Length > 0)
      {
        issue_id = int.Parse(reference_no.Substring(1, reference_no.Split('-')[0].Length - 1));
      }

    }

    private static List<GovQARecord> CreateTestRecord()
    {

      var testRecord = new List<GovQARecord>();
      //var n = new Note()
      //{
      //  note = "New Test Note; date: " + DateTime.Now.ToLongDateString(),
      //  // reference_number_id = "W000730-102410"
      //   reference_number_id = "W000730-102418"

      //};
      var records = new List<string>();

      records.Add("W000716-102218");


      foreach (var r in records)
      {
        testRecord.Add(new GovQARecord()
        {
          // reference_no = "W000730-102410",
          reference_no = r,

          service_request_status_id = 24,
          //note = n
        });

      }

      return testRecord;
    }

    public int SetReferenceNumberInvalid()
    {

      var param = new DynamicParameters();
      param.Add("@reference_no", reference_no);

      var sql = @"
        USE ClayGovQA;

        update GovQA_Reference_Table
        SET valid = 0
        WHERE REFERENCE_NO = @reference_no
      
      ";
      // using connection here to connect to the DB and run the query
      return -1;
    }

    public List<Note> GetNote()
    {

      if (public_works_id.Length > 0)
      {
        // Public works only has one note per record so the note is already in the data
        notes = new List<Note>();
        notes.Add(new Note() { note_id = this.note_id, note_text = this.note_text });
        return notes;
      }


      if (animal_services_id.Length > 0)
      {
        
        var param = new DynamicParameters();
        param.Add("@note_id", note_id);
        param.Add("@animal_services_id", animal_services_id);
        
        var query = @"
        
        
        ";
        try
        {
          
          notes = Program.Get_Data<Note>(query, param, Properties.Resources.Prod_ClayGovQA_Connection_String);
        }

        catch (Exception ex)
        {
          new ErrorLog(ex, query);
          return new List<Note>();

        }
      }

      return new List<Note>();
    }


  }


}