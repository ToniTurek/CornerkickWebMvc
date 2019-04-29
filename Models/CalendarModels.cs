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

    public CornerkickManager.csTrainingCamp.Camp camp { get; set; }
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

      CornerkickManager.Club club = MvcApplication.ckcore.ltClubs[iTeam];

      //DateTime dt = new DateTime(MvcApplication.ckcore.dtDatum.Year, MvcApplication.ckcore.dtDatum.Month, MvcApplication.ckcore.dtDatum.Day);
      DateTime dt = MvcApplication.ckcore.dtSeasonStart.Date;
      while (dt.CompareTo(MvcApplication.ckcore.dtSeasonEnd) < 0) {
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
        CornerkickManager.csTrainingCamp.Booking booking = MvcApplication.ckcore.tcp.getCurrentCamp(club, dt, true);
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

        // Cup
        foreach (CornerkickManager.Cup cup in MvcApplication.ckcore.ltCups) {
          int iSpTg = 0;
          foreach (CornerkickManager.Cup.Matchday md in cup.ltMatchdays) {
            if (dt.Date.Equals(md.dt.Date)) {
              if (md.ltGameData == null) continue;

              foreach (CornerkickGame.Game.Data gd in md.ltGameData) {
                int iIdH = gd.team[0].iTeamId;
                int iIdA = gd.team[1].iTeamId;
                if (iIdH == club.iId ||
                    iIdA == club.iId) {
                  string sH = "";
                  string sA = "";
                  if (iIdH >= 0 && iIdH < MvcApplication.ckcore.ltClubs.Count) sH = MvcApplication.ckcore.ltClubs[iIdH].sName;
                  if (iIdA >= 0 && iIdA < MvcApplication.ckcore.ltClubs.Count) sA = MvcApplication.ckcore.ltClubs[iIdA].sName;

                  string sRes = MvcApplication.ckcore.ui.getResultString(gd);

                  string sTitle = " " + cup.sName;
                  string sColor = "rgb(200, 0, 200)";
                  if (cup.iId == 1) {
                    sTitle = " Liga, " + (iSpTg + 1).ToString().PadLeft(2) + ". Spieltag";
                    sColor = "rgb(0, 175, 100)";
                  } else if (cup.iId == 2) {
                    string sPokalrunde = "";
                    sPokalrunde = MvcApplication.ckcore.sCupRound[cup.getKoRound(md.ltGameData.Count)];
                    sTitle += ", " + sPokalrunde;

                    sColor = "rgb(100, 100, 255)";
                  } else if (cup.iId == -5) {
                    sColor = "rgb(255, 200, 255)";
                  }
                  sTitle += ": " + sH + " vs. " + sA;
                  if (!string.IsNullOrEmpty(sRes)) sTitle += ", " + sRes;

                  ltEvents.Add(new DiaryEvent {
                    iID = ltEvents.Count,
                    sTitle = sTitle,
                    sStartDate = md.dt.ToString("yyyy-MM-ddTHH:mm:ss"),
                    sEndDate = md.dt.AddMinutes(105).ToString("yyyy-MM-ddTHH:mm:ss"),
                    sColor = sColor,
                    bEditable = false,
                    bAllDay = false
                  });

                  if (cup.iId == 2 && cup.iId2 == club.iLand && md.ltGameData.Count > 1) {
                    ltEvents.Add(new DiaryEvent {
                      iID = ltEvents.Count,
                      sTitle = " " + cup.sName + ", Auslosung " + MvcApplication.ckcore.sCupRound[cup.getKoRound(md.ltGameData.Count) - 1],
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
            } else if (cup.iId == 2 && cup.iId2 == club.iLand && iSpTg == 0 && dt.Date.Equals(MvcApplication.ckcore.dtSeasonStart.AddDays(6).Date)) {
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