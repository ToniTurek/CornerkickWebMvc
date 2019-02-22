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

    public CornerkickCore.csTrainingCamp.Camp camp { get; set; }
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

    public static List<DiaryEvent> getCalendarEvents()
    {
      List<DiaryEvent> ltEvents = new List<DiaryEvent>();

      int iTeam = Controllers.AccountController.ckUser().iTeam;

      if (iTeam >= MvcApplication.ckcore.ltClubs.Count) return ltEvents;

      CornerkickCore.Core.Club club = MvcApplication.ckcore.ltClubs[iTeam];

      //DateTime dt = new DateTime(MvcApplication.ckcore.dtDatum.Year, MvcApplication.ckcore.dtDatum.Month, MvcApplication.ckcore.dtDatum.Day);
      DateTime dt = MvcApplication.ckcore.dtSaisonstart.Date;
      while (dt.CompareTo(MvcApplication.ckcore.dtSaisonende) < 0) {
        // New Year
        if (dt.Day == 1 && dt.Month == 1) {
          ltEvents.Add(new DiaryEvent {
            iID = ltEvents.Count,
            sTitle = "Neujahr",
            sDescription = "Neujahr",
            sStartDate = dt.ToString("yyyy-MM-ddTHH:mm:ss"),
            sColor = "rgb(200, 200, 200)",
            bEditable = false,
            bAllDay = true
          });
        }

        // Trainingscamp
        bool bCampTravelDay = false;
        CornerkickCore.csTrainingCamp.Booking booking = MvcApplication.ckcore.tcp.getCurrentCamp(club, dt, true);
        if (booking.camp.iId > 0) {
          if (dt.Date.Equals(booking.dtArrival.Date)) {
            ltEvents.Add(new DiaryEvent {
              iID = ltEvents.Count,
              sTitle = "Abreise Trainingslager",
              sDescription = booking.camp.sName,
              sStartDate = booking.dtArrival.ToString("yyyy-MM-ddTHH:mm:ss"),
              sEndDate = booking.dtArrival.AddHours(8).ToString("yyyy-MM-ddTHH:mm:ss"),
              sColor = "rgb(255, 100, 0)",
              bEditable = false,
              bAllDay = false
            });

            bCampTravelDay = true;
          } else if (dt.Date.Equals(booking.dtDeparture.Date)) {
            ltEvents.Add(new DiaryEvent {
              iID = ltEvents.Count,
              sTitle = "Rückreise Trainingslager",
              sDescription = booking.camp.sName,
              sStartDate = booking.dtDeparture.AddHours(-8).ToString("yyyy-MM-ddTHH:mm:ss"),
              sEndDate = booking.dtDeparture.ToString("yyyy-MM-ddTHH:mm:ss"),
              sColor = "rgb(255, 100, 0)",
              bEditable = false,
              bAllDay = false
            });

            bCampTravelDay = true;
          } else {
            ltEvents.Add(new DiaryEvent {
              iID = ltEvents.Count,
              sTitle = "Trainingslager",
              sDescription = booking.camp.sName,
              sStartDate = dt.ToString("yyyy-MM-ddTHH:mm:ss"),
              //sEndDate = dt.AddHours(23).ToString("yyyy-MM-ddTHH:mm:ss"),
              sColor = "rgb(255, 150, 0)",
              bEditable = false,
              bAllDay = true
            });
          }
        }

        // Training
        if (club.training.iTraining[(int)dt.DayOfWeek] > 0 && !bCampTravelDay) {
          DateTime dtTmp = new DateTime(dt.Year, dt.Month, dt.Day, 10, 00, 00);

          ltEvents.Add(new DiaryEvent {
            iID = ltEvents.Count,
            sTitle = " Training (" + MvcApplication.ckcore.sTraining[club.training.iTraining[(int)dt.DayOfWeek]] + ")",
            sDescription = MvcApplication.ckcore.sTraining[club.training.iTraining[(int)dt.DayOfWeek]],
            sStartDate = dtTmp.ToString("yyyy-MM-ddTHH:mm:ss"),
            sEndDate = dtTmp.AddMinutes(120).ToString("yyyy-MM-ddTHH:mm:ss"),
            sColor = "rgb(255, 200, 0)",
            bEditable = false,
            bAllDay = false
          });
        }

        // Liga
        for (int iSpTg = 0; iSpTg < MvcApplication.ckcore.ltDtSpieltagLiga[club.iLand][club.iDivision].Count; iSpTg++) {
          if (MvcApplication.ckcore.ltErgebnisseLiga[club.iLand][club.iDivision].Count <= iSpTg) break;

          DateTime dtTmp = MvcApplication.ckcore.ltDtSpieltagLiga[club.iLand][club.iDivision][iSpTg];
          if (dt.Date.Equals(dtTmp.Date)) {
            string sErg = "";
            string sH = "";
            string sA = "";
            for (int iB = 0; iB < MvcApplication.ckcore.ltErgebnisseLiga[club.iLand][club.iDivision][iSpTg].Count; iB++) {
              if (MvcApplication.ckcore.ltErgebnisseLiga[club.iLand][club.iDivision][iSpTg][iB][0] == club.iId) {
                sH = club.sName;
                sA = MvcApplication.ckcore.ltClubs[MvcApplication.ckcore.ltErgebnisseLiga[club.iLand][club.iDivision][iSpTg][iB][1]].sName;
                sErg = " ," + MvcApplication.ckcore.ltErgebnisseLiga[club.iLand][club.iDivision][iSpTg][iB][2] + ":" + MvcApplication.ckcore.ltErgebnisseLiga[club.iLand][club.iDivision][iSpTg][iB][3];
                break;
              } else if (MvcApplication.ckcore.ltErgebnisseLiga[club.iLand][club.iDivision][iSpTg][iB][1] == club.iId) {
                sH = MvcApplication.ckcore.ltClubs[MvcApplication.ckcore.ltErgebnisseLiga[club.iLand][club.iDivision][iSpTg][iB][0]].sName;
                sA = club.sName;
                if (MvcApplication.ckcore.ltErgebnisseLiga[club.iLand][club.iDivision][iSpTg][iB][2] >= 0 && MvcApplication.ckcore.ltErgebnisseLiga[club.iLand][club.iDivision][iSpTg][iB][3] >= 0) {
                  sErg = " ," + MvcApplication.ckcore.ltErgebnisseLiga[club.iLand][club.iDivision][iSpTg][iB][2] + ":" + MvcApplication.ckcore.ltErgebnisseLiga[club.iLand][club.iDivision][iSpTg][iB][3];
                }
                break;
              }
            }

            ltEvents.Add(new DiaryEvent {
              iID = ltEvents.Count,
              sTitle = " Liga, " + (iSpTg + 1).ToString().PadLeft(2) + ". Spieltag: " + sH + " vs. " + sA + sErg,
              sStartDate = dtTmp.ToString("yyyy-MM-ddTHH:mm:ss"),
              sEndDate = dtTmp.AddMinutes(105).ToString("yyyy-MM-ddTHH:mm:ss"),
              sColor = "rgb(0, 175, 100)",
              bEditable = false,
              bAllDay = false
            });

            break;
          }
        }

        // Cup
        foreach (CornerkickCore.Core.Cup cup in MvcApplication.ckcore.ltCups) {
          int iSpTg = 0;
          foreach (CornerkickCore.Core.Matchday md in cup.ltMatchdays) {
            if (dt.Date.Equals(md.dt.Date)) {
              if (md.ltGameData == null) continue;

              foreach (CornerkickGame.Game.Data gd in md.ltGameData) {
                if (gd.team[0].iTeamId == club.iId ||
                    gd.team[1].iTeamId == club.iId) {
                  string sH = "";
                  string sA = "";
                  for (int iB = 0; iB < md.ltGameData.Count; iB++) {
                    if (md.ltGameData[iB].team[0].iTeamId == club.iId) {
                      sH = club.sName;
                      sA = MvcApplication.ckcore.ltClubs[md.ltGameData[iB].team[1].iTeamId].sName;
                      break;
                    } else if (md.ltGameData[iB].team[1].iTeamId == club.iId) {
                      sH = MvcApplication.ckcore.ltClubs[md.ltGameData[iB].team[0].iTeamId].sName;
                      sA = club.sName;
                    }
                  }

                  string sTitle = " " + cup.sName;
                  string sColor = "rgb(200, 0, 200)";
                  if (cup.iId == 2) {
                    string sPokalrunde = "";
                    sPokalrunde = MvcApplication.ckcore.sPokalrunde[MvcApplication.ckcore.tl.getPokalRundeVonNTeiln(md.ltGameData.Count) + 1];
                    sTitle += ", " + sPokalrunde;

                    sColor = "rgb(100, 100, 255)";
                  } else if (cup.iId == -5) {
                    sColor = "rgb(255, 200, 255)";
                  }
                  sTitle += ": " + sH + " vs. " + sA;

                  ltEvents.Add(new DiaryEvent {
                    iID = ltEvents.Count,
                    //sTitle = " " + cup.sName + ", " + (iSpTg + 1).ToString().PadLeft(2) + ". Spieltag: " + sH + " vs. " + sA,
                    sTitle = sTitle,
                    sStartDate = md.dt.ToString("yyyy-MM-ddTHH:mm:ss"),
                    sEndDate = md.dt.AddMinutes(105).ToString("yyyy-MM-ddTHH:mm:ss"),
                    sColor = sColor,
                    bEditable = false,
                    bAllDay = false
                  });

                  if (cup.iId == 2 && md.ltGameData.Count > 1) {
                    ltEvents.Add(new DiaryEvent {
                      iID = ltEvents.Count,
                      sTitle = " " + cup.sName + ", Auslosung " + MvcApplication.ckcore.sPokalrunde[MvcApplication.ckcore.tl.getPokalRundeVonNTeiln(md.ltGameData.Count)],
                      sStartDate = new DateTime(md.dt.Year, md.dt.Month, md.dt.AddDays(1).Day, 12,  0, 0).ToString("yyyy-MM-ddTHH:mm:ss"),
                      sEndDate   = new DateTime(md.dt.Year, md.dt.Month, md.dt.AddDays(1).Day, 12, 30, 0).ToString("yyyy-MM-ddTHH:mm:ss"),
                      sColor     = "rgb(100, 200, 255)",
                      bEditable = false,
                      bAllDay = false
                    });
                  }
                  break;
                }
              }
            } else if (cup.iId == 2 && iSpTg == 0 && dt.Date.Equals(MvcApplication.ckcore.dtSaisonstart.AddDays(6).Date)) {
              ltEvents.Add(new DiaryEvent {
                iID = ltEvents.Count,
                sTitle = " " + cup.sName + ", Auslosung 1. Runde",
                sStartDate = new DateTime(dt.Year, dt.Month, dt.Day, 12,  0, 0).ToString("yyyy-MM-ddTHH:mm:ss"),
                sEndDate   = new DateTime(dt.Year, dt.Month, dt.Day, 12, 30, 0).ToString("yyyy-MM-ddTHH:mm:ss"),
                sColor = "rgb(100, 200, 255)",
                bEditable = false,
                bAllDay = false
              });
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