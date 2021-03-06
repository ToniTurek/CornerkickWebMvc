﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using CornerkickWebMvc.Models;

namespace CornerkickWebMvc.Controllers
{
  [Authorize]
  public class AccountController : Controller
  {
    public AccountController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
    {
      UserManager   = userManager;
      SignInManager = signInManager;
    }

    public class ApplicationUserPlus
    {
      public ApplicationUser appUser { get; set; }

      public int iLand { get; set; }
      public int iLiga { get; set; }
      public int iClubIx { get; set; }
      public System.Web.HttpPostedFileBase fileEmblem { get; set; }

      public Color clH1 { get; set; }
      public Color clH2 { get; set; }
      public Color clH3 { get; set; }
      public Color clA1 { get; set; }
      public Color clA2 { get; set; }
      public Color clA3 { get; set; }

      public string sCallbackUrl { get; set; }

      public ApplicationUserPlus(ApplicationUser appUser, RegisterViewModel rvm = null)
      {
        this.appUser = appUser;

        this.iClubIx = -1;

        if (rvm != null) {
          this.iLand = rvm.Land;
          this.iLiga = rvm.Liga;

          this.iClubIx = rvm.iClubIx;
          this.fileEmblem = rvm.fileEmblem;

          this.clH1 = rvm.clH1;
          this.clH2 = rvm.clH2;
          this.clH3 = rvm.clH3;
          this.clA1 = rvm.clA1;
          this.clA2 = rvm.clA2;
          this.clA3 = rvm.clA3;
        }
      }
    }

#if _CONSOLE
    public static CornerkickConsole.CUI ckconsole;
#endif
    //public static System.Timers.Timer timerRefresh = new System.Timers.Timer(10000);

