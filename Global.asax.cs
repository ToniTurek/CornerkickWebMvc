﻿/*
 List of available macros (to be defined in project)
   _WebMvc
   _NO_UPLOAD
   _USE_AMAZON_S3
   _DEPLOY_ON_APPHB
   _DOCKER
*/

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using System.Web;
using System.Web.Mvc;
#if _WebMvc
using System.Web.Optimization;
using System.Web.Routing;
using System.Web.UI.WebControls;
#endif
using Microsoft.AspNet.Identity.Owin;

#if _USE_BLOB
using Microsoft.Azure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
#endif

namespace CornerkickWebMvc
{
#if _WebMvc
  public class MvcApplication : System.Web.HttpApplication
#else
  public class MvcApplication
#endif
  {
    public static CornerkickManager.Main ckcore;
    public static System.Timers.Timer timerCkCalender = null;
    public static System.Timers.Timer timerSave = null;
    public static List<string> ltLog = new List<string>();
    private static Random random = new Random();
    public static Settings settings = new Settings();
    public const string sVersion = "4.4.4";
    public static int iLoadState = 1; // 1: Initial value, 2: starting calendar steps, 0: ready for login, 3: error

    //private const double fStartDelay = 500.0; // [ms]
    private const double fLoadDelay = 1000.0; // [ms]

    public const int iCupIdLeague   =  1; // Leagues
    public const int iCupIdNatCup   =  2; // National cups
    public const int iCupIdGold     =  3; // Gold cup
    public const int iCupIdSilver   =  4; // Silver cup
    public const int iCupIdBronze   = 15; // Bronze cup
    public const int iCupIdTestgame =  5; // Test games
    public const int iCupIdWc       =  7; // World cup

    public class Settings
    {
      public int  iStartHour;
      public bool bEmailCertification;
      public bool bRegisterDuringGame;
      public bool bLoginPossible;
      public bool bMaintenance;

      public Settings()
      {
        iStartHour = -1;
        bLoginPossible = true;
        bEmailCertification = true;
        bRegisterDuringGame = true;
#if DEBUG
        bEmailCertification = false;
#endif
      }
    }

    //const string sSaveZip = "ckSave.zip";
    internal const string sFilenameSave = ".autosave.ckx";
    const string sFilenameSettings = "laststate.txt";

    internal static byte[] iNations = new byte[8] {
      36, // GER
      29, // ENG
      30, // ESP
      45, // ITA
      33, // FRA
      54, // NED
      13, // BRA
       3  // ARG
    }; // [CK Nat.] = sLand Nat.

    static DateTime dtLoadCk = new DateTime(); // The ck DateTime when game was (re-)started

    public static CornerkickManager.Club clubAdmin;

    internal class Mail
    {
      internal string sIdFrom { get; set; }
      internal string sIdTo { get; set; }
      internal bool bNew { get; set; }
      internal DateTime dt { get; set; }
      internal string sText { get; set; }
    }
    internal static List<Mail> ltMail;

    class TimerLoad : System.Timers.Timer
    {
      public string sHomeDir;
    }
    static TimerLoad timerLoad;

    //Timer timerStart;
    protected void Application_Start()
    {
      AreaRegistration.RegisterAllAreas();
#if _WebMvc
      FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
      RouteConfig.RegisterRoutes(RouteTable.Routes);
      BundleConfig.RegisterBundles(BundleTable.Bundles);
#endif

      // Create stopwatch
      System.Diagnostics.Stopwatch swStart = new System.Diagnostics.Stopwatch();

      // Start stopwatch
      swStart.Start();

      try {
        newCk();

        // Stop stopwatch
        swStart.Stop();
        TimeSpan tsStart = swStart.Elapsed;

        // Write elapsed time to log
        if (ckcore != null) ckcore.tl.writeLog("Elapsed time during start: " + tsStart.TotalSeconds.ToString("0.000") + "s");
      } catch {
      }

      /*
      timerStart = new Timer(fStartDelay);
      timerStart.Elapsed += new ElapsedEventHandler(timerStart_Tick);
      timerStart.Start();
      */
    }

    /*
    private void timerStart_Tick(object sender, EventArgs e)
    {
      timerStart.Stop();

      // Create stopwatch
      System.Diagnostics.Stopwatch swStart = new System.Diagnostics.Stopwatch();

      // Start stopwatch
      swStart.Start();

      try {
        newCk();

        // Stop stopwatch
        swStart.Stop();
        TimeSpan tsStart = swStart.Elapsed;

        // Write elapsed time to log
        if (ckcore != null) ckcore.tl.writeLog("Elapsed time during start: " + tsStart.TotalSeconds.ToString("0.000") + "s");
      } catch {
      }
    }
    */

    internal static void newCk(bool bLoadGame = true)
    {
      string sHomeDir = getHomeDir();

      // Create new cornerkick manager instance
      ckcore = new CornerkickManager.Main(sHomeDir: sHomeDir,
                                          bContinuingTime: true,
                                          iTrainingsPerDay: 3,
                                          iTrainingsPerDayMax: 3,
                                          bPlayerTransferOnlyOncePerSeason: true,
                                          iWriteGamesToDisk: 0,
                                          fMoralMin: 0.4f);

      ckcore.tl.writeLog("WebMvc START");

      Models.RegisterViewModel.ltLand.Clear();
      foreach (byte iN in iNations) {
        string sLand = "Land " + iN.ToString();
        if (CornerkickManager.Main.sLand != null && iN < CornerkickManager.Main.sLand.Length) sLand = CornerkickManager.Main.sLand[iN];

        Models.RegisterViewModel.ltLand.Add(new SelectListItem { Text = sLand, Value = iN.ToString(), Selected = iN == 36 });
      }

      /*
      Models.RegisterViewModel.ltSasn.Clear();
      Models.RegisterViewModel.ltSasn.Add(new SelectListItem { Text = "Saison 1", Value = "1", Selected = true });

      Models.RegisterViewModel.ltSpTg.Clear();
      Models.RegisterViewModel.ltSpTg.Add(new SelectListItem { Text = "1", Value = "1", Selected = true });
      */

      ckcore.dtDatum = new DateTime(DateTime.Now.Year, ckcore.dtDatum.Month, ckcore.dtDatum.Day);

      if (timerCkCalender == null) {
        timerCkCalender = new System.Timers.Timer(120000);
        timerCkCalender.Elapsed += new System.Timers.ElapsedEventHandler(timerCkCalender_Elapsed);
      }
      timerCkCalender.Enabled = false;

      if (timerSave == null) {
        timerSave = new System.Timers.Timer(15 * 60 * 1000); // 15 min.
        timerSave.Elapsed += new System.Timers.ElapsedEventHandler(timerSave_Elapsed);
      }
      timerSave.Enabled = false;

      // Load ck game
      if (bLoadGame) {
        iLoadState = 1;

        timerLoad = new TimerLoad {
          Interval = fLoadDelay,
          sHomeDir = sHomeDir
        };

        timerLoad.Elapsed += new ElapsedEventHandler(timerLoad_Tick);
        timerLoad.Start();
      }

#if _WebMvc
#if !DEBUG
      // Login of admin to start database
      string sAdminEmail = ConfigurationManager.AppSettings["ckAdminEmail"];
      if (!string.IsNullOrEmpty(sAdminEmail)) {
        Controllers.AccountController accountController = new Controllers.AccountController();
        Task<SignInStatus> tkLoginAdmin = Task.Run(async () => await accountController.SignInManager.PasswordSignInAsync(sAdminEmail, "test", isPersistent: false, shouldLockout: false));
      }
#endif
#endif
    }

    private static void timerLoad_Tick(object sender, EventArgs e)
    {
      try {
        if (timerLoad != null) timerLoad.Stop();
      } catch {
      }

      string sHomeDir = ((TimerLoad)sender).sHomeDir;

      Task<bool> tkLoadGame = loadAsync(sHomeDir);
    }

    private static void fillLeaguesWithCpuClubs(CornerkickManager.Cup league, CornerkickManager.Cup cup, byte nLeagueSize = 16)
    {
      Controllers.AccountController accountController = new Controllers.AccountController();

      int iC = 0;
      while (league.ltClubs[0].Count < nLeagueSize) {
        CornerkickManager.Club clb = accountController.createClub("Team_" + CornerkickManager.Main.sLand[league.iId2] + "_" + (iC + (league.iId3 * nLeagueSize) + 1).ToString(), (byte)league.iId2, (byte)league.iId3);
        accountController.addPlayerToClub(ref clb, iSkillChange: -1);

        ckcore.ltClubs.Add(clb);

        cup   .ltClubs[0].Add(clb);
        league.ltClubs[0].Add(clb);

        iC++;
      }
    }

    private static void createCupGold()
    {
      CornerkickManager.Cup cupGold = ckcore.tl.getCup(3);

      // Create Gold Cup
      if (cupGold == null) {
        cupGold = new CornerkickManager.Cup(bKo: true, bKoTwoGames: true, nGroups: 8, nQualifierKo: 2);
        ckcore.ltCups.Add(cupGold);
      }

      cupGold.iId = iCupIdGold;
      cupGold.sName = "Gold Cup";
      cupGold.settings.iNeutral = 2;
      cupGold.settings.iBonusStart    = 10000000; // 10 mio.
      cupGold.settings.iBonusCupWin   = 16000000; // 16 mio.
      cupGold.settings.iBonusVicGroup =  5000000; //  5 mio.
      cupGold.settings.bBonusReleaseCupWinInKo = true;
      cupGold.settings.iDayOfWeek = 3;
      cupGold.settings.fAttraction = 1.25f;
    }

    private static void createCupSilver()
    {
      CornerkickManager.Cup cupSilver = ckcore.tl.getCup(iCupIdSilver);

      // Create Silver Cup
      if (cupSilver == null) {
        cupSilver = new CornerkickManager.Cup(bKo: true, bKoTwoGames: true, nGroups: 8, nQualifierKo: 2);
        ckcore.ltCups.Add(cupSilver);
      }

      cupSilver.iId = iCupIdSilver;
      cupSilver.sName = "Silver Cup";
      cupSilver.settings.iNeutral = 2;
      cupSilver.settings.iBonusStart    =  7500000; //  7.5 mio.
      cupSilver.settings.iBonusCupWin   = 12000000; // 12 mio.
      cupSilver.settings.iBonusVicGroup =  3500000; //  3.5 mio.
      cupSilver.settings.bBonusReleaseCupWinInKo = true;
      cupSilver.settings.iDayOfWeek = 4;
      cupSilver.settings.fAttraction = 1.00f;
    }

    private static void createCupBronze()
    {
      CornerkickManager.Cup cupBronze = ckcore.tl.getCup(iCupIdBronze);

      // Create Bronze Cup
      if (cupBronze == null) {
        cupBronze = new CornerkickManager.Cup(bKo: true, bKoTwoGames: true, nGroups: 8, nQualifierKo: 2);
        ckcore.ltCups.Add(cupBronze);
      }

      cupBronze.iId = iCupIdBronze;
      cupBronze.sName = "Bronze Cup";
      cupBronze.settings.iNeutral = 2;
      cupBronze.settings.iBonusStart = 5000000; //  5 mio.
      cupBronze.settings.iBonusCupWin = 8000000; // 8 mio.
      cupBronze.settings.iBonusVicGroup = 2500000; //  2.5 mio.
      cupBronze.settings.bBonusReleaseCupWinInKo = true;
      cupBronze.settings.iDayOfWeek = 3;
      cupBronze.settings.fAttraction = 0.80f;
    }

    private static void createCupWc(DateTime dtLeagueEnd)
    {
      CornerkickManager.Cup cupWc = ckcore.tl.getCup(7);

      if (cupWc == null) {
        cupWc = new CornerkickManager.Cup(bKo: true, bKoTwoGames: false, nGroups: 2, bGroupsTwoGames: false, nQualifierKo: 2);
        cupWc.iId = 7;
        cupWc.sName = "Weltmeisterschaft";
        cupWc.settings.iNeutral = 1;
        cupWc.settings.dtStart = dtLeagueEnd.Date + new TimeSpan(20, 30, 00);
        cupWc.settings.dtEnd = ckcore.dtSeasonEnd.AddDays(-1).Date + new TimeSpan(20, 00, 00);
        cupWc.settings.fAttraction = 1.50f;
        ckcore.ltCups.Add(cupWc);

        int iGroup = 0;
        foreach (byte iN in iNations) {
          CornerkickManager.Club clbNat = new CornerkickManager.Club();
          clbNat.bNation = true;
          clbNat.iId = ckcore.ltClubs.Count;
          clbNat.sName = CornerkickManager.Main.sLand[iN];
          clbNat.iLand = iN;
          clbNat.ltTactic[0].formation = ckcore.ltFormationen[8];

          // Nat. staff
          clbNat.staff.iCondiTrainer = 6;
          clbNat.staff.iPhysio = 6;
          clbNat.staff.iMentalTrainer = 6;
          clbNat.staff.iDoctor = 6;
          clbNat.staff.iKibitzer = 3;

          // Nat. buildings
          clbNat.buildings.bgTrainingCourts.iLevel = 5;
          clbNat.buildings.bgGym.iLevel = 5;
          clbNat.buildings.bgSpa.iLevel = 5;

          ckcore.ltClubs.Add(clbNat);

          ckcore.doFormation(clbNat.iId);

          cupWc.ltClubs[iGroup / 4].Add(clbNat);
          iGroup++;
        }

        setNationColors();
      }
    }

    private static void setNationColors()
    {
      /*
      36, // GER
      29, // ENG
      30, // ESP
      45, // ITA
      33, // FRA
      54, // NED
      13, // BRA
       3  // ARG
       */
      foreach (int iN in iNations) {
        CornerkickManager.Club clbNat = CornerkickManager.Tool.getNation(iN, ckcore.ltClubs);

        if (iN == 36) { // GER
          clbNat.cl1[0] = System.Drawing.Color.White;
          clbNat.cl1[1] = System.Drawing.Color.Black;
          clbNat.cl1[2] = System.Drawing.Color.Black;

          clbNat.cl2[0] = System.Drawing.Color.Red;
          clbNat.cl2[1] = System.Drawing.Color.White;
          clbNat.cl2[2] = System.Drawing.Color.White;
        } else if (iN == 29) { // ENG
          clbNat.cl1[0] = System.Drawing.Color.White;
          clbNat.cl1[1] = System.Drawing.Color.FromArgb(15, 28, 115); // Blue
          clbNat.cl1[2] = System.Drawing.Color.FromArgb(15, 28, 115); // Blue

          clbNat.cl2[0] = System.Drawing.Color.FromArgb(255, 0, 0); // Red
          clbNat.cl2[1] = System.Drawing.Color.White;
          clbNat.cl2[2] = System.Drawing.Color.White;
        } else if (iN == 30) { // ESP
          clbNat.cl1[0] = System.Drawing.Color.FromArgb(255, 0, 0); // Red
          clbNat.cl1[1] = System.Drawing.Color.FromArgb(22, 70, 151); // Blue
          clbNat.cl1[2] = System.Drawing.Color.FromArgb(22, 70, 151); // Blue

          clbNat.cl2[0] = System.Drawing.Color.White;
          clbNat.cl2[1] = System.Drawing.Color.White;
          clbNat.cl2[2] = System.Drawing.Color.FromArgb(255, 0, 0); // Red
        } else if (iN == 45) { // ITA
          clbNat.cl1[0] = System.Drawing.Color.FromArgb(17, 62, 215); // Azure-blue
          clbNat.cl1[1] = System.Drawing.Color.White;
          clbNat.cl1[2] = System.Drawing.Color.White;

          clbNat.cl2[0] = System.Drawing.Color.White;
          clbNat.cl2[1] = System.Drawing.Color.FromArgb(17, 62, 215); // Azure-blue
          clbNat.cl2[2] = System.Drawing.Color.FromArgb(17, 62, 215); // Azure-blue
        } else if (iN == 33) { // FRA
          clbNat.cl1[0] = System.Drawing.Color.FromArgb(17, 40, 85); // Blue
          clbNat.cl1[1] = System.Drawing.Color.White;
          clbNat.cl1[2] = System.Drawing.Color.White;

          clbNat.cl2[0] = System.Drawing.Color.White;
          clbNat.cl2[1] = System.Drawing.Color.FromArgb(17, 40, 85); // Blue
          clbNat.cl2[2] = System.Drawing.Color.FromArgb(17, 40, 85); // Blue
        } else if (iN == 54) { // NED
          clbNat.cl1[0] = System.Drawing.Color.FromArgb(255, 79, 0); // Orange
          clbNat.cl1[1] = System.Drawing.Color.FromArgb(255, 79, 0);
          clbNat.cl1[2] = System.Drawing.Color.White;

          clbNat.cl2[0] = System.Drawing.Color.White;
          clbNat.cl2[1] = System.Drawing.Color.FromArgb(255, 79, 0);
          clbNat.cl2[2] = System.Drawing.Color.FromArgb(255, 79, 0);
        } else if (iN == 13) { // BRA
          clbNat.cl1[0] = System.Drawing.Color.FromArgb(255, 229, 0); // Yellow
          clbNat.cl1[1] = System.Drawing.Color.FromArgb(0, 60, 255); // Blue
          clbNat.cl1[2] = System.Drawing.Color.FromArgb(0, 60, 255); // Blue

          clbNat.cl2[0] = System.Drawing.Color.FromArgb(0, 60, 255); // Blue
          clbNat.cl2[1] = System.Drawing.Color.White;
          clbNat.cl2[2] = System.Drawing.Color.White;
        } else if (iN == 3) { // ARG
          clbNat.cl1[0] = System.Drawing.Color.FromArgb(214, 237, 255); // Light-blue
          clbNat.cl1[1] = System.Drawing.Color.White;
          clbNat.cl1[2] = System.Drawing.Color.White;

          clbNat.cl2[0] = System.Drawing.Color.FromArgb(0, 53, 94);
          clbNat.cl2[1] = System.Drawing.Color.FromArgb(214, 237, 255); // Light-blue
          clbNat.cl2[2] = System.Drawing.Color.White;
        }
      }
    }

    internal static int getStepsFromTargetToApproach()
    {
      return (int)((getCkTargetDate() - getCkApproachDate()).TotalMinutes / 15.0);
    }

    internal static DateTime getCkTargetDate()
    {
      DateTime dtCkTarget = ckcore.dtDatum.Date.Add(new TimeSpan(15, 30, 0));

      // If saturday and after target time --> add one day
      if ((int)ckcore.dtDatum.DayOfWeek == 6 && ckcore.dtDatum.TimeOfDay.CompareTo(new TimeSpan(15, 30, 0)) > 0) dtCkTarget = dtCkTarget.AddDays(1);

      while ((int)dtCkTarget.DayOfWeek != 6) dtCkTarget = dtCkTarget.AddDays(1);

      return dtCkTarget;
    }

    static TimeSpan tsTarget = new TimeSpan(20, 30, 0); // Target system time
    internal static DateTime getCkApproachDate()
    {
      double fDayRel = getDayRelBetweenNowAndTarget();

      // Get target ck date
      DateTime dtCkTarget = getCkTargetDate();

      return dtCkTarget.AddDays(-fDayRel * 7);
    }

    internal static double getDayRelBetweenNowAndTarget()
    {
      // German (and France) time:
      var euTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Central European Standard Time");
      DateTime euTime = TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneInfo.Local, euTimeZone);

      double fDayRel = (tsTarget - euTime.TimeOfDay).TotalDays;
      if (fDayRel < 0) fDayRel += 1.0;

      return fDayRel;
    }

