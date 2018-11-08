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
    // public List<Note> notes { get; set; } = new List<Note>();

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
          if (r.animal_services_id.Length == 0 && r.public_works_id.Length == 0)
          {
            // Just in case they add something not expected
            // r.reference_no = Regex.Match(r.animal_services_id, reference_number_pattern).Value;
          }

          else
          {
            badRecords.Add(r);
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

        USE ClayGovQA;
        DECLARE @min INT = (SELECT L.PubWorks_Last_Checked_cscID FROM GovQA_Last_Checked_Lookup L)
        
        USE PubWorks;

        SELECT 
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
          '%[WUP][0123456789][0123456789][0123456789][0123456789][0123456789][0123456789]-[0123456789][0123456789][0123456789][0123456789][0123456789][0123456789]%'
        AND CallDescription NOT LIKE '%THIS IS A TEST%'
        AND N.ID > @min


      ";
      try
      {

        records = Program.Get_Data<GovQARecord>(query, Properties.Resources.Prod_PubWorks_Connection_String);

        if (records == null)
        {
          return new List<GovQARecord>();
        }
        if (records.Count() > 0) { SetLastChecked("pubworks", records.Last().note_id); }

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
        
        USE ClayGovQA;

        DECLARE @min INT = (SELECT L.AnimalControl_Last_Checked_Activity_identity FROM GovQA_Last_Checked_Lookup L)

        SELECT 
          activity_identity note_id,
          CAST([activity_no] AS VARCHAR(10)) + '-' + CAST(activity_seq AS VARCHAR(2)) animal_services_id 
          ,[activity_seq]
          ,[extra5] reference_no
          ,activity_date
          ,userid username
        FROM [Animal].[SYSADM].[activity]
        WHERE extra5 LIKE 
          '[WUP][0123456789][0123456789][0123456789][0123456789][0123456789][0123456789]-[0123456789][0123456789][0123456789][0123456789][0123456789][0123456789]'
          AND activity_identity > @min
        
      ";
      try
      {

        records = Program.Get_Data<GovQARecord>(query, Properties.Resources.Prod_AnimalServices_Connection_String);


        if (records == null)
        {
          return new List<GovQARecord>();
        }

        if (records.Count() > 0) { SetLastChecked("animal", records.Last().note_id); }


        return records;

      }

      catch (Exception ex)
      {
        new ErrorLog(ex, query);
        return new List<GovQARecord>();

      }
    }

    public static List<string> InsertRecordsIntoReference_Table(List<GovQARecord> records)
    {
      // TODO: write code to lock the GovQA ids in table. return number of ids inserted into lock Table

      var badEntries = new List<string>();


      var query = @"
        USE ClayGovQA;
        INSERT INTO GovQA_Reference_Number_Table
        (reference_number,issue_id, animal_services_id, public_works_id, reference_date, added_by)

          SELECT 
            @reference_no reference_number, 
            @issue_id issue_id,
            @animal_services_id animal_services_id, 
            @public_works_id public_works_id, 
            @activity_date reference_date, 
            @username added_by 

      
      ";

      records = records.OrderBy(r => r.issue_id).ToList();

      foreach (var r in records)
      {
        var param = new DynamicParameters();

        param.Add("@reference_no", r.reference_no);
        param.Add("@issue_id", r.issue_id);
        param.Add("@animal_services_id", r.animal_services_id);
        param.Add("@public_works_id", r.public_works_id);
        param.Add("@activity_date", r.create_date == DateTime.MinValue ? r.activity_date : r.create_date);
        param.Add("@username", r.username);

        try
        {

          var i = Program.Exec_Scalar(query, param, Properties.Resources.Prod_ClayGovQA_Connection_String);

          if (i == -1)
          {
            badEntries.Add(r.reference_no);
          }
        }
        catch (Exception ex)
        {
          new ErrorLog(ex, query);

        }



      }



      return badEntries;
    }

    public static bool UpdateClosed_Reference_Numbers_Table()
    {
      // TODO: write code to update the date and time GovQA ticket is closed successfully
      var query = @"
  
        USE ClayGovQA;

        WITH AllReferenceNumbers AS (
          SELECT 
            activity_identity,
            [activity_no] animal_services_id
            ,[activity_seq]
            ,[extra5]
            ,activity_date
            ,userid
          FROM [Animal].[SYSADM].[activity]
          WHERE extra5 LIKE 
          '[WUP][0123456789][0123456789][0123456789][0123456789][0123456789][0123456789]-[0123456789][0123456789][0123456789][0123456789][0123456789][0123456789]')
          , IncompleteActivityNumbers AS (
          SELECT 
            AN.activity_identity,
            AN.[activity_no] animal_services_id
            ,AN.[activity_seq]
            ,AN.[extra5]
            ,AN.activity_date
            ,AN.userid
          FROM [Animal].[SYSADM].[activity] AN
          WHERE AN.activity_stat NOT IN ('COMPLETED'))
          ,all_pubworks_reference_numbers AS (
          SELECT
            N.ID NoteID, 
            N.cscID public_works_id, 
            U.Name username,
            N.Note,
            CASE WHEN C.CompletionDate IS NOT NULL THEN 1 ELSE 0 END Completed
          FROM PubWorks.dbo.cscAction N
          INNER JOIN PubWorks.dbo.csc C ON N.cscID = C.ID
          INNER JOIN PubWorks.dbo.usr U ON N.UserID = U.ID
          WHERE 
            N.Note LIKE
            '[WUP][0123456789][0123456789][0123456789][0123456789][0123456789][0123456789]-[0123456789][0123456789][0123456789][0123456789][0123456789][0123456789]')

        -- UNCOMMENT NEXT TWO LINES
        INSERT INTO GovQA_Closed_Reference_Numbers
        (data_id, reference_number)

        SELECT * FROM (
            SELECT   
              RT.animal_services_id data_id,
              RT.reference_number 
            FROM GovQA_Reference_Number_Table RT
            INNER JOIN  AllReferenceNumbers A ON A.animal_services_id  + '-' + CAST(ACTIVITY_SEQ AS VARCHAR(4)) = RT.animal_services_id
            WHERE A.animal_services_id NOT IN (SELECT animal_services_id FROM IncompleteActivityNumbers)
    
            UNION

            SELECT 
              'P' + CAST(A.public_works_id AS VARCHAR(10)), 
              RT.reference_number 
            FROM 
              GovQA_Reference_Number_Table RT
            LEFT OUTER JOIN all_pubworks_reference_numbers A ON A.public_works_id = RT.public_works_id
            WHERE A.public_works_id IS NOT NULL
              AND COMPLETED = 1 ) AS TMP
            ORDER BY LEFT(reference_number,7)
      ";
           
      try
      {

        var i = Program.Exec_Query(query, Properties.Resources.Prod_ClayGovQA_Connection_String);

        return i != -1;
      }
      catch (Exception ex)
      {
        new ErrorLog(ex, query);
        return false;
      }
      
    }

    public static List<int> GetIssueIdsToUpdate()
    {

      var query = @"
        USE ClayGovQA;

        SELECT DISTINCT r.issue_id FROM 
        GovQA_Closed_Reference_Numbers C 
        LEFT OUTER JOIN GovQA_Reference_Number_Table R ON C.reference_number = R.reference_number
        WHERE C.[status] != 'CLOSED'
           OR C.[status] IS NULL      ";

      try
      {
        var i = Program.Get_Data<int>(query, Properties.Resources.Prod_ClayGovQA_Connection_String);

        if(i != null)
        {
          return i;
        }
      }
      catch(Exception ex)
      {
        new ErrorLog(ex, query);
      }
      return new List<int>();
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

    public bool SetIsValidReferenceNumber(bool recordIsValid)
    {

      var param = new DynamicParameters();
      param.Add("@reference_no", reference_no);

      param.Add("@is_valid_record", recordIsValid);

      var query = @"
        USE ClayGovQA;

        update GovQA_Reference_Number_Table
        SET valid = @is_valid_record
        WHERE reference_number = @reference_no
      
      ";

      var i = Program.Exec_Scalar(query, param, Properties.Resources.Prod_ClayGovQA_Connection_String);

      // using connection here to connect to the DB and run the query
      return i != -1;
    }

    public static void SetLastChecked(string department, int record_id)
    {
      var param = new DynamicParameters();
      param.Add("@record_id", record_id);

      var query = @"
        USE ClayGovQA;

        UPDATE GovQA_Last_Checked_Lookup
        
      ";
      switch (department)
      {
        case "pubworks":
          query += "SET PubWorks_Last_Checked_cscID = @record_id";
          break;
        case "animal":
          query += "SET AnimalControl_Last_Checked_Activity_identity = @record_id";
          break;
      }


      try
      {
        var i = Program.Exec_Scalar(query, param, Properties.Resources.Prod_ClayGovQA_Connection_String);

        if (i == -1)
        {

        }

      }
      catch (Exception ex)
      {
        new ErrorLog(ex, query);

      }

    }

    //public List<Note> GetNote()
    //{

    //  if (public_works_id.Length > 0)
    //  {
    //    // Public works only has one note per record so the note is already in the data
    //    notes = new List<Note>();
    //    notes.Add(new Note() { note_id = this.note_id, note_text = this.note_text });
    //    return notes;
    //  }


    //  if (animal_services_id.Length > 0)
    //  {

    //    var param = new DynamicParameters();
    //    param.Add("@note_id", note_id);
    //    param.Add("@animal_services_id", animal_services_id);

    //    var query = @"


    //    ";
    //    try
    //    {

    //      notes = Program.Get_Data<Note>(query, param, Properties.Resources.Prod_ClayGovQA_Connection_String);
    //    }

    //    catch (Exception ex)
    //    {
    //      new ErrorLog(ex, query);
    //      return new List<Note>();

    //    }
    //  }

    //  return new List<Note>();
    //}


  }


}