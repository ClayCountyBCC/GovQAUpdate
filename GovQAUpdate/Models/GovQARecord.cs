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

    public int issue_id { get; set; } = -1;
    public string reference_no { get; set; } = ""; // Single beginning alpha, followed by six digits then the date format: MMDDYY (ex. W000123-042818))
    public int service_request_status_id { get; set; } = -1; // 24 = "open"; 34 = "in progress"; 44 = "complete"
    public string note { get; set; }
    
    
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