    internal static int getDeltaStepsBetweenNowAndApproach()
    {
      DateTime dtCkApproach = getCkApproachDate();
      return (int)((dtCkApproach - ckcore.dtDatum).TotalMinutes / 15.0);
    }

    internal static int getDeltaStepsBetweenNowAndTarget()
    {
      DateTime dtCkTarget = getCkTargetDate();
      return (int)((dtCkTarget - ckcore.dtDatum).TotalMinutes / 15.0);
    }

    internal static double getIntervalForOneWeek()
    {
      // Capital letters = Real-time
      //     MIN * S  * H   /  qu  * h  * d
      return (60 * 60 * 24) / (4.0 * 24 * 7);
    }

    internal static double getIntervalAve()
    {
      double fDayRel = getDayRelBetweenNowAndTarget();
      int iStepsDelta = getDeltaStepsBetweenNowAndTarget();
      return (fDayRel * 24 * 60 * 60) / iStepsDelta;
    }

    internal static TimeSpan getApproachTime()
    {
      return tsTarget.Add(TimeSpan.FromSeconds(getStepsFromTargetToApproach() * getIntervalForOneWeek()));
    }

    private static void timerCkCalender_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
    {
      // Disable save timer (not needed if calendar timer is on)
      timerSave.Enabled = false;

      if (timerCkCalender.Interval < 1000) timerCkCalender.Enabled = false;

      timerCkCalender.Enabled = performCalendarStep();
    }

