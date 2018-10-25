using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Dapper;
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
    public int issue_id
    {
      get
      {
        if (reference_no.Length > 0)
        {
          return int.Parse(reference_no.Substring(1, reference_no.Split('-')[0].Length - 1));
        }
        else
        {
          return -1;
        }
      }
    }

    public string animal_control_id { get; set; } = "";
    public string public_works_id { get; set; } = "";
    public DateTime create_date { get; set; } = DateTime.MinValue;
    public int service_request_status_id { get; set; } = -1; // 24 = "open"; 34 = "in progress"; 44 = "complete"; 45 = "Reopened"
    public Note note { get; set; } = new Note();

    public GovQARecord()
    {

    }


    public static string BuildURL()
    {

      var url = "";

      return url;
    }

    public static List<GovQARecord> GetRecordsToUpdate()
    {
      var records = new List<GovQARecord>();

      var sql = @"
      
        
      ";
      records.Add(CreateTestRecord());

      return records;
    }

    public static int LockGovQAIds()
    {
      // TODO: write code to lock the GovQA ids in table. return number of ids inserted into lock Table

      return -1;
    }

    public static int UpdateQALockTable()
    {
      // TODO: write code to update the date and time GovQA ticket is closed successfully
      return -1;
    }

    private static GovQARecord CreateTestRecord()
    {
      var testRecord = new GovQARecord();
      var n = new Note()
      {
        note = "New Test Note; date: " + DateTime.Now.ToLongDateString(),
        reference_number_id = "W000730-102410"
        // reference_number_id = "W000730-102418"

      };

      testRecord = new GovQARecord()
      {
        reference_no = "W000730-102410",
        // reference_number_id = "W000730-102418",

        service_request_status_id = 24,
        note = n
      };

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
  }
}
