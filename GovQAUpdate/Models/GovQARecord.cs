using System;
using System.Collections.Generic;
using Newtonsoft.Json;
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

    public DateTime create_date { get; set; } = DateTime.MinValue;
    public int service_request_status_id { get; set; } = -1; // 24 = "open"; 34 = "in progress"; 44 = "complete"; 45 = "Reopen
    public List<Note> note { get; set; }

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

      return records;
    }

    public static int LockGovQAIds()
    {
      // TODO: write code to lock the GovQA ids in table. return number of ids inserted into lock Table

      return -1;

    }

    public static int UpdateLockTableDate()
    {
      // TODO: write code to update the date and time GovQA ticket is closed successfully
      return -1;
    }

  }
}