    private static void timerSave_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
    {
      save(timerCkCalender, true);
    }

    internal static bool performCalendarStep(bool bSave = true)
    {
      CornerkickManager.Cup cupGold   = ckcore.tl.getCup(iCupIdGold);
      CornerkickManager.Cup cupSilver = ckcore.tl.getCup(iCupIdSilver);
      CornerkickManager.Cup cupBronze = ckcore.tl.getCup(iCupIdBronze);

      // Reset home dir
      string sHomeDir = getHomeDir();
      if (sHomeDir != null && !sHomeDir.Equals(ckcore.settings.sHomeDir)) {
        ckcore.tl.writeLog("Reset ck home dir from " + ckcore.settings.sHomeDir + " to " + sHomeDir);
        ckcore.settings.sHomeDir = sHomeDir;
      }

      if (ckcore.ltUser.Count == 0) return true;

      if (settings.iStartHour >= 0 && settings.iStartHour <= 24) {
        if (DateTime.Now.Hour != settings.iStartHour && DateTime.Now.Hour > 13) {
          if (ckcore.dtDatum.Equals(ckcore.dtSeasonStart) ||
             ((int)ckcore.dtDatum.DayOfWeek == 1 && ckcore.dtDatum.Hour == 0 && ckcore.dtDatum.Minute == 0)) {
            return true;
          }
        }
      }

      if (ckcore.dtDatum.Hour == 0 && ckcore.dtDatum.Minute == 0 && ckcore.dtDatum.Second == 0) {
        // Put player from cpu club on transferlist if too many
        const int iClubCpuPlayerMax = 25;
        const int iClubCpuPlayerMin = 16;
        for (int iC = 1; iC < ckcore.ltClubs.Count; iC++) {
          CornerkickManager.Club clbCpu = ckcore.ltClubs[iC];
          if (clbCpu.user != null) continue;
          if (clbCpu.bNation) continue;
          if (clbCpu.ltPlayer.Count <= iClubCpuPlayerMax) continue;

          ckcore.doFormation(iC);

          for (int iP = iClubCpuPlayerMax; iP < clbCpu.ltPlayer.Count; iP++) {
            CornerkickManager.Club clbCpuTake = null;

            // Find cpu club with to few players
            for (int jC = 1; jC < ckcore.ltClubs.Count; jC++) {
              if (iC == jC) continue;

              CornerkickManager.Club clbCpuTakeTmp = ckcore.ltClubs[jC];
              if (clbCpuTakeTmp.user != null) continue;
              if (clbCpuTakeTmp.bNation) continue;

              if (clbCpuTakeTmp.ltPlayer.Count < iClubCpuPlayerMin) {
                clbCpuTake = clbCpuTakeTmp;
                break;
              }
            }

            ckcore.tr.putPlayerOnTransferlist(clbCpu.ltPlayer[iP], 0);

            if (clbCpuTake != null) {
              ckcore.tr.transferPlayer(clbCpu, clbCpu.ltPlayer[iP], clbCpuTake, bForce: true);
            }
            /*
            int jP = iP;
            while (ckcore.tr.putPlayerOnTransferlist(clbCpu.ltPlayer[jP], 0) != 1 && jP > 0) jP--;

            if (clbCpuTake != null) {
              ckcore.tr.transferPlayer(clbCpu, clbCpu.ltPlayer[jP], clbCpuTake);
            }
            */
          }
        }

        // Check if new jouth player and put on transferlist
        CornerkickManager.Club club0 = ckcore.ltClubs[0];

        CornerkickManager.Player plNew = ckcore.plt.newPlayer(club0, iNat: iNations[random.Next(iNations.Length)]);
        ckcore.tr.putPlayerOnTransferlist(plNew, 0);

        // Player jouth
        foreach (CornerkickManager.Player plJ in club0.ltPlayerJouth) {
          ckcore.tr.putPlayerOnTransferlist(plJ, 0);
        }

        /*
        if (countCpuPlayerOnTransferlist() > 200) {
          for (int iT = 0; iT < ckcore.ltTransfer.Count; iT++) {
            CornerkickManager.Transfer.Item transfer = ckcore.ltTransfer[iT];
            if (club0.ltPlayer.IndexOf(transfer.player) >= 0) {
              ckcore.tr.removePlayerFromTransferlist(transfer.player);
              break;
            }
          }
        }
        */

        // retire cpu player
        if (club0.ltPlayer.Count > 1500) {
          ckcore.plt.retirePlayer(club0.ltPlayer[0], club0);
        }

        //checkCpuJouth();
      } // If midnight

      // Save .autosave
      if (bSave && ckcore.dtDatum.Minute == 0 && ckcore.dtDatum.Hour % 2 == 0) {
        save(timerCkCalender);
      }

      if ((ckcore.dtDatum.Equals(ckcore.dtSeasonStart) ||
          ckcore.dtDatum.Year < 1900) &&
          ckcore.iSeason == 0) {
        ltLog.Clear();
        ckcore.setNewSeason();
      }

      // Do next step
      int iRetCk = 0;
      try {
        iRetCk = ckcore.next();
      } catch (Exception e) {
        ckcore.tl.writeLog("performCalendarStep(): Error in ck next()" + Environment.NewLine + e.Message + e.StackTrace, CornerkickManager.Main.sErrorFile);
      }

      // Reset CPU player
      if (ckcore.dtDatum.TimeOfDay.Equals(new TimeSpan(15, 0, 0))) {
        for (int iC = 0; iC < ckcore.ltClubs.Count; iC++) {
          CornerkickManager.Club clb = ckcore.ltClubs[iC];
          if (clb.user != null) continue;

          float fContiCPU = 0.9f;
          float fFreshCPU = 1.0f;
          if (clb.iDivision > 0) {
            fContiCPU = 0.80f;
            fFreshCPU = 0.92f;
          }
          for (int iP = 0; iP < clb.ltPlayer.Count; iP++) {
            clb.ltPlayer[iP].plGame.fCondition = fContiCPU;
            clb.ltPlayer[iP].plGame.fFresh     = fFreshCPU;
            clb.ltPlayer[iP].plGame.fMoral     = Math.Max(clb.ltPlayer[iP].plGame.fMoral, 0.95f);
          }
        }
      }

      // Reset player moral if ...
      for (int iPl = 0; iPl < ckcore.ltPlayer.Count; iPl++) {
        CornerkickManager.Player pl = ckcore.ltPlayer[iPl];
        string sClubName = "vereinslos";
        if (pl.contract != null) sClubName = pl.contract.club.sName;

        // ... NaN
        if (float.IsNaN(pl.plGame.fMoral)) {
          pl.plGame.fMoral = 1f;
          ckcore.tl.writeLog("Reset moral (NaN) of player " + pl.plGame.sName + ", id: " + iPl.ToString() + ", club: " + sClubName, CornerkickManager.Main.sErrorFile);
        }

        // ... below minimum
        if (pl.plGame.fMoral < ckcore.settings.fMoralMin) {
          ckcore.tl.writeLog("Reset moral (" + pl.plGame.fMoral.ToString("0.0%") + ") of player " + pl.plGame.sName + ", id: " + iPl.ToString() + ", club: " + sClubName, CornerkickManager.Main.sErrorFile);
          pl.plGame.fMoral = ckcore.settings.fMoralMin;
        }
      }

      // CHEAT-SECTION
      // Jan no injury
      CornerkickManager.Player plJan = ckcore.ltPlayer[101];
      plJan.plGame.injury = null;
      CornerkickManager.Player plAaronSchulz = ckcore.ltPlayer[3029];
      plAaronSchulz.plGame.injury = null;

      // Janos/David money
      if (ckcore.dtDatum.Day == 1 && ckcore.dtDatum.Hour == 20 && ckcore.dtDatum.Minute == 0 && ckcore.dtDatum.Second == 0) {
        CornerkickManager.Club clbJanos = ckcore.ltClubs[5];
        CornerkickManager.Club clbDavid = ckcore.ltClubs[143];

        int iBalanceMin = 10000000;
        foreach (CornerkickManager.Club clbBalance in new CornerkickManager.Club[] { clbJanos, clbDavid }) {
          if (clbBalance.iBalance < iBalanceMin) {
            int iDeltaBalance = iBalanceMin - clbBalance.iBalance;
            CornerkickManager.Finance.doTransaction(clbBalance, ckcore.dtDatum, iDeltaBalance, 0, "Ausgleich");
          }
        }
      }
      // END OF CHEAT-SECTION

      // Assign random portrait if none
      for (int iPl = 0; iPl < ckcore.ltPlayer.Count; iPl++) {
        CornerkickManager.Player pl = ckcore.ltPlayer[iPl];
        if (pl == null) continue;

        try {
          if (pl.clSkin.B == 0) {
#if DEBUG
            string sBaseDir = getHomeDir();
#else
            string sBaseDir = Directory.GetParent(ckcore.settings.sHomeDir).FullName;
#endif
            string sDirPortrait = System.IO.Path.Combine(sBaseDir, "Content", "Images", "Portraits");

            if (System.IO.Directory.Exists(sDirPortrait)) {
              System.IO.DirectoryInfo diPortrait = new System.IO.DirectoryInfo(sDirPortrait);

              int nPortraitFiles = diPortrait.GetFiles("*.png").Length;
              ushort iPortraitId = (ushort)random.Next(nPortraitFiles);

              byte[] b = BitConverter.GetBytes(iPortraitId);

              pl.clSkin = System.Drawing.Color.FromArgb(b[0], b[1], 1);
            }
          }
        } catch {
        }
      }

      // Set training template if user has set no training
      if (ckcore.dtDatum.DayOfWeek == DayOfWeek.Monday && ckcore.dtDatum.Hour == 0 && ckcore.dtDatum.Minute == 0) {
        foreach (CornerkickManager.Club clb in ckcore.ltClubs) {
          if (clb.user == null) continue;

          bool bTraining = false;

          CornerkickManager.Main.TrainingPlan.Unit[][] tpWeek = Controllers.MemberController.getTrainingPlan(clb, 0);
          foreach (CornerkickManager.Main.TrainingPlan.Unit[] tpDay in tpWeek) {
            foreach (CornerkickManager.Main.TrainingPlan.Unit tu in tpDay) {
              if (tu.iType > 0 && tu.iType < 100) {
                bTraining = true;
                break;
              }
            }
            if (bTraining) break;
          }

          if (!bTraining) {
            Controllers.MemberController.setTrainingWeekTemplate(clb, 0, 2);
          }
        }
      }

      // Beginn of new season
      if (iRetCk == 4) {
        /*
        // Create second league
        if (ckcore.tl.getCup(1, iNations[0], 1) == null) {
          // Create league
          CornerkickManager.Cup league = new CornerkickManager.Cup(nGroups: 1, bGroupsTwoGames: true);
          league.iId = 1;
          league.iId2 = iNations[0];
          league.iId3 = 1;
          league.sName = "2. Liga " + CornerkickManager.Main.sLand[iNations[0]];
          league.settings.fAttraction = 0.75f;
          //league.settings.dtStart = ckcore.tl.getCup(1, iNations[0], 0).ltMatchdays[0].dt;
          //league.settings.dtEnd   = ckcore.tl.getCup(1, iNations[0], 0).ltMatchdays[ckcore.tl.getCup(1, iNations[0], 0).ltMatchdays.Count - 1].dt;
          ckcore.ltCups.Insert(2, league);

          fillLeaguesWithCpuClubs(league, ckcore.tl.getCup(2, iNations[0]));

          ckcore.calcMatchdays();
        }
        */

        // Draw leagues
        foreach (int iN in iNations) {
          for (int iDiv = 0; iDiv < 2; iDiv++) {
            CornerkickManager.Cup league = ckcore.tl.getCup(1, iN, iDiv);
            if (league == null) continue;
            league.draw(ckcore.dtDatum);
          }
        }

        // Draw gold/silver cup
        cupGold  .draw(ckcore.dtDatum);
        cupSilver.draw(ckcore.dtDatum);
        cupBronze.draw(ckcore.dtDatum);

        // Set club next game
        foreach (CornerkickManager.Club clb in ckcore.ltClubs) {
          clb.nextGame = ckcore.tl.getNextGame(clb, ckcore.dtDatum);
        }
      }

      // End of season
      if (iRetCk == 99) {
        // Clear national coaches
        clearNations();

        //////////////////////////////////////////////////
        // Nominate clubs to international cups
        //////////////////////////////////////////////////
        // Clear groups
        for (byte iG = 0; iG < cupGold  .ltClubs.Length; iG++) cupGold  .ltClubs[iG].Clear();
        for (byte iG = 0; iG < cupSilver.ltClubs.Length; iG++) cupSilver.ltClubs[iG].Clear();
        for (byte iG = 0; iG < cupBronze.ltClubs.Length; iG++) cupBronze.ltClubs[iG].Clear();

        // Add clubs ...
        int iGroupGold   = 0;
        int iGroupSilver = 0;
        int iGroupBronze = 0;
        foreach (int iN in iNations) {
          // ... of league iN ...
          CornerkickManager.Cup league = ckcore.tl.getCup(1, iN, 0);
          if (league == null) continue;

          List<CornerkickManager.Cup.TableItem> ltTbl = league.getTable();

          // ... to Gold Cup
          for (byte jL = 0; jL < 4; jL++) {
            if (iGroupGold >= cupGold.ltClubs.Length) iGroupGold = 0;
            cupGold.ltClubs[iGroupGold].Add(ltTbl[jL].club);
            iGroupGold++;
          }

          // ... to Silver Cup
          for (byte jL = 4; jL < 8; jL++) {
            if (iGroupSilver >= cupSilver.ltClubs.Length) iGroupSilver = 0;
            cupSilver.ltClubs[iGroupSilver].Add(ltTbl[jL].club);
            iGroupSilver++;
          }

          // ... to Bronze Cup
          for (byte jL = 8; jL < 12; jL++) {
            if (iGroupBronze >= cupBronze.ltClubs.Length) iGroupBronze = 0;
            cupBronze.ltClubs[iGroupBronze].Add(ltTbl[jL].club);
            iGroupBronze++;
          }
        }

        cupGold  .ltMatchdays.Clear();
        cupSilver.ltMatchdays.Clear();
        cupBronze.ltMatchdays.Clear();

        ckcore.calcMatchdays();

        return false;
      }

      // Remove testgame requests if in past
      List<CornerkickManager.Cup> ltCupsTmp = new List<CornerkickManager.Cup>(ckcore.ltCups);
      foreach (CornerkickManager.Cup cup in ltCupsTmp) {
        if (cup == null) continue;

        if (cup.iId == -iCupIdTestgame) {
          if (cup.ltMatchdays.Count < 1) continue;

          if (cup.ltMatchdays[0].dt.CompareTo(ckcore.dtDatum) <= 0) { // if request in past or now ...
            ckcore.ltCups.Remove(cup); // ... remove cup
          }
        }
      }

      // Inform user if transfer offer too low
      if (ckcore.dtDatum.TimeOfDay.Equals(new TimeSpan(12, 00, 00))) {
        // For each transfer
        foreach (CornerkickManager.Transfer.Item transfer in ckcore.ltTransfer) {
          if (transfer.player == null) continue;
          if (transfer.player.contract == null) continue;

          CornerkickManager.Club clbCpu = transfer.player.contract.club;
          if (clbCpu.user != null) continue; // If human user
          if (ckcore.ltUser.IndexOf(clbCpu.user) == 0) continue; // If main CPU user

          // Get max offer
          int iOfferMax = 0;
          foreach (CornerkickManager.Transfer.Offer offer in transfer.ltOffers) {
            iOfferMax = Math.Max(iOfferMax, offer.iFee);
          }

          // Inform users
          /*
          foreach (CornerkickManager.Transfer.Offer offer in transfer.ltOffers) {
            if (offer.iFee < iOfferMax) {
              ckcore.sendNews(offer.club.user, "Ihr Transferangebot für den Spieler " + transfer.player.sName + " ist leider nicht (mehr) hoch genug.");
            }
          }
          */
        }
      }

      // Reduce player for WC / Remove national coaches
      const int nPlayerNat = 22;
      CornerkickManager.Cup cupWc = ckcore.tl.getCup(7);
      DateTime dtWcSelectPlayerFinish = new DateTime();
      if (cupWc != null) {
        if (cupWc.ltMatchdays != null) {
          if (cupWc.ltMatchdays.Count > 0) {
            // Reduce player for WC
            dtWcSelectPlayerFinish = cupWc.ltMatchdays[0].dt.Date.AddDays(-6);

            if (ckcore.dtDatum.Equals(dtWcSelectPlayerFinish)) {
              // For each national team
              foreach (CornerkickManager.Club nat in ckcore.ltClubs) {
                if (!nat.bNation) continue;

                if (nat.user != null && nat.ltPlayer.Count < 11) {
                  ckcore.sendNews(nat.user, "Der Verband von " + nat.sName + " entscheidet sich dann doch für einen anderen Trainer.");
                  nat.user.nation = null;
                  nat.user = null;

                  // Nominate player
                  nat.ltPlayer.Clear();
                  nat.ltPlayer = ckcore.getBestPlayer(nat.iLand, iPlCount: nPlayerNat);
                }

                while (nat.ltPlayer.Count > nPlayerNat) nat.ltPlayer.RemoveAt(nPlayerNat);

                // Set player no.
                for (int iP = 0; iP < nat.ltPlayer.Count; iP++) nat.ltPlayer[iP].plGame.iNrNat = (byte)(iP + 1);
              }
            }

            // Remove national coaches
            CornerkickManager.Cup.Matchday mdWcFinal = cupWc.ltMatchdays[cupWc.ltMatchdays.Count - 1];
            if (mdWcFinal.ltGameData != null && mdWcFinal.ltGameData.Count == 1 && ckcore.dtDatum.Equals(mdWcFinal.dt.AddDays(1))) { // Final game
              clearNations();
            }
          }
        }
      }

      // Nominate user for WC
      DateTime dtWcDraw = new DateTime();
      bool bReturn = true;

      foreach (int iN in iNations) {
        CornerkickManager.Cup league = ckcore.tl.getCup(1, iN, 0);
        if (league == null) continue;
        if (league.ltMatchdays == null) continue;
        if (league.ltMatchdays.Count == 0) continue;

        dtWcDraw = new DateTime(Math.Max(league.ltMatchdays[league.ltMatchdays.Count - 1].dt.Ticks, dtWcDraw.Ticks));
      }

      if (cupGold != null) {
        if (cupGold.ltMatchdays != null && cupGold.ltMatchdays.Count > 0) {
          if (cupGold.ltMatchdays[cupGold.ltMatchdays.Count - 1].ltGameData != null) {
            if (cupGold.ltMatchdays[cupGold.ltMatchdays.Count - 1].ltGameData.Count == 1) {
              dtWcDraw = new DateTime(Math.Max(cupGold.ltMatchdays[cupGold.ltMatchdays.Count - 1].dt.Ticks, dtWcDraw.Ticks));
            }
          }
        }
      }

      //while (dtWcDraw.DayOfWeek != DayOfWeek.Sunday) dtWcDraw = dtWcDraw.AddDays(1);
      dtWcDraw = dtWcDraw.Date.AddDays(1).AddHours(12);

      //dtWcDraw = leagueGer.ltMatchdays[leagueGer.ltMatchdays.Count - 1].dt.Date.AddDays(1).AddHours(12);
      if (ckcore.dtDatum.Equals(dtWcDraw)) {
        cupWc.settings.dtStart = dtWcDraw.AddDays(13).Date + new TimeSpan(20, 30, 00);
        cupWc.settings.dtEnd = ckcore.dtSeasonEnd.AddDays(-1).Date + new TimeSpan(20, 00, 00);
        ckcore.calcMatchdays(cupWc, ckcore.dtSeasonStart, ckcore.dtSeasonEnd);
        cupWc.draw(ckcore.dtDatum);
      } else if (ckcore.dtDatum.Equals(dtWcDraw.AddMinutes(15))) {
        /*
        ///////////////////////////////////////////////////////////////////////////////////////////
        // Option A: Select national coaches from winner of leagues
        ///////////////////////////////////////////////////////////////////////////////////////////
        foreach (CornerkickManager.Cup league in ckcore.ltCups) { // For each 1st division league
          if (league.iId != 1) continue;
          if (league.iId3 > 0) continue;
          if (league.ltMatchdays == null) continue;
          if (league.ltMatchdays.Count < 2) continue;

          List<CornerkickManager.Cup.TableItem> tbl = league.getTable();
          foreach (CornerkickManager.Cup.TableItem item in tbl) {
            if (item.club.user != null) {
              if (ckcore.ltUser.IndexOf(item.club.user) == 0) continue; // If main CPU user

              CornerkickManager.Club nat = CornerkickManager.Tool.getNation(league.iId2, ckcore.ltClubs);
              if (nat == null) continue;

              nat.user = item.club.user;
              item.club.user.nation = nat;

              // Inform user
              if (dtWcSelectPlayerFinish.CompareTo(ckcore.dtDatum) > 0) ckcore.sendNews(item.club.user, "Bitte wählen Sie noch bis zum " + dtWcSelectPlayerFinish.ToString("d", Controllers.MemberController.getCiStatic(league.iId2)) + " Ihre " + nPlayerNat.ToString() + " Spieler für die Endrunde aus.");
              ckcore.sendNews(item.club.user, "Welche Ehre! Der Verband von " + nat.sName + " stellt Sie als Nationaltrainer für die kommende WM ein.");

              bReturn = false;
              break;
            }
          }
        }
        */

        ///////////////////////////////////////////////////////////////////////////////////////////
        // Option B: Select national coaches from club attraction factor
        ///////////////////////////////////////////////////////////////////////////////////////////
        // Collect list of nations based on their skill
        List<Nation2> ltNat2 = new List<Nation2>();
        foreach (int iNat in iNations) {
          CornerkickManager.Club nat = CornerkickManager.Tool.getNation(iNat, ckcore.ltClubs);
          if (nat == null) continue;

          Nation2 nat2 = new Nation2();
          nat2.nation = nat;

          List<CornerkickManager.Player> ltPlayerNatTmp = ckcore.getBestPlayer(iNat, iPlCount: nPlayerNat);
          foreach (CornerkickManager.Player plNatTmp in ltPlayerNatTmp) {
            nat2.fSkillTotal += CornerkickGame.Tool.getAveSkill(plNatTmp.plGame, bIdeal: true);
          }

          ltNat2.Add(nat2);
        }
        ltNat2 = ltNat2.OrderByDescending(o => o.fSkillTotal).ToList();

        // Collect user based on their clubs attraction factor
        List<User4Nat> ltUser4Nat = new List<User4Nat>();
        foreach (CornerkickManager.User usr in ckcore.ltUser) {
          if (usr.club == null) continue;

          ltUser4Nat.Add(new User4Nat { usr = usr, fAttrFactor = usr.club.getAttractionFactor(ckcore.iSeason, ltCups: ckcore.ltCups, dtNow: ckcore.dtDatum) });
        }
        ltUser4Nat = ltUser4Nat.OrderByDescending(o => o.fAttrFactor).ToList();

        // Link nations to user
        for (int iN2 = 0; iN2 < ltNat2.Count; iN2++) {
          if (iN2 >= ltUser4Nat.Count) break;
          if (ltUser4Nat[iN2].fAttrFactor < 100f) break;

          CornerkickManager.Club nat = ltNat2[iN2].nation;
          CornerkickManager.User usr = ltUser4Nat[iN2].usr;

          // Assign user to nat and vise versa
          nat.user = usr;
          usr.nation = nat;

          // Inform user
          if (dtWcSelectPlayerFinish.CompareTo(ckcore.dtDatum) > 0) ckcore.sendNews(usr, "Bitte wählen Sie noch bis zum " + dtWcSelectPlayerFinish.ToString("d", Controllers.MemberController.getCiStatic(nat.iLand)) + " Ihre " + nPlayerNat.ToString() + " Spieler für die Endrunde aus.");
          ckcore.sendNews(usr, "Welche Ehre! Der Verband von " + nat.sName + " stellt Sie als Nationaltrainer für die kommende WM ein.");
        }

        foreach (int iNat in iNations) {
          CornerkickManager.Club nat = CornerkickManager.Tool.getNation(iNat, ckcore.ltClubs);
          if (nat == null) continue;
          nat.ltPlayer.Clear();

          // Add all player of that nation
          if (nat.user == null) nat.ltPlayer = ckcore.getBestPlayer(iNat, iPlCount: nPlayerNat);
        }
      }

      return bReturn;
    }

