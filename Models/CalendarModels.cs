using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Globalization; // << dont forget to add this for converting dates to localtime

namespace CornerkickWebMvc.Models
{
  public class CalendarModels
  {
    //public IList<string> sCal { get; set; }
    public DateTime dtToday { get; set; }

    public string id { get; set; }
    public string title { get; set; }
    public string date { get; set; }
    public string start { get; set; }
    public string end { get; set; }
    public string url { get; set; }

    public bool allday { get; set; }
  }

  public class DiaryEvent
  {
    public int iID;
    public string sTitle;
    public int SomeImportantKeyID;
    public string sStartDate;
    public string sEndDate;
    public string StatusString;
    public string sColor;
    public string sBackgroundColor;
    public string sBorderColor;
    public string sTextColor;
    public bool bEditable;
    public string ClassName;

    public static List<DiaryEvent> getCalendarEvents()
    {
      List<DiaryEvent> ltEvents = new List<DiaryEvent>();

      if (Controllers.AccountController.ckUser.iTeam >= MvcApplication.ckcore.ltClubs.Count) return ltEvents;

      CornerkickCore.Core.Club club = MvcApplication.ckcore.ltClubs[Controllers.AccountController.ckUser.iTeam];

      //DateTime dt = new DateTime(MvcApplication.ckcore.dtDatum.Year, MvcApplication.ckcore.dtDatum.Month, MvcApplication.ckcore.dtDatum.Day);
      DateTime dt = MvcApplication.ckcore.dtSaisonstart.Date;
      while (dt.CompareTo(MvcApplication.ckcore.dtSaisonende) < 0) {
        // Training
        if ((int)dt.DayOfWeek > 0 && (int)dt.DayOfWeek < 6) {
          DateTime dtTmp = new DateTime(dt.Year, dt.Month, dt.Day, 10, 00, 00);

          ltEvents.Add(new DiaryEvent {
            iID = ltEvents.Count,
            sTitle = " Training",
            sStartDate = dtTmp.ToString("yyyy-MM-ddTHH:mm:ss"),
            sEndDate = dtTmp.AddMinutes(30).ToString("yyyy-MM-ddTHH:mm:ss"),
            sColor = "rgb(255, 200, 0)",
            bEditable = false
          });
        }

        // Liga
        for (int iSpTg = 0; iSpTg < MvcApplication.ckcore.ltDtSpieltagLiga[club.iLand][club.iSpielklasse].Count; iSpTg++) {
          DateTime dtTmp = MvcApplication.ckcore.ltDtSpieltagLiga[club.iLand][club.iSpielklasse][iSpTg];
          if (dt.Date.Equals(dtTmp.Date)) {
            string sH = "";
            string sA = "";
            for (int iB = 0; iB < MvcApplication.ckcore.ltErgebnisseLiga[club.iLand][club.iSpielklasse][iSpTg].Count; iB++) {
              if (MvcApplication.ckcore.ltErgebnisseLiga[club.iLand][club.iSpielklasse][iSpTg][iB][0] == club.iID) {
                sH = club.sName;
                sA = MvcApplication.ckcore.ltClubs[MvcApplication.ckcore.ltErgebnisseLiga[club.iLand][club.iSpielklasse][iSpTg][iB][1]].sName;
                break;
              } else if (MvcApplication.ckcore.ltErgebnisseLiga[club.iLand][club.iSpielklasse][iSpTg][iB][1] == club.iID) {
                sH = MvcApplication.ckcore.ltClubs[MvcApplication.ckcore.ltErgebnisseLiga[club.iLand][club.iSpielklasse][iSpTg][iB][0]].sName;
                sA = club.sName;
              }
            }

            ltEvents.Add(new DiaryEvent {
              iID = ltEvents.Count,
              sTitle = " Liga, " + (iSpTg + 1).ToString().PadLeft(2) + ". Spieltag: " + sH + " vs. " + sA,
              sStartDate = dtTmp.ToString("yyyy-MM-ddTHH:mm:ss"),
              sEndDate = dtTmp.AddMinutes(105).ToString("yyyy-MM-ddTHH:mm:ss"),
              sColor = "rgb(0, 100, 200)",
              bEditable = false
            });

            break;
          }
        }

        // Cup
        foreach (CornerkickCore.Core.Cup cup in MvcApplication.ckcore.ltCups) {
          int iSpTg = 0;
          foreach (CornerkickCore.Core.GameDay gameday in cup.ltGameday) {
            if (dt.Date.Equals(gameday.dt.Date)) {
              string sH = "";
              string sA = "";
              for (int iB = 0; iB < gameday.ltResult.Count; iB++) {
                if (gameday.ltResult[iB][0] == club.iID) {
                  sH = club.sName;
                  sA = MvcApplication.ckcore.ltClubs[gameday.ltResult[iB][1]].sName;
                  break;
                } else if (gameday.ltResult[iB][1] == club.iID) {
                  sH = MvcApplication.ckcore.ltClubs[gameday.ltResult[iB][0]].sName;
                  sA = club.sName;
                }
              }

              ltEvents.Add(new DiaryEvent {
                iID = ltEvents.Count,
                sTitle = " " + cup.sName + ", " + (iSpTg + 1).ToString().PadLeft(2) + ". Spieltag: " + sH + " vs. " + sA,
                sStartDate = dt.ToLongDateString(),
                sEndDate = dt.AddMinutes(105).ToLongDateString(),
                sColor = "rgb(0, 175, 100)",
                bEditable = false
              });

              break;
            }
            iSpTg++;
          }
        }

        dt = dt.AddDays(1);
      }

      return ltEvents;
    }
  }
}