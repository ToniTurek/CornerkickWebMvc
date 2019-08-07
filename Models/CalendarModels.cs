using System;
using System.Collections.Generic;
using System.Globalization; // << dont forget to add this for converting dates to localtime
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CornerkickWebMvc.Models
{
  public class Testgame
  {
    public DateTime dt { get; set; }
    public int iTeamHome { get; set; }
    public int iTeamAway { get; set; }
  }

  public class CalendarModels
  {
    public int iClubId { get; set; }

    //public IList<string> sCal { get; set; }
    public DateTime dtToday { get; set; }

    public string id { get; set; }
    public string title { get; set; }
    public string date { get; set; }
    public string start { get; set; }
    public string end { get; set; }
    public string url { get; set; }

    public bool allday { get; set; }

    public List<Testgame> ltTestgames { get; set; }

    public CornerkickManager.TrainingCamp.Camp camp { get; set; }
  }

  public class DiaryEvent
  {
    public int iID;
    public string sTitle;
    public string sDescription;
    public int SomeImportantKeyID;
    public string sStartDate;
    public string sEndDate;
    public string StatusString;
    public string sColor;
    public string sBackgroundColor;
    public string sBorderColor;
    public string sTextColor;
    public bool bEditable;
    public bool bAllDay;
    public string ClassName;
  }
}