    private class Nation2
    {
      public CornerkickManager.Club nation { get; set; }
      public float fSkillTotal { get; set; }
    }

    private class User4Nat
    {
      public CornerkickManager.User usr { get; set; }
      public float fAttrFactor { get; set; }
    }

    private static void clearNations()
    {
      foreach (CornerkickManager.User usrNat in ckcore.ltUser) usrNat.nation = null;
      foreach (CornerkickManager.Club nat in ckcore.ltClubs) {
        if (nat.bNation) {
          nat.user = null;
          nat.ltPlayer.Clear();
        }
      }
    }

    private static int countCpuPlayerOnTransferlist()
    {
      int nPl = 0;
      foreach (CornerkickManager.Transfer.Item transfer in ckcore.ltTransfer) {
        if (ckcore.ltClubs[0].ltPlayer.IndexOf(transfer.player) >= 0) nPl++;
      }

      return nPl;
    }

    /*
    private static void checkCpuJouth()
    {
      while (ckcore.ltClubs[0].ltJugendspielerID.Count > 0) {
        int iPlId = ckcore.ltClubs[0].ltJugendspielerID[0];
        ckcore.ltClubs[0].ltJugendspielerID.RemoveAt(0);
        ckcore.ltClubs[0].ltPlayerId.Add(iPlId);
        ckcore.ui.putPlayerOnTransferlist(iPlId, 0);
      }
    }
    */

