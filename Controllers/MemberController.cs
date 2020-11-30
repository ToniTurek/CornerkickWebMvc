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
    internal static bool[] bHideEocInfo; // Flag if end of contract info will be displayed

    public class Tutorial
    {
      public bool bShow;
      public int iLevel;
    }
    public static Tutorial[] ttUser; // User tutorial

    public static void initialiteTutorial()
    {
      // Set length of tutorial class array
      ttUser = new Tutorial[MvcApplication.ckcore.ltUser.Count];
      for (int iU = 0; iU < ttUser.Length; iU++) {
        // Initialize tutorial class
        ttUser[iU] = new Tutorial() { bShow = true, iLevel = 0 };

        // Get user info
        CornerkickManager.User usr = MvcApplication.ckcore.ltUser[iU];

        if (usr.lti != null) {
          if (usr.lti.Count > 3) ttUser[iU].bShow  = usr.lti[3] > 0;
          if (usr.lti.Count > 4) ttUser[iU].iLevel = usr.lti[4];
        }
      }
    }

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
    [Authorize]
    public ActionResult SwitchClubNationView()
    {
      SwitchClubNation();

      return RedirectToAction("Desk");
    }

    [HttpGet]
    public JsonResult SetTutorialLevel(bool bShow, int iLevel)
    {
      CornerkickManager.User usr = ckUser();
      if (usr == null) return Json(false, JsonRequestBehavior.AllowGet);

      if (ttUser != null) {
        int iUserIx = MvcApplication.ckcore.ltUser.IndexOf(usr);
        if (iUserIx >= 0 && iUserIx < ttUser.Length) {
          ttUser[iUserIx].bShow = bShow;
          ttUser[iUserIx].iLevel = iLevel;
        }
      }

      if (usr.lti == null) usr.lti = new List<int>();
      while (usr.lti.Count < 5) usr.lti.Add(0);

      usr.lti[3] = bShow ? 1 : 0;
      usr.lti[4] = iLevel;

      return Json(true, JsonRequestBehavior.AllowGet);
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

      // Show todays balance?
      desk.bShowBalanceToday = true;
      if (usr.lti != null) {
        if (usr.lti.Count > 2) desk.bShowBalanceToday = usr.lti[2] > 0;
      }

      // Assign tutorial
      if (ttUser != null) {
        int iUserIx = MvcApplication.ckcore.ltUser.IndexOf(usr);
        if (iUserIx >= 0 && iUserIx < ttUser.Length) {
          desk.tutorial = ttUser[iUserIx];
          if (desk.tutorial.iLevel == 31) desk.tutorial.iLevel = 32;
        }
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
      List<CornerkickManager.Cup.TableItem> ltTbl = league.getTable();
      int iPlaceLeague = 1;
      int iGms = 0;
      foreach (CornerkickManager.Cup.TableItem tbl in ltTbl) {
        if (tbl.iId == club.iId) {
          iGms = tbl.iWDL[0] + tbl.iWDL[1] + tbl.iWDL[2];
          break;
        }
        iPlaceLeague++;
      }
      if (iGms > 0) desk.sTabellenplatz = iPlaceLeague.ToString() + ". Platz nach " + iGms.ToString() + " von " + ((ltTbl.Count - 1) * 2).ToString() + " Spielen";

      // Nat. cup round
      desk.sPokalrunde = "-";
      CornerkickManager.Cup cup = MvcApplication.ckcore.tl.getCup(2, club.iLand);
      if (cup != null) {
        if (cup.ltMatchdays != null) {
          if (cup.ltMatchdays.Count > 0) {
            if (cup.ltMatchdays[0].ltGameData != null) {
              int nPartFirstRound = cup.ltClubs[0].Count;

              if (nPartFirstRound > 0) {
                int nRound = cup.getKoRound(nPartFirstRound);
                int iMdClub = Math.Max(cup.getMatchdays(club), 0);
                int iMdCurr = cup.getMatchday(MvcApplication.ckcore.dtDatum); // Current matchday

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

      // Gold/Silver/Bronze cup round
      foreach (int iCupId in new int[] { MvcApplication.iCupIdBronze, MvcApplication.iCupIdSilver, MvcApplication.iCupIdGold  }) {
        CornerkickManager.Cup cupInternat = MvcApplication.ckcore.tl.getCup(iCupId);

        if (cupInternat.checkClubInCup(club)) {
          int iMd = cupInternat.getMatchday(MvcApplication.ckcore.dtDatum);
          string sText = "";

          int iClubsTotal = cupInternat.getClubsTotal();
          if (iMd < 6) {
            sText = "Gruppenphase, " + cupInternat.getPlace(club, iMd, bGroupPhaseOnly: true).ToString() + ". Platz";
          } else {
            int iPlace = cupInternat.getPlace(club, iMd);

            if (iPlace > 9) {
              sText = "ausgeschieden (Gruppenphase, " + cupInternat.getPlace(club, iMd, bGroupPhaseOnly: true).ToString() + ". Platz)";
            } else {
              if (iPlace > 1) {
                byte iKoRound = cupInternat.getKoRound(iPlace);
                int iMdClub = Math.Max(cupInternat.getMatchdays(club), 0);

                if (iMdClub < iMd) sText = "ausgeschieden (" + CornerkickManager.Main.sCupRound[iKoRound - 1] + ")";
                else               sText = CornerkickManager.Main.sCupRound[iKoRound - 1];
              } else {
                sText = "gewonnen";
              }
            }
          }

          if      (iCupId == MvcApplication.iCupIdGold)   desk.sGoldCupRound   = sText;
          else if (iCupId == MvcApplication.iCupIdSilver) desk.sSilverCupRound = sText;
          else if (iCupId == MvcApplication.iCupIdBronze) desk.sBronzeCupRound = sText;
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
      float[] fTeamAve   = CornerkickManager.Tool.getTeamAve(club, MvcApplication.ckcore.dtDatum, MvcApplication.ckcore.dtSeasonEnd);
      float[] fTeamAve11 = CornerkickManager.Tool.getTeamAve(club, MvcApplication.ckcore.dtDatum, MvcApplication.ckcore.dtSeasonEnd, ptPitch: MvcApplication.ckcore.game.ptPitch, iPlStop: 11);
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
      public string sHeader { get; set; }
      public string sImg { get; set; }
    }

    static readonly byte[] iNewspaperTypes = new byte[] { CornerkickManager.Main.iNewsTypeNewYear, CornerkickManager.Main.iNewsTypeCupWin };
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

        if (iNewspaperTypes.Contains(news.iType)) continue; // No newspaper news

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
          sN = sN.Replace("WM",               "<a href=\"/Member/CupWc\">WM</a>");
          sN = sN.Replace("Testspiel",        "<a href=\"/Member/Calendar\">Testspiel</a>");
          sN = sN.Replace("Transferangebot",  "<a href=\"/Member/Transfer\">Transferangebot</a>");
          sN = sN.Replace("wählen Sie",       "<a href=\"/Member/Transfer\">wählen Sie</a>");

          for (int iNat = 0; iNat < MvcApplication.iNations.Length; iNat++) {
            string sReplace = "Pokal " + CornerkickManager.Main.sLand[MvcApplication.iNations[iNat]];
            sN = sN.Replace(sReplace, "<a href=\"/Member/Cup\">" + sReplace + "</a>");
          }
          foreach (CornerkickManager.Player pl in clb.ltPlayer) {
            if (sN.Contains(pl.plGame.sName)) {
              sN = sN.Replace(pl.plGame.sName, "<a href=\"/Member/PlayerDetails?i=" + pl.plGame.iId.ToString() + "\" target = \"\">" + pl.plGame.sName + "</a>");
              break;
            }
          }

          foreach (CornerkickManager.Player pl in clb.ltPlayerJouth) {
            if (sN.Contains(pl.plGame.sName)) {
              sN = sN.Replace(pl.plGame.sName, "<a href=\"/Member/PlayerDetails?i=" + pl.plGame.iId.ToString() + "\" target = \"\">" + pl.plGame.sName + "</a>");
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

    [HttpGet]
    public JsonResult DeskGetNewspaper()
    {
      CornerkickManager.User usr = ckUser();
      if (usr == null) return Json(null, JsonRequestBehavior.AllowGet);
      if (usr.ltNews == null) return Json(null, JsonRequestBehavior.AllowGet);

      CornerkickManager.Club clb = ckClub();

      List<DatatableNews> ltNews = new List<DatatableNews>();

      List<CornerkickManager.Main.News> ltCkNews = new List<CornerkickManager.Main.News>(usr.ltNews);
      foreach (CornerkickManager.Main.News news0 in MvcApplication.ckcore.ltUser[0].ltNews) {
        if (news0.iType >= 200) ltCkNews.Add(news0);
      }

      try {
        for (int iN = ltCkNews.Count - 1; iN >= 0; iN--) {
          CornerkickManager.Main.News news = ltCkNews[iN];

          if (iNewspaperTypes.Contains(news.iType) || news.iType >= 200) {
            if ((MvcApplication.ckcore.dtDatum - news.dt).TotalDays > 7) continue;

            string sN = news.sText;

            if (news.iType == CornerkickManager.Main.iNewsTypeNewYear) {
              sN = MvcApplication.ckcore.dtDatum.Year.ToString() + "!#" + sN;
              news.iId = -1;
            } else if (news.iType == CornerkickManager.Main.iNewsTypeCupWin) {
              sN = "Was für ein Erfolg!#" + sN;
            }

            CornerkickGame.Player pl = null;
            if (news.iType == 200 && news.iId >= 0 && news.iId < MvcApplication.ckcore.ltPlayer.Count) {
              pl = MvcApplication.ckcore.ltPlayer[news.iId].plGame;
              if (pl != null) sN = sN.Replace(pl.sName, "<a href=\"/Member/PlayerDetails?i=" + pl.iId.ToString() + "\" target = \"\">" + pl.sName + "</a>");
            }

            // Check for identical news already added
            bool bSameNews = false;
            foreach (DatatableNews dtnTmp in ltNews) {
              if (dtnTmp.sText.Equals(sN)) {
                bSameNews = true;
                break;
              }
            }
            if (bSameNews) continue;

            DatatableNews dtn = new DatatableNews();
            dtn.iId = news.iId;
            dtn.sDate = news.dt.ToString("d", getCi()) + " " + news.dt.ToString("t", getCi());
            dtn.sText = sN;
            dtn.iType = news.iType;
            dtn.sHeader = news.sFromId;

            if (news.iId >= 0) {
              if (news.iType == 71) {
                dtn.sImg = getClubEmblem(MvcApplication.ckcore.ltClubs[news.iId], sStyle: "width: 100%", bTiny: true);
              } else {
                dtn.sImg = getPlayerPortrait(MvcApplication.ckcore.ltPlayer[news.iId], sStyle: "width: 100%", bSmall: true);
              }
            }

            ltNews.Add(dtn);
          }
        }
      } catch (Exception e) {
        Console.WriteLine(e.Message);
      }

      JsonSerializerSettings _jsonSetting = new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore };

      return Json(ltNews.ToArray(), JsonRequestBehavior.AllowGet);
      //return Content(JsonConvert.SerializeObject(ltNews.ToArray(), _jsonSetting), "application/json");
      //return Json(new { aaData = ltNews.ToArray() }, JsonRequestBehavior.AllowGet);
    }

    [HttpGet]
    public JsonResult DeskGetEndingContractsInfo()
    {
      CornerkickManager.Club clb = ckClub();
      if (clb == null) return Json(null, JsonRequestBehavior.AllowGet);
      if (clb.bNation) return Json(null, JsonRequestBehavior.AllowGet);

      // Check hide info flag
      CornerkickManager.User usr = ckUser();
      int iUserIx = MvcApplication.ckcore.ltUser.IndexOf(usr);

      if (ttUser != null) {
        if (iUserIx >= 0 && iUserIx < bHideEocInfo.Length) {
          if (bHideEocInfo[iUserIx]) return Json(null, JsonRequestBehavior.AllowGet);
        }
      }

      // Return if before december
      if (MvcApplication.ckcore.dtDatum.Year < MvcApplication.ckcore.dtSeasonEnd.Year && MvcApplication.ckcore.dtDatum.Month < 12) return Json(null, JsonRequestBehavior.AllowGet);

      string sInfo = "";
      foreach (CornerkickManager.Player pl in clb.ltPlayer) {
        if (CornerkickManager.PlayerTool.checkIfContractIsEnding(pl, MvcApplication.ckcore.dtSeasonEnd, MvcApplication.ckcore.dtSeasonEnd)) sInfo += pl.plGame.sName + ", ";
      }
      foreach (CornerkickManager.Player plJ in clb.ltPlayerJouth) {
        if (CornerkickManager.PlayerTool.checkIfContractIsEnding(plJ, MvcApplication.ckcore.dtSeasonEnd, MvcApplication.ckcore.dtSeasonEnd)) sInfo += plJ.plGame.sName + ", ";
      }

      if (!string.IsNullOrEmpty(sInfo)) {
        if (sInfo.EndsWith(", ")) sInfo = sInfo.Remove(sInfo.Length - 2, 2);

        string sWhen = "sofort";
        if (MvcApplication.ckcore.dtDatum.Year < MvcApplication.ckcore.dtSeasonEnd.Year) sWhen = "nächstem Jahr";
        sInfo = "ACHTUNG! Folgende Spieler besitzen einen auslaufenden Vertrag und können ab " + sWhen + " von einem anderen Verein abgeworben werden:" + Environment.NewLine + sInfo;

        // Set flag to true to not show info again
        if (iUserIx >= 0 && iUserIx < bHideEocInfo.Length) bHideEocInfo[iUserIx] = true;
      }

      return Json(sInfo, JsonRequestBehavior.AllowGet);
    }

    public ContentResult GetLastGames()
    {
      CornerkickManager.Club club = ckClub();
      if (club == null) return Content("", "application/json");

      List<Models.DataPointGeneral>[] dataPoints = new List<Models.DataPointGeneral>[5];
      dataPoints[0] = new List<Models.DataPointGeneral>(); // League
      dataPoints[1] = new List<Models.DataPointGeneral>(); // Nat. cup
      dataPoints[2] = new List<Models.DataPointGeneral>(); // Gold/Silver/Bronze Cup
      dataPoints[3] = new List<Models.DataPointGeneral>(); // Testgame
      dataPoints[4] = new List<Models.DataPointGeneral>(); // National Team

      List<CornerkickGame.Game.Data> ltGameData = MvcApplication.ckcore.tl.getNextGames(club, MvcApplication.ckcore.dtDatum, false, true);
      int iLg = 0;
      foreach (CornerkickGame.Game.Data gs in ltGameData) {
        if (gs.iGameType < 1 || gs.iGameType == MvcApplication.iCupIdTestgame) continue; // Testgame
        if (gs.team[0].iTeamId == 0 && gs.team[1].iTeamId == 0) continue;

        CornerkickManager.Club clubH = null;
        if (gs.team[0].iTeamId >= 0 && gs.team[0].iTeamId < MvcApplication.ckcore.ltClubs.Count) clubH = MvcApplication.ckcore.ltClubs[gs.team[0].iTeamId];
        CornerkickManager.Club clubA = null;
        if (gs.team[1].iTeamId >= 0 && gs.team[1].iTeamId < MvcApplication.ckcore.ltClubs.Count) clubA = MvcApplication.ckcore.ltClubs[gs.team[1].iTeamId];

        string sDesc = gs.dt.ToString("d", getCi()) + "</br>";
        if (clubH != null && clubA != null) sDesc += clubH.sName + " - " + clubA.sName + "</br>";
        sDesc += CornerkickManager.UI.getResultString(gs);

        int iGameType = 0;
        if      (gs.iGameType == MvcApplication.iCupIdNatCup) iGameType = 1;
        else if (gs.iGameType == MvcApplication.iCupIdGold || gs.iGameType == MvcApplication.iCupIdSilver || gs.iGameType == MvcApplication.iCupIdBronze) iGameType = 2;
        else if (gs.iGameType == MvcApplication.iCupIdTestgame) iGameType = 3;
        else if (gs.iGameType == MvcApplication.iCupIdWc) iGameType = 4;

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

    [HttpGet]
    public JsonResult DeskGetBalanceToday()
    {
      CornerkickManager.Club clb = ckClub();
      if (clb == null) return Json(false, JsonRequestBehavior.AllowGet);

      List<CornerkickManager.Finance.Account> ltBalanceToday = new List<CornerkickManager.Finance.Account>();
      foreach (CornerkickManager.Finance.Account acc in clb.ltAccount) {
        if (acc.dt.Date.Equals(MvcApplication.ckcore.dtDatum.Date)) {
          ltBalanceToday.Add(acc);
        }
      }

      return Json(ltBalanceToday, JsonRequestBehavior.AllowGet);
    }

    public void DeskSetBalanceTodayDialog(bool bOn)
    {
      CornerkickManager.User usr = Controllers.MemberController.ckUserStatic(User);
      if (usr == null) return;

      if (usr.lti == null) usr.lti = new List<int>();
      while (usr.lti.Count < 3) usr.lti.Add(0);

      usr.lti[2] = bOn ? 1 : 0;
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
        int iMd = cup.getMatchday(MvcApplication.ckcore.dtDatum);
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

      CornerkickManager.Player.TrainingHistory trHistCurrent = new CornerkickManager.Player.TrainingHistory();
      trHistCurrent.dt = MvcApplication.ckcore.dtDatum;
      trHistCurrent.fKFM = CornerkickManager.Tool.getTeamAve(clb, MvcApplication.ckcore.dtDatum, MvcApplication.ckcore.dtSeasonEnd);

      List<Models.DataPointGeneral>[][] dataPoints = new List<Models.DataPointGeneral>[2][];
      dataPoints[0] = new List<Models.DataPointGeneral>[3];
      dataPoints[1] = new List<Models.DataPointGeneral>[3];

      for (byte j = 0; j < dataPoints[0].Length; j++) {
        dataPoints[0][j] = new List<Models.DataPointGeneral>();

        for (int i = 0; i < clb.ltTrainingHist.Count; i++) {
          CornerkickManager.Player.TrainingHistory trHist = clb.ltTrainingHist[i];

          if (trHist.dt.CompareTo(MvcApplication.ckcore.dtDatum.AddDays(-7)) >  0 &&
              trHist.dt.CompareTo(MvcApplication.ckcore.dtDatum)             <= 0) {
            long iDate = convertDateTimeToTimestamp(trHist.dt);

            string sTrainingName = "";
            if (tsTraining.Contains(trHist.dt.TimeOfDay)) {
              CornerkickManager.PlayerTool.Training tr = CornerkickManager.PlayerTool.getTraining(trHist.iType, MvcApplication.ckcore.plt.ltTraining);
              sTrainingName = tr.sName + " (Start)";
            } else if (tsTraining.Contains(trHist.dt.AddMinutes(-90).TimeOfDay) && i > 0) {
              CornerkickManager.PlayerTool.Training trLast = CornerkickManager.PlayerTool.getTraining(clb.ltTrainingHist[i - 1].iType, MvcApplication.ckcore.plt.ltTraining);
              sTrainingName = trLast.sName + " (Ende)";
            }

            dataPoints[0][j].Add(new Models.DataPointGeneral(iDate, trHist.fKFM[j], z: sTrainingName));
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
            camp.dtDeparture = MvcApplication.ckcore.dtDatum.AddDays(-1);
            camp.dtReturn    = MvcApplication.ckcore.dtDatum.AddDays(+8);
            break;
          }
        }

        // Clone list of player in club
        List<CornerkickManager.Player> ltPlayerTrExp = new List<CornerkickManager.Player>();
        foreach (CornerkickManager.Player pl in clb.ltPlayer) ltPlayerTrExp.Add(pl.Clone());

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
            CornerkickManager.Player plTmp = ltPlayerTrExp[iP];
            CornerkickManager.PlayerTool.Training tr = CornerkickManager.PlayerTool.getTraining(tu.iType, MvcApplication.ckcore.plt.ltTraining);
            CornerkickManager.PlayerTool.doTraining(ref plTmp,
                                                    tr,
                                                    MvcApplication.ckcore.plt.ltTraining,
                                                    clb.staff.iCondiTrainer,
                                                    clb.staff.iPhysio,
                                                    clb.buildings.bgGym.iLevel,
                                                    clb.buildings.bgSpa.iLevel,
                                                    tu.dt,
                                                    usr,
                                                    iTrainingPerDay: 3,
                                                    ltPlayerTeam: ltPlayerTrExp,
                                                    campBooking: camp,
                                                    bJouth: false,
                                                    bNoInjuries: true,
                                                    ltTrRule: clb.training.ltRule);
          }

          // ... get training history data
          CornerkickManager.Player.TrainingHistory trHistExp = new CornerkickManager.Player.TrainingHistory();
          trHistExp.dt   = tu.dt;
          trHistExp.fKFM = CornerkickManager.Tool.getTeamAve(ltPlayerTrExp, MvcApplication.ckcore.dtDatum, MvcApplication.ckcore.dtSeasonEnd);
          trHistExp.iType = tu.iType;

          // ... add training history data to dataPoints
          for (byte j = 0; j < dataPoints[1].Length; j++) {
            long iDate = convertDateTimeToTimestamp(trHistExp.dt);
            CornerkickManager.PlayerTool.Training tr = CornerkickManager.PlayerTool.getTraining(trHistExp.iType, MvcApplication.ckcore.plt.ltTraining);
            dataPoints[1][j].Add(new Models.DataPointGeneral(iDate, trHistExp.fKFM[j], z: tr.sName));
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
      foreach (CornerkickManager.Player pl in clb.ltPlayer) {
        fFAve[0] += (pl.fSkillTraining[ 0] + pl.fSkillTraining[16])                                                / 2f; // Speed + acceleration
        fFAve[1] += (pl.fSkillTraining[ 1] + pl.fSkillTraining[ 2])                                                / 2f; // Technik + Dribbling
        fFAve[2] +=  pl.fSkillTraining[ 3];                                                                              // Zweikampf
        fFAve[3] += (pl.fSkillTraining[ 4] + pl.fSkillTraining[ 5] + pl.fSkillTraining[ 6] + pl.fSkillTraining[7]) / 4f; // Abspiel
        fFAve[4] += (pl.fSkillTraining[ 8] + pl.fSkillTraining[ 9] + pl.fSkillTraining[10])                        / 3f; // Abschluss
        fFAve[5] += (pl.fSkillTraining[11] + pl.fSkillTraining[12])                                                / 2f; // Standards
        fFAve[6] += (pl.fSkillTraining[13] + pl.fSkillTraining[14] + pl.fSkillTraining[15])                        / 3f; // TW

        if (pl.plGame.fExperiencePos[0] > 0.999) nPlKeeper++;
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

      // Tutorial
      if (ttUser != null) {
        int iUserIx = MvcApplication.ckcore.ltUser.IndexOf(user);
        if (iUserIx >= 0 && iUserIx < ttUser.Length) {
          team.tutorial = ttUser[iUserIx];
          if (team.tutorial.iLevel == 2) team.tutorial.iLevel = 3;
        }
      }

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

      foreach (CornerkickManager.Player pl in club.ltPlayer) {
        Models.TeamModels.ltPlayer.Add(pl.plGame);
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

      CornerkickManager.Player pl = clb.ltPlayer[fromPosition - 1];
      if (!pl.plGame.bPlayed) {
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

      CornerkickManager.Player pl1 = club.ltPlayer[jPosMin];
      CornerkickManager.Player pl2 = club.ltPlayer[jPosMax];

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
            if (user.game.data.iGameType >                                                  0 &&
                user.game.data.iGameType <= user.game.player[iHA][jPosMin].iSuspension.Length &&
                user.game.player[iHA][jPosMin].iSuspension[user.game.data.iGameType - 1] > 0) return Json(Models.TeamModels.ltPlayer, JsonRequestBehavior.AllowGet); // ... player out is suspended

            CornerkickManager.Player pl = CornerkickManager.PlayerTool.getPlayerFromId(user.game.player[iHA][jPosMax].iId, club.ltPlayer);
            if (!club.bNation && CornerkickManager.PlayerTool.atNationalTeam(pl, MvcApplication.ckcore.ltClubs)) return Json(Models.TeamModels.ltPlayer, JsonRequestBehavior.AllowGet); // ... player in is at national team

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
        if      (clb.ltPlayer[iIx].plGame.iId == iID1) iIndex1 = iIx;
        else if (clb.ltPlayer[iIx].plGame.iId == iID2) iIndex2 = iIx;

        if (iIndex1 >= 0 && iIndex2 >= 0) break;
      }

      return Json(SwitchPlayerByIndex(iIndex1, iIndex2), JsonRequestBehavior.AllowGet);
    }

    public JsonResult SwitchPlayer(CornerkickManager.Player pl1, CornerkickManager.Player pl2)
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

      // Create copy of player list
      List<CornerkickManager.Player> ltPlayerTeam = new List<CornerkickManager.Player>(club.ltPlayer);

      // Update player numbers if nation
      if (club.bNation) {
        for (byte iP = 0; iP < Math.Min(ltPlayerTeam.Count, byte.MaxValue); iP++) ltPlayerTeam[iP].plGame.iNrNat = (byte)(iP + 1);
      }

      bool bGame = false;
      if (user.game != null) {
        bGame = !user.game.data.bFinished;
        if (bGame) {
          iGameType = user.game.data.iGameType;

          if (user.game.data.ltState.Count > 0) {
            byte iHA = 0;
            if (club.iId == user.game.data.team[1].iTeamId) iHA = 1;

            ltPlayerTeam.Clear();
            foreach (CornerkickGame.Player plG in user.game.player[iHA]) {
              ltPlayerTeam.Add(CornerkickManager.PlayerTool.getPlayerFromId(plG.iId, club.ltPlayer));
            }
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

        if (i == MvcApplication.ckcore.plt.getCaptainIx(club)) sName += " (C)";

        int iNat = int.Parse(ltLV[i][12]);
        string sNat = CornerkickManager.Main.sLandShort[iNat];

        int iVal = 0;
        int.TryParse(ltLV[i][ 9].Replace(".", ""), out iVal);
        int iSal = 0;
        int.TryParse(ltLV[i][10].Replace(".", ""), out iSal);

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

      Point ptNew = new Point(iX, iY);
      CornerkickGame.Tool.correctPos(ref ptNew);

      frmClub.ptPos[iIndexPlayer] = ptNew;

      frmClub.iId = 0;

      updatePlayerOfGame(ckUser().game, club);

      return Json("success", JsonRequestBehavior.AllowGet);
    }

    public JsonResult movePlayerRoa(int iIndexPlayer, int iDirection)
    {
      if (iIndexPlayer < 0) return Json("error", JsonRequestBehavior.AllowGet);

      CornerkickManager.Club club = ckClub();
      if (club == null) return Json(false, JsonRequestBehavior.AllowGet);

      CornerkickGame.Player pl = club.ltPlayer[iIndexPlayer].plGame;

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

      CornerkickGame.Player pl = club.ltPlayer[iIxPlayer].plGame;

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
      Models.TeamModels.ltiSubstitution.Clear();

      /*
      // Check if game running
      CornerkickManager.User user = ckUser();

      if (user.game != null) {
        if (!user.game.data.bFinished) {
          CornerkickManager.Club club = ckClub();

          byte iHA = 0;
          if (club.iId == user.game.data.team[1].iTeamId) iHA = 1;

          // Create list of temp player
          List<CornerkickManager.Player> ltPlRes = new List<CornerkickManager.Player>();
          for (int i = user.game.data.nPlStart + user.game.data.nPlRes; i < club.ltPlayer.Count; i++) ltPlRes.Add(club.ltPlayer[i].Clone());

          // Clear current list of players
          club.ltPlayer.Clear();

          // Add player from current game
          foreach (CornerkickGame.Player pl in user.game.player[iHA]) club.ltPlayer.Add(pl);

          // Add temp player
          foreach (CornerkickManager.Player plRes in ltPlRes) club.ltPlayer.Add(plRes);

          return RedirectToAction("Team");
        }
      }
      */

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

    public void TeamSetOffenceFlag(int iPlayerIx, bool bSet)
    {
      CornerkickManager.User usr = ckUser();
      CornerkickManager.Club clb = ckClub();
      if (clb == null) return;
      if (iPlayerIx < 0) return;
      if (clb.ltPlayer == null) return;
      if (iPlayerIx >= clb.ltPlayer.Count) return;

      clb.ltPlayer[iPlayerIx].plGame.bOffStandards = bSet;
      Models.TeamModels.ltPlayer[iPlayerIx] = clb.ltPlayer[iPlayerIx].plGame;
      updatePlayerOfGame(usr.game, clb);
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

      tD.ltPlayer2 = new List<Models.TeamModels.Player>();

      tD.formation = club.ltTactic[0].formation;

      int iP = 0;
      foreach (CornerkickGame.Player pl in Models.TeamModels.ltPlayer) {
        if (pl == null) continue;

        Models.TeamModels.Player pl2 = new Models.TeamModels.Player();

        pl2.sName = pl.sName;
        pl2.iNb = pl.iNr;

        CornerkickManager.Player plMng = CornerkickManager.PlayerTool.getPlayerFromId(pl.iId, club.ltPlayer);
        pl2.sNat = CornerkickManager.Main.sLandShort[plMng.iNat1];
        pl2.sPortrait = getPlayerPortrait(plMng, sStyle: "height: 100%; width: 100%; object-fit: contain", bSmall: true);

        // Check if player is suspended
        bool bSusp = false;
        int iSuspIx = -1;
        if      (usr.game      != null && !usr.game.data.bFinished) iSuspIx = usr.game.data.iGameType - 1;
        else if (club.nextGame != null)                             iSuspIx = club.nextGame.iGameType - 1;

        if (iSuspIx >= 0 && iSuspIx < pl.iSuspension.Length) bSusp = pl.iSuspension[iSuspIx] > 0;

        pl2.bSusp = bSusp;

        // Individuel player tactic
        pl2.iIxManMarking = pl.iIxManMarking;
        pl2.bOffStandards = pl.bOffStandards;

        if (tD.formation.ptPos.Length > iP) {
          pl2.iPos = CornerkickGame.Tool.getBasisPos(CornerkickGame.Tool.getPosRole(tD.formation.ptPos[iP], MvcApplication.ckcore.game.ptPitch));
          pl2.sSkillAve = CornerkickGame.Tool.getAveSkill(pl, CornerkickGame.Tool.getPosRole(tD.formation.ptPos[iP], MvcApplication.ckcore.game.ptPitch)).ToString("0.0");
        }

        tD.ltPlayer2.Add(pl2);

        iP++;
      }

      tD.sDivPlayer = TeamGetPlayerOffence(Models.TeamModels.ltPlayer, club, bMobile);

      if (iSP >= 0) {
        tD.plSelected = Models.TeamModels.ltPlayer[iSP];

        if (iSP < tD.formation.fIndOrientation.Length) {
          tD.fIndOrientation = tD.formation.fIndOrientation[iSP];
          if (tD.fIndOrientation < -1f) tD.fIndOrientation = CornerkickGame.Tool.getPlayerIndividualOrientationDefault(tD.ltPlayer2[iSP].iPos);
        }

        tD.sDivRoa               = TeamGetPlayerRadiusOfAction(iSP, club);
        tD.fIndOrientationMinMax = TeamGetIndOrientationMinMax(iSP, club);
      }

      tD.iCaptainIx = MvcApplication.ckcore.plt.getCaptainIx(club);

      // Team averages
      float[] fTeamAve11 = CornerkickManager.Tool.getTeamAve(club, MvcApplication.ckcore.dtDatum, MvcApplication.ckcore.dtSeasonEnd, ptPitch: MvcApplication.ckcore.game.ptPitch, iPlStop: 11);
      tD.sTeamAveSkill = fTeamAve11[3].ToString("0.00");
      tD.sTeamAveAge   = fTeamAve11[4].ToString("0.0");

      tD.bNation = club.bNation;

      // Opponent team
      if (club.nextGame != null) {
        if (club.bNation) tD.iKibitzer = 3;
        else              tD.iKibitzer = club.staff.iKibitzer;

        int iClubOpp = club.nextGame.team[1].iTeamId;
        if (club.nextGame.team[1].iTeamId == club.iId) iClubOpp = club.nextGame.team[0].iTeamId;

        tD.ltPlayerOpp2 = new List<Models.TeamModels.Player>();

        tD.bOppTeam = iClubOpp >= 0;

        if (tD.bOppTeam && tD.iKibitzer > 0) {
          CornerkickManager.Club clubOpp = MvcApplication.ckcore.ltClubs[iClubOpp];

          tD.formationOpp = clubOpp.ltTactic[0].formation;

          for (byte iPl = 0; iPl < club.nextGame.nPlStart; iPl++) {
            if (iPl >= clubOpp.ltPlayer.Count) break;

            CornerkickManager.Player plOpp = clubOpp.ltPlayer[iPl];
            if (plOpp == null) continue;

            Models.TeamModels.Player plOpp2 = new Models.TeamModels.Player();

            plOpp2.sName = plOpp.plGame.sName;
            plOpp2.iNb = plOpp.plGame.iNr;
            plOpp2.sNat = CornerkickManager.Main.sLandShort[plOpp.iNat1];
            plOpp2.sPortrait = getPlayerPortrait(plOpp, sStyle: "height: 100%; width: 100%; object-fit: contain", bSmall: true);

            if (tD.formationOpp.ptPos.Length > iPl) {
              plOpp2.iPos = CornerkickGame.Tool.getBasisPos(CornerkickGame.Tool.getPosRole(tD.formationOpp.ptPos[iPl], MvcApplication.ckcore.game.ptPitch));
            }

            float fPlOppAveSkill = CornerkickGame.Tool.getAveSkill(plOpp.plGame, 99);
            if (tD.iKibitzer == 3) fPlOppAveSkill = (float)Math.Round(fPlOppAveSkill * 2f) / 2f;
            plOpp2.sSkillAve = fPlOppAveSkill.ToString("0.0");

            tD.ltPlayerOpp2.Add(plOpp2);
          }

          // Opp. team averages
          float[] fTeamOppAve11 = CornerkickManager.Tool.getTeamAve(clubOpp, MvcApplication.ckcore.dtDatum, MvcApplication.ckcore.dtSeasonEnd, ptPitch: MvcApplication.ckcore.game.ptPitch, iPlStop: club.nextGame.nPlStart);
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
        foreach (CornerkickManager.Player pl in club.ltPlayer) {
          if (pl.plGame.iId == game.player[iHA][iPl].iId) {
            game.player[iHA][iPl] = pl.plGame;
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
      CornerkickManager.Player player = MvcApplication.ckcore.ltPlayer[iPlayer];
      double fStrength = CornerkickGame.Tool.getAveSkill(player.plGame, 99);
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

        //if (CornerkickGame.Tool.getPosRole(tc.formation.ptPos[iPl], MvcApplication.ckcore.game.ptPitch) == 1) continue; // If keeper --> continue

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

        sDiv += "<div onclick=\"javascript: selectPlayer(" + iPl.ToString() + ")\" ";
        sDiv += "id=\"divOffencePl_" + iPl.ToString() + "\" ";
        sDiv += "style=\"position: absolute; ";
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

      CornerkickGame.Player pl = club.ltPlayer[iPlayerIndex].plGame;

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

      CornerkickGame.Player pl = club.ltPlayer[iPlayerIndex].plGame;

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

    public ActionResult ContractsGetTableTeam(bool bPro, bool bJouth, byte iPos, bool bNextSeason)
    {
      CornerkickManager.Club club = ckClub();
      if (club == null) return Json(false, JsonRequestBehavior.AllowGet);

      int iGameType = 0;
      if (club.nextGame != null) iGameType = club.nextGame.iGameType;

      //The table or entity I'm querying
      List<Models.ContractsModel.DatatableEntry> query = new List<Models.ContractsModel.DatatableEntry>();

      List<List<CornerkickManager.Player>> ltPlayerProJouth = new List<List<CornerkickManager.Player>>();
      if (bPro)   ltPlayerProJouth.Add(club.ltPlayer);
      if (bJouth) ltPlayerProJouth.Add(club.ltPlayerJouth);

      bool bJouth2 = !bPro;
      int iIx = 0;
      foreach (List<CornerkickManager.Player> ltPlayerTeam in ltPlayerProJouth) {
        // Update player numbers if nation
        if (club.bNation) {
          for (byte iP = 0; iP < Math.Min(ltPlayerTeam.Count, byte.MaxValue); iP++) ltPlayerTeam[iP].plGame.iNrNat = (byte)(iP + 1);
        }

        List<string[]> ltLV = MvcApplication.ckcore.ui.listTeam(ltPlayerTeam, club, false, iGameType, nPlStart: 0, iPosFilter: iPos);

        for (int i = 0; i < ltLV.Count; i++) {
          if (bNextSeason && ltPlayerTeam[i].contract.iLength == 1) continue;

          string sName = ltLV[i][2];
          int iId = -1;
          int.TryParse(ltLV[i][0], out iId);

          int iNat = int.Parse(ltLV[i][12]);
          string sNat = CornerkickManager.Main.sLandShort[iNat];

          int iVal  = int.Parse(ltLV[i][ 9].Replace(".", ""));
          int iSal  = int.Parse(ltLV[i][10].Replace(".", ""));
          int iPlay = int.Parse(ltLV[i][22].Replace(".", ""));
          int iGoal = int.Parse(ltLV[i][23].Replace(".", ""));

          string sNb = ltLV[i][1];
          if (bJouth2) sNb = "-";

          int iContractLength = int.Parse(ltLV[i][11]);
          if (bNextSeason) iContractLength--;

          query.Add(new Models.ContractsModel.DatatableEntry {
            sID = ltLV[i][0],
            sNb = sNb,
            sName = sName,
            sPosition = ltLV[i][3],
            sSkill = ltLV[i][4],
            iValue = iVal,
            iSalary = iSal,
            iLength = iContractLength,
            sNat = sNat,
            sAge = ltLV[i][14],
            sTalent = ltLV[i][15],
            sSkillIdeal = ltLV[i][17],
            iBonusPlay = iPlay,
            iBonusGoal = iGoal,
            sFixTransferFee = ltLV[i][24],
            bJouth = bJouth2
          });
        }

        bJouth2 = true;
      }

      if (bNextSeason) {
        //DateTime dtSeasonStartNext = MvcApplication.ckcore.dtSeasonStart.AddYears(1);

        foreach (CornerkickManager.Player plNext in MvcApplication.ckcore.ltPlayer) {
          if (plNext.contractNext != null) {
            if (plNext.contractNext.club.iId == club.iId) {
              query.Add(new Models.ContractsModel.DatatableEntry {
                sID = plNext.plGame.iId.ToString(),
                sNb = "-",
                sName = plNext.plGame.sName + " *",
                sPosition = CornerkickManager.PlayerTool.getStrPos(plNext),
                sSkill = CornerkickGame.Tool.getAveSkill(plNext.plGame).ToString("0.0"),
                iValue = plNext.getValue(MvcApplication.ckcore.dtDatum, MvcApplication.ckcore.dtSeasonEnd) * 1000,
                iSalary = plNext.contractNext.iSalary,
                iLength = plNext.contractNext.iLength,
                sNat = CornerkickManager.Main.sLandShort[plNext.iNat1],
                sAge = ((int)plNext.plGame.getAge(MvcApplication.ckcore.dtDatum)).ToString(),
                sTalent = plNext.iTalent.ToString(),
                sSkillIdeal = CornerkickGame.Tool.getAveSkill(plNext.plGame, bIdeal: true).ToString("0.0"),
                iBonusPlay = plNext.contractNext.iPlay,
                iBonusGoal = plNext.contractNext.iGoal,
                sFixTransferFee = plNext.contractNext.iFixTransferFee > 0 ? plNext.contractNext.iFixTransferFee.ToString() : "-",
                bJouth = bJouth
              });
            }
          }
        }
      }

      return Json(new { aaData = query }, JsonRequestBehavior.AllowGet);
    }

    [Authorize]
    public ActionResult PlayerDetails(Models.PlayerModel plModel, int i = -1, HttpPostedFileBase file = null)
    {
      if (i < 0) return RedirectToAction("Desk");

      if (file != null) {
        AccountController.uploadFileAsync(file, "Portraits", i);
      }

      CornerkickManager.User usr = ckUser();
      if (usr == null) return Json(false, JsonRequestBehavior.AllowGet);

      CornerkickManager.Club club = ckClub();
      if (club == null) return Json(false, JsonRequestBehavior.AllowGet);

      // Tutorial
      if (ttUser != null) {
        int iUserIx = MvcApplication.ckcore.ltUser.IndexOf(usr);
        if (iUserIx >= 0 && iUserIx < ttUser.Length) {
          plModel.tutorial = ttUser[iUserIx];
          if (plModel.tutorial.iLevel == 7) plModel.tutorial.iLevel = 8;
        }
      }

      plModel.iPlayer = i;
      CornerkickManager.Player plDetails = MvcApplication.ckcore.ltPlayer[i];
      plModel.iPlayerIndTr = plDetails.iIndTraining;

      plModel.bOwnPlayer = CornerkickManager.PlayerTool.ownPlayer(club, plDetails);

      CornerkickManager.Club clbPlayer = null;
      if (plDetails.contract != null) {
        clbPlayer = plDetails.contract.club;
        plModel.bJouth = CornerkickManager.PlayerTool.ownPlayer(clbPlayer, plDetails, 2);
        plModel.bJouthBelow16 = plDetails.plGame.getAge(MvcApplication.ckcore.dtDatum) < 16;
        plModel.bJouthWithContract = plModel.bJouth && plDetails.contract.iSalary != CornerkickManager.Finance.iPlayerJouthSalary;

        plModel.sColorJersey1  = "rgb(" + clbPlayer.cl1[0].R.ToString() + "," + clbPlayer.cl1[0].G.ToString() + "," + clbPlayer.cl1[0].B.ToString() + ")";
        plModel.sColorJersey2  = "rgb(" + clbPlayer.cl1[1].R.ToString() + "," + clbPlayer.cl1[1].G.ToString() + "," + clbPlayer.cl1[1].B.ToString() + ")";
        plModel.sColorJerseyNb = "rgb(" + clbPlayer.cl1[2].R.ToString() + "," + clbPlayer.cl1[2].G.ToString() + "," + clbPlayer.cl1[2].B.ToString() + ")";
        //System.Drawing.Color clJerseyNo = getColorBW(clbPlayer);
        //plModel.sColorJerseyNo = "rgb(" + clJerseyNo.R.ToString() + "," + clJerseyNo.G.ToString() + "," + clJerseyNo.B.ToString() + ")";

        plModel.bCpuPlayerNotOnTransferlist = !plModel.bOwnPlayer && !MvcApplication.ckcore.plt.onTransferlist(plDetails) && !plModel.bJouth && plDetails.contractNext == null;
      }

      plModel.bNation = club.bNation;

      plModel.iContractYears = 1;

      plModel.sName = plDetails.plGame.sName;
      plModel.fTalentAve = plDetails.getTalentAve() + 1f;

      plModel.ltDdlNo = new List<SelectListItem>();
      plModel.iNo = plDetails.plGame.iNr;

      plModel.sPortrait = getPlayerPortrait(plDetails, "height: 100%; width: 100%; object-fit: contain");
      if (plDetails.contract == null) plModel.sEmblem = getClubEmblem(                   null, "height: 100%; width: 100%; object-fit: contain");
      else                            plModel.sEmblem = getClubEmblem(plDetails.contract.club, "height: 100%; width: 100%; object-fit: contain");

      List<int> ltNoExist = new List<int>();
      foreach (CornerkickManager.Player pl in club.ltPlayer) {
        ltNoExist.Add(pl.plGame.iNr);
      }

      if (plDetails.plGame.iNr == 0) {
        plModel.ltDdlNo.Add(
          new SelectListItem {
            Text  = "0",
            Value = "0"
          }
        );
      }

      for (int j = 1; j < 41; j++) {
        if (ltNoExist.IndexOf(j) >= 0 && j != plDetails.plGame.iNr) continue;

        plModel.ltDdlNo.Add(
          new SelectListItem {
            Text  = j.ToString(),
            Value = j.ToString()
          }
        );
      }

      // Current position
      plModel.iPos = 0;
      for (int iPl = 0; iPl < Math.Min(11, clbPlayer.ltPlayer.Count); iPl++) {
        if (plDetails == clbPlayer.ltPlayer[iPl]) {
          plDetails.plGame.iIndex = (byte)iPl;
          plModel.iPos = CornerkickGame.Tool.getPosRole(plDetails.plGame, clbPlayer.ltTactic[0].formation, MvcApplication.ckcore.game.ptPitch);
          break;
        }
      }

      // Skill table
      plModel.sSkillTable = PlayerDetailsGetSkillTable(plDetails, plModel.iPos);

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
      if (plDetails.plGame.injury != null) {
        Random rnd = new Random();
        if (plDetails.plGame.injury.iType >= CornerkickManager.Main.ltInjury.Length) plDetails.plGame.injury.iType = (byte)(CornerkickManager.Main.ltInjury.Length - 1);
        if (plDetails.plGame.injury.iType2 < 0 || plDetails.plGame.injury.iType2 >= CornerkickManager.Main.ltInjury[plDetails.plGame.injury.iType].Count) plDetails.plGame.injury.iType2 = (sbyte)rnd.Next(CornerkickManager.Main.ltInjury[plDetails.plGame.injury.iType].Count);
      }

      // Contract
      int iGamesPerSeason = 0;
      CornerkickManager.Cup league = MvcApplication.ckcore.tl.getCup(1, club.iLand, club.iDivision);
      if (league != null) iGamesPerSeason = league.getMatchdays(club);

      //plModel.sContractHappyFactor = CornerkickManager.PlayerTool.getHappyWithContractFactor(plDetails, MvcApplication.ckcore.dtDatum, MvcApplication.ckcore.dtSeasonEnd, iGamesPerSeason: iGamesPerSeason).ToString("0.0%");
      plModel.fContractHappyFactor = CornerkickManager.PlayerTool.getHappyWithContractFactor(plDetails, MvcApplication.ckcore.dtDatum, MvcApplication.ckcore.dtSeasonEnd, iGamesPerSeason: iGamesPerSeason);
 
      // Next / Prev. Player
      plModel.iPlIdPrev = -1;
      plModel.iPlIdNext = -1;

      int iIndex = club.ltPlayer.IndexOf(plDetails);

      if (iIndex >= 0) {
        if (iIndex >                       0) plModel.iPlIdPrev = club.ltPlayer[iIndex - 1].plGame.iId;
        if (iIndex < club.ltPlayer.Count - 1) plModel.iPlIdNext = club.ltPlayer[iIndex + 1].plGame.iId;
      }

      // Player is editable
      plModel.bEditable = (MvcApplication.ckcore.dtDatum - usr.dtClubStart).TotalHours < 24;
      plModel.bSeasonStart = MvcApplication.ckcore.dtDatum.Date.Equals(MvcApplication.ckcore.dtSeasonStart.Date);

      return View(plModel);
    }

    private static string[][] PlayerDetailsGetSkillTable(CornerkickManager.Player plDetails, byte iPos)
    {
      // Define skill order
      byte[] iSkills = new byte[] {
        101,
        CornerkickGame.Game.iSkillIxSpeed,
        100, // Speed with ball
        CornerkickGame.Game.iSkillIxAcceleration,
        CornerkickGame.Game.iSkillIxJump,
        CornerkickGame.Game.iSkillIxTechnic,
        CornerkickGame.Game.iSkillIxDuelOff,
        CornerkickGame.Game.iSkillIxDuelDef,
        CornerkickGame.Game.iSkillIxLowPassPower,
        CornerkickGame.Game.iSkillIxHighPassPower,
        CornerkickGame.Game.iSkillIxShootPower,
        CornerkickGame.Game.iSkillIxLowPassAcc,
        CornerkickGame.Game.iSkillIxHighPassAcc,
        CornerkickGame.Game.iSkillIxShootAcc,
        CornerkickGame.Game.iSkillIxFreekick,
        CornerkickGame.Game.iSkillIxHeader,
        CornerkickGame.Game.iSkillIxPenalty,
        CornerkickGame.Game.iSkillIxReaction,
        CornerkickGame.Game.iSkillIxCatch
      };

      // Define table
      string[][] sTable = new string[iSkills.Length][];
      for (int i = 0; i < sTable.Length; i++) sTable[i] = new string[9];

      // Speed
      int iIx = 0;
      foreach (byte iS in iSkills) {
        // Skill index
        sTable[iIx][0] = iS.ToString();

        // Skill category
        if      (iIx ==  0) sTable[iIx][1] = "Ausdauer";
        else if (iIx ==  1) sTable[iIx][1] = "Athletik";
        else if (iIx ==  5) sTable[iIx][1] = "Koordination";
        else if (iIx ==  7) sTable[iIx][1] = "Zweikampf";
        else if (iIx ==  8) sTable[iIx][1] = "Kraft";
        else if (iIx == 11) sTable[iIx][1] = "Zielgenauigkeit";
        else if (iIx == 15) sTable[iIx][1] = "Kopfball";
        else if (iIx == 16) sTable[iIx][1] = "Kognition";

        if (iS == 100) {
          sTable[iIx][3] = "Schnelligk. m. Ball";
          sTable[iIx][5] = (CornerkickGame.Tool.getSkillEff(plDetails.plGame, 0, iPos) - (CornerkickGame.Tool.getSkillEff(plDetails.plGame, 0, iPos) / CornerkickGame.Tool.getSkillEff(plDetails.plGame, 1, iPos))).ToString("0.0");
        } else if (iS == 101) {
          sTable[iIx][2] = (plDetails.iTalent[CornerkickGame.Game.iSkillCategoryIxEndurance] + 1).ToString();
          sTable[iIx][3] = "Kondition";
          sTable[iIx][4] = plDetails.plGame.fCondition.ToString("0.0%");
          sTable[iIx][8] = CornerkickGame.Game.iSkillCategoryIxEndurance.ToString();
        } else {
          sTable[iIx][2] = (plDetails.getTalent(iS) + 1).ToString();
          sTable[iIx][3] = CornerkickManager.PlayerTool.sSkills[iS];
          sTable[iIx][4] = plDetails.plGame.iSkill[iS].ToString();
          sTable[iIx][5] = CornerkickGame.Tool.getSkillEff(plDetails.plGame, (byte)iS, iPos).ToString("0.0");
          sTable[iIx][6] = (plDetails.fSkillTraining[iS] + 1f).ToString("0.0%");
          sTable[iIx][7] = plDetails.plGame.fIndTraining[iS].ToString("0.0%");
        }

        // Skill category index
        if (iS < CornerkickGame.Game.iSkillCategory.Length) sTable[iIx][8] = CornerkickGame.Game.iSkillCategory[iS].ToString();

        iIx++;
      }

      return sTable;
    }

    internal static System.Drawing.Color getColorBW(CornerkickManager.Club club)
    {
      return getColorBW(club.cl1[0]);
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

    private static string getPlayerPortrait(CornerkickManager.Player plPortrait, string sStyle = "", bool bSmall = false)
    {
      string sPortrait = "<img src=\"/Content/Images/Portraits/";

      if (!string.IsNullOrEmpty(sStyle)) sStyle = " style=\"" + sStyle + "\"";

      if (plPortrait == null) return sPortrait + "0.png\" alt=\"Portrait\" " + sStyle + " title=\"ohne\"/>";

      bool bUserPortrait;
      bool bSmallOk;
      string sPortraitFile = getPlayerPortraitFile(plPortrait, out bUserPortrait, out bSmallOk, bSmall);

      if (System.IO.File.Exists(sPortraitFile)) {
        if (bUserPortrait) {
          sPortrait = "<img src=\"/Content/Uploads/Portraits/" + plPortrait.plGame.iId.ToString();
        } else {
          sPortrait += getPlayerPortraitId(plPortrait).ToString();
        }

        if (bSmall && bSmallOk) sPortrait += "s";
      } else {
        sPortrait += "0";
      }

      sPortrait += ".png\" alt=\"Wappen\"" + sStyle + " title=\"" + plPortrait.plGame.sName + "\"/>";

      return sPortrait;
    }

    private static string getPlayerPortraitFile(CornerkickManager.Player plPortrait, bool bSmall = false)
    {
      bool bUserPortrait;
      bool bSmallOk;

      return getPlayerPortraitFile(plPortrait, out bUserPortrait, out bSmallOk, bSmall);
    }
    private static string getPlayerPortraitFile(CornerkickManager.Player plPortrait, out bool bUserPortrait, out bool bSmallOk, bool bSmall = false)
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
        sPortraitFile = System.IO.Path.Combine(sPortraitDir, plPortrait.plGame.iId.ToString());

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

    private static ushort getPlayerPortraitId(CornerkickManager.Player plPortrait)
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

    public JsonResult PlayerDetailsGetAveSkill(int iPlayerId)
    {
      if (iPlayerId < 0) return Json(0.0, JsonRequestBehavior.AllowGet);

      CornerkickGame.Player player = MvcApplication.ckcore.ltPlayer[iPlayerId].plGame;

      return Json(new float[] { CornerkickGame.Tool.getAveSkill(player), CornerkickGame.Tool.getAveSkill(player, bIdeal: true) }, JsonRequestBehavior.AllowGet);
    }

    public JsonResult PlayerDetailsGetDoping(int iPlayerId)
    {
      float[] fDoping = new float[4];

      if (iPlayerId < 0) return Json(fDoping, JsonRequestBehavior.AllowGet);

      CornerkickGame.Player player = MvcApplication.ckcore.ltPlayer[iPlayerId].plGame;

      if (player.doping != null && player.doping.fEffectMax > 0f) {
        fDoping[0] = player.doping.fEffect;
        fDoping[1] = player.doping.fEffect / player.doping.fEffectMax;
        fDoping[2] = player.doping.fReductionRate;
        fDoping[3] = player.doping.fDetectable * (player.doping.fEffect / player.doping.fEffectMax);
      }

      return Json(fDoping, JsonRequestBehavior.AllowGet);
    }

    public JsonResult setPlayerIndTraining(int iPlayer, int iIndTr)
    {
      CornerkickManager.Player player = MvcApplication.ckcore.ltPlayer[iPlayer];
      player.iIndTraining = (byte)iIndTr;

      return Json(iIndTr, JsonRequestBehavior.AllowGet);
    }

    public JsonResult setPlayerName(int iPlayer, string sName)
    {
      if (string.IsNullOrEmpty(sName)) {
        Response.StatusCode = 1;
        return Json("Name darf nicht leer sein!", JsonRequestBehavior.AllowGet);
        //return Json(new { message = "Name darf nicht leer sein!" }, JsonRequestBehavior.AllowGet);
      }

      foreach (CornerkickManager.Player pl in MvcApplication.ckcore.ltPlayer) {
        if (string.IsNullOrEmpty(pl.plGame.sName)) continue;

        if (pl.plGame.sName.Equals(sName)) {
          Response.StatusCode = 1;
          return Json("Name bereits vorhanden!", JsonRequestBehavior.AllowGet);
          //return Json(new { message = "Name bereits vorhanden!" }, JsonRequestBehavior.AllowGet);
        }
      }

      CornerkickGame.Player player = MvcApplication.ckcore.ltPlayer[iPlayer].plGame;
      player.sName = sName;

      return Json(player.sName, JsonRequestBehavior.AllowGet);
    }

    public JsonResult setPlayerNo(int iPlayer, byte iNo)
    {
      CornerkickGame.Player player = MvcApplication.ckcore.ltPlayer[iPlayer].plGame;
      player.iNr = iNo;

      return Json(player.iNr, JsonRequestBehavior.AllowGet);
    }

    public JsonResult PlayerMakeCaptain(int iPlayerId, byte iC)
    {
      if (iPlayerId < 0) return Json("Error", JsonRequestBehavior.AllowGet);

      CornerkickManager.Club club = ckClub();
      if (club == null) return Json(false, JsonRequestBehavior.AllowGet);

      if (club.iCaptainId == null) club.iCaptainId = new int[3];

      if (iC >= club.iCaptainId.Length) return Json("Error", JsonRequestBehavior.AllowGet);

      string sCaptain = "Kapitän";
      if (iC == 1) sCaptain = "Vize-Kapitän";

      MvcApplication.ckcore.plt.makePlayerCaptain(iPlayerId, iC, club);

      return Json("Sie haben " + MvcApplication.ckcore.ltPlayer[iPlayerId].plGame.sName + " zum " + sCaptain + " ernannt.", JsonRequestBehavior.AllowGet);
    }

    public JsonResult getClubCaptain(int iC)
    {
      CornerkickManager.Club club = ckClub();
      if (club == null) return Json(false, JsonRequestBehavior.AllowGet);

      if (club.iCaptainId[iC] >= 0) return Json(MvcApplication.ckcore.ltPlayer[club.iCaptainId[iC]].plGame.sName, JsonRequestBehavior.AllowGet);

      return Json(false, JsonRequestBehavior.AllowGet);
    }

    public JsonResult PlayerDetailsDoDoping(int iPlayerId, byte iDp)
    {
      if (iPlayerId < 0) return Json("Error", JsonRequestBehavior.AllowGet);
      if (iDp >= MvcApplication.ckcore.ltDoping.Count) return Json("Error", JsonRequestBehavior.AllowGet);

      CornerkickManager.Player pl = MvcApplication.ckcore.ltPlayer[iPlayerId];

      return Json(MvcApplication.ckcore.plt.doDoping(pl, MvcApplication.ckcore.ltDoping[iDp]), JsonRequestBehavior.AllowGet);
    }

    [HttpGet]
    public JsonResult PlayerDetailsGetDopingDesc(byte iDp)
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

    [HttpGet]
    public JsonResult PlayerDetailsMoveIntoProTeam(int iPlayerId)
    {
      CornerkickManager.Player pl = MvcApplication.ckcore.ltPlayer[iPlayerId];

      string sReturn = "Error";
      if (pl?.contract?.club != null) {
        CornerkickManager.Club clb = pl.contract.club;

        for (int iPlJ = 0; iPlJ < clb.ltPlayerJouth.Count; iPlJ++) {
          CornerkickManager.Player plJ = clb.ltPlayerJouth[iPlJ];

          if (plJ.plGame.iId == pl.plGame.iId) {
            if ((int)plJ.plGame.getAge(MvcApplication.ckcore.dtDatum) < 16) return Json("Error: Spieler zu jung für Profivertrag", JsonRequestBehavior.AllowGet);

            clb.ltPlayerJouth.RemoveAt(iPlJ);
            clb.ltPlayer.Add(plJ);

            // Reset jersey number
            plJ.plGame.iNr = 0;

            // Add club history to player
            pl.ltClubHistory.Add(new CornerkickManager.Player.ClubHistory() {
              club = clb,
              dt = MvcApplication.ckcore.dtDatum,
              iTransferFee = 0,
              bJouth = false
            });

            return Json("Sie haben den Jugendspieler " + pl.plGame.sName + " in ihr Profiteam übernommen.", JsonRequestBehavior.AllowGet);
          }
        }
      }

      return Json(sReturn, JsonRequestBehavior.AllowGet);
    }

    public ContentResult PlayerDetailsGetCFM(int iPlId)
    {
      CornerkickGame.Player pl = MvcApplication.ckcore.ltPlayer[iPlId].plGame;

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
      CornerkickManager.Player pl = MvcApplication.ckcore.ltPlayer[iPlId];

      List<Models.DataPointGeneral>[] ltDataPoints = new List<Models.DataPointGeneral>[2];
      ltDataPoints[0] = new List<Models.DataPointGeneral>(); // Skill
      ltDataPoints[1] = new List<Models.DataPointGeneral>(); // Value

      foreach (CornerkickManager.Player.History hty in pl.ltHistory) {
        long iDate = convertDateTimeToTimestamp(hty.dt);
        ltDataPoints[0].Add(new Models.DataPointGeneral(iDate, hty.fStrength));
        if (hty.iValue > 0) ltDataPoints[1].Add(new Models.DataPointGeneral(iDate, hty.iValue * 1000));
      }

      long iDateCurrent = convertDateTimeToTimestamp(MvcApplication.ckcore.dtDatum);
      ltDataPoints[0].Add(new Models.DataPointGeneral(iDateCurrent, CornerkickGame.Tool.getAveSkill(pl.plGame, bIdeal: true)));
      ltDataPoints[1].Add(new Models.DataPointGeneral(iDateCurrent, pl.getValue(MvcApplication.ckcore.dtDatum, MvcApplication.ckcore.dtSeasonEnd) * 1000));

      JsonSerializerSettings _jsonSetting = new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore };

      return Content(JsonConvert.SerializeObject(ltDataPoints, _jsonSetting), "application/json");
    }

    public ActionResult PlayerDetailsGetClubHistoryTable(int iPlayerId)
    {
      CornerkickManager.Player pl = MvcApplication.ckcore.ltPlayer[iPlayerId];

      //The table or entity I'm querying
      List<Models.PlayerModel.DatatableEntryClubHistory> ltDeClubHistory = new List<Models.PlayerModel.DatatableEntryClubHistory>();

      if (pl.ltClubHistory != null) {
        for (int iCh = 0; iCh < pl.ltClubHistory.Count; iCh++) {
          CornerkickManager.Player.ClubHistory ch = pl.ltClubHistory[iCh];

          // Get club name
          string sClubName = "vereinslos";
          if (ch.club != null) {
            sClubName = ch.club.sName;
            //<td align="center">@Html.ActionLink(MvcApplication.ckcore.ltClubs[ch.iClubId].sName, "ClubDetails", "Member", new { i = ch.iClubId }, new { target = "" })</td>
          }
          if (ch.bJouth) sClubName += " (Jugend)";

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

    public ActionResult PlayerDetailsGetInjuryHistoryTable(int iPlayerId)
    {
      CornerkickGame.Player pl = MvcApplication.ckcore.ltPlayer[iPlayerId].plGame;

      //The table or entity I'm querying
      List<Models.PlayerModel.DatatableEntryInjuryHistory> ltDeInjuryHistory = new List<Models.PlayerModel.DatatableEntryInjuryHistory>();

      if (pl.ltInjuryHistory != null) {
        for (int iIh = 0; iIh < pl.ltInjuryHistory.Count; iIh++) {
          CornerkickGame.Player.InjuryHistory ih = pl.ltInjuryHistory[iIh];

          // Remove corrupt entry
          if (ih.injury == null) {
            pl.ltInjuryHistory.RemoveAt(iIh);
            iIh--;
            continue;
          }

          ltDeInjuryHistory.Add(new Models.PlayerModel.DatatableEntryInjuryHistory {
            iIx = iIh,
            sDt = ih.dt.ToString("d", getCi()),
            sInjuryName = CornerkickManager.Main.ltInjury[ih.injury.iType][Math.Max((int)ih.injury.iType2, 0)],
            iInjuryLength = (int)ih.injury.fLengthMax
          });
        }
      }

      return Json(new { aaData = ltDeInjuryHistory.ToArray() }, JsonRequestBehavior.AllowGet);
    }

    public ContentResult GetPlayerTrainingHistotyData(int iPlayerId)
    {
      CornerkickManager.Player pl = MvcApplication.ckcore.ltPlayer[iPlayerId];

      CornerkickManager.Player.TrainingHistory trHistCurrent = new CornerkickManager.Player.TrainingHistory();
      trHistCurrent.dt = MvcApplication.ckcore.dtDatum;
      trHistCurrent.fKFM = new float[] { pl.plGame.fCondition, pl.plGame.fFresh, pl.plGame.fMoral, 0f, 0f };

      List<Models.DataPointGeneral>[] dataPoints = new List<Models.DataPointGeneral>[3];

      for (byte j = 0; j < dataPoints.Length; j++) {
        dataPoints[j] = new List<Models.DataPointGeneral>();

        for (int i = 0; i < pl.ltTrainingHistory.Count; i++) {
          CornerkickManager.Player.TrainingHistory trHist = pl.ltTrainingHistory[i];

          long iDate = convertDateTimeToTimestamp(trHist.dt);

          CornerkickManager.PlayerTool.Training tr = CornerkickManager.PlayerTool.getTraining(trHist.iType, MvcApplication.ckcore.plt.ltTraining);
          //string sTrainingName = "";
          //if (tr.iId >= 0) sTrainingName = tr.sName;
          //dataPoints[j].Add(new Models.DataPointGeneral(iDate, trHist.fKFM[j], z: sTrainingName));
          dataPoints[j].Add(new Models.DataPointGeneral(iDate, trHist.fKFM[j], z: tr.sName));
        }

        long iDateCurrent = convertDateTimeToTimestamp(trHistCurrent.dt);
        dataPoints[j].Add(new Models.DataPointGeneral(iDateCurrent, trHistCurrent.fKFM[j]));
      }

      JsonSerializerSettings _jsonSetting = new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore };

      return Content(JsonConvert.SerializeObject(dataPoints, _jsonSetting), "application/json");
    }

    public JsonResult PlayerDetailsGetStatistic(int iPlayer, bool bSeason = true)
    {
      CornerkickManager.Player player = MvcApplication.ckcore.ltPlayer[iPlayer];

      const byte nStatLength = 4;

      // Create EP statistic
      CornerkickGame.Player.Statistic plStat3 = player.plGame.getStatistic(3, bSeason);
      CornerkickGame.Player.Statistic plStat4 = player.plGame.getStatistic(4, bSeason);
      CornerkickGame.Player.Statistic plStatEP = new CornerkickGame.Player.Statistic();
      for (int iS = 0; iS < plStat3.iStat.Length; iS++) {
        plStatEP.iStat[iS] = plStat3.iStat[iS] + plStat4.iStat[iS];
      }

      CornerkickGame.Player.Statistic[] plStat = new CornerkickGame.Player.Statistic[nStatLength] { player.plGame.getStatistic(1, bSeason), player.plGame.getStatistic(2, bSeason), plStatEP, player.plGame.getStatistic(7, bSeason) };

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
      CornerkickManager.Player pl = MvcApplication.ckcore.ltPlayer[iPlId];

      CornerkickManager.Club clb = null;
      if (pl.contract != null) clb = pl.contract.club;

      if (!CornerkickManager.PlayerTool.ownPlayer(clb, pl)) return Content(null, "application/json");

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
        bool bJouth = CornerkickManager.PlayerTool.ownPlayer(clb, pl, 2);
        if (bJouth) {
          iCoach    = clb.staff.iJouthTrainer; // Jouth coach
          iBuilding = clb.buildings.bgJouthInternat.iLevel; // Jouth Internat
        } else {
          iCoach    = clb.staff.iCoTrainer; // Co-Coach
          iBuilding = clb.buildings.bgTrainingCourts.iLevel; // Training Court
        }
      }

      DateTime dt25 = MvcApplication.ckcore.dtDatum.AddYears(-25);
      double fChanceDev    =          CornerkickManager.PlayerTool.getChanceDevelopment(pl.plGame.dtBirthday, pl.getTalent(pl.iIndTraining), pl.plGame.iSkill[pl.iIndTraining], pl.plGame.fIndTraining[pl.iIndTraining], pl.plGame.fExperience, pl.iIndTraining, MvcApplication.ckcore.dtDatum, iCoach, iBuilding, fLevel);
      double[] fChanceDevFte = new double[8];
      fChanceDevFte[0] = fChanceDev - CornerkickManager.PlayerTool.getChanceDevelopment(pl.plGame.dtBirthday, pl.getTalent(pl.iIndTraining), pl.plGame.iSkill[pl.iIndTraining], pl.plGame.fIndTraining[pl.iIndTraining], pl.plGame.fExperience,               1, MvcApplication.ckcore.dtDatum, iCoach, iBuilding, fLevel);
      fChanceDevFte[1] = fChanceDev - CornerkickManager.PlayerTool.getChanceDevelopment(pl.plGame.dtBirthday, pl.getTalent(pl.iIndTraining),                                6f, pl.plGame.fIndTraining[pl.iIndTraining], pl.plGame.fExperience, pl.iIndTraining, MvcApplication.ckcore.dtDatum, iCoach, iBuilding, fLevel);
      fChanceDevFte[2] = fChanceDev - CornerkickManager.PlayerTool.getChanceDevelopment(pl.plGame.dtBirthday,                             4, pl.plGame.iSkill[pl.iIndTraining], pl.plGame.fIndTraining[pl.iIndTraining], pl.plGame.fExperience, pl.iIndTraining, MvcApplication.ckcore.dtDatum, iCoach, iBuilding, fLevel);
      fChanceDevFte[3] = fChanceDev - CornerkickManager.PlayerTool.getChanceDevelopment(pl.plGame.dtBirthday, pl.getTalent(pl.iIndTraining), pl.plGame.iSkill[pl.iIndTraining], pl.plGame.fIndTraining[pl.iIndTraining], 1f,                    pl.iIndTraining, MvcApplication.ckcore.dtDatum, iCoach, iBuilding, fLevel);
      fChanceDevFte[4] = fChanceDev - CornerkickManager.PlayerTool.getChanceDevelopment(                dt25, pl.getTalent(pl.iIndTraining), pl.plGame.iSkill[pl.iIndTraining], pl.plGame.fIndTraining[pl.iIndTraining], pl.plGame.fExperience, pl.iIndTraining, MvcApplication.ckcore.dtDatum, iCoach, iBuilding, fLevel);
      fChanceDevFte[5] = fChanceDev - CornerkickManager.PlayerTool.getChanceDevelopment(pl.plGame.dtBirthday, pl.getTalent(pl.iIndTraining), pl.plGame.iSkill[pl.iIndTraining], pl.plGame.fIndTraining[pl.iIndTraining], pl.plGame.fExperience, pl.iIndTraining, MvcApplication.ckcore.dtDatum,      0, iBuilding, fLevel);
      fChanceDevFte[6] = fChanceDev - CornerkickManager.PlayerTool.getChanceDevelopment(pl.plGame.dtBirthday, pl.getTalent(pl.iIndTraining), pl.plGame.iSkill[pl.iIndTraining], pl.plGame.fIndTraining[pl.iIndTraining], pl.plGame.fExperience, pl.iIndTraining, MvcApplication.ckcore.dtDatum, iCoach,         0, fLevel);
      fChanceDevFte[7] = fChanceDev - CornerkickManager.PlayerTool.getChanceDevelopment(pl.plGame.dtBirthday, pl.getTalent(pl.iIndTraining), pl.plGame.iSkill[pl.iIndTraining],                                      0f, pl.plGame.fExperience, pl.iIndTraining, MvcApplication.ckcore.dtDatum, iCoach, iBuilding, fLevel);

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

    public JsonResult PlayerDetailsForcePutOnTransferList(int iPlayerId)
    {
      if (iPlayerId < 0) return Json(null, JsonRequestBehavior.AllowGet);
      if (iPlayerId >= MvcApplication.ckcore.ltPlayer.Count) return Json(null, JsonRequestBehavior.AllowGet);

      CornerkickManager.Player pl = MvcApplication.ckcore.ltPlayer[iPlayerId];

      if (pl.contract == null) return Json(null, JsonRequestBehavior.AllowGet);
      CornerkickManager.Club clbPlayer = pl.contract.club;
      if (clbPlayer.user != null) return Json("Dies ist nur bei Computer-Vereinen möglich", JsonRequestBehavior.AllowGet);

      if (MvcApplication.ckcore.plt.onTransferlist(pl)) {
        return Json("Der Spieler " + pl.plGame.sName + " ist bereits auf der Transferliste", JsonRequestBehavior.AllowGet);
      }

      int iSecretMoney = getSecretMoneyForcePutOnTransferList(pl);

      CornerkickManager.Club clbUser = ckClub();
      if (clbUser.iBalanceSecret < iSecretMoney) {
        return Json("Sie haben leider nicht genug Schwarzgeld zur Hand...", JsonRequestBehavior.AllowGet);
      }

      JsonResult jsnResult = TransferPutOnTakeFromTransferList(iPlayerId);
      if (jsnResult.Data.Equals("Der Spieler " + MvcApplication.ckcore.ltPlayer[iPlayerId].plGame.sName + " wurde auf die Transferliste gesetzt")) {
        clbUser.iBalanceSecret -= iSecretMoney;
      }

      return jsnResult;
    }

    public JsonResult PlayerDetailsGetSecretMoneyForcePutOnTransferList(int iPlayerId)
    {
      if (iPlayerId < 0) return Json(null, JsonRequestBehavior.AllowGet);
      if (iPlayerId >= MvcApplication.ckcore.ltPlayer.Count) return Json(null, JsonRequestBehavior.AllowGet);

      CornerkickManager.Player pl = MvcApplication.ckcore.ltPlayer[iPlayerId];

      if (pl.contract == null) return Json(null, JsonRequestBehavior.AllowGet);
      CornerkickManager.Club clbPlayer = pl.contract.club;
      if (clbPlayer.user != null) return Json("Dies ist nur bei Computer-Vereinen möglich", JsonRequestBehavior.AllowGet);

      if (MvcApplication.ckcore.plt.onTransferlist(pl)) {
        return Json("Der Spieler " + pl.plGame.sName + " ist bereits auf der Transferliste", JsonRequestBehavior.AllowGet);
      } else {
        return Json("Um sich für Sie auf die Transferliste zu streiken, verlangt " + pl.plGame.sName + " etwas Schwarzgeld in Höhe von " + getSecretMoneyForcePutOnTransferList(pl).ToString("N0", getCi()) + " €", JsonRequestBehavior.AllowGet);
      }

      return Json(null, JsonRequestBehavior.AllowGet);
    }

    private int getSecretMoneyForcePutOnTransferList(CornerkickManager.Player pl)
    {
      return 1000 * (pl.getValue(MvcApplication.ckcore.dtDatum, MvcApplication.ckcore.dtSeasonEnd) / 50);
    }

    public class PlayerSalary
    {
      public int iPlayerId { get; set; }
      public int iYears { get; set; }
      public int iSalary { get; set; }
      public int iBonusPlay { get; set; }
      public int iBonusPoint { get; set; }
      public int iBonusGoal { get; set; }
      public int[] iaCupBonus { get; set; }
      public int iFixedFee { get; set; }
      public bool bNegotiateNextSeason { get; set; }
      public string sPlayerMood { get; set; }
    }
    // Copy of CornerkickGame.Player.Contract class without club for return
    public class ContractMvc
    {
      public byte iLength;        // Length of contract [years]
      public int iSalary;         // Salary [$/month]
      public int iGoal;           // Bonus goal
      public int iPlay;           // Bonus play
      public int iPoint;          // Bonus point
      public int[] iaCupBonus;    // Bonus cup
      public int iFixTransferFee; // Fix transfer fee
      public bool bTransferCurrentSeason; // Player was transferred in current season (no further transfer allowed)
      public float fMood; // Player mood while negotiating
    }
    [HttpPost]
    public JsonResult GetPlayerSalary(PlayerSalary ps)
    {
      if (ps.iPlayerId < 0) return Json("Invalid player", JsonRequestBehavior.AllowGet);

      CornerkickManager.Club club = ckClub();
      if (club == null) return Json(false, JsonRequestBehavior.AllowGet);

      CornerkickManager.Player plSalary = MvcApplication.ckcore.ltPlayer[ps.iPlayerId];

      int iGamesPerSeason = 0;
      CornerkickManager.Cup league = MvcApplication.ckcore.tl.getCup(1, club.iLand, club.iDivision);
      if (league != null) iGamesPerSeason = league.getMatchdays(club);

      bool bForceNewContract = checkIfNewContract(plSalary, club);

      // Get cup bonus class
      List<CornerkickManager.Player.Contract.CupBonus> ltCupBonus = getCupBonus(ps.iaCupBonus, plSalary.contract.club);

      CornerkickManager.Player.Contract contract = MvcApplication.ckcore.plt.negotiatePlayerContract(plSalary, club, (byte)ps.iYears, iSalaryOffer: ps.iSalary, iBonusPlayOffer: ps.iBonusPlay, iBonusPointOffer: ps.iBonusPoint, iBonusGoalOffer: ps.iBonusGoal, ltCupBonusOffer: ltCupBonus, iGamesPerSeason: iGamesPerSeason, iFixedFeeOffer: ps.iFixedFee, bNegotiate: ps.bNegotiateNextSeason, bForceNewContract: bForceNewContract);

      // Create reduced contract to return
      ContractMvc cctReqMvc = new ContractMvc();
      cctReqMvc.iLength                = contract.iLength;  // Length of contract [years]
      cctReqMvc.iSalary                = contract.iSalary;  // Salary [$/month]
      cctReqMvc.iPlay                  = contract.iPlay;    // Bonus play
      cctReqMvc.iPoint                 = contract.iPoint;   // Bonus point
      cctReqMvc.iGoal                  = contract.iGoal;    // Bonus goal
      cctReqMvc.iFixTransferFee        = contract.iFixTransferFee;  // Fix transfer fee
      cctReqMvc.bTransferCurrentSeason = contract.bTransferCurrentSeason; // Player was transferred in current season (no further transfer allowed)
      cctReqMvc.fMood                  = contract.fMood; // Player mood while negotiating

      ContractMvc cctOffMvc = new ContractMvc();
      CornerkickManager.Transfer.Offer offer = MvcApplication.ckcore.tr.getOffer(plSalary, club);
      if (offer?.contractOffered == null) {
        cctOffMvc = cctReqMvc;
      } else {
        cctOffMvc.iLength                = offer.contractOffered.iLength;  // Length of contract [years]
        cctOffMvc.iSalary                = offer.contractOffered.iSalary;  // Salary [$/month]
        cctOffMvc.iPlay                  = offer.contractOffered.iPlay;    // Bonus play
        cctOffMvc.iPoint                 = offer.contractOffered.iPoint;   // Bonus point
        cctOffMvc.iGoal                  = offer.contractOffered.iGoal;    // Bonus goal
        cctOffMvc.iFixTransferFee        = offer.contractOffered.iFixTransferFee;  // Fix transfer fee
        cctOffMvc.bTransferCurrentSeason = offer.contractOffered.bTransferCurrentSeason; // Player was transferred in current season (no further transfer allowed)
        cctOffMvc.fMood                  = offer.contractOffered.fMood; // Player mood while negotiating

        // Apply cup bonus
        if (offer.contractOffered.ltCupBonus != null) {
          List<int> ltCb = new List<int>();
          foreach (CornerkickManager.Player.Contract.CupBonus cb in offer.contractOffered.ltCupBonus) {
            ltCb.Add(cb.cup.iId);
            ltCb.Add(cb.iPlace);
            ltCb.Add(cb.iValue);
          }
          cctOffMvc.iaCupBonus = ltCb.ToArray();
        }
      }

      return Json(new ContractMvc[] { cctReqMvc, cctOffMvc }, JsonRequestBehavior.AllowGet);
    }

    // Returns
    //   0: contract extension
    //   1: new contract (for own jouth player)
    //   2: new contract for external player
    //   3: new contract for external player with ending contract
    [HttpGet]
    public JsonResult PlayerCheckIfNewContract(int iPlayerId)
    {
      if (iPlayerId < 0) return Json("Invalid player", JsonRequestBehavior.AllowGet);

      CornerkickManager.Club clb = ckClub();
      if (clb == null) return Json(false, JsonRequestBehavior.AllowGet);

      CornerkickManager.Player plContract = MvcApplication.ckcore.ltPlayer[iPlayerId];

      byte iType = 0;
      if (!CornerkickManager.PlayerTool.ownPlayer(clb, plContract)) {
        iType = 2;

        // If player has a club (and a contract) and not fixed transfer fee and is not on transfer list
        if (plContract.contract != null && plContract.contract.iFixTransferFee < 1 && !MvcApplication.ckcore.plt.onTransferlist(plContract)) iType++;
      } else if (checkIfNewContract(plContract, clb)) {
        iType = 1;
      }

      return Json(iType, JsonRequestBehavior.AllowGet);
    }

    // Returns
    //   true:  external player or jouth player with initial contract (not transfered)
    //   false: own player
    private bool checkIfNewContract(CornerkickManager.Player pl, CornerkickManager.Club clbUser)
    {
      bool bForceNewContract = true;

      CornerkickManager.Club clbPlayer = null;
      if (pl?.contract?.club != null) clbPlayer = pl.contract.club;

      if (clbUser != null) bForceNewContract = clbUser.iId != clbPlayer.iId;

      if (!bForceNewContract && CornerkickManager.PlayerTool.ownPlayer(clbPlayer, pl, 2)) {
        if (pl.contract.iSalary == CornerkickManager.Finance.iPlayerJouthSalary) bForceNewContract = true;
      }

      return bForceNewContract;
    }

    // iMode: 0 - Extention, 1 - new contract
    const byte iContractLengthMax = 5;
    [HttpPost]
    public JsonResult NegotiatePlayerContract(PlayerSalary ps)
    {
      // Initialize status code with ERROR
      Response.StatusCode = 1;

      CornerkickManager.Club clbUser = ckClub();
      if (clbUser == null) return Json("Error", JsonRequestBehavior.AllowGet);

      if (ps.iPlayerId < 0) return Json("Error", JsonRequestBehavior.AllowGet);
      if (ps.iYears < 0) return Json("0",     JsonRequestBehavior.AllowGet);

      if (ps.iPlayerId < 0) return Json("Error", JsonRequestBehavior.AllowGet);
      if (ps.iPlayerId >= MvcApplication.ckcore.ltPlayer.Count) return Json("Error", JsonRequestBehavior.AllowGet);

      // Get player
      CornerkickManager.Player plContract = MvcApplication.ckcore.ltPlayer[ps.iPlayerId];

      if (ps.iSalary < 0) return Json("Error", JsonRequestBehavior.AllowGet);
      if (ps.iBonusPlay < 0) return Json("Error", JsonRequestBehavior.AllowGet);
      if (ps.iBonusPoint < 0) return Json("Error", JsonRequestBehavior.AllowGet);
      if (ps.iBonusGoal < 0) return Json("Error", JsonRequestBehavior.AllowGet);
      if (ps.iFixedFee < 0) return Json("Error", JsonRequestBehavior.AllowGet);

      // Convert player mood to double
      ps.sPlayerMood = ps.sPlayerMood.Replace("%", string.Empty);
      ps.sPlayerMood = ps.sPlayerMood.Replace(".", string.Empty);
      ps.sPlayerMood = ps.sPlayerMood.Trim();
      float fPlayerMood = 1f;
      if (!float.TryParse(ps.sPlayerMood, out fPlayerMood)) return Json("Error", JsonRequestBehavior.AllowGet);
      fPlayerMood /= 100f;

      string sReturn = "";
      if (CornerkickManager.PlayerTool.ownPlayer(clbUser, plContract)) { // Contract extention
        byte iContractLegth = (byte)ps.iYears;

        if (!checkIfNewContract(plContract, clbUser)) iContractLegth += plContract.contract.iLength;

        if (iContractLegth > iContractLengthMax) return Json("Error: Maximale Vertragslänge = " + iContractLengthMax.ToString() + " Jahre", JsonRequestBehavior.AllowGet);

        plContract.contract.iLength = iContractLegth;
        plContract.contract.iSalary = ps.iSalary;
        plContract.contract.iPlay   = ps.iBonusPlay;
        plContract.contract.iPoint  = ps.iBonusPoint;
        plContract.contract.iGoal   = ps.iBonusGoal;
        plContract.contract.ltCupBonus = getCupBonus(ps.iaCupBonus, plContract.contract.club);
        plContract.contract.iFixTransferFee = ps.iFixedFee;
        plContract.contract.fMood = fPlayerMood;

        sReturn = "Der Vertrag mit " + plContract.plGame.sName + " wurde ";
        if (ps.iYears > 0) sReturn += "um " + ps.iYears.ToString() + " Jahre verlängert.";
        else               sReturn += "geändert.";

        if (plContract.contract?.club != null) {
          CornerkickManager.Club clb = plContract.contract.club;

          for (int iPlJ = 0; iPlJ < clb.ltPlayerJouth.Count; iPlJ++) {
            CornerkickManager.Player plJ = clb.ltPlayerJouth[iPlJ];

            if (plJ.plGame.iId == plContract.plGame.iId) {
              if ((int)plJ.plGame.getAge(MvcApplication.ckcore.dtDatum) < 16) return Json("Error: Spieler zu jung für Profivertrag", JsonRequestBehavior.AllowGet);

              clb.ltPlayerJouth.RemoveAt(iPlJ);
              clb.ltPlayer.Add(plJ);

              // Reset jersey number
              plJ.plGame.iNr = 0;

              // Add club history to player
              plContract.ltClubHistory.Add(new CornerkickManager.Player.ClubHistory() {
                club = clb,
                dt = MvcApplication.ckcore.dtDatum,
                iTransferFee = 0,
                bJouth = false
              });

              sReturn = "Der Jugendspieler " + plContract.plGame.sName + " hat ihr Angebot angenommen und gehört ab sofort dem Profikader an.";

              break;
            }
          }
        }

        // Remove hidden entry from transfer list
        MvcApplication.ckcore.tr.removePlayerFromTransferlist(plContract);
      } else { // New contract
        if (ps.iYears < 1) return Json("0", JsonRequestBehavior.AllowGet);
        if (ps.iYears > iContractLengthMax) return Json("Error: Maximale Vertragslänge = " + iContractLengthMax.ToString() + " Jahre", JsonRequestBehavior.AllowGet);

        // Create new offer
        CornerkickManager.Transfer.Offer offer = new CornerkickManager.Transfer.Offer();
        CornerkickManager.Player.Contract contract = new CornerkickManager.Player.Contract();
        contract.iLength = (byte)ps.iYears;
        contract.iSalary = ps.iSalary;
        contract.iPlay   = ps.iBonusPlay;
        contract.iPoint  = ps.iBonusPoint;
        contract.iGoal   = ps.iBonusGoal;
        contract.ltCupBonus = getCupBonus(ps.iaCupBonus, plContract.contract.club);
        contract.iFixTransferFee = ps.iFixedFee;
        contract.fMood = fPlayerMood;
        contract.club = clbUser;
        offer.contract = contract;
        offer.bNextSeason = ps.bNegotiateNextSeason;

        MvcApplication.ckcore.tr.addChangeOffer(ps.iPlayerId, offer);
        sReturn = "Sie haben sich mit dem Spieler " + plContract.plGame.sName + " auf eine Zusammenarbeit über " + ps.iYears.ToString() + " Jahre geeinigt.";
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

      CornerkickManager.Player player = MvcApplication.ckcore.ltPlayer[iPlayer];
      //uint iSallery = MvcApplication.ckcore.plr.getSalary(player, (byte)iYears, 0);
      player.contract.iLength += (byte)iYears;
      player.contract.iSalary = iSalary;
      MvcApplication.ckcore.ltPlayer[iPlayer] = player;

      return Json("Der Vertrag mit " + player.plGame.sName + " wurde um " + iYears.ToString() + " Jahre verlängert.", JsonRequestBehavior.AllowGet);
    }

    public JsonResult GetPlayerName(int iPlayer)
    {
      if (iPlayer < 0) return Json("", JsonRequestBehavior.AllowGet);

      return Json(MvcApplication.ckcore.ltPlayer[iPlayer].plGame.sName, JsonRequestBehavior.AllowGet);
    }

    /*
    // cupBonus: [][cup id, place, value]
    [Serializable]
    public class CupBonus
    {
      public int iCupId { get; set; }
      public int iPlace { get; set; }
      public int iValue { get; set; }
    }
    */
    public class ContractQuotient
    {
      public int iPlayerId { get; set; }
      public byte iYears { get; set; }
      public int iSalaryOff { get; set; }
      public int iBonusPlayOff { get; set; }
      public int iBonusPointOff { get; set; }
      public int iBonusGoalOff { get; set; }
      public int[] iaCupBonus { get; set; }
      public int iFixTransferFee { get; set; }
    }
    [HttpPost]
    //public JsonResult getContractQuotientOfferedRequired(int iPlayerId, byte iYears, int iSalaryOff, int iBonusPlayOff, int iBonusPointOff, int iBonusGoalOff, string[] ltCupBonus)
    public JsonResult getContractQuotientOfferedRequired(ContractQuotient cq)
    {
      if (cq.iPlayerId < 0) return Json("Invalid player", JsonRequestBehavior.AllowGet);

      CornerkickManager.Club clb = ckClub();
      if (clb == null) return Json(false, JsonRequestBehavior.AllowGet);

      const int iGamesPerSeason = 30;

      CornerkickManager.Player pl = MvcApplication.ckcore.ltPlayer[cq.iPlayerId];

      // Add current contract years
      byte iYearsReq = cq.iYears;
      if (!checkIfNewContract(pl, clb)) {
        iYearsReq += pl.contract.iLength;
      }

      // Get cup bonus list
      List<CornerkickManager.Player.Contract.CupBonus> ltCupBonus = getCupBonus(cq.iaCupBonus, pl.contract.club);

      int iSalaryTotReq = CornerkickManager.PlayerTool.getSalaryTotalRequired(pl, iYearsReq, MvcApplication.ckcore.dtDatum, MvcApplication.ckcore.dtSeasonEnd, iFixedFee: cq.iFixTransferFee);
      int iSalaryTotOff = CornerkickManager.PlayerTool.getSalaryTotal(iSalary: cq.iSalaryOff, iBonusPlay: cq.iBonusPlayOff, iBonusPoint: cq.iBonusPointOff, iBonusGoal: cq.iBonusGoalOff, ltCupBonus: ltCupBonus, clbPlayer: clb, iGamesPerSeason: iGamesPerSeason, fBonusGoalFactor: pl.getGoalBonusFactor());

      return Json(iSalaryTotOff / (float)iSalaryTotReq, JsonRequestBehavior.AllowGet);
    }

    private List<CornerkickManager.Player.Contract.CupBonus> getCupBonus(int[] iaCupBonus, CornerkickManager.Club clb)
    {
      if (iaCupBonus == null) return null;

      List<CornerkickManager.Player.Contract.CupBonus> ltCupBonus = null;
      int iCb = 0;
      for (int jCb = 0; jCb < iaCupBonus.Length; jCb++) {
        if (jCb % 3 == 0) {
          if (ltCupBonus == null) ltCupBonus = new List<CornerkickManager.Player.Contract.CupBonus>();

          CornerkickManager.Cup cup = null;
          if      (iaCupBonus[jCb] == 1) cup = MvcApplication.ckcore.tl.getCup(iaCupBonus[jCb], clb.iLand, clb.iDivision);
          else if (iaCupBonus[jCb] == 2) cup = MvcApplication.ckcore.tl.getCup(iaCupBonus[jCb], clb.iLand);
          else cup = MvcApplication.ckcore.tl.getCup(iaCupBonus[jCb]);
          jCb++;

          ltCupBonus.Add(
            new CornerkickManager.Player.Contract.CupBonus() {
              cup = cup,
              iPlace = (byte)iaCupBonus[jCb++],
              iValue = iaCupBonus[jCb]
            }
          );
        }
      }

      return ltCupBonus;
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
        foreach (CornerkickManager.Player plJ in clb.ltPlayerJouth) {
          /*
          // Change Birthday if too young
          if (MvcApplication.ckcore.game.tl.getPlayerAgeFloat(sp, MvcApplication.ckcore.dtDatum) < 15) {
            sp.dtGeburt = new DateTime(sp.dtGeburt.Year - 5, sp.dtGeburt.Month, sp.dtGeburt.Day);
            MvcApplication.ckcore.ltPlayer[iSp] = sp;
          }
          */

          Models.JouthModel.ltPlayerJouth.Add(plJ.plGame);
        }
      }

      return View(jouth);
    }

    public JsonResult JouthGetDatatable()
    {
      List<Models.JouthModel.DatatableJouth> ltDtJouth = new List<Models.JouthModel.DatatableJouth>();

      CornerkickManager.Club clb = ckClub();
      if (clb == null) return Json(null, JsonRequestBehavior.AllowGet);

      foreach (CornerkickManager.Player plJ in clb.ltPlayerJouth) {
        Models.JouthModel.DatatableJouth dtJouth = new Models.JouthModel.DatatableJouth();

        dtJouth.iId = plJ.plGame.iId;
        dtJouth.sName = plJ.plGame.sName;
        dtJouth.fAge = plJ.plGame.getAge(MvcApplication.ckcore.dtDatum);
        dtJouth.sPos = CornerkickManager.PlayerTool.getStrPos(plJ);
        dtJouth.fSkillAve = CornerkickGame.Tool.getAveSkill(plJ.plGame);
        dtJouth.fTalentAve = plJ.getTalentAve() + 1f;
        dtJouth.sNat = CornerkickManager.Main.sLandShort[plJ.iNat1];

        ltDtJouth.Add(dtJouth);
      }

      return Json(new { aaData = ltDtJouth }, JsonRequestBehavior.AllowGet);
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
      CornerkickManager.User usr = ckUser();
      if (usr == null) return View(mdTransfer);

      CornerkickManager.Club clb = ckClub();
      if (clb == null) return View(mdTransfer);

      mdTransfer.bSound = true;
      if (usr.lti.Count > 1) mdTransfer.bSound = usr.lti[1] > 0;

      mdTransfer.bTransferlistOpen = MvcApplication.ckcore.dtDatum.Date.CompareTo(MvcApplication.ckcore.dtSeasonStart.Date) > 0;

      mdTransfer.iContractYears = 1;

      mdTransfer.ddlFilterLeague.Add(new SelectListItem { Text = "Alle", Value = "", Selected = true });
      mdTransfer.ddlFilterLeague.Add(new SelectListItem { Text = "Computer", Value = "0" });
      foreach (CornerkickManager.Cup league in MvcApplication.ckcore.ltCups) {
        if (league.iId == 1) {
          mdTransfer.ddlFilterLeague.Add(new SelectListItem { Text = league.sName, Value = league.iId2.ToString() + ":" + league.iId3.ToString() });
        }
      }
      mdTransfer.ddlFilterLeague.Add(new SelectListItem { Text = "vereinslos", Value = "-1" });

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

      CornerkickManager.Player pl = MvcApplication.ckcore.ltPlayer[iPlayerId];

      if (MvcApplication.ckcore.plt.onTransferlist(pl)) {
        for (int iT = 0; iT < MvcApplication.ckcore.ltTransfer.Count; iT++) {
          CornerkickManager.Transfer.Item transfer = MvcApplication.ckcore.ltTransfer[iT];

          if (transfer.player == pl) {
            MvcApplication.ckcore.ltTransfer.RemoveAt(iT);
            break;
          }
        }

        return Json("Der Spieler " + pl.plGame.sName + " wurde von der Transferliste genommen", JsonRequestBehavior.AllowGet);
      } else {
        if (MvcApplication.ckcore.tr.putPlayerOnTransferlist(iPlayerId, 0) == 2) {
          return Json("Der Spieler " + MvcApplication.ckcore.ltPlayer[iPlayerId].plGame.sName + " kann in dieser Saison den Verein nicht mehr wechseln", JsonRequestBehavior.AllowGet);
        }

        if (checkIfTop10Player(pl)) {
          string sNewsPaper1 = pl.plGame.sName + " steht zum Verkauf!";

          string sNewsPaper2 = "";
          CornerkickManager.Club clbPlayer = null;
          if (pl.contract != null) clbPlayer = pl.contract.club;
          if (clbPlayer != null) {
            sNewsPaper2 = "Nach über&shy;ein&shy;stimmenden Medien&shy;berichten stehen die Zeichen zwischen ";
            sNewsPaper2 += clbPlayer.sName.Replace(" ", "&nbsp;");
            sNewsPaper2 += " und " + pl.plGame.sName + " (" + ((int)pl.plGame.getAge(MvcApplication.ckcore.dtDatum)).ToString() + " Jahre, " + CornerkickManager.PlayerTool.getStrPos(pl) + ", " + (pl.getValue(MvcApplication.ckcore.dtDatum, MvcApplication.ckcore.dtSeasonEnd) / 1000.0).ToString("0.0") + " mio. MW) auf Abschied.";
            //sNewsPaper2 += " Die kolportierte Ablösesumme soll bei ca. " + (pl.getValue(MvcApplication.ckcore.dtDatum) / 1000).ToString("0.0") + " mio. liegen";
          }
          MvcApplication.ckcore.sendNews(MvcApplication.ckcore.ltUser[0], sNewsPaper1 + "#" + sNewsPaper2, iType: 200, iId: pl.plGame.iId);
        }
        
        return Json("Der Spieler " + MvcApplication.ckcore.ltPlayer[iPlayerId].plGame.sName + " wurde auf die Transferliste gesetzt", JsonRequestBehavior.AllowGet);
      }

      return Json(null, JsonRequestBehavior.AllowGet);
    }

    private bool checkIfTop10Player(CornerkickManager.Player pl)
    {
      for (byte iPos = 1; iPos <= 11; iPos++) {
        foreach (CornerkickManager.Player plB in MvcApplication.ckcore.getBestPlayer(iPlCount: 10, iPos: iPos, fAgeMax: pl.plGame.getAge(MvcApplication.ckcore.dtDatum))) {
          if (pl.plGame.iId == plB.plGame.iId) return true;
        }
      }

      return false;
    }

    [HttpPost]
    public JsonResult TransferMakeOffer(int iPlayerId, int iTransferFee, int iTransferFeeSecret)
    {
      if (iPlayerId <                                     0) return Json(null, JsonRequestBehavior.AllowGet);
      if (iPlayerId >= MvcApplication.ckcore.ltPlayer.Count) return Json(null, JsonRequestBehavior.AllowGet);

      CornerkickManager.Player pl = MvcApplication.ckcore.ltPlayer[iPlayerId];

      string sReturn = "Error";

      CornerkickManager.Club club = ckClub();
      if (club == null) return Json(false, JsonRequestBehavior.AllowGet);

      CornerkickManager.Club clbGive = null;
      if (pl.contract != null) clbGive = pl.contract.club;

      // If no club ...
      if (clbGive == null) {
        // ... and not on transferlist already --> put on transferlist
        if (!MvcApplication.ckcore.plt.onTransferlist(pl)) MvcApplication.ckcore.tr.putPlayerOnTransferlist(pl, 0);
      }

      for (int iT = 0; iT < MvcApplication.ckcore.ltTransfer.Count; iT++) {
        CornerkickManager.Transfer.Item transfer = MvcApplication.ckcore.ltTransfer[iT];

        if (transfer.player == pl) {
          if (transfer.ltOffers != null) {
            for (int iO = 0; iO < transfer.ltOffers.Count; iO++) {
              CornerkickManager.Transfer.Offer offer = transfer.ltOffers[iO];

              if (offer.contract.club == club) {
                if (!MvcApplication.ckcore.fz.checkDispoLimit(iTransferFee, club)) {
                  transfer.ltOffers.Remove(offer);
                  return Json("Ihr Kreditrahmen ist leider nicht hoch genug", JsonRequestBehavior.AllowGet);
                }

                if (iTransferFeeSecret > club.iBalanceSecret) {
                  transfer.ltOffers.Remove(offer);
                  return Json("Sie haben nicht genug Schwarzgeld...", JsonRequestBehavior.AllowGet);
                }

                // No club
                if (clbGive == null) {
                  offer.iFee = 0;
                  offer.iFeeSecret = 0;
                  if (MvcApplication.ckcore.tr.transferPlayer(clbGive, iPlayerId, club)) {
                    sReturn = "Sie haben den vereinslosen Spieler " + pl.plGame.sName + " ablösefrei unter Vertrag genommen.";
                  }
                  break;
                }

                // Ending contract
                if (offer.bNextSeason && CornerkickManager.PlayerTool.checkIfContractIsEnding(transfer.player, MvcApplication.ckcore.dtDatum, MvcApplication.ckcore.dtSeasonEnd)) {
                  offer.iFee = 0;
                  offer.iFeeSecret = 0;
                  if (MvcApplication.ckcore.tr.transferPlayer(clbGive, iPlayerId, club, bNextSeason: true)) {
                    sReturn = "Sie haben den Spieler " + pl.plGame.sName + " ablösefrei für die nächste Saison verpflichtet.";
                    createNewspaperPlayerTransfer(pl, club, -1);
                  }
                  break;
                }

                // Fix transfer fee
                if (pl.contract != null && pl.contract.iFixTransferFee > 0) {
                  offer.iFee = pl.contract.iFixTransferFee;
                  offer.iFeeSecret = 0;
                  if (MvcApplication.ckcore.tr.transferPlayer(clbGive, iPlayerId, club, iTransferIx: iT)) {
                    sReturn = "Sie haben den Spieler " + pl.plGame.sName + " für die festgeschriebene Ablöse von " + offer.iFee.ToString("N0", getCi()) + " verpflichtet.";
                    MvcApplication.ckcore.sendNews(clbGive.user, "Ihr Spieler " + pl.plGame.sName + " wechselt mit sofortiger Wirkung für die festgeschriebene Ablöse von " + offer.iFee.ToString("N0", getCi()) + " zu " + club.sName, iType: CornerkickManager.Main.iNewsTypePlayerTransferOfferAccept, iId: iPlayerId);

                    createNewspaperPlayerTransfer(pl, club, pl.contract.iFixTransferFee);
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
                  MvcApplication.ckcore.sendNews(clbGive.user, "Sie haben ein neues Transferangebot für den Spieler " + pl.plGame.sName + " erhalten!", iType: CornerkickManager.Main.iNewsTypePlayerTransferNewOffer, iId: iPlayerId);
                  sReturn = "Sie haben das Transferangebot für dem Spieler " + pl.plGame.sName + " erfolgreich abgegeben.";

                  bool bNewspaperTalent = pl.getTalentAve() > 5f && pl.plGame.getAge(MvcApplication.ckcore.dtDatum) < 23;
                  if (bNewspaperTalent || checkIfTop10Player(pl)) {
                    string sTalent = "";
                    if (bNewspaperTalent) sTalent = "Talent ";
                    string sNewsPaper1 = club.sName + " vor Verpflichtung von " + sTalent + pl.plGame.sName;
                    string sNewsPaper2 = "Angeblich steht " + club.sName + " kurz vor der Verpflichtung von " + pl.plGame.sName + " (" + ((int)pl.plGame.getAge(MvcApplication.ckcore.dtDatum)).ToString() + " Jahre, " + CornerkickManager.PlayerTool.getStrPos(pl) + ", " + (pl.getValue(MvcApplication.ckcore.dtDatum, MvcApplication.ckcore.dtSeasonEnd) / 1000.0).ToString("0.0") + " mio. MW).";
                    MvcApplication.ckcore.sendNews(MvcApplication.ckcore.ltUser[0], sNewsPaper1 + "#" + sNewsPaper2, iType: 200, iId: pl.plGame.iId);
                  }
                }

                pl.plGame.character.fMoney += 0.05f;

                break;
              }
            }
          }
        }
      }

      return Json(sReturn, JsonRequestBehavior.AllowGet);
    }

    private void createNewspaperPlayerTransfer(CornerkickManager.Player pl, CornerkickManager.Club clbTake, int iTransferFee)
    {
      if (pl == null) return;
      if (clbTake == null) return;

      // Create news
      if (checkIfTop10Player(pl)) {
        if (iTransferFee < 0) {
          string sNewsPaper1 = pl.plGame.sName + " wechselt zu " + clbTake.sName;
          string sNewsPaper2 = "Wie heute bekannt gegeben wurde, schließt sich " + pl.plGame.sName + " (" + ((int)pl.plGame.getAge(MvcApplication.ckcore.dtDatum)).ToString() + " Jahre, " + CornerkickManager.PlayerTool.getStrPos(pl) + ", " + (pl.getValue(MvcApplication.ckcore.dtDatum, MvcApplication.ckcore.dtSeasonEnd) / 1000.0).ToString("0.0") + " mio. MW) zur neuen Saison " + clbTake.sName + " an.";
          MvcApplication.ckcore.sendNews(MvcApplication.ckcore.ltUser[0], sNewsPaper1 + "#" + sNewsPaper2, iType: 200, iId: pl.plGame.iId);
        } else {
          string sNewsPaper1 = pl.plGame.sName + " bei " + clbTake.sName + " vorgestellt";
          string sNewsPaper2 = "Auf der heutigen Presse&shy;konferenz wurde " + pl.plGame.sName + " (" + ((int)pl.plGame.getAge(MvcApplication.ckcore.dtDatum)).ToString() + " Jahre, " + CornerkickManager.PlayerTool.getStrPos(pl) + ", " + (pl.getValue(MvcApplication.ckcore.dtDatum, MvcApplication.ckcore.dtSeasonEnd) / 1000.0).ToString("0.0") + " mio. MW) offiziell vorgestellt. Die Ablöse&shy;summe soll angeblich bei " + (iTransferFee / 1000000.0).ToString("0.0") + " mio. liegen.";
          MvcApplication.ckcore.sendNews(MvcApplication.ckcore.ltUser[0], sNewsPaper1 + "#" + sNewsPaper2, iType: 200, iId: pl.plGame.iId);
        }
      }
    }

    public JsonResult AcceptTransferOffer(int iPlayerId, int iClubId)
    {
      string sReturn = "Error";

      CornerkickManager.Player pl = MvcApplication.ckcore.ltPlayer[iPlayerId];

      CornerkickManager.Club clubTake = MvcApplication.ckcore.ltClubs[iClubId];
      CornerkickManager.Transfer.Offer offer = MvcApplication.ckcore.tr.getOffer(pl, clubTake);

      if (MvcApplication.ckcore.tr.transferPlayer(ckClub(), iPlayerId, clubTake)) {
        // Create news
        createNewspaperPlayerTransfer(pl, clubTake, offer.iFee);

        sReturn = "Sie haben das Transferangebot für dem Spieler " + pl.plGame.sName + " angenommen. Er wechselt mit sofortiger Wirkung zu " + clubTake.sName;
      }

      return Json(sReturn, JsonRequestBehavior.AllowGet);
    }

    [HttpPost]
    public JsonResult CancelTransferOffer(int iPlayerId)
    {
      string sReturn = "Error";

      if (MvcApplication.ckcore.tr.cancelTransferOffer(iPlayerId, ckClub())) {
        CornerkickManager.Player player = MvcApplication.ckcore.ltPlayer[iPlayerId];
        player.plGame.character.fMoney -= 0.05f;
        sReturn = "Sie haben Ihr Transferangebot für dem Spieler " + player.plGame.sName + " zurückgezogen.";
      }

      return Json(sReturn, JsonRequestBehavior.AllowGet);
    }

    [HttpPost]
    public JsonResult TransferAddToRemFromFavorites(int iPlayerId)
    {
      CornerkickManager.User usr = ckUser();
      if (usr == null) return Json("", JsonRequestBehavior.AllowGet);

      CornerkickManager.Player plFav = MvcApplication.ckcore.ltPlayer[iPlayerId];
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

      CornerkickManager.Player plNom = MvcApplication.ckcore.ltPlayer[iPlayerId];

      if (plNom.iNat1 != nation.iLand) return Json(null, JsonRequestBehavior.AllowGet);

      bool bNominate = !CornerkickManager.PlayerTool.ownPlayer(nation, plNom);
      if (bNominate) nation.ltPlayer.Add   (plNom);
      else           nation.ltPlayer.Remove(plNom);

      return Json(bNominate, JsonRequestBehavior.AllowGet);
    }

    public ActionResult TransferGetDataTable(int iPos, int iFType, int iFValue, bool bJouth, int iType, bool bFixTransferFee, bool bEndingContract, int iClubId = -9, int iNation = -1)
    {
      //The table or entity I'm querying
      List<Models.DatatableEntryTransfer> ltDeTransfer = new List<Models.DatatableEntryTransfer>();

      //int iClub = -9;
      //if (bNoClub) iClub = -1;

      CornerkickManager.User usr = ckUser();

      List<CornerkickManager.Player> ltPlayer = null;
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
                                                                                                       iFType: iFType,
                                                                                                       iF: iFValue,
                                                                                                       bJouth: bJouth,
                                                                                                       ltPlayer: ltPlayer,
                                                                                                       bFixTransferFee: bFixTransferFee,
                                                                                                       bEndingContract: bEndingContract,
                                                                                                       iNation: iNation)) {
        try {
          string sClub = "vereinslos";
          if (transfer.player?.contract?.club != null) {
            if (transfer.player.contract.club.bNation) continue;

            sClub = transfer.player.contract.club.sName;
          }

          int iOffer = 0; // -2: not on transfer list, -1: negotiation cancelled, +1: already offered, +2: own player
          int iFixtransferfee = 0;

          CornerkickManager.Club clubUser = ckClub();
          if (clubUser.iId > 0) {
            if      (MvcApplication.ckcore.tr.negotiationCancelled(clubUser, transfer.player)) iOffer = -1;
            else if (MvcApplication.ckcore.tr.alreadyOffered      (clubUser, transfer.player)) iOffer = +1;
            else if (CornerkickManager.PlayerTool.ownPlayer       (clubUser, transfer.player)) iOffer = +2;
            // If player has a club (and a contract) and not fixed transfer fee and is not on transfer list: iOffer = -2
            else if (transfer.player.contract?.club != null && transfer.player.contract.iFixTransferFee < 1 && !MvcApplication.ckcore.plt.onTransferlist(transfer.player)) iOffer = -2;

            if (transfer.player.contract != null) iFixtransferfee = transfer.player.contract.iFixTransferFee;
          }

          string sDatePutOnTl = "-";
          if (transfer.dt.Year > 1) sDatePutOnTl = transfer.dt.ToString("d", getCi());

          ltDeTransfer.Add(new Models.DatatableEntryTransfer {
            playerId = transfer.player.plGame.iId,
            empty = "",
            iOffer = iOffer,
            index = (iTr + 1).ToString(),
            datum = sDatePutOnTl,
            name = transfer.player.plGame.sName,
            position = CornerkickManager.PlayerTool.getStrPos(transfer.player),
            strength = CornerkickGame.Tool.getAveSkill(transfer.player.plGame, bIdeal: false),
            strengthIdeal = CornerkickGame.Tool.getAveSkill(transfer.player.plGame, bIdeal: true),
            age = ((int)transfer.player.plGame.getAge(MvcApplication.ckcore.dtDatum)).ToString(),
            fTalentAve = transfer.player.getTalentAve() + 1f,
            mw = transfer.player.getValue(MvcApplication.ckcore.dtDatum, MvcApplication.ckcore.dtSeasonEnd) * 1000,
            fixtransferfee = iFixtransferfee,
            club = sClub,
            nat = CornerkickManager.Main.sLandShort[transfer.player.iNat1],
            bEndingContract = CornerkickManager.PlayerTool.checkIfContractIsEnding(transfer.player, MvcApplication.ckcore.dtDatum, MvcApplication.ckcore.dtSeasonEnd) && transfer.player.contractNext == null
          });
        } catch (Exception e) {
          MvcApplication.ckcore.tl.writeLog("Error in getTableTransfer(), iTr: " + iTr.ToString() + Environment.NewLine + e.Message + Environment.NewLine + e.Source + Environment.NewLine + e.Data + Environment.NewLine + e.StackTrace, CornerkickManager.Main.sErrorFile);
        }

        iTr++;
      }

      return Json(new { aaData = ltDeTransfer.ToArray() }, JsonRequestBehavior.AllowGet);
    }

    public ActionResult getTableTransferDetails2(int iPlayerId)
    {
      if (iPlayerId <                                     0) return Json(null, JsonRequestBehavior.AllowGet);
      if (iPlayerId >= MvcApplication.ckcore.ltPlayer.Count) return Json(null, JsonRequestBehavior.AllowGet);

      string sTable = "";

      CornerkickManager.Player pl = MvcApplication.ckcore.ltPlayer[iPlayerId];

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

          bool bOwnPlayer = CornerkickManager.PlayerTool.ownPlayer(clubUser, MvcApplication.ckcore.ltPlayer[iPlayerId]);

          foreach (CornerkickManager.Transfer.Offer offer in transfer.ltOffers) {
            if (bOwnPlayer || // If own player
                clubUser == offer.contract?.club) {
              string sClub = "vereinslos";
              if (offer.contract.club != null) {
                sClub = offer.contract.club.sName;
              }

              string sStyle = "";
              if (offer.contract.club == clubUser) sStyle = " style=\"font-weight:bold\"";

              sTable += "<tr id=\"rowTransferDetail_" + offer.contract.club.iId.ToString() + "\" class=\"noSelect\"" + sStyle + ">";
              sTable += "<td>" + (iTr + 1).ToString() + "</td>";
              sTable += "<td align=\"center\">" + offer.dt.ToString("d", getCi()) + "</td>";
              sTable += "<td align=\"center\">" + sClub + "</td>";
              sTable += "<td align=\"right\">" + offer.iFee.ToString("N0", getCi()) + " €" + "</td>";

              if (bOwnPlayer) {
                string sChecked = "";
                //if (iTr == 0) sChecked = " checked";
                sTable += "<td class=\"select-checkbox\"><input type=\"radio\" id=\"rB_OfferClubId_" + iTr.ToString() + "\" name=\"OfferClubId\" onclick=\"handleClick(this);\" value =\"" + offer.contract.club.iId.ToString() + "\"" + sChecked + " style=\"cursor: pointer\"></td>";
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

    public JsonResult TransferGetDdlClubFilter(string sLeagueId)
    {
      List<string[]> ltClubs = new List<string[]>();

      if (sLeagueId.Equals("0") || sLeagueId.Equals("-1")) {
        ltClubs.Add(new string[] { sLeagueId, ""} );
        return Json(ltClubs, JsonRequestBehavior.AllowGet);
      }

      string[] sLeagueIdSplit = sLeagueId.Split(':');
      if (sLeagueIdSplit.Length < 2) return Json(null, JsonRequestBehavior.AllowGet);

      int iLeagueId2 = int.Parse(sLeagueIdSplit[0]);
      int iLeagueId3 = int.Parse(sLeagueIdSplit[1]);

      CornerkickManager.Cup league = MvcApplication.ckcore.tl.getCup(1, iLeagueId2, iLeagueId3);

      // Spieltage zu Dropdown Menü hinzufügen
      foreach (CornerkickManager.Club clb in league.ltClubs[0]) {
        string[] sClub = new string[2];
        sClub[0] = clb.iId.ToString();
        sClub[1] = clb.sName;
        ltClubs.Add(sClub);
      }

      return Json(ltClubs, JsonRequestBehavior.AllowGet);
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

          CornerkickManager.Player pl = clb.ltPlayer[iPl];
          tactic.ltDdlStandards[iS].Add(new SelectListItem { Text = pl.plGame.sName, Value = iPl.ToString(), Selected = iPl == clb.iStandards[iS] });
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

      // Tutorial
      if (ttUser != null) {
        int iUserIx = MvcApplication.ckcore.ltUser.IndexOf(ckUser());
        if (iUserIx >= 0 && iUserIx < ttUser.Length) {
          tactic.tutorial = ttUser[iUserIx];
          if (tactic.tutorial.iLevel == 24) tactic.tutorial.iLevel = 25;
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
      } else if (iTaktik == 9) clb.ltTactic[iTactic].iGapOffsite = (int)Math.Round(fTaktik);

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
        CornerkickManager.Player pl = clb.ltPlayer[iPl];

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
        pl.plGame.iIndex = iPl;
        if (iPl < MvcApplication.ckcore.game.data.nPlStart) {
          byte iPosRole = CornerkickGame.Tool.getBasisPos(CornerkickGame.Tool.getPosRole(pl.plGame, clb.ltTactic[0].formation, MvcApplication.ckcore.game.ptPitch));
          sPos = CornerkickManager.Main.sPosition[iPosRole];
          sStrength = CornerkickGame.Tool.getAveSkill(pl.plGame, iPos: iPosRole, bIdeal: false).ToString(" (0.0)");
        } else {
          sPos = CornerkickManager.PlayerTool.getStrPos(pl);
          sStrength = CornerkickGame.Tool.getAveSkill(pl.plGame, bIdeal: false).ToString(" (0.0)");
        }

        if (bOut) sBox[0] += "<option" + sSelectedO + " value=\"" + iPl.ToString() + "\">" + pl.plGame.sName + " - " + sPos + sStrength + "</option>";
        else      sBox[1] += "<option" + sSelectedI + " value=\"" + iPl.ToString() + "\">" + pl.plGame.sName + " - " + sPos + sStrength + "</option>";
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

      while (clb.nextGame.team[iHA].ltSubstitutionsPlanned.Count <= iAS + 1) clb.nextGame.team[iHA].ltSubstitutionsPlanned.Add(new byte[3] { 0, 0, 0 });

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

      mdTraining.iTrainingCount = new int[MvcApplication.ckcore.plt.ltTraining.Count];

      int nTrTotal = 0;
      foreach (CornerkickManager.Player.TrainingHistory th in clb.ltTrainingHist) {
        bool bFound = false;
        foreach (TimeSpan tsTr in tsTraining) {
          if (tsTr.Equals(th.dt.TimeOfDay)) {
            bFound = true;
            break;
          }
        }
        if (!bFound) continue;

        if (th.iType > 0 && th.iType < mdTraining.iTrainingCount.Length) {
          mdTraining.iTrainingCount[th.iType]++;
          nTrTotal++;
        }
      }

      mdTraining.sTrainingCountRel = new string[mdTraining.iTrainingCount.Length];
      for (int iT = 0; iT < mdTraining.iTrainingCount.Length; iT++) {
        if (nTrTotal > 0) {
          mdTraining.sTrainingCountRel[iT] = (mdTraining.iTrainingCount[iT] / (float)nTrTotal).ToString("0.0%");
        } else {
          mdTraining.sTrainingCountRel[iT] = "-";
        }
      }

      // Tutorial
      if (ttUser != null) {
        CornerkickManager.User usr = ckUser();
        int iUserIx = MvcApplication.ckcore.ltUser.IndexOf(usr);
        if (iUserIx >= 0 && iUserIx < ttUser.Length) {
          mdTraining.tutorial = ttUser[iUserIx];
          if (mdTraining.tutorial.iLevel == 15) mdTraining.tutorial.iLevel = 16;
        }
      }

      return View(mdTraining);
    }

    internal static CornerkickManager.Main.TrainingPlan.Unit[][] getTrainingPlan(CornerkickManager.Club clb, int iWeek)
    {
      if (clb == null) return null;

      // Get last Sunday
      DateTime dtSunday = MvcApplication.ckcore.dtDatum.Date.AddDays(iWeek * 7);
      while (dtSunday.DayOfWeek != DayOfWeek.Sunday) dtSunday = dtSunday.AddDays(-1);

      List<CornerkickGame.Game.Data> ltNextGames = MvcApplication.ckcore.tl.getNextGames(clb, dtSunday);

      CornerkickManager.Main.TrainingPlan.Unit[][] ltTu = new CornerkickManager.Main.TrainingPlan.Unit[7][]; // For each day of week
      for (int iD = 0; iD < ltTu.Length; iD++) { // Loop until Saturday
        ltTu[iD] = new CornerkickManager.Main.TrainingPlan.Unit[3];

        // Add past trainings
        foreach (CornerkickManager.Player.TrainingHistory th in clb.ltTrainingHist) {
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

        // Add future trainings
        foreach (CornerkickManager.Main.TrainingPlan.Unit tu in clb.training.ltUnit) {
          if (tu.dt.Date.Equals(dtSunday.AddDays(iD))) {
            int iIxTimeOfDay = 0; // 1st training
            if      (tu.dt.Hour >= tsTraining[2].Hours) iIxTimeOfDay = 2; // 3rd training
            else if (tu.dt.Hour >= tsTraining[1].Hours) iIxTimeOfDay = 1; // 2nd training

            //if (tu.dt.CompareTo(MvcApplication.ckcore.dtDatum) <= 0) tu.iType *= -1;

            ltTu[iD][iIxTimeOfDay] = tu;
          }
        }
      }

      List<CornerkickGame.Game.Data> ltGames = MvcApplication.ckcore.tl.getNextGames(clb, dtSunday);

      for (int iD = 0; iD < ltTu.Length; iD++) { // Loop until Saturday
        for (int iT = 0; iT < ltTu[iD].Length; iT++) {
          // Set training type 'free' if null
          if (ltTu[iD][iT] == null) {
            DateTime dtTraining = dtSunday.AddDays(iD).Add(tsTraining[iT]);

            sbyte iType = 0;
            if (dtTraining.CompareTo(MvcApplication.ckcore.dtDatum) <= 0) iType = -1; // Past training

            ltTu[iD][iT] = new CornerkickManager.Main.TrainingPlan.Unit() { dt = dtTraining, iType = iType };
          }

          // Set game/travel/event
          int iEvent = CornerkickManager.Tool.checkIfGameTravelEventIsClose(clb, ltTu[iD][iT].dt, ltGames: ltGames);
          if (iEvent == 0) iEvent = CornerkickManager.Tool.checkIfGameTravelEventIsClose(clb, ltTu[iD][iT].dt.Add(MvcApplication.ckcore.settings.tsTrainingLength), ltGames: ltGames);

          if (iEvent > 0) {
            ltTu[iD][iT].iType = (sbyte)(100 + iEvent);
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
      while (dtStartCopy.DayOfWeek != DayOfWeek.Sunday) dtStartCopy = dtStartCopy.AddDays(+1);

      // First delete all trainings starting from next week ...
      DateTime dtTmp = dtStartCopy;
      while (dtTmp.CompareTo(MvcApplication.ckcore.dtSeasonEnd) < 0) {
        for (int iT = 0; iT < clb.training.ltUnit.Count; iT++) {
          if (clb.training.ltUnit[iT].dt.Date.Equals(dtTmp)) {
            clb.training.ltUnit.RemoveAt(iT--);
          }
        }

        dtTmp = dtTmp.AddDays(+1);
      }

      // ... then copy trainings plan of current week
      dtTmp = dtStartCopy;
      while (dtTmp.CompareTo(MvcApplication.ckcore.dtSeasonEnd) < 0) {
        for (byte iD = 0; iD < tuPlan.Length; iD++) {
          for (byte iT = 0; iT < tuPlan[iD].Length; iT++) {
            if (tuPlan[iD][iT].iType != 0 && tuPlan[iD][iT].iType < 100) { // // Not free and not game
              CornerkickManager.Main.TrainingPlan.Unit tuCopy = tuPlan[iD][iT].Clone();
              tuCopy.dt = dtTmp.Add(tuPlan[iD][iT].dt.TimeOfDay);

              if (tuCopy.iType < 0) tuCopy.iType = (sbyte)(-(tuCopy.iType + 1));

              clb.training.ltUnit.Add(tuCopy);
            }
          }

          dtTmp = dtTmp.AddDays(+1);
        }
      }
    }

    public class TrainingWeekTemplate
    {
      public string sName { get; set; }
      public CornerkickManager.Main.TrainingPlan.Unit[][] tuPlan { get; set; }
    }
    public static List<TrainingWeekTemplate> ltTrainingWeekTemplate;

    [HttpPost]
    public JsonResult TrainingSetTemplate(int iWeek, int iType)
    {
      CornerkickManager.Club clb = ckClub();
      if (clb == null) return Json(false, JsonRequestBehavior.AllowGet);

      setTrainingWeekTemplate(clb, iWeek, iType);

      return Json(true, JsonRequestBehavior.AllowGet);
    }

    public static void setTrainingWeekTemplate(CornerkickManager.Club clb, int iWeek, int iType)
    {
      if (clb == null) return;

      // Define training week templates
      ltTrainingWeekTemplate = new List<TrainingWeekTemplate>();

      // Condition
      TrainingWeekTemplate twt1 = new TrainingWeekTemplate();
      twt1.sName = "Kondition";
      twt1.tuPlan = new CornerkickManager.Main.TrainingPlan.Unit[7][];
      for (int iD = 0; iD < 7; iD++) {
        twt1.tuPlan[iD] = new CornerkickManager.Main.TrainingPlan.Unit[3];

        for (int iT = 0; iT < twt1.tuPlan[iD].Length; iT++) {
          twt1.tuPlan[iD][iT] = new CornerkickManager.Main.TrainingPlan.Unit();
        }
      }
      twt1.tuPlan[1][0].iType = 2;
      twt1.tuPlan[1][1].iType = 2;
      twt1.tuPlan[1][2].iType = 1;
      twt1.tuPlan[2][0].iType = 2;
      twt1.tuPlan[2][1].iType = 3;
      twt1.tuPlan[2][2].iType = 9;
      twt1.tuPlan[3][0].iType = 2;
      twt1.tuPlan[3][1].iType = 4;
      twt1.tuPlan[3][2].iType = 1;
      twt1.tuPlan[4][0].iType = 3;
      twt1.tuPlan[4][1].iType = 5;
      twt1.tuPlan[4][2].iType = 6;
      twt1.tuPlan[5][0].iType = 2;
      twt1.tuPlan[5][1].iType = 3;
      twt1.tuPlan[6][1].iType = 1;
      ltTrainingWeekTemplate.Add(twt1);

      // Regeneration
      TrainingWeekTemplate twt2 = new TrainingWeekTemplate();
      twt2.sName = "Kondition";
      twt2.tuPlan = new CornerkickManager.Main.TrainingPlan.Unit[7][];
      for (int iD = 0; iD < 7; iD++) {
        twt2.tuPlan[iD] = new CornerkickManager.Main.TrainingPlan.Unit[3];

        for (int iT = 0; iT < twt2.tuPlan[iD].Length; iT++) {
          twt2.tuPlan[iD][iT] = new CornerkickManager.Main.TrainingPlan.Unit();
        }
      }
      twt2.tuPlan[1][0].iType = 1;
      twt2.tuPlan[1][1].iType = 10;
      twt2.tuPlan[2][0].iType = 7;
      twt2.tuPlan[2][1].iType = 1;
      twt2.tuPlan[2][2].iType = 12;
      twt2.tuPlan[3][0].iType = 6;
      twt2.tuPlan[3][1].iType = 1;
      twt2.tuPlan[4][0].iType = 3;
      twt2.tuPlan[4][1].iType = 4;
      twt2.tuPlan[4][2].iType = 1;
      twt2.tuPlan[5][0].iType = 8;
      twt2.tuPlan[5][1].iType = 1;
      twt2.tuPlan[6][1].iType = 1;
      ltTrainingWeekTemplate.Add(twt2);

      // Mixed
      TrainingWeekTemplate twt3 = new TrainingWeekTemplate();
      twt3.sName = "Ausgeglichen";
      twt3.tuPlan = new CornerkickManager.Main.TrainingPlan.Unit[7][];
      for (int iD = 0; iD < 7; iD++) {
        twt3.tuPlan[iD] = new CornerkickManager.Main.TrainingPlan.Unit[3];

        for (int iT = 0; iT < twt3.tuPlan[iD].Length; iT++) {
          twt3.tuPlan[iD][iT] = new CornerkickManager.Main.TrainingPlan.Unit();
        }
      }
      twt3.tuPlan[1][0].iType = 2;
      twt3.tuPlan[1][1].iType = 3;
      twt3.tuPlan[2][0].iType = 4;
      twt3.tuPlan[2][1].iType = 8;
      twt3.tuPlan[2][2].iType = 12;
      twt3.tuPlan[3][0].iType = 5;
      twt3.tuPlan[3][1].iType = 9;
      twt3.tuPlan[4][0].iType = 3;
      twt3.tuPlan[4][1].iType = 10;
      twt3.tuPlan[4][2].iType = 1;
      twt3.tuPlan[5][0].iType = 7;
      twt3.tuPlan[5][1].iType = 6;
      twt3.tuPlan[6][0].iType = 1;
      twt3.tuPlan[6][1].iType = 1;
      ltTrainingWeekTemplate.Add(twt3);

      if (iType < 0) return;
      if (iType >= ltTrainingWeekTemplate.Count) return;

      // Get last Sunday
      DateTime dtStartCopy = MvcApplication.ckcore.dtDatum.AddDays(iWeek * 7).Date;
      while (dtStartCopy.DayOfWeek != DayOfWeek.Sunday) dtStartCopy = dtStartCopy.AddDays(-1);

      // First delete all trainings in this week ...
      /*
      CornerkickManager.Main.TrainingPlan.Unit[][] tuPlan = getTrainingPlan(clb, iWeek);
      tuPlan = null;
      */
      DateTime dtTmp = dtStartCopy;
      while (dtTmp.CompareTo(dtStartCopy.AddDays(7)) < 0) {
        for (int iT = 0; iT < clb.training.ltUnit.Count; iT++) {
          if (clb.training.ltUnit[iT].dt.Date.Equals(dtTmp)) {
            clb.training.ltUnit.RemoveAt(iT--);
          }
        }

        dtTmp = dtTmp.AddDays(+1);
      }

      TrainingWeekTemplate twt = ltTrainingWeekTemplate[iType];
      for (int iD = 0; iD < 7; iD++) {
        for (int iT = 0; iT < twt.tuPlan[iD].Length; iT++) {
          DateTime dtCopy = dtStartCopy.AddDays(iD).Add(tsTraining[iT]);
          if (dtCopy.CompareTo(MvcApplication.ckcore.dtDatum) < 0) continue;

          CornerkickManager.Main.TrainingPlan.Unit tuCopy = twt.tuPlan[iD][iT].Clone();
          tuCopy.dt = dtCopy;
          clb.training.ltUnit.Add(tuCopy);
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

      stadionModel.bSound = true;
      if (usr.lti.Count > 1) stadionModel.bSound = usr.lti[1] > 0;

      stadionModel.stadion = clb.stadium;

      stadionModel.sName = clb.stadium.sName;

      stadionModel.iSeats       = new int[3];
      stadionModel.iSeatsConstr = new int[3];
      for (byte iBT = 0; iBT < 3; iBT++) {
        stadionModel.iSeats      [iBT] = clb.stadium.getSeats(iType: iBT, iModeConstruction: 0);
        stadionModel.iSeatsConstr[iBT] = clb.stadium.getSeats(iType: iBT, iModeConstruction: 1);
      }

      // Blocks construction table
      stadionModel.sBlocksConstrName  = new string[clb.stadium.blocks.Length];
      stadionModel.iBlocksConstrSeats = new int   [clb.stadium.blocks.Length];
      stadionModel.sBlocksConstrType  = new string[clb.stadium.blocks.Length];
      stadionModel.iBlocksConstrDays  = new int   [clb.stadium.blocks.Length];
      for (int i = 0; i < clb.stadium.blocks.Length; i++) {
        stadionModel.sBlocksConstrName [i] = clb.stadium.blocks[i].sName;
        stadionModel.iBlocksConstrSeats[i] = clb.stadium.blocks[i].iSeats;
        stadionModel.sBlocksConstrType [i] = CornerkickManager.Stadium.sBlocktype[clb.stadium.blocks[i].iType];
        stadionModel.iBlocksConstrDays [i] = clb.stadium.blocks[i].iSeatsDaysConstruct;
      }

      // Topring
      stadionModel.bTopring = bStadiumGetTopring(clb);

      if (clb.stadium.iVideoDaysConstruct == 0) clb.stadium.iVideoNew = clb.stadium.iVideo;
      stadionModel.iVideo = clb.stadium.iVideoNew;

      stadionModel.iSnackbarNew = (byte)Math.Max(clb.stadium.iSnackbarNew - clb.stadium.iSnackbar, 0);
      stadionModel.iToiletsNew  = (byte)Math.Max(clb.stadium.iToiletsNew  - clb.stadium.iToilets,  0);

      stadionModel.iSnackbarReq = (byte)CornerkickManager.UI.getRequiredFeature(clb, 0);
      stadionModel.iToiletsReq  = (byte)CornerkickManager.UI.getRequiredFeature(clb, 1);

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

    public JsonResult StadiumGetSeats()
    {
      CornerkickManager.Club clb = ckClub();
      if (clb == null) return Json(false, JsonRequestBehavior.AllowGet);

      int[] iSeats = new int[6];

      for (byte iBT = 0; iBT < 3; iBT++) {
        iSeats[iBT    ] = clb.stadium.getSeats(iType: iBT, iModeConstruction: 0);
        iSeats[iBT + 3] = clb.stadium.getSeats(iType: iBT, iModeConstruction: 1);
      }

      return Json(iSeats, JsonRequestBehavior.AllowGet);
    }

    public JsonResult StadiumGetBuildCost(int iBlock, int iSeats, int iType)
    {
      CornerkickManager.Club clb = ckClub();
      if (clb == null) return Json(false, JsonRequestBehavior.AllowGet);

      int iSeatsMax = CornerkickManager.Stadium.getMaxSeats(clb.stadium, (byte)iType);
      if (iSeatsMax > 0 && iSeats > iSeatsMax) return Json(iSeatsMax, JsonRequestBehavior.AllowGet);

      CornerkickGame.Stadium stadiumNew = clb.stadium.Clone();
      stadiumNew.blocks[iBlock].iSeats = iSeats;
      stadiumNew.blocks[iBlock].iType = (byte)iType;

      int[] iCostDays = CornerkickManager.Stadium.getCostDaysContructStadium(stadiumNew, clb.stadium, ckUser());
      int iDispoOk = 0;
      if (MvcApplication.ckcore.fz.checkDispoLimit(iCostDays[0], clb)) iDispoOk = 1;

      int[] iCostDaysDispo = new int[] { iCostDays[0], iCostDays[1], iDispoOk };

      return Json(iCostDaysDispo, JsonRequestBehavior.AllowGet);
    }

    public JsonResult StadiumChangeSet(int iBlock, int iSeats, int iType)
    {
      CornerkickManager.Club clb = ckClub();
      if (clb == null) return Json(false, JsonRequestBehavior.AllowGet);

      CornerkickGame.Stadium stadiumNew = clb.stadium.Clone();
      stadiumNew.blocks[iBlock].iSeats = iSeats;
      stadiumNew.blocks[iBlock].iType = (byte)iType;

      MvcApplication.ckcore.ui.buildStadium(ref clb, stadiumNew);

      return Json("Der Ausbau des Stadions wurde in Auftrag gegeben", JsonRequestBehavior.AllowGet);
    }

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

    public JsonResult StadiumGetTopring()
    {
      CornerkickManager.Club clb = ckClub();
      return Json(bStadiumGetTopring(clb), JsonRequestBehavior.AllowGet);
    }

    public JsonResult StadiumGetTopringProgress()
    {
      CornerkickManager.Club clb = ckClub();
      if (clb == null) return Json(false, JsonRequestBehavior.AllowGet);

      CornerkickGame.Stadium st = clb.stadium.Clone();
      st.bTopring = false;

      int[] iCostDays = CornerkickManager.Stadium.getCostDaysContructStadium(clb.stadium, st, ckUser());
      return Json((iCostDays[1] - clb.stadium.iTopringDaysConstruct) / (float)iCostDays[1], JsonRequestBehavior.AllowGet);
    }

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
      if (nVideoDaysConstract    > 0) fVideoDaysConstract    = (nVideoDaysConstract    - clb.stadium.iVideoDaysConstruct  ) / (float)nVideoDaysConstract;
      if (nSnackbarDaysConstract > 0) fSnackbarDaysConstract = (nSnackbarDaysConstract - clb.stadium.iSnackbarDaysConstruct) / (float)nSnackbarDaysConstract;
      if (nToiletsDaysConstract  > 0) fToiletsDaysConstract  = (nToiletsDaysConstract  - clb.stadium.iToiletsDaysConstruct ) / (float)nToiletsDaysConstract;

      return Json(new string[3][] {
        new string [4] { CornerkickManager.Stadium.sVideo[clb.stadium.iVideo], CornerkickManager.Stadium.sVideo[clb.stadium.iVideoNew], clb.stadium.iVideoDaysConstruct   .ToString(), fVideoDaysConstract   .ToString("0.0%") },
        new string [4] { clb.stadium.iSnackbar.ToString(),                     clb.stadium.iSnackbarNew.ToString(),                     clb.stadium.iSnackbarDaysConstruct.ToString(), fSnackbarDaysConstract.ToString("0.0%") },
        new string [4] { clb.stadium.iToilets .ToString(),                     clb.stadium.iToiletsNew .ToString(),                     clb.stadium.iToiletsDaysConstruct .ToString(), fToiletsDaysConstract .ToString("0.0%") }
      }, JsonRequestBehavior.AllowGet);
    }

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

    public JsonResult StadiumBuildVideo(byte iLevel)
    {
      CornerkickManager.Club clb = ckClub();
      if (clb == null) return Json(false, JsonRequestBehavior.AllowGet);

      CornerkickGame.Stadium stadiumNew = clb.stadium.Clone();
      stadiumNew.iVideo = iLevel;

      MvcApplication.ckcore.ui.buildStadium(ref clb, stadiumNew);

      return Json("Der Bau der Anzeigentafel wurde in Auftrag gegeben", JsonRequestBehavior.AllowGet);
    }

    // Snackbars
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

    public JsonResult StadiumBuildSnackbar(int iBuildNew)
    {
      CornerkickManager.Club clb = ckClub();
      if (clb == null) return Json(false, JsonRequestBehavior.AllowGet);

      CornerkickGame.Stadium stadiumNew = clb.stadium.Clone();
      stadiumNew.iSnackbar = (byte)(stadiumNew.iSnackbar + iBuildNew);

      MvcApplication.ckcore.ui.buildStadium(ref clb, stadiumNew);

      return Json("Der Ausbau der Imbissbuden wurde in Auftrag gegeben", JsonRequestBehavior.AllowGet);
    }

    // Toilets
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

    public JsonResult StadiumBuildToilets(int iBuildNew)
    {
      CornerkickManager.Club clb = ckClub();
      if (clb == null) return Json(false, JsonRequestBehavior.AllowGet);

      CornerkickGame.Stadium stadiumNew = clb.stadium.Clone();
      stadiumNew.iToilets = (byte)(clb.stadium.iToilets + iBuildNew);

      MvcApplication.ckcore.ui.buildStadium(ref clb, stadiumNew);

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

    public JsonResult StadiumGetPitchQuality()
    {
      CornerkickManager.Club clb = ckClub();
      if (clb == null) return Json(0.0, JsonRequestBehavior.AllowGet);

      return Json(clb.stadium.fPitchQuality, JsonRequestBehavior.AllowGet);
    }

    public JsonResult StadiumRenewPitchCost()
    {
      CornerkickManager.Club clb = ckClub();
      if (clb == null) return Json(false, JsonRequestBehavior.AllowGet);

      return Json(
        new string[] {
                        Math.Min(1f - clb.stadium.fPitchQuality, 0.1f).ToString("0%"),
                        MvcApplication.ckcore.st.getCostStadiumRenewPitch(clb.stadium, 0.1f, ckUser()).ToString("N0", getCi())
                      },
      JsonRequestBehavior.AllowGet);
    }

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
      CornerkickManager.User usr = ckUser();
      if (usr == null) return View(mdStadionSurr);

      CornerkickManager.Club clb = ckClub();
      if (clb == null) return View(mdStadionSurr);

      mdStadionSurr.bSound = true;
      if (usr.lti.Count > 1) mdStadionSurr.bSound = usr.lti[1] > 0;

      mdStadionSurr.ddlTrainingsgel  = new List<SelectListItem>();
      mdStadionSurr.ddlGym           = new List<SelectListItem>();
      mdStadionSurr.ddlSpa           = new List<SelectListItem>();
      mdStadionSurr.ddlJouthInternat = new List<SelectListItem>();
      mdStadionSurr.ddlClubHouse     = new List<SelectListItem>();
      mdStadionSurr.ddlClubMuseum    = new List<SelectListItem>();

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

      mdStadionSurr.iFanshop       = clb.buildings.bgFanshop.iLevel;
      if (clb.buildings.bgFanshop.ctn != null) mdStadionSurr.iFanshop = Math.Max(mdStadionSurr.iFanshop, clb.buildings.bgFanshop.ctn.iLevelNew);

      mdStadionSurr.sColor1 = "rgb(" + clb.cl1[0].R.ToString() + "," + clb.cl1[0].G.ToString() + "," + clb.cl1[0].B.ToString() + ")";

      return View(mdStadionSurr);
    }

    public JsonResult StadiumSurrGetBuildings()
    {
      CornerkickManager.Club clb = ckClub();
      if (clb == null) return Json(false, JsonRequestBehavior.AllowGet);

      CornerkickManager.User usr = ckUser();

      Models.StadiumSurroundingsModel.Building[] bdgsAll = new Models.StadiumSurroundingsModel.Building[10];
      for (byte iB = 0; iB < bdgsAll.Length; iB++) bdgsAll[iB] = new Models.StadiumSurroundingsModel.Building();

      Models.StadiumSurroundingsModel.Buildings buildings = new Models.StadiumSurroundingsModel.Buildings();
      buildings.ltBuildings     = new List<Models.StadiumSurroundingsModel.Building>();
      buildings.ltBuildingsFree = new List<Models.StadiumSurroundingsModel.Building>();

      int[] iCostDays = new int[2];

      // Training courts
      byte iType = 0;
      bdgsAll[iType].iType = iType;
      bdgsAll[iType].sCategory = CornerkickManager.Stadium.bdgTrainingCourts.sTypeName;
      bdgsAll[iType].iLevel = clb.buildings.bgTrainingCourts.iLevel;
      bdgsAll[iType].iLevelMax = CornerkickManager.Stadium.bdgTrainingCourts.sLevelNames.Length - 1;
      bdgsAll[iType].sName = CornerkickManager.Stadium.bdgTrainingCourts.sLevelNames[clb.buildings.bgTrainingCourts.iLevel];
      if (clb.buildings.bgTrainingCourts.iLevel + 1 < CornerkickManager.Stadium.bdgTrainingCourts.sLevelNames.Length) bdgsAll[iType].sNameNext = CornerkickManager.Stadium.bdgTrainingCourts.sLevelNames[clb.buildings.bgTrainingCourts.iLevel + 1];
      bdgsAll[iType].nRepeat = CornerkickManager.Stadium.bdgTrainingCourts.iGround[Math.Max(clb.buildings.bgTrainingCourts.iLevel, clb.buildings.bgTrainingCourts.ctn != null ? clb.buildings.bgTrainingCourts.ctn.iLevelNew : (byte)0)];
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
      else                                                                                                                                               buildings.ltBuildingsFree.Add(bdgsAll[iType]);

      // Gym
      iType++;
      bdgsAll[iType].iType = iType;
      bdgsAll[iType].sCategory = CornerkickManager.Stadium.bdgGym.sTypeName;
      bdgsAll[iType].iLevel = clb.buildings.bgGym.iLevel;
      bdgsAll[iType].iLevelMax = CornerkickManager.Stadium.bdgGym.sLevelNames.Length - 1;
      bdgsAll[iType].sName = CornerkickManager.Stadium.bdgGym.sLevelNames[clb.buildings.bgGym.iLevel];
      if (clb.buildings.bgGym.iLevel + 1 < CornerkickManager.Stadium.bdgGym.sLevelNames.Length) bdgsAll[iType].sNameNext = CornerkickManager.Stadium.bdgGym.sLevelNames[clb.buildings.bgGym.iLevel + 1];
      bdgsAll[iType].nRepeat = CornerkickManager.Stadium.bdgGym.iGround[Math.Max(clb.buildings.bgGym.iLevel, clb.buildings.bgGym.ctn != null ? clb.buildings.bgGym.ctn.iLevelNew : (byte)0)];
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
      else                                                                                                              buildings.ltBuildingsFree.Add(bdgsAll[iType]);

      // Spa
      iType++;
      bdgsAll[iType].iType = iType;
      bdgsAll[iType].sCategory = CornerkickManager.Stadium.bdgSpa.sTypeName;
      bdgsAll[iType].iLevel = clb.buildings.bgSpa.iLevel;
      bdgsAll[iType].iLevelMax = CornerkickManager.Stadium.bdgSpa.sLevelNames.Length - 1;
      bdgsAll[iType].sName = CornerkickManager.Stadium.bdgSpa.sLevelNames[clb.buildings.bgSpa.iLevel];
      if (clb.buildings.bgSpa.iLevel + 1 < CornerkickManager.Stadium.bdgSpa.sLevelNames.Length) bdgsAll[iType].sNameNext = CornerkickManager.Stadium.bdgSpa.sLevelNames[clb.buildings.bgSpa.iLevel + 1];
      bdgsAll[iType].nRepeat = CornerkickManager.Stadium.bdgSpa.iGround[Math.Max(clb.buildings.bgSpa.iLevel, clb.buildings.bgSpa.ctn != null ? clb.buildings.bgSpa.ctn.iLevelNew : (byte)0)];
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
      else                                                                                                              buildings.ltBuildingsFree.Add(bdgsAll[iType]);

      // Jouth internat
      iType++;
      bdgsAll[iType].iType = iType;
      bdgsAll[iType].sCategory = CornerkickManager.Stadium.bdgJouthInternat.sTypeName;
      bdgsAll[iType].iLevel = clb.buildings.bgJouthInternat.iLevel;
      bdgsAll[iType].iLevelMax = CornerkickManager.Stadium.bdgJouthInternat.sLevelNames.Length - 1;
      bdgsAll[iType].sName = CornerkickManager.Stadium.bdgJouthInternat.sLevelNames[clb.buildings.bgJouthInternat.iLevel];
      if (clb.buildings.bgJouthInternat.iLevel + 1 < CornerkickManager.Stadium.bdgJouthInternat.sLevelNames.Length) bdgsAll[iType].sNameNext = CornerkickManager.Stadium.bdgJouthInternat.sLevelNames[clb.buildings.bgJouthInternat.iLevel + 1];
      bdgsAll[iType].nRepeat = CornerkickManager.Stadium.bdgJouthInternat.iGround[Math.Max(clb.buildings.bgJouthInternat.iLevel, clb.buildings.bgJouthInternat.ctn != null ? clb.buildings.bgJouthInternat.ctn.iLevelNew : (byte)0)];
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
      else                                                                                                                                            buildings.ltBuildingsFree.Add(bdgsAll[iType]);

      // Club House
      iType++;
      bdgsAll[iType].iType = iType;
      bdgsAll[iType].sCategory = CornerkickManager.Stadium.bdgClubHouse.sTypeName;
      bdgsAll[iType].iLevel = clb.buildings.bgClubHouse.iLevel;
      bdgsAll[iType].iLevelMax = CornerkickManager.Stadium.bdgClubHouse.sLevelNames.Length - 1;
      bdgsAll[iType].sName = CornerkickManager.Stadium.bdgClubHouse.sLevelNames[clb.buildings.bgClubHouse.iLevel];
      if (clb.buildings.bgClubHouse.iLevel + 1 < CornerkickManager.Stadium.bdgClubHouse.sLevelNames.Length) bdgsAll[iType].sNameNext = CornerkickManager.Stadium.bdgClubHouse.sLevelNames[clb.buildings.bgClubHouse.iLevel + 1];
      bdgsAll[iType].nRepeat = CornerkickManager.Stadium.bdgClubHouse.iGround[Math.Max(clb.buildings.bgClubHouse.iLevel, clb.buildings.bgClubHouse.ctn != null ? clb.buildings.bgClubHouse.ctn.iLevelNew : (byte)0)];
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
      else                                                                                                                                buildings.ltBuildingsFree.Add(bdgsAll[iType]);

      // Club Museum
      iType++;
      bdgsAll[iType].iType = iType;
      bdgsAll[iType].sCategory = CornerkickManager.Stadium.bdgClubMuseum.sTypeName;
      bdgsAll[iType].iLevel = clb.buildings.bgClubMuseum.iLevel;
      bdgsAll[iType].iLevelMax = CornerkickManager.Stadium.bdgClubMuseum.sLevelNames.Length - 1;
      bdgsAll[iType].sName = CornerkickManager.Stadium.bdgClubMuseum.sLevelNames[clb.buildings.bgClubMuseum.iLevel];
      if (clb.buildings.bgClubMuseum.iLevel + 1 < CornerkickManager.Stadium.bdgClubMuseum.sLevelNames.Length) bdgsAll[iType].sNameNext = CornerkickManager.Stadium.bdgClubMuseum.sLevelNames[clb.buildings.bgClubMuseum.iLevel + 1];
      bdgsAll[iType].nRepeat = CornerkickManager.Stadium.bdgClubMuseum.iGround[Math.Max(clb.buildings.bgClubMuseum.iLevel, clb.buildings.bgClubMuseum.ctn != null ? clb.buildings.bgClubMuseum.ctn.iLevelNew : (byte)0)];
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
      else                                                                                                                                   buildings.ltBuildingsFree.Add(bdgsAll[iType]);

      // Carpark
      iType++;
      iCostDays = CornerkickManager.Stadium.getCostDaysContructCarpark(Math.Max(clb.stadium.iCarpark + 1, clb.stadium.iCarparkNew), clb.stadium.iCarpark, usr);
      bdgsAll[iType].iType = iType;
      bdgsAll[iType].sCategory = CornerkickManager.Stadium.sCarparkName;
      bdgsAll[iType].bTypeInt = true;
      bdgsAll[iType].iLevel = clb.stadium.iCarpark;
      bdgsAll[iType].sName = clb.stadium.iCarpark.ToString();
      bdgsAll[iType].sNameNext = clb.stadium.iCarparkNew.ToString();
      bdgsAll[iType].nRepeat = (int)Math.Ceiling(Math.Max(clb.stadium.iCarpark, clb.stadium.iCarparkNew) / (float)CornerkickManager.Stadium.iCarparkPerGround);
      bdgsAll[iType].nDaysConstructTotal = iCostDays[1];
      bdgsAll[iType].iLevelReq = CornerkickManager.UI.getRequiredFeature(clb, 2);
      if (clb.stadium.iCarparkNew > clb.stadium.iCarpark) {
        bdgsAll[iType].nDaysConstruct = clb.stadium.iCarparkDaysConstruct;
      } else {
        bdgsAll[iType].sCostConstructNext = iCostDays[0].ToString("N0", getCi());
        bdgsAll[iType].bDispoOk = MvcApplication.ckcore.fz.checkDispoLimit(iCostDays[0], clb);
      }
      if (clb.stadium.iCarpark > 0 || clb.stadium.iCarparkNew > 0) buildings.ltBuildings    .Add(bdgsAll[iType]);
      else                                                         buildings.ltBuildingsFree.Add(bdgsAll[iType]);

      // Ticketcounter
      iType++;
      iCostDays = CornerkickManager.Stadium.getCostDaysContructTicketcounter(Math.Max(clb.stadium.iTicketcounter + 1, clb.stadium.iTicketcounterNew), clb.stadium.iTicketcounter, usr);
      bdgsAll[iType].iType = iType;
      bdgsAll[iType].sCategory = CornerkickManager.Stadium.sTicketcounterName;
      bdgsAll[iType].bTypeInt = true;
      bdgsAll[iType].iLevel = clb.stadium.iTicketcounter;
      bdgsAll[iType].sName = clb.stadium.iTicketcounter.ToString();
      bdgsAll[iType].sNameNext = clb.stadium.iTicketcounterNew.ToString();
      bdgsAll[iType].nRepeat = (int)Math.Ceiling(Math.Max(clb.stadium.iTicketcounter, clb.stadium.iTicketcounterNew) / (float)CornerkickManager.Stadium.iTicketcounterPerGround);
      bdgsAll[iType].nDaysConstructTotal = iCostDays[1];
      bdgsAll[iType].iLevelReq = CornerkickManager.UI.getRequiredFeature(clb, 3);
      if (clb.stadium.iTicketcounterNew > clb.stadium.iTicketcounter) {
        bdgsAll[iType].nDaysConstruct = clb.stadium.iTicketcounterDaysConstruct;
      } else {
        bdgsAll[iType].sCostConstructNext = iCostDays[0].ToString("N0", getCi());
        bdgsAll[iType].bDispoOk = MvcApplication.ckcore.fz.checkDispoLimit(iCostDays[0], clb);
      }
      if (clb.stadium.iTicketcounter > 0 || clb.stadium.iTicketcounterNew > 0) buildings.ltBuildings    .Add(bdgsAll[iType]);
      else                                                                     buildings.ltBuildingsFree.Add(bdgsAll[iType]);

      // Fanshops
      iType++;
      bdgsAll[iType].iType = iType;
      bdgsAll[iType].sCategory = clb.buildings.bgFanshop.sName;
      bdgsAll[iType].bTypeInt = true;
      bdgsAll[iType].iLevel = clb.buildings.bgFanshop.iLevel;
      bdgsAll[iType].sName = clb.buildings.bgFanshop.iLevel.ToString();
      bdgsAll[iType].nRepeat = (int)Math.Ceiling(Math.Max(clb.buildings.bgFanshop.iLevel, clb.buildings.bgFanshop.ctn != null ? clb.buildings.bgFanshop.ctn.iLevelNew : (byte)0) / (float)CornerkickManager.Stadium.iFanshopPerGround);
      bdgsAll[iType].iLevelReq = CornerkickManager.UI.getRequiredFeature(clb, 4, iCustomers: (int)clb.getAttractionFactor(MvcApplication.ckcore.iSeason));
      if (clb.buildings.bgFanshop.ctn != null && clb.buildings.bgFanshop.ctn.iLevelNew > clb.buildings.bgFanshop.iLevel) {
        bdgsAll[iType].sNameNext = clb.buildings.bgFanshop.ctn.iLevelNew.ToString();
        bdgsAll[iType].nDaysConstruct = (int)clb.buildings.bgFanshop.ctn.fDaysConstruct;
        bdgsAll[iType].nDaysConstructTotal = CornerkickManager.Stadium.getCostDaysBuildFanshop(clb)[1];
      } else {
        iCostDays = CornerkickManager.Stadium.getCostDaysBuildFanshop(clb.buildings.bgFanshop.iLevel + 1, clb.buildings.bgFanshop.iLevel);
        bdgsAll[iType].sNameNext = (clb.buildings.bgFanshop.iLevel + 1).ToString();
        bdgsAll[iType].sCostConstructNext = iCostDays[0].ToString("N0", getCi());
        bdgsAll[iType].nDaysConstructTotal = iCostDays[1];
        bdgsAll[iType].bDispoOk = MvcApplication.ckcore.fz.checkDispoLimit(iCostDays[0], clb);
      }
      if (clb.buildings.bgFanshop.iLevel > 0 || (clb.buildings.bgFanshop.ctn != null && clb.buildings.bgFanshop.ctn.iLevelNew > 0)) buildings.ltBuildings    .Add(bdgsAll[iType]);
      else                                                                                                                          buildings.ltBuildingsFree.Add(bdgsAll[iType]);

      // Mass-transit
      iType++;
      bdgsAll[iType].iType = iType;
      bdgsAll[iType].sCategory = CornerkickManager.Stadium.bdgMassTransit.sTypeName;
      bdgsAll[iType].iLevel = clb.buildings.bgMassTransit.iLevel;
      bdgsAll[iType].iLevelMax = CornerkickManager.Stadium.bdgMassTransit.sLevelNames.Length - 1;
      bdgsAll[iType].sName = CornerkickManager.Stadium.bdgMassTransit.sLevelNames[clb.buildings.bgMassTransit.iLevel];
      if (clb.buildings.bgMassTransit.iLevel + 1 < CornerkickManager.Stadium.bdgMassTransit.sLevelNames.Length) bdgsAll[iType].sNameNext = CornerkickManager.Stadium.bdgMassTransit.sLevelNames[clb.buildings.bgMassTransit.iLevel + 1];
      bdgsAll[iType].nRepeat = CornerkickManager.Stadium.bdgMassTransit.iGround[Math.Max(clb.buildings.bgMassTransit.iLevel, clb.buildings.bgMassTransit.ctn != null ? clb.buildings.bgMassTransit.ctn.iLevelNew : (byte)0)];
      if (clb.buildings.bgMassTransit.ctn != null && clb.buildings.bgMassTransit.ctn.iLevelNew > clb.buildings.bgMassTransit.iLevel) {
        bdgsAll[iType].nDaysConstruct = (int)clb.buildings.bgMassTransit.ctn.fDaysConstruct;
        bdgsAll[iType].nDaysConstructTotal = CornerkickManager.Stadium.getCostDaysBuildMassTransit(clb)[1];
      } else {
        iCostDays = CornerkickManager.Stadium.getCostDaysBuildMassTransit(clb.buildings.bgMassTransit.iLevel + 1, clb.buildings.bgMassTransit.iLevel);
        bdgsAll[iType].sCostConstructNext = iCostDays[0].ToString("N0", getCi());
        bdgsAll[iType].nDaysConstructTotal = iCostDays[1];
        bdgsAll[iType].bDispoOk = MvcApplication.ckcore.fz.checkDispoLimit(iCostDays[0], clb);
      }
      if (clb.buildings.bgMassTransit.iLevel > 0 || (clb.buildings.bgMassTransit.ctn != null && clb.buildings.bgMassTransit.ctn.iLevelNew > 0)) buildings.ltBuildings    .Add(bdgsAll[iType]);
      else                                                                                                                                      buildings.ltBuildingsFree.Add(bdgsAll[iType]);

      buildings.iGround = (byte)Math.Max(clb.buildings.iGround, buildings.ltBuildings.Count);
      buildings.sCostBuyGround = CornerkickManager.Stadium.getCostBuyGround(clb.buildings.iGround).ToString("N0", getCi());

      return Json(buildings, JsonRequestBehavior.AllowGet);
    }

    [HttpGet]
    public JsonResult StadiumSurrGetSurrTypeNumber(int iType, int iNew, int iCurrent)
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
      else if (iType == 8) iCostDays = CornerkickManager.Stadium.getCostDaysBuildBuilding        (clb, iType, clb.buildings.bgFanshop.iLevel + iNew);

      bdg.sCostConstructNext  = iCostDays[0].ToString("N0", getCi());
      bdg.nDaysConstructTotal = iCostDays[1];

      return Json(bdg, JsonRequestBehavior.AllowGet);
    }

    public JsonResult StadiumBuildBuilding(int iType, int iLevel)
    {
      CornerkickManager.Club clb = ckClub();
      if (clb == null) return Json(false, JsonRequestBehavior.AllowGet);

      CornerkickManager.User usr = ckUser();

      string[] sConstructionNames = new string[] {
        CornerkickManager.Stadium.bdgTrainingCourts.sTypeName,
        CornerkickManager.Stadium.bdgGym.sTypeName,
        CornerkickManager.Stadium.bdgSpa.sTypeName,
        CornerkickManager.Stadium.bdgJouthInternat.sTypeName,
        CornerkickManager.Stadium.bdgClubHouse.sTypeName,
        CornerkickManager.Stadium.bdgClubMuseum.sTypeName,
        CornerkickManager.Stadium.sCarparkName,
        CornerkickManager.Stadium.sTicketcounterName,
        CornerkickManager.Stadium.sFanshopName,
        CornerkickManager.Stadium.bdgMassTransit.sTypeName
      };

      if (iType == 6) { // Carpark
        if (clb.stadium.iCarparkNew != iLevel) {
          // Check if enough grounds are available
          if (Math.Ceiling(iLevel / (float)CornerkickManager.Stadium.iCarparkPerGround) > Math.Ceiling(clb.stadium.iCarpark / (float)CornerkickManager.Stadium.iCarparkPerGround)) {
            if (clb.buildings.iGround <= CornerkickManager.Stadium.getRequiredGrounds(clb)) {
              return Json("Sie benötigen erst ein neues Grundstück", JsonRequestBehavior.AllowGet);
            }
          }

          clb.stadium.iCarparkNew = iLevel;
          int[] iCostDaysCp = CornerkickManager.Stadium.getCostDaysContructCarpark(iLevel, clb.stadium.iCarpark, usr);
          clb.stadium.iCarparkDaysConstruct = iCostDaysCp[1];

          CornerkickManager.Finance.doTransaction(clb, MvcApplication.ckcore.dtDatum, -iCostDaysCp[0], CornerkickManager.Finance.iTransferralTypePayStadiumSurr, "Bau " + sConstructionNames[iType]);
        }
      } else if (iType == 7) { // Ticketcounter
        if (clb.stadium.iTicketcounterNew != iLevel) {
          // Check if enough grounds are available
          if (Math.Ceiling(iLevel / (float)CornerkickManager.Stadium.iTicketcounterPerGround) > Math.Ceiling(clb.stadium.iTicketcounter / (float)CornerkickManager.Stadium.iTicketcounterPerGround)) {
            if (clb.buildings.iGround <= CornerkickManager.Stadium.getRequiredGrounds(clb)) {
              return Json("Sie benötigen erst ein neues Grundstück", JsonRequestBehavior.AllowGet);
            }
          }

          clb.stadium.iTicketcounterNew = (byte)iLevel;
          int[] iCostDaysTc = CornerkickManager.Stadium.getCostDaysContructTicketcounter(iLevel, clb.stadium.iTicketcounter, usr);
          clb.stadium.iTicketcounterDaysConstruct = iCostDaysTc[1];

          CornerkickManager.Finance.doTransaction(clb, MvcApplication.ckcore.dtDatum, -iCostDaysTc[0], CornerkickManager.Finance.iTransferralTypePayStadiumSurr, "Bau " + sConstructionNames[iType]);
        }
      } else {
        if (!CornerkickManager.UI.doConstruction(clb, iType, (byte)iLevel, MvcApplication.ckcore.dtDatum, sConstructionNames[iType])) {
          return Json("Sie benötigen erst ein neues Grundstück", JsonRequestBehavior.AllowGet);
        }
      }

      return Json("Der Bau des " + sConstructionNames[iType] + "s wurde in Auftrag gegeben", JsonRequestBehavior.AllowGet);
    }
    
    [HttpGet]
    public JsonResult StadiumSurrBuyGround()
    {
      CornerkickManager.Club clb = ckClub();
      if (clb == null) return Json(false, JsonRequestBehavior.AllowGet);

      int iCost = CornerkickManager.Stadium.getCostBuyGround(clb.buildings.iGround);

      // Check dispo
      if (!MvcApplication.ckcore.fz.checkDispoLimit(iCost, clb)) {
        return Json(false, JsonRequestBehavior.AllowGet);
      }

      // Add and pay ground
      clb.buildings.iGround++;
      CornerkickManager.Finance.doTransaction(clb, MvcApplication.ckcore.dtDatum, -iCost, CornerkickManager.Finance.iTransferralTypePayStadiumSurrGround);

      return Json(true, JsonRequestBehavior.AllowGet);
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

      // Tutorial
      if (ttUser != null) {
        int iUserIx = MvcApplication.ckcore.ltUser.IndexOf(ckUser());
        if (iUserIx >= 0 && iUserIx < ttUser.Length) {
          personal.tutorial = ttUser[iUserIx];
          if (personal.tutorial.iLevel == 19) personal.tutorial.iLevel = 20;
        }
      }

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

      int iKosten = (int)(clb.getSalaryStuff() / 12f);
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
      CornerkickManager.Finance.doTransaction(clb, MvcApplication.ckcore.dtDatum, -iPayOff, CornerkickManager.Finance.iTransferralTypePaySalaryStaff, "Abfindungen");

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
    [Authorize]
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

        mdClub.sAttrFc = mdClub.club.getAttractionFactor(MvcApplication.ckcore.iSeason, ltCups: MvcApplication.ckcore.ltCups, dtNow: MvcApplication.ckcore.dtDatum).ToString("0.0");
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
      if (league == null) return Content(null, "application/json");

      // Add past league places
      CornerkickManager.Main.Success sucLge = CornerkickManager.Tool.getSuccess(clb, league);
      if (sucLge != null) {
        for (int iS = 0; iS < sucLge.ltCupPlace.Count; iS++) {
          ltDataPoints.Add(new Models.DataPointGeneral(sucLge.ltCupPlace[iS][1], sucLge.ltCupPlace[iS][0]));
        }
      }

      // Add current league place
      ltDataPoints.Add(new Models.DataPointGeneral(MvcApplication.ckcore.iSeason, league.getPlace(clb)));

      JsonSerializerSettings _jsonSetting = new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore };

      return Content(JsonConvert.SerializeObject(ltDataPoints, _jsonSetting), "application/json");
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

    public static string getStringRecordGame(CornerkickManager.Club clb, int iGameType, sbyte iWDD, byte iHA, System.Security.Principal.IPrincipal User = null)
    {
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

      int[] iLtCupIds = new int[] { MvcApplication.iCupIdLeague, MvcApplication.iCupIdNatCup, MvcApplication.iCupIdGold, MvcApplication.iCupIdSilver, MvcApplication.iCupIdBronze, MvcApplication.iCupIdTestgame };
      List<Models.DataPointGeneral>[] ltDataPoints = new List<Models.DataPointGeneral>[iLtCupIds.Length];

      for (int iC = 0; iC < iLtCupIds.Length; iC++) {
        ltDataPoints[iC] = new List<Models.DataPointGeneral>();

        CornerkickManager.Cup cup = null;
        if      (iLtCupIds[iC] == MvcApplication.iCupIdLeague) cup = MvcApplication.ckcore.tl.getCup(iLtCupIds[iC], iId2: clb.iLand, iId3: clb.iDivision);
        else if (iLtCupIds[iC] == MvcApplication.iCupIdNatCup) cup = MvcApplication.ckcore.tl.getCup(iLtCupIds[iC], iId2: clb.iLand);
        else                                                   cup = MvcApplication.ckcore.tl.getCup(iLtCupIds[iC]);
        CornerkickManager.Main.Success suc = CornerkickManager.Tool.getSuccess(clb, cup);

        for (int iS = 1; iS <= MvcApplication.ckcore.iSeason; iS++) {
          List<float> ltAttractionFactorCup = new List<float>();
          float fAttrF = clb.getAttractionFactor(suc, MvcApplication.ckcore.iSeason, iSeasonSelected: iS, ltCups: MvcApplication.ckcore.ltCups, dtNow: MvcApplication.ckcore.dtDatum);

          if (fAttrF > 0f) {
            string sCupPlace = "";
            for (int iCP = 0; iCP < suc.ltCupPlace.Count; iCP++) {
              if (suc.ltCupPlace[iCP][1] == iS) {
                sCupPlace = " (" + suc.ltCupPlace[iCP][0].ToString() + ". Platz)";
                break;
              }
            }

            ltDataPoints[iC].Add(new Models.DataPointGeneral(iS, fAttrF, z: sCupPlace));
          }
        }
      }

      JsonSerializerSettings _jsonSetting = new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore };

      return Content(JsonConvert.SerializeObject(ltDataPoints, _jsonSetting), "application/json");
    }

    [Authorize]
    public ActionResult Merchandising(Models.MerchandisingModel mdMerchandising)
    {
      CornerkickManager.Club clb = ckClub();
      if (clb == null) return View(mdMerchandising);

      mdMerchandising.marketer = clb.merchMarketer;
      if (clb.merchMarketer == null) mdMerchandising.sMarketerMoney = getMerchandisingMarketerOffer(clb).ToString("N0", getCi());
      else                           mdMerchandising.sMarketerMoney = clb.merchMarketer.iMoney.ToString("N0", getCi());

      mdMerchandising.bFanshopsAvailable = clb.buildings.bgFanshop.iLevel > 0;
      mdMerchandising.bMarketer = clb.merchMarketer != null;

      // Season of budget plan
      mdMerchandising.sctSeason = new List<SelectListItem>();
      mdMerchandising.sctSeason.Add(new SelectListItem { Text = MvcApplication.ckcore.dtSeasonStart.Year.ToString(), Value = "-1" });
      foreach (CornerkickManager.Club.MerchandisingHistory mh in clb.ltMerchandisingHistory) {
        mdMerchandising.sctSeason.Add(new SelectListItem { Text = (MvcApplication.ckcore.dtSeasonStart.Year - (MvcApplication.ckcore.iSeason - mh.iSeason)).ToString(), Value = mh.iSeason.ToString() });
      }

      // Secret Balance
      mdMerchandising.fBalanceSecretFracMerchandisingIncome = clb.fBalanceSecretFracMerchandisingIncome * 100f;

      return View(mdMerchandising);
    }
    private int getMerchandisingMarketerOffer(CornerkickManager.Club clb)
    {
      double fSeasonFrac = (MvcApplication.ckcore.dtSeasonEnd - MvcApplication.ckcore.dtDatum).TotalDays / (MvcApplication.ckcore.dtSeasonEnd - MvcApplication.ckcore.dtSeasonStart).TotalDays;

      return (int)(Math.Max(clb.getAttractionFactor(MvcApplication.ckcore.iSeason), 500) * fSeasonFrac) * 5000;
    }

    public JsonResult MerchandisingGetItems(int iSeason)
    {
      List<Models.MerchandisingModel.DatatableMerchandising> ltDtItems = new List<Models.MerchandisingModel.DatatableMerchandising>();

      CornerkickManager.Club clb = ckClub();
      if (clb == null) return Json(null, JsonRequestBehavior.AllowGet);

      int iIx = 0;
      foreach (CornerkickManager.Merchandising.Item mi in MvcApplication.ckcore.ltMerchandising) {
        Models.MerchandisingModel.DatatableMerchandising dtm = new Models.MerchandisingModel.DatatableMerchandising();

        CornerkickManager.Club.MerchandisingItem cmi = clb.getMerchandisingItem(mi, iSeason);

        dtm.iIx = iIx++;
        dtm.iId = mi.iId;
        dtm.sName = mi.sName;
        dtm.sPriceBasic = mi.fPriceBuy.ToString("0.00") + " €";
        dtm.sPriceBuy = dtm.sPriceBasic;
        dtm.fPriceSell = mi.fPriceBuy;
        if (cmi != null) {
          dtm.iPresent = cmi.iPresent;
          dtm.sPricePresentBuyAve = cmi.fPricePresentBuyAve.ToString("0.00") + " €";
          dtm.iValuePresent = (int)(cmi.fPricePresentBuyAve * cmi.iPresent);
          dtm.iSold = cmi.iSold;
          dtm.fPriceSell = cmi.fPrice;
          dtm.iItemIncome = cmi.iIncome;
          dtm.iWinLoose = cmi.iIncome - (int)((dtm.iPresent + dtm.iSold) * cmi.fPricePresentBuyAve);
          if (dtm.iSold > 0) dtm.sPriceSellAve = (cmi.iIncome / (float)dtm.iSold).ToString("0.00") + " €";
        }
        dtm.bPlayerJersey = mi.bPlayerJersey;

        ltDtItems.Add(dtm);
      }

      return Json(new { aaData = ltDtItems }, JsonRequestBehavior.AllowGet);
    }

    [HttpGet]
    public JsonResult MerchandisingGetJerseyItemSubTable()
    {
      string sTable = "";

      CornerkickManager.Club clbUser = ckClub();

      sTable += "<table id=\"tableSoldJerseys\" class=\"display compact nowrap\" > ";
      sTable += "<thead>";
      sTable += "<tr>";
      sTable += "<th>#</th>";
      sTable += "<th>Name</th>";
      sTable += "<th>Nummer</th>";
      sTable += "<th>verkauft</th>";
      sTable += "</tr>";
      sTable += "</thead>";
      sTable += "<tbody>";

      List<CornerkickManager.Player> ltPl = new List<CornerkickManager.Player>(clbUser.ltPlayer);
      ltPl = ltPl.OrderByDescending(x => x.iSoldJerseys).ToList();

      int i = 0;
      foreach (CornerkickManager.Player pl in ltPl) {
        sTable += "<tr id=\"rowSoldJersey_" + pl.plGame.iId.ToString() + "\">";
        sTable += "<td align=\"right\">" + (i + 1).ToString() + "</td>";
        sTable += "<td align=\"center\">" + pl.plGame.sName + "</td>";
        sTable += "<td align=\"center\">" + pl.plGame.iNr.ToString() + "</td>";
        sTable += "<td align=\"right\">" + pl.iSoldJerseys.ToString("N0", getCi()) + "</td>";
        sTable += "</tr>";

        i++;
      }

      sTable += "</tbody>";
      sTable += "</table>";

      return Json(sTable, JsonRequestBehavior.AllowGet);
    }

    public JsonResult getMerchandisingPrice(int iItemId, int iAmount)
    {
      CornerkickManager.Merchandising.Item mi = CornerkickManager.Merchandising.getItem(iItemId, MvcApplication.ckcore.ltMerchandising);
      if (mi == null) return Json("0.00", JsonRequestBehavior.AllowGet);

      return Json(mi.getPrice(iAmount), JsonRequestBehavior.AllowGet);
    }

    public JsonResult buyMerchandisingItem(int iItemId, int iAmount, string sPriceBuy, string sPriceSell)
    {
      CornerkickManager.Merchandising.Item mi = CornerkickManager.Merchandising.getItem(iItemId, MvcApplication.ckcore.ltMerchandising);
      if (mi == null) return Json(false, JsonRequestBehavior.AllowGet);

      CornerkickManager.Club clb = ckClub();
      if (clb == null) return Json(false, JsonRequestBehavior.AllowGet);

      sPriceBuy = sPriceBuy.Replace("€", "").Trim();
      float fPriceBuy = 0f;
      if (!float.TryParse(sPriceBuy, NumberStyles.Currency, CultureInfo.InvariantCulture, out fPriceBuy)) return Json(false, JsonRequestBehavior.AllowGet);

      float fPriceSell = 0f;
      if (!float.TryParse(sPriceSell, NumberStyles.Currency, CultureInfo.InvariantCulture, out fPriceSell)) return Json(false, JsonRequestBehavior.AllowGet);

      CornerkickManager.Merchandising.buyMerchandisingItem(clb, mi, iAmount, fPriceBuy, fPriceSell, MvcApplication.ckcore.dtDatum);

      return Json(true, JsonRequestBehavior.AllowGet);
    }

    public JsonResult setMerchandisingItemPrice(int iItemId, float fPrice)
    {
      CornerkickManager.Merchandising.Item mi = CornerkickManager.Merchandising.getItem(iItemId, MvcApplication.ckcore.ltMerchandising);
      if (mi == null) return Json(false, JsonRequestBehavior.AllowGet);

      CornerkickManager.Club clb = ckClub();
      if (clb == null) return Json(false, JsonRequestBehavior.AllowGet);

      clb.setMerchandisingItemPrice(mi, fPrice);

      return Json(true, JsonRequestBehavior.AllowGet);
    }

    public JsonResult MerchandisingTakeMarketer()
    {
      CornerkickManager.Club clb = ckClub();
      if (clb == null) return Json(false, JsonRequestBehavior.AllowGet);

      clb.merchMarketer = new CornerkickManager.Club.MerchandisingMarketer();
      clb.merchMarketer.marketer = MvcApplication.ckcore.ltMerchandisingMarketer[0];
      clb.merchMarketer.iMoney = getMerchandisingMarketerOffer(clb);

      CornerkickManager.Finance.doTransaction(clb, MvcApplication.ckcore.dtDatum, clb.merchMarketer.iMoney, CornerkickManager.Finance.iTransferralTypeInMerchandisingMarketer);

      return Json(true, JsonRequestBehavior.AllowGet);
    }

    public JsonResult MerchandisingSetBalanceSecret(float fBalanceSecret)
    {
      CornerkickManager.Club clb = ckClub();
      if (clb == null) return Json("Kein club", JsonRequestBehavior.AllowGet);

      if (fBalanceSecret < 0.0) {
        return Json("Falsche Eingabe!", JsonRequestBehavior.AllowGet);
      }

      if (fBalanceSecret > 20) {
        return Json("Maximal 20% erlaubt!", JsonRequestBehavior.AllowGet);
      }

      clb.fBalanceSecretFracMerchandisingIncome = fBalanceSecret / 100f;

      return Json(null, JsonRequestBehavior.AllowGet);
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
        mlLeague.iDivision = clb.iDivision;
      }

      iSeasonGlobal = MvcApplication.ckcore.iSeason;

      CornerkickManager.Cup league = MvcApplication.ckcore.tl.getCup(MvcApplication.iCupIdLeague, mlLeague.iLand, mlLeague.iDivision);
      if (league == null) return View(mlLeague);

      int iMd = league.getMatchday(MvcApplication.ckcore.dtDatum);
      mlLeague.league = league;
      mlLeague.ltTbl  = league.getTable(iMd - 1, 0);
      mlLeague.iLeagueSize = league.getParticipants(iMd, MvcApplication.ckcore.dtDatum).Count;

      // Add lands to dropdown list
      foreach (int iLand in MvcApplication.iNations) {
        mlLeague.ddlLand.Add(new SelectListItem { Text = CornerkickManager.Main.sLand[iLand], Value = iLand.ToString() });
      }

      mlLeague.ddlDivision.Add(new SelectListItem { Text = "1", Value = "0" });
      if (clb != null && clb.iLand == MvcApplication.iNations[0]) mlLeague.ddlDivision.Add(new SelectListItem { Text = "2", Value = "1" });

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
      CornerkickManager.Cup league = MvcApplication.ckcore.tl.getCup(MvcApplication.iCupIdLeague, iLand, iDivision);

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
        iMd = MvcApplication.ckcore.tl.getCup(MvcApplication.iCupIdLeague, iLand, iDivision).getMatchdaysTotal();
      } else { // Current seasons
        // Get current matchday
        iMd = MvcApplication.ckcore.tl.getMatchday(iLand, iDivision, MvcApplication.ckcore.dtDatum, MvcApplication.iCupIdLeague);

        // Increment matchday if match is today or tomorrow
        CornerkickManager.Club clb = ckClub();
        if (clb != null) {
          CornerkickGame.Game.Data gdNext = MvcApplication.ckcore.tl.getNextGame(clb, MvcApplication.ckcore.dtDatum, iGameType: MvcApplication.iCupIdLeague);
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
          if (cupGlobal.iId == iType && (iLand < 0 || cupGlobal.iId2 == iLand) && (iDivision < 0 || cupGlobal.iId3 == iDivision)) return cupGlobal;
        }

        string sFileLoad = System.IO.Path.Combine(MvcApplication.getHomeDir(), "archive");
        List<CornerkickManager.Cup> ltCupsTmp = MvcApplication.ckcore.io.readCups(sFileLoad, iSeason);

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

    // DEPRECATED, use getTableDatatable() instead
    private string getTable(CornerkickManager.Cup cup, int iMatchday, sbyte iGroup = -1, byte iHA = 0, CornerkickManager.Club clubUser = null, int iColor1 = 0, int iColor2 = 0, int iColor3 = 0, int iColor4 = 0)
    {
      List<CornerkickManager.Cup.TableItem> ltTblLast = cup.getTable(iMatchday - 1, iHA, iGroup);
      List<CornerkickManager.Cup.TableItem> ltTbl     = cup.getTable(iMatchday,     iHA, iGroup);

      string sBox = "";
      for (var i = 0; i < ltTbl.Count; i++) {
        CornerkickManager.Cup.TableItem tbpl = ltTbl[i];
        int iGames = tbpl.iWDL[0] + tbpl.iWDL[1] + tbpl.iWDL[2];
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

        string sEmblem = getClubEmblem(tbpl.club, sStyle: "width: 12px", bTiny: true);

        sBox += "<tr " + sStyle + "\">";
        sBox += "<td class=\"first\" bgcolor=\"" + sBgColor + "\" align=\"center\"><b>" + k + "</b></td>";
        sBox += "<td style=\"color: " + sColor + "\" align=\"center\"> " + sPlaceLast + "</td>";
        sBox += "<td>" + sEmblem + "</td>";
        sBox += "<td><a href=\"/Member/ClubDetails?iClub=" + tbpl.iId.ToString() + "\" target=\"\">" + tbpl.sName + "</a></td>";
        sBox += "<td>&nbsp;</td>";
        sBox += "<td align=\"right\">" + iGames.ToString() + "</td>";
        sBox += "<td>&nbsp;</td>";
        sBox += "<td align=\"right\">" + tbpl.iWDL[0].ToString() + "</td>";
        sBox += "<td align=\"right\">" + tbpl.iWDL[1].ToString() + "</td>";
        sBox += "<td align=\"right\">" + tbpl.iWDL[2].ToString() + "</td>";
        sBox += "<td>&nbsp;</td>";
        sBox += "<td align=\"center\">" + tbpl.iGoals.ToString() + ":" + tbpl.iGoalsOpp.ToString() + "</td>";
        sBox += "<td align=\"right\">" + iDiff.ToString() + "</td>";
        sBox += "<td>&nbsp;</td>";
        sBox += "<td align=\"right\">" + tbpl.iPoints.ToString() + "</td>";
        sBox += "</tr>";
      }

      return sBox;
    }

    public class DatatableTableLeague
    {
      public int iId { get; set; }
      public int iPl { get; set; }
      public int iPlLast { get; set; }
      public bool bBold { get; set; }
      public string sBgColor { get; set; }
      public string sEmblem { get; set; }
      public string sClubName { get; set; }
      public int iGames { get; set; }
      public int iWin { get; set; }
      public int iDraw { get; set; }
      public int iDefeat { get; set; }
      public string sGoals { get; set; }
      public int iGoalsDiff { get; set; }
      public int iPoints { get; set; }
    }
    public JsonResult getTableDatatable(int iSeason, int iType, int iLand, int iDivision, int iMatchday, sbyte iGroup = -1, bool bH = false, bool bA = false, int iColor1 = 0, int iColor2 = 0, int iColor3 = 0, int iColor4 = 0)
    {
      List<DatatableTableLeague> ltDtTable = new List<DatatableTableLeague>();

      CornerkickManager.Club clb = ckClub();

      CornerkickManager.Cup cup = getCup(iSeason, iType, iLand, iDivision);

      byte iHA = 0;
      if (bH) iHA = 1;
      else if (bA) iHA = 2;
      List<CornerkickManager.Cup.TableItem> ltTblLast = cup.getTable(iMatchday - 1, iHA, iGroup);
      List<CornerkickManager.Cup.TableItem> ltTbl     = cup.getTable(iMatchday,     iHA, iGroup);

      for (var i = 0; i < ltTbl.Count; i++) {
        CornerkickManager.Cup.TableItem tbpl = ltTbl[i];
        DatatableTableLeague dtL = new DatatableTableLeague();

        string sBgColor = "white";
        if      (iColor1 > 0 && i < iColor1) sBgColor = "#ffffcc";
        else if (iColor2 > 0 && i < iColor2) sBgColor = "#ccffcc";
        else if (iColor3 > 0 && i < iColor3) sBgColor = "#cce5ff";
        else if (iColor4 > 0 && i < iColor4) sBgColor = "#ffcccc";
        else if (iColor1 < 0 && i >= ltTbl.Count + iColor1) sBgColor = "#ffffcc";
        else if (iColor2 < 0 && i >= ltTbl.Count + iColor2) sBgColor = "#ccffcc";
        else if (iColor3 < 0 && i >= ltTbl.Count + iColor3) sBgColor = "#cce5ff";
        else if (iColor4 < 0 && i >= ltTbl.Count + iColor4) sBgColor = "#ffcccc";
        dtL.sBgColor = sBgColor;

        var k = i + 1;

        dtL.iPl = i + 1;
        for (var iLast = 0; iLast < ltTblLast.Count; iLast++) {
          if (ltTblLast[iLast].iId == tbpl.iId) {
            if (i != iLast) {
              dtL.iPlLast = iLast + 1;
            }
            break;
          }
        }

        // Create tiny emblem file
        if (string.IsNullOrEmpty(getClubEmblemPath(tbpl.iId, bNation: false, bTiny: true))) {
          string sEmblemFile = getClubEmblemPath(tbpl.iId, bNation: false, bTiny: false);

          if (!string.IsNullOrEmpty(sEmblemFile)) {
            resizeImage(sEmblemFile, 48, "_tiny");
          }
        }

        dtL.sEmblem = getClubEmblem(tbpl.club, sStyle: "width: 24px", bTiny: true);
        dtL.sClubName = "<a href=\"/Member/ClubDetails?iClub=" + tbpl.iId.ToString() + "\" target=\"\">" + tbpl.sName + "</a>";
        dtL.iGames = tbpl.iWDL[0] + tbpl.iWDL[1] + tbpl.iWDL[2];
        dtL.iWin    = tbpl.iWDL[0];
        dtL.iDraw   = tbpl.iWDL[1];
        dtL.iDefeat = tbpl.iWDL[2];
        dtL.sGoals = tbpl.iGoals.ToString() + ":" + tbpl.iGoalsOpp.ToString();
        dtL.iGoalsDiff = tbpl.iGoals - tbpl.iGoalsOpp;
        dtL.iPoints = tbpl.iPoints;

        dtL.bBold = false;
        if (clb != null && tbpl.iId == clb.iId) dtL.bBold = true;

        ltDtTable.Add(dtL);
      }

      return Json(new { aaData = ltDtTable }, JsonRequestBehavior.AllowGet);
    }

    public JsonResult setLeagueTeams(int iSeason, int iLand, byte iDivision, int iMatchday)
    {
      if (iMatchday < 1) return Json("", JsonRequestBehavior.AllowGet);

      CornerkickManager.Club clbUser = ckClub();

      CornerkickManager.Cup league = getCup(iSeason, MvcApplication.iCupIdLeague, iLand, iDivision);

      if (iMatchday > league.ltMatchdays.Count) return Json("", JsonRequestBehavior.AllowGet);

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

        string sLiveGameTd = getLiveGameTd(gd);
        if (!string.IsNullOrEmpty(sLiveGameTd)) {
          sBox += sLiveGameTd;
        } else {
          if (gd.team[0].iGoals + gd.team[1].iGoals >= 0) {
            sBox += "<td style=\"white-space: nowrap\" align=\"center\">" + CornerkickManager.UI.getResultString(gd) + "</td>";
          } else {
            sBox += "<td style=\"white-space: nowrap\" align=\"center\">-</td>";
          }
        }
        sBox += "</tr>";
      }

      return Json(sBox, JsonRequestBehavior.AllowGet);
    }

    private string getLiveGameTd(CornerkickGame.Game.Data gd)
    {
      foreach (CornerkickManager.User usr in MvcApplication.ckcore.ltUser) {
        if (usr?.game != null) {
          if (!usr.game.data.bFinished) {
            if (usr.game.data.team[0].iTeamId == gd.team[0].iTeamId ||
                usr.game.data.team[1].iTeamId == gd.team[1].iTeamId) {
              return "<td style=\"white-space: nowrap; color: #ff8c00\" align=\"center\">" + CornerkickManager.UI.getResultString(usr.game.data) + " - " + MvcApplication.ckcore.ui.getMinuteString(usr.game.tsMinute, false) + " Min.</td>";
            }
          }
        }
      }

      return null;
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
        if (MvcApplication.ckcore.ltPlayer[sc.iId].contract?.club?.iId == clbUser?.iId) sStyle2 = "font-weight:bold";

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

    public class DatatableScorer
    {
      public int iIx { get; set; }
      public int iId { get; set; }
      public string sPlName { get; set; }
      public string sClubName { get; set; }
      public int iGoals { get; set; }
      public int iAssists { get; set; }
      public int iScorer { get; set; }
      public bool bBold { get; set; }
    }
    public JsonResult GetScorerTable(byte iGameType, int iLand, int iDivision)
    {
      List<DatatableScorer> ltDtScorer = new List<DatatableScorer>();

      CornerkickManager.Club clb = ckClub();

      List<CornerkickManager.UI.Scorer> ltScorer = MvcApplication.ckcore.ui.getScorer(iGameType, iLand: iLand, iDivision: iDivision, bNation: iGameType == 7);

      int iIx = 0;
      foreach (CornerkickManager.UI.Scorer sc in ltScorer) {
        DatatableScorer dts = new DatatableScorer();
        dts.iIx = iIx + 1;
        dts.iId = sc.iId;
        dts.sPlName = sc.sName;
        dts.sClubName = sc.sTeam;
        dts.iGoals = sc.iGoals;
        dts.iAssists = sc.iAssists;
        dts.iScorer = sc.iGoals + sc.iAssists;

        if (CornerkickManager.PlayerTool.ownPlayer(clb, MvcApplication.ckcore.ltPlayer[sc.iId])) dts.bBold = true;

        ltDtScorer.Add(dts);

        iIx++;
      }

      return Json(new { aaData = ltDtScorer }, JsonRequestBehavior.AllowGet);
    }

    public ContentResult GetLeaguePlaceHistory(int iSeason = 0)
    {
      CornerkickManager.Club club = ckClub();
      if (club == null) return null;

      CornerkickManager.Cup league = getCup(iSeason, MvcApplication.iCupIdLeague, club.iLand, club.iDivision);
      if (league == null) return null;

      List<Models.DataPointGeneral> dataPoints = new List<Models.DataPointGeneral>();

      for (int iMd = 1; iMd < league.getMatchday(MvcApplication.ckcore.dtDatum) + 1; iMd++) {
        int iPlace = league.getPlace(club, iMd);
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

      CornerkickManager.Cup cup = MvcApplication.ckcore.tl.getCup(MvcApplication.iCupIdNatCup, cupModel.iLand);

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
      CornerkickManager.Cup cup = getCup(iSaison, MvcApplication.iCupIdNatCup, iLand);
      if (cup == null) return Json(null, JsonRequestBehavior.AllowGet);

      string[] ltMd = new string[cup.getMatchdaysTotal()];
      // Spieltage zu Dropdown Menü hinzufügen
      for (int iMd = 0; iMd < ltMd.Length; iMd++) {
        ltMd[iMd] = (iMd + 1).ToString();

        int nRound = cup.getKoRound(cup.ltClubs[0].Count);
        ltMd[iMd] += ";" + CornerkickManager.Main.sCupRound[nRound - iMd - 1];
      }

      return Json(ltMd, JsonRequestBehavior.AllowGet);
    }

    private string getCupTeams(CornerkickManager.Cup cup, int iMatchday, int iGroup = -1)
    {
      if (cup == null) return "";
      if (cup.ltMatchdays == null) return "";
      if (cup.ltMatchdays.Count == 0) return "";

      CornerkickManager.Club clbUser = ckClub();

      string sBox = "";

      iMatchday = Math.Min(iMatchday, cup.ltMatchdays.Count - 1);
      iMatchday = Math.Max(iMatchday, 0);
      CornerkickManager.Cup.Matchday md = cup.ltMatchdays[iMatchday];

      if (md.ltGameData == null || md.ltGameData.Count == 0) {
        List<CornerkickManager.Club> ltClubs = cup.getParticipants(iMatchday, MvcApplication.ckcore.dtDatum);

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

        string sLiveGameTd = getLiveGameTd(gd);
        if (!string.IsNullOrEmpty(sLiveGameTd)) {
          sBox += sLiveGameTd;
        } else {
          if (gd.team[0].iGoals >= 0 && gd.team[1].iGoals >= 0) {
            sBox += "<td style=\"white-space: nowrap\" align=\"center\">" + CornerkickManager.UI.getResultString(gd) + "</td>";
          } else {
            sBox += "<td style=\"white-space: nowrap\" align=\"center\">-</td>";
          }
        }

        sBox += "</tr>";
      }

      return sBox;
    }

    public JsonResult CupGetMatchday(int iSaison, int iLand)
    {
      CornerkickManager.Cup cup = getCup(iSaison, MvcApplication.iCupIdNatCup, iLand);

      // Get current matchday
      int iMd = cup.getMatchday(MvcApplication.ckcore.dtDatum);

      // Increment matchday if next match is today or tomorrow
      if (iMd + 1 < cup.ltMatchdays.Count) {
        if ((cup.ltMatchdays[iMd + 1].dt.Date - MvcApplication.ckcore.dtDatum.Date).Days < 2) iMd++;
      }

      // Limit to 1
      iMd = Math.Max(iMd, 1);

      return Json(iMd, JsonRequestBehavior.AllowGet);
    }

    public JsonResult setCup(Models.CupModel cupModel, int iSaison, int iLand, int iMatchday)
    {
      CornerkickManager.Cup cup = getCup(iSaison, MvcApplication.iCupIdNatCup, iLand);

      return Json(getCupTeams(cup, iMatchday - 1), JsonRequestBehavior.AllowGet);
    }

    //[Authorize]
    public ActionResult CupGold(Models.CupGoldModel cupGoldModel)
    {
      CornerkickManager.Club clbUser = ckClub();

      cupGoldModel.ddlSeason = getDdlSeason();
      if (cupGoldModel.ddlSeason.Count > 0) cupGoldModel.ddlSeason.RemoveAt(0);
      cupGoldModel.iSeason = MvcApplication.ckcore.iSeason;

      CornerkickManager.Cup cupGold = MvcApplication.ckcore.tl.getCup(MvcApplication.iCupIdGold);
      cupGoldModel.iMatchday = Math.Min(cupGold.getMatchday(MvcApplication.ckcore.dtDatum), cupGold.ltMatchdays.Count);

      cupGoldModel.ddlMatchday = new List<SelectListItem>();
      for (int iMd = 0; iMd < Math.Max(6, cupGoldModel.iMatchday + 1); iMd++) {
        string sText = (iMd + 1).ToString();
        if (iMd > 5) sText = CornerkickManager.Main.sCupRound[3 - ((iMd - 6) / 2)];

        cupGoldModel.ddlMatchday.Add(new SelectListItem { Text = sText, Value = (iMd + 1).ToString() });
      }

      // Increment matchday if next match is today or tomorrow
      if (cupGoldModel.iMatchday + 1 < cupGold.ltMatchdays.Count) {
        if ((cupGold.ltMatchdays[cupGoldModel.iMatchday + 1].dt.Date - MvcApplication.ckcore.dtDatum.Date).Days < 2) cupGoldModel.iMatchday++;
      }

      // Initialize group
      cupGoldModel.iGroup = 0;
      for (int iG = 0; iG < cupGold.ltClubs.Length; iG++) {
        if (cupGold.checkClubInCup(clbUser, iG)) {
          cupGoldModel.iGroup = iG;
          break;
        }
      }

      return View(cupGoldModel);
    }

    public JsonResult setCupGold(int iSaison, int iMatchday, int iGroup)
    {
      CornerkickManager.Cup cupGold = getCup(iSaison, MvcApplication.iCupIdGold);

      if (!cupGold.checkCupGroupPhase(iMatchday)) iGroup = -1;

      return Json(getCupTeams(cupGold, iMatchday, iGroup), JsonRequestBehavior.AllowGet);
    }

    //[Authorize]
    public ActionResult CupSilver(Models.CupSilverModel cupSilverModel)
    {
      CornerkickManager.Club clbUser = ckClub();

      cupSilverModel.ddlSeason = getDdlSeason();
      if (cupSilverModel.ddlSeason.Count > 0) cupSilverModel.ddlSeason.RemoveAt(0);
      cupSilverModel.iSeason = MvcApplication.ckcore.iSeason;

      CornerkickManager.Cup cupSilver = MvcApplication.ckcore.tl.getCup(MvcApplication.iCupIdSilver);
      cupSilverModel.iMatchday = Math.Min(cupSilver.getMatchday(MvcApplication.ckcore.dtDatum), cupSilver.ltMatchdays.Count);

      cupSilverModel.ddlMatchday = new List<SelectListItem>();
      for (int iMd = 0; iMd < Math.Max(6, cupSilverModel.iMatchday + 1); iMd++) {
        string sText = (iMd + 1).ToString();
        if (iMd > 5) sText = CornerkickManager.Main.sCupRound[3 - ((iMd - 6) / 2)];

        cupSilverModel.ddlMatchday.Add(new SelectListItem { Text = sText, Value = (iMd + 1).ToString() });
      }

      // Increment matchday if next match is today or tomorrow
      if (cupSilverModel.iMatchday + 1 < cupSilver.ltMatchdays.Count) {
        if ((cupSilver.ltMatchdays[cupSilverModel.iMatchday + 1].dt.Date - MvcApplication.ckcore.dtDatum.Date).Days < 2) cupSilverModel.iMatchday++;
      }

      // Initialize group
      cupSilverModel.iGroup = 0;
      for (int iG = 0; iG < cupSilver.ltClubs.Length; iG++) {
        if (cupSilver.checkClubInCup(clbUser, iG)) {
          cupSilverModel.iGroup = iG;
          break;
        }
      }

      return View(cupSilverModel);
    }

    public JsonResult setCupSilver(int iSaison, int iMatchday, int iGroup)
    {
      CornerkickManager.Cup cupSilver = getCup(iSaison, MvcApplication.iCupIdSilver);

      if (!cupSilver.checkCupGroupPhase(iMatchday)) iGroup = -1;

      return Json(getCupTeams(cupSilver, iMatchday, iGroup), JsonRequestBehavior.AllowGet);
    }

    //[Authorize]
    public ActionResult CupBronze(Models.CupBronzeModel cupBronzeModel)
    {
      CornerkickManager.Club clbUser = ckClub();

      cupBronzeModel.ddlSeason = getDdlSeason();
      if (cupBronzeModel.ddlSeason.Count > 0) cupBronzeModel.ddlSeason.RemoveAt(0);
      cupBronzeModel.iSeason = MvcApplication.ckcore.iSeason;

      CornerkickManager.Cup cupBronze = MvcApplication.ckcore.tl.getCup(MvcApplication.iCupIdBronze);
      cupBronzeModel.iMatchday = Math.Min(cupBronze.getMatchday(MvcApplication.ckcore.dtDatum), cupBronze.ltMatchdays.Count);

      cupBronzeModel.ddlMatchday = new List<SelectListItem>();
      for (int iMd = 0; iMd < Math.Max(6, cupBronzeModel.iMatchday + 1); iMd++) {
        string sText = (iMd + 1).ToString();
        if (iMd > 5) sText = CornerkickManager.Main.sCupRound[3 - ((iMd - 6) / 2)];

        cupBronzeModel.ddlMatchday.Add(new SelectListItem { Text = sText, Value = (iMd + 1).ToString() });
      }

      // Increment matchday if next match is today or tomorrow
      if (cupBronzeModel.iMatchday + 1 < cupBronze.ltMatchdays.Count) {
        if ((cupBronze.ltMatchdays[cupBronzeModel.iMatchday + 1].dt.Date - MvcApplication.ckcore.dtDatum.Date).Days < 2) cupBronzeModel.iMatchday++;
      }

      // Initialize group
      cupBronzeModel.iGroup = 0;
      for (int iG = 0; iG < cupBronze.ltClubs.Length; iG++) {
        if (cupBronze.checkClubInCup(clbUser, iG)) {
          cupBronzeModel.iGroup = iG;
          break;
        }
      }

      return View(cupBronzeModel);
    }

    public JsonResult setCupBronze(int iSaison, int iMatchday, int iGroup)
    {
      CornerkickManager.Cup cupBronze = getCup(iSaison, MvcApplication.iCupIdBronze);

      if (!cupBronze.checkCupGroupPhase(iMatchday)) iGroup = -1;

      return Json(getCupTeams(cupBronze, iMatchday, iGroup), JsonRequestBehavior.AllowGet);
    }

    //[Authorize]
    public ActionResult CupWc(Models.CupWcModel cupWcModel)
    {
      cupWcModel.ddlSeason = getDdlSeason();
      cupWcModel.iSeason = MvcApplication.ckcore.iSeason;

      CornerkickManager.Cup cupWc = MvcApplication.ckcore.tl.getCup(MvcApplication.iCupIdWc);
      cupWcModel.iMatchday = Math.Min(cupWc.getMatchday(MvcApplication.ckcore.dtDatum), cupWc.ltMatchdays.Count);
      cupWcModel.iMatchday = Math.Max(cupWcModel.iMatchday, 0);

      // Increment matchday if next match is today or tomorrow
      if (cupWcModel.iMatchday + 1 < cupWc.ltMatchdays.Count) {
        if ((cupWc.ltMatchdays[cupWcModel.iMatchday + 1].dt.Date - MvcApplication.ckcore.dtDatum.Date).Days < 2) cupWcModel.iMatchday++;
      }

      return View(cupWcModel);
    }

    public JsonResult setCupWc(int iSaison, int iMatchday, int iGroup)
    {
      CornerkickManager.Cup cupWc = getCup(iSaison, MvcApplication.iCupIdWc);

      if (!cupWc.checkCupGroupPhase(iMatchday)) iGroup = -1;

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
      CornerkickManager.Club clb = ckClub();
      if (clb == null) return View(cal);

      cal.iClubId = clb.iId;

      cal.ddlTestgameClubs = new List<SelectListItem>();
      foreach (CornerkickManager.Club clbTg in MvcApplication.ckcore.ltClubs) {
        if (clbTg.iId == cal.iClubId) continue;
        if (clbTg.iLand < 0) continue;

        //CornerkickGame.Game.Data gdNext = MvcApplication.ckcore.tl.getNextGame(clbTg, MvcApplication.ckcore.dtDatum, false);

        cal.ddlTestgameClubs.Add(new SelectListItem { Text = clbTg.sName, Value = clbTg.iId.ToString() });
      }

      // Tutorial
      if (ttUser != null) {
        int iUserIx = MvcApplication.ckcore.ltUser.IndexOf(ckUser());
        if (iUserIx >= 0 && iUserIx < ttUser.Length) {
          cal.tutorial = ttUser[iUserIx];
          if (cal.tutorial.iLevel == 21) cal.tutorial.iLevel = 22;
        }
      }

      return View(cal);
    }

    public class DatatableTestGames
    {
      public string sDateIso { get; set; }
      public string sDate { get; set; }
      public string sTeamH { get; set; }
      public string sTeamA { get; set; }
    }
    public JsonResult CalendarGetTestgamesDatatable()
    {
      List<DatatableTestGames> ltDtTestgames = new List<DatatableTestGames>();

      List<Models.Testgame> ltTestgames = new List<Models.Testgame>();
      foreach (CornerkickManager.Cup cup in MvcApplication.ckcore.ltCups) {
        if (cup.iId == -MvcApplication.iCupIdTestgame) {
          foreach (CornerkickManager.Cup.Matchday md in cup.ltMatchdays) {
            foreach (CornerkickGame.Game.Data gd in md.ltGameData) {
              if (gd.team[1].iTeamId == ckClub().iId) {
                Models.Testgame tg = new Models.Testgame();
                tg.dt = md.dt;
                tg.iTeamHome = gd.team[0].iTeamId;
                tg.iTeamAway = gd.team[1].iTeamId;
                ltTestgames.Add(tg);
              }
            }
          }
        }
      }

      foreach (CornerkickWebMvc.Models.Testgame tg in ltTestgames) {
        DatatableTestGames dtTestgames = new DatatableTestGames();

        dtTestgames.sDate    = tg.dt.ToString("dd.MM.yyyy HH:mm");
        dtTestgames.sDateIso = tg.dt.ToString("yyyy-MM-ddTHH:mm");
        dtTestgames.sTeamH = MvcApplication.ckcore.ltClubs[tg.iTeamHome].sName;
        dtTestgames.sTeamA = MvcApplication.ckcore.ltClubs[tg.iTeamAway].sName;

        ltDtTestgames.Add(dtTestgames);
      }

      return Json(new { aaData = ltDtTestgames }, JsonRequestBehavior.AllowGet);
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
        if (!dt.Date.Equals(MvcApplication.ckcore.dtSeasonEnd.Date)) { // Always but not on day of season end
          ltEvents.Add(new Models.DiaryEvent {
            iID = ltEvents.Count,
            sTitle = "Nachtruhe",
            sDescription = "Nachtruhe",
            sStartDate = dt.Add(CornerkickManager.Main.tsNightStart).ToString("yyyy-MM-ddTHH:mm:ss"),
            sEndDate = dt.AddDays(1).Add(CornerkickManager.Main.tsNightEnd).ToString("yyyy-MM-ddTHH:mm:ss"),
            sColor = "rgb(0, 0, 140)",
            bEditable = false,
            bAllDay = false
          });
        }

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
        CornerkickManager.TrainingCamp.Booking booking = CornerkickManager.TrainingCamp.getCurrentCamp(club, dt, bInclTravel: true, bDate: true);
        if (booking != null) {
          if (dt.Date.Equals(booking.dtDeparture.Date)) {
            ltEvents.Add(new Models.DiaryEvent {
              iID = ltEvents.Count,
              sTitle = "Abreise Trainingslager",
              sDescription = booking.camp.sName,
              sStartDate = booking.dtDeparture.ToString("yyyy-MM-ddTHH:mm:ss"),
              sEndDate = booking.dtDeparture.Add(booking.camp.tsTravel).ToString("yyyy-MM-ddTHH:mm:ss"),
              sColor = "rgb(255, 100, 0)",
              bEditable = false,
              bAllDay = false
            });
          } else if (dt.Date.Equals(booking.dtReturn.Date)) {
            ltEvents.Add(new Models.DiaryEvent {
              iID = ltEvents.Count,
              sTitle = "Rückreise Trainingslager",
              sDescription = booking.camp.sName,
              sStartDate = booking.dtReturn.Subtract(booking.camp.tsTravel).ToString("yyyy-MM-ddTHH:mm:ss"),
              sEndDate = booking.dtReturn.ToString("yyyy-MM-ddTHH:mm:ss"),
              sColor = "rgb(255, 100, 0)",
              bEditable = false,
              bAllDay = false
            });
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

        // Events
        foreach (CornerkickManager.Club.Event.Item evi in club.ltEvent) {
          if (evi.dt.Date.Equals(dt)) {
            ltEvents.Add(new Models.DiaryEvent {
              iID = ltEvents.Count,
              sTitle = evi.ev.sName,
              sDescription = evi.ev.sName,
              sStartDate = evi.dt.ToString("yyyy-MM-ddTHH:mm:ss"),
              sEndDate = evi.dt.Add(evi.ev.tsLength).ToString("yyyy-MM-ddTHH:mm:ss"),
              sColor = "rgb(200, 200, 200)",
              sTextColor = "rgb(0, 0, 0)",
              bEditable = false,
              bAllDay = false
            });
          }
        }

        // Future training
        foreach (CornerkickManager.Main.TrainingPlan.Unit tu in club.training.ltUnit) {
          if (tu.dt.Date.Equals(dt) && tu.iType > 0 && tu.iType < 100) {
            string sTrainingName = CornerkickManager.PlayerTool.getTraining(tu.iType, MvcApplication.ckcore.plt.ltTraining).sName;

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
        foreach (CornerkickManager.Player.TrainingHistory th in club.ltTrainingHist) {
          if (th.dt.Date.Equals(dt) && th.iType > 1 && th.iType < 100) {
            string sTrainingName = CornerkickManager.PlayerTool.getTraining(th.iType, MvcApplication.ckcore.plt.ltTraining).sName;

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

                  string sRes = CornerkickManager.UI.getResultString(gd);

                  string sTitle = " " + cup.sName;
                  string sColor = "rgb(200, 0, 200)";
                  if (cup.iId == MvcApplication.iCupIdLeague) { // Nat. league
                    sTitle = " Liga, " + (iMd + 1).ToString().PadLeft(2) + ". Spieltag";
                    sColor = "rgb(0, 175, 100)";
                  } else if (cup.iId == MvcApplication.iCupIdNatCup) { // Nat. Cup
                    sTitle += ", " + CornerkickManager.Main.sCupRound[cup.getKoRound(md.ltGameData.Count)];
                    sColor = "rgb(100, 100, 255)";
                  } else if (cup.iId == MvcApplication.iCupIdGold || cup.iId == MvcApplication.iCupIdSilver || cup.iId == MvcApplication.iCupIdBronze) { // Int. games
                    sColor = "rgb(255, 200, 14)";
                  } else if (cup.iId == -MvcApplication.iCupIdTestgame) { // Testgame requests
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

                  if (cup.iId == MvcApplication.iCupIdNatCup && cup.iId2 == club.iLand && md.ltGameData.Count > 1) {
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
            } else if (cup.iId == MvcApplication.iCupIdNatCup && cup.iId2 == club.iLand && iMd == 0 && dt.Date.Equals(MvcApplication.ckcore.dtSeasonStart.AddDays(6).Date)) {
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

      // End of season
      ltEvents.Add(new Models.DiaryEvent {
        iID = ltEvents.Count,
        sTitle = "Saisonende",
        sDescription = "Saisonende",
        sStartDate = MvcApplication.ckcore.dtSeasonEnd.ToString("yyyy-MM-ddTHH:mm:ss"),
        sEndDate = MvcApplication.ckcore.dtSeasonEnd.Date.AddDays(1).ToString("yyyy-MM-ddTHH:mm:ss"),
        sColor = "rgb(0, 0, 0)",
        bEditable = false,
        bAllDay = false
      });

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
#if !DEBUG
          CornerkickGame.Game.Data gdNext = MvcApplication.ckcore.tl.getNextGame(clubRequest, md.dt, bPre: false);
          if (gdNext != null && (gdNext.dt - md.dt).TotalDays < 4) return Json("Anfrage für Testspiel abgelehnt. Begründung: Zu nah am nächsten Spiel", JsonRequestBehavior.AllowGet);

          CornerkickGame.Game.Data gdPrev = MvcApplication.ckcore.tl.getNextGame(clubRequest, md.dt, bPre: true);
          if (gdPrev != null && (md.dt - gdPrev.dt).TotalDays < 4) return Json("Anfrage für Testspiel abgelehnt. Begründung: Zu kurz nach letztem Spiel", JsonRequestBehavior.AllowGet);

#endif
          createTestgame(md);

          sReturn = "Testspiel am " + md.dt.ToString("d", getCi()) + " " + md.dt.ToString("t", getCi()) + " gegen " + clubRequest.sName + " vereinbart";

          club.nextGame = MvcApplication.ckcore.tl.getNextGame(club, MvcApplication.ckcore.dtDatum);

          // Clean club training plan for test game
          club.cleanTraining(MvcApplication.ckcore.settings.tsTrainingLength, club.nextGame);
        } else {
          CornerkickManager.Cup cup = new CornerkickManager.Cup();
          cup.iId = -MvcApplication.iCupIdTestgame;
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

        if (cup.iId == -MvcApplication.iCupIdTestgame) {
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

                  // Clean clubs training plan for test game
                  club .cleanTraining(MvcApplication.ckcore.settings.tsTrainingLength, club .nextGame);
                  clubH.cleanTraining(MvcApplication.ckcore.settings.tsTrainingLength, clubH.nextGame);

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
      CornerkickManager.Cup cupTestGames = MvcApplication.ckcore.tl.getCup(MvcApplication.iCupIdTestgame);

      if (cupTestGames == null) {
        cupTestGames = new CornerkickManager.Cup();
        cupTestGames.iId = MvcApplication.iCupIdTestgame;
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

        if (cup.iId == -MvcApplication.iCupIdTestgame) {
          foreach (CornerkickManager.Cup.Matchday md in cup.ltMatchdays) {
            if (md.dt.Equals(dt)) {
              foreach (CornerkickGame.Game.Data gd in md.ltGameData) {
                if (gd.team[1].iTeamId == clb.iId) {
                  cup.iId = MvcApplication.iCupIdTestgame;
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
        if (data.iGameType == MvcApplication.iCupIdTestgame) continue;

        if (dtStart.Date.Date.CompareTo(data.dt) == 0) return Json(  "Abreise am Spieltag nicht erlaubt!", JsonRequestBehavior.AllowGet);
        if (dtEnd  .Date.Date.CompareTo(data.dt) == 0) return Json("Rückreise am Spieltag nicht erlaubt!", JsonRequestBehavior.AllowGet);

        if (dtStart.Date.Date.CompareTo(data.dt) < 0 /* Start date before game date */ &&
            dtEnd  .Date.Date.CompareTo(data.dt) > 0 /* End date after game date */) {
          return Json("Trainingslager über Spieltag nicht erlaubt!", JsonRequestBehavior.AllowGet);
        }
      }

      CornerkickManager.TrainingCamp.Camp camp = MvcApplication.ckcore.tcp.ltCamps[iIx];

      CornerkickManager.TrainingCamp.bookCamp(ref club, camp, dtStart, dtEnd, MvcApplication.ckcore.dtDatum, MvcApplication.ckcore.settings.tsTrainingLength);

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

    public JsonResult CalendarAddTeamEvent(int iEventId, string sStart, int iH, int iM)
    {
      string sReturn = "Error";

      DateTime dtStart = new DateTime();
      if (!DateTime.TryParse(sStart, out dtStart)) return Json("Not a valid start date format!", JsonRequestBehavior.AllowGet);
      dtStart = dtStart.Date.AddHours(iH).AddMinutes(iM);

      CornerkickManager.Club clb = ckClub();
      if (clb == null) return Json(sReturn, JsonRequestBehavior.AllowGet);

      CornerkickManager.Club.Event.Item evi = new CornerkickManager.Club.Event.Item();
      evi.ev = MvcApplication.ckcore.tl.getEvent((byte)iEventId);
      evi.dt = dtStart;

      if (iEventId == 1) {
        sReturn = "Sie haben die Krisensitzung anberaumt.";
      } else if (iEventId == 2) {
        if (dtStart.Hour < 19) {
          return Json("Fehler: Die Mannschaft wünscht sich eine Weihnachtsfeier, die nicht vor 19 Uhr startet.", JsonRequestBehavior.AllowGet);
        }

        sReturn = "Sie haben die Weihnachtsfeier gebucht.";
      } else if (iEventId == 3) {
        if (dtStart.Add(evi.ev.tsLength).Hour > 19) {
          return Json("Fehler: Startdatum für den Jugendtag zu spät! Wählen Sie einen früheren Beginn.", JsonRequestBehavior.AllowGet);
        }

        sReturn = "Sie haben den Jugendtag geplant.";
      } else if (iEventId == 4) {
        sReturn = "Sie haben die Sommerparty geplant.";
      } else if (iEventId == 5) {
        sReturn = "Sie haben den Gehirnwäschetermin gebucht.";
      }

      clb.ltEvent.Add(evi);

      // Clean club training plan for event
      clb.cleanTraining(MvcApplication.ckcore.settings.tsTrainingLength);

      return Json(sReturn, JsonRequestBehavior.AllowGet);
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

        if (bd[0].iPaySalary == 0) bd[0].iPaySalary = clb.getSalaryPlayer();
        if (bd[0].iPayStaff  == 0) bd[0].iPayStaff  = clb.getSalaryStuff ();
      } else  if (usr.ltBudget.Count - 1 - iYear < usr.ltBudget.Count) {
        bd[0] = usr.ltBudget[usr.ltBudget.Count - 1 - iYear][0];
        bd[1] = usr.ltBudget[usr.ltBudget.Count - 1 - iYear][1];
      }

      long[][] iBudget = new long[2][];
      iBudget[0] = new long[17]; // Plan
      iBudget[1] = new long[17]; // Real

      for (byte i = 0; i < 2; i++) {
        iBudget[i][ 0] = bd[i].iInSpec;
        iBudget[i][ 1] = bd[i].iInBonusCup;
        iBudget[i][ 2] = bd[i].iInBonusSponsor;
        iBudget[i][ 3] = bd[i].iInTransfer;
        iBudget[i][ 4] = bd[i].iInMerchandising;
        iBudget[i][ 5] = bd[i].iInMisc;
        iBudget[i][ 6] = bd[i].iPaySalary;
        iBudget[i][ 7] = bd[i].iPayStaff;
        iBudget[i][ 8] = bd[i].iPayTransfer;
        iBudget[i][ 9] = bd[i].iPayStadium + bd[i].iPayStadiumSurr;
        iBudget[i][10] = bd[i].iPayTravel;
        iBudget[i][11] = bd[i].iPayInterest;
        iBudget[i][12] = bd[i].iPayMerchandising;
        iBudget[i][13] = bd[i].iPayMisc;

        long iInTotal  = MvcApplication.ckcore.fz.getBudgetInTotal (bd[i]);
        long iPayTotal = MvcApplication.ckcore.fz.getBudgetPayTotal(bd[i]);
        iBudget[i][14] = iInTotal;
        iBudget[i][15] = iPayTotal;

        iBudget[i][16] = iInTotal - iPayTotal;
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
          if      (gd.iGameType == MvcApplication.iCupIdLeague) sCupName = " - Liga";
          else if (gd.iGameType == MvcApplication.iCupIdNatCup) sCupName = " - Pokal";
          else if (gd.iGameType == MvcApplication.iCupIdGold) sCupName = " - Gold-Cup";
          else if (gd.iGameType == MvcApplication.iCupIdSilver) sCupName = " - Silver-Cup";
          else if (gd.iGameType == MvcApplication.iCupIdBronze) sCupName = " - Bronze-Cup";
          else if (gd.iGameType == MvcApplication.iCupIdTestgame) sCupName = " - Testspiel";

          int iSpecTotal = gd.iSpectators[0] + gd.iSpectators[1] + gd.iSpectators[2];
          if (iSpecTotal > 0) {
            string sInfo0 = gd.dt.ToString("d", getCi()) + sCupName + "</br>" +
                            (gd.iSpectators[0] + gd.iSpectators[1] + gd.iSpectators[2]).ToString("N0", getCi()) + " (" + gd.iSpectators[0].ToString("N0", getCi()) + "/" + gd.iSpectators[1].ToString("N0", getCi()) + "/" + gd.iSpectators[2].ToString("N0", getCi()) + ")";
            dataPoints[0].Add(new Models.DataPointGeneral(i, iSpecTotal, sInfo0));
          }

          int iStadiumSeats = Math.Max(gd.stadium.getSeats(), iSpecTotal);
          if (iStadiumSeats > 0) {
            string sInfo1 = gd.stadium.getSeats().ToString("N0", getCi()) + " (" + gd.stadium.getSeats(0).ToString("N0", getCi()) + "/" + gd.stadium.getSeats(1).ToString("N0", getCi()) + "/" + gd.stadium.getSeats(2).ToString("N0", getCi()) + ")</br>" +
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
        user.budget.iInSpec          = iBudgetIn[0];
        user.budget.iInBonusCup      = iBudgetIn[1];
        user.budget.iInBonusSponsor  = iBudgetIn[2];
        user.budget.iInMerchandising = iBudgetIn[3];
        user.budget.iInTransfer      = iBudgetIn[4];
        user.budget.iInMisc          = iBudgetIn[5];
      }

      if (iBudgetPay != null) {
        user.budget.iPaySalary        = iBudgetPay[0];
        user.budget.iPayStaff         = iBudgetPay[1];
        user.budget.iPayStadium       = iBudgetPay[2];
        user.budget.iPayMerchandising = iBudgetPay[3];
        user.budget.iPayTransfer      = iBudgetPay[4];
        user.budget.iPayTravel        = iBudgetPay[5];
        user.budget.iPayInterest      = iBudgetPay[6];
        user.budget.iPayMisc          = iBudgetPay[7];
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

    public JsonResult GetBalance()
    {
      CornerkickManager.Club clb = ckClub();
      if (clb == null) return Json(null, JsonRequestBehavior.AllowGet);

      return Json(new int[2] { clb.iBalance, clb.iBalanceSecret }, JsonRequestBehavior.AllowGet);
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
      CornerkickManager.User usr = ckUser();
      if (usr == null) return View(sponsorModel);

      CornerkickManager.Club clb = ckClub();
      if (clb == null) return View(sponsorModel);

      sponsorModel.bSound = true;
      if (usr.lti.Count > 1) sponsorModel.bSound = usr.lti[1] > 0;

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
      sponsorModel.sColorJersey = "rgb(" + clb.cl1[0].R.ToString() + "," + clb.cl1[0].G.ToString() + "," + clb.cl1[0].B.ToString() + ")";

      // Tutorial
      if (ttUser != null) {
        int iUserIx = MvcApplication.ckcore.ltUser.IndexOf(ckUser());
        if (iUserIx >= 0 && iUserIx < ttUser.Length) {
          sponsorModel.tutorial = ttUser[iUserIx];
          if (sponsorModel.tutorial.iLevel == 28) sponsorModel.tutorial.iLevel = 29;
        }
      }

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

    [HttpGet]
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

    [HttpGet]
    public ActionResult SponsorGetTableBoard()
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

          if (spon.iType != 1) continue;

          Models.DatatableEntrySponsorBoard deSponsorBoard = new Models.DatatableEntrySponsorBoard();
          deSponsorBoard.bOffer = bOffer;
          deSponsorBoard.iId = spon.iId;
          if (bOffer) deSponsorBoard.iIndex = iSpOffer - 1;
          spon.iId = (byte)Math.Min(spon.iId, MvcApplication.ckcore.fz.ltSponsoren.Count - 1);
          deSponsorBoard.sName = MvcApplication.ckcore.fz.ltSponsoren[spon.iId].name;
          deSponsorBoard.iMoneyVicHome = spon.iMoneyVicHome;
          deSponsorBoard.nBoards = spon.nBoards;
          deSponsorBoard.iYears = spon.iYears;

          query.Add(deSponsorBoard);
        }

        bOffer = true;
      }

      return Json(new { aaData = query.ToArray() }, JsonRequestBehavior.AllowGet);
    }

    [HttpGet]
    public ActionResult SponsorGetTableSpecial()
    {
      CornerkickManager.Club clb = ckClub();
      if (clb == null) return Json(false, JsonRequestBehavior.AllowGet);

      //The table or entity I'm querying
      List<Models.DatatableEntrySponsorSpecial> query = new List<Models.DatatableEntrySponsorSpecial>();

      int iSpOffer = 0;
      foreach (CornerkickManager.Finance.SponsorSpecial spsp in MvcApplication.ckcore.fz.ltSponsorSpecial) {
        Models.DatatableEntrySponsorSpecial deSponsorSpecial = new Models.DatatableEntrySponsorSpecial();
        deSponsorSpecial.bOffer = true;
        if (clb.ltSponsorSpecial != null) {
          foreach (CornerkickManager.Finance.SponsorSpecial.Contract spspc in clb.ltSponsorSpecial) {
            if (spspc.spsp.iId == spsp.iId) {
              deSponsorSpecial.bOffer = false;
              break;
            }
          }
        }
        deSponsorSpecial.iId = spsp.iId;
        deSponsorSpecial.sName = spsp.sName;
        deSponsorSpecial.iMoney = spsp.iMoney;
        if (spsp.iType == 1) deSponsorSpecial.sCondition = "Kein Gegentor";
        else if (spsp.iType == 2) deSponsorSpecial.sCondition = "4 oder mehr eigene Tore";
        else if (spsp.iType == 3) deSponsorSpecial.sCondition = "Keine Karten";
        else if (spsp.iType == 4) deSponsorSpecial.sCondition = "3 oder mehr Gegentore";
        else if (spsp.iType == 5) deSponsorSpecial.sCondition = "Kein eigenes Tor";

        query.Add(deSponsorSpecial);
      }

      return Json(new { aaData = query.ToArray() }, JsonRequestBehavior.AllowGet);
    }

    [HttpPost]
    public JsonResult SponsorSetSpecial(int iSponsorId)
    {
      CornerkickManager.Club clb = ckClub();
      if (clb == null) return Json(false, JsonRequestBehavior.AllowGet);

      if (clb.ltSponsorSpecial == null) clb.ltSponsorSpecial = new List<CornerkickManager.Finance.SponsorSpecial.Contract>();

      // Check if already two special sponsors
      if (clb.ltSponsorSpecial.Count > 1) return Json(false, JsonRequestBehavior.AllowGet);

      clb.ltSponsorSpecial.Add(new CornerkickManager.Finance.SponsorSpecial.Contract() { spsp = MvcApplication.ckcore.fz.getSponsorSpecial((byte)iSponsorId) });

      return Json(true, JsonRequestBehavior.AllowGet);
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
                                           Selected = i == 19
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
      int iDivision = 0;
      if (clbUser != null) {
        iLand = clbUser.iLand;
        iDivision = clbUser.iDivision;
      }

      statisticModel.sCupId = iLand.ToString() + ":" + iDivision.ToString();
      for (int iC = 0; iC < MvcApplication.ckcore.ltCups.Count; iC++) {
        CornerkickManager.Cup cup = MvcApplication.ckcore.ltCups[iC];

        if (cup.iId != 1) continue; // If not league

        statisticModel.ddlLeagues.Add(new SelectListItem {
                                        Text = cup.sName,
                                        Value = cup.iId2.ToString() + ":" + cup.iId3.ToString(),
                                        Selected = iLand == cup.iId2 && iDivision == cup.iId3
        }
        );
      }

      statisticModel.sPlayerSkillBest = new string[CornerkickManager.PlayerTool.sSkills.Length][];
      for (byte iS = 0; iS < CornerkickManager.PlayerTool.sSkills.Length; iS++) {
        if (iS == 17) continue; // Game intelligence skill skill
        if (iS == CornerkickGame.Player.iIndTrainingIxFoot) continue; // Both foot skill

        statisticModel.sPlayerSkillBest[iS] = new string[4]; // Skill name, player name, skill value, club

        CornerkickManager.Player plSkillBest = null;
        float fSkillBest = 0f;

        foreach (CornerkickManager.Player pl in MvcApplication.ckcore.ltPlayer) {
          if (pl.checkRetired()) continue;

          // Get position role
          byte iPos = 0;
          for (byte jPos = 1; jPos <= pl.plGame.fExperiencePos.Length; jPos++) {
            if (CornerkickGame.Tool.bPlayerMainPos(pl.plGame, iPos: jPos)) {
              iPos = jPos;
              break;
            }
          }

          float fSkillTmp = CornerkickGame.Tool.getSkillEff(pl.plGame, iS, iPos);
          if (fSkillTmp > fSkillBest) {
            plSkillBest = pl;
            fSkillBest = fSkillTmp;
          }
        }

        statisticModel.sPlayerSkillBest[iS][0] = CornerkickManager.PlayerTool.sSkills[iS];
        if (plSkillBest != null) {
          statisticModel.sPlayerSkillBest[iS][1] = plSkillBest.plGame.sName;
          statisticModel.sPlayerSkillBest[iS][2] = fSkillBest.ToString("0.000");
          string sClubName = "vereinslos";
          if (plSkillBest.contract?.club != null) sClubName = plSkillBest.contract.club.sName;
          statisticModel.sPlayerSkillBest[iS][3] = sClubName;
        }
      }

      return View(statisticModel);
    }

    [HttpGet]
    public JsonResult StatisticGetBest11(int iNat, int iF, bool bJouth = false)
    {
      Models.TeamModels.TeamData tD = new Models.TeamModels.TeamData();
      tD.ltPlayer2 = new List<Models.TeamModels.Player>();

      tD.formation = MvcApplication.ckcore.ltFormationen[iF];

      List<CornerkickManager.Player> ltPlayerBest = new List<CornerkickManager.Player>();

      for (byte iP = 0; iP < 11; iP++) {
        float fStrength = 0f;
        tD.ltPlayer2.Add(null);
        ltPlayerBest.Add(null);

        byte iPosExact = CornerkickGame.Tool.getPosRole(MvcApplication.ckcore.ltFormationen[iF].ptPos[iP], MvcApplication.ckcore.game.ptPitch);
        byte iPos = CornerkickGame.Tool.getBasisPos(iPosExact);

        foreach (CornerkickManager.Player pl in MvcApplication.ckcore.ltPlayer) {
          if (pl.bRetire) continue;
          if (bJouth && pl.plGame.getAge(MvcApplication.ckcore.dtDatum) > 18f) continue;
          if (iNat >= 0 && pl.iNat1 != iNat) continue;

          // Check if club is nation
          if (pl.contract?.club != null) {
            if (pl.contract.club.bNation) continue;
          }

          // Check if same player already in same role
          if (iPos > 0) {
            bool bSame = false;
            foreach (CornerkickManager.Player plSame in ltPlayerBest) {
              if (plSame != null && plSame.plGame.iId == pl.plGame.iId && plSame.plGame.fExperiencePos[iPos - 1] > 0.999) {
                bSame = true;
                break;
              }
            }
            if (bSame) continue;
          }

          float fStrengthTmp = CornerkickGame.Tool.getAveSkill(pl.plGame, iPos);
          if (fStrengthTmp > fStrength) {
            if (tD.ltPlayer2[iP] == null) tD.ltPlayer2[iP] = new Models.TeamModels.Player();

            tD.ltPlayer2[iP].sName = pl.plGame.sName;
            tD.ltPlayer2[iP].iNb = (byte)(iP + 1);
            tD.ltPlayer2[iP].sNat = CornerkickManager.Main.sLandShort[pl.iNat1];
            tD.ltPlayer2[iP].sPortrait = getPlayerPortrait(pl, sStyle: "height: 100%; width: 100%; object-fit: contain", bSmall: true);
            tD.ltPlayer2[iP].iPos = iPos;

            if (tD.formation.ptPos.Length > iP) {
              tD.ltPlayer2[iP].sSkillAve = fStrengthTmp.ToString("0.0");
            }

            fStrength = fStrengthTmp;

            if (pl.contract?.club != null) tD.ltPlayer2[iP].sTeamname = pl.contract.club.sName;
            tD.ltPlayer2[iP].sAge = pl.plGame.getAge(MvcApplication.ckcore.dtDatum).ToString("0.0");

            ltPlayerBest[iP] = pl;
          }
        }
      }

      float[] fTeamAve11 = CornerkickManager.Tool.getTeamAve(ltPlayerBest, MvcApplication.ckcore.dtDatum, MvcApplication.ckcore.dtSeasonEnd, iPlStop: 11);
      tD.fTeamAveStrength = fTeamAve11[3];
      tD.fTeamAveAge      = fTeamAve11[4];

      return Json(tD, JsonRequestBehavior.AllowGet);
    }

    [HttpGet]
    public JsonResult StatisticGetTableTeams(string sCupId)
    {
      //The table or entity I'm querying
      List<Models.DatatableEntryTeams> ltDeTeams = new List<Models.DatatableEntryTeams>();

      int iLand = -1;
      int iDivision = -1;

      if (!string.IsNullOrEmpty(sCupId)) {
        string[] sCupIdSplit = sCupId.Split(':');
        if (sCupIdSplit.Length > 1) {
          iLand = int.Parse(sCupIdSplit[0]);
          iDivision = int.Parse(sCupIdSplit[1]);
        }
      }

      int iC = 0;
      foreach (CornerkickManager.Club clb in MvcApplication.ckcore.ltClubs) {
        if (clb.iLand < 0) continue;
        if (clb.bNation) continue;
        if (iLand >= 0 && iLand != clb.iLand) continue;
        if (iDivision >= 0 && iDivision != clb.iDivision) continue;

        float[] fAve = CornerkickManager.Tool.getTeamAve(clb, MvcApplication.ckcore.dtDatum, MvcApplication.ckcore.dtSeasonEnd, bTeamValue: true);
        string sSkill = fAve[3].ToString("0.0");
        string sAge   = fAve[4].ToString("0.0");
        int    iVal   = (int)fAve[5];

        float[] fAve11 = CornerkickManager.Tool.getTeamAve(clb, MvcApplication.ckcore.dtDatum, MvcApplication.ckcore.dtSeasonEnd, ptPitch: MvcApplication.ckcore.game.ptPitch, iPlStop: 11, bTeamValue: true);
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
          fAttrFactor = clb.getAttractionFactor(MvcApplication.ckcore.iSeason, ltCups: MvcApplication.ckcore.ltCups, dtNow: MvcApplication.ckcore.dtDatum),
          sLeague = sLeagueName
        });

        iC++;
      }

      return Json(new { aaData = ltDeTeams.ToArray() }, JsonRequestBehavior.AllowGet);
    }

    [HttpGet]
    public JsonResult StatisticGetTransferTable()
    {
      //The table or entity I'm querying
      List<Models.PlayerModel.DatatableEntryClubHistory> ltDeClubHistory = new List<Models.PlayerModel.DatatableEntryClubHistory>();

      int iT = 1;
      foreach (CornerkickManager.Player pl in MvcApplication.ckcore.ltPlayer) {
        for (int iCh = 0; iCh < pl.ltClubHistory.Count; iCh++) {
          CornerkickManager.Player.ClubHistory ch = pl.ltClubHistory[iCh];

          if (ch.iTransferFee > 0) {
            if (ch.club == null) {
              ch.iTransferFee = 0;
              continue;
            }

            // Get name of new club
            string sClubTakeName = ch.club.sName;

            // Get name of old club
            if (iCh > 0) {
              CornerkickManager.Player.ClubHistory chLast = pl.ltClubHistory[iCh - 1];

              // If no last club --> no transfer --> continue
              if (chLast.club == null) {
                ch.iTransferFee = 0;
                continue;
              }

              string sClubGiveName = chLast.club.sName;

              ltDeClubHistory.Add(new Models.PlayerModel.DatatableEntryClubHistory {
                iIx = iT++,
                sPlayerName = pl.plGame.sName,
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

    [HttpGet]
    public JsonResult StatisticGetStadiumsTable()
    {
      //The table or entity I'm querying
      List<Models.DatatableEntryStadiums> ltDeStadiums = new List<Models.DatatableEntryStadiums>();

      int iT = 1;
      foreach (CornerkickManager.Club clb in MvcApplication.ckcore.ltClubs) {
        Models.DatatableEntryStadiums dteStadium = new Models.DatatableEntryStadiums();

        dteStadium.sName = clb.stadium.sName;
        dteStadium.sClubName = clb.sName;
        dteStadium.iType0 = clb.stadium.getSeats(0);
        dteStadium.iType1 = clb.stadium.getSeats(1);
        dteStadium.iType2 = clb.stadium.getSeats(2);

        foreach (CornerkickGame.Stadium.Block blk in clb.stadium.blocks) {
          if (blk.iSeatsDaysConstruct > 0) {
            if      (blk.iType == 0) dteStadium.iType0Ctn += blk.iSeats;
            else if (blk.iType == 1) dteStadium.iType1Ctn += blk.iSeats;
            else if (blk.iType == 2) dteStadium.iType2Ctn += blk.iSeats;
          }
        }

        dteStadium.iTotal = dteStadium.iType0 + dteStadium.iType1 + dteStadium.iType2;
        dteStadium.iTotalCtn = dteStadium.iType0Ctn + dteStadium.iType1Ctn + dteStadium.iType2Ctn;

        dteStadium.bTopring = clb.stadium.bTopring && clb.stadium.iTopringDaysConstruct == 0;

        ltDeStadiums.Add(dteStadium);
      }

      ltDeStadiums = ltDeStadiums.OrderByDescending(o => o.iTotal).ToList().GetRange(0, 20);
      for (int i = 0; i < ltDeStadiums.Count; i++) {
        ltDeStadiums[i].iIx = i + 1;
      }

      return Json(new { aaData = ltDeStadiums.ToArray() }, JsonRequestBehavior.AllowGet);
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

      // Delete mail from Amazon S3
      AmazonS3FileTransfer as3 = new AmazonS3FileTransfer();
      as3.deleteFile("mail/" + getMailFilename(user, dtMail, bFullPath: false));

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

    private static string getMailFilename(CornerkickManager.User user, DateTime dtMail, bool bFullPath = true)
    {
      string sFilename = user.id + "_" + dtMail.ToString("yyyyMMddHHmmss") + ".txt";
      if (!bFullPath) return sFilename;

      return System.IO.Path.Combine(MvcApplication.ckcore.settings.sHomeDir, "mail", sFilename);
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

      string sFileWl = System.IO.Path.Combine(MvcApplication.ckcore.settings.sHomeDir, "wishlist.json");
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
      string sFileWl = System.IO.Path.Combine(MvcApplication.ckcore.settings.sHomeDir, "wishlist.json");

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
      if (dt.TimeOfDay.CompareTo(CornerkickManager.Main.tsNightStart) >= 0) return -3;
      if (dt.TimeOfDay.CompareTo(CornerkickManager.Main.tsNightEnd)   <= 0) return -3;

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