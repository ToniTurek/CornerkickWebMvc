using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Services;
using System.Web.Services.Protocols;
using Microsoft.AspNet.Identity;
using Newtonsoft.Json;

namespace CornerkickWebMvc.Controllers
{
  public class MemberController : Controller
  {
    public readonly static string[] sCultureInfo = new string[82] {
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "en-GB",
      "",
      "",
      "",
      "fr-FR",
      "",
      "",
      "de-DE",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      ""
    };

    internal static bool[] bShowClub = new bool[MvcApplication.iNations.Length]; // Flag if club will be used if nation is possible

    public MemberController()
    {
#if _CONSOLE
      ConsoleNews();
#endif
    }

    public static CornerkickManager.User ckUserStatic(System.Security.Principal.IPrincipal User)
    {
      if (User == null) return null;

      string sUserId = User.Identity.GetUserId();
      foreach (CornerkickManager.User usr in MvcApplication.ckcore.ltUser) {
        if (usr.id.Equals(sUserId)) return usr;
      }

      return null;
    }

    public static CornerkickManager.Club ckClubStatic(System.Security.Principal.IPrincipal User)
    {
      CornerkickManager.User usr = ckUserStatic(User);
      if (usr == null) return null;

      // National team
      if (usr.nation != null) {
        for (byte iN = 0; iN < bShowClub.Length; iN++) {
          if (usr.nation.iLand == MvcApplication.iNations[iN]) {
            if (!bShowClub[iN]) return usr.nation;
            break;
          }
        }
      }

      // Club
      return usr.club;
    }

    public CornerkickManager.User ckUser()
    {
      if (User == null) return null;

      string sUserId = User.Identity.GetUserId();
      foreach (CornerkickManager.User usr in MvcApplication.ckcore.ltUser) {
        if (usr.id.Equals(sUserId)) return usr;
      }

      return null;
    }

    public CornerkickManager.Club ckClub()
    {
      if (AccountController.checkUserIsAdmin(User.Identity.Name)) {
        if (MvcApplication.clubAdmin != null) return MvcApplication.clubAdmin;
      }

      CornerkickManager.User usr = ckUser();
      if (usr == null) return null;

      // National team
      if (usr.nation != null) {
        for (byte iN = 0; iN < bShowClub.Length; iN++) {
          if (usr.nation.iLand == MvcApplication.iNations[iN]) {
            if (!bShowClub[iN]) return usr.nation;
            break;
          }
        }
      }

      // Club
      return usr.club;
    }

    public static CultureInfo getCiStatic(System.Security.Principal.IPrincipal User)
    {
      CornerkickManager.Club clb = ckClubStatic(User);
      if (clb != null) {
        int iLandUser = clb.iLand;
        if (iLandUser >= 0 && iLandUser < sCultureInfo.Length) return new CultureInfo(sCultureInfo[iLandUser]);
      }

      return CultureInfo.CurrentCulture;
    }
    public static CultureInfo getCiStatic(int iLand)
    {
      if (iLand >= 0 && iLand < sCultureInfo.Length) return new CultureInfo(sCultureInfo[iLand]);

      return CultureInfo.CurrentCulture;
    }

    public CultureInfo getCi()
    {
      CornerkickManager.Club clb = ckClub();
      if (clb != null) {
        int iLandUser = clb.iLand;
        if (iLandUser >= 0 && iLandUser < sCultureInfo.Length) return new CultureInfo(sCultureInfo[iLandUser]);
      }

      return CultureInfo.CurrentCulture;
    }

    //
    // GET: /HelloWorld/
    [Authorize]
    public string Index()
    {
      return "This is my <b>default</b> action...";
    }

    //
    // GET: /HelloWorld/Welcome/
    [Authorize]
    public string Welcome(string name, int numTimes = 1)
    {
      return HttpUtility.HtmlEncode("Hello " + name + ", NumTimes is: " + numTimes);
    }

    public JsonResult SwitchClubNation()
    {
      CornerkickManager.User usr = ckUser();
      if (usr == null) return null;

      if (usr.nation != null) {
        for (byte iN = 0; iN < bShowClub.Length; iN++) {
          if (usr.nation.iLand == MvcApplication.iNations[iN]) {
            bShowClub[iN] = !bShowClub[iN];
            break;
          }
        }
      }

      return Json(bShowClub, JsonRequestBehavior.AllowGet);
    }

#if _CONSOLE
    //////////////////////////////////////////////////////////////////////////
    /// <summary>
    /// Console
    /// </summary>
    /// <returns></returns>
    //////////////////////////////////////////////////////////////////////////
    public ActionResult ConsoleNews()
    {
      if (AccountController.ckconsole == null) return View("Console", "");

      string s = MvcApplication.ckcore.sWochentag[(int)MvcApplication.ckcore.dtDatum.DayOfWeek] + ", " + MvcApplication.ckcore.dtDatum.ToShortDateString() + ", " + MvcApplication.ckcore.dtDatum.ToShortTimeString() + " Uhr\n\n";

      if (ModelState.IsValid) {
        ModelState.Clear();
      }

      for (int iN = 0; iN < AccountController.ckUser.ltNews.Count; iN++) {
        CornerkickManager.Main.News news = AccountController.ckUser.ltNews[iN];
        if (news.bUnread) {
          s += news.sNews + '\n';
          //news.bUnread = false;
          //AccountController.ckUser.ltNews[iN] = news;
        }
      }

      ViewData["sNews"] = s;

      //return RedirectToAction("Console", "Member");
      return View("Console", "");
    }

    [Authorize]
    public ActionResult Console()
    {
      if (AccountController.ckconsole == null) return View("Console", "");

      string s = "";

      if (ModelState.IsValid) {
        ModelState.Clear();
      }
      //foreach (string s1 in AccountController.ckconsole.ltPrint) s += s1 + '\n';

      ViewData["s"] = s;

      //return RedirectToAction("Console", "Member");
      return View("Console", "");
    }

    public ActionResult ConsoleInput(Models.ConsoleModels input)
    {
      ViewData["s"] = "";

      if (ModelState.IsValid) {
        //TODO: SubscribeUser(model.Email);
      }

      if (string.IsNullOrEmpty(input.sIn)) AccountController.ckconsole.resetMenu();

      if (passInputToCk(input.sIn)) AccountController.ckconsole.game(AccountController.ckUser);
      Console();

      return View("Console", "");
    }

    private bool passInputToCk(string sIn)
    {
      //AccountController.ckconsole.sInput = sIn;
      return true;
    }
#endif

    //////////////////////////////////////////////////////////////////////////
    /// <summary>
    /// Desk
    /// </summary>
    /// <param name="desk"></param>
    /// <returns></returns>
    //////////////////////////////////////////////////////////////////////////
    [Authorize]
    public ActionResult Desk(Models.DeskModel desk, Models.LeagueModels mlLeague)
    {
      if (ModelState.IsValid) {
        ModelState.Clear();
      }

      CornerkickManager.User usr = ckUser();
      if (usr        == null) return View(desk);
      if (usr.ltNews == null) return View(desk);
      desk.user = usr;

      if (usr.lti != null) {
        if (usr.lti.Count > 0) desk.iDeleteLog = usr.lti[0];
      }

      CornerkickManager.Club club = ckClub();
      if (club == null) return View(desk);
      desk.club = club;

      // Date next game
      desk.sDtNextGame = "";
      if (club.nextGame != null) {
        if (club.nextGame.dt.Date.Equals(MvcApplication.ckcore.dtDatum.Date)) {
          desk.sDtNextGame = "Heute, " + club.nextGame.dt.ToString("t", getCi()) + " Uhr";
        } else {
          desk.sDtNextGame = club.nextGame.dt.ToString("d", getCi()) + " (" + (club.nextGame.dt.Date - MvcApplication.ckcore.dtDatum.Date).TotalDays.ToString("0") + "d)";
        }

        // Weather
        desk.iWeather = club.nextGame.iWeather;
      } else {
        desk.sDtNextGame = "Saisonende: " + MvcApplication.ckcore.dtSeasonEnd.ToString("d", getCi()) + " (" + (MvcApplication.ckcore.dtSeasonEnd.Date - MvcApplication.ckcore.dtDatum.Date).TotalDays.ToString("0") + "d)";
      }

      // Get Table
      CornerkickManager.Cup league = MvcApplication.ckcore.tl.getCup(1, club.iLand, club.iDivision);

      desk.sTabellenplatz = "-";
      List<CornerkickManager.Tool.TableItem> ltTbl = CornerkickManager.Tool.getLeagueTable(league);
      int iPlatz = 1;
      int iSpl = 0;
      foreach (CornerkickManager.Tool.TableItem tbl in ltTbl) {
        if (tbl.iId == club.iId) {
          iSpl = tbl.iGUV[0] + tbl.iGUV[1] + tbl.iGUV[2];
          break;
        }
        iPlatz++;
      }
      if (iSpl > 0) desk.sTabellenplatz = iPlatz.ToString() + ". Platz nach " + iSpl.ToString() + " von " + ((ltTbl.Count - 1) * 2).ToString() + " Spielen";

      // Get Cup Round
      desk.sPokalrunde = "-";
      CornerkickManager.Cup cup = MvcApplication.ckcore.tl.getCup(2, club.iLand);
      if (cup != null) {
        if (cup.ltMatchdays != null) {
          if (cup.ltMatchdays.Count > 0) {
            if (cup.ltMatchdays[0].ltGameData != null) {
              int nPartFirstRound = cup.ltClubs[0].Count;

              if (nPartFirstRound > 0) {
                int nRound = cup.getKoRound(nPartFirstRound);
                int iMdClub = Math.Max(MvcApplication.ckcore.tl.getMatchdays(cup, club), 0);
                int iMdCurr = CornerkickManager.Tool.getMatchday(cup, MvcApplication.ckcore.dtDatum); // Current matchday

                string sCupRound = "";
                if (nRound - iMdClub - 1 >= 0) {
                  sCupRound = CornerkickManager.Main.sCupRound[nRound - iMdClub - 1];

                  if (iMdClub < iMdCurr) desk.sPokalrunde = "ausgeschieden (" + sCupRound + ")";
                  else                   desk.sPokalrunde = sCupRound;
                }
              }
            }
          }
        }
      }

      /*
      desk.sVDL = "";
      List<CornerkickGame.Game.GameSummary> ltGameSummary = MvcApplication.ckcore.tl.getNextGames(club, false, true);
      foreach (CornerkickGame.Game.GameSummary gs in ltGameSummary) {
        if        (gs.iGoalsH == gs.team[1].iGoals) {
          if      (gs.team[0].iTeamId == club.iId) desk.sVDL += "U, ";
          else if (gs.team[1].iTeamId == club.iId) desk.sVDL += "u, ";
        } else if (gs.iGoalsH >  gs.team[1].iGoals) {
          if      (gs.team[0].iTeamId == club.iId) desk.sVDL += "S, ";
          else if (gs.team[1].iTeamId == club.iId) desk.sVDL += "n, ";
        } else if (gs.iGoalsH <  gs.team[1].iGoals) {
          if      (gs.team[0].iTeamId == club.iId) desk.sVDL += "N, ";
          else if (gs.team[1].iTeamId == club.iId) desk.sVDL += "s, ";
        }
      }
      desk.sVDL = desk.sVDL.Trim();
      if (string.IsNullOrEmpty(desk.sVDL)) desk.sVDL = "-";
      */

      // Kondi/Frische/Moral
      float[] fTeamAve   = MvcApplication.ckcore.tl.getTeamAve(club);
      float[] fTeamAve11 = MvcApplication.ckcore.tl.getTeamAve(club, 11);
      desk.sKFM = "K: " + fTeamAve[0].ToString("0.0%") + ", F: " + fTeamAve[1].ToString("0.0%") + ", M: " + fTeamAve[2].ToString("0.0%");
      desk.sStrength = "Durchschnittsstärke (Startelf): " + fTeamAve[3].ToString("0.00") + fTeamAve11[3].ToString(" (0.00)");

      // Check if emblem exist
      desk.bEmblemExist = !getClubEmblem(club).StartsWith("<img src=\"/Content/Uploads/emblems/0.png");

      return View(desk);
    }

    public class DatatableNews
    {
      public int    iId { get; set; }
      public int    iType { get; set; }
      public string sDate { get; set; }
      public string sText { get; set; }
      public bool   bOld { get; set; }
    }

    public JsonResult DeskGetNews()
    {
      CornerkickManager.User usr = ckUser();
      if (usr        == null) return Json(null, JsonRequestBehavior.AllowGet);
      if (usr.ltNews == null) return Json(null, JsonRequestBehavior.AllowGet);

      int iDeleteLog = 0;
      if (usr.lti != null) {
        if (usr.lti.Count > 0) iDeleteLog = usr.lti[0];
      }

      CornerkickManager.Club clb = ckClub();

      List<DatatableNews> ltNews = new List<DatatableNews>();

      for (int iN = 0; iN < usr.ltNews.Count; iN++) {
        CornerkickManager.Main.News news = usr.ltNews[iN];

        if (news.iType < 99/* && news.bUnread*/) {
          if (news.bRead && news.bRead2) {
            if        (iDeleteLog == 1 && (MvcApplication.ckcore.dtDatum - news.dt).TotalDays >  7) {
              usr.ltNews.Remove(news);
              iN--;
              continue;
            } else if (iDeleteLog == 2 && (MvcApplication.ckcore.dtDatum - news.dt).TotalDays > 14) {
              usr.ltNews.Remove(news);
              iN--;
              continue;
            } else if (iDeleteLog == 3 && (MvcApplication.ckcore.dtDatum - news.dt).TotalDays > 30) {
              usr.ltNews.Remove(news);
              iN--;
              continue;
            }
          }

          string sN = news.sText;

          sN = sN.Replace("Anleitung",        "<a href=\"/Home/UserManual\">Anleitung</a>");
          sN = sN.Replace("Stadions",         "<a href=\"/Member/Stadion\">Stadions</a>");
          sN = sN.Replace("Stadionumgebung",  "<a href=\"/Member/StadiumSurroundings\">Stadionumgebung</a>");
          sN = sN.Replace("Jugendmannschaft", "<a href=\"/Member/Jouth\">Jugendmannschaft</a>");
          sN = sN.Replace("Jugendspieler",    "<a href=\"/Member/Jouth\">Jugendspieler</a>");
          sN = sN.Replace("Hauptsponsor",     "<a href=\"/Member/Sponsor\">Hauptsponsor</a>");
          sN = sN.Replace("Gold Cup",         "<a href=\"/Member/CupGold\">Gold Cup</a>");
          sN = sN.Replace("Silver Cup",       "<a href=\"/Member/CupSilver\">Silver Cup</a>");
          sN = sN.Replace("Testspiel",        "<a href=\"/Member/Calendar\">Testspiel</a>");
          sN = sN.Replace("Transferangebot",  "<a href=\"/Member/Transfer\">Transferangebot</a>");

          for (int iNat = 0; iNat < MvcApplication.iNations.Length; iNat++) {
            string sReplace = "Pokal " + CornerkickManager.Main.sLand[MvcApplication.iNations[iNat]];
            sN = sN.Replace(sReplace, "<a href=\"/Member/Cup\">" + sReplace + "</a>");
          }
          foreach (CornerkickGame.Player pl in clb.ltPlayer) {
            if (sN.Contains(pl.sName)) {
              sN = sN.Replace(pl.sName, "<a href=\"/Member/PlayerDetails?i=" + pl.iId.ToString() + "\" target = \"\">" + pl.sName + "</a>");
              break;
            }
          }

          foreach (CornerkickGame.Player pl in clb.ltPlayerJouth) {
            if (sN.Contains(pl.sName)) {
              sN = sN.Replace(pl.sName, "<a href=\"/Member/PlayerDetails?i=" + pl.iId.ToString() + "\" target = \"\">" + pl.sName + "</a>");
              break;
            }
          }

          DatatableNews dtn = new DatatableNews();
          dtn.iId = iN;
          dtn.sDate = news.dt.ToString("d", getCi()) + " " + news.dt.ToString("t", getCi());
          dtn.sText = sN;
          dtn.iType = news.iType;
          dtn.bOld = news.bRead;

          ltNews.Add(dtn);

          news.bRead  = true;
          news.bRead2 = true;
          usr.ltNews[iN] = news;
        }
      }

      return Json(new { aaData = ltNews }, JsonRequestBehavior.AllowGet);
    }

    public void SetDeleteLog(int iDeleteAfter)
    {
      CornerkickManager.User usr = ckUser();
      if (usr == null) return;

      if (usr.lti == null) usr.lti = new List<int>();
      if (usr.lti.Count < 1) usr.lti.Add(0);

      usr.lti[0] = iDeleteAfter;
    }

    public ContentResult GetLastGames()
    {
      CornerkickManager.Club club = ckClub();
      if (club == null) return Content("", "application/json");

      List<Models.DataPointGeneral>[] dataPoints = new List<Models.DataPointGeneral>[5];
      dataPoints[0] = new List<Models.DataPointGeneral>(); // League
      dataPoints[1] = new List<Models.DataPointGeneral>(); // Nat. cup
      dataPoints[2] = new List<Models.DataPointGeneral>(); // Gold/Silver Cup
      dataPoints[3] = new List<Models.DataPointGeneral>(); // Testgame
      dataPoints[4] = new List<Models.DataPointGeneral>(); // National Team

      List<CornerkickGame.Game.Data> ltGameData = MvcApplication.ckcore.tl.getNextGames(club, MvcApplication.ckcore.dtDatum, false, true);
      int iLg = 0;
      foreach (CornerkickGame.Game.Data gs in ltGameData) {
        if (gs.iGameType < 1 || gs.iGameType == 5) continue; // Testgame
        if (gs.team[0].iTeamId == 0 && gs.team[1].iTeamId == 0) continue;

        CornerkickManager.Club clubH = null;
        if (gs.team[0].iTeamId >= 0 && gs.team[0].iTeamId < MvcApplication.ckcore.ltClubs.Count) clubH = MvcApplication.ckcore.ltClubs[gs.team[0].iTeamId];
        CornerkickManager.Club clubA = null;
        if (gs.team[1].iTeamId >= 0 && gs.team[1].iTeamId < MvcApplication.ckcore.ltClubs.Count) clubA = MvcApplication.ckcore.ltClubs[gs.team[1].iTeamId];

        string sDesc = gs.dt.ToString("d", getCi()) + "</br>";
        if (clubH != null && clubA != null) sDesc += clubH.sName + " - " + clubA.sName + "</br>";
        sDesc += MvcApplication.ckcore.ui.getResultString(gs);

        int iGameType = 0;
        if      (gs.iGameType == 2) iGameType = 1;
        else if (gs.iGameType == 3 || gs.iGameType == 4) iGameType = 2;
        else if (gs.iGameType == 5) iGameType = 3;
        else if (gs.iGameType == 7) iGameType = 4;

        if (gs.team[0].iGoals == gs.team[1].iGoals) {
          dataPoints[iGameType].Add(new Models.DataPointGeneral(--iLg,  0, sDesc));
        } else if ((gs.team[0].iGoals > gs.team[1].iGoals && gs.team[0].iTeamId == club.iId) || 
                   (gs.team[0].iGoals < gs.team[1].iGoals && gs.team[1].iTeamId == club.iId)) {
          dataPoints[iGameType].Add(new Models.DataPointGeneral(--iLg, +1, sDesc));
        } else if ((gs.team[0].iGoals < gs.team[1].iGoals && gs.team[0].iTeamId == club.iId) ||
                   (gs.team[0].iGoals > gs.team[1].iGoals && gs.team[1].iTeamId == club.iId)) {
          dataPoints[iGameType].Add(new Models.DataPointGeneral(--iLg, -1, sDesc));
        }
      }

      JsonSerializerSettings _jsonSetting = new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore };

      return Content(JsonConvert.SerializeObject(dataPoints, _jsonSetting), "application/json");
    }

    [Authorize]
    public ActionResult PreviewGame()
    {
      Models.PreviewGameModel mdPreview = new Models.PreviewGameModel();

      CornerkickManager.Club clb = ckClub();
      if (clb == null) return View(mdPreview);

      List<CornerkickGame.Game.Data> ltGdNextGames = MvcApplication.ckcore.tl.getNextGames(clb, MvcApplication.ckcore.dtDatum);
      for (byte j = 0; j < ltGdNextGames.Count; j++) {
        CornerkickGame.Game.Data gd = ltGdNextGames[j];
        mdPreview.ddlGames.Add(new SelectListItem { Text = gd.dt.ToString("d", getCi()), Value = j.ToString() });
      }

      return View(mdPreview);
    }

    public JsonResult PreviewGameDrawGame(int iGame)
    {
      CornerkickManager.Club clb = ckClub();
      if (clb == null) return Json(null, JsonRequestBehavior.AllowGet);

      List<CornerkickGame.Game.Data> ltGdNextGames = MvcApplication.ckcore.tl.getNextGames(clb, MvcApplication.ckcore.dtDatum);
      if (iGame >= ltGdNextGames.Count) return Json(null, JsonRequestBehavior.AllowGet);

      CornerkickGame.Game.Data gdNext = ltGdNextGames[iGame];

      string sBox = "";

      CornerkickManager.Cup cup = MvcApplication.ckcore.tl.getCup(gdNext);
      if (cup != null) {
        sBox += "<div style=\"position: relative; width: 100%; text-align: center\">";
        sBox += "<text>" + cup.sName + " - ";
        int iMd = CornerkickManager.Tool.getMatchday(cup, MvcApplication.ckcore.dtDatum);
        if (cup.checkCupGroupPhase(iMd, 0)) sBox += (iMd + 1).ToString() + ". Spieltag";
        else                                sBox += CornerkickManager.Main.sCupRound[cup.getMatchdaysTotal() - iMd - 1];
        sBox += "</text>";
        sBox += "</div>";
      }

      sBox += "<div style=\"position: relative; width: 100%; text-align: center\">";
      sBox += "<text>Anstoß: " + gdNext.dt.ToString("d", getCi()) + ", " + gdNext.dt.ToString("t", getCi()) + " Uhr</text>";
      sBox += "</div>";

      sBox += "<div style=\"position: relative; width: 100%; height: 30px; font-size: 150% \">";
      sBox += "<div style=\"position: absolute; width: 47%; text-align: right\" > ";
      sBox += "<text>" + gdNext.team[0].sTeam + "</text>";
      sBox += "</div>";
      sBox += "<div style=\"position: absolute; width: 6%; left: 47%; text-align: center\">";
      sBox += "<text>vs.</text>";
      sBox += "</div>";
      sBox += "<div style=\"position: absolute; width: 47%; left: 53%; text-align: left\">";
      sBox += "<text>" + gdNext.team[1].sTeam + "</text>";
      sBox += "</div>";
      sBox += "</div>";
      if (gdNext.stadium != null && !string.IsNullOrEmpty(gdNext.stadium.sName) && gdNext.stadium.getSeats() > 0) {
        sBox += "<div style=\"position: relative; width: 100%; text-align: center\">";
        sBox += "<text>" + gdNext.stadium.sName + " (" + gdNext.stadium.getSeats().ToString("N0", getCi()) + ")</text>";
        sBox += "</div>";
      }

      // Referee box
      byte iRefereeBoxHeight = 80;
      if (iGame == 0) iRefereeBoxHeight = 160;
      sBox += "<div style=\"position: relative; width: 116px; height: " + iRefereeBoxHeight.ToString() + "px; float: right; margin-top: 10px; text-align: left; padding: 8px; border: 1px solid black; -webkit-border-radius: 10px; -moz-border-radius: 10px\">";
      sBox += "<u>Schiedsrichter:<br></u>";
      sBox += "<text>Qualität: " + gdNext.referee.fQuality.ToString("0.0%") + "<br></text>";
      sBox += "<text>Härte: "    + gdNext.referee.fStrict .ToString("0.0%") + "</text>";
      if (iGame == 0) {
        sBox += "<input id=\"tbCorruptReferee\" class=\"form-control\" type=\"text\" value=\"0\" style=\"position: relative; width: 100px; text-align: right\">";
        sBox += "<button type=\"submit\" id=\"bnCorruptReferee\" class=\"btn btn-default\" style=\"position: relative; width: 100px; height: 52px; text-align: center\" onclick=\"corruptReferee()\">Schiri<br>bestechen</button>";
      }
      sBox += "</div>";

      return Json(sBox, JsonRequestBehavior.AllowGet);
    }

    public JsonResult PreviewGameCorruptReferee(int iMoney)
    {
      CornerkickManager.Club clb = ckClub();
      if (clb == null) return Json(null, JsonRequestBehavior.AllowGet);

      if (clb.iBalanceSecret < iMoney) return Json("Leider haben Sie nicht mehr genug Kleingeld in der schwarzen Kasse...", JsonRequestBehavior.AllowGet);

      CornerkickGame.Game.Data gdNext = MvcApplication.ckcore.tl.getNextGame(clb, MvcApplication.ckcore.dtDatum);

      for (byte iHA = 0; iHA < 2; iHA++) {
        if (clb.iId == gdNext.team[iHA].iTeamId) {
          gdNext.team[iHA].fRefereeCorrupt = Math.Min(iMoney / (float)500000, 0.2f); // Max 20% corruption with 100.000€
          clb.iBalanceSecret -= iMoney;
          break;
        }
      }

      return Json("Der Schiedsrichter steckt das Geld ein und zieht pfeifend von dannen...", JsonRequestBehavior.AllowGet);
    }

    public ContentResult GetTeamDevelopmentData(bool bExpected = false, int iTrainingsCamp = 0)
    {
      CornerkickManager.User usr = ckUser();
      if (usr == null) return Content("", "application/json");
      CornerkickManager.Club clb = ckClub();
      if (clb == null) return Content("", "application/json");

      CornerkickManager.Main.TrainingHistory trHistCurrent = new CornerkickManager.Main.TrainingHistory();
      trHistCurrent.dt = MvcApplication.ckcore.dtDatum;
      trHistCurrent.fKFM = MvcApplication.ckcore.tl.getTeamAve(clb);

      List<Models.DataPointGeneral>[][] dataPoints = new List<Models.DataPointGeneral>[2][];
      dataPoints[0] = new List<Models.DataPointGeneral>[3];
      dataPoints[1] = new List<Models.DataPointGeneral>[3];

      for (byte j = 0; j < dataPoints[0].Length; j++) {
        dataPoints[0][j] = new List<Models.DataPointGeneral>();

        for (int i = 0; i < clb.ltTrainingHist.Count; i++) {
          CornerkickManager.Main.TrainingHistory trHist = clb.ltTrainingHist[i];
          if (trHist.dt.CompareTo(MvcApplication.ckcore.dtDatum.AddDays(-7)) >  0 &&
              trHist.dt.CompareTo(MvcApplication.ckcore.dtDatum)             <= 0) {
            long iDate = convertDateTimeToTimestamp(trHist.dt);
            dataPoints[0][j].Add(new Models.DataPointGeneral(iDate, trHist.fKFM[j]));
          }
        }

        long iDateCurrent = convertDateTimeToTimestamp(trHistCurrent.dt);
        dataPoints[0][j].Add(new Models.DataPointGeneral(iDateCurrent, trHistCurrent.fKFM[j]));
      }

      if (bExpected) {
        // Initialize dataPoints list
        for (byte j = 0; j < dataPoints[1].Length; j++) dataPoints[1][j] = new List<Models.DataPointGeneral>();

        // Add current training history data to dataPoints
        for (byte j = 0; j < dataPoints[1].Length; j++) {
          dataPoints[1][j].Add(dataPoints[0][j][dataPoints[0][j].Count - 1]);
        }

        // Get training camp
        CornerkickManager.TrainingCamp.Booking camp = new CornerkickManager.TrainingCamp.Booking();
        foreach (CornerkickManager.TrainingCamp.Camp cp in MvcApplication.ckcore.tcp.ltCamps) {
          if (cp.iId == iTrainingsCamp) {
            camp.camp = cp;
            camp.dtArrival   = MvcApplication.ckcore.dtDatum.AddDays(-1);
            camp.dtDeparture = MvcApplication.ckcore.dtDatum.AddDays(+8);
            break;
          }
        }

        // Clone list of player in club
        List<CornerkickGame.Player> ltPlayerTrExp = new List<CornerkickGame.Player>();
        foreach (CornerkickGame.Player pl in clb.ltPlayer) ltPlayerTrExp.Add(pl.Clone());

        // Sort by training date
        List<CornerkickManager.Main.TrainingPlan.Unit> ltTrUnits = clb.training.ltUnit.OrderBy(tu => tu.dt).ToList();

        // Add training if none for the next 7 days
        for (int iD = 0; iD < 7; iD++) {
          List<CornerkickManager.Main.TrainingPlan.Unit> ltTrUnitsToday = CornerkickManager.Main.TrainingPlan.getTrainingUnitsToday(ltTrUnits, clb.ltTrainingHist, MvcApplication.ckcore.dtDatum.AddDays(iD));

          if (ltTrUnitsToday == null) {
            ltTrUnits.Add(new CornerkickManager.Main.TrainingPlan.Unit() { dt = MvcApplication.ckcore.dtDatum.Date.AddDays(iD).Add(tsTraining[0]), iType = 0 });
            ltTrUnits.Add(new CornerkickManager.Main.TrainingPlan.Unit() { dt = MvcApplication.ckcore.dtDatum.Date.AddDays(iD).Add(tsTraining[1]), iType = 0 });
            ltTrUnits.Add(new CornerkickManager.Main.TrainingPlan.Unit() { dt = MvcApplication.ckcore.dtDatum.Date.AddDays(iD).Add(tsTraining[2]), iType = 0 });
          }

          for (int iToD = 0; iToD < 3; iToD++) {
            bool bTuFound = false;
            for (int iTu = 0; iTu < ltTrUnitsToday.Count; iTu++) {
              if (ltTrUnitsToday[iTu].dt.TimeOfDay.Equals(tsTraining[iToD])) {
                bTuFound = true;
                break;
              }
            }
            if (bTuFound) continue;

            CornerkickManager.Main.TrainingPlan.Unit tuTmp = new CornerkickManager.Main.TrainingPlan.Unit() { dt = MvcApplication.ckcore.dtDatum.Date.AddDays(iD).Add(tsTraining[iToD]), iType = 0 };
            ltTrUnits.Add(tuTmp);
            ltTrUnitsToday.Add(tuTmp);
          }
        }

        // Get next saturday
        DateTime dtNextSaturday = MvcApplication.ckcore.dtDatum.Date;
        while ((int)dtNextSaturday.DayOfWeek != 6) dtNextSaturday = dtNextSaturday.AddDays(1);

        // Until next saturday ...
        foreach (CornerkickManager.Main.TrainingPlan.Unit tu in ltTrUnits) {
          if (tu.iType < 0) continue;
          if (tu.dt.CompareTo(MvcApplication.ckcore.dtDatum) < 0) continue; // If in past
          if (tu.dt.CompareTo(dtNextSaturday.Add(new TimeSpan(15, 30, 00))) > 0) break; // If too far in future

          //if      ((int)dtTmp.DayOfWeek == 0 && dtTmp.Hour > 10) break;
          //else if ((int)dtTmp.DayOfWeek == 1 && dtTmp.Hour < 10) break;

          // ... do training for each player
          for (int iP = 0; iP < ltPlayerTrExp.Count; iP++) {
            CornerkickGame.Player plTmp = ltPlayerTrExp[iP];
            CornerkickManager.Player.Training tr = CornerkickManager.Player.getTraining(tu.iType, MvcApplication.ckcore.plr.ltTraining);
            CornerkickManager.Player.doTraining(ref plTmp,
                                                tr,
                                                MvcApplication.ckcore.plr.ltTraining,
                                                clb.staff.iCondiTrainer,
                                                clb.staff.iPhysio,
                                                clb.buildings.bgGym.iLevel,
                                                clb.buildings.bgSpa.iLevel,
                                                tu.dt,
                                                usr,
                                                iTrainingPerDay: 1,
                                                ltPlayerTeam: ltPlayerTrExp,
                                                campBooking: camp,
                                                bJouth: false,
                                                bNoInjuries: true,
                                                ltTrRule: clb.training.ltRule);
          }

          // ... get training history data
          CornerkickManager.Main.TrainingHistory trHistExp = new CornerkickManager.Main.TrainingHistory();
          trHistExp.dt   = tu.dt;
          trHistExp.fKFM = MvcApplication.ckcore.tl.getTeamAve(ltPlayerTrExp, clb.ltTactic[0].formation);

          // ... add training history data to dataPoints
          for (byte j = 0; j < dataPoints[1].Length; j++) {
            long iDate = convertDateTimeToTimestamp(trHistExp.dt);
            dataPoints[1][j].Add(new Models.DataPointGeneral(iDate, trHistExp.fKFM[j]));
          }
        }
      }

      JsonSerializerSettings _jsonSetting = new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore };

      return Content(JsonConvert.SerializeObject(dataPoints, _jsonSetting), "application/json");
    }

    public ContentResult GetTeamFAve()
    {
      CornerkickManager.Club clb = ckClub();
      if (clb == null) return Content("", "application/json");

      Models.DataPointGeneral[] dataPoints = new Models.DataPointGeneral[7];

      double[] fFAve = new double[7];
      int nPlKeeper = 0;
      foreach (CornerkickGame.Player pl in clb.ltPlayer) {
        fFAve[0] += (pl.fSkillTraining[ 0] + pl.fSkillTraining[16])                                                / 2f; // Speed + acceleration
        fFAve[1] += (pl.fSkillTraining[ 1] + pl.fSkillTraining[ 2])                                                / 2f; // Technik + Dribbling
        fFAve[2] +=  pl.fSkillTraining[ 3];                                                                              // Zweikampf
        fFAve[3] += (pl.fSkillTraining[ 4] + pl.fSkillTraining[ 5] + pl.fSkillTraining[ 6] + pl.fSkillTraining[7]) / 4f; // Abspiel
        fFAve[4] += (pl.fSkillTraining[ 8] + pl.fSkillTraining[ 9] + pl.fSkillTraining[10])                        / 3f; // Abschluss
        fFAve[5] += (pl.fSkillTraining[11] + pl.fSkillTraining[12])                                                / 2f; // Standards
        fFAve[6] += (pl.fSkillTraining[13] + pl.fSkillTraining[14] + pl.fSkillTraining[15])                        / 3f; // TW

        if (pl.fExperiencePos[0] > 0.999) nPlKeeper++;
      }

      for (int iF = 0; iF < fFAve.Length; iF++) {
        if (iF < fFAve.Length - 1) dataPoints[iF] = new Models.DataPointGeneral(iF, fFAve[iF] / clb.ltPlayer.Count);
        else if (nPlKeeper > 0)    dataPoints[iF] = new Models.DataPointGeneral(iF, fFAve[iF] / nPlKeeper);
      }

      JsonSerializerSettings _jsonSetting = new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore };

      return Content(JsonConvert.SerializeObject(dataPoints, _jsonSetting), "application/json");

      /*
      return Json(JsonConvert.SerializeObject(dataPoints), JsonRequestBehavior.AllowGet);
      */
    }

    //////////////////////////////////////////////////////////////////////////
    /// <summary>
    /// Team
    /// </summary>
    /// <param name="team"></param>
    /// <returns></returns>
    //////////////////////////////////////////////////////////////////////////
    [Authorize]
    public ActionResult Team(Models.TeamModels team)
    {
      // Formationen
      team.ltsFormations    = new List<SelectListItem>();
      team.ltsFormationsOwn = new List<SelectListItem>();

      CornerkickManager.User user = ckUser();
      CornerkickManager.Club club = ckClub();
      team.club = club;
      if (club == null) return View(team);

      team.bAdmin = AccountController.checkUserIsAdmin(User.Identity.GetUserName());

      team.bGame = false;
      if (user != null && user.game != null) team.bGame = !user.game.data.bFinished;

      setModelLtPlayer(user);

      team.ltsFormations.Add(new SelectListItem { Text = "0 - Eigene", Value = "0" });
      for (int i = 0; i < MvcApplication.ckcore.ltFormationen.Count; i++) {
        team.ltsFormations.Add(new SelectListItem { Text     = (i + 1).ToString() + " - " + MvcApplication.ckcore.ltFormationen[i].sName,
                                                    Value    = (i + 1).ToString(),
                                                    Selected = (i + 1) == club.ltTactic[0].formation.iId
                                                  }
        );
      }

      if (user != null && user.ltFormations != null) {
        for (int i = 0; i < user.ltFormations.Count; i++) {
          // Reset id of user formations
          CornerkickGame.Tactic.Formation frmUser = user.ltFormations[i];
          frmUser.iId = MvcApplication.ckcore.ltFormationen.Count + i + 1;
          user.ltFormations[i] = frmUser;

          team.ltsFormations.Add(new SelectListItem { Text     = (MvcApplication.ckcore.ltFormationen.Count + i + 1).ToString() + " - " + frmUser.sName,
                                                      Value    = (MvcApplication.ckcore.ltFormationen.Count + i + 1).ToString(),
                                                      Selected = (MvcApplication.ckcore.ltFormationen.Count + i + 1) == club.ltTactic[0].formation.iId
                                                    }
          );
        }
      }

      return View(team);
    }

    private void setModelLtPlayer(CornerkickManager.User user)
    {
      Models.TeamModels.ltPlayer = new List<CornerkickGame.Player>();

      CornerkickManager.Club club = ckClub();
      if (club == null) return;

      foreach (CornerkickGame.Player pl in club.ltPlayer) {
        Models.TeamModels.ltPlayer.Add(pl);
      }

      if (user.game != null) {
        if (!user.game.data.bFinished) {
          Models.TeamModels.ltsSubstitution = new List<string[]>();

          byte iHA = 0;
          if (club.iId == user.game.data.team[1].iTeamId) iHA = 1;

          Models.TeamModels.ltPlayer = user.game.player[iHA].ToList();

          Models.TeamModels.iSubRest = user.game.iSubstitutionsLeft[iHA];

          if (Models.TeamModels.ltiSubstitution != null) {
            foreach (int[] iSub in Models.TeamModels.ltiSubstitution) {
              Models.TeamModels.ltsSubstitution.Add(new string[] { user.game.player[iHA][iSub[0]].sName, user.game.player[iHA][iSub[1]].sName, iSub[2].ToString() });
            }
          }
        }
      }
    }

    public void UpdateRow(int fromPosition, int toPosition)
    {
      if (fromPosition < 1 || toPosition < 1) return;

      CornerkickManager.Club clb = ckClub();

      CornerkickGame.Player pl = clb.ltPlayer[fromPosition - 1];
      if (!pl.bPlayed) {
        clb.ltPlayer.RemoveAt(fromPosition - 1);
        clb.ltPlayer.Insert(toPosition - 1, pl);

        setModelLtPlayer(ckUser());

        CkAufstellungFormation(clb.ltTactic[0].formation.iId);
      }
    }

    public JsonResult SwitchPlayerByIndex(int iIndex1, int iIndex2)
    {
      if (iIndex1 < 0 || iIndex2 < 0) return Json(false, JsonRequestBehavior.AllowGet);

      CornerkickManager.User user = ckUser();
      CornerkickManager.Club club = ckClub();
      if (club == null) return Json(false, JsonRequestBehavior.AllowGet);

      int jPosMin = Math.Min(iIndex1, iIndex2);
      int jPosMax = Math.Max(iIndex1, iIndex2);

      CornerkickGame.Player pl1 = club.ltPlayer[jPosMin];
      CornerkickGame.Player pl2 = club.ltPlayer[jPosMax];

      // Switch player in club list
      club.ltPlayer.Remove(pl1);
      club.ltPlayer.Remove(pl2);

      club.ltPlayer.Insert(jPosMin, pl2);
      club.ltPlayer.Insert(jPosMax, pl1);

      if (user.game != null) {
        if (!user.game.data.bFinished) {
          byte iHA = 0;
          if (club.iId == user.game.data.team[1].iTeamId) iHA = 1;

          // If switch of player in starting 11 --> do it directly
          if (jPosMin < user.game.data.nPlStart && jPosMax < user.game.data.nPlStart) {
            user.game.doSubstitution(iHA, (byte)jPosMin, (byte)jPosMax);
          } else {
            // Return if ...
            if (user.game.player[iHA][jPosMax].bPlayed) return Json(Models.TeamModels.ltPlayer, JsonRequestBehavior.AllowGet); // ... player in has already played
            if (user.game.iSubstitutionsLeft[iHA] == 0) return Json(Models.TeamModels.ltPlayer, JsonRequestBehavior.AllowGet); // ... no subs left
            if (!club.bNation && CornerkickManager.Player.atNationalTeam(user.game.player[iHA][jPosMax], MvcApplication.ckcore.ltClubs)) return Json(Models.TeamModels.ltPlayer, JsonRequestBehavior.AllowGet); // ... player in is at national team
            if (user.game.data.iGameType >                                                  0 &&
                user.game.data.iGameType <= user.game.player[iHA][jPosMin].iSuspension.Length &&
                user.game.player[iHA][jPosMin].iSuspension[user.game.data.iGameType - 1] > 0) return Json(Models.TeamModels.ltPlayer, JsonRequestBehavior.AllowGet); // ... player out is suspended

            if (Models.TeamModels.ltiSubstitution == null) {
              Models.TeamModels.ltiSubstitution = new List<int[]>();
            }
            Models.TeamModels.ltiSubstitution.Add(new int[] { jPosMin, jPosMax, 0 });
          }
        }
      }

      setModelLtPlayer(user);

      return Json(Models.TeamModels.ltPlayer, JsonRequestBehavior.AllowGet);
    }

    public JsonResult SwitchPlayerByID(int iID1, int iID2)
    {
      if (iID1 < 0) return null;
      if (iID2 < 0) return null;
      if (iID1 >= MvcApplication.ckcore.ltPlayer.Count) return null;
      if (iID2 >= MvcApplication.ckcore.ltPlayer.Count) return null;

      CornerkickManager.Club clb = ckClub();
      if (clb == null) return null;

      int iIndex1 = -1;
      int iIndex2 = -1;
      for (int iIx = 0; iIx < clb.ltPlayer.Count; iIx++) {
        if      (clb.ltPlayer[iIx].iId == iID1) iIndex1 = iIx;
        else if (clb.ltPlayer[iIx].iId == iID2) iIndex2 = iIx;

        if (iIndex1 >= 0 && iIndex2 >= 0) break;
      }

      return Json(SwitchPlayerByIndex(iIndex1, iIndex2), JsonRequestBehavior.AllowGet);
    }

    public JsonResult SwitchPlayer(CornerkickGame.Player pl1, CornerkickGame.Player pl2)
    {
      CornerkickManager.Club clb = ckClub();

      int iIndex1 = clb.ltPlayer.IndexOf(pl1);
      int iIndex2 = clb.ltPlayer.IndexOf(pl2);

      return Json(SwitchPlayerByIndex(iIndex1, iIndex2), JsonRequestBehavior.AllowGet);
    }

    public class DatatableEntryTeam
    {
      public int    iIndex { get; set; }
      public string sID { get; set; }
      public string sNr { get; set; }
      public string sNull { get; set; }
      public string sName { get; set; }
      public string sPosition { get; set; }
      public string sStaerke { get; set; }
      public string sKondi { get; set; }
      public string sFrische { get; set; }
      public string sMoral { get; set; }
      public string sStaerkeIdeal { get; set; }
      public string sErf { get; set; }
      public string sForm { get; set; }
      public string sAlter { get; set; }
      public string sTalent { get; set; }
      public bool   bSubstituted { get; set; }
      public string sLeader { get; set; }
      public int iMarktwert { get; set; }
      public int iGehalt { get; set; }
      public string sLz { get; set; }
      public string sNat { get; set; }
      public int iSuspended { get; set; }
      public string sCaptain { get; set; }
      public string sGrade { get; set; }
      public bool bAtNationalTeam { get; set; }
    }

    public ActionResult getTableTeam(int iPlayerMax)
    {
      /*
      List<CornerkickGame.Player> ltSpieler = new List<CornerkickGame.Player>();
      foreach (int iSp in AccountController.ckClub.ltPlayer) {
        ltSpieler.Add(MvcApplication.ckcore.ltPlayer[iSp]);
      }
      */
      var club = ckClub();
      if (club == null) return Json(false, JsonRequestBehavior.AllowGet);

      var user = ckUser();

      int iGameType = 0;
      if (club.nextGame != null) iGameType = club.nextGame.iGameType;

      List<CornerkickGame.Player> ltPlayerTeam = club.ltPlayer;

      // Update player numbers if nation
      if (club.bNation) {
        for (byte iP = 0; iP < Math.Min(ltPlayerTeam.Count, byte.MaxValue); iP++) ltPlayerTeam[iP].iNrNat = (byte)(iP + 1);
      }

      bool bGame = false;
      if (user.game != null) {
        bGame = !user.game.data.bFinished;
        if (bGame) {
          iGameType = user.game.data.iGameType;

          if (user.game.data.ltState.Count > 0) {
            byte iHA = 0;
            if (club.iId == user.game.data.team[1].iTeamId) iHA = 1;

            ltPlayerTeam = user.game.player[iHA].ToList();
          }
        }
      }

      List<string[]> ltLV = MvcApplication.ckcore.ui.listTeam(ltPlayerTeam, club, bGame, iGameType, iPlayerMax);

      //The table or entity I'm querying
      DatatableEntryTeam[] query = new DatatableEntryTeam[ltLV.Count];

      for (int i = 0; i < query.Length; i++) {
        int iSusp = 0;
        int.TryParse(ltLV[i][18], out iSusp);

        string sName = ltLV[i][2];
        int iId = -1;
        int.TryParse(ltLV[i][0], out iId);

        if (i == MvcApplication.ckcore.plr.getCaptainIx(club)) sName += " (C)";

        int iNat = int.Parse(ltLV[i][12]);
        string sNat = CornerkickManager.Main.sLandShort[iNat];

        int iVal = int.Parse(ltLV[i][ 9].Replace(".", ""));
        int iSal = int.Parse(ltLV[i][10].Replace(".", ""));

        //Hard coded data here that I want to replace with query results
        query[i] = new DatatableEntryTeam { iIndex = i + 1, sID = ltLV[i][0], sNr = ltLV[i][1], sNull = "", sName = sName, sPosition = ltLV[i][3], sStaerke = ltLV[i][4], sKondi = ltLV[i][5], sFrische = ltLV[i][6], sMoral = ltLV[i][7], sErf = ltLV[i][8], iMarktwert = iVal, iGehalt = iSal, sLz = ltLV[i][11], sNat = sNat, sForm = ltLV[i][13], sAlter = ltLV[i][14], sTalent = ltLV[i][15], bSubstituted = ltLV[i][16] == "ausg", sLeader = ltLV[i][19], sStaerkeIdeal = ltLV[i][17], iSuspended = iSusp, sCaptain = ltLV[i][20], sGrade = ltLV[i][21], bAtNationalTeam = ltLV[i][1].StartsWith("-") };
      }

      return Json(new { aaData = query }, JsonRequestBehavior.AllowGet);
    }

    [HttpPost]
    public ActionResult CkAufstellungKI(int iType = 0)
    {
      // Check if game running
      CornerkickManager.User user = ckUser();
      if (user.game != null) {
        if (!user.game.data.bFinished) return RedirectToAction("Team");
      }

      MvcApplication.ckcore.doFormation(MvcApplication.ckcore.ltClubs.IndexOf(user.club));

      return RedirectToAction("Team");
    }

    public JsonResult doFormationKI(int iType = 0)
    {
      // Check if game running
      CornerkickManager.User user = ckUser();
      if (user.game != null) {
        if (!user.game.data.bFinished) return Json("error", JsonRequestBehavior.AllowGet);
      }

      CornerkickManager.Club club = ckClub();

      MvcApplication.ckcore.doFormation(club.iId, iType);

      return Json("success", JsonRequestBehavior.AllowGet);
    }

    public JsonResult movePlayer(int iIndexPlayer, int iDirection, byte iTactic = 0)
    {
      CornerkickManager.Club club = ckClub();
      if (club == null) return Json(false, JsonRequestBehavior.AllowGet);

      if (iIndexPlayer < 0 || iIndexPlayer >= club.ltTactic[0].formation.ptPos.Length) return Json("error", JsonRequestBehavior.AllowGet);

      CornerkickGame.Tactic.Formation frmClub = club.ltTactic[iTactic].formation;
      if (iDirection == 1) {
        if (frmClub.ptPos[iIndexPlayer].X < (MvcApplication.ckcore.game.ptPitch.X / 2) - 1) {
          frmClub.ptPos[iIndexPlayer].X += 2;
        }
      } else if (iDirection == 2) {
        if (frmClub.ptPos[iIndexPlayer].Y < +MvcApplication.ckcore.game.ptPitch.Y) {
          frmClub.ptPos[iIndexPlayer].Y += 1;
          if (frmClub.ptPos[iIndexPlayer].Y % 2 == 0) frmClub.ptPos[iIndexPlayer].X += 1;
          else                                        frmClub.ptPos[iIndexPlayer].X -= 1;
        }
      } else if (iDirection == 3) {
        if (frmClub.ptPos[iIndexPlayer].X > 1) {
          frmClub.ptPos[iIndexPlayer].X -= 2;
        }
      } else if (iDirection == 4) {
        if (frmClub.ptPos[iIndexPlayer].Y > -MvcApplication.ckcore.game.ptPitch.Y) {
          frmClub.ptPos[iIndexPlayer].Y -= 1;
          if (frmClub.ptPos[iIndexPlayer].Y % 2 == 0) frmClub.ptPos[iIndexPlayer].X += 1;
          else                                        frmClub.ptPos[iIndexPlayer].X -= 1;
        }
      }

      frmClub.iId = 0;

      updatePlayerOfGame(ckUser().game, club);

      return Json("success", JsonRequestBehavior.AllowGet);
    }

    public JsonResult TeamMovePlayerToXY(int iIndexPlayer, int iX, int iY, byte iTactic = 0)
    {
      CornerkickManager.Club club = ckClub();
      if (club == null) return Json(false, JsonRequestBehavior.AllowGet);

      if (iIndexPlayer < 0 || iIndexPlayer >= club.ltTactic[0].formation.ptPos.Length) return Json("error", JsonRequestBehavior.AllowGet);

      if (iX <                                        0) return Json(null, JsonRequestBehavior.AllowGet);
      if (iX > MvcApplication.ckcore.game.ptPitch.X / 2) return Json(null, JsonRequestBehavior.AllowGet);
      if (iY < -MvcApplication.ckcore.game.ptPitch.Y)    return Json(null, JsonRequestBehavior.AllowGet);
      if (iY > +MvcApplication.ckcore.game.ptPitch.Y)    return Json(null, JsonRequestBehavior.AllowGet);

      CornerkickGame.Tactic.Formation frmClub = club.ltTactic[iTactic].formation;

      frmClub.ptPos[iIndexPlayer].X = iX;
      frmClub.ptPos[iIndexPlayer].Y = iY;

      frmClub.iId = 0;

      updatePlayerOfGame(ckUser().game, club);

      return Json("success", JsonRequestBehavior.AllowGet);
    }

    public JsonResult movePlayerRoa(int iIndexPlayer, int iDirection)
    {
      if (iIndexPlayer < 0) return Json("error", JsonRequestBehavior.AllowGet);

      CornerkickManager.Club club = ckClub();
      if (club == null) return Json(false, JsonRequestBehavior.AllowGet);

      CornerkickGame.Player pl = club.ltPlayer[iIndexPlayer];

      if      (iDirection == 1) pl.fRadOfAction[0] += 0.2f;
      else if (iDirection == 2) pl.fRadOfAction[0] -= 0.2f;
      else if (iDirection == 3) pl.fRadOfAction[1] += 0.2f;
      else if (iDirection == 4) pl.fRadOfAction[1] -= 0.2f;

      //if (iIndex >= Models.TeamModels.ltPlayer.Count) setModelLtPlayer(usr);
      Models.TeamModels.ltPlayer[iIndexPlayer] = pl;

      updatePlayerOfGame(ckUser().game, club);

      return Json("success", JsonRequestBehavior.AllowGet);
    }

    public JsonResult TeamSetIndOrientation(int iIndexPlayer, int iTactic, float fIndOrientation)
    {
      if (iIndexPlayer < 0) return Json("error", JsonRequestBehavior.AllowGet);

      CornerkickManager.Club club = ckClub();
      if (club == null) return Json(false, JsonRequestBehavior.AllowGet);

      club.ltTactic[iTactic].formation.fIndOrientation[iIndexPlayer] = fIndOrientation;

      updatePlayerOfGame(ckUser().game, club);

      return Json(fIndOrientation.ToString(), JsonRequestBehavior.AllowGet);
    }

    public JsonResult TeamSetManMarking(int iIxPlayer, sbyte iIxPlayerOpp)
    {
      if (iIxPlayer < 0) return Json("error", JsonRequestBehavior.AllowGet);

      CornerkickManager.Club club = ckClub();
      if (club == null) return Json(false, JsonRequestBehavior.AllowGet);

      CornerkickGame.Player pl = club.ltPlayer[iIxPlayer];

      if (pl.iIxManMarking == iIxPlayerOpp) pl.iIxManMarking = -1;
      else                                  pl.iIxManMarking = iIxPlayerOpp;

      Models.TeamModels.ltPlayer[iIxPlayer] = pl;

      updatePlayerOfGame(ckUser().game, club);

      return Json("success", JsonRequestBehavior.AllowGet);
    }

    [HttpPost]
    public ActionResult TeamSetSubstitutions()
    {
      // Check if game running
      CornerkickManager.User user = ckUser();
      if (user.game != null) {
        if (!user.game.data.bFinished) {
          if (Models.TeamModels.ltiSubstitution != null) {
            CornerkickManager.Club club = ckClub();
            byte iHA = 0;
            if (club.iId == user.game.data.team[1].iTeamId) iHA = 1;

            foreach (int[] iSub in Models.TeamModels.ltiSubstitution) {
              user.game.substitute(iHA == 0, (byte)iSub[0], (byte)iSub[1], 0);
            }
            Models.TeamModels.ltiSubstitution.Clear();
          }
        }
      }

      return RedirectToAction("Team");
    }

    [HttpPost]
    public ActionResult TeamUnsetSubstitutions()
    {
      // Check if game running
      CornerkickManager.User user = ckUser();
      if (user.game != null) {
        if (!user.game.data.bFinished) {
          CornerkickManager.Club club = ckClub();

          Models.TeamModels.ltiSubstitution = new List<int[]>();

          byte iHA = 0;
          if (club.iId == user.game.data.team[1].iTeamId) iHA = 1;

          // Create list of temp player
          List<CornerkickGame.Player> ltPlRes = new List<CornerkickGame.Player>();
          for (int i = user.game.data.nPlStart + user.game.data.nPlRes; i < club.ltPlayer.Count; i++) ltPlRes.Add(club.ltPlayer[i].Clone());

          // Clear current list of players
          club.ltPlayer.Clear();

          // Add player from current game
          foreach (CornerkickGame.Player pl in user.game.player[iHA]) club.ltPlayer.Add(pl);

          // Add temp player
          foreach (CornerkickGame.Player plRes in ltPlRes) club.ltPlayer.Add(plRes);

          return RedirectToAction("Team");
        }
      }

      return RedirectToAction("Team");
    }

    public JsonResult GetSubstitutionList()
    {
      List<string[]> ltsSubstitution = new List<string[]>();

      CornerkickManager.User user = ckUser();
      CornerkickManager.Club club = ckClub();
      if (club == null) return Json(false, JsonRequestBehavior.AllowGet);

      if (user.game != null) {
        if (!user.game.data.bFinished) {
          byte iHA = 0;
          if (club.iId == user.game.data.team[1].iTeamId) iHA = 1;

          Models.TeamModels.iSubRest = user.game.iSubstitutionsLeft[iHA];

          if (Models.TeamModels.ltiSubstitution != null) {
            foreach (int[] iSub in Models.TeamModels.ltiSubstitution) {
              ltsSubstitution.Add(new string[] { user.game.player[iHA][iSub[0]].sName, user.game.player[iHA][iSub[1]].sName, iSub[2].ToString() });
            }
          }
        }
      }

      return Json(ltsSubstitution, JsonRequestBehavior.AllowGet);
    }

    public JsonResult CkAufstellungFormation(int iF, int iSP = -1, bool bMobile = false, byte iTactic = 0)
    {
      CornerkickManager.User usr  = ckUser();
      CornerkickManager.Club club = ckClub();
      if (club == null) return Json(false, JsonRequestBehavior.AllowGet);

      if (iF >= 0) {
        if        (iF < MvcApplication.ckcore.ltFormationen.Count) {
          club.ltTactic[0].formation = MvcApplication.ckcore.ltFormationen[iF];
        } else if (iF < MvcApplication.ckcore.ltFormationen.Count + usr.ltFormations.Count) {
          club.ltTactic[0].formation = usr.ltFormations[iF - MvcApplication.ckcore.ltFormationen.Count];
        }

        updatePlayerOfGame(usr.game, club);
      }

      setModelLtPlayer(usr);

      Models.TeamModels.TeamData tD = new Models.TeamModels.TeamData();
      //return Json(Models.TeamModels.ltPlayer, JsonRequestBehavior.AllowGet);

      tD.ltPlayer = Models.TeamModels.ltPlayer;
      tD.formation = club.ltTactic[0].formation;
      tD.ltPlayerPos      = new List<byte>  ();
      tD.ltPlayerAveSkill = new List<string>();
      tD.ltPlayerNat      = new List<string>();
      tD.ltPlayerPortrait = new List<string>();
      tD.ltPlayerSusp     = new List<bool>  ();
      foreach (CornerkickGame.Player pl in tD.ltPlayer) {
        tD.ltPlayerNat.Add(CornerkickManager.Main.sLandShort[pl.iNat1]);
        tD.ltPlayerPortrait.Add(getPlayerPortrait(pl, sStyle: "height: 100%; width: 100%; object-fit: contain", bSmall: true));

        // Check if player is suspended
        bool bSusp = false;
        int iSuspIx = -1;
        if      (usr.game      != null && !usr.game.data.bFinished) iSuspIx = usr.game.data.iGameType - 1;
        else if (club.nextGame != null)                             iSuspIx = club.nextGame.iGameType - 1;

        if (iSuspIx >= 0 && iSuspIx < pl.iSuspension.Length) bSusp = pl.iSuspension[iSuspIx] > 0;

        tD.ltPlayerSusp.Add(bSusp);
      }

      int iP = 0;
      foreach (System.Drawing.Point ptPos in tD.formation.ptPos) {
        tD.ltPlayerPos     .Add(CornerkickGame.Tool.getBasisPos(CornerkickGame.Tool.getPosRole(ptPos, MvcApplication.ckcore.game.ptPitch)));
        tD.ltPlayerAveSkill.Add(CornerkickGame.Tool.getAveSkill(tD.ltPlayer[iP], CornerkickGame.Tool.getPosRole(ptPos, MvcApplication.ckcore.game.ptPitch)).ToString("0.0"));
        iP++;
      }

      tD.sDivPlayer = TeamGetPlayerOffence(tD.ltPlayer, club, bMobile);

      if (iSP >= 0) {
        tD.plSelected = tD.ltPlayer[iSP];
        tD.fIndOrientation = tD.formation.fIndOrientation[iSP];
        if (tD.fIndOrientation < -1f) tD.fIndOrientation = CornerkickGame.Tool.getPlayerIndividualOrientationDefault(tD.ltPlayerPos[iSP]);

        tD.sDivRoa               = TeamGetPlayerRadiusOfAction(iSP, club);
        tD.fIndOrientationMinMax = TeamGetIndOrientationMinMax(iSP, club);
      }

      tD.iCaptainIx = MvcApplication.ckcore.plr.getCaptainIx(club);

      // Team averages
      float[] fTeamAve11 = MvcApplication.ckcore.tl.getTeamAve(club, 11);
      tD.sTeamAveSkill = fTeamAve11[3].ToString("0.00");
      tD.sTeamAveAge   = fTeamAve11[4].ToString("0.0");

      tD.bNation = club.bNation;

      if (club.nextGame != null) {
        if (club.bNation) tD.iKibitzer = 3;
        else              tD.iKibitzer = club.staff.iKibitzer;

        int iClubOpp = club.nextGame.team[1].iTeamId;
        if (club.nextGame.team[1].iTeamId == club.iId) iClubOpp = club.nextGame.team[0].iTeamId;

        tD.ltPlayerOpp = new List<CornerkickGame.Player>();
        tD.ltPlayerOppPos = new List<byte>();
        tD.ltPlayerOppAveSkill = new List<string>();
        tD.ltPlayerOppPortrait = new List<string>();

        tD.bOppTeam = iClubOpp >= 0;

        if (tD.bOppTeam && tD.iKibitzer > 0) {
          CornerkickManager.Club clubOpp = MvcApplication.ckcore.ltClubs[iClubOpp];

          tD.formationOpp = clubOpp.ltTactic[0].formation;

          for (byte iPl = 0; iPl < club.nextGame.nPlStart; iPl++) {
            if (iPl >= clubOpp.ltPlayer.Count) break;

            CornerkickGame.Player plOpp = clubOpp.ltPlayer[iPl];

            tD.ltPlayerOpp.Add(plOpp);
            tD.ltPlayerOppPos.Add(CornerkickGame.Tool.getBasisPos(CornerkickGame.Tool.getPosRole(tD.formationOpp.ptPos[iPl], MvcApplication.ckcore.game.ptPitch)));

            float fPlOppAveSkill = CornerkickGame.Tool.getAveSkill(plOpp, 99);
            if (tD.iKibitzer == 3) fPlOppAveSkill = (float)Math.Round(fPlOppAveSkill * 2f) / 2f;
            tD.ltPlayerOppAveSkill.Add(fPlOppAveSkill.ToString("0.0"));
            tD.ltPlayerOppPortrait.Add(getPlayerPortrait(plOpp, sStyle: "height: 100%; width: 100%; object-fit: contain", bSmall: true));
          }

          // Opp. team averages
          float[] fTeamOppAve11 = MvcApplication.ckcore.tl.getTeamAve(clubOpp, club.nextGame.nPlStart);
          tD.sTeamOppAveSkill = fTeamOppAve11[3].ToString("0.00");
          tD.sTeamOppAveAge   = fTeamOppAve11[4].ToString("0.0");
        }
      }

      return Json(tD, JsonRequestBehavior.AllowGet);
    }

    // Set positions to player of current game
    private void updatePlayerOfGame(CornerkickGame.Game game, CornerkickManager.Club club)
    {
      if (game == null) return;
      if (game.data.bFinished) return;

      byte iHA = 0;
      if (game.data.team[1].iTeamId == club.iId) iHA = 1;

      // Update player
      for (byte iPl = 0; iPl < game.data.nPlStart; iPl++) {
        foreach (CornerkickGame.Player pl in club.ltPlayer) {
          if (pl.iId == game.player[iHA][iPl].iId) {
            game.player[iHA][iPl] = pl;
            break;
          }
        }
      }

      // Update formation
      game.data.team[iHA].ltTactic = club.ltTactic;
    }

    public JsonResult saveFormation(string sName, byte iTactic = 0)
    {
      if (string.IsNullOrEmpty(sName)) Json(-1, JsonRequestBehavior.AllowGet);

      CornerkickManager.User user = ckUser();
      CornerkickManager.Club club = ckClub();
      if (club == null) return Json(false, JsonRequestBehavior.AllowGet);

      club.ltTactic[iTactic].formation.iId = MvcApplication.ckcore.ltFormationen.Count + user.ltFormations.Count + 1;

      CornerkickGame.Tactic.Formation frmUser = CornerkickGame.Tactic.newFormation(11);
      frmUser.iId = club.ltTactic[iTactic].formation.iId;
      frmUser.sName = sName;
      for (int iPt = 0; iPt < club.ltTactic[iTactic].formation.ptPos.Length; iPt++) frmUser.ptPos[iPt] = club.ltTactic[iTactic].formation.ptPos[iPt];

      user.ltFormations.Add(frmUser);

      return Json(MvcApplication.ckcore.ltFormationen.Count + user.ltFormations.Count, JsonRequestBehavior.AllowGet);
    }

    public JsonResult deleteFormation(int iFormation)
    {
      CornerkickManager.User user = ckUser();

      if (iFormation >= MvcApplication.ckcore.ltFormationen.Count + 1 && iFormation < MvcApplication.ckcore.ltFormationen.Count + user.ltFormations.Count + 1) {
        CornerkickManager.Club club = ckClub();

        user.ltFormations.RemoveAt(iFormation - MvcApplication.ckcore.ltFormationen.Count - 1);

        CkAufstellungFormation(0);
      }

      return Json(1, JsonRequestBehavior.AllowGet);
    }

    public JsonResult GetPlayerStrength(int iPlayer)
    {
      CornerkickGame.Player player = MvcApplication.ckcore.ltPlayer[iPlayer];
      double fStrength = CornerkickGame.Tool.getAveSkill(player, 99);
      return Json(fStrength, JsonRequestBehavior.AllowGet);
    }

    private string TeamGetPlayerOffence(List<CornerkickGame.Player> ltPlayer, CornerkickManager.Club club, bool bMobile = false, byte iTactic = 0)
    {
      string sDiv = "";

      if (ltPlayer       == null) return "";
      if (ltPlayer.Count ==    0) return "";

      
      for (byte iPl = 0; iPl < MvcApplication.ckcore.game.data.nPlStart; iPl++) {
        if (iPl >= ltPlayer.Count) break;

        CornerkickGame.Player pl = ltPlayer[iPl];
        if (pl == null) continue;

        CornerkickGame.Tactic tc = club.ltTactic[iTactic];

        if (CornerkickGame.Tool.getPosRole(tc.formation.ptPos[iPl], MvcApplication.ckcore.game.ptPitch) == 1) continue; // If keeper --> continue

        float fHeight = 0.05f;
        if (bMobile) fHeight = 0.07f;

        float fWidth  = fHeight * (3f / 2f);

        int iXOffence = CornerkickGame.Tool.getXPosOffence(tc, iPl, MvcApplication.ckcore.game.ptPitch);
        float fTop  = 1f - (iXOffence / (float)MvcApplication.ckcore.game.ptPitch.X);
        float fLeft = (tc.formation.ptPos[iPl].Y + MvcApplication.ckcore.game.ptPitch.Y) / (float)(2 * MvcApplication.ckcore.game.ptPitch.Y);

        fTop  -= fHeight / 2f;
        fLeft -= fWidth  / 2f;

        // Set player no.
        string sPlNo = pl.iNr.ToString();
        if (club.bNation) sPlNo = (iPl + 1).ToString();

        sDiv += "<div onclick=\"javascript: selectPlayer(" + iPl.ToString() + ")\" style=\"position: absolute; ";
        sDiv += "top: "    + fTop   .ToString("0.00%", System.Globalization.CultureInfo.InvariantCulture) + "; ";
        sDiv += "left: "   + fLeft  .ToString("0.00%", System.Globalization.CultureInfo.InvariantCulture) + "; ";
        sDiv += "height: " + fHeight.ToString("0.00%", System.Globalization.CultureInfo.InvariantCulture) + "; ";
        sDiv += "width: "  + fWidth .ToString("0.00%", System.Globalization.CultureInfo.InvariantCulture) + "; ";
        sDiv += "border: 4px solid blue; ";
        sDiv += "background-color: white; ";
        sDiv += "-webkit-border-radius: 50%; ";
        sDiv += "-moz-border-radius: 50%; ";
        sDiv += "cursor: pointer; ";
        sDiv += "-webkit-box-shadow: 0px 0px 8px 8px rgba(0, 0, 0, .3); box-shadow: 0px 0px 8px 8px rgba(0, 0, 0, .3)";
        sDiv += "\">";

        int iFontSize = 230;
        if (bMobile) iFontSize = 125;
        sDiv += "<b style=\"position: absolute; width: 100%; text-align:center; font-size:" + iFontSize.ToString() + "%; color: black\">";
        sDiv += sPlNo;
        sDiv += "</b>";
        sDiv += "</div>";
      }

      return sDiv;
    }

    private string TeamGetPlayerRadiusOfAction(int iPlayerIndex, CornerkickManager.Club club, byte iTactic = 0)
    {
      string sDiv = "";

      if (iPlayerIndex < 0 || iPlayerIndex >= MvcApplication.ckcore.game.data.nPlStart || iPlayerIndex >= club.ltPlayer.Count) return "";

      CornerkickGame.Player pl = club.ltPlayer[iPlayerIndex];

      CornerkickGame.Tactic tc = club.ltTactic[iTactic];

      int iXOffence = CornerkickGame.Tool.getXPosOffence(tc, iPlayerIndex, MvcApplication.ckcore.game.ptPitch);

      for (double fRoa = 0.5; fRoa < 1.01; fRoa += 0.1) {
        System.Drawing.Point ptRoaTL = MvcApplication.ckcore.game.tl.getRndPos(tc.formation.ptPos[iPlayerIndex].X, pl, tc.formation, tc.fOrientation, +1, -1, fRoa, fRoa);
        System.Drawing.Point ptRoaBR = MvcApplication.ckcore.game.tl.getRndPos(tc.formation.ptPos[iPlayerIndex].X, pl, tc.formation, tc.fOrientation, -1, +1, fRoa, fRoa);

        float fTopRoa  = 1f - ((iXOffence + ptRoaTL.X) / (float)MvcApplication.ckcore.game.ptPitch.X);
        float fLeftRoa = (tc.formation.ptPos[iPlayerIndex].Y + ptRoaTL.Y + MvcApplication.ckcore.game.ptPitch.Y) / (float)(2 * MvcApplication.ckcore.game.ptPitch.Y);
        float fHeightRoa = Math.Abs(ptRoaBR.X - ptRoaTL.X) / (float)     MvcApplication.ckcore.game.ptPitch.X;
        float fWidthRoa  = Math.Abs(ptRoaBR.Y - ptRoaTL.Y) / (float)(2 * MvcApplication.ckcore.game.ptPitch.Y);

        sDiv += "<div ";
        sDiv += "style=\"position: absolute; ";
        sDiv += "top: "    + fTopRoa   .ToString("0.00%", System.Globalization.CultureInfo.InvariantCulture) + "; ";
        sDiv += "left: "   + fLeftRoa  .ToString("0.00%", System.Globalization.CultureInfo.InvariantCulture) + "; ";
        sDiv += "height: " + fHeightRoa.ToString("0.00%", System.Globalization.CultureInfo.InvariantCulture) + "; ";
        sDiv += "width: "  + fWidthRoa .ToString("0.00%", System.Globalization.CultureInfo.InvariantCulture) + "; ";
        sDiv += "border: 1px solid yellow; ";
        sDiv += "\">"; // end of style section

        sDiv += "<text style=\"color: yellow\">";
        sDiv += fRoa.ToString(" 0%");
        sDiv += "</text>";

        /*
        if (fRoa > 0.89 && fRoa < 0.91) {
          sDiv += "<div style=\"position: absolute; width: 40px; top: -22px; left: 50%; z-index: 10\"> " +
                    "<img id=\"img_arrow_1\" onclick =\"javascript:moveRoa(1)\" style =\"position: relative; width: 100%; cursor: pointer\" src=\"/Content/Images/arrow_up.png\"/>" +
                  "</div>";
          sDiv += "<div style=\"position: absolute; width: 40px; top: 0px; left: 50%; z-index: 10\"> " +
                    "<img id=\"img_arrow_1\" onclick =\"javascript:moveRoa(2)\" style =\"position: relative; width: 100%; cursor: pointer\" src=\"/Content/Images/arrow_down.png\"/>" +
                  "</div>";
          sDiv += "<div style=\"position: absolute; width: 16px; top: 50%; left: -20px; z-index: 10\"> " +
                    "<img id=\"img_arrow_1\" onclick =\"javascript:moveRoa(3)\" style =\"position: relative; width: 100%; cursor: pointer\" src=\"/Content/Images/arrow_left.png\"/>" +
                  "</div>";
          sDiv += "<div style=\"position: absolute; width: 16px; top: 50%; left: 2px; z-index: 10\"> " +
                    "<img id=\"img_arrow_1\" onclick =\"javascript:moveRoa(4)\" style =\"position: relative; width: 100%; cursor: pointer\" src=\"/Content/Images/arrow_right.png\"/>" +
                  "</div>";
        }
        */

        sDiv += "</div>";
      }

      float fTopButton  = 1f - (iXOffence / (float)MvcApplication.ckcore.game.ptPitch.X);
      float fLeftButton = (club.ltTactic[iTactic].formation.ptPos[iPlayerIndex].Y + MvcApplication.ckcore.game.ptPitch.Y) / (float)(2 * MvcApplication.ckcore.game.ptPitch.Y);
      sDiv += "<div style=\"position: absolute; width: 8%; min-width: 40px; top: "
                + (fTopButton  - 0.100f).ToString("0.00%", System.Globalization.CultureInfo.InvariantCulture)
                + "; left: " 
                + (fLeftButton - 0.030f).ToString("0.00%", System.Globalization.CultureInfo.InvariantCulture) 
                + "; z-index: 10\"> " +
                "<img id=\"img_arrow_1\" onclick =\"javascript:moveRoa(1)\" style =\"position: relative; width: 100%; cursor: pointer\" src=\"/Content/Images/arrow_up.png\"/>" +
              "</div>";
      sDiv += "<div style=\"position: absolute; width: 8%; min-width: 40px; top: "
                + (fTopButton  - 0.045f).ToString("0.00%", System.Globalization.CultureInfo.InvariantCulture)
                + "; left: "
                + (fLeftButton - 0.030f).ToString("0.00%", System.Globalization.CultureInfo.InvariantCulture)
                + "; z-index: 10\"> " +
                "<img id=\"img_arrow_1\" onclick =\"javascript:moveRoa(2)\" style =\"position: relative; width: 100%; cursor: pointer\" src=\"/Content/Images/arrow_down.png\"/>" +
              "</div>";
      sDiv += "<div style=\"position: absolute; width: 3%; min-width: 15px; top: "
                + (fTopButton  - 0.015f).ToString("0.00%", System.Globalization.CultureInfo.InvariantCulture)
                + "; left: "
                + (fLeftButton - 0.120f).ToString("0.00%", System.Globalization.CultureInfo.InvariantCulture)
                + "; z-index: 10\"> " +
                "<img id=\"img_arrow_1\" onclick =\"javascript:moveRoa(3)\" style =\"position: relative; width: 100%; cursor: pointer\" src=\"/Content/Images/arrow_left.png\"/>" +
              "</div>";
      sDiv += "<div style=\"position: absolute; width: 3%; min-width: 15px; top: "
                + (fTopButton  - 0.015f).ToString("0.00%", System.Globalization.CultureInfo.InvariantCulture)
                + "; left: "
                + (fLeftButton - 0.070f).ToString("0.00%", System.Globalization.CultureInfo.InvariantCulture)
                + "; z-index: 10\"> " +
                "<img id=\"img_arrow_1\" onclick =\"javascript:moveRoa(4)\" style =\"position: relative; width: 100%; cursor: pointer\" src=\"/Content/Images/arrow_right.png\"/>" +
              "</div>";

      return sDiv;
    }

    private float[] TeamGetIndOrientationMinMax(int iPlayerIndex, CornerkickManager.Club club, byte iTactic = 0)
    {
      float[] fIndOrientationMinMax = new float[2];

      if (iPlayerIndex < 0 || iPlayerIndex >= MvcApplication.ckcore.game.data.nPlStart || iPlayerIndex >= club.ltPlayer.Count) return fIndOrientationMinMax;

      CornerkickGame.Player pl = club.ltPlayer[iPlayerIndex];

      CornerkickGame.Tactic tc = club.ltTactic[iTactic];
      fIndOrientationMinMax[0] = CornerkickGame.Tool.getXPosOffence(tc.formation.ptPos[iPlayerIndex].X, tc.fOrientation, -1f, MvcApplication.ckcore.game.ptPitch.X) / (float)MvcApplication.ckcore.game.ptPitch.X;
      fIndOrientationMinMax[1] = CornerkickGame.Tool.getXPosOffence(tc.formation.ptPos[iPlayerIndex].X, tc.fOrientation, +1f, MvcApplication.ckcore.game.ptPitch.X) / (float)MvcApplication.ckcore.game.ptPitch.X;

      return fIndOrientationMinMax;
    }

    //////////////////////////////////////////////////////////////////////////
    /// <summary>
    /// Contracts
    /// </summary>
    /// <param name="Contracts"></param>
    /// <returns></returns>
    //////////////////////////////////////////////////////////////////////////
    [Authorize]
    public ActionResult Contracts(Models.ContractsModel mdContracts)
    {
      return View(mdContracts);
    }

    public ActionResult ContractsGetTableTeam()
    {
      CornerkickManager.Club club = ckClub();
      if (club == null) return Json(false, JsonRequestBehavior.AllowGet);

      int iGameType = 0;
      if (club.nextGame != null) iGameType = club.nextGame.iGameType;

      //The table or entity I'm querying
      Models.ContractsModel.DatatableEntry[] query = new Models.ContractsModel.DatatableEntry[club.ltPlayer.Count + club.ltPlayerJouth.Count];

      int iIx = 0;
      foreach (List<CornerkickGame.Player> ltPlayerTeam in new List<CornerkickGame.Player>[] { club.ltPlayer, club.ltPlayerJouth }) {
        // Update player numbers if nation
        if (club.bNation) {
          for (byte iP = 0; iP < Math.Min(ltPlayerTeam.Count, byte.MaxValue); iP++) ltPlayerTeam[iP].iNrNat = (byte)(iP + 1);
        }

        List<string[]> ltLV = MvcApplication.ckcore.ui.listTeam(ltPlayerTeam, club, false, iGameType, nPlStart: 0);

        for (int i = 0; i < ltPlayerTeam.Count; i++) {
          string sName = ltLV[i][2];
          int iId = -1;
          int.TryParse(ltLV[i][0], out iId);

          int iNat = int.Parse(ltLV[i][12]);
          string sNat = CornerkickManager.Main.sLandShort[iNat];

          int iVal    = int.Parse(ltLV[i][ 9].Replace(".", ""));
          int iSal    = int.Parse(ltLV[i][10].Replace(".", ""));
          int iPlay   = int.Parse(ltLV[i][22].Replace(".", ""));
          int iGoal   = int.Parse(ltLV[i][23].Replace(".", ""));

          string sNb = ltLV[i][1];
          if (iIx > club.ltPlayer.Count) sNb = "-";

          //Hard coded data here that I want to replace with query results
          query[iIx++] = new Models.ContractsModel.DatatableEntry {
            sID = ltLV[i][0],
            sNb = sNb,
            sName = sName,
            sPosition = ltLV[i][3],
            sSkill = ltLV[i][4],
            iValue = iVal,
            iSalary = iSal,
            sLength = ltLV[i][11],
            sNat = sNat,
            sAge = ltLV[i][14],
            sTalent = ltLV[i][15],
            sSkillIdeal = ltLV[i][17],
            iBonusPlay = iPlay,
            iBonusGoal = iGoal,
            sFixTransferFee = ltLV[i][24],
            bJouth = iIx > club.ltPlayer.Count
          };
        }
      }

      return Json(new { aaData = query }, JsonRequestBehavior.AllowGet);
    }

    [Authorize]
    public ActionResult PlayerDetails(Models.PlayerModel plModel, int i, HttpPostedFileBase file = null)
    {
      if (file != null) {
        AccountController.uploadFileAsync(file, "Portraits", i);
      }

      CornerkickManager.User usr = ckUser();
      if (usr == null) return Json(false, JsonRequestBehavior.AllowGet);

      CornerkickManager.Club club = ckClub();
      if (club == null) return Json(false, JsonRequestBehavior.AllowGet);

      plModel.iPlayer = i;
      CornerkickGame.Player plDetails = MvcApplication.ckcore.ltPlayer[i];
      plModel.iPlayerIndTr = plDetails.iIndTraining;

      plModel.bOwnPlayer = CornerkickManager.Player.ownPlayer(club, plDetails);
      if (plDetails.iClubId >= 0 && plDetails.iClubId < MvcApplication.ckcore.ltClubs.Count) {
        plModel.bJouth = MvcApplication.ckcore.ltClubs[plDetails.iClubId].ltPlayerJouth.IndexOf(plDetails) >= 0;
        plModel.bJouthBelow16 = plDetails.getAge(MvcApplication.ckcore.dtDatum) < 16;
      }

      plModel.bNation = club.bNation;

      plModel.iContractYears = 1;

      plModel.sName = plDetails.sName;

      plModel.ltDdlNo = new List<SelectListItem>();
      plModel.iNo = plDetails.iNr;

      plModel.sPortrait = getPlayerPortrait(plDetails, "height: 100%; width: 100%; object-fit: contain");
      plModel.sEmblem = getClubEmblem(plDetails.iClubId, "height: 100%; width: 100%; object-fit: contain");

      plModel.sColorJersey = "rgb(" + club.cl[0].R.ToString() + "," + club.cl[0].G.ToString() + "," + club.cl[0].B.ToString() + ")";
      System.Drawing.Color clJerseyNo = getColorBW(club);
      plModel.sColorJerseyNo = "rgb(" + clJerseyNo.R.ToString() + "," + clJerseyNo.G.ToString() + "," + clJerseyNo.B.ToString() + ")"; 

      List<int> ltNoExist = new List<int>();
      foreach (CornerkickGame.Player pl in club.ltPlayer) {
        ltNoExist.Add(pl.iNr);
      }

      if (plDetails.iNr == 0) {
        plModel.ltDdlNo.Add(
          new SelectListItem {
            Text  = "0",
            Value = "0"
          }
        );
      }

      for (int j = 1; j < 41; j++) {
        if (ltNoExist.IndexOf(j) >= 0 && j != plDetails.iNr) continue;

        plModel.ltDdlNo.Add(
          new SelectListItem {
            Text  = j.ToString(),
            Value = j.ToString()
          }
        );
      }

      // Captain
      plModel.bCaptain  = i == club.iCaptainId[0];
      plModel.bCaptain2 = i == club.iCaptainId[1];

      // Doping
      plModel.ddlDoping = new List<SelectListItem>();
      byte iDp = 0;
      foreach (CornerkickGame.Player.Doping dp in MvcApplication.ckcore.ltDoping) {
        plModel.ddlDoping.Add(
          new SelectListItem {
            Text  = dp.sName,
            Value = iDp++.ToString()
          }
        );
      }

      // Injury
      if (plDetails.injury != null) {
        Random rnd = new Random();
        if (plDetails.injury.iType >= CornerkickManager.Main.ltInjury.Length) plDetails.injury.iType = (byte)(CornerkickManager.Main.ltInjury.Length - 1);
        if (plDetails.injury.iType2 < 0 || plDetails.injury.iType2 >= CornerkickManager.Main.ltInjury[plDetails.injury.iType].Count) plDetails.injury.iType2 = (sbyte)rnd.Next(CornerkickManager.Main.ltInjury[plDetails.injury.iType].Count);
      }

      // Next / Prev. Player
      plModel.iPlIdPrev = -1;
      plModel.iPlIdNext = -1;

      int iIndex = club.ltPlayer.IndexOf(plDetails);

      if (iIndex >= 0) {
        if (iIndex >                       0) plModel.iPlIdPrev = club.ltPlayer[iIndex - 1].iId;
        if (iIndex < club.ltPlayer.Count - 1) plModel.iPlIdNext = club.ltPlayer[iIndex + 1].iId;
      }

      // Player is editable
      plModel.bEditable = (MvcApplication.ckcore.dtDatum - usr.dtClubStart).TotalHours < 24;

      return View(plModel);
    }

    internal static System.Drawing.Color getColorBW(CornerkickManager.Club club)
    {
      return getColorBW(club.cl[0]);
    }
    internal static System.Drawing.Color getColorBW(System.Drawing.Color cl)
    {
      System.Drawing.Color clBW = System.Drawing.Color.Black;
      if (checkColorBW(cl)) clBW = System.Drawing.Color.White;

      return clBW;
    }
    internal static bool checkColorBW(System.Drawing.Color cl)
    {
      return cl.R + cl.G + cl.B < 300;
    }

    private static string getPlayerPortrait(CornerkickGame.Player plPortrait, string sStyle = "", bool bSmall = false)
    {
      string sPortrait = "<img src=\"/Content/Images/Portraits/";

      if (!string.IsNullOrEmpty(sStyle)) sStyle = " style=\"" + sStyle + "\"";

      if (plPortrait == null) return sPortrait + "0.png\" alt=\"Portrait\" " + sStyle + " title=\"ohne\"/>";

      bool bUserPortrait;
      bool bSmallOk;
      string sPortraitFile = getPlayerPortraitFile(plPortrait, out bUserPortrait, out bSmallOk, bSmall);

      if (System.IO.File.Exists(sPortraitFile)) {
        if (bUserPortrait) {
          sPortrait = "<img src=\"/Content/Uploads/Portraits/" + plPortrait.iId.ToString();
        } else {
          sPortrait += getPlayerPortraitId(plPortrait).ToString();
        }

        if (bSmall && bSmallOk) sPortrait += "s";
      } else {
        sPortrait += "0";
      }

      sPortrait += ".png\" alt=\"Wappen\"" + sStyle + " title=\"" + plPortrait.sName + "\"/>";

      return sPortrait;
    }
    private static string getPlayerPortraitFile(CornerkickGame.Player plPortrait, out bool bUserPortrait, out bool bSmallOk, bool bSmall = false)
    {
#if DEBUG
      string sBaseDir = MvcApplication.getHomeDir();
#else
      string sBaseDir = System.Web.HttpContext.Current.Server.MapPath("~");
#endif
      string sPortraitDir = System.IO.Path.Combine(sBaseDir, "Content", "Uploads", "Portraits");
      string sPortraitFile = "";

      bUserPortrait = true;
      bSmallOk = false;

      // First: try user uploaded portraits
      for (byte i = 0; i < 2; i++) { // If bSmall --> try both
        sPortraitFile = System.IO.Path.Combine(sPortraitDir, plPortrait.iId.ToString());

        // Create small image
        if (bSmall) resizeImage(sPortraitFile + ".png", 100, "s");

        if (bSmall && i == 0) sPortraitFile += "s";
        sPortraitFile += ".png";

        if (System.IO.File.Exists(sPortraitFile)) {
          if (bSmall && i == 0) bSmallOk = true;
          return sPortraitFile;
        }
      }

      // Second: try default portraits
      for (byte i = 0; i < 2; i++) { // If bSmall --> try both
        sPortraitFile = System.IO.Path.Combine(sBaseDir, "Content", "Images", "Portraits", getPlayerPortraitId(plPortrait).ToString());

        // Create small image
        if (bSmall) resizeImage(sPortraitFile + ".png", 100, "s");

        if (bSmall && i == 0) sPortraitFile += "s";
        sPortraitFile += ".png";

        bUserPortrait = false;

        if (System.IO.File.Exists(sPortraitFile)) {
          if (bSmall && i == 0) bSmallOk = true;
          return sPortraitFile;
        }
      }

      return null;
    }

    private static void resizeImage(string sImageFileDatum, int iNewImageWidth, string sNewImageAppendix)
    {
      if (string.IsNullOrEmpty(sImageFileDatum)) return;
      if (!System.IO.File.Exists(sImageFileDatum)) return;

      string sNewImageFile = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(sImageFileDatum), System.IO.Path.GetFileNameWithoutExtension(sImageFileDatum)) + sNewImageAppendix + System.IO.Path.GetExtension(sImageFileDatum);
      if (System.IO.File.Exists(sNewImageFile)) return; // Return if file already exist

      Image imgEmblem = Image.FromFile(sImageFileDatum);
      int iHeight = (int)(imgEmblem.Height * iNewImageWidth / (double)imgEmblem.Width);
      Size szEmblemTiny = new Size(iNewImageWidth, iHeight);
      Image imgEmblemTiny = (Image)(new Bitmap(imgEmblem, szEmblemTiny));
      imgEmblemTiny.Save(sNewImageFile);
    }

    private static ushort getPlayerPortraitId(CornerkickGame.Player plPortrait)
    {
      byte[] byteArrPortrait = new byte[] { plPortrait.clSkin.R, plPortrait.clSkin.G };
      return BitConverter.ToUInt16(byteArrPortrait, 0);

      /*
      string sDirPortrait = System.IO.Path.Combine(MvcApplication.getHomeDir(), "Content", "Images", "Portraits");

      if (System.IO.Directory.Exists(sDirPortrait)) {
        System.IO.DirectoryInfo diPortrait = new System.IO.DirectoryInfo(sDirPortrait);

      }
      */
    }

    public ActionResult setPlayerIndTraining(int iPlayer, int iIndTr)
    {
      CornerkickGame.Player player = MvcApplication.ckcore.ltPlayer[iPlayer];
      player.iIndTraining = (byte)iIndTr;

      return Json(iIndTr, JsonRequestBehavior.AllowGet);
    }

    public ActionResult setPlayerName(int iPlayer, string sName)
    {
      if (string.IsNullOrEmpty(sName)) {
        Response.StatusCode = 1;
        return Json("Name darf nicht leer sein!", JsonRequestBehavior.AllowGet);
        //return Json(new { message = "Name darf nicht leer sein!" }, JsonRequestBehavior.AllowGet);
      }

      foreach (CornerkickGame.Player pl in MvcApplication.ckcore.ltPlayer) {
        if (string.IsNullOrEmpty(pl.sName)) continue;

        if (pl.sName.Equals(sName)) {
          Response.StatusCode = 1;
          return Json("Name bereits vorhanden!", JsonRequestBehavior.AllowGet);
          //return Json(new { message = "Name bereits vorhanden!" }, JsonRequestBehavior.AllowGet);
        }
      }

      CornerkickGame.Player player = MvcApplication.ckcore.ltPlayer[iPlayer];
      player.sName = sName;

      return Json(player.sName, JsonRequestBehavior.AllowGet);
    }

    public ActionResult setPlayerNo(int iPlayer, byte iNo)
    {
      CornerkickGame.Player player = MvcApplication.ckcore.ltPlayer[iPlayer];
      player.iNr = iNo;
      //MvcApplication.ckcore.ltPlayer[iPlayer] = player;

      return Json(player.iNr, JsonRequestBehavior.AllowGet);
    }

    public ActionResult PlayerMakeCaptain(int iPlayerId, byte iC)
    {
      if (iPlayerId < 0) return Json("Error", JsonRequestBehavior.AllowGet);

      CornerkickManager.Club club = ckClub();
      if (club == null) return Json(false, JsonRequestBehavior.AllowGet);

      if (club.iCaptainId == null) club.iCaptainId = new int[3];

      if (iC >= club.iCaptainId.Length) return Json("Error", JsonRequestBehavior.AllowGet);

      string sCaptain = "Kapitän";
      if (iC == 1) sCaptain = "Vize-Kapitän";

      MvcApplication.ckcore.plr.makePlayerCaptain(iPlayerId, iC, club);

      return Json("Sie haben " + MvcApplication.ckcore.ltPlayer[iPlayerId].sName + " zum " + sCaptain + " ernannt.", JsonRequestBehavior.AllowGet);
    }

    public ActionResult getClubCaptain(int iC)
    {
      CornerkickManager.Club club = ckClub();
      if (club == null) return Json(false, JsonRequestBehavior.AllowGet);

      if (club.iCaptainId[iC] >= 0) return Json(MvcApplication.ckcore.ltPlayer[club.iCaptainId[iC]].sName, JsonRequestBehavior.AllowGet);

      return Json("", JsonRequestBehavior.AllowGet);
    }

    public ActionResult PlayerDetailsDoDoping(int iPlayerId, byte iDp)
    {
      if (iPlayerId < 0) return Json("Error", JsonRequestBehavior.AllowGet);
      if (iDp >= MvcApplication.ckcore.ltDoping.Count) return Json("Error", JsonRequestBehavior.AllowGet);

      CornerkickGame.Player pl = MvcApplication.ckcore.ltPlayer[iPlayerId];

      return Json(MvcApplication.ckcore.plr.doDoping(pl, MvcApplication.ckcore.ltDoping[iDp]), JsonRequestBehavior.AllowGet);
    }

    public ActionResult PlayerDetailsGetDopingDesc(byte iDp)
    {
      if (iDp >= MvcApplication.ckcore.ltDoping.Count) return Json("", JsonRequestBehavior.AllowGet);

      CornerkickGame.Player.Doping dp = MvcApplication.ckcore.ltDoping[iDp];

      string sDesc = "Steigerung max. Kondition: " + dp.fEffectMax    .ToString("0.0%") + "<br>" +
                     " Einmaliger Frischegewinn: " + dp.fFreshGain    .ToString("0.0%") + "<br>" +
                     "       Reduktionsrate / d: " + dp.fReductionRate.ToString("0.0%") + "<br>" +
                     "    Max. Detektionsrisiko: " + dp.fDetectable   .ToString("0.0%") + "<br>" +
                     "                   Kosten: " + dp.iCost.ToString("N0", getCi()) + " €";

      return Json(sDesc, JsonRequestBehavior.AllowGet);
    }

    public ContentResult PlayerDetailsGetCFM(int iPlId)
    {
      CornerkickGame.Player pl = MvcApplication.ckcore.ltPlayer[iPlId];

      Models.DataPointGeneral[] dataPoints = new Models.DataPointGeneral[4];

      float fCondiMax = 1f;
      if (pl.doping != null) fCondiMax += pl.doping.fEffect;
      dataPoints[0] = new Models.DataPointGeneral(0, fCondiMax,     "");
      dataPoints[1] = new Models.DataPointGeneral(0, pl.fCondition, pl.fCondition.ToString("0.00%"));
      dataPoints[2] = new Models.DataPointGeneral(1, pl.fFresh,     pl.fFresh.ToString("0.00%"));
      dataPoints[3] = new Models.DataPointGeneral(2, pl.fMoral,     pl.fMoral.ToString("0.00%"));

      //JsonSerializerSettings _jsonSetting = new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore };

      return Content(JsonConvert.SerializeObject(dataPoints), "application/json");
    }

    public ContentResult PlayerDetailsGetDevelopmentData(int iPlId)
    {
      CornerkickGame.Player pl = MvcApplication.ckcore.ltPlayer[iPlId];

      List<Models.DataPointGeneral>[] ltDataPoints = new List<Models.DataPointGeneral>[2];
      ltDataPoints[0] = new List<Models.DataPointGeneral>(); // Skill
      ltDataPoints[1] = new List<Models.DataPointGeneral>(); // Value

      foreach (CornerkickGame.Player.History hty in pl.ltHistory) {
        long iDate = convertDateTimeToTimestamp(hty.dt);
        ltDataPoints[0].Add(new Models.DataPointGeneral(iDate, hty.fStrength));
        if (hty.iValue > 0) ltDataPoints[1].Add(new Models.DataPointGeneral(iDate, hty.iValue * 1000));
      }

      long iDateCurrent = convertDateTimeToTimestamp(MvcApplication.ckcore.dtDatum);
      ltDataPoints[0].Add(new Models.DataPointGeneral(iDateCurrent, CornerkickGame.Tool.getAveSkill(pl, bIdeal: true)));
      ltDataPoints[1].Add(new Models.DataPointGeneral(iDateCurrent, pl.getValue(MvcApplication.ckcore.dtDatum) * 1000));

      JsonSerializerSettings _jsonSetting = new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore };

      return Content(JsonConvert.SerializeObject(ltDataPoints, _jsonSetting), "application/json");
    }

    public ActionResult PlayerDetailsGetClubHistoryTable(int iPlayerId)
    {
      CornerkickGame.Player pl = MvcApplication.ckcore.ltPlayer[iPlayerId];

      //The table or entity I'm querying
      List<Models.PlayerModel.DatatableEntryClubHistory> ltDeClubHistory = new List<Models.PlayerModel.DatatableEntryClubHistory>();

      if (pl.ltClubHistory != null) {
        for (int iCh = 0; iCh < pl.ltClubHistory.Count; iCh++) {
          CornerkickGame.Player.ClubHistory ch = pl.ltClubHistory[iCh];

          // Remove corrupt entry
          if (ch.iClubId >= MvcApplication.ckcore.ltClubs.Count) {
            pl.ltClubHistory.RemoveAt(iCh);
            iCh--;
            continue;
          }

          // Get club name
          string sClubName = "vereinslos";
          if (ch.iClubId >= 0) {
            sClubName = MvcApplication.ckcore.ltClubs[ch.iClubId].sName;
            //<td align="center">@Html.ActionLink(MvcApplication.ckcore.ltClubs[ch.iClubId].sName, "ClubDetails", "Member", new { i = ch.iClubId }, new { target = "" })</td>
          }

          ltDeClubHistory.Add(new Models.PlayerModel.DatatableEntryClubHistory {
            iIx = iCh,
            sClubTakeName = sClubName,
            sDt = ch.dt.ToString("d", getCi()),
            iValue = pl.getValueHistory(ch.dt) * 1000,
            iTransferFee = ch.iTransferFee
          });
        }
      }

      return Json(new { aaData = ltDeClubHistory.ToArray() }, JsonRequestBehavior.AllowGet);
    }

    public JsonResult PlayerDetailsGetStatistic(int iPlayer, bool bSeason = true)
    {
      CornerkickGame.Player player = MvcApplication.ckcore.ltPlayer[iPlayer];

      const byte nStatLength = 4;

      // Create EP statistic
      CornerkickGame.Player.Statistic plStat3 = player.getStatistic(3, bSeason);
      CornerkickGame.Player.Statistic plStat4 = player.getStatistic(4, bSeason);
      CornerkickGame.Player.Statistic plStatEP = new CornerkickGame.Player.Statistic();
      for (int iS = 0; iS < plStat3.iStat.Length; iS++) {
        plStatEP.iStat[iS] = plStat3.iStat[iS] + plStat4.iStat[iS];
      }

      CornerkickGame.Player.Statistic[] plStat = new CornerkickGame.Player.Statistic[nStatLength] { player.getStatistic(1, bSeason), player.getStatistic(2, bSeason), plStatEP, player.getStatistic(7, bSeason) };

      int[] iGoalsTotal = new int[nStatLength] { player.getGoalsTotal(1, bSeason), player.getGoalsTotal(2, bSeason), player.getGoalsTotal(3, bSeason) + player.getGoalsTotal(4, bSeason), player.getGoalsTotal(7, bSeason) };

      string sBox = "";

      sBox += "<tr>";
      sBox += "<td align=\"right\">Spiele</td>";
      for (int i = 0; i < nStatLength; i++) {
        int[] iStat = plStat[i].iStat;
        sBox += "<td align=\"center\">" + iStat[0] + "</td>";
      }
      sBox += "</tr>";

      sBox += "<tr>";
      sBox += "<td align=\"right\">Minuten</td>";
      for (int i = 0; i < nStatLength; i++) {
        int[] iStat = plStat[i].iStat;
        sBox += "<td align=\"center\">" + iStat[28].ToString() + "</td>";
      }
      sBox += "</tr>";

      sBox += "<tr>";
      sBox += "<td align=\"right\">Min./Spiel</td>";
      for (int i = 0; i < nStatLength; i++) {
        int[] iStat = plStat[i].iStat;
        string sMinPerGame = "-";
        if (iStat[0] > 0) sMinPerGame = (iStat[28] / (float)iStat[0]).ToString("0");
        sBox += "<td align=\"center\">" + sMinPerGame + "</td>";
      }
      sBox += "</tr>";

      sBox += "<tr>";
      sBox += "<td align=\"right\">Note</td>";
      for (int i = 0; i < nStatLength; i++) {
        int[] iStat = plStat[i].iStat;
        string sGrade = "-";
        if (iStat[30] > 0) sGrade = ((iStat[29] * 0.1f) / iStat[30]).ToString("0.0");
        sBox += "<td align=\"center\">" + sGrade + "</td>";
      }
      sBox += "</tr>";

      sBox += "<tr>";
      sBox += "<td align=\"right\">Tore</td>";
      for (int i = 0; i < nStatLength; i++) {
        int[] iStat = plStat[i].iStat;
        sBox += "<td align=\"center\">" + iGoalsTotal[i].ToString() + "</td>";
      }
      sBox += "</tr>";

      sBox += "<tr>";
      sBox += "<td align=\"right\">mit rechts</td>";
      for (int i = 0; i < nStatLength; i++) {
        int[] iStat = plStat[i].iStat;
        sBox += "<td align=\"center\">" + iStat[1].ToString() + "</td>";
      }
      sBox += "</tr>";

      sBox += "<tr>";
      sBox += "<td align=\"right\">mit links</td>";
      for (int i = 0; i < nStatLength; i++) {
        int[] iStat = plStat[i].iStat;
        sBox += "<td align=\"center\">" + iStat[2].ToString() + "</td>";
      }
      sBox += "</tr>";

      sBox += "<tr>";
      sBox += "<td align=\"right\">per Kopf</td>";
      for (int i = 0; i < nStatLength; i++) {
        int[] iStat = plStat[i].iStat;
        sBox += "<td align=\"center\">" + iStat[3].ToString() + "</td>";
      }
      sBox += "</tr>";

      sBox += "<tr>";
      sBox += "<td align=\"right\">Tore pro Spiel</td>";
      for (int i = 0; i < nStatLength; i++) {
        int[] iStat = plStat[i].iStat;
        string s = "-";
        if (iStat[0] > 0) s = (iGoalsTotal[i] / (float)iStat[0]).ToString("0.00");
        sBox += "<td align=\"center\">" + s + "</td>";
      }
      sBox += "</tr>";

      sBox += "<tr>";
      sBox += "<td align=\"right\">11m +</td>";
      for (int i = 0; i < nStatLength; i++) {
        int[] iStat = plStat[i].iStat;
        sBox += "<td align=\"center\">" + iStat[4].ToString() + "</td>";
      }
      sBox += "</tr>";

      sBox += "<tr>";
      sBox += "<td align=\"right\">11m -</td>";
      for (int i = 0; i < nStatLength; i++) {
        int[] iStat = plStat[i].iStat;
        sBox += "<td align=\"center\">" + iStat[5].ToString() + "</td>";
      }
      sBox += "</tr>";

      sBox += "<tr>";
      sBox += "<td align=\"right\">11m</td>";
      for (int i = 0; i < nStatLength; i++) {
        int[] iStat = plStat[i].iStat;
        string s11m = "-";
        if (iStat[4] + iStat[5] > 0) s11m = (iStat[4] / (float)(iStat[4] + iStat[5])).ToString("0.0%");
        sBox += "<td align=\"center\">" + s11m + "</td>";
      }
      sBox += "</tr>";

      sBox += "<tr>";
      sBox += "<td align=\"right\">Freistoß +</td>";
      for (int i = 0; i < nStatLength; i++) {
        int[] iStat = plStat[i].iStat;
        sBox += "<td align=\"center\">" + iStat[6].ToString() + "</td>";
      }
      sBox += "</tr>";

      sBox += "<tr>";
      sBox += "<td align=\"right\">Freistoß -</td>";
      for (int i = 0; i < nStatLength; i++) {
        int[] iStat = plStat[i].iStat;
        sBox += "<td align=\"center\">" + iStat[7].ToString() + "</td>";
      }
      sBox += "</tr>";

      sBox += "<tr>";
      sBox += "<td align=\"right\">Freistoß</td>";
      for (int i = 0; i < nStatLength; i++) {
        int[] iStat = plStat[i].iStat;
        string s = "-";
        if (iStat[6] + iStat[7] > 0) s = (iStat[6] / (float)(iStat[6] + iStat[7])).ToString("0.0%");
        sBox += "<td align=\"center\">" + s + "</td>";
      }
      sBox += "</tr>";

      sBox += "<tr>";
      sBox += "<td align=\"right\">Torvorlagen</td>";
      for (int i = 0; i < nStatLength; i++) {
        int[] iStat = plStat[i].iStat;
        sBox += "<td align=\"center\">" + iStat[8].ToString() + "</td>";
      }
      sBox += "</tr>";

      sBox += "<tr>";
      sBox += "<td align=\"right\">Torschüsse</td>";
      for (int i = 0; i < nStatLength; i++) {
        int[] iStat = plStat[i].iStat;
        sBox += "<td align=\"center\">" + iStat[9].ToString() + "</td>";
      }
      sBox += "</tr>";

      sBox += "<tr>";
      sBox += "<td align=\"right\">Tors. pro Tor</td>";
      for (int i = 0; i < nStatLength; i++) {
        int[] iStat = plStat[i].iStat;
        string s = "-";
        if (iGoalsTotal[i] > 0) s = (iStat[9] / (float)iGoalsTotal[i]).ToString("0.00");
        sBox += "<td align=\"center\">" + s + "</td>";
      }
      sBox += "</tr>";

      sBox += "<tr>";
      sBox += "<td align=\"right\">Schüsse aufs Tor</td>";
      for (int i = 0; i < nStatLength; i++) {
        int[] iStat = plStat[i].iStat;
        sBox += "<td align=\"center\">" + iStat[10].ToString() + "</td>";
        }
      sBox += "</tr>";

      sBox += "<tr>";
      sBox += "<td align=\"right\">Torschussvorl.</td>";
      for (int i = 0; i < nStatLength; i++) {
        int[] iStat = plStat[i].iStat;
        sBox += "<td align=\"center\">" + iStat[27].ToString() + "</td>";
      }
      sBox += "</tr>";

      sBox += "<tr>";
      sBox += "<td align=\"right\">Abspiel +</td>";
      for (int i = 0; i < nStatLength; i++) {
        int[] iStat = plStat[i].iStat;
        sBox += "<td align=\"center\">" + iStat[15].ToString() + "</td>";
      }
      sBox += "</tr>";

      sBox += "<tr>";
      sBox += "<td align=\"right\">Abspiel -</td>";
      for (int i = 0; i < nStatLength; i++) {
        int[] iStat = plStat[i].iStat;
        sBox += "<td align=\"center\">" + iStat[16].ToString() + "</td>";
      }
      sBox += "</tr>";

      sBox += "<tr>";
      sBox += "<td align=\"right\">Abspiel</td>";
      for (int i = 0; i < nStatLength; i++) {
        int[] iStat = plStat[i].iStat;
        string s = "-";
        if (iStat[15] + iStat[16] > 0) s = (iStat[15] / (float)(iStat[15] + iStat[16])).ToString("0.0%");
        sBox += "<td align=\"center\">" + s + "</td>";
      }
      sBox += "</tr>";

      sBox += "<tr>";
      sBox += "<td align=\"right\">Pässe abgef.</td>";
      for (int i = 0; i < nStatLength; i++) {
        int[] iStat = plStat[i].iStat;
        sBox += "<td align=\"center\">" + iStat[26].ToString() + "</td>";
      }
      sBox += "</tr>";

      sBox += "<tr>";
      sBox += "<td align=\"right\">Zweikampf def. +</td>";
      for (int i = 0; i < nStatLength; i++) {
        int[] iStat = plStat[i].iStat;
        sBox += "<td align=\"center\">" + iStat[17].ToString() + "</td>";
      }
      sBox += "</tr>";

      sBox += "<tr>";
      sBox += "<td align=\"right\">Zweikampf def. -</td>";
      for (int i = 0; i < nStatLength; i++) {
        int[] iStat = plStat[i].iStat;
        sBox += "<td align=\"center\">" + iStat[18].ToString() + "</td>";
      }
      sBox += "</tr>";

      sBox += "<tr>";
      sBox += "<td align=\"right\">Zweikampf def.</td>";
      for (int i = 0; i < nStatLength; i++) {
        int[] iStat = plStat[i].iStat;
        string s = "-";
        if (iStat[17] + iStat[18] > 0) s = (iStat[17] / (float)(iStat[17] + iStat[18])).ToString("0.0%");
        sBox += "<td align=\"center\">" + s + "</td>";
      }
      sBox += "</tr>";

      sBox += "<tr>";
      sBox += "<td align=\"right\">Zweikampf off. +</td>";
      for (int i = 0; i < nStatLength; i++) {
        int[] iStat = plStat[i].iStat;
        sBox += "<td align=\"center\">" + iStat[19].ToString() + "</td>";
      }
      sBox += "</tr>";

      sBox += "<tr>";
      sBox += "<td align=\"right\">Zweikampf off. -</td>";
      for (int i = 0; i < nStatLength; i++) {
        int[] iStat = plStat[i].iStat;
        sBox += "<td align=\"center\">" + iStat[20].ToString() + "</td>";
      }
      sBox += "</tr>";

      sBox += "<tr>";
      sBox += "<td align=\"right\">Zweikampf off.</td>";
      for (int i = 0; i < nStatLength; i++) {
        int[] iStat = plStat[i].iStat;
        string s = "-";
        if (iStat[19] + iStat[20] > 0) s = (iStat[19] / (float)(iStat[19] + iStat[20])).ToString("0.0%");
        sBox += "<td align=\"center\">" + s + "</td>";
      }
      sBox += "</tr>";

      sBox += "<tr>";
      sBox += "<td align=\"right\">Fouls</td>";
      for (int i = 0; i < nStatLength; i++) {
        int[] iStat = plStat[i].iStat;
        sBox += "<td align=\"center\">" + iStat[21].ToString() + "</td>";
      }
      sBox += "</tr>";

      sBox += "<tr>";
      sBox += "<td align=\"right\">Gelbe Karten</td>";
      for (int i = 0; i < nStatLength; i++) {
        int[] iStat = plStat[i].iStat;
        sBox += "<td align=\"center\">" + iStat[22].ToString() + "</td>";
      }
      sBox += "</tr>";

      sBox += "<tr>";
      sBox += "<td align=\"right\">Gelb-Rote Karten</td>";
      for (int i = 0; i < nStatLength; i++) {
        int[] iStat = plStat[i].iStat;
        sBox += "<td align=\"center\">" + iStat[23].ToString() + "</td>";
      }
      sBox += "</tr>";

      sBox += "<tr>";
      sBox += "<td align=\"right\">Rote Karten</td>";
      for (int i = 0; i < nStatLength; i++) {
        int[] iStat = plStat[i].iStat;
        sBox += "<td align=\"center\">" + iStat[24].ToString() + "</td>";
      }
      sBox += "</tr>";

      return Json(sBox, JsonRequestBehavior.AllowGet);
    }

    public ContentResult PlayerDetailsGetChanceDevelopment(int iPlId)
    {
      CornerkickGame.Player pl = MvcApplication.ckcore.ltPlayer[iPlId];

      CornerkickManager.Club clb = null;
      if (pl.iClubId >= 0) clb = MvcApplication.ckcore.ltClubs[pl.iClubId];

      if (!CornerkickManager.Player.ownPlayer(clb, pl)) return Content(null, "application/json");

      CornerkickManager.User user = ckUser();
      if (user == null) return Content(null, "application/json");

      // Get diff. level
      float fLevel = 1f;
      if      (user.iLevel == 0) fLevel = 2.0f;
      else if (user.iLevel == 1) fLevel = 1.4f;
      else if (user.iLevel == 2) fLevel = 1.0f;
      else if (user.iLevel == 3) fLevel = 0.8f;

      byte iCoach = 1;
      byte iBuilding = 1;
      if (clb != null) {
        bool bJouth = CornerkickManager.Player.ownPlayer(clb, pl, 2);
        if (bJouth) {
          iCoach    = clb.staff.iJouthTrainer; // Jouth coach
          iBuilding = clb.buildings.bgJouthInternat.iLevel; // Jouth Internat
        } else {
          iCoach    = clb.staff.iCoTrainer; // Co-Coach
          iBuilding = clb.buildings.bgTrainingCourts.iLevel; // Training Court
        }
      }

      DateTime dt25 = MvcApplication.ckcore.dtDatum.AddYears(-25);
      double fChanceDev    =              CornerkickManager.Player.getChanceDevelopment(pl.dtBirthday, pl.iTalent, pl.iSkill[pl.iIndTraining], pl.fIndTraining[pl.iIndTraining], pl.fExperience, pl.iIndTraining, MvcApplication.ckcore.dtDatum, iCoach, iBuilding, fLevel);
      double[] fChanceDevFte = new double[8];
      fChanceDevFte[0] = fChanceDev - CornerkickManager.Player.getChanceDevelopment(pl.dtBirthday, pl.iTalent, pl.iSkill[pl.iIndTraining], pl.fIndTraining[pl.iIndTraining], pl.fExperience,               1, MvcApplication.ckcore.dtDatum, iCoach, iBuilding, fLevel);
      fChanceDevFte[1] = fChanceDev - CornerkickManager.Player.getChanceDevelopment(pl.dtBirthday, pl.iTalent,                         6f, pl.fIndTraining[pl.iIndTraining], pl.fExperience, pl.iIndTraining, MvcApplication.ckcore.dtDatum, iCoach, iBuilding, fLevel);
      fChanceDevFte[2] = fChanceDev - CornerkickManager.Player.getChanceDevelopment(pl.dtBirthday,          4, pl.iSkill[pl.iIndTraining], pl.fIndTraining[pl.iIndTraining], pl.fExperience, pl.iIndTraining, MvcApplication.ckcore.dtDatum, iCoach, iBuilding, fLevel);
      fChanceDevFte[3] = fChanceDev - CornerkickManager.Player.getChanceDevelopment(pl.dtBirthday, pl.iTalent, pl.iSkill[pl.iIndTraining], pl.fIndTraining[pl.iIndTraining], 1f,             pl.iIndTraining, MvcApplication.ckcore.dtDatum, iCoach, iBuilding, fLevel);
      fChanceDevFte[4] = fChanceDev - CornerkickManager.Player.getChanceDevelopment(         dt25, pl.iTalent, pl.iSkill[pl.iIndTraining], pl.fIndTraining[pl.iIndTraining], pl.fExperience, pl.iIndTraining, MvcApplication.ckcore.dtDatum, iCoach, iBuilding, fLevel);
      fChanceDevFte[5] = fChanceDev - CornerkickManager.Player.getChanceDevelopment(pl.dtBirthday, pl.iTalent, pl.iSkill[pl.iIndTraining], pl.fIndTraining[pl.iIndTraining], pl.fExperience, pl.iIndTraining, MvcApplication.ckcore.dtDatum,      0, iBuilding, fLevel);
      fChanceDevFte[6] = fChanceDev - CornerkickManager.Player.getChanceDevelopment(pl.dtBirthday, pl.iTalent, pl.iSkill[pl.iIndTraining], pl.fIndTraining[pl.iIndTraining], pl.fExperience, pl.iIndTraining, MvcApplication.ckcore.dtDatum, iCoach,         0, fLevel);
      fChanceDevFte[7] = fChanceDev - CornerkickManager.Player.getChanceDevelopment(pl.dtBirthday, pl.iTalent, pl.iSkill[pl.iIndTraining],                               0f, pl.fExperience, pl.iIndTraining, MvcApplication.ckcore.dtDatum, iCoach, iBuilding, fLevel);

      double fChanceDevTot = 0.0;
      double fChanceDevTotPls = 0.0;
      int nChanceDevTotPls = 0;
      for (int iFte = 0; iFte < fChanceDevFte.Length; iFte++) {
        fChanceDevTot += fChanceDevFte[iFte];
        if (fChanceDevFte[iFte] > 0.0) {
          fChanceDevTotPls += fChanceDevFte[iFte];
          nChanceDevTotPls++;
        }
      }
      for (int iFte = 0; iFte < fChanceDevFte.Length; iFte++) {
        if (fChanceDevTot > 0.0) {
          fChanceDevFte[iFte] *= fChanceDev / fChanceDevTot;
        } else {
          fChanceDevFte[iFte] *= fChanceDev / -fChanceDevTot;
          if (fChanceDevFte[iFte] > 0.0) fChanceDevFte[iFte] += (2 * fChanceDev) / nChanceDevTotPls;
        }
      }

      Models.DataPointGeneral[] dataPoints = new Models.DataPointGeneral[9];
      for (int iFte = 0; iFte < fChanceDevFte.Length; iFte++) dataPoints[iFte] = new Models.DataPointGeneral(iFte, fChanceDevFte[iFte], fChanceDevFte[iFte].ToString("0.00%"));
      dataPoints[fChanceDevFte.Length] = new Models.DataPointGeneral(fChanceDevFte.Length, fChanceDev, fChanceDev.ToString("0.00%"));

      return Content(JsonConvert.SerializeObject(dataPoints), "application/json");
    }

    [HttpPost]
    public JsonResult GetPlayerSalary(int iPlayerId, byte iYears, int iSalaryOffer = 0, int iBonusPlayOffer = 0, int iBonusGoalOffer = 0, int iFixedFee = 0, bool bNegotiate = true)
    {
      if (iPlayerId < 0) return Json("Invalid player", JsonRequestBehavior.AllowGet);

      CornerkickManager.Club club = ckClub();
      if (club == null) return Json(false, JsonRequestBehavior.AllowGet);

      CornerkickGame.Player plSalary = MvcApplication.ckcore.ltPlayer[iPlayerId];

      int iGamesPerSeason = MvcApplication.ckcore.tl.getMatchdays(MvcApplication.ckcore.tl.getCup(1, club.iLand, club.iDivision), club);

      CornerkickManager.Club clbPlayer = null;
      if (plSalary.iClubId >= 0) clbPlayer = MvcApplication.ckcore.ltClubs[plSalary.iClubId];

      bool bForceNewContract = false;
      if (clbPlayer != null) bForceNewContract = club.iId != clbPlayer.iId;

      CornerkickGame.Player.Contract contract = MvcApplication.ckcore.tl.negotiatePlayerContract(plSalary, club, iYears, iSalaryOffer, iBonusPlayOffer, iBonusGoalOffer, iGamesPerSeason, iFixedFee, bNegotiate: bNegotiate, bForceNewContract: bForceNewContract);

      return Json(contract, JsonRequestBehavior.AllowGet);
    }

    // iMode: 0 - Extention, 1 - new contract
    [HttpPost]
    public JsonResult NegotiatePlayerContract(int iId, int iYears, string sSalary, string sBonusPlay, string sBonusGoal, string sFixTransferFee, string sPlayerMood, int iMode)
    {
      // Initialize status code with ERROR
      Response.StatusCode = 1;

      if (iId    < 0) return Json("Error", JsonRequestBehavior.AllowGet);
      if (iYears < 1) return Json("0",     JsonRequestBehavior.AllowGet);

      if (iId < 0) return Json("Error", JsonRequestBehavior.AllowGet);
      if (iId >= MvcApplication.ckcore.ltPlayer.Count) return Json("Error", JsonRequestBehavior.AllowGet);

      // Get player
      CornerkickGame.Player plContract = MvcApplication.ckcore.ltPlayer[iId];

      // Convert salary to int
      int iSalary = convertCurrencyToInt(sSalary);
      if (iSalary < 0) return Json("Error", JsonRequestBehavior.AllowGet);

      // Convert salary to int
      int iBonusPlay = convertCurrencyToInt(sBonusPlay);
      if (iBonusPlay < 0) return Json("Error", JsonRequestBehavior.AllowGet);

      // Convert salary to int
      int iBonusGoal = convertCurrencyToInt(sBonusGoal);
      if (iBonusGoal < 0) return Json("Error", JsonRequestBehavior.AllowGet);

      // Convert salary to int
      int iFixTransferFee = convertCurrencyToInt(sFixTransferFee);
      if (iFixTransferFee < 0) return Json("Error", JsonRequestBehavior.AllowGet);

      // Convert player mood to double
      sPlayerMood = sPlayerMood.Replace("%", string.Empty);
      sPlayerMood = sPlayerMood.Replace(".", string.Empty);
      sPlayerMood = sPlayerMood.Trim();
      float fPlayerMood = 1f;
      if (!float.TryParse(sPlayerMood, out fPlayerMood)) return Json("Error", JsonRequestBehavior.AllowGet);
      fPlayerMood /= 100f;

      string sReturn = "";
      if (iMode == 0) { // contract extention
        if (plContract.contract.iLength + iYears > 10) return Json("Error: Maximale Vertragslänge = 10 Jahre", JsonRequestBehavior.AllowGet);

        plContract.contract.iLength += (byte)iYears;
        plContract.contract.iSalary = iSalary;
        plContract.contract.iPlay = iBonusPlay;
        plContract.contract.iGoal = iBonusGoal;
        plContract.contract.iFixTransferFee = iFixTransferFee;
        plContract.contract.fMood = fPlayerMood;

        if (plContract.iClubId >= 0 && plContract.iClubId < MvcApplication.ckcore.ltClubs.Count) {
          CornerkickManager.Club clb = MvcApplication.ckcore.ltClubs[plContract.iClubId];

          for (int iPlJ = 0; iPlJ < clb.ltPlayerJouth.Count; iPlJ++) {
            CornerkickGame.Player plJ = clb.ltPlayerJouth[iPlJ];
            if (plJ.iId == plContract.iId) {
              if ((int)plJ.getAge(MvcApplication.ckcore.dtDatum) < 16) return Json("Error: Spieler zu jung für Profivertrag", JsonRequestBehavior.AllowGet);

              clb.ltPlayerJouth.RemoveAt(iPlJ);
              clb.ltPlayer.Add(plJ);
              break;
            }
          }
        }

        sReturn = "Der Vertrag mit " + plContract.sName + " wurde um " + iYears.ToString() + " Jahre verlängert.";
      } else { // new contract
        if (iYears > 10) return Json("Error: Maximale Vertragslänge = 10 Jahre", JsonRequestBehavior.AllowGet);

        // create new offer
        CornerkickManager.Transfer.Offer offer = new CornerkickManager.Transfer.Offer();
        CornerkickGame.Player.Contract contract = new CornerkickGame.Player.Contract();
        contract.iLength = (byte)iYears;
        contract.iSalary = iSalary;
        contract.iPlay = iBonusPlay;
        contract.iGoal = iBonusGoal;
        contract.iFixTransferFee = iFixTransferFee;
        contract.fMood = fPlayerMood;
        offer.contract = contract;
        offer.club = ckClub();

        MvcApplication.ckcore.tr.addChangeOffer(iId, offer);
        sReturn = "Sie haben sich mit dem Spieler " + plContract.sName + " auf eine Zusammenarbeit über " + iYears.ToString() + " Jahre geeinigt.";
      }

      // Set status code to OK
      Response.StatusCode = 200;

      return Json(sReturn, JsonRequestBehavior.AllowGet);
    }

    private int convertCurrencyToInt(string s)
    {
      s = s.Replace("€", string.Empty);
      s = s.Replace(".", string.Empty);
      s = s.Trim();

      int i = 0;
      if (!int.TryParse(s, out i)) return -99999;

      return i;
    }

    [HttpPost]
    public JsonResult ExtentPlayerContract(int iPlayer, int iYears, string sSalary)
    {
      if (iPlayer < 0) return Json("Error", JsonRequestBehavior.AllowGet);
      if (iYears  < 1) return Json("0",     JsonRequestBehavior.AllowGet);

      sSalary = sSalary.Replace("€", string.Empty);
      sSalary = sSalary.Replace(".", string.Empty);
      sSalary = sSalary.Trim();

      int iSalary = 0;
      if (!int.TryParse(sSalary, out iSalary)) return Json("Error", JsonRequestBehavior.AllowGet);

      CornerkickGame.Player player = MvcApplication.ckcore.ltPlayer[iPlayer];
      //uint iSallery = MvcApplication.ckcore.plr.getSalary(player, (byte)iYears, 0);
      player.contract.iLength += (byte)iYears;
      player.contract.iSalary = iSalary;
      MvcApplication.ckcore.ltPlayer[iPlayer] = player;

      return Json("Der Vertrag mit " + player.sName + " wurde um " + iYears.ToString() + " Jahre verlängert.", JsonRequestBehavior.AllowGet);
    }

    [HttpPost]
    public JsonResult GetPlayerName(int iPlayer)
    {
      if (iPlayer < 0) return Json("", JsonRequestBehavior.AllowGet);

      return Json(MvcApplication.ckcore.ltPlayer[iPlayer].sName, JsonRequestBehavior.AllowGet);
    }

    //////////////////////////////////////////////////////////////////////////
    /// <summary>
    /// Jouth
    /// </summary>
    /// <param name="Jouth"></param>
    /// <returns></returns>
    //////////////////////////////////////////////////////////////////////////
    [Authorize]
    public ActionResult Jouth(Models.JouthModel jouth)
    {
      CornerkickManager.Club clb = ckClub();

      Models.JouthModel.ltPlayerJouth = new List<CornerkickGame.Player>();

      if (clb != null) {
        foreach (CornerkickGame.Player plJ in clb.ltPlayerJouth) {
          /*
          // Change Birthday if too young
          if (MvcApplication.ckcore.game.tl.getPlayerAgeFloat(sp, MvcApplication.ckcore.dtDatum) < 15) {
            sp.dtGeburt = new DateTime(sp.dtGeburt.Year - 5, sp.dtGeburt.Month, sp.dtGeburt.Day);
            MvcApplication.ckcore.ltPlayer[iSp] = sp;
          }
          */

          Models.JouthModel.ltPlayerJouth.Add(plJ);
        }
      }

      return View(jouth);
    }

    [HttpPost]
    public JsonResult TakeJouth(int iJouthID)
    {
      if (iJouthID < 0) return Json("", JsonRequestBehavior.AllowGet);

      CornerkickManager.Club clb = ckClub();
      if (clb == null) return Json(false, JsonRequestBehavior.AllowGet);

      CornerkickGame.Player plJ = MvcApplication.ckcore.ltPlayer[iJouthID];
      clb.ltPlayerJouth.Remove(plJ);
      clb.ltPlayer     .Add   (plJ);

      plJ.contract = MvcApplication.ckcore.plr.getContract(plJ, 2, MvcApplication.ckcore.fz.iMoneyTotal());

      return Json("", JsonRequestBehavior.AllowGet);
    }

    //////////////////////////////////////////////////////////////////////////
    /// <summary>
    /// Transfer
    /// </summary>
    /// <param name="Transfer"></param>
    /// <returns></returns>
    //////////////////////////////////////////////////////////////////////////
    [Authorize]
    public ActionResult Transfer(Models.TransferModel mdTransfer)
    {
      CornerkickManager.Club clb = ckClub();
      if (clb == null) return View(mdTransfer);

      mdTransfer.iContractYears = 1;

      mdTransfer.bNation = clb.bNation;

      mdTransfer.ddlFilterNation.Add(new SelectListItem { Text = "Alle", Value = "-1", Selected = !clb.bNation });
      foreach (byte iN in MvcApplication.iNations) {
        string sLand = "";
        bool bSelected = false;

        if (clb.bNation) {
          sLand = CornerkickManager.Main.sLand[iN];
          bSelected = iN == clb.iLand;
        } else {
          CornerkickManager.Cup leage = MvcApplication.ckcore.tl.getCup(1, iN, 0);
          if (leage != null) sLand = leage.sName;
        }

        mdTransfer.ddlFilterNation.Add(new SelectListItem { Text = sLand, Value = iN.ToString(), Selected = bSelected });
      }

      return View(mdTransfer);
    }

    [HttpPost]
    public JsonResult TransferPutOnTakeFromTransferList(int iPlayerId)
    {
      if (iPlayerId < 0) return Json(null, JsonRequestBehavior.AllowGet);
      if (iPlayerId >= MvcApplication.ckcore.ltPlayer.Count) return Json(null, JsonRequestBehavior.AllowGet);

      CornerkickGame.Player pl = MvcApplication.ckcore.ltPlayer[iPlayerId];

      if (MvcApplication.ckcore.plr.onTransferlist(pl)) {
        for (int iT = 0; iT < MvcApplication.ckcore.ltTransfer.Count; iT++) {
          CornerkickManager.Transfer.Item transfer = MvcApplication.ckcore.ltTransfer[iT];

          if (transfer.player == pl) {
            MvcApplication.ckcore.ltTransfer.RemoveAt(iT);
            break;
          }
        }

        return Json("Der Spieler " + pl.sName + " wurde von der Transferliste genommen", JsonRequestBehavior.AllowGet);
      } else {
        if (MvcApplication.ckcore.tr.putPlayerOnTransferlist(iPlayerId, 0) == 2) {
          return Json("Der Spieler " + MvcApplication.ckcore.ltPlayer[iPlayerId].sName + " kann in dieser Saison den Verein nicht mehr wechslen", JsonRequestBehavior.AllowGet);
        }

        return Json("Der Spieler " + MvcApplication.ckcore.ltPlayer[iPlayerId].sName + " wurde auf die Transferliste gesetzt", JsonRequestBehavior.AllowGet);
      }

      return Json(null, JsonRequestBehavior.AllowGet);
    }

    [HttpPost]
    public JsonResult TakeFromTransferList(int iPlayerId)
    {
      if (iPlayerId <                                     0) return Json(null, JsonRequestBehavior.AllowGet);
      if (iPlayerId >= MvcApplication.ckcore.ltPlayer.Count) return Json(null, JsonRequestBehavior.AllowGet);

      CornerkickGame.Player pl = MvcApplication.ckcore.ltPlayer[iPlayerId];

      for (int iT = 0; iT < MvcApplication.ckcore.ltTransfer.Count; iT++) {
        CornerkickManager.Transfer.Item transfer = MvcApplication.ckcore.ltTransfer[iT];

        if (transfer.player == pl) {
          MvcApplication.ckcore.ltTransfer.RemoveAt(iT);
          break;
        }
      }

      return Json("Der Spieler " + pl.sName + " wurde von der Transferliste genommen", JsonRequestBehavior.AllowGet);
    }

    [HttpPost]
    public JsonResult MakeTransferOffer(int iPlayerId, int iTransferFee, int iTransferFeeSecret)
    {
      if (iPlayerId <                                     0) return Json(null, JsonRequestBehavior.AllowGet);
      if (iPlayerId >= MvcApplication.ckcore.ltPlayer.Count) return Json(null, JsonRequestBehavior.AllowGet);

      CornerkickGame.Player pl = MvcApplication.ckcore.ltPlayer[iPlayerId];

      string sReturn = "Error";

      CornerkickManager.Club club = ckClub();
      if (club == null) return Json(false, JsonRequestBehavior.AllowGet);

      CornerkickManager.Club clbGive = null;
      if (pl.iClubId >= 0) clbGive = MvcApplication.ckcore.ltClubs[pl.iClubId];

      // If no club ...
      if (clbGive == null) {
        // ... and not on transferlist already --> put on transferlist
        if (!MvcApplication.ckcore.plr.onTransferlist(pl)) MvcApplication.ckcore.tr.putPlayerOnTransferlist(pl, 0);
      }

      for (int iT = 0; iT < MvcApplication.ckcore.ltTransfer.Count; iT++) {
        CornerkickManager.Transfer.Item transfer = MvcApplication.ckcore.ltTransfer[iT];

        if (transfer.player == pl) {
          if (transfer.ltOffers != null) {
            for (int iO = 0; iO < transfer.ltOffers.Count; iO++) {
              CornerkickManager.Transfer.Offer offer = transfer.ltOffers[iO];
              if (offer.club == club) {
                if (!MvcApplication.ckcore.fz.checkDispoLimit(iTransferFee, club)) {
                  transfer.ltOffers.Remove(offer);
                  return Json("Ihr Kreditrahmen ist leider nicht hoch genug", JsonRequestBehavior.AllowGet);
                }

                if (iTransferFeeSecret > club.iBalanceSecret) {
                  transfer.ltOffers.Remove(offer);
                  return Json("Sie haben nicht genug Schwarzgeld...", JsonRequestBehavior.AllowGet);
                }

                if (clbGive == null) {
                  offer.iFee = 0;
                  offer.iFeeSecret = 0;
                  if (MvcApplication.ckcore.tr.transferPlayer(clbGive, iPlayerId, club)) {
                    sReturn = "Sie haben den vereinslosen Spieler " + pl.sName + " ablösefrei unter Vertrag genommen.";
                  }
                  break;
                }

                if (pl.contract.iFixTransferFee > 0) {
                  offer.iFee = pl.contract.iFixTransferFee;
                  offer.iFeeSecret = 0;
                  if (MvcApplication.ckcore.tr.transferPlayer(clbGive, iPlayerId, club, iTransferIx: iT)) {
                    sReturn = "Sie haben den Spieler " + pl.sName + " für die festgeschriebene Ablöse von " + offer.iFee.ToString("N0", getCi()) + " verpflichtet.";
                    MvcApplication.ckcore.sendNews(clbGive.user, "Ihr Spieler " + pl.sName + " wechselt mit sofortiger Wirkung für die festgeschriebene Ablöse von " + offer.iFee.ToString("N0", getCi()) + " zu " + club.sName, iType: CornerkickManager.Main.iNewsTypePlayerTransferOfferAccept, iId: iPlayerId);
                  }
                  break;
                }

                offer.dt = MvcApplication.ckcore.dtDatum;

                offer.iFee       = iTransferFee;
                offer.iFeeSecret = iTransferFeeSecret;

                transfer.ltOffers[iO] = offer;
                MvcApplication.ckcore.ltTransfer[iT] = transfer;

                MvcApplication.ckcore.tr.informUser(transfer, offer);

                if (clbGive != null) {
                  MvcApplication.ckcore.sendNews(clbGive.user, "Sie haben ein neues Transferangebot für den Spieler " + pl.sName + " erhalten!", iType: CornerkickManager.Main.iNewsTypePlayerTransferNewOffer, iId: iPlayerId);
                  sReturn = "Sie haben das Transferangebot für dem Spieler " + pl.sName + " erfolgreich abgegeben.";
                }

                pl.character.fMoney += 0.05f;

                break;
              }
            }
          }
        }
      }

      return Json(sReturn, JsonRequestBehavior.AllowGet);
    }

    [HttpPost]
    public JsonResult AcceptTransferOffer(int iPlayerId, int iClubId)
    {
      string sReturn = "Error";

      CornerkickManager.Club clubTake = MvcApplication.ckcore.ltClubs[iClubId];
      if (MvcApplication.ckcore.tr.transferPlayer(ckClub(), iPlayerId, clubTake)) {
        sReturn = "Sie haben das Transferangebot für dem Spieler " + MvcApplication.ckcore.ltPlayer[iPlayerId].sName + " angenommen. Er wechselt mit sofortiger Wirkung zu " + clubTake.sName;
      }

      return Json(sReturn, JsonRequestBehavior.AllowGet);
    }

    [HttpPost]
    public JsonResult CancelTransferOffer(int iPlayerId)
    {
      string sReturn = "Error";

      if (MvcApplication.ckcore.tr.cancelTransferOffer(iPlayerId, ckClub())) {
        CornerkickGame.Player player = MvcApplication.ckcore.ltPlayer[iPlayerId];
        player.character.fMoney -= 0.05f;
        sReturn = "Sie haben Ihr Transferangebot für dem Spieler " + player.sName + " zurückgezogen.";
      }

      return Json(sReturn, JsonRequestBehavior.AllowGet);
    }

    [HttpPost]
    public JsonResult TransferAddToRemFromFavorites(int iPlayerId)
    {
      CornerkickManager.User usr = ckUser();
      if (usr == null) return Json("", JsonRequestBehavior.AllowGet);

      CornerkickGame.Player plFav = MvcApplication.ckcore.ltPlayer[iPlayerId];
      if (usr.ltPlayerFavorites.IndexOf(plFav) >= 0) usr.ltPlayerFavorites.Remove(plFav);
      else                                           usr.ltPlayerFavorites.Add(plFav);

      return Json("Ok", JsonRequestBehavior.AllowGet);
    }

    [HttpPost]
    public JsonResult TransferNominatePlayer(int iPlayerId)
    {
      CornerkickManager.Club nation = ckClub();
      if (nation == null) return Json(null, JsonRequestBehavior.AllowGet);
      if (!nation.bNation) return Json(null, JsonRequestBehavior.AllowGet);

      CornerkickGame.Player plNom = MvcApplication.ckcore.ltPlayer[iPlayerId];

      if (plNom.iNat1 != nation.iLand) return Json(null, JsonRequestBehavior.AllowGet);

      bool bNominate = !CornerkickManager.Player.ownPlayer(nation, plNom);
      if (bNominate) nation.ltPlayer.Add   (plNom);
      else           nation.ltPlayer.Remove(plNom);

      return Json(bNominate, JsonRequestBehavior.AllowGet);
    }

    public ActionResult getTableTransfer(int iPos, int iFType, int iFValue, bool bJouth, int iType, bool bFixTransferFee, int iClubId = -9, int iNation = -1)
    {
      //The table or entity I'm querying
      List<Models.DatatableEntryTransfer> ltDeTransfer = new List<Models.DatatableEntryTransfer>();

      //int iClub = -9;
      //if (bNoClub) iClub = -1;

      CornerkickManager.User usr = ckUser();

      List<CornerkickGame.Player> ltPlayer = null;
      if (iType == 1) {
        ltPlayer = MvcApplication.ckcore.ltPlayer;
      } else if (iType == 2) {
        if (usr == null) return Json(new { aaData = ltDeTransfer.ToArray() }, JsonRequestBehavior.AllowGet);

        ltPlayer = usr.ltPlayerFavorites;
      }

      int iTr = 0;
      foreach (CornerkickManager.Transfer.Item transfer in MvcApplication.ckcore.ui.filterTransferlist(sName: "",
                                                                                                       iClubId: iClubId,
                                                                                                       iPos: iPos,
                                                                                                       fStrength: -1f,
                                                                                                       iTalent: 0,
                                                                                                       iFType: iFType,
                                                                                                       iF: iFValue,
                                                                                                       bJouth: bJouth,
                                                                                                       ltPlayer: ltPlayer,
                                                                                                       bFixTransferFee: bFixTransferFee,
                                                                                                       iNation: iNation)) {
        string sClub = "vereinslos";
        if (transfer.player.iClubId >= 0) {
          sClub = MvcApplication.ckcore.ltClubs[transfer.player.iClubId].sName;
        }

        int iOffer = 0;
        int iFixtransferfee = 0;
        CornerkickManager.Club clubUser = ckClub();
        if (clubUser.iId > 0 && transfer.player.contract != null) {
          if      (MvcApplication.ckcore.tr.negotiationCancelled(clubUser, transfer.player)) iOffer = -1;
          else if (MvcApplication.ckcore.tr.alreadyOffered      (clubUser, transfer.player)) iOffer = +1;
          else if (CornerkickManager.Player.ownPlayer           (clubUser, transfer.player)) iOffer = +2;
          else if (transfer.player.iClubId >= 0 && transfer.player.contract.iFixTransferFee < 1 && !MvcApplication.ckcore.plr.onTransferlist(transfer.player)) iOffer = -2;

          iFixtransferfee = transfer.player.contract.iFixTransferFee;
        }

        string sDatePutOnTl = "-";
        if (transfer.dt.Year > 1) sDatePutOnTl = transfer.dt.ToString("d", getCi());

        ltDeTransfer.Add(new Models.DatatableEntryTransfer {
          playerId = transfer.player.iId,
          empty = "",
          iOffer = iOffer,
          index = (iTr + 1).ToString(),
          datum = sDatePutOnTl,
          name = transfer.player.sName,
          position = CornerkickManager.Player.getStrPos(transfer.player),
          strength      = CornerkickGame.Tool.getAveSkill(transfer.player, bIdeal: false).ToString("0.0"),
          strengthIdeal = CornerkickGame.Tool.getAveSkill(transfer.player, bIdeal: true) .ToString("0.0"),
          age = ((int)transfer.player.getAge(MvcApplication.ckcore.dtDatum)).ToString(),
          talent = (transfer.player.iTalent + 1).ToString(),
          mw = transfer.player.getValue(MvcApplication.ckcore.dtDatum) * 1000,
          fixtransferfee = iFixtransferfee,
          club = sClub,
          nat = CornerkickManager.Main.sLandShort[transfer.player.iNat1]
        });

        iTr++;
      }

      return Json(new { aaData = ltDeTransfer.ToArray() }, JsonRequestBehavior.AllowGet);
    }

    [HttpPost]
    public ActionResult getTableTransferDetails2(int iPlayerId)
    {
      if (iPlayerId <                                     0) return Json(null, JsonRequestBehavior.AllowGet);
      if (iPlayerId >= MvcApplication.ckcore.ltPlayer.Count) return Json(null, JsonRequestBehavior.AllowGet);

      string sTable = "";

      CornerkickGame.Player pl = MvcApplication.ckcore.ltPlayer[iPlayerId];

      CornerkickManager.Club clubUser = ckClub();

      sTable += "<table id=\"tableTransferDetails\" cellspacing=\"0\" style =\"width: auto\" class=\"display responsive nowrap\" > ";
      sTable += "<thead>";
      sTable += "<tr>";
      sTable += "<th>#</th>";
      sTable += "<th>Datum</th>";
      sTable += "<th>Verein</th>";
      sTable += "<th>Ablöse</th>";
      sTable += "<th></th>";
      sTable += "</tr>";
      sTable += "</thead>";
      sTable += "<tbody>";

      int iTr = 0;
      foreach (CornerkickManager.Transfer.Item transfer in MvcApplication.ckcore.ltTransfer) {
        if (transfer.player == pl) {
          if (transfer.ltOffers == null) break;

          bool bOwnPlayer = CornerkickManager.Player.ownPlayer(clubUser, MvcApplication.ckcore.ltPlayer[iPlayerId]);

          foreach (CornerkickManager.Transfer.Offer offer in transfer.ltOffers) {
            if (bOwnPlayer || // If own player
                clubUser == offer.club) {
              string sClub = "vereinslos";
              if (offer.club != null) {
                sClub = offer.club.sName;
              }

              string sStyle = "";
              if (offer.club == clubUser) sStyle = "font-weight:bold";

              sTable += "<tr id=\"rowTransferDetail_" + offer.club.iId.ToString() + "\" style=" + sStyle + ">";
              sTable += "<td>" + (iTr + 1).ToString() + "</td>";
              sTable += "<td align=\"center\">" + offer.dt.ToString("d", getCi()) + "</td>";
              sTable += "<td align=\"center\">" + sClub + "</td>";
              sTable += "<td align=\"right\">" + offer.iFee.ToString("N0", getCi()) + " €" + "</td>";

              if (bOwnPlayer) {
                string sChecked = "";
                //if (iTr == 0) sChecked = " checked";
                sTable += "<td><input type=\"radio\" id=\"rB_OfferClubId_" + iTr.ToString() + "\" name=\"OfferClubId\" onclick=\"handleClick(this);\" value =\"" + offer.club.iId.ToString() + "\"" + sChecked + "></td>";
              }

              sTable += "</tr>";

              iTr++;
            }
          }

          break;
        }
      }

      sTable += "</tbody>";
      sTable += "</table>";

      // Reset sTable if no offers
      if (iTr == 0) sTable = "";

      return Json(sTable, JsonRequestBehavior.AllowGet);
    }

    //////////////////////////////////////////////////////////////////////////
    /// <summary>
    /// Taktik
    /// </summary>
    /// <param name="Taktik"></param>
    /// <returns></returns>
    //////////////////////////////////////////////////////////////////////////
    [Authorize]
    public ActionResult Taktik(Models.TaktikModel tactic)
    {
      CornerkickManager.Club clb = ckClub();
      if (clb == null) return View(tactic);
      tactic.tactic = clb.ltTactic[0];

      string[] sStandards = new string[4] { "11m", "Freistoß", "Ecke R.", "Ecke L." };

      tactic.ltDdlStandards = new List<SelectListItem>[4];
      for (byte iS = 0; iS < 4; iS++) {
        tactic.ltDdlStandards[iS] = new List<SelectListItem>();
        tactic.ltDdlStandards[iS].Add(new SelectListItem { Text = "auto (" + sStandards[iS] + ")", Value = "-1" });
        for (byte iPl = 0; iPl < MvcApplication.ckcore.game.data.nPlStart; iPl++) {
          if (clb.ltPlayer.Count <= iPl) break;

          CornerkickGame.Player pl = clb.ltPlayer[iPl];
          tactic.ltDdlStandards[iS].Add(new SelectListItem { Text = pl.sName, Value = iPl.ToString(), Selected = iPl == clb.iStandards[iS] });
        }
      }

      const byte nSubs = 3;
      tactic.ddlAutoSubsOut = new List<SelectListItem>[nSubs];
      tactic.ddlAutoSubsIn  = new List<SelectListItem>[nSubs];
      tactic.iAutoSubsMin   = new int                 [nSubs];

      if (clb.nextGame != null) {
        byte iHA = 0;
        if (clb.nextGame.team[1].iTeamId == clb.iId) iHA = 1;
        for (byte iAS = 0; iAS < nSubs; iAS++) {
          tactic.iAutoSubsMin[iAS] = 60;
          if (iAS < clb.nextGame.team[iHA].ltSubstitutionsPlanned.Count) tactic.iAutoSubsMin[iAS] = clb.nextGame.team[iHA].ltSubstitutionsPlanned[iAS][2];
        }
      }

      return View(tactic);
    }

    public ActionResult setTaktik(int iTaktik, float fTaktik, byte iTactic)
    {
      float fRet = 0f;

      CornerkickManager.User usr = ckUser();
      CornerkickManager.Club clb = ckClub();
      if (clb == null) return Json(false, JsonRequestBehavior.AllowGet);

      if      (iTaktik == 0) clb.ltTactic[iTactic].fOrientation = fTaktik;
      else if (iTaktik == 1) clb.ltTactic[iTactic].fPower       = fTaktik;
      else if (iTaktik == 2) clb.ltTactic[iTactic].fShootFreq   = fTaktik;
      else if (iTaktik == 3) clb.ltTactic[iTactic].fAggressive  = fTaktik;
      else if (iTaktik == 4) clb.ltTactic[iTactic].fPassRisk    = fTaktik;
      else if (iTaktik == 5) clb.ltTactic[iTactic].fPassLength  = fTaktik;
      else if (iTaktik == 6) clb.ltTactic[iTactic].fPassFreq    = fTaktik;
      else if (iTaktik == 7) {
        clb.ltTactic[iTactic].fPassLeft  = fTaktik;
        if (clb.ltTactic[iTactic].fPassLeft + clb.ltTactic[iTactic].fPassRight > 1f) clb.ltTactic[iTactic].fPassRight = (float)Math.Round(1f - clb.ltTactic[iTactic].fPassLeft,  2);
        fRet = clb.ltTactic[iTactic].fPassRight;
      } else if (iTaktik == 8) {
        clb.ltTactic[iTactic].fPassRight = fTaktik;
        if (clb.ltTactic[iTactic].fPassLeft + clb.ltTactic[iTactic].fPassRight > 1f) clb.ltTactic[iTactic].fPassLeft  = (float)Math.Round(1f - clb.ltTactic[iTactic].fPassRight, 2);
        fRet = clb.ltTactic[iTactic].fPassLeft;
      } else if (iTaktik == 9) clb.ltTactic[iTactic].iAngriffAbseits = (int)Math.Round(fTaktik);

      // Set tactic of current game
      if (usr.game != null) {
        if (!usr.game.data.bFinished) {
          if      (usr.game.data.team[0].iTeamId == clb.iId) usr.game.data.team[0].ltTactic[iTactic] = clb.ltTactic[iTactic];
          else if (usr.game.data.team[1].iTeamId == clb.iId) usr.game.data.team[1].ltTactic[iTactic] = clb.ltTactic[iTactic];
        }
      }

      return Json(fRet, JsonRequestBehavior.AllowGet);
    }

    public ActionResult TacticSetOffsite(bool bOffsite, byte iTactic)
    {
      CornerkickManager.User usr = ckUser();
      CornerkickManager.Club clb = ckClub();
      if (clb == null) return Json(false, JsonRequestBehavior.AllowGet);

      clb.ltTactic[iTactic].bOffsite = bOffsite;

      if (usr.game != null) {
        if (!usr.game.data.bFinished) {
          if      (usr.game.data.team[0].iTeamId == clb.iId) usr.game.data.team[0].ltTactic[iTactic] = clb.ltTactic[iTactic];
          else if (usr.game.data.team[1].iTeamId == clb.iId) usr.game.data.team[1].ltTactic[iTactic] = clb.ltTactic[iTactic];
        }
      }

      return Json(null, JsonRequestBehavior.AllowGet);
    }

    public ActionResult setStandards(int iStandard, int iIndexPlayer)
    {
      CornerkickManager.User usr = ckUser();
      CornerkickManager.Club clb = ckClub();
      if (clb == null) return Json(false, JsonRequestBehavior.AllowGet);

      clb.iStandards[iStandard] = iIndexPlayer;

      // Set standards of current game
      if (usr.game != null) {
        if (!usr.game.data.bFinished) {
          if      (usr.game.data.team[0].iTeamId == clb.iId) usr.game.data.team[0].iStandards = clb.iStandards;
          else if (usr.game.data.team[1].iTeamId == clb.iId) usr.game.data.team[1].iStandards = clb.iStandards;
        }
      }

      return Json("", JsonRequestBehavior.AllowGet);
    }

    public JsonResult getHtmlAutoSubOut(Models.TaktikModel tactic, byte iAS)
    {
      string[] sBox = new string[2]; // [Out, In]

      CornerkickManager.Club clb = ckClub();
      if (clb == null) return Json(false, JsonRequestBehavior.AllowGet);

      if (clb.nextGame == null) Json(sBox, JsonRequestBehavior.AllowGet);

      byte iHA = 0;
      if (clb.nextGame.team[1].iTeamId == clb.iId) iHA = 1;

      sBox[0] += "<select name=\"sAutoSubsOut" + iAS.ToString() + "\" class=\"form-control\" id =\"ddlAutoSubOut" + iAS.ToString() + "\" onchange =\"setAutoSubs(" + iAS.ToString() + ")\"><option value=\"-1\">aus</option>";
      sBox[1] += "<select name=\"sAutoSubsIn" + iAS.ToString() + "\" class=\"form-control\" id =\"ddlAutoSubIn" + iAS.ToString() + "\" onchange =\"setAutoSubs(" + iAS.ToString() + ")\"><option value=\"-1\">aus</option>";

      // foreach player
      for (byte iPl = 0; iPl < MvcApplication.ckcore.game.data.nPlStart + MvcApplication.ckcore.game.data.nPlRes; iPl++) {
        CornerkickGame.Player pl = clb.ltPlayer[iPl];

        bool bOut = iPl < MvcApplication.ckcore.game.data.nPlStart;

        bool bContinue = false;
        byte jAS = 0;
        while (jAS < iAS) {
          if ( bOut && iPl == clb.nextGame.team[iHA].ltSubstitutionsPlanned[jAS][0]) bContinue = true;
          if (!bOut && iPl == clb.nextGame.team[iHA].ltSubstitutionsPlanned[jAS][1]) bContinue = true;
          jAS++;
        }
        if (bContinue) continue;

        string sSelectedO = "";
        string sSelectedI = "";
        if (iAS < clb.nextGame.team[iHA].ltSubstitutionsPlanned.Count) {
          if (clb.nextGame.team[iHA].ltSubstitutionsPlanned[iAS][1] > clb.nextGame.team[iHA].ltSubstitutionsPlanned[iAS][0]) {
            if (iPl == clb.nextGame.team[iHA].ltSubstitutionsPlanned[iAS][0]) sSelectedO = " selected=\"selected\"";
            if (iPl == clb.nextGame.team[iHA].ltSubstitutionsPlanned[iAS][1]) sSelectedI = " selected=\"selected\"";
          }
        }

        string sPos = "";
        string sStrength = "";
        pl.iIndex = iPl;
        if (iPl < MvcApplication.ckcore.game.data.nPlStart) {
          byte iPosRole = CornerkickGame.Tool.getBasisPos(CornerkickGame.Tool.getPosRole(pl, clb.ltTactic[0].formation, MvcApplication.ckcore.game.ptPitch));
          sPos = CornerkickManager.Main.sPosition[iPosRole];
          sStrength = CornerkickGame.Tool.getAveSkill(pl, iPos: iPosRole, bIdeal: false).ToString(" (0.0)");
        } else {
          sPos = CornerkickManager.Player.getStrPos(pl);
          sStrength = CornerkickGame.Tool.getAveSkill(pl, bIdeal: false).ToString(" (0.0)");
        }

        if (bOut) sBox[0] += "<option" + sSelectedO + " value=\"" + iPl.ToString() + "\">" + pl.sName + " - " + sPos + sStrength + "</option>";
        else      sBox[1] += "<option" + sSelectedI + " value=\"" + iPl.ToString() + "\">" + pl.sName + " - " + sPos + sStrength + "</option>";
      }

      sBox[0] += "</select>";
      sBox[1] += "</select>";

      return Json(sBox, JsonRequestBehavior.AllowGet);
    }

    public ActionResult setAutoSubs(int iAS, int iIndexPlayerOut, int iIndexPlayerIn, byte iMin)
    {
      CornerkickManager.Club clb = ckClub();
      if (clb == null) return Json(false, JsonRequestBehavior.AllowGet);

      if (clb.nextGame == null) Json(false, JsonRequestBehavior.AllowGet);

      byte iHA = 0;
      if (clb.nextGame.team[1].iTeamId == clb.iId) iHA = 1;

      while (clb.nextGame.team[iHA].ltSubstitutionsPlanned.Count <= iAS) clb.nextGame.team[iHA].ltSubstitutionsPlanned.Add(new byte[3] { 0, 0, 0 });

      if (iIndexPlayerOut < 0 || iIndexPlayerIn < 0) {
        iIndexPlayerOut = 0;
        iIndexPlayerIn  = 0;
      }
      clb.nextGame.team[iHA].ltSubstitutionsPlanned[iAS][0] = (byte)iIndexPlayerOut;
      clb.nextGame.team[iHA].ltSubstitutionsPlanned[iAS][1] = (byte)iIndexPlayerIn;
      clb.nextGame.team[iHA].ltSubstitutionsPlanned[iAS][2] = iMin;

      bool bValid = iIndexPlayerOut >= 0 && iIndexPlayerIn >= 0 && iIndexPlayerOut != iIndexPlayerIn && iMin >= 0;

      if (!bValid) {
        int jAS = iAS + 1;
        while (jAS < 3) {
          clb.nextGame.team[iHA].ltSubstitutionsPlanned[jAS][0] = 0;
          clb.nextGame.team[iHA].ltSubstitutionsPlanned[jAS][1] = 0;
          jAS++;
        }
      }

      return Json(bValid, JsonRequestBehavior.AllowGet);
    }

    //////////////////////////////////////////////////////////////////////////
    /// <summary>
    /// Training
    /// </summary>
    /// <param name="Training"></param>
    /// <returns></returns>
    //////////////////////////////////////////////////////////////////////////
    readonly static TimeSpan[] tsTraining = new TimeSpan[] { new TimeSpan(9, 30, 00), new TimeSpan(12, 00, 00), new TimeSpan(16, 30, 00) };

    [Authorize]
    public ActionResult Training(Models.TrainingModel mdTraining)
    {
      CornerkickManager.Club clb = ckClub();
      if (clb == null) return View(mdTraining);

      return View(mdTraining);
    }

    private static CornerkickManager.Main.TrainingPlan.Unit[][] getTrainingPlan(CornerkickManager.Club clb, int iWeek)
    {
      if (clb == null) return null;

      // Get last Sunday
      DateTime dtSunday = MvcApplication.ckcore.dtDatum.Date.AddDays(iWeek * 7);
      while ((int)(dtSunday.DayOfWeek) != 0) dtSunday = dtSunday.AddDays(-1);

      CornerkickManager.Main.TrainingPlan.Unit[][] ltTu = new CornerkickManager.Main.TrainingPlan.Unit[7][]; // For each day of week
      for (int iD = 0; iD < ltTu.Length; iD++) { // Loop until Saturday
        ltTu[iD] = new CornerkickManager.Main.TrainingPlan.Unit[3];

        foreach (CornerkickManager.Main.TrainingHistory th in clb.ltTrainingHist) {
          if (th.dt.Date.Equals(dtSunday.AddDays(iD))) {
            int iIxTimeOfDay = 0; // 1st training
            if      (th.dt.Hour >= tsTraining[2].Hours) iIxTimeOfDay = 2; // 3rd training
            else if (th.dt.Hour >= tsTraining[1].Hours) iIxTimeOfDay = 1; // 2nd training

            if (ltTu[iD][iIxTimeOfDay] != null && ltTu[iD][iIxTimeOfDay].iType < 0) continue; // Already set

            CornerkickManager.Main.TrainingPlan.Unit tuPast = new CornerkickManager.Main.TrainingPlan.Unit();
            tuPast.dt = th.dt;
            tuPast.iType = (sbyte)-(th.iType + 1);

            ltTu[iD][iIxTimeOfDay] = tuPast;
          }
        }

        foreach (CornerkickManager.Main.TrainingPlan.Unit tu in clb.training.ltUnit) {
          if ((tu.dt.Date - dtSunday.AddDays(iD)).TotalDays == 0) {
            int iIxTimeOfDay = 0; // 1st training
            if      (tu.dt.Hour >= tsTraining[2].Hours) iIxTimeOfDay = 2; // 3rd training
            else if (tu.dt.Hour >= tsTraining[1].Hours) iIxTimeOfDay = 1; // 2nd training

            //if (tu.dt.CompareTo(MvcApplication.ckcore.dtDatum) <= 0) tu.iType *= -1;

            ltTu[iD][iIxTimeOfDay] = tu;
          }
        }
      }

      // Set training type 'free' if null
      for (int iD = 0; iD < ltTu.Length; iD++) { // Loop until Saturday
        for (int iT = 0; iT < ltTu[iD].Length; iT++) {
          if (ltTu[iD][iT] == null) {
            DateTime dtTraining = dtSunday.AddDays(iD).Add(tsTraining[iT]);

            sbyte iType = 0;
            if (dtTraining.CompareTo(MvcApplication.ckcore.dtDatum) <= 0) iType = -1; // Past training

            ltTu[iD][iT] = new CornerkickManager.Main.TrainingPlan.Unit() { dt = dtTraining, iType = iType };
          }
        }
      }

      return ltTu;
    }

    [HttpPost]
    public JsonResult TrainingGetPlan(int iWeek)
    {
      return Json(getTrainingPlan(ckClub(), iWeek), JsonRequestBehavior.AllowGet);
    }

    public ActionResult setTraining(int iTrainingType, int iDay, int iIxTimeOfDay)
    {
      CornerkickManager.Club clb = ckClub();
      if (clb == null) return Json(false, JsonRequestBehavior.AllowGet);

      // Get last Sunday
      DateTime dtSunday = MvcApplication.ckcore.dtDatum.Date;
      while ((int)(dtSunday.DayOfWeek) != 0) dtSunday = dtSunday.AddDays(-1);

      DateTime dtTraining = dtSunday.AddDays(iDay).Add(tsTraining[iIxTimeOfDay]);

      if (dtTraining.CompareTo(MvcApplication.ckcore.dtDatum) < 0) return Json(false, JsonRequestBehavior.AllowGet); // Return, if in past

      CornerkickManager.Main.TrainingPlan.Unit tu = clb.training.getTrainingUnit(dtTraining);
      if (tu == null) {
        tu = new CornerkickManager.Main.TrainingPlan.Unit();
        tu.dt = dtTraining;
        clb.training.ltUnit.Add(tu);
      }

      tu.iType = (sbyte)iTrainingType;

      return Json(iTrainingType, JsonRequestBehavior.AllowGet);
    }

    [HttpPost]
    public JsonResult TrainingCopyPlan(int iWeek)
    {
      CornerkickManager.Club clb = ckClub();
      if (clb == null) return Json(false, JsonRequestBehavior.AllowGet);

      copyTrainingPlan(clb, iWeek);

      return Json(true, JsonRequestBehavior.AllowGet);
    }

    internal static void copyTrainingPlan(CornerkickManager.Club clb, int iWeek)
    {
      CornerkickManager.Main.TrainingPlan.Unit[][] tuPlan = getTrainingPlan(clb, iWeek);

      // Get next Sunday
      DateTime dtStartCopy = MvcApplication.ckcore.dtDatum.AddDays(iWeek * 7).Date;
      while ((int)(dtStartCopy.DayOfWeek) != 0) dtStartCopy = dtStartCopy.AddDays(+1);

      // First delete all trainings starting from next week
      DateTime dtTmp = dtStartCopy;
      while (dtTmp.CompareTo(MvcApplication.ckcore.dtSeasonEnd) < 0) {
        for (int iT = 0; iT < clb.training.ltUnit.Count; iT++) {
          if (clb.training.ltUnit[iT].dt.Date.Equals(dtTmp)) {
            clb.training.ltUnit.RemoveAt(iT--);
          }
        }

        dtTmp = dtTmp.AddDays(+1);
      }

      // Then copy trainings plan of current week
      dtTmp = dtStartCopy;
      while (dtTmp.CompareTo(MvcApplication.ckcore.dtSeasonEnd) < 0) {
        for (byte iD = 0; iD < tuPlan.Length; iD++) {
          for (byte iT = 0; iT < tuPlan[iD].Length; iT++) {
            if (tuPlan[iD][iT].iType >= 0) {
              // Check if already training planned
              /*
              bool bAlreadySet = false;
              foreach (CornerkickManager.Main.Training.Unit tu in ltTuToday) {
                if ((int)(tu.dt.DayOfWeek) == (int)(dtTmp.DayOfWeek) && tu.dt.TimeOfDay.Equals(tuPlan[iD][iT].dt.TimeOfDay)) {
                  bAlreadySet = true;
                  break;
                }
              }
              if (bAlreadySet) continue;
              */

              CornerkickManager.Main.TrainingPlan.Unit tuCopy = tuPlan[iD][iT].Clone();
              tuCopy.dt = dtTmp.Add(tuPlan[iD][iT].dt.TimeOfDay);
              clb.training.ltUnit.Add(tuCopy);
            }
          }

          dtTmp = dtTmp.AddDays(+1);
        }
      }
    }

    //////////////////////////////////////////////////////////////////////////
    /// <summary>
    /// Stadion
    /// </summary>
    /// <param name="Stadion"></param>
    /// <returns></returns>
    //////////////////////////////////////////////////////////////////////////
    [Authorize]
    public ActionResult Stadion(Models.StadionModel stadionModel)
    {
      CornerkickManager.User usr = ckUser();
      if (usr == null) return Json(false, JsonRequestBehavior.AllowGet);

      CornerkickManager.Club clb = ckClub();
      if (clb == null) return View(stadionModel);

      if (clb.stadium.blocks == null) return View(stadionModel);

      stadionModel.stadion = clb.stadium;

      stadionModel.sName = clb.stadium.sName;

      stadionModel.iSeats = new int[clb.stadium.blocks.Length];
      for (int i = 0; i < clb.stadium.blocks.Length; i++) {
        stadionModel.iSeats[i] = clb.stadium.blocks[i].iSeats;
      }

      stadionModel.iSeatType = new int[clb.stadium.blocks.Length];
      for (int i = 0; i < clb.stadium.blocks.Length; i++) {
        stadionModel.iSeatType[i] = clb.stadium.blocks[i].iType;
      }

      stadionModel.iSeatsBuild = new int[clb.stadium.blocks.Length];
      for (int i = 0; i < clb.stadium.blocks.Length; i++) {
        stadionModel.iSeatsBuild[i] = clb.stadium.blocks[i].iSeatsDaysConstruct;
      }

      stadionModel.bOberring = bStadiumGetTopring(clb);

      if (clb.stadium.iVideoDaysConstruct == 0) clb.stadium.iVideoNew = clb.stadium.iVideo;
      stadionModel.iVideo = clb.stadium.iVideoNew;

      stadionModel.stadionNew = convertToStadion(stadionModel.iSeats, stadionModel.iSeatType, stadionModel.iSeatsBuild);

      stadionModel.iSnackbarNew = (byte)Math.Max(clb.stadium.iSnackbarNew - clb.stadium.iSnackbar, 0);
      stadionModel.iToiletsNew  = (byte)Math.Max(clb.stadium.iToiletsNew  - clb.stadium.iToilets,  0);

      /*
      stModel.stadionNew = MvcApplication.ckcore.ini.newStadion();
      for (int i = 0; i < stModel.stadionNew.iPlaetze.Length; i++) {
        stModel.stadionNew.iPlaetze[i] = clb.stadium.iSeats[i];
      }

      if (stadionModel.iSeats == null) {
        stadionModel.iSeats = new int[clb.stadium.iSeats.Length];
        for (int i = 0; i < clb.stadium.iSeats.Length; i++) {
          stadionModel.iSeats[i] = clb.stadium.iSeats[i];
        }
      }
      */

      // Stadium name editable
      stadionModel.bEditable = (MvcApplication.ckcore.dtDatum - usr.dtClubStart).TotalHours < 24;
      stadionModel.bEditable = stadionModel.bEditable || (stadionModel.sName.StartsWith("Team_") && stadionModel.sName.EndsWith(" Stadion"));
      stadionModel.bEditable = stadionModel.bEditable || stadionModel.sName.Equals(clb.sName + " Stadion");

      return View(stadionModel);
    }

    /*
    public JsonResult StadionChange(Models.StadionModel stadionModel, string sId, int iSeats)
    {
      CornerkickManager.Club clb = ckClub();

      byte iB = 255;
      string[] sB = sId.Split('_');
      if (sB.Length < 1) return Json(0, JsonRequestBehavior.AllowGet);

      byte.TryParse(sB[sB.Length - 1], out iB);
      if (iB > stadionModel.stadion.iPlaetze.Length) return Json(0, JsonRequestBehavior.AllowGet);
      stadionModel.stadion.iPlaetze[iB] = iSeats;

      UpdateModel(stadionModel);

      int iKosten = MvcApplication.ckcore.tl.iKostenStadionAusbau(stadionModel.stadion, clb.stadium);

      return Json(iKosten, JsonRequestBehavior.AllowGet);
      //return View(stadionModel);
    }
    */

    [HttpPost]
    public JsonResult StadiumChange(int[] iSeats, int[] iArt)
    {
      CornerkickManager.Club clb = ckClub();
      if (clb == null) return Json(false, JsonRequestBehavior.AllowGet);

      CornerkickGame.Stadium stadiumNew = getStadiumUpdate(clb.stadium, iSeats, iArt);

      int[] iCostDays = CornerkickManager.Stadium.getCostDaysContructStadium(stadiumNew, clb.stadium, ckUser());
      int iDispoOk = 0;
      if (MvcApplication.ckcore.fz.checkDispoLimit(iCostDays[0], clb)) iDispoOk = 1;

      string[] sKostenDauer = new string[] { iCostDays[0].ToString("N0", getCi()), iCostDays[1].ToString(), iDispoOk.ToString() };

      return Json(sKostenDauer, JsonRequestBehavior.AllowGet);
    }

    [HttpPost]
    public JsonResult StadiumChangeSet(int[] iSeats, int[] iArt)
    {
      CornerkickManager.Club clb = ckClub();
      if (clb == null) return Json(false, JsonRequestBehavior.AllowGet);

      CornerkickGame.Stadium stadiumNew = getStadiumUpdate(clb.stadium, iSeats, iArt);

      MvcApplication.ckcore.ui.buildStadium(ref clb, stadiumNew);

      return Json("Der Ausbau des Stadions wurde in Auftrag gegeben", JsonRequestBehavior.AllowGet);
    }

    [HttpPost]
    public JsonResult StadiumGetCostTopring()
    {
      CornerkickManager.Club clb = ckClub();
      if (clb == null) return Json(false, JsonRequestBehavior.AllowGet);

      if (clb.stadium.bTopring) return Json(false, JsonRequestBehavior.AllowGet);
      if (clb.stadium.iTopringDaysConstruct > 0) return Json(false, JsonRequestBehavior.AllowGet);

      CornerkickGame.Stadium stadion = clb.stadium.Clone();
      stadion.bTopring = !clb.stadium.bTopring;

      int[] iCostDays = CornerkickManager.Stadium.getCostDaysContructStadium(stadion, clb.stadium, ckUser());
      int iDispoOk = 0;
      if (MvcApplication.ckcore.fz.checkDispoLimit(iCostDays[0], clb)) iDispoOk = 1;

      string[] sCostDays = new string[] { (iCostDays[0] / 1000000).ToString("N0", getCi()) + " mio. €", iCostDays[1].ToString(), iDispoOk.ToString() };

      return Json(sCostDays, JsonRequestBehavior.AllowGet);
    }

    [HttpPost]
    public JsonResult StadiumGetMaxSeats()
    {
      CornerkickManager.Club clb = ckClub();
      if (clb == null) return Json(false, JsonRequestBehavior.AllowGet);

      if (clb.stadium.bTopring) return Json(false, JsonRequestBehavior.AllowGet);
      if (clb.stadium.iTopringDaysConstruct > 0) return Json(false, JsonRequestBehavior.AllowGet);

      CornerkickGame.Stadium st = clb.stadium.Clone();
      st.bTopring = true;

      int iSeatsMaxLv0 = CornerkickManager.Stadium.getMaxSeats(st, 0);

      int[] iSeatsMax = new int[3];
      for (byte iS = 0; iS < iSeatsMax.Length; iS++) {
        iSeatsMax[iS] = (int)(iSeatsMaxLv0 * CornerkickManager.Stadium.getFactorBlockSeats(iS, 0));
      }

      return Json(iSeatsMax, JsonRequestBehavior.AllowGet);
    }

    private bool bStadiumGetTopring(CornerkickManager.Club clb)
    {
      return clb.stadium.bTopring && clb.stadium.iTopringDaysConstruct == 0;
    }

    [HttpPost]
    public JsonResult StadiumGetTopring()
    {
      CornerkickManager.Club clb = ckClub();
      return Json(bStadiumGetTopring(clb), JsonRequestBehavior.AllowGet);
    }

    [HttpPost]
    public JsonResult StadiumGetBlockProgress()
    {
      CornerkickManager.Club clb = ckClub();
      if (clb == null) return Json(false, JsonRequestBehavior.AllowGet);

      int nBlocksMax = clb.stadium.blocks.Length;
      if (!clb.stadium.bTopring) nBlocksMax = 10;
      float[] fPg = new float[nBlocksMax];

      for (byte iB = 0; iB < nBlocksMax; iB++) {
        int iDaysMax = CornerkickManager.Stadium.getCostDaysContructStadiumBlock(clb.stadium.blocks[iB], clb.stadium.blocks[iB], 0, ckUser())[0];
        if (clb.stadium.blocks[iB].iSeatsDaysConstruct > 0) fPg[iB] = (clb.stadium.blocks[iB].iSeatsDaysConstructIni - clb.stadium.blocks[iB].iSeatsDaysConstruct) / (float)clb.stadium.blocks[iB].iSeatsDaysConstructIni;
        else                                                fPg[iB] = -1f;
      }

      return Json(fPg, JsonRequestBehavior.AllowGet);
    }

    [HttpPost]
    public JsonResult StadiumGetTopringProgress()
    {
      CornerkickManager.Club clb = ckClub();
      if (clb == null) return Json(false, JsonRequestBehavior.AllowGet);

      CornerkickGame.Stadium st = clb.stadium.Clone();
      st.bTopring = false;

      int[] iCostDays = CornerkickManager.Stadium.getCostDaysContructStadium(clb.stadium, st, ckUser());
      return Json((iCostDays[1] - clb.stadium.iTopringDaysConstruct) / (float)iCostDays[1], JsonRequestBehavior.AllowGet);
    }

    [HttpPost]
    public JsonResult StadiumGetExtras()
    {
      CornerkickManager.Club clb = ckClub();
      if (clb == null) return Json(false, JsonRequestBehavior.AllowGet);

      CornerkickManager.User usr = ckUser();

      int nVideoDaysConstract    = CornerkickManager.Stadium.iVideoDaysConstruct[clb.stadium.iVideoNew];
      int nSnackbarDaysConstract = CornerkickManager.Stadium.getCostDaysContructSnackbar(clb.stadium.iSnackbarNew, clb.stadium.iSnackbar, usr)[1];
      int nToiletsDaysConstract  = CornerkickManager.Stadium.getCostDaysContructToilets (clb.stadium.iToiletsNew,  clb.stadium.iToilets,  usr)[1];
      float fVideoDaysConstract    = 0f;
      float fSnackbarDaysConstract = 0f;
      float fToiletsDaysConstract  = 0f;
      if (nVideoDaysConstract    > 0) fSnackbarDaysConstract = (nVideoDaysConstract    - clb.stadium.iVideoDaysConstruct  ) / (float)nVideoDaysConstract;
      if (nSnackbarDaysConstract > 0) fSnackbarDaysConstract = (nSnackbarDaysConstract - clb.stadium.iSnackbarDaysConstruct) / (float)nSnackbarDaysConstract;
      if (nToiletsDaysConstract  > 0) fToiletsDaysConstract  = (nToiletsDaysConstract  - clb.stadium.iToiletsDaysConstruct ) / (float)nToiletsDaysConstract;

      return Json(new string[3][] {
        new string [4] { CornerkickManager.Stadium.sVideo[clb.stadium.iVideo], CornerkickManager.Stadium.sVideo[clb.stadium.iVideoNew], clb.stadium.iVideoDaysConstruct   .ToString(), fVideoDaysConstract   .ToString("0.0%") },
        new string [4] { clb.stadium.iSnackbar.ToString(),                     clb.stadium.iSnackbarNew.ToString(),                     clb.stadium.iSnackbarDaysConstruct.ToString(), fSnackbarDaysConstract.ToString("0.0%") },
        new string [4] { clb.stadium.iToilets .ToString(),                     clb.stadium.iToiletsNew .ToString(),                     clb.stadium.iToiletsDaysConstruct .ToString(), fToiletsDaysConstract .ToString("0.0%") }
      }, JsonRequestBehavior.AllowGet);
    }

    [HttpPost]
    public JsonResult StadiumBuildTopring()
    {
      CornerkickManager.Club clb = ckClub();
      if (clb == null) return Json(false, JsonRequestBehavior.AllowGet);

      CornerkickGame.Stadium stadion = clb.stadium.Clone();
      stadion.bTopring = !clb.stadium.bTopring;

      MvcApplication.ckcore.ui.buildStadium(ref clb, stadion);

      return Json("Der Bau des Oberrings wurde in Auftrag gegeben", JsonRequestBehavior.AllowGet);
    }

    // Video-wall
    [HttpPost]
    public JsonResult StadiumGetCostVideo(byte iLevel)
    {
      string[] sCostDaysDispo = new string[3] { "0", "0", "0" };
      CornerkickManager.Club clb = ckClub();
      if (clb == null) return Json(false, JsonRequestBehavior.AllowGet);

      if (clb.stadium.iVideoNew != iLevel) {
        int iDispoOk = 0;
        if (MvcApplication.ckcore.fz.checkDispoLimit(CornerkickManager.Stadium.iVideoCost[iLevel], clb)) iDispoOk = 1;

        sCostDaysDispo[0] = CornerkickManager.Stadium.iVideoCost[iLevel].ToString("N0", getCi());
        sCostDaysDispo[1] = CornerkickManager.Stadium.iVideoDaysConstruct[iLevel].ToString();
        sCostDaysDispo[2] = iDispoOk.ToString();
      }

      return Json(sCostDaysDispo, JsonRequestBehavior.AllowGet);
    }

    [HttpPost]
    public JsonResult StadiumBuildVideo(byte iLevel)
    {
      CornerkickManager.Club clb = ckClub();
      if (clb == null) return Json(false, JsonRequestBehavior.AllowGet);

      CornerkickGame.Stadium stadion = clb.stadium.Clone();
      stadion.iVideoNew = iLevel;

      MvcApplication.ckcore.ui.buildStadium(ref clb, stadion);

      return Json("Der Bau der Anzeigentafel wurde in Auftrag gegeben", JsonRequestBehavior.AllowGet);
    }

    // Snackbars
    [HttpPost]
    public JsonResult StadiumGetCostSnackbar(int iBuildNew)
    {
      string[] sCostDaysDispo = new string[3] { "0", "0", "0" };

      CornerkickManager.User usr = ckUser();
      CornerkickManager.Club clb = ckClub();
      if (clb == null) return Json(false, JsonRequestBehavior.AllowGet);

      if (iBuildNew != 0) {
        int iDispoOk = 0;
        int[] iCostDays = CornerkickManager.Stadium.getCostDaysContructSnackbar(clb.stadium.iSnackbarNew + iBuildNew, clb.stadium.iSnackbarNew, usr);
        if (MvcApplication.ckcore.fz.checkDispoLimit(iCostDays[0], clb)) iDispoOk = 1;

        sCostDaysDispo[0] = iCostDays[0].ToString("N0", getCi());
        sCostDaysDispo[1] = iCostDays[1].ToString();
        sCostDaysDispo[2] = iDispoOk.ToString();
      }

      return Json(sCostDaysDispo, JsonRequestBehavior.AllowGet);
    }

    [HttpPost]
    public JsonResult StadiumBuildSnackbar(int iBuildNew)
    {
      CornerkickManager.Club clb = ckClub();
      if (clb == null) return Json(false, JsonRequestBehavior.AllowGet);

      CornerkickGame.Stadium stadion = clb.stadium.Clone();
      stadion.iSnackbarNew = (byte)(stadion.iSnackbar + iBuildNew);

      MvcApplication.ckcore.ui.buildStadium(ref clb, stadion);

      return Json("Der Ausbau der Imbissbuden wurde in Auftrag gegeben", JsonRequestBehavior.AllowGet);
    }

    // Toilets
    [HttpPost]
    public JsonResult StadiumGetCostToilets(int iBuildNew)
    {
      string[] sCostDaysDispo = new string[3] { "0", "0", "0" };

      CornerkickManager.User usr = ckUser();
      CornerkickManager.Club clb = ckClub();
      if (clb == null) return Json(false, JsonRequestBehavior.AllowGet);

      if (iBuildNew != 0) {
        int iDispoOk = 0;
        int[] iCostDays = CornerkickManager.Stadium.getCostDaysContructToilets(clb.stadium.iToiletsNew + iBuildNew, clb.stadium.iToiletsNew, usr);
        if (MvcApplication.ckcore.fz.checkDispoLimit(iCostDays[0], clb)) iDispoOk = 1;

        sCostDaysDispo[0] = iCostDays[0].ToString("N0", getCi());
        sCostDaysDispo[1] = iCostDays[1].ToString();
        sCostDaysDispo[2] = iDispoOk.ToString();
      }

      return Json(sCostDaysDispo, JsonRequestBehavior.AllowGet);
    }

    [HttpPost]
    public JsonResult StadiumBuildToilets(int iBuildNew)
    {
      CornerkickManager.Club clb = ckClub();
      if (clb == null) return Json(false, JsonRequestBehavior.AllowGet);

      CornerkickGame.Stadium stadion = clb.stadium.Clone();
      stadion.iToiletsNew = (byte)(clb.stadium.iToilets + iBuildNew);

      MvcApplication.ckcore.ui.buildStadium(ref clb, stadion);

      return Json("Der Ausbau der Toiletten wurde in Auftrag gegeben", JsonRequestBehavior.AllowGet);
    }

    internal CornerkickGame.Stadium convertToStadion(int[] iSeats, int[] iSeatType, int[] iSeatsBuild)
    {
      CornerkickGame.Stadium stadium = new CornerkickGame.Stadium();
      if (iSeats != null) {
        for (int i = 0; i < iSeats.Length; i++) stadium.blocks[i].iSeats = iSeats[i];
      }
      if (iSeatType != null) {
        for (int i = 0; i < iSeatType.Length; i++) stadium.blocks[i].iType = (byte)iSeatType[i];
      }
      if (iSeatsBuild != null) {
        for (int i = 0; i < iSeatsBuild.Length; i++) stadium.blocks[i].iSeatsDaysConstruct = iSeatsBuild[i];
      }

      return stadium;
    }

    internal CornerkickGame.Stadium getStadiumUpdate(CornerkickGame.Stadium stadiumCurrent, int[] iSeats, int[] iSeatType)
    {
      CornerkickGame.Stadium stadiumNew = stadiumCurrent.Clone();
      if (iSeats != null) {
        for (int i = 0; i < iSeats.Length; i++) stadiumNew.blocks[i].iSeats = iSeats[i];
      }
      if (iSeatType != null) {
        for (int i = 0; i < iSeatType.Length; i++) stadiumNew.blocks[i].iType = (byte)iSeatType[i];
      }

      return stadiumNew;
    }

    [HttpPost]
    public JsonResult StadiumRenewPitchCost()
    {
      CornerkickManager.Club clb = ckClub();
      if (clb == null) return Json(false, JsonRequestBehavior.AllowGet);

      return Json(MvcApplication.ckcore.st.getCostStadiumRenewPitch(clb.stadium, 0.1f, ckUser()).ToString("N0", getCi()), JsonRequestBehavior.AllowGet);
    }

    [HttpPost]
    public JsonResult StadiumRenewPitch()
    {
      CornerkickManager.Club clb = ckClub();
      if (clb == null) return Json(false, JsonRequestBehavior.AllowGet);

      MvcApplication.ckcore.ui.renewStadiumPitch(ref clb, 0.1f);

      return Json("Der Stadionrasen wurde erneuert", JsonRequestBehavior.AllowGet);
    }

    public void StadiumSetName(string sName)
    {
      CornerkickManager.Club clb = ckClub();
      if (clb == null) return;

      clb.stadium.sName = sName;
    }

    //////////////////////////////////////////////////////////////////////////
    /// <summary>
    /// Stadion
    /// </summary>
    /// <param name="Stadion"></param>
    /// <returns></returns>
    //////////////////////////////////////////////////////////////////////////
    [Authorize]
    public ActionResult StadiumSurroundings(Models.StadiumSurroundingsModel mdStadionSurr)
    {
      CornerkickManager.Club clb = ckClub();
      if (clb == null) return View(mdStadionSurr);

      mdStadionSurr.ddlTrainingsgel  = new List<SelectListItem>();
      mdStadionSurr.ddlGym           = new List<SelectListItem>();
      mdStadionSurr.ddlSpa           = new List<SelectListItem>();
      mdStadionSurr.ddlJouthInternat = new List<SelectListItem>();
      mdStadionSurr.ddlClubHouse     = new List<SelectListItem>();
      mdStadionSurr.ddlClubMuseum    = new List<SelectListItem>();

      /* DEPRECATED
      for (int i = clb.buildings.bgTrainingCourts.ctn.iLevelNew + 1; i < CornerkickManager.Stadium.sTrainingCourts.Length; i++) mdStadionSurr.ddlTrainingsgel .Add(new SelectListItem { Text = CornerkickManager.Stadium.sTrainingCourts[i], Value = i.ToString() });
      for (int i = clb.buildings.iGym           [1] + 1; i < CornerkickManager.Stadium.sGym           .Length; i++) mdStadionSurr.ddlGym          .Add(new SelectListItem { Text = CornerkickManager.Stadium.sGym           [i], Value = i.ToString() });
      for (int i = clb.buildings.iSpa           [1] + 1; i < CornerkickManager.Stadium.sSpa           .Length; i++) mdStadionSurr.ddlSpa          .Add(new SelectListItem { Text = CornerkickManager.Stadium.sSpa           [i], Value = i.ToString() });
      for (int i = clb.buildings.iJouthInternat [1] + 1; i < CornerkickManager.Stadium.sJouthInternat .Length; i++) mdStadionSurr.ddlJouthInternat.Add(new SelectListItem { Text = CornerkickManager.Stadium.sJouthInternat [i], Value = i.ToString() });
      for (int i = clb.buildings.iClubHouse     [1] + 1; i < CornerkickManager.Stadium.sClubHouse     .Length; i++) mdStadionSurr.ddlClubHouse    .Add(new SelectListItem { Text = CornerkickManager.Stadium.sClubHouse     [i], Value = i.ToString() });
      for (int i = clb.buildings.iClubMuseum    [1] + 1; i < CornerkickManager.Stadium.sClubMuseum    .Length; i++) mdStadionSurr.ddlClubMuseum   .Add(new SelectListItem { Text = CornerkickManager.Stadium.sClubMuseum    [i], Value = i.ToString() });
      */

      mdStadionSurr.iTrainingsgel     = clb.buildings.bgTrainingCourts.iLevel;
      //mdStadionSurr.iTrainingNew      = clb.buildings.iTrainingCourts[2];
      mdStadionSurr.iGym              = clb.buildings.bgGym.iLevel;
      //mdStadionSurr.iGymNew           = clb.buildings.iGym[2];
      mdStadionSurr.iSpa              = clb.buildings.bgSpa.iLevel;
      //mdStadionSurr.iSpaNew           = clb.buildings.iSpa[2];
      mdStadionSurr.iJouthInternat    = clb.buildings.bgJouthInternat.iLevel;
      //mdStadionSurr.iJouthInternatNew = clb.buildings.iJouthInternat[2];
      mdStadionSurr.iClubHouse        = clb.buildings.bgClubHouse.iLevel;
      //mdStadionSurr.iClubHouseNew     = clb.buildings.iClubHouse[2];
      mdStadionSurr.iClubMuseum       = clb.buildings.bgClubMuseum.iLevel;
      //mdStadionSurr.iClubMuseumNew    = clb.buildings.iClubMuseum[2];
      mdStadionSurr.iCarpark       = Math.Max(clb.stadium.iCarpark, clb.stadium.iCarparkNew);
      //mdStadionSurr.iCarparkNew    = clb.stadium.iCarparkNew;
      mdStadionSurr.iCounter       = Math.Max(clb.stadium.iTicketcounter, clb.stadium.iTicketcounterNew);
      //mdStadionSurr.iCounterNew    = clb.stadium.iTicketcounterNew;

      return View(mdStadionSurr);
    }

    [HttpPost]
    public JsonResult StadiumSurrGetBuildings()
    {
      CornerkickManager.Club clb = ckClub();
      if (clb == null) return Json(false, JsonRequestBehavior.AllowGet);

      CornerkickManager.User usr = ckUser();

      Models.StadiumSurroundingsModel.Building[] bdgsAll = new Models.StadiumSurroundingsModel.Building[8];
      for (byte iB = 0; iB < bdgsAll.Length; iB++) bdgsAll[iB] = new Models.StadiumSurroundingsModel.Building();

      Models.StadiumSurroundingsModel.Buildings buildings = new Models.StadiumSurroundingsModel.Buildings();
      buildings.ltBuildings     = new List<Models.StadiumSurroundingsModel.Building>();
      buildings.ltBuildingsFree = new List<Models.StadiumSurroundingsModel.Building>();

      int[] iCostDays = new int[2];

      byte iType = 0;
      bdgsAll[iType].iType = iType;
      bdgsAll[iType].sCategory = CornerkickManager.Stadium.sTrainingCourtsName;
      bdgsAll[iType].iLevel = clb.buildings.bgTrainingCourts.iLevel;
      bdgsAll[iType].sName = CornerkickManager.Stadium.sTrainingCourts[clb.buildings.bgTrainingCourts.iLevel];
      bdgsAll[iType].sNameNext = CornerkickManager.Stadium.sTrainingCourts[clb.buildings.bgTrainingCourts.iLevel + 1];
      if (clb.buildings.bgTrainingCourts.ctn != null && clb.buildings.bgTrainingCourts.ctn.iLevelNew > clb.buildings.bgTrainingCourts.iLevel) {
        bdgsAll[0].nDaysConstruct      = (int)clb.buildings.bgTrainingCourts.ctn.fDaysConstruct;
        bdgsAll[0].nDaysConstructTotal = CornerkickManager.Stadium.getCostDaysBuildTrainingCourts(clb)[1];
      } else {
        iCostDays = CornerkickManager.Stadium.getCostDaysBuildTrainingCourts(clb.buildings.bgTrainingCourts.iLevel + 1, clb.buildings.bgTrainingCourts.iLevel);
        bdgsAll[iType].sCostConstructNext = iCostDays[0].ToString("N0", getCi());
        bdgsAll[iType].nDaysConstructTotal = iCostDays[1];
        bdgsAll[iType].bDispoOk = MvcApplication.ckcore.fz.checkDispoLimit(iCostDays[0], clb);
      }
      if (clb.buildings.bgTrainingCourts.iLevel > 0 || (clb.buildings.bgTrainingCourts.ctn != null && clb.buildings.bgTrainingCourts.ctn.iLevelNew > 0)) buildings.ltBuildings    .Add(bdgsAll[iType]);
      else                                                                              buildings.ltBuildingsFree.Add(bdgsAll[iType]);

      iType++;
      bdgsAll[iType].iType = iType;
      bdgsAll[iType].sCategory = CornerkickManager.Stadium.sGymName;
      bdgsAll[iType].iLevel = clb.buildings.bgGym.iLevel;
      bdgsAll[iType].sName = CornerkickManager.Stadium.sGym[clb.buildings.bgGym.iLevel];
      bdgsAll[iType].sNameNext = CornerkickManager.Stadium.sGym[clb.buildings.bgGym.iLevel + 1];
      if (clb.buildings.bgGym.ctn != null && clb.buildings.bgGym.ctn.iLevelNew > clb.buildings.bgGym.iLevel) {
        bdgsAll[iType].nDaysConstruct = (int)clb.buildings.bgGym.ctn.fDaysConstruct;
        bdgsAll[iType].nDaysConstructTotal = CornerkickManager.Stadium.getCostDaysBuildGym(clb)[1];
      } else {
        iCostDays = CornerkickManager.Stadium.getCostDaysBuildGym(clb.buildings.bgGym.iLevel + 1, clb.buildings.bgGym.iLevel);
        bdgsAll[iType].sCostConstructNext = iCostDays[0].ToString("N0", getCi());
        bdgsAll[iType].nDaysConstructTotal = iCostDays[1];
        bdgsAll[iType].bDispoOk = MvcApplication.ckcore.fz.checkDispoLimit(iCostDays[0], clb);
      }
      if (clb.buildings.bgGym.iLevel > 0 || (clb.buildings.bgGym.ctn != null && clb.buildings.bgGym.ctn.iLevelNew > 0)) buildings.ltBuildings    .Add(bdgsAll[iType]);
      else                                                        buildings.ltBuildingsFree.Add(bdgsAll[iType]);

      iType++;
      bdgsAll[iType].iType = iType;
      bdgsAll[iType].sCategory = CornerkickManager.Stadium.sSpaName;
      bdgsAll[iType].iLevel = clb.buildings.bgSpa.iLevel;
      bdgsAll[iType].sName = CornerkickManager.Stadium.sSpa[clb.buildings.bgSpa.iLevel];
      bdgsAll[iType].sNameNext = CornerkickManager.Stadium.sSpa[clb.buildings.bgSpa.iLevel + 1];
      if (clb.buildings.bgSpa.ctn != null && clb.buildings.bgSpa.ctn.iLevelNew > clb.buildings.bgSpa.iLevel) {
        bdgsAll[iType].nDaysConstruct = (int)clb.buildings.bgSpa.ctn.fDaysConstruct;
        bdgsAll[iType].nDaysConstructTotal = CornerkickManager.Stadium.getCostDaysBuildSpa(clb)[1];
      } else {
        iCostDays = CornerkickManager.Stadium.getCostDaysBuildSpa(clb.buildings.bgSpa.iLevel + 1, clb.buildings.bgSpa.iLevel);
        bdgsAll[iType].sCostConstructNext = iCostDays[0].ToString("N0", getCi());
        bdgsAll[iType].nDaysConstructTotal = iCostDays[1];
        bdgsAll[iType].bDispoOk = MvcApplication.ckcore.fz.checkDispoLimit(iCostDays[0], clb);
      }
      if (clb.buildings.bgSpa.iLevel > 0 || (clb.buildings.bgSpa.ctn != null && clb.buildings.bgSpa.ctn.iLevelNew > 0)) buildings.ltBuildings    .Add(bdgsAll[iType]);
      else                                                        buildings.ltBuildingsFree.Add(bdgsAll[iType]);

      iType++;
      bdgsAll[iType].iType = iType;
      bdgsAll[iType].sCategory = CornerkickManager.Stadium.sJouthInternatName;
      bdgsAll[iType].iLevel = clb.buildings.bgJouthInternat.iLevel;
      bdgsAll[iType].sName = CornerkickManager.Stadium.sJouthInternat[clb.buildings.bgJouthInternat.iLevel];
      bdgsAll[iType].sNameNext = CornerkickManager.Stadium.sJouthInternat[clb.buildings.bgJouthInternat.iLevel + 1];
      if (clb.buildings.bgJouthInternat.ctn != null && clb.buildings.bgJouthInternat.ctn.iLevelNew > clb.buildings.bgJouthInternat.iLevel) {
        bdgsAll[iType].nDaysConstruct = (int)clb.buildings.bgJouthInternat.ctn.fDaysConstruct;
        bdgsAll[iType].nDaysConstructTotal = CornerkickManager.Stadium.getCostDaysBuildJouthInternat(clb)[1];
      } else {
        iCostDays = CornerkickManager.Stadium.getCostDaysBuildJouthInternat(clb.buildings.bgJouthInternat.iLevel + 1, clb.buildings.bgJouthInternat.iLevel);
        bdgsAll[iType].sCostConstructNext = iCostDays[0].ToString("N0", getCi());
        bdgsAll[iType].nDaysConstructTotal = iCostDays[1];
        bdgsAll[iType].bDispoOk = MvcApplication.ckcore.fz.checkDispoLimit(iCostDays[0], clb);
      }
      if (clb.buildings.bgJouthInternat.iLevel > 0 || (clb.buildings.bgJouthInternat.ctn != null && clb.buildings.bgJouthInternat.ctn.iLevelNew > 0)) buildings.ltBuildings    .Add(bdgsAll[iType]);
      else                                                                            buildings.ltBuildingsFree.Add(bdgsAll[iType]);

      iType++;
      bdgsAll[iType].iType = iType;
      bdgsAll[iType].sCategory = CornerkickManager.Stadium.sClubHouseName;
      bdgsAll[iType].iLevel = clb.buildings.bgClubHouse.iLevel;
      bdgsAll[iType].sName = CornerkickManager.Stadium.sClubHouse[clb.buildings.bgClubHouse.iLevel];
      bdgsAll[iType].sNameNext = CornerkickManager.Stadium.sClubHouse[clb.buildings.bgClubHouse.iLevel + 1];
      if (clb.buildings.bgClubHouse.ctn != null && clb.buildings.bgClubHouse.ctn.iLevelNew > clb.buildings.bgClubHouse.iLevel) {
        bdgsAll[iType].nDaysConstruct = (int)clb.buildings.bgClubHouse.ctn.fDaysConstruct;
        bdgsAll[iType].nDaysConstructTotal = CornerkickManager.Stadium.getCostDaysBuildClubHouse(clb)[1];
      } else {
        iCostDays = CornerkickManager.Stadium.getCostDaysBuildClubHouse(clb.buildings.bgClubHouse.iLevel + 1, clb.buildings.bgClubHouse.iLevel);
        bdgsAll[iType].sCostConstructNext = iCostDays[0].ToString("N0", getCi());
        bdgsAll[iType].nDaysConstructTotal = iCostDays[1];
        bdgsAll[iType].bDispoOk = MvcApplication.ckcore.fz.checkDispoLimit(iCostDays[0], clb);
      }
      if (clb.buildings.bgClubHouse.iLevel > 0 || (clb.buildings.bgClubHouse.ctn != null && clb.buildings.bgClubHouse.ctn.iLevelNew > 0)) buildings.ltBuildings    .Add(bdgsAll[iType]);
      else                                                                    buildings.ltBuildingsFree.Add(bdgsAll[iType]);

      iType++;
      bdgsAll[iType].iType = iType;
      bdgsAll[iType].sCategory = CornerkickManager.Stadium.sClubMuseumName;
      bdgsAll[iType].iLevel = clb.buildings.bgClubMuseum.iLevel;
      bdgsAll[iType].sName = CornerkickManager.Stadium.sClubMuseum[clb.buildings.bgClubMuseum.iLevel];
      bdgsAll[iType].sNameNext = CornerkickManager.Stadium.sClubMuseum[clb.buildings.bgClubMuseum.iLevel + 1];
      if (clb.buildings.bgClubMuseum.ctn != null && clb.buildings.bgClubMuseum.ctn.iLevelNew > clb.buildings.bgClubMuseum.iLevel) {
        bdgsAll[iType].nDaysConstruct = (int)clb.buildings.bgClubMuseum.ctn.fDaysConstruct;
        bdgsAll[iType].nDaysConstructTotal = CornerkickManager.Stadium.getCostDaysBuildClubMuseum(clb)[1];
      } else {
        iCostDays = CornerkickManager.Stadium.getCostDaysBuildClubMuseum(clb.buildings.bgClubMuseum.iLevel + 1, clb.buildings.bgClubMuseum.iLevel);
        bdgsAll[iType].sCostConstructNext = iCostDays[0].ToString("N0", getCi());
        bdgsAll[iType].nDaysConstructTotal = iCostDays[1];
        bdgsAll[iType].bDispoOk = MvcApplication.ckcore.fz.checkDispoLimit(iCostDays[0], clb);
      }
      if (clb.buildings.bgClubMuseum.iLevel > 0 || (clb.buildings.bgClubMuseum.ctn != null && clb.buildings.bgClubMuseum.ctn.iLevelNew > 0)) buildings.ltBuildings    .Add(bdgsAll[iType]);
      else                                                                      buildings.ltBuildingsFree.Add(bdgsAll[iType]);

      iType++;
      iCostDays = CornerkickManager.Stadium.getCostDaysContructCarpark(clb.stadium.iCarparkNew, clb.stadium.iCarpark, usr);
      bdgsAll[iType].sCategory = CornerkickManager.Stadium.sCarparkName;
      bdgsAll[iType].iType = iType;
      bdgsAll[iType].iLevel = clb.stadium.iCarpark;
      bdgsAll[iType].sName = clb.stadium.iCarpark.ToString();
      bdgsAll[iType].sNameNext = clb.stadium.iCarparkNew.ToString();
      bdgsAll[iType].nDaysConstructTotal = iCostDays[1];
      if (clb.stadium.iCarparkNew > clb.stadium.iCarpark) {
        bdgsAll[iType].nDaysConstruct = clb.stadium.iCarparkDaysConstruct;
      } else {
        bdgsAll[iType].sCostConstructNext = iCostDays[0].ToString("N0", getCi());
        bdgsAll[iType].bDispoOk = MvcApplication.ckcore.fz.checkDispoLimit(iCostDays[0], clb);
      }
      if (clb.stadium.iCarpark > 0 || clb.stadium.iCarparkNew > 0) buildings.ltBuildings    .Add(bdgsAll[iType]);
      else                                                         buildings.ltBuildingsFree.Add(bdgsAll[iType]);

      iType++;
      iCostDays = CornerkickManager.Stadium.getCostDaysContructTicketcounter(clb.stadium.iTicketcounterNew, clb.stadium.iTicketcounter, usr);
      bdgsAll[iType].sCategory = CornerkickManager.Stadium.sTicketcounterName;
      bdgsAll[iType].iType = iType;
      bdgsAll[iType].iLevel = clb.stadium.iTicketcounter;
      bdgsAll[iType].sName = clb.stadium.iTicketcounter.ToString();
      bdgsAll[iType].sNameNext = clb.stadium.iTicketcounterNew.ToString();
      bdgsAll[iType].nDaysConstructTotal = iCostDays[1];
      if (clb.stadium.iTicketcounterNew > clb.stadium.iTicketcounter) {
        bdgsAll[iType].nDaysConstruct = clb.stadium.iTicketcounterDaysConstruct;
      } else {
        bdgsAll[iType].sCostConstructNext = iCostDays[0].ToString("N0", getCi());
        bdgsAll[iType].bDispoOk = MvcApplication.ckcore.fz.checkDispoLimit(iCostDays[0], clb);
      }
      if (clb.stadium.iTicketcounter > 0 || clb.stadium.iTicketcounterNew > 0) buildings.ltBuildings    .Add(bdgsAll[iType]);
      else                                                                     buildings.ltBuildingsFree.Add(bdgsAll[iType]);

      buildings.iGround = (byte)Math.Max(clb.buildings.iGround, buildings.ltBuildings.Count);
      buildings.sCostBuyGround = CornerkickManager.Stadium.getCostBuyGround(clb.buildings.iGround).ToString("N0", getCi());

      return Json(buildings, JsonRequestBehavior.AllowGet);
    }

    [HttpPost]
    public JsonResult StadiumSurrGetCarparkTicketCounter(int iType, int iNew, int iCurrent)
    {
      CornerkickManager.Club clb = ckClub();
      if (clb == null) return Json(false, JsonRequestBehavior.AllowGet);

      CornerkickManager.User usr = ckUser();
      if (usr == null) return Json(false, JsonRequestBehavior.AllowGet);

      Models.StadiumSurroundingsModel.Building bdg = new Models.StadiumSurroundingsModel.Building();

      bdg.iType = (byte)iType;
      bdg.iLevel = (iCurrent + iNew) - 1;
      bdg.sNameNext = (iCurrent + iNew).ToString();

      int[] iCostDays = new int[2];
      if      (iType == 6) iCostDays = CornerkickManager.Stadium.getCostDaysContructCarpark      (clb.stadium.iCarpark       + iNew, clb.stadium.iCarpark,       usr);
      else if (iType == 7) iCostDays = CornerkickManager.Stadium.getCostDaysContructTicketcounter(clb.stadium.iTicketcounter + iNew, clb.stadium.iTicketcounter, usr);

      bdg.sCostConstructNext  = iCostDays[0].ToString("N0", getCi());
      bdg.nDaysConstructTotal = iCostDays[1];

      return Json(bdg, JsonRequestBehavior.AllowGet);
    }

    [HttpPost]
    public JsonResult StadiumBuildBuilding(int iType, int iLevel)
    {
      CornerkickManager.Club clb = ckClub();
      if (clb == null) return Json(false, JsonRequestBehavior.AllowGet);

      CornerkickManager.User usr = ckUser();

      string[] sConstructionNames = new string[] {
        CornerkickManager.Stadium.sTrainingCourtsName,
        CornerkickManager.Stadium.sGymName,
        CornerkickManager.Stadium.sSpaName,
        CornerkickManager.Stadium.sJouthInternatName,
        CornerkickManager.Stadium.sClubHouseName,
        CornerkickManager.Stadium.sClubMuseumName,
        CornerkickManager.Stadium.sCarparkName,
        CornerkickManager.Stadium.sTicketcounterName
      };

      if (iType < 6) {
        CornerkickManager.UI.doConstruction(clb, iType, (byte)iLevel, MvcApplication.ckcore.dtDatum, sConstructionNames[iType]);
      } else if (iType == 6) { // Carpark
        if (clb.stadium.iCarparkNew != iLevel) {
          clb.stadium.iCarparkNew = iLevel;
          int[] iCostDaysCp = CornerkickManager.Stadium.getCostDaysContructCarpark(iLevel, clb.stadium.iCarpark, usr);
          clb.stadium.iCarparkDaysConstruct = iCostDaysCp[1];

          CornerkickManager.Finance.doTransaction(ref clb, MvcApplication.ckcore.dtDatum, -iCostDaysCp[0], "Bau " + sConstructionNames[iType], CornerkickManager.Finance.iTransferralTypePayStadiumSurr);
        }
      } else if (iType == 7) { // Ticketcounter
        if (clb.stadium.iTicketcounterNew != iLevel) {
          clb.stadium.iTicketcounterNew = (byte)iLevel;
          int[] iCostDaysTc = CornerkickManager.Stadium.getCostDaysContructTicketcounter(iLevel, clb.stadium.iTicketcounter, usr);
          clb.stadium.iTicketcounterDaysConstruct = iCostDaysTc[1];

          CornerkickManager.Finance.doTransaction(ref clb, MvcApplication.ckcore.dtDatum, -iCostDaysTc[0], "Bau " + sConstructionNames[iType], CornerkickManager.Finance.iTransferralTypePayStadiumSurr);
        }
      }

      return Json("Der Bau des " + sConstructionNames[iType] + "s wurde in Auftrag gegeben", JsonRequestBehavior.AllowGet);
    }
    
    [HttpPost]
    public JsonResult StadiumSurrBuyGround()
    {
      CornerkickManager.Club clb = ckClub();
      if (clb == null) return Json(false, JsonRequestBehavior.AllowGet);

      int iCost = CornerkickManager.Stadium.getCostBuyGround(clb.buildings.iGround);
      clb.buildings.iGround++;
      CornerkickManager.Finance.doTransaction(ref clb, MvcApplication.ckcore.dtDatum, -iCost, "Grundstückskauf", CornerkickManager.Finance.iTransferralTypePayStadiumSurr);

      return Json("Das Grundstück wurde erworben", JsonRequestBehavior.AllowGet);
    }

    //////////////////////////////////////////////////////////////////////////
    /// <summary>
    /// Personal
    /// </summary>
    /// <param name="Personal"></param>
    /// <returns></returns>
    //////////////////////////////////////////////////////////////////////////
    [Authorize]
    public ActionResult Personal(Models.PersonalModel personal)
    {
      CornerkickManager.Club clb = ckClub();
      if (clb == null) return View(personal);

      personal.staff = clb.staff;

      personal.ltDdlPersonalCoachCo       = new List<SelectListItem>();
      personal.ltDdlPersonalCoachCondi    = new List<SelectListItem>();
      personal.ltDdlPersonalMasseur       = new List<SelectListItem>();
      personal.ltDdlPersonalMental        = new List<SelectListItem>();
      personal.ltDdlPersonalMed           = new List<SelectListItem>();
      personal.ltDdlPersonalJouthCoach    = new List<SelectListItem>();
      personal.ltDdlPersonalJouthScouting = new List<SelectListItem>();
      personal.ltDdlPersonalKibitzer      = new List<SelectListItem>();

      for (byte i = 7; i > 0; i--) personal.ltDdlPersonalCoachCo.Add(new SelectListItem{ Text = "Level: " + i.ToString() + " - " + CornerkickManager.Finance.iCostCoachCo[i].ToString("N0", getCi()) + " €/Monat", Value = i.ToString() });
      personal.ltDdlPersonalCoachCo.Add(new SelectListItem { Text = "-", Value = "0" });

      for (byte i = 7; i > 0; i--) personal.ltDdlPersonalCoachCondi.Add(new SelectListItem { Text = "Level: " + i.ToString() + " - " + CornerkickManager.Finance.iCostCoachCondi[i].ToString("N0", getCi()) + " €/Monat", Value = i.ToString() });
      personal.ltDdlPersonalCoachCondi.Add(new SelectListItem { Text = "-", Value = "0" });

      for (byte i = 7; i > 0; i--) personal.ltDdlPersonalMasseur.Add(new SelectListItem { Text = "Level: " + i.ToString() + " - " + CornerkickManager.Finance.iCostMasseur[i].ToString("N0", getCi()) + " €/Monat", Value = i.ToString() });
      personal.ltDdlPersonalMasseur.Add(new SelectListItem { Text = "-", Value = "0" });

      for (byte i = 7; i > 0; i--) personal.ltDdlPersonalMental.Add(new SelectListItem { Text = "Level: " + i.ToString() + " - " + CornerkickManager.Finance.iCostMental[i].ToString("N0", getCi()) + " €/Monat", Value = i.ToString() });
      personal.ltDdlPersonalMental.Add(new SelectListItem { Text = "-", Value = "0" });

      for (byte i = 7; i > 0; i--) personal.ltDdlPersonalMed.Add(new SelectListItem { Text = "Level: " + i.ToString() + " - " + CornerkickManager.Finance.iCostMed[i].ToString("N0", getCi()) + " €/Monat", Value = i.ToString() });
      personal.ltDdlPersonalMed.Add(new SelectListItem { Text = "-", Value = "0" });

      for (byte i = 7; i > 0; i--) personal.ltDdlPersonalJouthCoach.Add(new SelectListItem { Text = "Level: " + i.ToString() + " - " + CornerkickManager.Finance.iCostJouthCoach[i].ToString("N0", getCi()) + " €/Monat", Value = i.ToString() });
      personal.ltDdlPersonalJouthCoach.Add(new SelectListItem { Text = "-", Value = "0" });

      for (byte i = 7; i > 0; i--) personal.ltDdlPersonalJouthScouting.Add(new SelectListItem { Text = "Level: " + i.ToString() + " - " + CornerkickManager.Finance.iCostJouthScouting[i].ToString("N0", getCi()) + " €/Monat", Value = i.ToString() });
      personal.ltDdlPersonalJouthScouting.Add(new SelectListItem { Text = "-", Value = "0" });

      personal.ltDdlPersonalKibitzer.Add(new SelectListItem { Text = "Level: 4 - " + CornerkickManager.Finance.iCostKibitzer[4].ToString("N0", getCi()) + " €/Monat", Value = "4" });
      personal.ltDdlPersonalKibitzer.Add(new SelectListItem { Text = "Level: 3 - " + CornerkickManager.Finance.iCostKibitzer[3].ToString("N0", getCi()) + " €/Monat", Value = "3" });
      personal.ltDdlPersonalKibitzer.Add(new SelectListItem { Text = "Level: 2 - " + CornerkickManager.Finance.iCostKibitzer[2].ToString("N0", getCi()) + " €/Monat", Value = "2" });
      personal.ltDdlPersonalKibitzer.Add(new SelectListItem { Text = "Level: 1 - " + CornerkickManager.Finance.iCostKibitzer[1].ToString("N0", getCi()) + " €/Monat", Value = "1" });
      personal.ltDdlPersonalKibitzer.Add(new SelectListItem { Text = "-", Value = "0" });

      return View(personal);
    }

    [HttpPost]
    public JsonResult PersonalCheckCost(int[] iLevel)
    {
      CornerkickManager.Club clb = new CornerkickManager.Club();

      clb.staff.iCoTrainer     = (byte)iLevel[0];
      clb.staff.iCondiTrainer  = (byte)iLevel[1];
      clb.staff.iPhysio        = (byte)iLevel[2];
      clb.staff.iMentalTrainer = (byte)iLevel[3];
      clb.staff.iDoctor        = (byte)iLevel[4];
      clb.staff.iJouthTrainer  = (byte)iLevel[5];
      clb.staff.iJouthScouting = (byte)iLevel[6];
      clb.staff.iKibitzer      = (byte)iLevel[7];

      int iKosten = (int)(MvcApplication.ckcore.tl.getStuffSalary(clb) / 12f);
      string sKosten = iKosten.ToString("N0", getCi());

      sKosten = "Kosten: " + sKosten + " €/Monat";

      // Add personal pay-off costs
      int iPayOff = getPersonalPayOff(iLevel);
      if (iPayOff > 0) sKosten += ", Abfindungen: " + iPayOff.ToString("N0", getCi()) + " €";

      return Json(sKosten, JsonRequestBehavior.AllowGet);
    }

    private int getPersonalPayOff(int[] iLevelNew)
    {
      int iPayOff = 0;
      for (byte iP = 0; iP < 8; iP++) iPayOff += getPersonalPayOff(iP, iLevelNew[iP]);

      return iPayOff;
    }

    private int getPersonalPayOff(int iPersonal, int iLevelNew)
    {
      CornerkickManager.Club clb = ckClub();
      if (clb == null) return 0;

      int iMoney = 0;
      int iMonthDiff = (MvcApplication.ckcore.dtSeasonEnd.Month - MvcApplication.ckcore.dtDatum.Month) + (12 * (MvcApplication.ckcore.dtSeasonEnd.Year - MvcApplication.ckcore.dtDatum.Year));
      if      (iPersonal == 0 && iLevelNew != clb.staff.iCoTrainer    ) iMoney = iMonthDiff * (CornerkickManager.Finance.iCostCoachCo      [clb.staff.iCoTrainer    ] / 2);
      else if (iPersonal == 1 && iLevelNew != clb.staff.iCondiTrainer ) iMoney = iMonthDiff * (CornerkickManager.Finance.iCostCoachCondi   [clb.staff.iCondiTrainer ] / 2);
      else if (iPersonal == 2 && iLevelNew != clb.staff.iPhysio       ) iMoney = iMonthDiff * (CornerkickManager.Finance.iCostMasseur      [clb.staff.iPhysio       ] / 2);
      else if (iPersonal == 3 && iLevelNew != clb.staff.iMentalTrainer) iMoney = iMonthDiff * (CornerkickManager.Finance.iCostMental       [clb.staff.iMentalTrainer] / 2);
      else if (iPersonal == 4 && iLevelNew != clb.staff.iDoctor       ) iMoney = iMonthDiff * (CornerkickManager.Finance.iCostMed          [clb.staff.iDoctor       ] / 2);
      else if (iPersonal == 5 && iLevelNew != clb.staff.iJouthTrainer ) iMoney = iMonthDiff * (CornerkickManager.Finance.iCostJouthCoach   [clb.staff.iJouthTrainer ] / 2);
      else if (iPersonal == 6 && iLevelNew != clb.staff.iJouthScouting) iMoney = iMonthDiff * (CornerkickManager.Finance.iCostJouthScouting[clb.staff.iJouthScouting] / 2);
      else if (iPersonal == 7 && iLevelNew != clb.staff.iKibitzer     ) iMoney = iMonthDiff * (CornerkickManager.Finance.iCostKibitzer     [clb.staff.iKibitzer     ] / 2);

      return iMoney;
    }

    [HttpPost]
    public JsonResult PersonalHire(int[] iLevel)
    {
      CornerkickManager.Club clb = ckClub();
      if (clb == null) return Json(false, JsonRequestBehavior.AllowGet);

      // First: Pay personal pay-off costs
      int iPayOff = getPersonalPayOff(iLevel);
      CornerkickManager.Finance.doTransaction(ref clb, MvcApplication.ckcore.dtDatum, -iPayOff, "Abfindungen", CornerkickManager.Finance.iTransferralTypePaySalaryStaff);

      // Then, hire new personal
      clb.staff.iCoTrainer     = (byte)iLevel[0];
      clb.staff.iCondiTrainer  = (byte)iLevel[1];
      clb.staff.iPhysio        = (byte)iLevel[2];
      clb.staff.iMentalTrainer = (byte)iLevel[3];
      clb.staff.iDoctor        = (byte)iLevel[4];
      clb.staff.iJouthTrainer  = (byte)iLevel[5];
      clb.staff.iJouthScouting = (byte)iLevel[6];
      clb.staff.iKibitzer      = (byte)iLevel[7];

      return Json("Neues Personal eingestellt!", JsonRequestBehavior.AllowGet);
    }

    //////////////////////////////////////////////////////////////////////////
    /// <summary>
    /// ClubDetails
    /// </summary>
    /// <param name="Club Details"></param>
    /// <returns></returns>
    //////////////////////////////////////////////////////////////////////////
    public ActionResult ClubDetails(Models.ClubModel mdClub, int iClub, HttpPostedFileBase file = null)
    {
      if (file != null) AccountController.uploadFileAsync(file, "emblems", iClub);

      if (iClub >= 0 && iClub < MvcApplication.ckcore.ltClubs.Count) {
        mdClub.club = MvcApplication.ckcore.ltClubs[iClub];

        mdClub.sEmblem = getClubEmblem(mdClub.club, "height: 100%; width: 100%; object-fit: contain");

        // Check if own club and emblem exist
        if (ckClub() == mdClub.club) {
          mdClub.bEmblemEditable = mdClub.sEmblem.StartsWith("<img src=\"/Content/Uploads/emblems/0.png");
        }

        mdClub.sAttrFc = mdClub.club.getAttractionFactor(MvcApplication.ckcore.iSeason).ToString("0.0");
      }

      return View(mdClub);
    }

    public ContentResult ClubDetailsGetPlaceHistory(int iClubId)
    {
      if (iClubId < 0) return Content(null, "application/json");

      CornerkickManager.Club clb = MvcApplication.ckcore.ltClubs[iClubId];

      List<Models.DataPointGeneral> ltDataPoints = new List<Models.DataPointGeneral>();

      // Get league
      CornerkickManager.Cup league = MvcApplication.ckcore.tl.getCup(1, clb.iLand, clb.iDivision);

      // Add past league places
      CornerkickManager.Main.Success sucLge = CornerkickManager.Tool.getSuccess(clb, league);
      if (sucLge != null) {
        for (int iS = 0; iS < sucLge.ltCupPlace.Count; iS++) {
          ltDataPoints.Add(new Models.DataPointGeneral(sucLge.ltCupPlace[iS][1], sucLge.ltCupPlace[iS][0]));
        }
      }

      // Add current league place
      ltDataPoints.Add(new Models.DataPointGeneral(MvcApplication.ckcore.iSeason, CornerkickManager.Tool.getLeaguePlace(league, clb)));

      JsonSerializerSettings _jsonSetting = new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore };

      return Content(JsonConvert.SerializeObject(ltDataPoints, _jsonSetting), "application/json");
    }

    private string getClubEmblem(int iClubId, string sStyle = "", bool bTiny = false)
    {
      CornerkickManager.Club clb = null;
      if (iClubId >= 0 && iClubId < MvcApplication.ckcore.ltClubs.Count) clb = MvcApplication.ckcore.ltClubs[iClubId];

      return getClubEmblem(clb, sStyle: sStyle, bTiny: bTiny);
    }
    private static string getClubEmblem(CornerkickManager.Club clb, string sStyle = "", bool bTiny = false)
    {
      string sEmblem = "<img src=\"/Content/Uploads/emblems/";

      if (!string.IsNullOrEmpty(sStyle)) sStyle = " style=\"" + sStyle + "\"";

      if (clb == null) return sEmblem + "0.png\" alt=\"Wappen\" " + sStyle + " title=\"vereinslos\"/>";

      string sEmblemFile = "";
      if (clb.bNation) {
        sEmblemFile = getClubEmblemPath(clb.iLand, bNation: clb.bNation, bTiny: bTiny);
        if (bTiny && string.IsNullOrEmpty(sEmblemFile)) sEmblemFile = getClubEmblemPath(clb.iLand, bNation: clb.bNation, bTiny: false);
      } else {
        sEmblemFile = getClubEmblemPath(clb.iId,   bNation: clb.bNation, bTiny: bTiny);
        if (bTiny && string.IsNullOrEmpty(sEmblemFile)) sEmblemFile = getClubEmblemPath(clb.iId,   bNation: clb.bNation, bTiny: false);
      }

      if (clb.bNation) {
        sEmblem = "<img src=\"/Content/Icons/flags/";
        if (System.IO.File.Exists(sEmblemFile)) sEmblem += CornerkickManager.Main.sLandShort[clb.iLand];
        else                                    sEmblem += "0";
        bTiny = false;
      } else {
        if (System.IO.File.Exists(sEmblemFile)) {
          sEmblem += clb.iId.ToString();
        } else {
          sEmblem += "0";
          bTiny = false;
        }
      }

      if (bTiny) sEmblem += "_tiny";

      sEmblem += ".png\" alt=\"Wappen\"" + sStyle + " title=\"" + clb.sName + "\"/>";

      return sEmblem;
    }
    private static string getClubEmblemPath(int iClubId, bool bNation = false, bool bTiny = false)
    {
      string sTiny = "";
      if (bTiny) sTiny = "_tiny";
#if DEBUG
      string sEmblemFile = System.IO.Path.Combine(MvcApplication.getHomeDir(), "Content", "Uploads", "emblems", iClubId.ToString() + sTiny + ".png");
      if (bNation) sEmblemFile = System.IO.Path.Combine(MvcApplication.getHomeDir(), "Content", "Icons", "flags", CornerkickManager.Main.sLandShort[iClubId] + ".png");
#else
      string sEmblemFile = System.IO.Path.Combine(System.Web.HttpContext.Current.Server.MapPath("~"), "Content", "Uploads", "emblems", iClubId.ToString() + sTiny + ".png");
      if (bNation) sEmblemFile = System.IO.Path.Combine(System.Web.HttpContext.Current.Server.MapPath("~"), "Content", "Icons", "flags", CornerkickManager.Main.sLandShort[iClubId] + ".png");
#endif

      if (!System.IO.File.Exists(sEmblemFile)) return null;

      return sEmblemFile;
    }

    public static string getStringRecordGame(System.Security.Principal.IPrincipal User, int iGameType, sbyte iWDD, byte iHA)
    {
      CornerkickManager.Club clb = ckClubStatic(User);

      CornerkickGame.Game.Data gdRecord = MvcApplication.ckcore.ui.getRecordGame(clb, iGameType, iWDD, iHA);

      if (gdRecord != null) {
        string sTeamH = gdRecord.team[0].sTeam;
        string sTeamA = gdRecord.team[1].sTeam;

        if (string.IsNullOrEmpty(sTeamH) && gdRecord.team[0].iTeamId >= 0) sTeamH = MvcApplication.ckcore.ltClubs[gdRecord.team[0].iTeamId].sName;
        if (string.IsNullOrEmpty(sTeamA) && gdRecord.team[1].iTeamId >= 0) sTeamA = MvcApplication.ckcore.ltClubs[gdRecord.team[1].iTeamId].sName;

        string sTeamOpp = sTeamA;
        if (clb.iId == gdRecord.team[1].iTeamId) sTeamOpp = sTeamH;

        return gdRecord.team[0].iGoals.ToString() + ":" + gdRecord.team[1].iGoals.ToString() + " vs. " + sTeamOpp + ", " + gdRecord.dt.ToString("d", getCiStatic(User));
      }

      return "-";
    }

    public ContentResult ClubGetAttrFtr(int iClubId)
    {
      if (iClubId < 0) return Content(null, "application/json");

      CornerkickManager.Club clb = MvcApplication.ckcore.ltClubs[iClubId];
      if (clb == null) return Content(null, "application/json");

      int[] iLtCupIds = new int[] { 1, 2, 3, 4, 5 };
      List<Models.DataPointGeneral>[] ltDataPoints = new List<Models.DataPointGeneral>[iLtCupIds.Length];

      for (int iSuc = 0; iSuc < clb.ltSuccess.Count; iSuc++) {
        CornerkickManager.Main.Success suc = CornerkickManager.Tool.getSuccess(clb, MvcApplication.ckcore.tl.getCup(iLtCupIds[iSuc]));
        if (suc == null) continue;

        ltDataPoints[iSuc] = new List<Models.DataPointGeneral>();

        for (int iS = 1; iS < MvcApplication.ckcore.iSeason; iS++) {
          List<float> ltAttractionFactorCup = new List<float>();
          float fAttrF = CornerkickManager.Club.getAttractionFactor(suc, MvcApplication.ckcore.iSeason, iSeasonSelected: iS);

          if (fAttrF > 0) {
            string sCupPlace = "";
            for (int iCP = 0; iCP < suc.ltCupPlace.Count; iCP++) {
              if (suc.ltCupPlace[iCP][1] == iS) {
                sCupPlace = " (" + suc.ltCupPlace[iCP][0].ToString() + ". Platz)";
                break;
              }
            }

            ltDataPoints[iSuc].Add(new Models.DataPointGeneral(iS, fAttrF, z: sCupPlace));
          }
        }
      }

      JsonSerializerSettings _jsonSetting = new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore };

      return Content(JsonConvert.SerializeObject(ltDataPoints, _jsonSetting), "application/json");
    }

    //////////////////////////////////////////////////////////////////////////
    /// <summary>
    /// League
    /// </summary>
    /// <param name="league"></param>
    /// <returns></returns>
    //////////////////////////////////////////////////////////////////////////
    //[Authorize]
    static CornerkickManager.Cup cupGlobal = null;
    static int iSeasonGlobal = 0;
    public ActionResult League(Models.LeagueModels mlLeague)
    {
      CornerkickManager.Club clb = ckClub();
      if (clb != null) {
        mlLeague.iClubId = clb.iId;
        mlLeague.iLand   = clb.iLand;
        mlLeague.iSpKl   = clb.iDivision;
      }

      iSeasonGlobal = MvcApplication.ckcore.iSeason;

      CornerkickManager.Cup league = MvcApplication.ckcore.tl.getCup(1, mlLeague.iLand, mlLeague.iSpKl);
      if (league == null) return View(mlLeague);

      int iMd = CornerkickManager.Tool.getMatchday(league, MvcApplication.ckcore.dtDatum);
      mlLeague.league = league;
      mlLeague.ltTbl  = CornerkickManager.Tool.getLeagueTable(league, iMd - 1, 0);
      mlLeague.iLeagueSize = MvcApplication.ckcore.tl.getCupParticipants(league, iMd).Count;

      // Add lands to dropdown list
      foreach (int iLand in MvcApplication.iNations) {
        mlLeague.ddlLand.Add(new SelectListItem { Text = CornerkickManager.Main.sLand[iLand], Value = iLand.ToString() });
      }

      mlLeague.ddlSeason = getDdlSeason();
      mlLeague.iSeason = MvcApplication.ckcore.iSeason;

      return View(mlLeague);
    }

    private List<SelectListItem> getDdlSeason()
    {
      List<SelectListItem> ddlSeason = new List<SelectListItem>();

      for (int iS = 1; iS <= MvcApplication.ckcore.iSeason; iS++) {
        ddlSeason.Add(new SelectListItem { Text = iS.ToString(), Value = iS.ToString() });
      }

      return ddlSeason;
    }

    public JsonResult getDdlMatchdays(int iSeason, int iLand, byte iDivision)
    {
      CornerkickManager.Cup league = MvcApplication.ckcore.tl.getCup(1, iLand, iDivision);

      string[] ltMd = new string[league.getMatchdaysTotal()];
      // Spieltage zu Dropdown Menü hinzufügen
      for (int iMd = 0; iMd < ltMd.Length; iMd++) {
        ltMd[iMd] = (iMd + 1).ToString();
      }

      return Json(ltMd, JsonRequestBehavior.AllowGet);
    }

    public JsonResult LeagueGetMatchday(int iSeason, int iLand, byte iDivision)
    {
      int iMd = 0;

      if (iSeason < MvcApplication.ckcore.iSeason) { // Past seasons
        iMd = MvcApplication.ckcore.tl.getCup(1, iLand, iDivision).getMatchdaysTotal();
      } else { // Current seasons
        // Get current matchday
        iMd = MvcApplication.ckcore.tl.getMatchday(iLand, iDivision, MvcApplication.ckcore.dtDatum, 1);

        // Increment matchday match is if today or tomorrow
        CornerkickManager.Club clb = ckClub();
        if (clb != null) {
          CornerkickGame.Game.Data gdNext = MvcApplication.ckcore.tl.getNextGame(clb, MvcApplication.ckcore.dtDatum, iGameType: 1);
          if (gdNext != null && (gdNext.dt.Date - MvcApplication.ckcore.dtDatum.Date).Days < 2) iMd++;
        }

        // Limit to 1
        iMd = Math.Max(iMd, 1);
      }

      return Json(iMd, JsonRequestBehavior.AllowGet);
    }

    internal static CornerkickManager.Cup getCup(int iSeason, int iType, int iLand = -1, int iDivision = -1)
    {
      CornerkickManager.Cup cup = null;

      if (iSeason <= 0) iSeason = MvcApplication.ckcore.iSeason;

      if (iSeason < MvcApplication.ckcore.iSeason) { // Past seasons
        if (iSeason == iSeasonGlobal && cupGlobal != null) {
          if (cupGlobal.iId == iType && (iLand < 0 || cupGlobal.iId2 == iLand)) return cupGlobal;
        }

        string sFileLoad = System.IO.Path.Combine(MvcApplication.getHomeDir(), "archive");
        List<CornerkickManager.Cup> ltCupsTmp = MvcApplication.ckcore.io.readCup(sFileLoad, iSeason);

        foreach (CornerkickManager.Cup cp in ltCupsTmp) {
          if (cp.iId == iType && (iLand < 0 || cp.iId2 == iLand) && (iDivision < 0 || cp.iId3 == iDivision)) {
            cup = cp;
            cupGlobal = cp;
            iSeasonGlobal = iSeason;
            break;
          }
        }
      } else { // Current season
        cup = MvcApplication.ckcore.tl.getCup(iType, iLand, iDivision);
      }

      return cup;
    }

    public JsonResult setLeague(Models.LeagueModels mlLeague, int iSeason, int iLand, byte iDivision, int iMatchday, byte iHA)
    {
      CornerkickManager.Club clb = ckClub();
      CornerkickManager.Cup league = getCup(iSeason, 1, iLand, iDivision);

      string sBox = getTable(league, iMatchday, -1, iHA, clb, 1, 4, 8, -1);

      return Json(sBox, JsonRequestBehavior.AllowGet);
    }

    private string getTable(CornerkickManager.Cup cup, int iMatchday, sbyte iGroup = -1, byte iHA = 0, CornerkickManager.Club clubUser = null, int iColor1 = 0, int iColor2 = 0, int iColor3 = 0, int iColor4 = 0)
    {
      List<CornerkickManager.Tool.TableItem> ltTblLast = CornerkickManager.Tool.getLeagueTable(cup, iMatchday - 1, iHA, iGroup);
      List<CornerkickManager.Tool.TableItem> ltTbl     = CornerkickManager.Tool.getLeagueTable(cup, iMatchday,     iHA, iGroup);

      string sBox = "";
      for (var i = 0; i < ltTbl.Count; i++) {
        CornerkickManager.Tool.TableItem tbpl = ltTbl[i];
        int iGames = tbpl.iGUV[0] + tbpl.iGUV[1] + tbpl.iGUV[2];
        int iDiff = tbpl.iGoals - tbpl.iGoalsOpp;

        string sStyle = "";
        if (clubUser != null && tbpl.iId == clubUser.iId) sStyle = "style=\"font-weight:bold\" ";

        string sBgColor = "white";
        if      (iColor1 > 0 && i <  iColor1) sBgColor = "#ffffcc";
        else if (iColor2 > 0 && i <  iColor2) sBgColor = "#ccffcc";
        else if (iColor3 > 0 && i <  iColor3) sBgColor = "#cce5ff";
        else if (iColor4 > 0 && i <  iColor4) sBgColor = "#ffcccc";
        else if (iColor1 < 0 && i >= ltTbl.Count + iColor1) sBgColor = "#ffffcc";
        else if (iColor2 < 0 && i >= ltTbl.Count + iColor2) sBgColor = "#ccffcc";
        else if (iColor3 < 0 && i >= ltTbl.Count + iColor3) sBgColor = "#cce5ff";
        else if (iColor4 < 0 && i >= ltTbl.Count + iColor4) sBgColor = "#ffcccc";

        var k = i + 1;

        string sPlaceLast = "-";
        string sColor = "black";
        for (var iLast = 0; iLast < ltTblLast.Count; iLast++) {
          if (ltTblLast[iLast].iId == tbpl.iId) {
            if (i != iLast) {
              sPlaceLast = (iLast + 1).ToString();
              if (i > iLast) sColor = "red";
              else sColor = "green";
            }
            break;
          }
        }

        // Create tiny emblem file
        if (string.IsNullOrEmpty(getClubEmblemPath(tbpl.iId, bNation: false, bTiny: true))) {
          string sEmblemFile = getClubEmblemPath(tbpl.iId, bNation: false, bTiny: false);

          if (!string.IsNullOrEmpty(sEmblemFile)) {
            resizeImage(sEmblemFile, 24, "_tiny");
          }
        }

        string sEmblem = getClubEmblem(tbpl.iId, sStyle: "width: 12px", bTiny: true);

        sBox += "<tr " + sStyle + "\">";
        sBox += "<td class=\"first\" bgcolor=\"" + sBgColor + "\" align=\"center\"><b>" + k + "</b></td>";
        sBox += "<td style=\"color: " + sColor + "\" align=\"center\"> " + sPlaceLast + "</td>";
        sBox += "<td>" + sEmblem + "</td>";
        sBox += "<td><a href=\"/Member/ClubDetails?iClub=" + tbpl.iId.ToString() + "\" target=\"\">" + tbpl.sName + "</a></td>";
        sBox += "<td>&nbsp;</td>";
        sBox += "<td align=\"right\">" + iGames.ToString() + "</td>";
        sBox += "<td>&nbsp;</td>";
        sBox += "<td align=\"right\">" + tbpl.iGUV[0].ToString() + "</td>";
        sBox += "<td align=\"right\">" + tbpl.iGUV[1].ToString() + "</td>";
        sBox += "<td align=\"right\">" + tbpl.iGUV[2].ToString() + "</td>";
        sBox += "<td>&nbsp;</td>";
        sBox += "<td align=\"center\">" + tbpl.iGoals.ToString() + ":" + tbpl.iGoalsOpp.ToString() + "</td>";
        sBox += "<td align=\"right\">" + iDiff.ToString() + "</td>";
        sBox += "<td>&nbsp;</td>";
        sBox += "<td align=\"right\">" + tbpl.iPoints.ToString() + "</td>";
        sBox += "</tr>";
      }

      return sBox;
    }

    public JsonResult setLeagueTeams(int iSeason, int iLand, byte iDivision, int iMatchday)
    {
      if (iMatchday < 1) return Json("", JsonRequestBehavior.AllowGet);

      CornerkickManager.Club clbUser = ckClub();

      CornerkickManager.Cup league = getCup(iSeason, 1, iLand, iDivision);

      CornerkickManager.Cup.Matchday md = new CornerkickManager.Cup.Matchday();
      md.ltGameData = new List<CornerkickGame.Game.Data>();

      string sBox = "";
      foreach (CornerkickGame.Game.Data gd in league.ltMatchdays[iMatchday - 1].ltGameData) {
        string sClubNameH = "-";
        string sClubNameA = "-";
        int iIdH = gd.team[0].iTeamId;
        int iIdA = gd.team[1].iTeamId;
        if (gd.team[0].iTeamId >= 0) sClubNameH = MvcApplication.ckcore.ltClubs[iIdH].sName;
        if (gd.team[1].iTeamId >= 0) sClubNameA = MvcApplication.ckcore.ltClubs[iIdA].sName;

        string sStyle = "";
        if (clbUser != null && (iIdH == clbUser.iId || iIdA == clbUser.iId)) sStyle = " style=\"font-weight:bold\"";

        sBox += "<tr" + sStyle + ">";
        sBox += "<td>" + gd.dt.ToString("d", getCi()) + "&nbsp;" + gd.dt.ToString("t", getCi()) + "</td>";

        sBox += "<td style=\"white-space: nowrap\" align=\"right\"><a href=\"/Member/ClubDetails?iClub=" + iIdH.ToString() + "\" target=\"\">" + sClubNameH + "</a></td>";
        sBox += "<td style=\"white-space: nowrap\" align=\"center\">&nbsp;-&nbsp;</td>";
        sBox += "<td style=\"white-space: nowrap\" align=\"left\"><a href=\"/Member/ClubDetails?iClub=" + iIdA.ToString() + "\" target=\"\">" + sClubNameA + "</a></td>";

        if (gd.team[0].iGoals + gd.team[1].iGoals >= 0) {
          sBox += "<td style=\"white-space: nowrap\" align=\"center\">" + MvcApplication.ckcore.ui.getResultString(gd) + "</td>";
        } else {
          sBox += "<td style=\"white-space: nowrap\" align=\"center\">-</td>";
        }
        sBox += "</tr>";
      }

      return Json(sBox, JsonRequestBehavior.AllowGet);
    }

    public JsonResult LeagueCupGetScorer(byte iGameType, int iLand, int iDivision)
    {
      CornerkickManager.Club clbUser = ckClub();

      string sBox = "";

      var ltScorer = MvcApplication.ckcore.ui.getScorer(iGameType, iLand: iLand, iDivision: iDivision, bNation: iGameType == 7);

      int iNo = 0;
      foreach (CornerkickManager.UI.Scorer sc in ltScorer) {
        iNo++;
        string sStyle2 = "";
        if (clbUser != null && MvcApplication.ckcore.ltPlayer[sc.iId].iClubId == clbUser.iId) sStyle2 = "font-weight:bold";

        sBox += "<tr style=" + sStyle2 + " class=\"\">";
        sBox += "<td class=\"first\">" + iNo.ToString() + "</td>";
        sBox += "<td style=\"white-space: nowrap\">" + sc.sName + "</td>";
        sBox += "<td style=\"white-space: nowrap\">" + sc.sTeam + "</td>";
        sBox += "<td style=\"white-space: nowrap\" align=\"right\">" + sc.iGoals  .ToString() + "</td>";
        sBox += "<td style=\"white-space: nowrap\" align=\"right\">" + sc.iAssists.ToString() + "</td>";
        sBox += "<td style=\"white-space: nowrap\" align=\"right\">" + (sc.iGoals + sc.iAssists).ToString() + "</td>";
        sBox += "</tr>";
      }

      return Json(sBox, JsonRequestBehavior.AllowGet);
    }

    public ContentResult GetLeaguePlaceHistory(int iSeason = 0)
    {
      CornerkickManager.Club club = ckClub();

      CornerkickManager.Cup league = getCup(iSeason, 1, club.iLand, club.iDivision);

      List<Models.DataPointGeneral> dataPoints = new List<Models.DataPointGeneral>();

      for (int iMd = 1; iMd < CornerkickManager.Tool.getMatchday(league, MvcApplication.ckcore.dtDatum) + 1; iMd++) {
        int iPlace = CornerkickManager.Tool.getLeaguePlace(league, club, iMd);
        if (iPlace > 0) dataPoints.Add(new Models.DataPointGeneral(iMd, iPlace));
      }

      JsonSerializerSettings _jsonSetting = new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore };

      return Content(JsonConvert.SerializeObject(dataPoints, _jsonSetting), "application/json");
    }

    //[Authorize]
    public ActionResult Cup(Models.CupModel cupModel)
    {
      cupModel.ltErg = new List<List<CornerkickGame.Game.Data>>();

      CornerkickManager.Club clb = ckClub();
      if (clb != null) {
        cupModel.iClubId = clb.iId;
        cupModel.iLand = clb.iLand;
      }

      CornerkickManager.Cup cup = MvcApplication.ckcore.tl.getCup(2, cupModel.iLand);

      if (cup == null) return View(cupModel);
      if (cup.ltMatchdays == null) return View(cupModel);
      if (cup.ltMatchdays.Count < 1) return View(cupModel);

      // Add lands to dropdown list
      foreach (int iLand in MvcApplication.iNations) {
        cupModel.ddlLand.Add(new SelectListItem { Text = CornerkickManager.Main.sLand[iLand], Value = iLand.ToString() });
      }

      cupModel.ddlSeason = getDdlSeason();
      cupModel.iSeason = MvcApplication.ckcore.iSeason;

      return View(cupModel);
    }

    public JsonResult CupGetDdlMatchdays(int iSaison, int iLand)
    {
      CornerkickManager.Cup cup = getCup(iSaison, 2, iLand);
      if (cup == null) return Json(null, JsonRequestBehavior.AllowGet);

      string[] ltMd = new string[cup.getMatchdaysTotal()];
      // Spieltage zu Dropdown Menü hinzufügen
      for (int iMd = 0; iMd < ltMd.Length; iMd++) {
        ltMd[iMd] = (iMd + 1).ToString();

        int nRound = cup.getKoRound(cup.ltClubs[0].Count);
        ltMd[iMd] += ";" + CornerkickManager.Main.sCupRound[nRound - iMd - 1];
      }

      /*
      // Spieltage zu Dropdown Menü hinzufügen
      if (cup.ltMatchdays[0].ltGameData != null) {
        int nRound = cup.getKoRound(cup.ltClubs[0].Count);
        while (cupModel.ltDdlSpTg.Count < nRound) {
          int iMd = cupModel.ltDdlSpTg.Count + 1;
          cupModel.ltDdlSpTg.Add(new SelectListItem { Text = CornerkickManager.Main.sCupRound[nRound - iMd], Value = iMd.ToString() });
        }
      }
      */

      return Json(ltMd, JsonRequestBehavior.AllowGet);
    }

    private string getCupTeams(CornerkickManager.Cup cup, int iMatchday, int iGroup = -1)
    {
      if (cup == null) return "";
      if (cup.ltMatchdays == null) return "";

      CornerkickManager.Club clbUser = ckClub();

      string sBox = "";

      iMatchday = Math.Min(iMatchday, cup.ltMatchdays.Count - 1);
      CornerkickManager.Cup.Matchday md = cup.ltMatchdays[iMatchday];

      if (md.ltGameData == null || md.ltGameData.Count == 0) {
        List<CornerkickManager.Club> ltClubs = MvcApplication.ckcore.tl.getCupParticipants(cup, iMatchday);

        foreach (CornerkickManager.Club clb in ltClubs) {
          if (iGroup >= 0 && iGroup < cup.ltClubs.Length && cup.ltClubs[iGroup].IndexOf(clb) < 0) continue;

          var sStyle = "";
          if (clbUser != null && clb == clbUser) sStyle = "font-weight:bold";

          sBox += "<p><a style=" + sStyle + " href=\"/Member/ClubDetails?iClub=" + clb.iId.ToString() + "\" target=\"_blank\">" + clb.sName + "</a></p>";
        }

        return sBox;
      }

      foreach (CornerkickGame.Game.Data gd in md.ltGameData) {
        string sClubNameH = "freilos";
        string sClubNameA = "freilos";
        int iIdH = gd.team[0].iTeamId;
        int iIdA = gd.team[1].iTeamId;

        if (gd.team[0].iTeamId >= 0) sClubNameH = MvcApplication.ckcore.ltClubs[iIdH].sName;
        if (gd.team[1].iTeamId >= 0) sClubNameA = MvcApplication.ckcore.ltClubs[iIdA].sName;

        if (cup.checkCupGroupPhase(iMatchday) && iGroup >= 0 && iGroup < cup.ltClubs.Length) {
          if (cup.ltClubs[iGroup].IndexOf(MvcApplication.ckcore.ltClubs[iIdH]) < 0 || cup.ltClubs[iGroup].IndexOf(MvcApplication.ckcore.ltClubs[iIdA]) < 0) continue;
        }

        var sStyle = "";
        if (clbUser != null && (iIdH == clbUser.iId || iIdA == clbUser.iId)) sStyle = "font-weight:bold";

        sBox += "<tr style=" + sStyle + ">";
        sBox += "<td>" + gd.dt.ToString("d", getCi()) + "&nbsp;" + gd.dt.ToString("t", getCi()) + "</td>";

        sBox += "<td style=\"white-space: nowrap\" align=\"right\"><a href=\"/Member/ClubDetails?iClub=" + iIdH.ToString() + "\" target=\"\">" + sClubNameH + "</a></td>";
        sBox += "<td style=\"white-space: nowrap\" align=\"center\">&nbsp;-&nbsp;</td>";
        sBox += "<td style=\"white-space: nowrap\" align=\"left\"><a href=\"/Member/ClubDetails?iClub=" + iIdA.ToString() + "\" target=\"\">" + sClubNameA + "</a></td>";

        if (gd.team[0].iGoals >= 0 && gd.team[1].iGoals >= 0) {
          sBox += "<td style=\"white-space: nowrap\" align=\"center\">" + MvcApplication.ckcore.ui.getResultString(gd) + "</td>";
        } else {
          sBox += "<td style=\"white-space: nowrap\" align=\"center\">-</td>";
        }
        sBox += "</tr>";
      }

      return sBox;
    }

    public JsonResult CupGetMatchday(int iSaison, int iLand)
    {
      CornerkickManager.Cup cup = getCup(iSaison, 2, iLand);

      // Get current matchday
      int iMd = CornerkickManager.Tool.getMatchday(cup, MvcApplication.ckcore.dtDatum);

      CornerkickManager.Club clb = ckClub();
      if (clb != null) {
        // Increment matchday match is if today or tomorrow
        CornerkickGame.Game.Data gdNext = MvcApplication.ckcore.tl.getNextGame(clb, MvcApplication.ckcore.dtDatum, iGameType: 2);
        if (gdNext != null && (gdNext.dt.Date - MvcApplication.ckcore.dtDatum.Date).Days < 2) iMd++;
      }

      // Limit to 1
      iMd = Math.Max(iMd, 1);

      return Json(iMd, JsonRequestBehavior.AllowGet);
    }

    public JsonResult setCup(Models.CupModel cupModel, int iSaison, int iLand, int iMatchday)
    {
      CornerkickManager.Cup cup = getCup(iSaison, 2, iLand);

      return Json(getCupTeams(cup, iMatchday - 1), JsonRequestBehavior.AllowGet);
    }

    //[Authorize]
    public ActionResult CupGold(Models.CupGoldModel cupGoldModel)
    {
      cupGoldModel.ddlSeason = getDdlSeason();
      if (cupGoldModel.ddlSeason.Count > 0) cupGoldModel.ddlSeason.RemoveAt(0);
      cupGoldModel.iSeason = MvcApplication.ckcore.iSeason;

      CornerkickManager.Cup cupGold = MvcApplication.ckcore.tl.getCup(3);
      cupGoldModel.iMatchday = Math.Min(CornerkickManager.Tool.getMatchday(cupGold, MvcApplication.ckcore.dtDatum), cupGold.ltMatchdays.Count - 1);

      cupGoldModel.ddlMatchday = new List<SelectListItem>();
      for (int iMd = 0; iMd < Math.Max(6, cupGoldModel.iMatchday + 1); iMd++) {
        string sText = (iMd + 1).ToString();
        if (iMd > 5) sText = CornerkickManager.Main.sCupRound[3 - ((iMd - 6) / 2)];

        cupGoldModel.ddlMatchday.Add(new SelectListItem { Text = sText, Value = iMd.ToString() });
      }

      return View(cupGoldModel);
    }

    public JsonResult setCupGold(int iSaison, int iMatchday, int iGroup)
    {
      CornerkickManager.Cup cupGold = getCup(iSaison, 3);

      return Json(getCupTeams(cupGold, iMatchday, iGroup), JsonRequestBehavior.AllowGet);
    }

    //[Authorize]
    public ActionResult CupSilver(Models.CupSilverModel cupSilverModel)
    {
      cupSilverModel.ddlSeason = getDdlSeason();
      if (cupSilverModel.ddlSeason.Count > 0) cupSilverModel.ddlSeason.RemoveAt(0);
      cupSilverModel.iSeason = MvcApplication.ckcore.iSeason;

      CornerkickManager.Cup cupSilver = MvcApplication.ckcore.tl.getCup(4);
      cupSilverModel.iMatchday = Math.Min(CornerkickManager.Tool.getMatchday(cupSilver, MvcApplication.ckcore.dtDatum), cupSilver.ltMatchdays.Count - 1);

      cupSilverModel.ddlMatchday = new List<SelectListItem>();
      for (int iMd = 0; iMd < Math.Max(6, cupSilverModel.iMatchday + 1); iMd++) {
        string sText = (iMd + 1).ToString();
        if (iMd > 5) sText = CornerkickManager.Main.sCupRound[3 - ((iMd - 6) / 2)];

        cupSilverModel.ddlMatchday.Add(new SelectListItem { Text = sText, Value = iMd.ToString() });
      }

      return View(cupSilverModel);
    }

    public JsonResult setCupSilver(int iSaison, int iMatchday, int iGroup)
    {
      CornerkickManager.Cup cupSilver = getCup(iSaison, 4);

      return Json(getCupTeams(cupSilver, iMatchday, iGroup), JsonRequestBehavior.AllowGet);
    }

    //[Authorize]
    public ActionResult CupWc(Models.CupWcModel cupWcModel)
    {
      cupWcModel.ddlSeason = getDdlSeason();
      cupWcModel.iSeason = MvcApplication.ckcore.iSeason;

      CornerkickManager.Cup cupWc = MvcApplication.ckcore.tl.getCup(7);
      cupWcModel.iMatchday = Math.Min(CornerkickManager.Tool.getMatchday(cupWc, MvcApplication.ckcore.dtDatum), cupWc.ltMatchdays.Count - 1);

      return View(cupWcModel);
    }

    public JsonResult setCupWc(int iSaison, int iMatchday, int iGroup)
    {
      CornerkickManager.Cup cupWc = getCup(iSaison, 7);

      return Json(getCupTeams(cupWc, iMatchday, iGroup), JsonRequestBehavior.AllowGet);
    }

    public JsonResult CupGetLeague(int iCupId, int iSaison, int iMatchday, sbyte iGroup)
    {
      string sBox = "";

      CornerkickManager.Cup cup = MvcApplication.ckcore.tl.getCup(iCupId);
      if (cup.checkCupGroupPhase(iMatchday)) {
        sBox = getTable(cup, iMatchday + 1, iGroup, iColor1: 2);
      }

      return Json(sBox, JsonRequestBehavior.AllowGet);
    }

    //////////////////////////////////////////////////////////////////////////
    /// <summary>
    /// Calendar
    /// </summary>
    /// <returns></returns>
    //////////////////////////////////////////////////////////////////////////
    [Authorize]
    public ActionResult Calendar(Models.CalendarModels cal)
    {
      /*
      if (cal.sCal == null) cal.sCal = new List<string>();
      cal.sCal.Add("Kalender 1");
      cal.sCal.Add("Kalender 2");
      cal.sCal.Add("Kalender 3");
      ViewData["sCal"] = cal.sCal;
      */

      cal.iClubId = ckClub().iId;

      cal.ltTestgames = new List<Models.Testgame>();
      foreach (CornerkickManager.Cup cup in MvcApplication.ckcore.ltCups) {
        if (cup.iId == -5) {
          foreach (CornerkickManager.Cup.Matchday md in cup.ltMatchdays) {
            foreach (CornerkickGame.Game.Data gd in md.ltGameData) {
              if (gd.team[1].iTeamId == ckClub().iId) {
                Models.Testgame tg = new Models.Testgame();
                tg.dt = md.dt;
                tg.iTeamHome = gd.team[0].iTeamId;
                tg.iTeamAway = gd.team[1].iTeamId;
                cal.ltTestgames.Add(tg);
              }
            }
          }
        }
      }

      cal.ddlTestgameClubs = new List<SelectListItem>();
      foreach (CornerkickManager.Club clbTg in MvcApplication.ckcore.ltClubs) {
        if (clbTg.iId == cal.iClubId) continue;
        if (clbTg.iLand < 0) continue;

        //CornerkickGame.Game.Data gdNext = MvcApplication.ckcore.tl.getNextGame(clbTg, MvcApplication.ckcore.dtDatum, false);

        cal.ddlTestgameClubs.Add(new SelectListItem { Text = clbTg.sName, Value = clbTg.iId.ToString() });
      }

      return View(cal);
    }

    public List<Models.DiaryEvent> getCalendarEvents()
    {
      List<Models.DiaryEvent> ltEvents = new List<Models.DiaryEvent>();

      CornerkickManager.Club club = ckClub();

      DateTime dtStartWeek = MvcApplication.ckcore.dtDatum;
      while ((int)(dtStartWeek.DayOfWeek) != 0) dtStartWeek = dtStartWeek.AddDays(-1);

      int iDay = 0;
      //DateTime dt = new DateTime(MvcApplication.ckcore.dtDatum.Year, MvcApplication.ckcore.dtDatum.Month, MvcApplication.ckcore.dtDatum.Day);
      DateTime dt = MvcApplication.ckcore.dtSeasonStart.Date;
      while (dt.CompareTo(MvcApplication.ckcore.dtSeasonEnd) < 0) {
        // Night
        ltEvents.Add(new Models.DiaryEvent {
          iID = ltEvents.Count,
          sTitle = "Nachtruhe",
          sDescription = "Nachtruhe",
          sStartDate = dt.Add(new TimeSpan(23, 0, 0)).ToString("yyyy-MM-ddTHH:mm:ss"),
          sEndDate = dt.AddDays(1).Add(new TimeSpan(7, 0, 0)).ToString("yyyy-MM-ddTHH:mm:ss"),
          sColor = "rgb(0, 0, 140)",
          bEditable = false,
          bAllDay = false
        });

        // New Year
        if (dt.Day == 1 && dt.Month == 1) {
          ltEvents.Add(new Models.DiaryEvent {
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
        CornerkickManager.TrainingCamp.Booking booking = MvcApplication.ckcore.tcp.getCurrentCamp(club, dt, true);
        if (booking.camp.iId > 0) {
          if (dt.Date.Equals(booking.dtArrival.Date)) {
            ltEvents.Add(new Models.DiaryEvent {
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
            ltEvents.Add(new Models.DiaryEvent {
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
            ltEvents.Add(new Models.DiaryEvent {
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

        // Future training
        foreach (CornerkickManager.Main.TrainingPlan.Unit tu in club.training.ltUnit) {
          if (tu.dt.Date.Equals(dt) && tu.iType > 0 && !bCampTravelDay) {
            string sTrainingName = CornerkickManager.Player.getTraining(tu.iType, MvcApplication.ckcore.plr.ltTraining).sName;

            ltEvents.Add(new Models.DiaryEvent {
              iID = ltEvents.Count,
              sTitle = " Training (" + sTrainingName + ")",
              sDescription = sTrainingName,
              sStartDate = tu.dt.ToString("yyyy-MM-ddTHH:mm:ss"),
              sEndDate = tu.dt.AddMinutes(90).ToString("yyyy-MM-ddTHH:mm:ss"),
              sColor = "rgb(255, 255, 0)",
              sTextColor = "rgb(100, 100, 100)",
              bEditable = false,
              bAllDay = false
            });
          }
        }

        // Past trainings
        foreach (CornerkickManager.Main.TrainingHistory th in club.ltTrainingHist) {
          if (th.dt.Date.Equals(dt) && th.iType > 1) {
            string sTrainingName = CornerkickManager.Player.getTraining(th.iType, MvcApplication.ckcore.plr.ltTraining).sName;

            ltEvents.Add(new Models.DiaryEvent {
              iID = ltEvents.Count,
              sTitle = " Training (" + sTrainingName + ")",
              sDescription = sTrainingName,
              sStartDate = th.dt.ToString("yyyy-MM-ddTHH:mm:ss"),
              sEndDate = th.dt.AddMinutes(90).ToString("yyyy-MM-ddTHH:mm:ss"),
              sColor = "rgb(255, 255, 180)",
              sTextColor = "rgb(100, 100, 100)",
              bEditable = false,
              bAllDay = false
            });
          }
        }

        // Cup
        foreach (CornerkickManager.Cup cup in MvcApplication.ckcore.ltCups) {
          int iMd = 0;
          foreach (CornerkickManager.Cup.Matchday md in cup.ltMatchdays) {
            if (dt.Date.Equals(md.dt.Date)) {
              if (md.ltGameData == null || md.ltGameData.Count == 0) { // If no data
                if (cup.iId == 2 && cup.iId2 == club.iLand && iMd == 0) { // If nat. cup and first round
                  ltEvents.Add(new Models.DiaryEvent {
                    iID = ltEvents.Count,
                    sTitle = " " + cup.sName + ", 1. Pokalrunde",
                    sStartDate = md.dt.ToString("yyyy-MM-ddTHH:mm:ss"),
                    sEndDate = md.dt.AddMinutes(105).ToString("yyyy-MM-ddTHH:mm:ss"),
                    sColor = "rgb(100, 100, 255)",
                    bEditable = false,
                    bAllDay = false
                  });
                }

                continue;
              }

              foreach (CornerkickGame.Game.Data gd in md.ltGameData) {
                int iIdH = gd.team[0].iTeamId;
                int iIdA = gd.team[1].iTeamId;
                if (iIdH == club.iId ||
                    iIdA == club.iId ||
                    (cup.iId == 2 && iMd == 0) ||
                    cup.iId == 7) {
                  string sH = "";
                  string sA = "";
                  if (iIdH >= 0 && iIdH < MvcApplication.ckcore.ltClubs.Count) sH = MvcApplication.ckcore.ltClubs[iIdH].sName;
                  if (iIdA >= 0 && iIdA < MvcApplication.ckcore.ltClubs.Count) sA = MvcApplication.ckcore.ltClubs[iIdA].sName;

                  string sRes = MvcApplication.ckcore.ui.getResultString(gd);

                  string sTitle = " " + cup.sName;
                  string sColor = "rgb(200, 0, 200)";
                  if (cup.iId == 1) { // Nat. league
                    sTitle = " Liga, " + (iMd + 1).ToString().PadLeft(2) + ". Spieltag";
                    sColor = "rgb(0, 175, 100)";
                  } else if (cup.iId == 2) { // Nat. Cup
                    sTitle += ", " + CornerkickManager.Main.sCupRound[cup.getKoRound(md.ltGameData.Count)];
                    sColor = "rgb(100, 100, 255)";
                  } else if (cup.iId == 3 || cup.iId == 4) { // Int. games
                    sColor = "rgb(255, 200, 14)";
                  } else if (cup.iId == -5) { // Testgame requests
                    sColor = "rgb(255, 200, 255)";
                  } else if (cup.iId == 7) { // World cup
                    sColor = "rgb( 91, 146, 229)";
                  }
                  sTitle += ": " + sH + " vs. " + sA;
                  if (!string.IsNullOrEmpty(sRes)) sTitle += ", " + sRes;

                  ltEvents.Add(new Models.DiaryEvent {
                    iID = ltEvents.Count,
                    sTitle = sTitle,
                    sStartDate = md.dt.ToString("yyyy-MM-ddTHH:mm:ss"),
                    sEndDate = md.dt.AddMinutes(105).ToString("yyyy-MM-ddTHH:mm:ss"),
                    sColor = sColor,
                    bEditable = false,
                    bAllDay = false
                  });

                  if (cup.iId == 2 && cup.iId2 == club.iLand && md.ltGameData.Count > 1) {
                    ltEvents.Add(new Models.DiaryEvent {
                      iID = ltEvents.Count,
                      sTitle = " " + cup.sName + ", Auslosung " + CornerkickManager.Main.sCupRound[cup.getKoRound(md.ltGameData.Count) - 1],
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
            } else if (cup.iId == 2 && cup.iId2 == club.iLand && iMd == 0 && dt.Date.Equals(MvcApplication.ckcore.dtSeasonStart.AddDays(6).Date)) {
              ltEvents.Add(new Models.DiaryEvent {
                iID = ltEvents.Count,
                sTitle = " " + cup.sName + ", Auslosung 1. Runde",
                sStartDate = new DateTime(dt.Year, dt.Month, dt.Day, 12,  0, 0).ToString("yyyy-MM-ddTHH:mm:ss"),
                sEndDate   = new DateTime(dt.Year, dt.Month, dt.Day, 12, 30, 0).ToString("yyyy-MM-ddTHH:mm:ss"),
                sColor = "rgb(100, 200, 255)",
                bEditable = false,
                bAllDay = false
              });
            }

            iMd++;
          }
        }

        dt = dt.AddDays(1);
        iDay++;
      }

      return ltEvents;
    }

    public ActionResult PostCalendarData()
    {
      //var result = getCalendarEvents();

      var ApptListForDate = getCalendarEvents();
      var eventList = from e in ApptListForDate
                      select new
                      {
                        id          = e.iID,
                        title       = e.sTitle,
                        description = e.sDescription,
                        start       = e.sStartDate,
                        end         = e.sEndDate,
                        color       = e.sColor,
                        textColor   = e.sTextColor,
                        editable    = e.bEditable,
                        allDay      = e.bAllDay
                      };

      var rows = eventList.ToArray();

      return Json(rows, JsonRequestBehavior.AllowGet);
    }

    public JsonResult AddTestGameToCalendar(string title, string start, int iH, int iM, int iTeamId)
    {
      string sReturn = "Error";

      CornerkickManager.Club club = ckClub();
      if (club == null) return Json(sReturn, JsonRequestBehavior.AllowGet);

      int iTeamIdUser = club.iId;
      //int iTeamId = -1;
      //int.TryParse(sTeamId, out iTeamId);
      if (iTeamId >= 0 && iTeamId < MvcApplication.ckcore.ltClubs.Count && iTeamId != iTeamIdUser) {
        CornerkickManager.Cup.Matchday md = new CornerkickManager.Cup.Matchday();
        DateTime.TryParse(start, out md.dt);
        md.dt = md.dt.Date.AddHours(iH).AddMinutes(iM);

        md.ltGameData = new List<CornerkickGame.Game.Data>();
        CornerkickGame.Game.Data gd = new CornerkickGame.Game.Data();
        gd.team[0].iTeamId = iTeamIdUser;
        gd.team[1].iTeamId = iTeamId;
        gd.dt = md.dt;

        md.ltGameData.Add(gd);

        // Inform user
        CornerkickManager.Club clubRequest = MvcApplication.ckcore.ltClubs[iTeamId];
        if (clubRequest.user == null) {
          CornerkickGame.Game.Data gdNext = MvcApplication.ckcore.tl.getNextGame(clubRequest, md.dt, bPre: false);
          if (gdNext != null && (gdNext.dt - md.dt).TotalDays < 4) return Json("Anfrage für Testspiel abgelehnt. Begründung: Zu nah am nächsten Spiel", JsonRequestBehavior.AllowGet);

          CornerkickGame.Game.Data gdPrev = MvcApplication.ckcore.tl.getNextGame(clubRequest, md.dt, bPre: true);
          if (gdPrev != null && (md.dt - gdPrev.dt).TotalDays < 4) return Json("Anfrage für Testspiel abgelehnt. Begründung: Zu kurz nach letztem Spiel", JsonRequestBehavior.AllowGet);

          createTestgame(md);

          sReturn = "Testspiel am " + md.dt.ToString("d", getCi()) + " " + md.dt.ToString("t", getCi()) + " gegen " + clubRequest.sName + " vereinbart";

          club.nextGame = MvcApplication.ckcore.tl.getNextGame(club, MvcApplication.ckcore.dtDatum);
        } else {
          CornerkickManager.Cup cup = new CornerkickManager.Cup();
          cup.iId = -5;
          cup.sName = "Anfrage Testspiel";
          cup.ltMatchdays.Add(md);
          MvcApplication.ckcore.ltCups.Add(cup);

          sReturn = "Anfrage für Testspiel am " + md.dt.ToString("d", getCi()) + " " + md.dt.ToString("t", getCi()) + " gegen " + MvcApplication.ckcore.ltClubs[iTeamId].sName + " gesendet";

          MvcApplication.ckcore.sendNews(clubRequest.user, "Sie haben eine neue Anfrage für ein Testspiel erhalten.", iType: 0, iId: iTeamId);
        }
      }

      return Json(sReturn, JsonRequestBehavior.AllowGet);
    }

    [HttpPost]
    public JsonResult AcceptTestgame(string sDateTestgame)
    {
      DateTime dt = new DateTime();
      if (!DateTime.TryParse(sDateTestgame, out dt)) {
        Response.StatusCode = 1;
        return Json(new { message = "Error parsing '" + sDateTestgame + "' to Date" }, JsonRequestBehavior.AllowGet);
      }

      CornerkickManager.Club club = ckClub();
      if (club == null) return Json(false, JsonRequestBehavior.AllowGet);

      for (int iC = 0; iC < MvcApplication.ckcore.ltCups.Count; iC++) {
        CornerkickManager.Cup cup = MvcApplication.ckcore.ltCups[iC];

        if (cup.iId == -5) {
          foreach (CornerkickManager.Cup.Matchday md in cup.ltMatchdays) {
            if (md.dt.Equals(dt)) {
              foreach (CornerkickGame.Game.Data gd in md.ltGameData) {
                if (gd.team[1].iTeamId == club.iId) {
                  createTestgame(md);

                  MvcApplication.ckcore.ltCups.RemoveAt(iC);

                  club.nextGame = MvcApplication.ckcore.tl.getNextGame(club, MvcApplication.ckcore.dtDatum);

                  CornerkickManager.Club clubH = MvcApplication.ckcore.ltClubs[gd.team[0].iTeamId];
                  clubH.nextGame = MvcApplication.ckcore.tl.getNextGame(clubH, MvcApplication.ckcore.dtDatum);

                  MvcApplication.ckcore.sendNews(clubH.user, "Ihre Anfrage an " + club.sName + " für ein Testspiel am " + dt.ToString("d", getCi()) + " um " + dt.ToString("t", getCi()) + " wurde akzeptiert!", iType: 0, iId: gd.team[0].iTeamId);

                  return Json("", JsonRequestBehavior.AllowGet);
                }
              }
            }
          }
        }
      }

      return Json("", JsonRequestBehavior.AllowGet);
    }

    private void createTestgame(CornerkickManager.Cup.Matchday md)
    {
      CornerkickManager.Cup cupTestGames = MvcApplication.ckcore.tl.getCup(5);

      if (cupTestGames == null) {
        cupTestGames = new CornerkickManager.Cup();
        cupTestGames.iId = 5;
        cupTestGames.settings.fAttraction = 0.5f;
        cupTestGames.settings.iNeutral = 1;
        cupTestGames.settings.fAttraction = 0.25f;
        cupTestGames.sName = "Testspiel";
        MvcApplication.ckcore.ltCups.Add(cupTestGames);
      }

      cupTestGames.ltMatchdays.Add(md);
    }

    [HttpPost]
    public JsonResult DeclineTestgame(string sDateTestgame)
    {
      DateTime dt = new DateTime();
      if (!DateTime.TryParse(sDateTestgame, out dt)) {
        return Json("", JsonRequestBehavior.AllowGet);
      }

      CornerkickManager.Club clb = ckClub();
      if (clb == null) return Json(false, JsonRequestBehavior.AllowGet);

      for (int iC = 0; iC < MvcApplication.ckcore.ltCups.Count; iC++) {
        CornerkickManager.Cup cup = MvcApplication.ckcore.ltCups[iC];

        if (cup.iId == -5) {
          foreach (CornerkickManager.Cup.Matchday md in cup.ltMatchdays) {
            if (md.dt.Equals(dt)) {
              foreach (CornerkickGame.Game.Data gd in md.ltGameData) {
                if (gd.team[1].iTeamId == clb.iId) {
                  cup.iId = 5;
                  MvcApplication.ckcore.ltCups.RemoveAt(iC);

                  CornerkickManager.Club clubH = MvcApplication.ckcore.ltClubs[gd.team[0].iTeamId];
                  MvcApplication.ckcore.sendNews(clubH.user, "Ihre Anfrage an " + clb.sName + " für ein Testspiel am " + dt.ToString("dd.MM.yyyy") + " um " + dt.ToString("hh:mm") + " wurde abgelehnt!", iType: 0, iId: gd.team[0].iTeamId);

                  return Json("", JsonRequestBehavior.AllowGet);
                }
              }
            }
          }
        }
      }

      return Json("", JsonRequestBehavior.AllowGet);
    }

    public JsonResult bookTrainingscamp(int iIx, string sStart, int nDays)
    {
      DateTime dtStart = new DateTime();
      if (!DateTime.TryParse(sStart, out dtStart)) return Json("Not a valid start date format!", JsonRequestBehavior.AllowGet);
      dtStart = dtStart.Date.AddHours(9);

      if ((int)(dtStart.Date - MvcApplication.ckcore.dtDatum.Date).TotalDays < 2) {
        return Json("Trainingslager müssen min. 2 Tage im vorraus gebucht werden!", JsonRequestBehavior.AllowGet);
      }

      DateTime dtEnd = dtStart.Date.AddDays(nDays).AddHours(18);

      CornerkickManager.Club club = ckClub();
      if (club == null) return Json(false, JsonRequestBehavior.AllowGet);

      foreach (CornerkickGame.Game.Data data in MvcApplication.ckcore.tl.getNextGames(club, MvcApplication.ckcore.dtDatum, false)) {
        if (data.iGameType == 5) continue;

        if (dtStart.Date.Date.CompareTo(data.dt) == 0) return Json(  "Abreise am Spieltag nicht erlaubt!", JsonRequestBehavior.AllowGet);
        if (dtEnd  .Date.Date.CompareTo(data.dt) == 0) return Json("Rückreise am Spieltag nicht erlaubt!", JsonRequestBehavior.AllowGet);

        if (dtStart.Date.Date.CompareTo(data.dt) < 0 /* Start date before game date */ &&
            dtEnd  .Date.Date.CompareTo(data.dt) > 0 /* End date after game date */) {
          return Json("Trainingslager über Spieltag nicht erlaubt!", JsonRequestBehavior.AllowGet);
        }
      }

      CornerkickManager.TrainingCamp.Camp camp = MvcApplication.ckcore.tcp.ltCamps[iIx];

      MvcApplication.ckcore.tcp.bookCamp(ref club, camp, dtStart, dtEnd);

      return Json("Trainingslager gebucht!", JsonRequestBehavior.AllowGet);
    }

    [HttpPost]
    public JsonResult getStringDaysUntilNextGame(DateTime dtStart, int iIgnoreGameType = 0)
    {
      int nDays = getDaysUntilNextGame(dtStart, iIgnoreGameType);

      List<string> ltDays = new List<string>();
      for (int iDay = 1; iDay < nDays; iDay++) {
        ltDays.Add(iDay.ToString().PadLeft(2) + " (Abreise: " + dtStart.Date.AddDays(iDay).ToLongDateString() + ")");
        //ltDays.Add((iDay + 1).ToString().PadLeft(2) + " (Rückreise: " + dtStart.Date.AddDays(iDay).ToString("d", getCi()) + ")");
      }

      return Json(ltDays.ToArray(), JsonRequestBehavior.AllowGet);
    }

    [HttpPost]
    public int getDaysUntilNextGame(DateTime dtStart, int iIgnoreGameType = 0)
    {
      int nDays = 999;

      CornerkickManager.Club club = ckClub();
      if (club == null) return nDays;

      List<CornerkickGame.Game.Data> ltGdNext = MvcApplication.ckcore.tl.getNextGames(club, dtStart, false);
      foreach (CornerkickGame.Game.Data data in ltGdNext) {
        if (data.iGameType == iIgnoreGameType) continue;

        nDays = Math.Min(nDays, (int)(data.dt - dtStart).TotalDays);
      }

      nDays = Math.Min(nDays, (int)(MvcApplication.ckcore.dtSeasonEnd - dtStart).TotalDays);

      return nDays;
    }

    public int getDaysUntilNextGame(DateTime dtStart, List<int> ltIgnoreGameTypes)
    {
      int nDays = 999;

      foreach (int iIgnoreGameType in ltIgnoreGameTypes) {
        nDays = Math.Min(nDays, getDaysUntilNextGame(dtStart, iIgnoreGameType));
      }

      return nDays;
    }

    public JsonResult setSelectedCamp(Models.CalendarModels mdCalendar, int iIx)
    {
      mdCalendar.camp = MvcApplication.ckcore.tcp.ltCamps[iIx];

      return Json(mdCalendar.camp, JsonRequestBehavior.AllowGet);
    }

    private static DateTime convertTimestampToDateTime(double timestamp)
    {
      var origin = new DateTime(1970, 1, 1, 0, 0, 0, 0);
      return origin.AddSeconds(timestamp);
    }

    private static long convertDateTimeToTimestamp(DateTime dt)
    {
      var origin = new DateTime(1970, 1, 1, 0, 0, 0, 0);
      //return (long)(dt - origin).TotalSeconds;
      return (dt.AddHours(-2).Ticks - 621355968000000000) / 10000;
    }

    //////////////////////////////////////////////////////////////////////////
    /// <summary>
    /// Finanzen
    /// </summary>
    /// <param name="Finanzen"></param>
    /// <returns></returns>
    //////////////////////////////////////////////////////////////////////////
    [Authorize]
    public ActionResult Finance(Models.FinanceModel financeModel)
    {
      CornerkickManager.User usr = ckUser();
      if (usr == null) return View(financeModel);

      CornerkickManager.Club clb = ckClub();
      if (clb == null) return View(financeModel);

      financeModel.ltAccount = clb.ltAccount;

      financeModel.iEintritt1 = clb.iAdmissionPrice[0];
      financeModel.iEintritt2 = clb.iAdmissionPrice[1];
      financeModel.iEintritt3 = clb.iAdmissionPrice[2];

      financeModel.iPriceSeason1 = clb.iAdmissionPriceSeasonal[0];
      financeModel.iPriceSeason2 = clb.iAdmissionPriceSeasonal[1];
      financeModel.iPriceSeason3 = clb.iAdmissionPriceSeasonal[2];
      financeModel.fSeasonalTicketsMaxFrac = clb.fSeasonalTicketsMaxFrac * 100f;

      financeModel.iSeasonalTickets = new int[clb.iSpectatorsSeasonal.Length];
      financeModel.iSeasonalTickets = clb.iSpectatorsSeasonal;

      financeModel.bEditable = MvcApplication.ckcore.dtDatum.Date.Equals(MvcApplication.ckcore.dtSeasonStart.Date);

      // Year of budget plan
      financeModel.ddlYear = new List<SelectListItem>();
      financeModel.ddlYear.Add(new SelectListItem { Text = MvcApplication.ckcore.dtSeasonStart.Year.ToString(), Value = "-1" });
      for (int iY = 0; iY < usr.ltBudget.Count; iY++) {
        financeModel.ddlYear.Add(new SelectListItem { Text = (MvcApplication.ckcore.dtSeasonStart.Year - (iY + 1)).ToString(), Value = iY.ToString() });
      }

      // Secret Balance
      financeModel.fBalanceSecretFracAdmissionPrice = clb.fBalanceSecretFracAdmissionPrice * 100f;
      financeModel.sBalanceSecret                   = clb.iBalanceSecret.ToString("N0", getCi()) + " €";

      return View(financeModel);
    }

    public JsonResult FinanceGetBudgetPlan(int iYear)
    {
      CornerkickManager.User usr = ckUser();
      if (usr == null) return Json(null, JsonRequestBehavior.AllowGet);

      CornerkickManager.Club clb = ckClub();
      if (clb == null) return Json(null, JsonRequestBehavior.AllowGet);

      CornerkickManager.Finance.Budget[] bd = new CornerkickManager.Finance.Budget[2];
      if (iYear < 0) {
        bd[0] = usr.budget;
        bd[1] = MvcApplication.ckcore.ui.getActualBudget(clb);

        if (bd[0].iPaySalary == 0) bd[0].iPaySalary = MvcApplication.ckcore.tl.getPlayerSalary(clb);
        if (bd[0].iPayStaff  == 0) bd[0].iPayStaff  = MvcApplication.ckcore.tl.getStuffSalary (clb);
      } else  if (iYear < usr.ltBudget.Count) {
        bd[0] = usr.ltBudget[iYear][0];
        bd[1] = usr.ltBudget[iYear][1];
      }

      long[][] iBudget = new long[2][];
      iBudget[0] = new long[13]; // Plan
      iBudget[1] = new long[13]; // Real

      for (byte i = 0; i < 2; i++) {
        iBudget[i][0] = bd[i].iInSpec;
        iBudget[i][1] = bd[i].iInBonusCup;
        iBudget[i][2] = bd[i].iInBonusSponsor;
        iBudget[i][3] = bd[i].iInTransfer;
        iBudget[i][4] = bd[i].iPaySalary;
        iBudget[i][5] = bd[i].iPayStaff;
        iBudget[i][6] = bd[i].iPayTransfer;
        iBudget[i][7] = bd[i].iPayStadium;
        iBudget[i][8] = bd[i].iPayTravel;
        iBudget[i][9] = bd[i].iPayInterest;

        long iInTotal  = MvcApplication.ckcore.fz.getBudgetInTotal (bd[i]);
        long iPayTotal = MvcApplication.ckcore.fz.getBudgetPayTotal(bd[i]);
        iBudget[i][10] = iInTotal;
        iBudget[i][11] = iPayTotal;

        iBudget[i][12] = iInTotal - iPayTotal;
      }

      return Json(iBudget, JsonRequestBehavior.AllowGet);
    }

    public ContentResult FinanceGetDevelopmentData(Models.FinanceModel financeModel)
    {
      CornerkickManager.Club clb = ckClub();
      if (clb == null) return Content("", "application/json");

      List<Models.DataPointGeneral> dataPoints = new List<Models.DataPointGeneral>();

      foreach (CornerkickManager.Finance.Account kto in clb.ltAccount) {
        if (kto.dt.CompareTo(MvcApplication.ckcore.dtDatum.AddDays(-30)) > 0) {
          long iDate = convertDateTimeToTimestamp(kto.dt);
          dataPoints.Add(new Models.DataPointGeneral(iDate, kto.iBalance));
        }
      }

      long iDateCurrent = convertDateTimeToTimestamp(MvcApplication.ckcore.dtDatum);
      dataPoints.Add(new Models.DataPointGeneral(iDateCurrent, clb.iBalance));

      JsonSerializerSettings _jsonSetting = new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore };

      return Content(JsonConvert.SerializeObject(dataPoints, _jsonSetting), "application/json");
    }

    public ContentResult FinanceGetSpec(Models.FinanceModel financeModel)
    {
      CornerkickManager.Club clb = ckClub();
      if (clb == null) return Content("", "application/json");

      List<Models.DataPointGeneral>[] dataPoints = new List<Models.DataPointGeneral>[2];
      dataPoints[0] = new List<Models.DataPointGeneral>(); // Actual spec.
      dataPoints[1] = new List<Models.DataPointGeneral>(); // Stadium size

      List<CornerkickGame.Game.Data> ltGdPast = MvcApplication.ckcore.tl.getNextGames(clb, MvcApplication.ckcore.dtDatum, false, true);
      int i = 0;
      foreach (CornerkickGame.Game.Data gd in ltGdPast) {
        if (gd.team[0].iTeamId == clb.iId) {
          i--;

          string sCupName = "";
          if      (gd.iGameType == 1) sCupName = " - Liga";
          else if (gd.iGameType == 2) sCupName = " - Pokal";
          else if (gd.iGameType == 3) sCupName = " - Gold-Cup";
          else if (gd.iGameType == 4) sCupName = " - Silver-Cup";
          else if (gd.iGameType == 5) sCupName = " - Testspiel";

          int iSpecTotal = gd.iSpectators[0] + gd.iSpectators[1] + gd.iSpectators[2];
          if (iSpecTotal > 0) {
            string sInfo0 = gd.dt.ToString("d", getCi()) + sCupName + "</br>" +
                            (gd.iSpectators[0] + gd.iSpectators[1] + gd.iSpectators[2]).ToString("N0", getCi()) + " (" + gd.iSpectators[0].ToString("N0", getCi()) + "/" + gd.iSpectators[1].ToString("N0", getCi()) + "/" + gd.iSpectators[2].ToString("N0", getCi()) + ")" + "</br>" +
                            gd.team[1].sTeam;
            dataPoints[0].Add(new Models.DataPointGeneral(i, iSpecTotal, sInfo0));
          }

          int iStadiumSeats = Math.Max(gd.stadium.getSeats(), iSpecTotal);
          if (iStadiumSeats > 0) {
            string sInfo1 = gd.dt.ToString("d", getCi()) + "</br>" +
                            gd.stadium.getSeats().ToString("N0", getCi()) + " (" + gd.stadium.getSeats(0).ToString("N0", getCi()) + "/" + gd.stadium.getSeats(1).ToString("N0", getCi()) + "/" + gd.stadium.getSeats(2).ToString("N0", getCi()) + ")" + "</br>" +
                            gd.team[1].sTeam;
            dataPoints[1].Add(new Models.DataPointGeneral(i, iStadiumSeats, sInfo1));
          }
        }
      }

      JsonSerializerSettings _jsonSetting = new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore };

      return Content(JsonConvert.SerializeObject(dataPoints, _jsonSetting), "application/json");
    }

    [HttpPost]
    public JsonResult FinanceSetAdmissionPrice(int[] iEintritt)
    {
      CornerkickManager.Club clb = ckClub();
      if (clb == null) return Json(false, JsonRequestBehavior.AllowGet);

      clb.iAdmissionPrice[0] = iEintritt[0];
      clb.iAdmissionPrice[1] = iEintritt[1];
      clb.iAdmissionPrice[2] = iEintritt[2];

      int iInSpec = 0;
      CornerkickManager.Cup league = MvcApplication.ckcore.tl.getCup(1, clb.iLand, clb.iDivision);
      if (league != null) {
        iInSpec = (clb.stadium.getSeats(0) * clb.iAdmissionPrice[0]) +
                  (clb.stadium.getSeats(1) * clb.iAdmissionPrice[1]) +
                  (clb.stadium.getSeats(2) * clb.iAdmissionPrice[2]);
        int nGamesHome = league.getMatchdaysTotal();
        iInSpec *= nGamesHome;
      }

      return Json(iInSpec.ToString("N0", getCi()) + "€", JsonRequestBehavior.AllowGet);
    }

    [HttpPost]
    public JsonResult FinanceSetAdmissionPriceSeasonal(int[] iPrice, float fSeasonalTicketsMaxFrac)
    {
      CornerkickManager.Club clb = ckClub();
      if (clb == null) return Json(false, JsonRequestBehavior.AllowGet);

      clb.iAdmissionPriceSeasonal[0] = iPrice[0];
      clb.iAdmissionPriceSeasonal[1] = iPrice[1];
      clb.iAdmissionPriceSeasonal[2] = iPrice[2];

      clb.fSeasonalTicketsMaxFrac = fSeasonalTicketsMaxFrac / 100f;

      return Json(null, JsonRequestBehavior.AllowGet);
    }

    [HttpPost]
    public JsonResult SetBudget(int[] iBudgetIn, int[] iBudgetPay)
    {
      CornerkickManager.User user = ckUser();

      if (iBudgetIn  != null) {
        user.budget.iInSpec         = iBudgetIn[0];
        user.budget.iInBonusCup     = iBudgetIn[1];
        user.budget.iInBonusSponsor = iBudgetIn[2];
        user.budget.iInTransfer     = iBudgetIn[3];
      }

      if (iBudgetPay != null) {
        user.budget.iPaySalary   = iBudgetPay[0];
        user.budget.iPayStaff    = iBudgetPay[1];
        user.budget.iPayTransfer = iBudgetPay[2];
        user.budget.iPayStadium  = iBudgetPay[3];
        user.budget.iPayInterest = iBudgetPay[4];
      }

      return Json(true, JsonRequestBehavior.AllowGet);
    }

    [HttpPost]
    public JsonResult FinanceSetBalanceSecret(float fBalanceSecretFracAdmissionPrice)
    {
      CornerkickManager.Club clb = ckClub();
      if (clb == null) return Json(false, JsonRequestBehavior.AllowGet);

      if (fBalanceSecretFracAdmissionPrice < 0.0) {
        return Json("Falsche Eingabe!", JsonRequestBehavior.AllowGet);
      }

      if (fBalanceSecretFracAdmissionPrice > 20) {
        return Json("Maximal 20% erlaubt!", JsonRequestBehavior.AllowGet);
      }

      clb.fBalanceSecretFracAdmissionPrice = fBalanceSecretFracAdmissionPrice / 100f;

      return Json(true, JsonRequestBehavior.AllowGet);
    }

    //////////////////////////////////////////////////////////////////////////
    /// <summary>
    /// Sponsor
    /// </summary>
    /// <param name="Sponsor"></param>
    /// <returns></returns>
    //////////////////////////////////////////////////////////////////////////
    [Authorize]
    public ActionResult Sponsor(Models.SponsorModel sponsorModel)
    {
      CornerkickManager.Club clb = ckClub();
      if (clb == null) return View(sponsorModel);

      Random rnd = new Random();

      if (clb.sponsorMain.iId >= MvcApplication.ckcore.fz.ltSponsoren.Count) {
        clb.sponsorMain.iId = (byte)rnd.Next(1, MvcApplication.ckcore.fz.ltSponsoren.Count);
      }

      sponsorModel.sponsorMain     = clb.sponsorMain;
      sponsorModel.ltSponsorBoards = clb.ltSponsorBoards;
      sponsorModel.ltSponsorOffers = clb.ltSponsorOffers;

      // Collect sponsor names
      sponsorModel.ltSponsorNames = new List<string>();
      foreach (CornerkickManager.Finance.Spons spns in MvcApplication.ckcore.fz.ltSponsoren) {
        sponsorModel.ltSponsorNames.Add(spns.name);
      }

      sponsorModel.ltSponsorBoardIds = new List<int>();
      for (int iS = 0; iS < sponsorModel.ltSponsorBoards.Count; iS++) {
        CornerkickManager.Finance.Sponsor spon = sponsorModel.ltSponsorBoards[iS];

        if (spon.iId >= MvcApplication.ckcore.fz.ltSponsoren.Count) {
          spon.iId = (byte)rnd.Next(1, MvcApplication.ckcore.fz.ltSponsoren.Count);
        }

        for (int iB = 0; iB < spon.nBoards; iB++) sponsorModel.ltSponsorBoardIds.Add(spon.iId);
      }

      sponsorModel.sEmblem = getClubEmblem(clb, "height: 100%; width: 100%; object-fit: contain");
      sponsorModel.sColorJersey = "rgb(" + clb.cl[0].R.ToString() + "," + clb.cl[0].G.ToString() + "," + clb.cl[0].B.ToString() + ")";

      return View(sponsorModel);
    }

    [HttpPost]
    public JsonResult SponsorSet(int iSponsorIndex)
    {
      CornerkickManager.Club clb = ckClub();
      if (clb == null) return Json(false, JsonRequestBehavior.AllowGet);

      string sCash = MvcApplication.ckcore.ui.setSponsor(ref clb, clb.ltSponsorOffers[iSponsorIndex]).ToString("N0", getCi());

      return Json(sCash, JsonRequestBehavior.AllowGet);
    }

    [HttpPost]
    public JsonResult getLtSponsorBoardIds()
    {
      CornerkickManager.Club clb = ckClub();
      if (clb == null) return Json(false, JsonRequestBehavior.AllowGet);

      List<int> ltSponsorBoardIds = new List<int>();
      foreach (CornerkickManager.Finance.Sponsor spon in clb.ltSponsorBoards) {
        for (int iB = 0; iB < spon.nBoards; iB++) ltSponsorBoardIds.Add(spon.iId);
      }

      return Json(ltSponsorBoardIds, JsonRequestBehavior.AllowGet);
    }

    public ActionResult getTableSponsorBoard()
    {
      CornerkickManager.Club club = ckClub();
      if (club == null) return Json(false, JsonRequestBehavior.AllowGet);

      //The table or entity I'm querying
      List<Models.DatatableEntrySponsorBoard> query = new List<Models.DatatableEntrySponsorBoard>();

      int iSpOffer = 0;
      bool bOffer = false;
      foreach (List<CornerkickManager.Finance.Sponsor> ltSponsor in new List<CornerkickManager.Finance.Sponsor>[] { club.ltSponsorBoards, club.ltSponsorOffers }) {
        for (int iS = 0; iS < ltSponsor.Count; iS++) {
          CornerkickManager.Finance.Sponsor spon = ltSponsor[iS];

          if (bOffer) iSpOffer++;

          if (spon.bMain) continue;

          Models.DatatableEntrySponsorBoard deSponsorBoard = new Models.DatatableEntrySponsorBoard();
          deSponsorBoard.bOffer = bOffer;
          deSponsorBoard.iId = spon.iId;
          if (bOffer) deSponsorBoard.iIndex = iSpOffer - 1;
          spon.iId = (byte)Math.Min(spon.iId, MvcApplication.ckcore.fz.ltSponsoren.Count - 1);
          deSponsorBoard.sName = MvcApplication.ckcore.fz.ltSponsoren[spon.iId].name;
          deSponsorBoard.sMoneyVicHome = spon.iMoneyVicHome.ToString("N0", getCi());
          deSponsorBoard.nBoards = spon.nBoards;
          deSponsorBoard.iYears = spon.iYears;

          query.Add(deSponsorBoard);
        }

        bOffer = true;
      }

      return Json(new { aaData = query.ToArray() }, JsonRequestBehavior.AllowGet);
    }

    //////////////////////////////////////////////////////////////////////////
    /// <summary>
    /// Statistic
    /// </summary>
    /// <param name="Statistic"></param>
    /// <returns></returns>
    //////////////////////////////////////////////////////////////////////////
    public ActionResult Statistic(Models.StatisticModel statisticModel)
    {
      statisticModel.ddlNations = new List<SelectListItem>();
      statisticModel.ddlNations.Add(new SelectListItem {
                                      Text = "Weltauswahl",
                                      Value = "-1",
                                      Selected = true
                                    }
      );
      for (int iN = 0; iN < MvcApplication.iNations.Length; iN++) {
        statisticModel.ddlNations.Add(new SelectListItem {
                                        Text  = CornerkickManager.Main.sLand[MvcApplication.iNations[iN]],
                                        Value = MvcApplication.iNations[iN].ToString()
                                      }
        );
      }

      statisticModel.ltsFormations = new List<SelectListItem>();
      for (int i = 0; i < MvcApplication.ckcore.ltFormationen.Count; i++) {
        statisticModel.ltsFormations.Add(new SelectListItem {
                                           Text = (i + 1).ToString() + " - " + MvcApplication.ckcore.ltFormationen[i].sName,
                                           Value = i.ToString(),
                                           Selected = i == 9
                                         }
        );
      }

      statisticModel.ddlLeagues = new List<SelectListItem>();

      statisticModel.ddlLeagues.Add(new SelectListItem {
                                      Text = "alle",
                                      Value = "-1"
                                    }
      );

      CornerkickManager.Club clbUser = ckClub();

      int iLand = 0;
      if (clbUser != null) iLand = clbUser.iLand;

      statisticModel.iLeague = iLand;
      for (int iC = 0; iC < MvcApplication.ckcore.ltCups.Count; iC++) {
        CornerkickManager.Cup cup = MvcApplication.ckcore.ltCups[iC];

        if (cup.iId != 1) continue; // If not league

        statisticModel.ddlLeagues.Add(new SelectListItem {
                                        Text = cup.sName,
                                        Value = cup.iId2.ToString(),
                                        Selected = iLand == cup.iId2
                                      }
        );
      }

      statisticModel.sPlayerSkillBest = new string[CornerkickManager.Player.sSkills.Length][];
      for (byte iS = 0; iS < CornerkickManager.Player.sSkills.Length; iS++) {
        if (iS == CornerkickGame.Player.iIndTrainingIxFoot) continue; // Both foot skill

        statisticModel.sPlayerSkillBest[iS] = new string[4]; // Skill name, player name, skill value, club

        CornerkickGame.Player plSkillBest = null;
        float fSkillBest = 0f;

        foreach (CornerkickGame.Player pl in MvcApplication.ckcore.ltPlayer) {
          if (pl.checkRetired()) continue;

          // Get position role
          byte iPos = 0;
          for (byte jPos = 1; jPos <= pl.fExperiencePos.Length; jPos++) {
            if (CornerkickGame.Tool.bPlayerMainPos(pl, iPos: jPos)) {
              iPos = jPos;
              break;
            }
          }

          float fSkillTmp = CornerkickGame.Tool.getSkillEff(pl, iS, iPos);
          if (fSkillTmp > fSkillBest) {
            plSkillBest = pl;
            fSkillBest = fSkillTmp;
          }
        }

        statisticModel.sPlayerSkillBest[iS][0] = CornerkickManager.Player.sSkills[iS];
        if (plSkillBest != null) {
          statisticModel.sPlayerSkillBest[iS][1] = plSkillBest.sName;
          statisticModel.sPlayerSkillBest[iS][2] = fSkillBest.ToString("0.000");
          string sClubName = "vereinslos";
          if (plSkillBest.iClubId >= 0) sClubName = MvcApplication.ckcore.ltClubs[plSkillBest.iClubId].sName;
          statisticModel.sPlayerSkillBest[iS][3] = sClubName;
        }
      }

      return View(statisticModel);
    }

    public JsonResult StatisticGetBest11(int iNat, int iF, bool bJouth = false)
    {
      Models.TeamModels.TeamData tD = new Models.TeamModels.TeamData();
      tD.ltPlayer         = new List<CornerkickGame.Player>();
      tD.ltPlayerAveSkill = new List<string>();
      tD.ltPlayerPos      = new List<byte>  ();
      tD.ltPlayerTeamname = new List<string>();
      tD.ltPlayerAge      = new List<string>();
      tD.ltPlayerNat      = new List<string>();

      tD.formation = MvcApplication.ckcore.ltFormationen[iF];

      for (byte iP = 0; iP < 11; iP++) {
        float fStrength = 0f;
        tD.ltPlayer        .Add(null);
        tD.ltPlayerPos     .Add(0);
        tD.ltPlayerAveSkill.Add(null);
        tD.ltPlayerTeamname.Add("vereinslos");
        tD.ltPlayerAge     .Add(null);
        tD.ltPlayerNat     .Add(null);

        byte iPosExact = CornerkickGame.Tool.getPosRole(MvcApplication.ckcore.ltFormationen[iF].ptPos[iP], MvcApplication.ckcore.game.ptPitch);
        byte iPos = CornerkickGame.Tool.getBasisPos(iPosExact);

        foreach (CornerkickGame.Player pl in MvcApplication.ckcore.ltPlayer) {
          if (pl.bRetire) continue;
          if (bJouth && pl.getAge(MvcApplication.ckcore.dtDatum) > 18f) continue;
          if (iNat >= 0 && pl.iNat1 != iNat) continue;

          // Check if club is nation
          if (pl.iClubId >= 0) {
            if (MvcApplication.ckcore.ltClubs[pl.iClubId].bNation) continue;
          }

          // Check if same player already in same role
          if (iPos > 0) {
            bool bSame = false;
            foreach (CornerkickGame.Player plSame in tD.ltPlayer) {
              if (plSame != null && plSame.iId == pl.iId && plSame.fExperiencePos[iPos - 1] > 0.999) {
                bSame = true;
                break;
              }
            }
            if (bSame) continue;
          }

          float fStrengthTmp = CornerkickGame.Tool.getAveSkill(pl, iPos);
          if (fStrengthTmp > fStrength) {
            tD.ltPlayer[iP] = pl.Clone(true);

            //tD.ltPlayer        [iP].ptPosDefault = MvcApplication.ckcore.ltFormationen[iF].ptPos[iP];
            tD.ltPlayer        [iP].iNr = (byte)(iP + 1);
            tD.ltPlayerAveSkill[iP] = fStrengthTmp.ToString("0.0");

            fStrength = fStrengthTmp;
          }
        }

        tD.ltPlayerPos[iP] = iPos;
        if (tD.ltPlayer[iP] != null) {
          if (tD.ltPlayer[iP].iClubId >= 0) tD.ltPlayerTeamname[iP] = MvcApplication.ckcore.ltClubs[tD.ltPlayer[iP].iClubId].sName;
          tD.ltPlayerAge[iP] = tD.ltPlayer[iP].getAge(MvcApplication.ckcore.dtDatum).ToString("0.0");
          tD.ltPlayerNat[iP] = CornerkickManager.Main.sLandShort[tD.ltPlayer[iP].iNat1];
        }
      }

      float[] fTeamAve11 = MvcApplication.ckcore.tl.getTeamAve(tD.ltPlayer, tD.formation, 11);
      tD.fTeamAveStrength = fTeamAve11[3];
      tD.fTeamAveAge      = fTeamAve11[4];

      return Json(tD, JsonRequestBehavior.AllowGet);
    }

    public ActionResult StatisticGetTableTeams(int iLand = -1)
    {
      //The table or entity I'm querying
      List<Models.DatatableEntryTeams> ltDeTeams = new List<Models.DatatableEntryTeams>();

      int iC = 0;
      foreach (CornerkickManager.Club clb in MvcApplication.ckcore.ltClubs) {
        if (clb.iLand < 0) continue;
        if (clb.bNation) continue;
        if (iLand >= 0 && iLand != clb.iLand) continue;

        float[] fAve = MvcApplication.ckcore.tl.getTeamAve(clb, bTeamValue: true);
        string sSkill = fAve[3].ToString("0.0");
        string sAge   = fAve[4].ToString("0.0");
        int    iVal   = (int)fAve[5];

        float[] fAve11 = MvcApplication.ckcore.tl.getTeamAve(clb, 11, bTeamValue: true);
        string sSkill11 = fAve11[3].ToString("0.0");
        string sAge11   = fAve11[4].ToString("0.0");
        int    iVal11   = (int)fAve11[5];

        CornerkickManager.Cup league = MvcApplication.ckcore.tl.getCup(1, clb.iLand, clb.iDivision);
        string sLeagueName = "-";
        if (league != null) sLeagueName = league.sName;

        ltDeTeams.Add(new Models.DatatableEntryTeams {
          iIx = iC,
          iTeamId = clb.iId,
          sTeamName = clb.sName,
          sTeamAveSkill = sSkill,
          sTeamAveAge = sAge,
          iTeamValueTotal = iVal,
          nPlayer = clb.ltPlayer.Count,
          sTeamAveSkill11 = sSkill11,
          sTeamAveAge11 = sAge11,
          iTeamValueTotal11 = iVal11,
          sLeague = sLeagueName
        });

        iC++;
      }

      return Json(new { aaData = ltDeTeams.ToArray() }, JsonRequestBehavior.AllowGet);
    }

    public ActionResult StatisticGetTransferTable()
    {
      //The table or entity I'm querying
      List<Models.PlayerModel.DatatableEntryClubHistory> ltDeClubHistory = new List<Models.PlayerModel.DatatableEntryClubHistory>();

      int iT = 1;
      foreach (CornerkickGame.Player pl in MvcApplication.ckcore.ltPlayer) {
        for (int iCh = 0; iCh < pl.ltClubHistory.Count; iCh++) {
          CornerkickGame.Player.ClubHistory ch = pl.ltClubHistory[iCh];

          if (ch.iTransferFee > 0) {
            if (ch.iClubId < 0 || ch.iClubId >= MvcApplication.ckcore.ltClubs.Count) {
              ch.iTransferFee = 0;
              ch.iClubId = -1;
              continue;
            }

            // Get name of new club
            string sClubTakeName = MvcApplication.ckcore.ltClubs[ch.iClubId].sName;

            // Get name of old club
            if (iCh > 0) {
              CornerkickGame.Player.ClubHistory chLast = pl.ltClubHistory[iCh - 1];

              // If no last club --> no transfer --> continue
              if (chLast.iClubId < 0 || chLast.iClubId >= MvcApplication.ckcore.ltClubs.Count) {
                ch.iTransferFee = 0;
                chLast.iClubId = -1;
                continue;
              }

              string sClubGiveName = MvcApplication.ckcore.ltClubs[chLast.iClubId].sName;

              ltDeClubHistory.Add(new Models.PlayerModel.DatatableEntryClubHistory {
                iIx = iT++,
                sPlayerName = pl.sName,
                sClubTakeName = sClubTakeName,
                sClubGiveName = sClubGiveName,
                sDt = ch.dt.ToString("d", getCi()),
                iValue = pl.getValueHistory(ch.dt) * 1000,
                iTransferFee = ch.iTransferFee
              });
            }
          }
        }
      }

      ltDeClubHistory = ltDeClubHistory.OrderByDescending(o => o.iTransferFee).ToList().GetRange(0, 20);
      for (int i = 0; i < ltDeClubHistory.Count; i++) {
        ltDeClubHistory[i].iIx = i + 1;
      }

      return Json(new { aaData = ltDeClubHistory.ToArray() }, JsonRequestBehavior.AllowGet);
    }

    //////////////////////////////////////////////////////////////////////////
    /// <summary>
    /// Mail
    /// </summary>
    /// <param name="Mail"></param>
    /// <returns></returns>
    //////////////////////////////////////////////////////////////////////////
    public ActionResult Mail(Models.UserModel mdUser)
    {
      mdUser.ltDdlUser = new List<SelectListItem>();

      mdUser.ltDdlUser.Add(new SelectListItem { Text = "-", Value = "-1" });

      // Positionen zu Dropdown Menü hinzufügen
      for (int iU = 0; iU < MvcApplication.ckcore.ltUser.Count; iU++) {
        CornerkickManager.User userTmp = MvcApplication.ckcore.ltUser[iU];
        if (userTmp.id == User.Identity.GetUserId()) continue;

        string sName = userTmp.sFirstname + " " + userTmp.sSurname;

        sName += " (" + userTmp.club.sName + ")";

        mdUser.ltDdlUser.Add(new SelectListItem { Text = sName, Value = iU.ToString() });
      }

      CornerkickManager.User user = ckUser();

      mdUser.ltUserMail = new List<CornerkickManager.Main.News>();

      if (user.ltNews != null) {
        foreach (CornerkickManager.Main.News news in user.ltNews) {
          if (news.iType == 99) mdUser.ltUserMail.Add(news);
        }
      }

      return View(mdUser);
    }

    public class DatatableEntryMail
    {
      public int iIndex { get; set; }
      public string sFromId { get; set; }
      public string sFrom { get; set; }
      public string sDt { get; set; }
      public string sFirstLine { get; set; }
      public string sText { get; set; }
      public bool bNew { get; set; }
    }

    public ActionResult MailGetList()
    {
      CornerkickManager.User usr = ckUser();

      //The table or entity I'm querying
      List<DatatableEntryMail> ltDeMail = new List<DatatableEntryMail>();

      foreach (MvcApplication.Mail mail in MvcApplication.ltMail) {
        int iIx = 0;
        if (mail.sIdTo.Equals(usr.id)) {
          string[] sTextSplit = mail.sText.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
          if (sTextSplit.Length < 1) continue;

          string sFrom = "-";
          if (string.IsNullOrEmpty(mail.sIdFrom)) {
            sFrom = "Admin";
          } else {
            CornerkickManager.User user = MvcApplication.ckcore.tl.getUserFromId(mail.sIdFrom);
            if (user == null) continue;

            sFrom = user.sFirstname + " " + user.sSurname;
            sFrom += " (" + user.club.sName + ")";
          }

          ltDeMail.Add(new DatatableEntryMail { iIndex = iIx, sFromId = mail.sIdFrom, sFrom = sFrom, sDt = mail.dt.ToString("d", getCi()) + " " + mail.dt.ToString("HH:mm:ss"), sFirstLine = sTextSplit[0], sText = mail.sText, bNew = mail.bNew });

          iIx++;
        }
      }
      /*
      string sDirMail = System.IO.Path.Combine(CornerkickManager.Main.sHomeDir, "mail");
      if (System.IO.Directory.Exists(sDirMail)) {
        System.IO.DirectoryInfo diMail = new System.IO.DirectoryInfo(sDirMail);

        int iIx = 0;
        foreach (var fileMail in diMail.GetFiles(usr.id + "_*.txt")) {
          string sContent = System.IO.File.ReadAllText(fileMail.FullName);
          string[] sContentSplit = sContent.Split(new[] { Environment.NewLine }, StringSplitOptions.None);

          string sHeader    = sContentSplit[0];
          string sFirstLine = sContentSplit[1];

          string sFromId = sHeader.Split()[1];
          string sDate   = sHeader.Split()[2];
          bool bNew = sHeader.Split()[3].Equals("true");

          DateTime dtMail = DateTime.ParseExact(sDate, "yyyyMMddHHmmss", CultureInfo.InvariantCulture);

          string sFrom = "-";
          if (string.IsNullOrEmpty(sFromId)) {
            sFrom = "Admin";
          } else {
            CornerkickManager.User user = MvcApplication.ckcore.tl.getUserFromId(sFromId);
            if (user == null) continue;

            sFrom = user.sFirstname + " " + user.sSurname;
            sFrom += " (" + user.club.sName + ")";
          }

          ltDeMail.Add(new DatatableEntryMail { iIndex = iIx, sFromId = sFromId, sFrom = sFrom, sDt = dtMail.ToString("d", getCi()) + " " + dtMail.ToString("HH:mm:ss"), sFirstLine = sFirstLine, sText = sContent.Replace(sHeader + Environment.NewLine, ""), bNew = bNew });

          iIx++;
        }
      }
      */

      return Json(new { aaData = ltDeMail.ToArray() }, JsonRequestBehavior.AllowGet);
    }

    [HttpPost]
    public JsonResult MailSend(int iTo, string sText)
    {
      if (iTo < 0) return Json("Error. Unknown user", JsonRequestBehavior.AllowGet);
      if (string.IsNullOrEmpty(sText)) return Json("Error. Nachricht leer", JsonRequestBehavior.AllowGet);

      CornerkickManager.User usr = ckUser();
      CornerkickManager.User usrTo = MvcApplication.ckcore.ltUser[iTo];

      /*
      string sDirMail = System.IO.Path.Combine(CornerkickManager.Main.sHomeDir, "mail");
      if (!System.IO.Directory.Exists(sDirMail)) System.IO.Directory.CreateDirectory(sDirMail);
      */

      MvcApplication.Mail mail = new MvcApplication.Mail();
      mail.dt = DateTime.Now;
      mail.bNew = true;
      mail.sIdTo = usrTo.id;
      mail.sIdFrom = usr.id;
      mail.sText = sText;

      if (MvcApplication.ltMail == null) MvcApplication.ltMail = new List<MvcApplication.Mail>();
      MvcApplication.ltMail.Add(mail);
      /*
      string sDateNow = DateTime.Now.ToString("yyyyMMddHHmmss");
      string sFilenameMail = usrTo.id + "_" + sDateNow + ".txt";
      using (System.IO.StreamWriter fileMail = new System.IO.StreamWriter(System.IO.Path.Combine(sDirMail, sFilenameMail))) {
        // Add header
        sText = usrTo.id + " " + usr.id + " " + sDateNow + " true" + Environment.NewLine + sText;
        fileMail.Write(sText);
        fileMail.Close();
      }
      */

      return Json("Nachricht an " + usrTo.sFirstname + " " + usrTo.sSurname + " gesendet!", JsonRequestBehavior.AllowGet);
    }

    [HttpPost]
    public JsonResult MailMarkRead(string sDate, string sFrom)
    {
      CornerkickManager.User user = ckUser();

      DateTime dtMail;
      if (!getDateTimeFromMailStr(sDate, out dtMail)) {
        MvcApplication.ckcore.tl.writeLog("Unable to mark email (user: " + user.id + ", date: " + sDate + ") as read", CornerkickManager.Main.sErrorFile);
        return Json(false, JsonRequestBehavior.AllowGet);
      }

      MvcApplication.Mail mail = getMail(user, dtMail);
      if (mail != null) mail.bNew = false;

      /*
      string sFileMail = getMailFilename(user, dtMail);
      string sFileMailFull = System.IO.Path.Combine(CornerkickManager.Main.sHomeDir, "mail", sFileMail);
      if (System.IO.File.Exists(sFileMailFull)) {
        string[] lines = System.IO.File.ReadAllLines(sFileMailFull);

        string[] sFirstLine = lines[0].Split();
        if (sFirstLine.Length > 3) {
          bool bRead = false;
          bool.TryParse(sFirstLine[3], out bRead);

          if (bRead) {
            lines[0] = lines[0].Replace("true", "false");
            try {
              System.IO.File.WriteAllLines(sFileMailFull, lines);
              return Json(true, JsonRequestBehavior.AllowGet);
            } catch {
              return Json(false, JsonRequestBehavior.AllowGet);
            }
          }
        }
      }
      */

      return Json(true, JsonRequestBehavior.AllowGet);
    }

    private bool getDateTimeFromMailStr(string sDate, out DateTime dtMail)
    {
      dtMail = new DateTime();

      string[] sDateSplit = sDate.Split();

      if (sDateSplit.Length < 2) return false;

      // First: parse date part of string with user culture info
      DateTime dtDate = new DateTime();
      if (!DateTime.TryParse(sDateSplit[0], getCi(), DateTimeStyles.None, out dtDate)) {
        MvcApplication.ckcore.tl.writeLog("Unable to parse email date: " + sDate, CornerkickManager.Main.sErrorFile);
        return false;
      }

      TimeSpan tsTime = new TimeSpan();
      if (!TimeSpan.TryParseExact(sDateSplit[1], "g", CultureInfo.InvariantCulture, TimeSpanStyles.AssumeNegative, out tsTime)) {
        MvcApplication.ckcore.tl.writeLog("Unable to parse email time: " + sDate, CornerkickManager.Main.sErrorFile);
        return false;
      }

      dtMail = dtDate.Add(tsTime);

      return true;
    }

    [HttpPost]
    public JsonResult MailDeleteTxt(string sDate, string sFrom)
    {
      CornerkickManager.User user = ckUser();
      if (user == null) return Json(false, JsonRequestBehavior.AllowGet);

      DateTime dtMail;
      if (!getDateTimeFromMailStr(sDate, out dtMail)) {
        MvcApplication.ckcore.tl.writeLog("Unable to delete email (user: " + user.id + ", date: " + sDate + ")", CornerkickManager.Main.sErrorFile);
        return Json(false, JsonRequestBehavior.AllowGet);
      }

      MvcApplication.Mail mail = getMail(user, dtMail);
      MvcApplication.ltMail.Remove(mail);

      /*
      string sMail = getMailFilename(user, dtMail);
      if (System.IO.File.Exists(sMail)) {
        try {
          System.IO.File.Delete(sMail);
        } catch {
          MvcApplication.ckcore.tl.writeLog("Unable to delete mail: " + sMail, CornerkickManager.Main.sErrorFile);
        }
      } else {
        MvcApplication.ckcore.tl.writeLog("Mail to delete '" + sMail + "' not found!", CornerkickManager.Main.sErrorFile);
      }
      */

      return Json(false, JsonRequestBehavior.AllowGet);
    }

    private static MvcApplication.Mail getMail(CornerkickManager.User user, DateTime dtMail)
    {
      for (int iM = 0; iM < MvcApplication.ltMail.Count; iM++) {
        MvcApplication.Mail mail = MvcApplication.ltMail[iM];
        if (mail.sIdTo.Equals(user.id) && Math.Abs((mail.dt - dtMail).TotalSeconds) < 1) return mail;
      }

      return null;
    }

    private static string getMailFilename(CornerkickManager.User user, DateTime dtMail)
    {
      return System.IO.Path.Combine(CornerkickManager.Main.sHomeDir, "mail", user.id + "_" + dtMail.ToString("yyyyMMddHHmmss") + ".txt");
    }

    public static int MailCountNewMails(CornerkickManager.User usr)
    {
      if (usr == null) return 0;
      if (MvcApplication.ltMail == null) return 0;

      int iMails = 0;

      for (int iM = 0; iM < MvcApplication.ltMail.Count; iM++) {
        MvcApplication.Mail mail = MvcApplication.ltMail[iM];
        if (mail.sIdTo.Equals(usr.id) && mail.bNew) iMails++;
      }

      /*
      string sDirMail = System.IO.Path.Combine(CornerkickManager.Main.sHomeDir, "mail");
      if (System.IO.Directory.Exists(sDirMail)) {
        System.IO.DirectoryInfo diMail = new System.IO.DirectoryInfo(sDirMail);

        foreach (var fileMail in diMail.GetFiles(usr.id + "_*.txt")) {
          string sContent = System.IO.File.ReadAllText(fileMail.FullName);
          string[] sContentSplit = sContent.Split(new[] { Environment.NewLine }, StringSplitOptions.None);

          string sHeader = sContentSplit[0];
          if (sHeader.Split().Length > 3) {
            bool bNew = sHeader.Split()[3].Equals("true");
            if (bNew) iMails++;
          }
        }
      }
      */

      return iMails;
    }

    [Authorize]
    public ActionResult WishList(Models.WishListModel mdWl)
    {
      ViewBag.Message = "Cornerkick Wish-list";

      mdWl.bAdmin = AccountController.checkUserIsAdmin(User.Identity.GetUserName());

      return View(mdWl);
    }

    public class DatatableWish
    {
      public int iIx { get; set; }
      public bool bVoted { get; set; } // If user has voted for this wish
      public string sTitle { get; set; }
      public string sDesc { get; set; }
      public string sOwner { get; set; }
      public int iComplexity { get; set; }
      public int iVotes { get; set; }
      public float fRank { get; set; }
      public float fProgress { get; set; }
      public string sDate { get; set; }
      public string sVerRelease { get; set; }
    }
    public JsonResult WishListGetWishes()
    {
      Models.WishListModel.ltWish = new List<Models.WishListModel.Wish>();
      List<DatatableWish> ltDtWish = new List<DatatableWish>();

      CornerkickManager.User usr = ckUser();

      var jss = new System.Web.Script.Serialization.JavaScriptSerializer();

      string sFileWl = System.IO.Path.Combine(CornerkickManager.Main.sHomeDir, "wishlist.json");
      if (System.IO.File.Exists(sFileWl)) {
        Models.WishListModel.WishJson wishJson = jss.Deserialize<Models.WishListModel.WishJson>(System.IO.File.ReadAllText(sFileWl));

        Models.WishListModel.ltWish = wishJson.Wish.OrderByDescending(wish => wish.ranking).ToList();

        for (int iW = 0; iW < Models.WishListModel.ltWish.Count; iW++) {
          DateTime dt = new DateTime();
          if (!string.IsNullOrEmpty(Models.WishListModel.ltWish[iW].date)) dt = DateTime.ParseExact(Models.WishListModel.ltWish[iW].date, "yyyyMMddHHmm", CultureInfo.InvariantCulture);

          CornerkickManager.User usrOwner = MvcApplication.ckcore.tl.getUserFromId(Models.WishListModel.ltWish[iW].owner);
          string sUsername = "unbekannt";
          if (usrOwner != null) sUsername = usrOwner.sFirstname + " " + usrOwner.sSurname;

          if (Models.WishListModel.ltWish[iW].votes == null) Models.WishListModel.ltWish[iW].votes = new List<string>();

          string sVer = "-";
          if (!string.IsNullOrEmpty(Models.WishListModel.ltWish[iW].version)) sVer = Models.WishListModel.ltWish[iW].version;
          if (!string.IsNullOrEmpty(Models.WishListModel.ltWish[iW].dateRel)) sVer += " (" + Models.WishListModel.ltWish[iW].dateRel + ")";

          bool bVoted = false;
          if (usr != null) bVoted = Models.WishListModel.ltWish[iW].votes.Contains(usr.id);

          ltDtWish.Add(new DatatableWish() { iIx = iW + 1, sTitle = Models.WishListModel.ltWish[iW].title, sDesc = Models.WishListModel.ltWish[iW].description, sOwner = sUsername, iComplexity = Models.WishListModel.ltWish[iW].complexity, iVotes = Models.WishListModel.ltWish[iW].votes.Count, fRank = Models.WishListModel.ltWish[iW].ranking, sDate = dt.ToString("d", getCi()), fProgress = Models.WishListModel.ltWish[iW].progress, sVerRelease = sVer, bVoted = bVoted });
        }
      }

      return Json(new { aaData = ltDtWish }, JsonRequestBehavior.AllowGet);
    }

    public ActionResult WishListAddWish(string sTitle, string sDesc)
    {
      CornerkickManager.User user = ckUser();

      if (user == null) return Json("Error", JsonRequestBehavior.AllowGet);
      if (string.IsNullOrEmpty(sTitle)) return Json("Error: Titel darf nicht leer sein!", JsonRequestBehavior.AllowGet);
      if (string.IsNullOrEmpty(sDesc))  return Json("Error: Beschreibung darf nicht leer sein!", JsonRequestBehavior.AllowGet);

      Models.WishListModel.Wish wish = new Models.WishListModel.Wish();
      wish.title = sTitle;
      wish.description = sDesc;
      wish.owner = user.id;
      wish.date = DateTime.Now.ToString("yyyyMMddHHmm");

      if (Models.WishListModel.ltWish == null) Models.WishListModel.ltWish = new List<Models.WishListModel.Wish>();
      Models.WishListModel.ltWish.Add(wish);

      writeWishesToJsonFile(Models.WishListModel.ltWish);

      return Json(true, JsonRequestBehavior.AllowGet);
    }

    public ActionResult WishListVoteWish(int iWishIx)
    {
      CornerkickManager.User user = ckUser();
      if (user == null) return Json("Error", JsonRequestBehavior.AllowGet);

      if (Models.WishListModel.ltWish == null) return Json("Error", JsonRequestBehavior.AllowGet);
      if (iWishIx > Models.WishListModel.ltWish.Count) return Json("Error", JsonRequestBehavior.AllowGet);

      // Remove vote from all wishes
      for (int iW = 0; iW < Models.WishListModel.ltWish.Count; iW++) {
        Models.WishListModel.Wish wish = Models.WishListModel.ltWish[iW];

        if (wish.votes == null) wish.votes = new List<string>();

        for (int iV = 0; iV < wish.votes.Count; iV++) {
          wish.votes.Remove(user.id);
        }
      }

      Models.WishListModel.ltWish[iWishIx - 1].votes.Add(user.id);

      // Recalculate ranking
      calculateWishRating(Models.WishListModel.ltWish);

      writeWishesToJsonFile(Models.WishListModel.ltWish);

      return Json(true, JsonRequestBehavior.AllowGet);
    }

    private void calculateWishRating(List<Models.WishListModel.Wish> ltWish)
    {
      for (int iW = 0; iW < ltWish.Count; iW++) {
        Models.WishListModel.Wish wish = ltWish[iW];

        if (wish.votes == null) wish.votes = new List<string>();

        if      (wish.complexity <= 0.1)   wish.ranking = 0f;
        else if (wish.progress   >  0.999) wish.ranking = -1f;
        else                               wish.ranking = wish.votes.Count / (float)wish.complexity;
      }
    }

    private void writeWishesToJsonFile(List<Models.WishListModel.Wish> ltWish)
    {
      string sFileWl = System.IO.Path.Combine(CornerkickManager.Main.sHomeDir, "wishlist.json");

      if (System.IO.File.Exists(sFileWl)) {
        // Create backup
        try {
          string sFileWlBck = sFileWl + ".bck";

          // Delete backup file if exist
          if (System.IO.File.Exists(sFileWlBck)) {
            try {
              System.IO.File.Delete(sFileWlBck);
            } catch {
              MvcApplication.ckcore.tl.writeLog("Unable to delete wishlist backup file: " + sFileWlBck, CornerkickManager.Main.sErrorFile);
            }
          }

          // Create backup file
          System.IO.File.Move(sFileWl, sFileWl + ".bck");
        } catch {
          MvcApplication.ckcore.tl.writeLog("Unable to create backup of wishlist: " + sFileWl, CornerkickManager.Main.sErrorFile);
        }

        // Delete current wishlist file
        try {
          System.IO.File.Delete(sFileWl);
        } catch {
          MvcApplication.ckcore.tl.writeLog("Unable to delete wishlist file: " + sFileWl, CornerkickManager.Main.sErrorFile);
        }
      }

      string sJsonWishes = "";
      try {
        Models.WishListModel.WishJson wishJson = new Models.WishListModel.WishJson();
        wishJson.Wish = ltWish;
        sJsonWishes = JsonConvert.SerializeObject(wishJson);
      } catch {
        MvcApplication.ckcore.tl.writeLog("Unable to serialize wishlist");
        return;
      }

      // Write string to file
      try {
        System.IO.File.WriteAllText(sFileWl, sJsonWishes);
      } catch (Exception e) {
        MvcApplication.ckcore.tl.writeLog("Unable to write wishlist to file: " + sFileWl + Environment.NewLine + e.Message, CornerkickManager.Main.sErrorFile);
      }
    }

    public ActionResult WishListUpdateWish(int iWishIx, string sComplexity, string sProgress, string sVer, string sDateRel)
    {
      if (Models.WishListModel.ltWish == null) return Json("Error", JsonRequestBehavior.AllowGet);
      if (iWishIx > Models.WishListModel.ltWish.Count) return Json("Error", JsonRequestBehavior.AllowGet);

      int iComplex = -1;
      if (!string.IsNullOrEmpty(sComplexity)) int  .TryParse(sComplexity, out iComplex);

      float fProgress = -1f;
      if (!string.IsNullOrEmpty(sProgress))   float.TryParse(sProgress,   out fProgress);

      if (iComplex  >= 0) Models.WishListModel.ltWish[iWishIx - 1].complexity = iComplex;
      if (fProgress >= 0) Models.WishListModel.ltWish[iWishIx - 1].progress   = fProgress;
      if (!string.IsNullOrEmpty(sVer))     Models.WishListModel.ltWish[iWishIx - 1].version = sVer;
      if (!string.IsNullOrEmpty(sDateRel)) Models.WishListModel.ltWish[iWishIx - 1].dateRel = sDateRel;
      if (fProgress > 0.999f) Models.WishListModel.ltWish[iWishIx - 1].votes.Clear();

      calculateWishRating  (Models.WishListModel.ltWish);
      writeWishesToJsonFile(Models.WishListModel.ltWish);

      return Json(true, JsonRequestBehavior.AllowGet);
    }

    //////////////////////////////////////////////////////////////////////////
    /// <summary>
    /// User
    /// </summary>
    /// <param name="Statistic"></param>
    /// <returns></returns>
    //////////////////////////////////////////////////////////////////////////
    public ActionResult UserView(Models.UserModel mdUser)
    {
      mdUser.usr = ckUser();

      return View(mdUser);
    }

    public int compareDates(DateTime dt)
    {
      CornerkickManager.Club club = ckClub();
      if (club == null) return 0;

      List<CornerkickGame.Game.Data> ltGd = MvcApplication.ckcore.tl.getNextGames(club, MvcApplication.ckcore.dtDatum, false);

      foreach (CornerkickGame.Game.Data gd in ltGd) {
        /*
        double fTotH = (dt - gd.dt).TotalHours;
        if (fTotH > 0.0 && fTotH < +4) return -2;
        if (fTotH < 0.0 && fTotH > -4) return -2;
        */
        if (gd.dt.Date.Equals(dt.Date)) return -2;
        if (Math.Abs((dt - gd.dt).TotalHours) < 8) return -2;
      }

      return dt.CompareTo(MvcApplication.ckcore.dtDatum);
    }

    // 
    // GET: /HelloWorld/Welcome/ 
    [Authorize]
    public string Print(string s)
    {
      return s;
    }

    // GET: Member
    [Authorize]
    public ActionResult Saison()
    {
      return View();
    }

  }
}