    internal static async Task<bool> saveAsync(System.Timers.Timer timerCkCalender, bool bForce = false)
    {
      save(timerCkCalender, bForce: bForce);
      return true;
    }
    internal static void save(System.Timers.Timer timerCkCalender, bool bForce = false)
    {
      // Don't save if calendar to fast
      if (timerCkCalender.Interval < 10000 && !bForce) return;

      string sHomeDir = getHomeDir();
      if (string.IsNullOrEmpty(sHomeDir)) sHomeDir = ckcore.settings.sHomeDir;

#if DEBUG
      sHomeDir = "C:\\Users\\Jan\\Documents\\Visual Studio 2017\\Projects\\Cornerkick.git\\CornerkickWebMvc";
#else
      try {
#if _DEPLOY_ON_AZURE
        sHomeDir = HttpContext.Current.Server.MapPath("~");
#endif
        if (sHomeDir.EndsWith("\\")) sHomeDir = sHomeDir.Remove(sHomeDir.Length - 1);
      } catch (HttpException exp) {
        ckcore.tl.writeLog("save: HttpException: " + exp.Message.ToString());
#if _DEPLOY_ON_AZURE
        sHomeDir = "D:\\home\\site\\wwwroot";
#endif
      } catch {
        ckcore.tl.writeLog("save: unable to create sHomeDir from Server.MapPath", CornerkickManager.Main.sErrorFile);
#if _DEPLOY_ON_AZURE
        sHomeDir = "D:\\home\\site\\wwwroot";
#endif
      }
#endif

#if _USE_AMAZON_S3
      AmazonS3FileTransfer as3 = new AmazonS3FileTransfer();
#endif

      // Clear CPU user news before saving
      for (int iN = 0; iN < ckcore.ltUser[0].ltNews.Count; iN++) {
        if (ckcore.ltUser[0].ltNews[iN].iType < 200) {
          ckcore.ltUser[0].ltNews.RemoveAt(iN);
          iN--;
          continue;
        }

        if ((ckcore.dtDatum - ckcore.ltUser[0].ltNews[iN].dt).TotalDays > 7) {
          ckcore.ltUser[0].ltNews.RemoveAt(iN);
          iN--;
          continue;
        }
      }

      // Clear CPU clubs before saving
      foreach (CornerkickManager.Club clb in ckcore.ltClubs) {
        if (clb.user == null) {
          clb.ltAccount      .Clear();
          clb.ltTrainingHist .Clear();
          clb.ltSponsorOffers.Clear();
          clb.iBalance = 0;
        }
      }

      // Compose filename
      string sFilenameSave2 = ".autosave_" + ckcore.dtDatum.ToString("yyyy-MM-dd_HH-mm") + ".ckx";
      string sFileSave2 = Path.Combine(sHomeDir, "save", sFilenameSave2);
      ckcore.tl.writeLog("save file: " + sFileSave2);

      // Save
      bool bSaveOk = false;
      try {
        bSaveOk = ckcore.io.save(sFileSave2);
      } catch (Exception e) {
        ckcore.tl.writeLog("ERROR: could not save to file " + sFileSave2 + Environment.NewLine + e.Message + e.StackTrace, CornerkickManager.Main.sErrorFile);
      }

      // Upload save
      if (bSaveOk) {
#if _USE_AMAZON_S3
        as3.uploadFile(sFileSave2, sFilenameSave2, "application/zip");
#endif

        // Copy autosave file with datum to basic one (could use file link)
        string sFileSave = sHomeDir + "/save/" + sFilenameSave;
        if (System.IO.File.Exists(sFileSave)) {
          try {
            System.IO.File.Delete(sFileSave);
          } catch {
          }
        }

        System.IO.File.Copy(sFileSave2, sFileSave);
#if _USE_AMAZON_S3
        as3.uploadFile(sFileSave, sFilenameSave, "application/zip");
#endif
      }

      // Write last ck state to file
      saveLaststate(sHomeDir);

      //saveMerchHistory(sHomeDir);

#if _USE_AMAZON_S3
      // Upload games
      DirectoryInfo diGames = new DirectoryInfo(Path.Combine(sHomeDir, "save", "games"));
      ckcore.tl.writeLog("Directory info games: '" + diGames.FullName + "'. Exist: " + diGames.Exists.ToString());

      if (diGames.Exists) {
        FileInfo[] ltCkgFiles = diGames.GetFiles("*.ckgx");
        ckcore.tl.writeLog("File info games length: " + ltCkgFiles.Length.ToString());

        foreach (FileInfo ckg in ltCkgFiles) {
          DateTime dtGame;
          int iTeamIdH;
          int iTeamIdA;
          int iCupId;

          Controllers.ViewGameController.getFilenameInfo(ckg, out dtGame, out iTeamIdH, out iTeamIdA, out iCupId);

          if (dtGame.CompareTo(dtLoadCk) < 0) continue; // If game was already present when ck was started
          if (iCupId == iCupIdTestgame) continue; // If game is test-game

          string sFileGameSave = Path.Combine(sHomeDir, "save", "games", ckg.Name);
          as3.uploadFile(sFileGameSave, "save/games/" + ckg.Name, "application/zip");
        }
      }
#endif

      // save log dir
      if (Directory.Exists(sHomeDir + "/log")) {
        string sFileZipLog = sHomeDir + "/log.zip";

#if !DEBUG
        // Delete existing zip file
        if (File.Exists(sFileZipLog)) {
          try {
            File.Delete(sFileZipLog);
          } catch {
          }
        }

        ZipFile.CreateFromDirectory(sHomeDir + "/log", sFileZipLog);

        try {
#if _USE_BLOB
          CornerkickWebMvc.Controllers.BlobsController bcontr = new Controllers.BlobsController();
          bcontr.uploadBlob("blobLog", sFileZipLog);
#endif
#if _USE_AMAZON_S3
          as3.uploadFile(sFileZipLog, "ckLog_" + DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss"), "application/zip");
#endif
        } catch (Exception e) {
#if _USE_BLOB
          ckcore.tl.writeLog("ERROR: could not upload log.zip file to blob", ckcore.sErrorFile);
#endif
#if _USE_AMAZON_S3
          ckcore.tl.writeLog("ERROR: could not upload log.zip file to amazon s3" + Environment.NewLine + e.Message, CornerkickManager.Main.sErrorFile);
#endif
        }
#endif
      }

#if _USE_AMAZON_S3
      saveMails(as3);
#else
      saveMails();
#endif

#if _USE_AMAZON_S3
      // Upload wishlist
      as3.uploadFile(Path.Combine(sHomeDir, "wishlist.json"), "wishlist.json");
#endif
    }

    internal static void saveLaststate(string sTargetDir)
    {
      string sFileSettings = Path.Combine(sTargetDir, sFilenameSettings);

      using (System.IO.StreamWriter fileSettings = new System.IO.StreamWriter(sFileSettings)) {
        fileSettings.WriteLine((timerCkCalender.Interval / 1000.0).ToString("g", CultureInfo.InvariantCulture));
        fileSettings.WriteLine(timerCkCalender.Enabled.ToString());
        fileSettings.WriteLine(DateTime.Now.ToString("s", CultureInfo.InvariantCulture));

        int iGameSpeed = 0;
        if (ckcore.ltUser.Count > 0 && ckcore.ltUser[0].nextGame != null) iGameSpeed = ckcore.ltUser[0].nextGame.iGameSpeed;
        fileSettings.WriteLine(iGameSpeed.ToString());

        fileSettings.WriteLine(settings.bEmailCertification.ToString());
        fileSettings.WriteLine(settings.bRegisterDuringGame.ToString());

        fileSettings.WriteLine(settings.bMaintenance.ToString());

        fileSettings.Close();
      }

#if _USE_AMAZON_S3
      AmazonS3FileTransfer as3 = new AmazonS3FileTransfer();
      as3.uploadFile(sFileSettings, "laststate");
#endif
    }

#if _USE_AMAZON_S3
    private static void saveMails(AmazonS3FileTransfer as3 = null)
#else
    private static void saveMails()
#endif
    {
      string sDirMail = System.IO.Path.Combine(ckcore.settings.sHomeDir, "mail");
      if (!System.IO.Directory.Exists(sDirMail)) System.IO.Directory.CreateDirectory(sDirMail);

      if (MvcApplication.ltMail == null) return;

      foreach (MvcApplication.Mail mail in ltMail) {
        string sDateTime = mail.dt.ToString("yyyyMMddHHmmss");
        string sFilenameMail  = mail.sIdTo + "_" + sDateTime + ".txt";
        string sFilenameMail2 = System.IO.Path.Combine(sDirMail, sFilenameMail);

        using (System.IO.StreamWriter fileMail = new System.IO.StreamWriter(sFilenameMail2)) {
          string sText = mail.sIdTo + " " + mail.sIdFrom + " " + sDateTime + " " + mail.bNew.ToString() + Environment.NewLine + mail.sText;
          fileMail.Write(sText);
          fileMail.Close();

          if (as3 != null) as3.uploadFile(sFilenameMail2, "mail/" + sFilenameMail);
        }
      }
    }

    internal static async Task<bool> loadAsync(string sHomeDir)
    {
      // Create stopwatch
      System.Diagnostics.Stopwatch swLoad = new System.Diagnostics.Stopwatch();

      // Start stopwatch
      swLoad.Start();

      if (string.IsNullOrEmpty(sHomeDir)) return false;

      string sFileLoad = Path.Combine(sHomeDir, "save", sFilenameSave);

#if !DEBUG
#if _USE_BLOB
      string sFileZipLog = sHomeDir + "log.zip";

      CornerkickWebMvc.Controllers.BlobsController bcontr = new Controllers.BlobsController();
      if (!System.IO.File.Exists(sFileLoad)) bcontr.downloadBlob("blobSave", sFileLoad);
      bcontr.downloadBlob("blobLog", sFileZipLog);
#endif
#if _USE_AMAZON_S3
      AmazonS3FileTransfer as3 = new AmazonS3FileTransfer();

      if (!System.IO.File.Exists(sFileLoad)) {
        try {
          as3.downloadFile(sFilenameSave, sFileLoad);
        } catch {
          ckcore.tl.writeLog("ERROR: Unable to download file " + sFilenameSave + " to: " + sFileLoad, CornerkickManager.Main.sErrorFile);
        }
        /*
        if (Directory.Exists(sHomeDir + "save")) {
          try {
            Directory.Delete(sHomeDir + "save", true);
          } catch {
            ckcore.tl.writeLog("ERROR: unable to delete existing temp. load directory: " + sHomeDir + "save", ckcore.sErrorFile);
          }
        }

        Directory.CreateDirectory(sHomeDir + "save");
        if (System.IO.File.Exists(sHomeDir + sSaveZip)) ZipFile.ExtractToDirectory(sHomeDir + sSaveZip, sHomeDir + "save");
        */
      }

      // Download log async
      //Task<bool> tkDownloadLog = Task.Run(async () => await downloadFileAsync(as3, "ckLog", sHomeDir + "/log.zip"));

      // Download Google ads.txt async
      Task<bool> tkDownloadAds = Task.Run(async () => await downloadFileAsync(as3, "ads.txt", Path.Combine(sHomeDir, "..", "ads.txt")));
#endif
#endif

      // Load ck state
      if (ckcore.io.load(sFileLoad)) {
        ckcore.tl.writeLog("File " + sFileLoad + " loaded.");

        // Set admin user to CPU
        if (ckcore.ltClubs.Count > 0) ckcore.ltClubs[0].user = null;

        // Set length of EocInfo flag
        Controllers.MemberController.bHideEocInfo = new bool[MvcApplication.ckcore.ltUser.Count];

        // Set length of tutorial class array
        Controllers.MemberController.initialiteTutorial();

        // Set retired players name to none
        List<CornerkickManager.Player> ltPlayerRet = CornerkickManager.PlayerTool.getRetiredPlayer(ckcore.ltPlayer);
        for (int iPl = 0; iPl < ltPlayerRet.Count; iPl++) {
          ltPlayerRet[iPl].plGame.sName = "";
        }

        // Transfer player from club0 to cpu-club if too few
        for (int iC = 1; iC < ckcore.ltClubs.Count; iC++) {
          CornerkickManager.Club clb = ckcore.ltClubs[iC];
          if (clb.bNation) continue;
          if (clb.user != null) continue;

          CornerkickManager.Club clb0 = ckcore.ltClubs[0];
          while (clb.ltPlayer.Count < 16) ckcore.tr.transferPlayer(clb0, clb0.ltPlayer[0], clb, bForce: true);
        }

        // Delete past trainings from club
        for (int iC = 1; iC < ckcore.ltClubs.Count; iC++) {
          CornerkickManager.Club clb = ckcore.ltClubs[iC];

          for (int iTU = 0; iTU < clb.training.ltUnit.Count; iTU++) {
            if (clb.training.ltUnit[iTU].dt.CompareTo(ckcore.dtDatum) < 0) {
              clb.training.ltUnit.RemoveAt(iTU);
              iTU--;
            }
          }
        }

        // TMP section
        // END TMP section

        dtLoadCk = ckcore.dtDatum;

        string sFileLastState = Path.Combine(sHomeDir, sFilenameSettings);
#if !DEBUG
#if _USE_AMAZON_S3
        if (!System.IO.File.Exists(sFileLastState)) as3.downloadFile("laststate", sFileLastState);
#endif

        if (System.IO.File.Exists(sFileLastState)) {
          ckcore.tl.writeLog("Reading laststate from file: " + sFileLastState);

          string[] sStateFileContent = System.IO.File.ReadAllLines(sFileLastState);

          DateTime dtLast = new DateTime();
          if (sStateFileContent.Length > 3) {
            //double fInterval = getIntervalAve(); // Calendar interval [s]
            double fInterval = 0.0; // Calendar interval [s]

            NumberStyles style = NumberStyles.Number | NumberStyles.AllowDecimalPoint;
            fInterval = double.Parse(sStateFileContent[0], style, CultureInfo.InvariantCulture);
            ckcore.tl.writeLog("Set calendar interval to " + fInterval.ToString("0.000") + "s");

            bool bCalendarRunning = false;
            bool.TryParse(sStateFileContent[1], out bCalendarRunning);

            if (fInterval > 10.0 && bCalendarRunning && DateTime.TryParse(sStateFileContent[2], out dtLast)) {
              //double fTotalMin = (DateTime.Now - dtLast).TotalMinutes;
              //int nSteps = (int)(fTotalMin / (fInterval / 60f));
              int nSteps = getDeltaStepsBetweenNowAndApproach();

              if (nSteps > 0) {
                ckcore.tl.writeLog("Last step was at " + dtLast.ToString("s", CultureInfo.InvariantCulture) + " (now: " + DateTime.Now.ToString("s", CultureInfo.InvariantCulture) + ")");

                int iGameSpeed = 0; // Calendar interval [s]
                int.TryParse(sStateFileContent[3], out iGameSpeed);
                ckcore.tl.writeLog("Set game speed to " + iGameSpeed.ToString() + "ms");

                // Perform calendar steps in background
                Task<bool> tkPerformCalendarSteps = Task.Run(async () => await performCalendarStepsAsync(iGameSpeed, fInterval, bCalendarRunning));
              } else {
                timerCkCalender.Interval = fInterval * 1000.0; // Convert [s] to [ms]
                timerCkCalender.Enabled = bCalendarRunning;
                ckcore.tl.writeLog("Calendar Interval set to " + timerCkCalender.Interval.ToString() + " ms");
              }
            }

            if (sStateFileContent.Length > 5) {
              bool.TryParse(sStateFileContent[4], out settings.bEmailCertification);
              bool.TryParse(sStateFileContent[5], out settings.bRegisterDuringGame);
            }

            if (sStateFileContent.Length > 6) bool.TryParse(sStateFileContent[6], out settings.bMaintenance);
          }
        } else {
          ckcore.tl.writeLog("laststate file '" + sFileLastState + "' does not exist");
        }

#if _USE_AMAZON_S3
        // Download emblems
        Task<bool> tkDownloadEmblems = Task.Run(async () => await downloadFilesAsync(as3, "emblems/", sHomeDir + "/../Content/Uploads/", ".png"));

        // Download portraits
        Task<bool> tkDownloadPortraits = Task.Run(async () => await downloadFilesAsync(as3, "Portraits/", sHomeDir + "/../Content/Uploads/", ".png"));

        // Download mails
        Task<bool> tkDownloadMail = Task.Run(async () => await downloadMails(as3, sHomeDir));

        // Download wishlist
        Task<bool> tkDownloadWl = Task.Run(async () => await downloadFileAsync(as3, "wishlist.json", Path.Combine(sHomeDir, "wishlist.json")));

        // Download archive cups
        if (!System.IO.Directory.Exists(Path.Combine(sHomeDir, "archive"))) System.IO.Directory.CreateDirectory(Path.Combine(sHomeDir, "archive"));

        for (int iS = 1; iS < ckcore.iSeason; iS++) {
          string sCupDir = Path.Combine(sHomeDir, "archive", iS.ToString());
          string sCupKey = "archive/" + iS.ToString() + "/Cup";

          if (!System.IO.Directory.Exists(sCupDir)) System.IO.Directory.CreateDirectory(sCupDir);

          try {
            Task<bool> tkDownloadCups = Task.Run(async () => await downloadFileAsync(as3, sCupKey, Path.Combine(sCupDir, "Cup")));
          } catch {
            ckcore.tl.writeLog("ERROR: Unable to download games", CornerkickManager.Main.sErrorFile);
          }
        }

        // Download archive games
        try {
          Task<bool> tkDownloadGames = Task.Run(async () => await downloadFilesAsync(as3, "save/games/", sHomeDir, ".ckgx"));
        } catch {
          ckcore.tl.writeLog("ERROR: Unable to download games", CornerkickManager.Main.sErrorFile);
        }
#endif
#else
        readMails();
#endif

        // Stop stopwatch
        swLoad.Stop();
        TimeSpan tsLoad = swLoad.Elapsed;

        // Write elapsed time to log
        ckcore.tl.writeLog("Elapsed time during load: " + tsLoad.TotalSeconds.ToString("0.000") + "s");

        // If no calendar timer --> enable save timer to save every 15 min.
        timerSave.Enabled = !timerCkCalender.Enabled;

        iLoadState = 0; // Success

        return true;
      }

      // New game
      DateTime dtLeagueStart;
      DateTime dtLeagueEnd;
      ckcore.setSeasonStartEndDates(out dtLeagueStart, out dtLeagueEnd);

      /////////////////////////////////////////////////////////////////////
      // Create nat. Cups and Leagues
      foreach (int iLand in iNations) {
        // Create nat. cup
        CornerkickManager.Cup cup = new CornerkickManager.Cup(bKo: true);
        cup.iId = 2;
        cup.iId2 = iLand;
        cup.sName = "Pokal " + CornerkickManager.Main.sLand[iLand];
        cup.settings.fAttraction = 1.0f;
        cup.settings.iNeutral = 1;
        cup.settings.iBonusCupWin = 8000000; // 8 mio.
        cup.settings.bBonusReleaseCupWinInKo = true;
        ckcore.ltCups.Add(cup);

        // Create league
        CornerkickManager.Cup league = new CornerkickManager.Cup(nGroups: 1, bGroupsTwoGames: true);
        league.iId = 1;
        league.iId2 = iLand;
        league.iId3 = 0;
        league.sName = "Liga " + CornerkickManager.Main.sLand[iLand];
        league.settings.fAttraction = 1.0f;
        ckcore.ltCups.Add(league);

        fillLeaguesWithCpuClubs(league, cup);
      }

      /////////////////////////////////////////////////////////////////////
      // Create Internat. cups
      createCupGold();
      createCupSilver();
      createCupBronze();
      createCupWc(dtLeagueEnd);

      ckcore.calcMatchdays();
      ckcore.tl.getCup(7).draw(ckcore.dtDatum);

      ckcore.dtDatum = ckcore.dtSeasonStart;

      // If error while loading ...
      timerSave.Enabled = false; // ... do not overwrite the file
      settings.bLoginPossible = false; // ... disable user login

      iLoadState = 3; // Error or new game

      return false;
    }

#if _USE_AMAZON_S3
    private static async Task<bool> downloadMails(AmazonS3FileTransfer as3, string sHomeDir)
    {
      as3.downloadAllFiles("mail/", sHomeDir, null, ".txt");
      readMails();

      return true;
    }
#endif

    private static void readMails()
    {
      string sDirMail = System.IO.Path.Combine(ckcore.settings.sHomeDir, "mail");
      if (System.IO.Directory.Exists(sDirMail)) {
        ltMail = new List<Mail>();

        System.IO.DirectoryInfo diMail = new System.IO.DirectoryInfo(sDirMail);

        foreach (var fileMail in diMail.GetFiles("*.txt")) {
          string sContent = System.IO.File.ReadAllText(fileMail.FullName);
          string[] sContentSplit = sContent.Split(new[] { Environment.NewLine }, StringSplitOptions.None);

          string[] sHeader = sContentSplit[0].Split();
          if (sHeader.Length < 3) continue;

          string sToId   = sHeader[0];
          string sFromId = sHeader[1];
          string sDate   = sHeader[2];
          bool bNew      = true;
          if (sHeader.Length > 3) bNew = sHeader[3].Equals("true");

          DateTime dtMail = DateTime.ParseExact(sDate, "yyyyMMddHHmmss", CultureInfo.InvariantCulture);

          Mail mail = new Mail();
          mail.sIdTo = sToId;
          mail.sIdFrom = sFromId;
          mail.bNew = bNew;
          mail.dt = dtMail;
          mail.sText = "";
          for (int iL = 1; iL < sContentSplit.Length; iL++) { // For each line of text
            mail.sText += sContentSplit[iL] + Environment.NewLine;
          }

          ltMail.Add(mail);

          // Delete mail file after import
          try {
            fileMail.Delete();
          } catch {
            ckcore.tl.writeLog("Unable to delete mail: " + fileMail.FullName, CornerkickManager.Main.sErrorFile);
          }
        }
      }
    }

#if _USE_AMAZON_S3
    private static async Task<bool> downloadFileAsync(AmazonS3FileTransfer as3, string sKey, string sFile)
    {
      as3.downloadFile(sKey, sFile);
      return true;
    }

    private static async Task<bool> downloadFilesAsync(AmazonS3FileTransfer as3, string sS3SubDir, string sTargetPath, string sFiles)
    {
      as3.downloadAllFiles(sS3SubDir, sTargetPath, null, sFiles);
      return true;
    }
#endif

    private static async Task<bool> performCalendarStepsAsync(int iGameSpeed, double fInterval, bool bCalendarRunning)
    {
      // Create stopwatch
      System.Diagnostics.Stopwatch swCalSteps = new System.Diagnostics.Stopwatch();

      // Start stopwatch
      swCalSteps.Start();

      Controllers.AdminController.setGameSpeedToAllUsers(0);

      int nSteps = getDeltaStepsBetweenNowAndApproach();

      if (nSteps > 0) {
        iLoadState = 2;

        ckcore.tl.writeLog("Performing approx. " + nSteps.ToString() + " calendar steps");

        int iS = 0;
        while (getCkApproachDate().CompareTo(ckcore.dtDatum) > 0) {
          try {
            bool bBreak = !performCalendarStep(false);
            if (bBreak) {
              bCalendarRunning = false;
              break;
            }
          } catch (Exception e) {
            ckcore.tl.writeLog("performCalendarStepsAsync(): Error in performCalendarStep() at step: " + iS.ToString() + Environment.NewLine + e.Message + e.StackTrace, CornerkickManager.Main.sErrorFile);
          }

          iS++;
        }
      }

      if (iGameSpeed > 0) Controllers.AdminController.setGameSpeedToAllUsers(iGameSpeed);

      timerCkCalender.Interval = fInterval * 1000.0; // Convert [s] to [ms]
      timerCkCalender.Enabled = bCalendarRunning;

      ckcore.tl.writeLog("Calendar Interval set to " + timerCkCalender.Interval.ToString() + " ms");

      // Stop stopwatch
      swCalSteps.Stop();

      // Write elapsed time to log
      ckcore.tl.writeLog("Elapsed time while performing calendar steps: " + swCalSteps.Elapsed.TotalSeconds.ToString("0.000") + "s");

      iLoadState = 0;

      return true;
    }

#if _USE_BLOB
    // Blob Container
    internal static CloudBlobContainer GetCloudBlobContainer()
    {
      string sCcmSetting = CloudConfigurationManager.GetSetting("ckstored");
      CloudStorageAccount storageAccount = CloudStorageAccount.Parse(sCcmSetting);
      CloudBlobClient     blobClient     = storageAccount.CreateCloudBlobClient();
      CloudBlobContainer  container      = blobClient.GetContainerReference("blobContainerSave");

      return container;
    }

    public static bool UploadBlob()
    {
      CloudBlobContainer container = GetCloudBlobContainer();
      CloudBlockBlob blob = container.GetBlockBlobReference("blobSave");

#if DEBUG
      string path = "C:\\Users\\Jan\\Documents\\Visual Studio 2017\\Projects\\Cornerkick.git\\CornerkickWebMvc\\";
#else
      string path = System.Web.HttpContext.Current.Server.MapPath("~/");
#endif

      if (!System.IO.File.Exists(@"C:\\Users\\Jan\\Documents\\Visual Studio 2017\\Projects\\Cornerkick.git\\CornerkickWebMvc\\log\\ck.log")) return false;

      using (var fileStream = System.IO.File.OpenRead(@"C:\\Users\\Jan\\Documents\\Visual Studio 2017\\Projects\\Cornerkick.git\\CornerkickWebMvc\\log\\ck.log")) {
        blob.UploadFromStream(fileStream);
      }

      return true;
    }
#endif

    public static string getHomeDir()
    {
#if _DOCKER
      return "C:\\inetpub\\wwwroot\\";
#else
#if DEBUG
      return "C:\\Users\\Jan\\Documents\\Visual Studio 2017\\Projects\\Cornerkick.git\\CornerkickWebMvc\\";
#else
#if _WebMvc
      if (HttpContext.Current == null) return null;

#if _DEPLOY_ON_APPHB
      return Path.Combine(HttpContext.Current.Server.MapPath("~"), "App_Data");
#endif

      return HttpContext.Current.Server.MapPath("~");
#endif
#endif
#endif

      return null;
    }

  }
}