    private Random random = new Random();
    private ApplicationSignInManager _signInManager;
    private ApplicationUserManager _userManager;
    public static CultureInfo ciUser;
    public static List<ApplicationUserPlus> ltRegisterAppUser;
    const string sFilenameRegisterUser = "registerUser.txt";
    readonly string[] sCultureInfo = new string[82] {
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

    public AccountController()
    {
      //timerRefresh.Elapsed += new System.Timers.ElapsedEventHandler(timerRefresh_Elapsed);
      //timerRefresh.Enabled = true;
    }

    /*
    static void timerRefresh_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
    {
      var result = new MemberController().Console();
    }
    */

#if DEBUG
    public void iniCk()
#else
    private void iniCk()
#endif
    {
#if _CONSOLE
      //ckconsole = new CornerkickConsole.CUI(MvcApplication.ckcore);
#endif

      CornerkickManager.User usr = ckUser();
      if (usr      == null) return;
      if (usr.club == null) return;

      int iLandUser = usr.club.iLand;
      if (iLandUser >= 0 && iLandUser < sCultureInfo.Length) ciUser = new CultureInfo(sCultureInfo[iLandUser]);

      if (usr.ltNews != null) {
        for (int iN = 0; iN < usr.ltNews.Count; iN++) {
          CornerkickManager.Main.News news = usr.ltNews[iN];
          if (news.bRead2) news.bRead = true;

          news.bRead2 = true;

          usr.ltNews[iN] = news;
        }
      }
    }

    public CornerkickManager.User ckUser()
    {
      string sUserId = User.Identity.GetUserId();
      foreach (CornerkickManager.User usr in MvcApplication.ckcore.ltUser) {
        if (usr.id.Equals(sUserId)) return usr;
      }

      return null;
    }

    public static int getiUser(CornerkickManager.User usr)
    {
      for (int iU = 0; iU < MvcApplication.ckcore.ltUser.Count; iU++) {
        if (MvcApplication.ckcore.ltUser[iU].id.Equals(usr.id)) {
          return iU;
        }
      }

      return -1;
    }

    public CornerkickManager.Club ckClub()
    {
      CornerkickManager.User usr = ckUser();
      if (usr == null) return null;

      return usr.club;
    }

    public void addUserToCk(ApplicationUserPlus applicationUser, bool bAdmin = false)
    {
      addUserToCk(applicationUser.appUser, applicationUser.iLand, applicationUser.clH1, applicationUser.clH2, applicationUser.clH3, applicationUser.clA1, applicationUser.clA2, applicationUser.clA3, bAdmin: bAdmin, iClubExist: applicationUser.iClubIx, fileEmblem: applicationUser.fileEmblem);
    }
#if DEBUG
    public void addUserToCk(ApplicationUser applicationUser, int iLand, System.Drawing.Color clH1, System.Drawing.Color clH2, System.Drawing.Color clH3, System.Drawing.Color clA1, System.Drawing.Color clA2, System.Drawing.Color clA3, bool bAdmin = false, int iClubExist = -1, HttpPostedFileBase fileEmblem = null)
#else
    private void addUserToCk(ApplicationUser applicationUser, int iLand, System.Drawing.Color clH1, System.Drawing.Color clH2, System.Drawing.Color clH3, System.Drawing.Color clA1, System.Drawing.Color clA2, System.Drawing.Color clA3, bool bAdmin = false, int iClubExist = -1, HttpPostedFileBase fileEmblem = null)
#endif
    {
      // Check if user is already added
      foreach (CornerkickManager.User usrExist in MvcApplication.ckcore.ltUser) {
        if (usrExist.id.Equals(applicationUser.Id)) return;
      }

      // Get existing club
      CornerkickManager.Club clubExist = null;
      if (iClubExist >= 0 && iClubExist < MvcApplication.ckcore.ltClubs.Count) clubExist = MvcApplication.ckcore.ltClubs[iClubExist];

      int iDivision = 0;

      MvcApplication.ckcore.tl.writeLog("  add User to Ck: " + applicationUser.Vorname + " " + applicationUser.Nachname);

      if (bAdmin) {
        MvcApplication.ckcore.ltClubs.Add(new CornerkickManager.Club());
        CornerkickManager.User usrAdmin = createUser(applicationUser);
        MvcApplication.ckcore.ltUser.Add(usrAdmin);

        return;
      }

      for (int iD = 2; iD >= 0; iD--) {
        if (MvcApplication.ckcore.tl.getCup(1, iLand, iD) != null) {
          iDivision = iD;
          break;
        }
      }

      if (iLand     < 0) return;
      if (iDivision < 0) return;

      CornerkickManager.Cup league = MvcApplication.ckcore.tl.getCup(1, iLand, iDivision);
      CornerkickManager.Cup cup    = MvcApplication.ckcore.tl.getCup(2, iLand);

#if DEBUG
      int nUser = 1;
      if (clubExist != null) nUser = 1;
      for (byte iU = 0; iU < nUser; iU++) {
#endif
      CornerkickManager.Club clb = null;
      if (clubExist == null) {
        // Remove CPU club from league and nat. cup
        for (int iC = 0; iC < MvcApplication.ckcore.ltClubs.Count; iC++) {
          CornerkickManager.Club clubCpu = MvcApplication.ckcore.ltClubs[iC];
          if (clubCpu      == null) continue;
          if (clubCpu.user != null) continue;

          if (league != null) {
            if (league.ltClubs[0].IndexOf(clubCpu) >= 0) {
              /*
              league.ltClubs[0].Remove(clubCpu);

              if (cup != null) {
                if (cup.ltClubs[0].IndexOf(clubCpu) >= 0) cup.ltClubs[0].Remove(clubCpu);
              }

              MvcApplication.ckcore.ltClubs.Remove(clubCpu);
              foreach (CornerkickGame.Player plRemove in clubCpu.ltPlayer) MvcApplication.ckcore.ltPlayer.Remove(plRemove);
              clubCpu = null;
              */

              // Retire all cpu player
              while (clubCpu.ltPlayer     .Count > 0) MvcApplication.ckcore.plt.retirePlayer(clubCpu.ltPlayer     [0], clubCpu);
              while (clubCpu.ltPlayerJouth.Count > 0) MvcApplication.ckcore.plt.retirePlayer(clubCpu.ltPlayerJouth[0], clubCpu);

              clb = createClub(applicationUser.Vereinsname, (byte)iLand, (byte)iDivision, clubCpu);

              // Assign club colors
              clubCpu.cl1[0] = clH1;
              clubCpu.cl1[1] = clH2;
              clubCpu.cl1[2] = clH3;
              clubCpu.cl2[0] = clA1;
              clubCpu.cl2[1] = clA2;
              clubCpu.cl2[2] = clA3;

              addPlayerToClub(ref clubCpu);

              // Do initial formation
              MvcApplication.ckcore.doFormation(clubCpu.iId);

#if DEBUG
              clubCpu.sName += "_" + clubCpu.iId.ToString();
#endif
              
              break;
            }
          }
        }

        if (clb == null) return;
      } else {
        clb = clubExist;
        clb.sName = applicationUser.Vereinsname;
      }

      // Set club account to 10 mio.
      clb.iBalance = 10000000;
      clb.iBalanceSecret = 0;
      
      CornerkickManager.User usr = createUser(applicationUser);
      usr.iLevel = 1;
      usr.club = clb;
      usr.nextGame.iGameSpeed = 250;
      usr.dtClubStart = MvcApplication.ckcore.dtDatum;
      
#if DEBUG
      if (iU == 0) {
#endif
      clb.user = usr;
      MvcApplication.ckcore.ltUser.Add(usr);

      Controllers.MemberController.initialiteTutorial();
#if DEBUG
      }
#endif

      if (clb != null && fileEmblem != null) {
        Task<bool> tkUploadEmblem = Task.Run(async () => await uploadFileAsync(fileEmblem, "emblems", clb.iId));
      }

#if DEBUG
      if (iU == 0) {
#endif
      string sWelcomeMsg = usr.sFirstname + " " + usr.sSurname + ", herzlich Willkommen bei Ihrem neuen Verein " + clb.sName + "!";
      MvcApplication.ckcore.sendNews(usr, sWelcomeMsg,  3, usr.club.iId);
      string sWelcomeMsg2 = "Schauen Sie sich die Anleitung um mehr über die Funktionsweise von Cornerkick zu erfahren.";
      MvcApplication.ckcore.sendNews(usr, sWelcomeMsg2, 3, usr.club.iId);

      // Create newspaper
      string sNewspaper = "Herzlich Willkommen!#" + usr.sFirstname + " " + usr.sSurname + " steigt als neuer Manager bei " + clb.sName + " ein. ";
      sNewspaper += "Aktuell befindet sich der Verein in der " + league.sName + ". ";
      sNewspaper += "In Fach&shy;kreisen werden dem Verein unter der neuen Leitung große Ambitionen nachgesagt...";
      MvcApplication.ckcore.sendNews(MvcApplication.ckcore.ltUser[0], sNewspaper, 203);
#if DEBUG
      }
      }
#endif

      /*
      if (!MvcApplication.timerCkCalender.Enabled && iClubExist < 0) {
        MvcApplication.ckcore.calcMatchdays();
        MvcApplication.ckcore.drawCup(league);
      }
      */
    }

    private CornerkickManager.User createUser(ApplicationUser applicationUser)
    {
      CornerkickManager.User usr = new CornerkickManager.User();

      if (applicationUser == null) return usr;

      usr.id = applicationUser.Id;

      usr.sFirstname = applicationUser.Vorname .Trim();
      usr.sSurname   = applicationUser.Nachname.Trim();
      
      return usr;
    }

    internal static bool checkUserIsAdmin(string sEmail, string sPw = "")
    {
      string sAdminEmail = ConfigurationManager.AppSettings["ckAdminEmail"];
      if (string.IsNullOrEmpty(sAdminEmail)) return false;
      if (!sEmail.Equals(sAdminEmail)) return false;

      if (!string.IsNullOrEmpty(sPw)) {
        if (!sPw.Equals("!Cornerkick1")) return false;
      }

      return true;
    }
    internal static bool checkUserIsAdmin(ApplicationUser appUserAdmin)
    {
      if (appUserAdmin == null) return false;

      return checkUserIsAdmin(appUserAdmin.Email);
    }
    internal static bool checkUserIsAdmin(RegisterViewModel regModel)
    {
      return checkUserIsAdmin(regModel.Email, regModel.Password);
    }
    internal static bool checkUserIsAdmin(System.Security.Principal.IPrincipal user)
    {
      return checkUserIsAdmin(user.Identity.Name);
    }

    internal CornerkickManager.Club createClub(string sTeamname, byte iLand, byte iLiga, CornerkickManager.Club clbReplace = null)
    {
      CornerkickManager.Club clb = clbReplace;
      if (clbReplace == null) clb = new CornerkickManager.Club();

      clb.sName = sTeamname.Trim();
      if (string.IsNullOrEmpty(clb.sName)) clb.sName = "Team";

      if (clbReplace == null) clb.iId = MvcApplication.ckcore.ltClubs.Count;

      clb.iLand = iLand;
      clb.iDivision = iLiga;

      clb.ltTactic[0].formation = MvcApplication.ckcore.ltFormationen[7];

#if DEBUG
      clb.training.ltUnit.Add(new CornerkickManager.Main.TrainingPlan.Unit() { dt = MvcApplication.ckcore.dtDatum.Date.AddDays(1).Add(new TimeSpan(9, 30, 0)), iType = 2 });
      clb.training.ltUnit.Add(new CornerkickManager.Main.TrainingPlan.Unit() { dt = MvcApplication.ckcore.dtDatum.Date.AddDays(1).Add(new TimeSpan(9, 30, 0)), iType = 3 });
      clb.training.ltUnit.Add(new CornerkickManager.Main.TrainingPlan.Unit() { dt = MvcApplication.ckcore.dtDatum.Date.AddDays(1).Add(new TimeSpan(9, 30, 0)), iType = 4 });
      clb.training.ltUnit.Add(new CornerkickManager.Main.TrainingPlan.Unit() { dt = MvcApplication.ckcore.dtDatum.Date.AddDays(1).Add(new TimeSpan(9, 30, 0)), iType = 6 });
      clb.training.ltUnit.Add(new CornerkickManager.Main.TrainingPlan.Unit() { dt = MvcApplication.ckcore.dtDatum.Date.AddDays(1).Add(new TimeSpan(9, 30, 0)), iType = 9 });
      clb.training.ltUnit.Add(new CornerkickManager.Main.TrainingPlan.Unit() { dt = MvcApplication.ckcore.dtDatum.Date.AddDays(1).Add(new TimeSpan(9, 30, 0)), iType = 1 });
#endif

      for (byte iB = 0; iB < 3; iB++) {
        clb.stadium.blocks[iB].iSeats = 2000;
        clb.stadium.blocks[iB].iType = 1;
      }
      for (byte iB = 3; iB < 8; iB++) clb.stadium.blocks[iB].iSeats = 1000;

      // Clear sponsors
      clb.sponsorMain = new CornerkickManager.Finance.Sponsor();
      clb.ltSponsorBoards.Clear();
      clb.ltSponsorOffers.Add(createDefaultSponsor());

      // Clear account
      clb.ltAccount.Clear();

      // Remove staff
      clb.staff = new CornerkickManager.Main.Staff();

      // Clear training
      clb.training = new CornerkickManager.Main.TrainingPlan();

      // Clear captain
      clb.iCaptainId = new int[3] { -1, -1, -1 };

      // Clear record games
      clb.ltGameRecord.Clear();

      // Stadium
      clb.stadium.sName = clb.sName + " Stadion";
      clb.stadium.iTicketcounter = 1;
      clb.stadium.iCarpark = 5;
      clb.buildings.iGround = 2;

      clb.iAdmissionPrice[0] =  10;
      clb.iAdmissionPrice[1] =  30;
      clb.iAdmissionPrice[2] = 100;

      clb.iAdmissionPriceSeasonal[0] =  200;
      clb.iAdmissionPriceSeasonal[1] =  600;
      clb.iAdmissionPriceSeasonal[2] = 2000;

      // Clear position last season (for new sponsors)
      clb.iPosLastSeason = 0;

      // Clear successes
      clb.ltSuccess.Clear();

      return clb;
    }

    private CornerkickManager.Finance.Sponsor createDefaultSponsor()
    {
      CornerkickManager.Finance.Sponsor sponUser = CornerkickManager.Finance.newSponsor();

      sponUser.iType = 0;
      sponUser.iId = 1;
      sponUser.iYears = 1;
      sponUser.iGeldJahr     = 15000000; // 15 mio.
      sponUser.iGeldMeister  =  1000000; //  1 mio.
      sponUser.iMoneyVicHome =    10000;

      return sponUser;
    }

    internal void addPlayerToClub(ref CornerkickManager.Club club, int iSkillChange = 0)
    {
      int   iSpeed  = 0;
      float  fTalent = 0;
      float fAlter  = 0f;
      Random rnd = new Random();

      for (byte iPos = 1; iPos < 12; iPos++) {
        for (byte iPl = 0; iPl < 2; iPl++) {
          CornerkickManager.Player pl = MvcApplication.ckcore.plt.newPlayer(club, iPos);
#if DEBUG
          pl.plGame.fFresh = 1f;
#endif

          // Change player skill
          for (int iS = 1; iS < pl.plGame.iSkill.Length; iS++) {
            pl.plGame.iSkill[iS] = (byte)(pl.plGame.iSkill[iS] + iSkillChange);
          }

          pl.contract = CornerkickManager.PlayerTool.getContract(pl, (byte)rnd.Next(1, 4), club, MvcApplication.ckcore.dtDatum, MvcApplication.ckcore.dtSeasonEnd);

          pl.plGame.iNr = (byte)(iPos + (11 * iPl));

          // Count speed
          iSpeed  += pl.plGame.iSkill[0];

          // Count talent
          fTalent += pl.getTalentAve();

          // Count age
          float fAge = pl.plGame.getAge(MvcApplication.ckcore.dtDatum);
          fAlter += fAge;
        }
      }

      // Equalize player speed
      byte iCount7 = 0;
      byte iCount5 = 0;
      for (int iPl = 0; iPl < club.ltPlayer.Count; iPl++) {
        CornerkickManager.Player pl = club.ltPlayer[iPl];
        pl.plGame.iSkill[0] = 6;
      }

      while (iCount7 < 2 || iCount5 < 1) {
        CornerkickManager.Player pl = club.ltPlayer[random.Next(club.ltPlayer.Count)];

        if (!CornerkickGame.Tool.bPlayerMainPos(pl.plGame, 1)) { // if not keeper
          if (iCount7 < 2) {
            pl.plGame.iSkill[0] = 7;
            iCount7++;
          } else if (iCount5 < 1) {
            pl.plGame.iSkill[0] = 5;
            iCount5++;
          }
        }
      }
      /*
      while (iSpeed < 6 * club.ltPlayer.Count) {
        for (byte iPl = 0; iPl < club.ltPlayer.Count; iPl++) {
          CornerkickGame.Player pl = MvcApplication.ckcore.ltPlayer[club.ltPlayer[iPl]];
          if (pl.iF[0] < 6) {
            pl.iF[0]++;
            MvcApplication.ckcore.ltPlayer[club.ltPlayer[iPl]] = pl;

            iSpeed++;
            break;
          }
        }
      }

      while (iSpeed > 6 * club.ltPlayer.Count) {
        for (byte iPl = 0; iPl < club.ltPlayer.Count; iPl++) {
          CornerkickGame.Player pl = MvcApplication.ckcore.ltPlayer[club.ltPlayer[iPl]];
          if (pl.iF[0] > 6) {
            pl.iF[0]--;
            MvcApplication.ckcore.ltPlayer[club.ltPlayer[iPl]] = pl;

            iSpeed++;
            break;
          }
        }
      }
      */

      // Equalize player talent
      while (fTalent > 4.51 * club.ltPlayer.Count) {
        CornerkickManager.Player pl = club.ltPlayer[random.Next(club.ltPlayer.Count)];
        if (pl.getTalentAve() > 2f) {
          for (byte iT = 0; iT < pl.iTalent.Length; iT++) {
            if (pl.iTalent[iT] > 0) pl.iTalent[iT]--;
          }

          fTalent -= 1f;
        }
      }

      while (fTalent < 4.49 * club.ltPlayer.Count) {
        CornerkickManager.Player pl = club.ltPlayer[random.Next(club.ltPlayer.Count)];
        if (pl.getTalentAve() < 7f) {
          for (byte iT = 0; iT < pl.iTalent.Length; iT++) {
            if (pl.iTalent[iT] < 9) pl.iTalent[iT]++;
          }

          fTalent += 1f;
        }
      }

      // Equalize player age
      while (fAlter > 24.0f * club.ltPlayer.Count) {
        CornerkickManager.Player pl = club.ltPlayer[random.Next(club.ltPlayer.Count)];

        if (pl.plGame.getAge(MvcApplication.ckcore.dtDatum) > 22) {
          try {
            pl.plGame.dtBirthday = new DateTime(pl.plGame.dtBirthday.Year + 1, pl.plGame.dtBirthday.Month, pl.plGame.dtBirthday.Day); // make younger
          } catch {
            continue;
          }

          fAlter -= 1f;
        }
      }

      while (fAlter < 24.0f * club.ltPlayer.Count) {
        CornerkickManager.Player pl = club.ltPlayer[random.Next(club.ltPlayer.Count)];

        if (pl.plGame.getAge(MvcApplication.ckcore.dtDatum) < 30) {
          try {
            pl.plGame.dtBirthday = new DateTime(pl.plGame.dtBirthday.Year - 1, pl.plGame.dtBirthday.Month, pl.plGame.dtBirthday.Day); // make older
          } catch {
            continue;
          }

          fAlter += 1f;
        }
      }

      // Equalize factor of player (age - 10) * talent 
      // --> (24.0 - 10) * 4.5 = 63.0
      int iBreak = 0;
      float fAgeTalent = getAgeTalent(club);
      while (fAgeTalent > 64.0 || fAgeTalent < 62.0) {
        CornerkickManager.Player pl1 = club.ltPlayer[random.Next(club.ltPlayer.Count)];
        CornerkickManager.Player pl2 = club.ltPlayer[random.Next(club.ltPlayer.Count)];

        if (pl1.getTalentAve() > 2f && pl1.getTalentAve() < 7f &&
            pl2.getTalentAve() > 2f && pl2.getTalentAve() < 7f) {
          int iDeltaTalent = +1;
          if ((fAgeTalent > 70.75 && pl1.plGame.dtBirthday.Year > pl2.plGame.dtBirthday.Year) ||
              (fAgeTalent < 68.75 && pl1.plGame.dtBirthday.Year < pl2.plGame.dtBirthday.Year)) iDeltaTalent = -1;

          for (byte iT = 0; iT < pl1.iTalent.Length; iT++) pl1.iTalent[iT] = (byte)Math.Max(pl1.iTalent[iT] - iDeltaTalent, 0);
          for (byte iT = 0; iT < pl2.iTalent.Length; iT++) pl2.iTalent[iT] = (byte)Math.Min(pl2.iTalent[iT] + iDeltaTalent, 9);

          fAgeTalent = getAgeTalent(club);
        } else if (pl1.plGame.getAge(MvcApplication.ckcore.dtDatum) > 22 &&
                   pl2.plGame.getAge(MvcApplication.ckcore.dtDatum) < 30) {
          try {
            pl1.plGame.dtBirthday = new DateTime(pl1.plGame.dtBirthday.Year + 1, pl1.plGame.dtBirthday.Month, pl1.plGame.dtBirthday.Day); // make younger
          } catch {
            MvcApplication.ckcore.tl.writeLog("ERROR: cannot make player " + pl1.plGame.sName + " younger. Player birthday: " + pl1.plGame.dtBirthday.ToShortDateString());
          }

          try {
            pl2.plGame.dtBirthday = new DateTime(pl2.plGame.dtBirthday.Year - 1, pl2.plGame.dtBirthday.Month, pl2.plGame.dtBirthday.Day); // make older
          } catch {
            MvcApplication.ckcore.tl.writeLog("ERROR: cannot make player " + pl2.plGame.sName + " older. Player birthday: " + pl2.plGame.dtBirthday.ToShortDateString());
          }

          fAgeTalent = getAgeTalent(club);
        }

        iBreak++;
        if (iBreak > 1000) {
          break;
        }
      }
    }

    private float getAgeTalent(CornerkickManager.Club club)
    {
      float fAgeTalent = 0f;

      // Count age
      foreach (CornerkickManager.Player pl in club.ltPlayer) {
        fAgeTalent += (pl.plGame.getAge(MvcApplication.ckcore.dtDatum) - 10f) * pl.getTalentAve();
      }

      return fAgeTalent / club.ltPlayer.Count;
    }

    [HttpPost]
    public static async Task<bool> uploadFileAsync(HttpPostedFileBase file, string sFolder, int iFileId)
    {
#if DEBUG
      //return false;
#endif

      try {
        if (file != null && file.ContentLength > 0) {
          string sFileExt = Path.GetExtension(file.FileName);
          //string sFileExt = ".png";

          // Get base directory
          string sBaseDir = MvcApplication.ckcore.settings.sHomeDir;
          if (string.IsNullOrEmpty(sBaseDir)) sBaseDir = MvcApplication.getHomeDir();
#if !DEBUG
          sBaseDir = System.IO.Directory.GetParent(sBaseDir).FullName;
#endif

          string sFileParentDir = Path.Combine(sBaseDir, "Content", "Uploads", sFolder);

          // Create directory if not existing
          if (!Directory.Exists(sFileParentDir)) Directory.CreateDirectory(sFileParentDir);

          string sFilenameLocal = Path.Combine(sFileParentDir, iFileId.ToString() + sFileExt);
          MvcApplication.ckcore.tl.writeLog("Save file to '" + sFilenameLocal + "'");
          file.SaveAs(sFilenameLocal);

          if (!Path.GetExtension(file.FileName).Equals(".png")) {
            System.Drawing.Image img = System.Drawing.Image.FromFile(sFilenameLocal);
            img.Save(Path.Combine(sFileParentDir, iFileId.ToString() + ".png"), System.Drawing.Imaging.ImageFormat.Png);
          }

          AmazonS3FileTransfer as3 = new AmazonS3FileTransfer();
          as3.uploadFile(sFilenameLocal, sFolder + "/" + iFileId.ToString() + ".png", "image/custom");
        }

        return true;
      } catch {
        return false;
      }
    }

    public static async Task<bool> deleteFileAsync(string sKey)
    {
      try {
        AmazonS3FileTransfer as3 = new AmazonS3FileTransfer();
        as3.deleteFile(sKey);

        return true;
      } catch {
        return false;
      }
    }

    public ApplicationSignInManager SignInManager {
      get {
        return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
      }
      private set {
        _signInManager = value;
      }
    }

    public ApplicationUserManager UserManager {
      get {
        return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
      }
      private set {
        _userManager = value;
      }
    }

    //
    // GET: /Account/Login
    [AllowAnonymous]
    public ActionResult Login(string returnUrl)
    {
      ViewBag.ReturnUrl = returnUrl;
      return View();
    }

    //
    // POST: /Account/Login
    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult> Login(LoginViewModel model, string returnUrl)
    {
      MvcApplication.ckcore.tl.writeLog("Login of user: " + model.Email);

      if (!ModelState.IsValid) return View(model);

      if (!MvcApplication.settings.bLoginPossible) return View(model);

      if (MvcApplication.settings.bEmailCertification) {
        // Require the user to have a confirmed email before they can log on.
        var user = await UserManager.FindByNameAsync(model.Email);
        if (user != null) {
          if (!await UserManager.IsEmailConfirmedAsync(user.Id)) {
            ViewBag.errorMessage = "Um dich einloggen zu können, musst Du deine e-mail bestätigen! Bitte überprüfe deinen e-mail Account.";
            return View("Error");
          }
        }
      }

      // Anmeldefehler werden bezüglich einer Kontosperre nicht gezählt.
      // Wenn Sie aktivieren möchten, dass Kennwortfehler eine Sperre auslösen, ändern Sie in "shouldLockout: true".
      var result = await SignInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, shouldLockout: false);
      MvcApplication.ckcore.tl.writeLog("  Login result: " + result.ToString());

      switch (result) {
        case SignInStatus.Success:
          //appUser = new ApplicationUser { UserName = model.Email, Email = model.Email };
          ApplicationUser appUser = await UserManager.FindByNameAsync(model.Email);
          if (appUser == null/* || !(await UserManager.IsEmailConfirmedAsync(appUser.Id))*/) {
            // Don't reveal that the user does not exist or is not confirmed

            ModelState.AddModelError("", "Ungültiger Anmeldeversuch.");

            return View("ForgotPasswordConfirmation");
          }

          iniCk();

          string sNoLoginMailList = ConfigurationManager.AppSettings["ckNoLoginMail"];
          bool bNoLoginMail = false;

#if !DEBUG
          if (!string.IsNullOrEmpty(sNoLoginMailList)) {
            foreach (string sNoMail in sNoLoginMailList.Split(';')) {
              if (sNoMail.Equals(model.Email, StringComparison.CurrentCultureIgnoreCase)) {
                bNoLoginMail = true;
                break;
              }
            }
          }

          // Send mail to admin
          if (!bNoLoginMail) {
            try {
              await UserManager.SendEmailAsync(MvcApplication.ckcore.ltUser[0].id, "User login: " + model.Email, model.Email + " has logged in");
            } catch {
            }
          }
#endif

          return RedirectToAction("Desk", "Member");
        case SignInStatus.LockedOut:
          return View("Lockout");
        case SignInStatus.RequiresVerification:
          return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = model.RememberMe });
        case SignInStatus.Failure:
        default:
          ModelState.AddModelError("", "Ungültiger Anmeldeversuch.");
          return View(model);
      }
    }

    //
    // GET: /Account/VerifyCode
    [AllowAnonymous]
    public async Task<ActionResult> VerifyCode(string provider, string returnUrl, bool rememberMe)
    {
      // Verlangen, dass sich der Benutzer bereits mit seinem Benutzernamen/Kennwort oder einer externen Anmeldung angemeldet hat.
      if (!await SignInManager.HasBeenVerifiedAsync()) return View("Error");

      return View(new VerifyCodeViewModel { Provider = provider, ReturnUrl = returnUrl, RememberMe = rememberMe });
    }

    //
    // POST: /Account/VerifyCode
    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult> VerifyCode(VerifyCodeViewModel model)
    {
      if (!ModelState.IsValid) return View(model);

      // Der folgende Code schützt vor Brute-Force-Angriffen der zweistufigen Codes. 
      // Wenn ein Benutzer in einem angegebenen Zeitraum falsche Codes eingibt, wird das Benutzerkonto 
      // für einen bestimmten Zeitraum gesperrt. 
      // Sie können die Einstellungen für Kontosperren in "IdentityConfig" konfigurieren.
      var result = await SignInManager.TwoFactorSignInAsync(model.Provider, model.Code, isPersistent:  model.RememberMe, rememberBrowser: model.RememberBrowser);
      switch (result)
      {
        case SignInStatus.Success:
          return RedirectToLocal(model.ReturnUrl);
        case SignInStatus.LockedOut:
          return View("Lockout");
        case SignInStatus.Failure:
        default:
          ModelState.AddModelError("", "Ungültiger Code.");
          return View(model);
      }
    }

    [HttpGet]
    [AllowAnonymous]
    public JsonResult LoginGetStatus()
    {
      if (MvcApplication.settings.bMaintenance) {
        return Json(4, JsonRequestBehavior.AllowGet);
      } else if (!MvcApplication.settings.bLoginPossible) {
        return Json(3, JsonRequestBehavior.AllowGet);
      } else if (MvcApplication.iLoadState == 1) {
        return Json(1, JsonRequestBehavior.AllowGet);
      } else if (MvcApplication.iLoadState == 2) {
        return Json(2, JsonRequestBehavior.AllowGet);
      }

      return Json(0, JsonRequestBehavior.AllowGet);
    }

    //
    // GET: /Account/Register
    [HttpGet]
    [AllowAnonymous]
    public ActionResult Register()
    {
      RegisterViewModel mdRegister = new RegisterViewModel();

      mdRegister.ddlDivision = new List<SelectListItem>();

      /*
      foreach (CornerkickManager.Cup league in MvcApplication.ckcore.ltCups) {
        if (league.iId == 1 && league.iId2 == ) continue;

        if (clb.user == null) return Json(true, JsonRequestBehavior.AllowGet);
      }
      */
      mdRegister.ddlDivision.Add(new SelectListItem { Text = "Liga 2", Value = "1", Selected = true });
      mdRegister.Liga = 1;

      // Add clubs to ddl in register view (need to be relocated if multiple leagues are available)
      mdRegister.ltClubs = new List<SelectListItem>();

      for (int iC = 1; iC < MvcApplication.ckcore.ltClubs.Count; iC++) {
        CornerkickManager.Club clb = MvcApplication.ckcore.ltClubs[iC];

        if (clb.bNation) continue;
        if (clb.user != null) continue;

        if (clb.iLand != MvcApplication.iNations[0]) continue; // Remove if all leagues are available

        mdRegister.ltClubs.Add(new SelectListItem { Text = clb.sName, Value = iC.ToString() });
      }

      mdRegister.bRegisterPossible = checkRegisterPossible();

      // Set jersey colors
      mdRegister.clH1 = System.Drawing.Color.White;
      mdRegister.clH2 = System.Drawing.Color.Blue;
      mdRegister.clH3 = System.Drawing.Color.Black;
      mdRegister.clA1 = System.Drawing.Color.White;
      mdRegister.clA2 = System.Drawing.Color.Red;
      mdRegister.clA3 = System.Drawing.Color.Black;

      return View(mdRegister);
    }

    private bool checkRegisterPossible()
    {
      if (MvcApplication.ckcore.dtDatum.Date.Equals(MvcApplication.ckcore.dtSeasonStart.Date) ||
          MvcApplication.ckcore.dtSeasonStart.Year < 1000 ||
          MvcApplication.settings.bRegisterDuringGame) {
        return true;
      }

      return false;
    }

    //
    // POST: /Account/Register
    private RegisterViewModel rvmDEBUG;
    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult> Register(RegisterViewModel model)
    {
      model.bRegisterPossible = checkRegisterPossible();

      // Check emblem
      if (model.fileEmblem != null) {
        List<string> sImageExtensions = new List<string> { ".JPG", ".JPE", ".BMP", ".GIF", ".PNG" };
        if (!sImageExtensions.Contains(Path.GetExtension(model.fileEmblem.FileName).ToUpperInvariant())) {
          ModelState.AddModelError("", "Nur Bilddateien möglich!");
          return View(model);
        }
      }

      DebugInfo("Register");

      bool bAdminFirst = false;
#if !DEBUG
      // first user must be admin
      if (MvcApplication.ckcore.ltUser.Count == 0) {
        if (!checkUserIsAdmin(model)) return View(model);
      }

      // Set admin details
      if (MvcApplication.ckcore.ltUser.Count == 0) {
        model.Vorname = "Admin";
        model.Nachname = "Admin";
        model.Verein = "Computer";
      }

      bAdminFirst = true;
#endif

      MvcApplication.ckcore.tl.writeLog("Register new user: " + model.Vorname + " " + model.Nachname + ", Team: " + model.Verein);

      if (ModelState.IsValid) {
        //appUser = new ApplicationUser { UserName = model.Vorname + " " + model.Nachname, Email = model.Email, Vorname = model.Vorname, Nachname = model.Nachname, Vereinsname = model.Verein };
        ApplicationUser appUser = new ApplicationUser { UserName = model.Email, Email = model.Email, Vorname = model.Vorname, Nachname = model.Nachname, Vereinsname = model.Verein };

        var result = await UserManager.CreateAsync(appUser, model.Password);
        MvcApplication.ckcore.tl.writeLog("  Register succeeded: " + result.Succeeded.ToString());

        if (result.Succeeded) {
          rvmDEBUG = model;

          if (MvcApplication.ckcore.ltUser.Count == 0/* && bAdminFirst && MvcApplication.ckcore.ltClubs.Count == 0*/) { // admin user
            await SignInManager.SignInAsync(appUser, isPersistent:false, rememberBrowser:false);

            CornerkickManager.User usrAdmin = createUser(appUser);
            MvcApplication.ckcore.ltUser.Add(usrAdmin);

            // Initialize dummy club
            CornerkickManager.Club club0 = new CornerkickManager.Club();
            club0.iId = 0;
            club0.sName = "Computer";

            club0.staff.iJouthScouting = 10;

            for (int iPl = 0; iPl < 100; iPl++) {
              CornerkickManager.Player sp = MvcApplication.ckcore.plt.newPlayer(club0);
              MvcApplication.ckcore.tr.putPlayerOnTransferlist(sp.plGame.iId, 0);
            }

            MvcApplication.ckcore.ltClubs.Add(club0);
            // END Initialize dummy club
          } else { // no admin
            ApplicationUserPlus appUserPlus = new ApplicationUserPlus(appUser, rvm: model);

            if (MvcApplication.settings.bEmailCertification) {
              sendActivationLinkAsync(appUserPlus);

              // Uncomment to debug locally
              // TempData["ViewBagLink"] = callbackUrl;

              ViewBag.Message = MvcHtmlString.Create("In den nächsten Minuten solltest du eine e-mail bekommen. Bitte überprüfe deine e-mails um dein CORNERKICK-MANAGER Konto zu bestätigen!<br/><br/>Bekommst du innerhalb der nächsten 30 Minuten keine mail, schreibe bitte an <a href=\"mailto:mail@cornerkick-manager.de\">mail@cornerkick-manager.de</a>.");
            } else {
              await SignInManager.SignInAsync(appUser, isPersistent: false, rememberBrowser: false);

              // Create club
              addUserToCk(appUserPlus, false);
              iniCk();
            }

#if !DEBUG
            // Send mail to admin
            try {
              await UserManager.SendEmailAsync(MvcApplication.ckcore.ltUser[0].id, "New user: " + model.Email, model.Email + " has registered." + Environment.NewLine + "Land: " + model.Land.ToString());
            } catch {
            }
#endif
          }

          // Save
          //Task<bool> tkSave = Task.Run(async () => await MvcApplication.saveAsync(MvcApplication.timerCkCalender));

          if (MvcApplication.settings.bEmailCertification) {
            return View("Info");
          }

          return RedirectToAction("Desk", "Member");
        }

        AddErrors(result);
      }

      // Wurde dieser Punkt erreicht, ist ein Fehler aufgetreten; Formular erneut anzeigen.
      return View(model);
    }

    private async Task<bool> sendActivationLinkAsync(ApplicationUserPlus appUserPlus)
    {
      // Weitere Informationen zum Aktivieren der Kontobestätigung und Kennwortzurücksetzung finden Sie unter https://go.microsoft.com/fwlink/?LinkID=320771
      // E-Mail-Nachricht mit diesem Link senden
      string code = await UserManager.GenerateEmailConfirmationTokenAsync(appUserPlus.appUser.Id);
      appUserPlus.sCallbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = appUserPlus.appUser.Id, code = code }, protocol: Request.Url.Scheme);

      // Write log
      MvcApplication.ckcore.tl.writeLog("E-mail confirmation callbackUrl for user '" + appUserPlus.appUser.Id + "': " + appUserPlus.sCallbackUrl);

      // Send email
      UserManager.SendEmailAsync(appUserPlus.appUser.Id, "Konto bestätigen", "Bitte bestätige Dein Cornerkick-Manager Konto. Klicke dazu <a href=\"" + appUserPlus.sCallbackUrl + "\">hier</a><br/><br/><br/>Sollte der link nicht funktioniert, kopiere den folgenden Text und füge ihn in die Adresszeile deines Browsers ein:<br/><br/>" + appUserPlus.sCallbackUrl);

      // Store model in list and save to file
      if (ltRegisterAppUser == null) ltRegisterAppUser = new List<ApplicationUserPlus>();
      bool bExist = false;
      foreach (ApplicationUserPlus aup in ltRegisterAppUser) {
        if (aup.appUser.Email.Equals(appUserPlus.appUser.Email)) {
          bExist = true;
          break;
        }
      }
      if (!bExist) {
        ltRegisterAppUser.Add(appUserPlus);
        saveRegisterUser();
      }

      return true;
    }

    //
    // GET: /Account/Register
    [HttpGet]
    [AllowAnonymous]
    public ActionResult ResentActivationLink()
    {
      RegisterViewModel mdRegister = new RegisterViewModel();

      return View(mdRegister);
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult> ResentActivationLink(RegisterViewModel model)
    {
      ApplicationUser appUser = await UserManager.FindByNameAsync(model.Email);
      if (appUser != null) {
        ApplicationUserPlus appUserPlus = new ApplicationUserPlus(appUser, rvm: model);

        sendActivationLinkAsync(appUserPlus);

        ViewBag.Message = "In den nächsten Minuten solltest Du eine e-mail bekommen. Bitte überprüfe Deine e-mails um Dein CORNERKICK-MANAGER Konto zu bestätigen!";

        return View("Info");
      }

      ViewBag.Message = "Fehler: Die angegebene e-mail konnte nicht gefunden werden. Bitte versuchen Sie es erneut.";

      return View("Info");
    }

    [AllowAnonymous]
    [HttpGet]
    public ActionResult RegisterCheckLeague(int iLand, int iDivision)
    {
      CornerkickManager.Cup league = MvcApplication.ckcore.tl.getCup(1, iLand, iDivision);

      int[] iUser = new int[2];
      foreach (CornerkickManager.Club clb in league.ltClubs[0]) {
        if (clb == null) continue;

        if (clb.user == null) iUser[1]++;
        iUser[0]++;
      }

      return Json(iUser, JsonRequestBehavior.AllowGet);
    }

    [AllowAnonymous]
    [HttpPost]
    public ActionResult CheckExistingTeamName(string Verein)
    {
      foreach (CornerkickManager.Club clubExist in MvcApplication.ckcore.ltClubs) {
        if (clubExist.sName.Equals(Verein)) return Json(false, JsonRequestBehavior.AllowGet);
      }
      return Json(true, JsonRequestBehavior.AllowGet);
    }

    private string DebugInfo(string sText)
    {
      return HttpUtility.HtmlEncode(sText);
    }

    //
    // GET: /Account/ConfirmEmail
    [AllowAnonymous]
    public async Task<ActionResult> ConfirmEmail(string userId, string code)
    {
      if (userId == null || code == null) return View("Error");

      // Read register user from file
      if (ltRegisterAppUser == null || ltRegisterAppUser.Count == 0) {
        string sFileRegisterUser = Path.Combine(MvcApplication.getHomeDir(), sFilenameRegisterUser);
#if _USE_AMAZON_S3
        AmazonS3FileTransfer as3 = new AmazonS3FileTransfer();
        if (!System.IO.File.Exists(sFileRegisterUser)) as3.downloadFile("registerUser", sFilenameRegisterUser);
#endif

        if (System.IO.File.Exists(sFileRegisterUser)) {
          ltRegisterAppUser = new List<ApplicationUserPlus>();
          string lineRegUsr;
          System.IO.StreamReader fileRegUsr = new System.IO.StreamReader(@sFileRegisterUser);

          while ((lineRegUsr = fileRegUsr.ReadLine()) != null) {
            string[] sLineRegUsrSplit = lineRegUsr.Split(';');
            if (sLineRegUsrSplit.Length < 11) continue;

            //ApplicationUser au = new ApplicationUser();
            ApplicationUser au = await UserManager.FindByNameAsync(sLineRegUsrSplit[0]);
            if (au == null) continue;

            /*
            au.Email = sLineRegUsrSplit[0];
            au.Vereinsname = sLineRegUsrSplit[1];
            au.Vorname = sLineRegUsrSplit[2];
            au.Nachname = sLineRegUsrSplit[3];
            */

            ApplicationUserPlus aup = new ApplicationUserPlus(au);

            int iLand = 0;
            int.TryParse(sLineRegUsrSplit[4], out iLand);
            aup.iLand = iLand;

            int iLiga = 0;
            int.TryParse(sLineRegUsrSplit[5], out iLiga);
            aup.iLiga = iLiga;

            byte[] iClH1 = new byte[3];
            string[] sLineRegUsrClH1Split = sLineRegUsrSplit[6].Split('/');
            if (sLineRegUsrClH1Split.Length > 2) {
              for (byte iCl = 0; iCl < 3; iCl++) byte.TryParse(sLineRegUsrClH1Split[iCl], out iClH1[iCl]);
            }
            if (iClH1[0] + iClH1[1] + iClH1[2] > 0) aup.clH1 = System.Drawing.Color.FromArgb(iClH1[0], iClH1[1], iClH1[2]);

            byte[] iClH2 = new byte[3];
            string[] sLineRegUsrClH2Split = sLineRegUsrSplit[7].Split('/');
            if (sLineRegUsrClH2Split.Length > 2) {
              for (byte iCl = 0; iCl < 3; iCl++) byte.TryParse(sLineRegUsrClH2Split[iCl], out iClH2[iCl]);
            }
            if (iClH2[0] + iClH2[1] + iClH2[2] > 0) aup.clH2 = System.Drawing.Color.FromArgb(iClH2[0], iClH2[1], iClH2[2]);

            byte[] iClH3 = new byte[3];
            string[] sLineRegUsrClH3Split = sLineRegUsrSplit[8].Split('/');
            if (sLineRegUsrClH3Split.Length > 2) {
              for (byte iCl = 0; iCl < 3; iCl++) byte.TryParse(sLineRegUsrClH3Split[iCl], out iClH3[iCl]);
            }
            if (iClH3[0] + iClH3[1] + iClH3[2] > 0) aup.clH3 = System.Drawing.Color.FromArgb(iClH3[0], iClH3[1], iClH3[2]);

            byte[] iClA1 = new byte[3];
            string[] sLineRegUsrClA1Split = sLineRegUsrSplit[9].Split('/');
            if (sLineRegUsrClA1Split.Length > 2) {
              for (byte iCl = 0; iCl < 3; iCl++) byte.TryParse(sLineRegUsrClA1Split[iCl], out iClA1[iCl]);
            }
            if (iClA1[0] + iClA1[1] + iClA1[2] > 0) aup.clA1 = System.Drawing.Color.FromArgb(iClA1[0], iClA1[1], iClA1[2]);

            byte[] iClA2 = new byte[3];
            string[] sLineRegUsrClA2Split = sLineRegUsrSplit[10].Split('/');
            if (sLineRegUsrClA2Split.Length > 2) {
              for (byte iCl = 0; iCl < 3; iCl++) byte.TryParse(sLineRegUsrClA2Split[iCl], out iClA2[iCl]);
            }
            if (iClA2[0] + iClA2[1] + iClA2[2] > 0) aup.clA2 = System.Drawing.Color.FromArgb(iClA2[0], iClA2[1], iClA2[2]);

            byte[] iClA3 = new byte[3];
            string[] sLineRegUsrClA3Split = sLineRegUsrSplit[11].Split('/');
            if (sLineRegUsrClA3Split.Length > 2) {
              for (byte iCl = 0; iCl < 3; iCl++) byte.TryParse(sLineRegUsrClA3Split[iCl], out iClA3[iCl]);
            }
            if (iClA3[0] + iClA3[1] + iClA3[2] > 0) aup.clA3 = System.Drawing.Color.FromArgb(iClA3[0], iClA3[1], iClA3[2]);

            ltRegisterAppUser.Add(aup);
          }

          fileRegUsr.Close();
        }
      }

      var result = await UserManager.ConfirmEmailAsync(userId, code);

      if (result.Succeeded) {
        // Create club
        ApplicationUser appUser = UserManager.FindById(userId);

        foreach (ApplicationUserPlus aup in ltRegisterAppUser) {
          if (aup == null) continue;
          if (aup.appUser == null) continue;

          if (appUser.Email.Equals(aup.appUser.Email)) {
            addUserToCk(aup, bAdmin: false);
            iniCk();

            ltRegisterAppUser.Remove(aup);
            saveRegisterUser();

#if !DEBUG
            // Send mail to admin
            try {
              await UserManager.SendEmailAsync(MvcApplication.ckcore.ltUser[0].id, "New user has confirmed email: " + appUser.Email, appUser.Email + " has confirmed email.");
            } catch {
            }
#endif

            return View("ConfirmEmail");
          }
        }
      }

      return View("Error");
    }

    //
    // GET: /Account/ForgotPassword
    [AllowAnonymous]
    public ActionResult ForgotPassword()
    {
      return View();
    }

    //
    // POST: /Account/ForgotPassword
    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult> ForgotPassword(ForgotPasswordViewModel model)
    {
      if (ModelState.IsValid) {
        var usr = await UserManager.FindByNameAsync(model.Email);
        if (usr == null || !(await UserManager.IsEmailConfirmedAsync(usr.Id))) {
          // Nicht anzeigen, dass der Benutzer nicht vorhanden ist oder nicht bestätigt wurde.
          return View("ForgotPasswordConfirmation");
        }

        // Weitere Informationen zum Aktivieren der Kontobestätigung und Kennwortzurücksetzung finden Sie unter https://go.microsoft.com/fwlink/?LinkID=320771
        // E-Mail-Nachricht mit diesem Link senden
        string code = await UserManager.GeneratePasswordResetTokenAsync(usr.Id);
        var callbackUrl = Url.Action("ResetPassword", "Account", new { userId = usr.Id, code = code }, protocol: Request.Url.Scheme);		
        await UserManager.SendEmailAsync(usr.Id, "Kennwort zurücksetzen", "Bitte setzen Sie Ihr Kennwort zurück. Klicken Sie dazu <a href=\"" + callbackUrl + "\">hier</a>");
        return RedirectToAction("ForgotPasswordConfirmation", "Account");
      }

      // Wurde dieser Punkt erreicht, ist ein Fehler aufgetreten; Formular erneut anzeigen.
      return View(model);
    }

    //
    // GET: /Account/ForgotPasswordConfirmation
    [AllowAnonymous]
    public ActionResult ForgotPasswordConfirmation()
    {
      return View();
    }

    //
    // GET: /Account/ResetPassword
    [AllowAnonymous]
    public ActionResult ResetPassword(string code)
    {
      return code == null ? View("Error") : View();
    }

    //
    // POST: /Account/ResetPassword
    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult> ResetPassword(ResetPasswordViewModel model)
    {
      if (!ModelState.IsValid) {
        return View(model);
      }
      var usr = await UserManager.FindByNameAsync(model.Email);
      if (usr == null) {
        // Nicht anzeigen, dass der Benutzer nicht vorhanden ist.
        return RedirectToAction("ResetPasswordConfirmation", "Account");
      }
      var result = await UserManager.ResetPasswordAsync(usr.Id, model.Code, model.Password);
      if (result.Succeeded) {
        return RedirectToAction("ResetPasswordConfirmation", "Account");
      }
      AddErrors(result);
      return View();
    }

    //
    // GET: /Account/ResetPasswordConfirmation
    [AllowAnonymous]
    public ActionResult ResetPasswordConfirmation()
    {
      return View();
    }

    //
    // POST: /Account/ExternalLogin
    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public ActionResult ExternalLogin(string provider, string returnUrl)
    {
      // Umleitung an den externen Anmeldeanbieter anfordern
      return new ChallengeResult(provider, Url.Action("ExternalLoginCallback", "Account", new { ReturnUrl = returnUrl }));
    }

    //
    // GET: /Account/SendCode
    [AllowAnonymous]
    public async Task<ActionResult> SendCode(string returnUrl, bool rememberMe)
    {
      var userId = await SignInManager.GetVerifiedUserIdAsync();
      if (userId == null) {
        return View("Error");
      }
      var userFactors = await UserManager.GetValidTwoFactorProvidersAsync(userId);
      var factorOptions = userFactors.Select(purpose => new SelectListItem { Text = purpose, Value = purpose }).ToList();
      return View(new SendCodeViewModel { Providers = factorOptions, ReturnUrl = returnUrl, RememberMe = rememberMe });
    }

    //
    // POST: /Account/SendCode
    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult> SendCode(SendCodeViewModel model)
    {
      if (!ModelState.IsValid) {
        return View();
      }

      // Token generieren und senden
      if (!await SignInManager.SendTwoFactorCodeAsync(model.SelectedProvider)) {
        return View("Error");
      }
      return RedirectToAction("VerifyCode", new { Provider = model.SelectedProvider, ReturnUrl = model.ReturnUrl, RememberMe = model.RememberMe });
    }

    //
    // GET: /Account/ExternalLoginCallback
    [AllowAnonymous]
    public async Task<ActionResult> ExternalLoginCallback(string returnUrl)
    {
      var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync();
      if (loginInfo == null) {
        return RedirectToAction("Login");
      }

      // Benutzer mit diesem externen Anmeldeanbieter anmelden, wenn der Benutzer bereits eine Anmeldung besitzt
      var result = await SignInManager.ExternalSignInAsync(loginInfo, isPersistent: false);
      switch (result) {
        case SignInStatus.Success:
          //return RedirectToLocal(returnUrl);
          return RedirectToAction("Login");
        case SignInStatus.LockedOut:
          return View("Lockout");
        case SignInStatus.RequiresVerification:
          return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = false });
        case SignInStatus.Failure:
        default:
          // Benutzer auffordern, ein Konto zu erstellen, wenn er kein Konto besitzt
          ViewBag.ReturnUrl = returnUrl;
          ViewBag.LoginProvider = loginInfo.Login.LoginProvider;
          return View("ExternalLoginConfirmation", new ExternalLoginConfirmationViewModel { Email = loginInfo.Email });
      }
    }

    //
    // POST: /Account/ExternalLoginConfirmation
    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult> ExternalLoginConfirmation(ExternalLoginConfirmationViewModel model, string returnUrl)
    {
      if (User.Identity.IsAuthenticated) {
        return RedirectToAction("Index", "Manage");
      }

      if (ModelState.IsValid) {
        // Informationen zum Benutzer aus dem externen Anmeldeanbieter abrufen
        var info = await AuthenticationManager.GetExternalLoginInfoAsync();
        if (info == null) {
          return View("ExternalLoginFailure");
        }
        var usr = new ApplicationUser { UserName = model.Email, Email = model.Email };
        var result = await UserManager.CreateAsync(usr);
        if (result.Succeeded) {
          result = await UserManager.AddLoginAsync(usr.Id, info.Login);
          if (result.Succeeded) {
            await SignInManager.SignInAsync(usr, isPersistent: false, rememberBrowser: false);
            return RedirectToLocal(returnUrl);
          }
        }
        AddErrors(result);
      }

      ViewBag.ReturnUrl = returnUrl;
      return View(model);
    }

    //
    // POST: /Account/LogOff
    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult LogOff()
    {
      AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
      return RedirectToAction("Index", "Home");
    }

    //
    // GET: /Account/ExternalLoginFailure
    [AllowAnonymous]
    public ActionResult ExternalLoginFailure()
    {
      return View();
    }

    //
    // GET: /Account/Login
    [AllowAnonymous]
    public ActionResult RemoveUser()
    {
      return View();
    }

    //
    // POST: /Account/Register
    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult> RemoveUser(RemoveUserViewModel mdRemoveUser)
    {
      string sUserName = User.Identity.GetUserName();
      string sUserId   = User.Identity.GetUserId();

      DebugInfo("Remove User");
      MvcApplication.ckcore.tl.writeLog("Remove user: " + sUserId + " (mail: " + sUserName + ")");

      if (ModelState.IsValid) {
        // Require the user to have a confirmed email before they can log on.
        var user = await UserManager.FindByNameAsync(sUserName);

        if (user == null) {
          AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
          return RedirectToAction("Index", "Home");
        }

        if (MvcApplication.settings.bEmailCertification && !await UserManager.IsEmailConfirmedAsync(user.Id)) {
          //ViewBag.errorMessage = "Um dich einloggen zu können, musst Du deine e-mail bestätigen! Bitte überprüfe deinen e-mail Account.";
          return View("Error");
        }

        var result = await UserManager.DeleteAsync(user);

        if (result.Succeeded) {
          CornerkickManager.User usr = ckUser();

          if (usr.club != null) {
            // Set CPU name to club
            string sNameNew = "";
            int iC = 0;
            while (iC < 10000) {
              iC++;
              sNameNew = "Team_" + CornerkickManager.Main.sLand[usr.club.iLand] + "_" + iC.ToString();

              bool bFound = true;
              foreach (CornerkickManager.Club clbExist in MvcApplication.ckcore.ltClubs) {
                if (clbExist.sName.Equals(sNameNew)) {
                  bFound = false;
                  break;
                }
              }

              if (bFound) break;
            }
            if (!string.IsNullOrEmpty(sNameNew)) usr.club.sName = sNameNew;

            // Clear user
            usr.club.user = null;

            // Delete emblem
            string sBaseDir = MvcApplication.ckcore.settings.sHomeDir;
            if (string.IsNullOrEmpty(sBaseDir)) sBaseDir = MvcApplication.getHomeDir();
#if !DEBUG
            sBaseDir = System.IO.Directory.GetParent(sBaseDir).FullName;
#endif

            foreach (string sFileExt in new string[3] { ".png", ".jpg", ".gif" }) {
              string sFilenameLocal = Path.Combine(sBaseDir, "Content", "Uploads", "emblems", usr.club.iId.ToString() + sFileExt);
              try {
                System.IO.File.Delete(sFilenameLocal);
              } catch {
              }

              // Remove emblem from aws
              Task<bool> tkDeleteEmblem = Task.Run(async () => await deleteFileAsync("emblems/" + usr.club.iId + sFileExt));
            }
          }

          if (usr.nation != null) usr.nation.user = null;
          MvcApplication.ckcore.ltUser.Remove(usr);

          MvcApplication.ckcore.tl.writeLog("  Removing user succeeded: " + result.Succeeded.ToString());

          // Sign out
          AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);

          // Send mail to admin
          await UserManager.SendEmailAsync(MvcApplication.ckcore.ltUser[0].id, "User retired: " + sUserName, sUserName + " has retired");

          return RedirectToAction("Index", "Home");
        }
      }

      return View();
    }

    protected override void Dispose(bool disposing)
    {
      if (disposing) {
        if (_userManager != null) {
          _userManager.Dispose();
          _userManager = null;
        }

        if (_signInManager != null) {
          _signInManager.Dispose();
          _signInManager = null;
        }
      }

      base.Dispose(disposing);
    }

    private void saveRegisterUser()
    {
      string sFileRegisterUser = Path.Combine(MvcApplication.getHomeDir(), sFilenameRegisterUser);

      using (System.IO.StreamWriter fileSettings = new System.IO.StreamWriter(sFileRegisterUser)) {
        foreach (ApplicationUserPlus au in ltRegisterAppUser) {
          if (au == null) continue;
          if (au.appUser == null) continue;

          fileSettings.Write(au.appUser.Email + ";");
          fileSettings.Write(au.appUser.Vereinsname + ";");
          fileSettings.Write(au.appUser.Vorname + ";");
          fileSettings.Write(au.appUser.Nachname + ";");
          fileSettings.Write(au.iLand.ToString() + ";");
          fileSettings.Write(au.iLiga.ToString() + ";");
          fileSettings.Write(au.clH1.R.ToString() + "/" + au.clH1.G.ToString() + "/" + au.clH1.B.ToString() + ";");
          fileSettings.Write(au.clH2.R.ToString() + "/" + au.clH2.G.ToString() + "/" + au.clH2.B.ToString() + ";");
          fileSettings.Write(au.clH3.R.ToString() + "/" + au.clH3.G.ToString() + "/" + au.clH3.B.ToString() + ";");
          fileSettings.Write(au.clA1.R.ToString() + "/" + au.clA1.G.ToString() + "/" + au.clA1.B.ToString() + ";");
          fileSettings.Write(au.clA2.R.ToString() + "/" + au.clA2.G.ToString() + "/" + au.clA2.B.ToString() + ";");
          fileSettings.Write(au.clA3.R.ToString() + "/" + au.clA3.G.ToString() + "/" + au.clA3.B.ToString() + ";");
          fileSettings.Write(au.appUser.Id + ";" + au.sCallbackUrl);
          fileSettings.WriteLine();
        }

        fileSettings.Close();
      }

#if _USE_AMAZON_S3
      AmazonS3FileTransfer as3 = new AmazonS3FileTransfer();
      as3.uploadFile(sFileRegisterUser, "registerUser");
#endif
    }

    #region Hilfsprogramme
    // Wird für XSRF-Schutz beim Hinzufügen externer Anmeldungen verwendet
    private const string XsrfKey = "XsrfId";

      private IAuthenticationManager AuthenticationManager
      {
          get
          {
              return HttpContext.GetOwinContext().Authentication;
          }
      }

      private void AddErrors(IdentityResult result)
      {
          foreach (var error in result.Errors)
          {
              ModelState.AddModelError("", error);
          }
      }

      private ActionResult RedirectToLocal(string returnUrl)
      {
          if (Url.IsLocalUrl(returnUrl))
          {
              return Redirect(returnUrl);
          }
          return RedirectToAction("Index", "Home");
      }

      internal class ChallengeResult : HttpUnauthorizedResult
      {
          public ChallengeResult(string provider, string redirectUri)
              : this(provider, redirectUri, null)
          {
          }

          public ChallengeResult(string provider, string redirectUri, string userId)
          {
              LoginProvider = provider;
              RedirectUri = redirectUri;
              UserId = userId;
          }

          public string LoginProvider { get; set; }
          public string RedirectUri { get; set; }
          public string UserId { get; set; }

          public override void ExecuteResult(ControllerContext context)
          {
              var properties = new AuthenticationProperties { RedirectUri = RedirectUri };
              if (UserId != null)
              {
                  properties.Dictionary[XsrfKey] = UserId;
              }
              context.HttpContext.GetOwinContext().Authentication.Challenge(properties, LoginProvider);
          }
      }
#endregion
  }
}