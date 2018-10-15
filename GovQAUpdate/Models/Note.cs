using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GovQAUpdate
{
  public class Note
  {
    public string reference_number_id { get; set; }
    public string note { get; set; }

    public Note()
    {

    }


    public static List<Note> GetNotes()
    {
      
      return new List<Note>();
    }

    //private bool UpdateNotes()
    //{
    //  foreach (var n in r.note)
    //  {
    //    if (n.note.Length > 0)
    //    {
    //      json = Program.GetJSON(Program.CreateWebRequest($@"{BaseNoteUpdateURI}{r.issue_id}&note={r.note}", Token.Headers, "GET")).ToString();
    //      if (json != null)
    //      {
    //        var tokenObject = JsonConvert.DeserializeObject<string>(json);
    //        if (tokenObject == "Success")
    //        {
    //          closedIssues.Add(r.issue_id * -1);
    //        }

    //      }
    //    }
    //  }
    //}
  }
}
