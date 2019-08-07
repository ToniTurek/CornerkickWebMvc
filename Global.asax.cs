//#define _USE_BLOB
#define _USE_AMAZON_S3
//#define _DEPLOY_ON_AZURE
#define _DEPLOY_ON_APPHB

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using System.Web.UI.WebControls;

#if _USE_BLOB
using Microsoft.Azure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
#endif

namespace CornerkickWebMvc
{
  public class MvcApplication : System.Web.HttpApplication
  {
    public static CornerkickManager.Main ckcore;
    public static System.Timers.Timer timerCkCalender = null;
    public static System.Timers.Timer timerSave = null;
    public static List<string> ltLog = new List<string>();
    private static Random random = new Random();
    public static Settings settings = new Settings();

    public class Settings
    {
      public int  iStartHour;
      public bool bEmailCertification;
      public bool bRegisterDuringGame;

      public Settings()
      {
        iStartHour = -1;
        bEmailCertification = true;
        bRegisterDuringGame = true;
      }
    }

    //const string sSaveZip = "ckSave.zip";
    const string sFilenameSave     = ".autosave.ckx";
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

    protected void Application_Start()
    {
      AreaRegistration.RegisterAllAreas();
      FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
      RouteConfig.RegisterRoutes(RouteTable.Routes);
      BundleConfig.RegisterBundles(BundleTable.Bundles);

      try {
        newCk();
      } catch {
      }
    }

    internal static void newCk()
    {
      string sHomeDir = getHomeDir();

      ckcore = new CornerkickManager.Main();

#if !DEBUG
      ckcore.sHomeDir = sHomeDir;
#endif

      ckcore.tl.writeLog("WebMvc START");

      Models.RegisterViewModel.ltLand.Clear();
      foreach (byte iN in iNations) Models.RegisterViewModel.ltLand.Add(new SelectListItem { Text = ckcore.sLand[iN], Value = iN.ToString(), Selected = iN == 36 });

      Models.RegisterViewModel.ltSpKl.Clear();
      Models.RegisterViewModel.ltSpKl.Add(new SelectListItem { Text = "Liga 1", Value = "1", Selected = true });

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

      // Create stopwatch
      System.Diagnostics.Stopwatch swLoad = new System.Diagnostics.Stopwatch();

      // Start stopwatch
      swLoad.Start();

      // Load autosave
      if (load()) {
        // Add clubs to ddl in register view (need to be relocated if multiple leagues are available)
        Models.RegisterViewModel.ltClubs.Clear();

        for (int iC = 1; iC < ckcore.ltClubs.Count; iC++) {
          CornerkickManager.Club clb = ckcore.ltClubs[iC];

          if (clb.bNation) continue;
          if (clb.user != null) continue;

          if (clb.iLand != iNations[0]) continue; // Remove if all leagues are available

          Models.RegisterViewModel.ltClubs.Add(new SelectListItem { Text = clb.sName, Value = iC.ToString() });
        }
      } else {
        // New game
        DateTime dtLeagueStart;
        DateTime dtLeagueEnd;
        MvcApplication.ckcore.setSeasonStartEndDates(out dtLeagueStart, out dtLeagueEnd);

        /////////////////////////////////////////////////////////////////////
        // Create nat. Cups and Leagues
        foreach (int iLand in iNations) {
          // Create nat. cup
          CornerkickManager.Cup cup = new CornerkickManager.Cup(bKo: true);
          cup.iId = 2;
          cup.iId2 = iLand;
          cup.sName = "Pokal " + ckcore.sLand[iLand];
          ckcore.ltCups.Add(cup);

          // Create league
          CornerkickManager.Cup league = new CornerkickManager.Cup(nGroups: 1, bGroupsTwoGames: true);
          league.iId = 1;
          league.iId2 = iLand;
          league.iId3 = 0;
          league.sName = "Liga " + ckcore.sLand[iLand];
          ckcore.ltCups.Add(league);

          fillLeaguesWithCpuClubc(league, cup);
        }

        /////////////////////////////////////////////////////////////////////
        // Create Internat. cups
        createCupGold();
        createCupSilver();
        createCupWc(dtLeagueEnd);

        ckcore.calcMatchdays();
        MvcApplication.ckcore.drawCup(MvcApplication.ckcore.tl.getCup(7));

        ckcore.dtDatum = ckcore.dtSeasonStart;
      }

      // Stop stopwatch
      swLoad.Stop();
      TimeSpan tsLoad = swLoad.Elapsed;

      // Write elapsed time to log
      ckcore.tl.writeLog("Elapsed time during load: " + tsLoad.TotalSeconds.ToString("0.000") + "s");

      // If no calendar timer --> enable save timer to save every 15 min.
      timerSave.Enabled = !timerCkCalender.Enabled;
    }

    private static void fillLeaguesWithCpuClubc(CornerkickManager.Cup league, CornerkickManager.Cup cup, byte nLeagueSize = 16)
    {
      Controllers.AccountController accountController = new Controllers.AccountController();

      int iC = 0;
      while (league.ltClubs[0].Count < nLeagueSize) {
        CornerkickManager.Club clb = accountController.createClub("Team_" + MvcApplication.ckcore.sLand[league.iId2] + "_" + (iC + 1).ToString(), (byte)league.iId2, 0);
        MvcApplication.ckcore.ltClubs.Add(clb);

        cup   .ltClubs[0].Add(clb);
        league.ltClubs[0].Add(clb);

        iC++;
      }
    }

    private static void createCupGold()
    {
      CornerkickManager.Cup cupGold = MvcApplication.ckcore.tl.getCup(3);

      if (cupGold == null) {
        // Create Gold Cup
        cupGold = new CornerkickManager.Cup(bKo: true, bKoTwoGames: true, nGroups: 8, nQualifierKo: 2);
        cupGold.iId = 3;
        cupGold.sName = "Gold Cup";
        cupGold.settings.iNeutral = 2;
        cupGold.settings.iBonusCupWin = 30000000; // 30 mio.
        cupGold.settings.bBonusReleaseCupWinInKo = true;
        cupGold.settings.iDayOfWeek = 3;
        MvcApplication.ckcore.ltCups.Add(cupGold);
      }
    }

    private static void createCupSilver()
    {
      CornerkickManager.Cup cupSilver = MvcApplication.ckcore.tl.getCup(4);

      if (cupSilver == null) {
        // Create Silver Cup
        cupSilver = new CornerkickManager.Cup(bKo: true, bKoTwoGames: true, nGroups: 8, nQualifierKo: 2);
        cupSilver.iId = 4;
        cupSilver.sName = "Silver Cup";
        cupSilver.settings.iNeutral = 2;
        cupSilver.settings.iBonusCupWin = 15000000; // 15 mio.
        cupSilver.settings.bBonusReleaseCupWinInKo = true;
        cupSilver.settings.iDayOfWeek = 4;
        MvcApplication.ckcore.ltCups.Add(cupSilver);
      }
    }

    private static void createCupWc(DateTime dtLeagueEnd)
    {
      CornerkickManager.Cup cupWc = MvcApplication.ckcore.tl.getCup(7);

      if (cupWc == null) {
        cupWc = new CornerkickManager.Cup(bKo: true, bKoTwoGames: false, nGroups: 2, bGroupsTwoGames: false, nQualifierKo: 2);
        cupWc.iId = 7;
        cupWc.sName = "World Cup";
        cupWc.settings.iNeutral = 1;
        cupWc.settings.dtStart = dtLeagueEnd.Date + new TimeSpan(20, 30, 00);
        cupWc.settings.dtEnd = MvcApplication.ckcore.dtSeasonEnd.AddDays(-1).Date + new TimeSpan(20, 00, 00);
        MvcApplication.ckcore.ltCups.Add(cupWc);

        int iGroup = 0;
        foreach (byte iN in MvcApplication.iNations) {
          CornerkickManager.Club clbNat = new CornerkickManager.Club();
          clbNat.bNation = true;
          clbNat.iId = MvcApplication.ckcore.ltClubs.Count;
          clbNat.sName = MvcApplication.ckcore.sLand[iN];
          clbNat.iLand = iN;
          clbNat.formation = MvcApplication.ckcore.ltFormationen[8];

          MvcApplication.ckcore.ltClubs.Add(clbNat);

          MvcApplication.ckcore.doFormation(clbNat.iId);

          cupWc.ltClubs[iGroup / 4].Add(clbNat);
          iGroup++;
        }
      }
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

    private static bool performCalendarStep(bool bSave = true)
    {
      if (ckcore.ltUser.Count == 0) return true;

      if (settings.iStartHour >= 0 && settings.iStartHour <= 24) {
        if (DateTime.Now.Hour != settings.iStartHour && DateTime.Now.Hour > 13) {
          if (ckcore.dtDatum.Equals(ckcore.dtSeasonStart) ||
             ((int)ckcore.dtDatum.DayOfWeek == 1 && ckcore.dtDatum.Hour == 0 && ckcore.dtDatum.Minute == 0)) {
            return true;
          }
        }
      }

      // Check if new jouth player and put on transferlist
      if (ckcore.dtDatum.Hour == 0 && ckcore.dtDatum.Minute == 0) {
        CornerkickManager.Club club0 = ckcore.ltClubs[0];

        CornerkickGame.Player plNew = ckcore.plr.newPlayer(club0);
        ckcore.ui.putPlayerOnTransferlist(plNew.iId, 0);

        // Player jouth
        foreach (CornerkickGame.Player plJ in club0.ltPlayerJouth) {
          ckcore.ui.putPlayerOnTransferlist(plJ, 0);
        }

        if (countCpuPlayerOnTransferlist() > 200) {
          for (int iT = 0; iT < ckcore.ltTransfer.Count; iT++) {
            CornerkickManager.Transfer.Item transfer = ckcore.ltTransfer[iT];
            if (club0.ltPlayer.IndexOf(transfer.player) >= 0) {
              ckcore.tr.removePlayerFromTransferlist(transfer.player);
              break;
            }
          }
        }

        // retire cpu player
        if (club0.ltPlayer.Count > 500) {
          ckcore.plr.retirePlayer(club0.ltPlayer[0]);
        }

        //checkCpuJouth();
      }

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
      int iRetCk = ckcore.next(true);

      // End of season
      if (iRetCk == 99) {
        CornerkickManager.Cup cupGold   = ckcore.tl.getCup(3);
        CornerkickManager.Cup cupSilver = ckcore.tl.getCup(4);

        for (byte iG = 0; iG < cupGold  .ltClubs.Length; iG++) cupGold  .ltClubs[iG].Clear();
        for (byte iG = 0; iG < cupSilver.ltClubs.Length; iG++) cupSilver.ltClubs[iG].Clear();

        // Add clubs ...
        int iGroupGold   = 0;
        int iGroupSilver = 0;
        foreach (int iN in iNations) {
          // ... of league iN ...
          CornerkickManager.Cup league = MvcApplication.ckcore.tl.getCup(1, iN, 0);
          if (league == null) continue;

          List<CornerkickManager.Tool.TableItem> ltTbl = MvcApplication.ckcore.tl.getLeagueTable(league);

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
        }

        MvcApplication.ckcore.calcMatchdays();

        MvcApplication.ckcore.drawCup(cupGold);
        MvcApplication.ckcore.drawCup(cupSilver);

        return false;
      }

      // Remove testgame requests if in past
      List<CornerkickManager.Cup> ltCupsTmp = new List<CornerkickManager.Cup>(ckcore.ltCups);
      foreach (CornerkickManager.Cup cup in ltCupsTmp) {
        if (cup == null) continue;

        if (cup.iId == -5) {
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
          if (transfer.player.iClubId <                     0) continue;
          if (transfer.player.iClubId >= ckcore.ltClubs.Count) continue;

          CornerkickManager.Club clbCpu = ckcore.ltClubs[transfer.player.iClubId];
          if (clbCpu.user != null) continue; // If human user
          if (MvcApplication.ckcore.ltUser.IndexOf(clbCpu.user) == 0) continue; // If main CPU user

          // Get max offer
          int iOfferMax = 0;
          foreach (CornerkickManager.Transfer.Offer offer in transfer.ltOffers) {
            iOfferMax = Math.Max(iOfferMax, offer.iFee);
          }

          // Inform users
          foreach (CornerkickManager.Transfer.Offer offer in transfer.ltOffers) {
            if (offer.iFee < iOfferMax) {
              ckcore.Info(offer.club.user, "Ihr Transferangebot für den Spieler " + transfer.player.sName + " ist leider nicht (mehr) hoch genug.");
            }
          }
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

                while (nat.ltPlayer.Count > nPlayerNat) nat.ltPlayer.RemoveAt(nPlayerNat);

                // Set player no.
                for (int iP = 0; iP < nat.ltPlayer.Count; iP++) nat.ltPlayer[iP].iNrNat = (byte)(iP + 1);
              }
            }

            // Remove national coaches
            CornerkickManager.Cup.Matchday mdWcFinal = cupWc.ltMatchdays[cupWc.ltMatchdays.Count - 1];
            if (mdWcFinal.ltGameData.Count == 1 && ckcore.dtDatum.Equals(mdWcFinal.dt.AddDays(1))) { // Final game
              foreach (CornerkickManager.User usrNat in ckcore.ltUser) usrNat.iNat = -1;
              foreach (CornerkickManager.Club nat in ckcore.ltClubs) {
                if (nat.bNation) nat.user = null;
              }
            }
          }
        }
      }

      // Nominate user for WC
      bool bReturn = true;
      foreach (CornerkickManager.Cup league in ckcore.ltCups) { // For each 1st division league
        if (league.iId  != 1) continue;
        if (league.iId3 >  0) continue;
        if (league.ltMatchdays == null) continue;
        if (league.ltMatchdays.Count < 2) continue;

        if (ckcore.dtDatum.Equals(league.ltMatchdays[league.ltMatchdays.Count - 1].dt.AddDays(1))) {
          List<CornerkickManager.Tool.TableItem> tbl = ckcore.tl.getLeagueTable(league);
          foreach (CornerkickManager.Tool.TableItem item in tbl) {
            if (item.club.user != null) {
              if (MvcApplication.ckcore.ltUser.IndexOf(item.club.user) == 0) continue; // If main CPU user

              item.club.user.iNat = league.iId2;

              // Add all player of that nation
              CornerkickManager.Club nat = ckcore.tl.getNation(league.iId2);
              nat.ltPlayer = ckcore.getBestPlayer(league.iId2);
              nat.user = item.club.user;

              // Inform user
              if (dtWcSelectPlayerFinish.CompareTo(ckcore.dtDatum) > 0) ckcore.Info(item.club.user, "Bitte wählen Sie noch bis zum " + dtWcSelectPlayerFinish.ToString("d", Controllers.MemberController.getCiStatic(league.iId2)) + " Ihre " + nPlayerNat.ToString() + " Spieler für die Endrunde aus.");
              ckcore.Info(item.club.user, "Welche Ehre! Der Verband von " + nat.sName + " stellt Sie als Nationaltrainer für die kommende WM ein.");

              bReturn = false;
              break;
            }
          }
        }
      }

      return bReturn;
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

    internal static void save(System.Timers.Timer timerCkCalender, bool bForce = false)
    {
      // Don't save if calendar to fast
      if (timerCkCalender.Interval < 10000 && !bForce) return;

      string sHomeDir = ckcore.sHomeDir;
      if (string.IsNullOrEmpty(sHomeDir)) sHomeDir = getHomeDir();

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
        ckcore.tl.writeLog("save: unable to create sHomeDir from Server.MapPath", ckcore.sErrorFile);
#if _DEPLOY_ON_AZURE
        sHomeDir = "D:\\home\\site\\wwwroot";
#endif
      }
#endif

#if _USE_AMAZON_S3
      AmazonS3FileTransfer as3 = new AmazonS3FileTransfer();
#endif

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
      } catch {
        ckcore.tl.writeLog("ERROR: could not save to file " + sFileSave2, ckcore.sErrorFile);
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

#if _USE_AMAZON_S3
      // Upload games
      DirectoryInfo diGames = new DirectoryInfo(Path.Combine(sHomeDir, "save", "games"));
      ckcore.tl.writeLog("Directory info games: '" + diGames.FullName + "'. Exist: " + diGames.Exists.ToString());
      if (diGames.Exists) {
        FileInfo[] ltCkgFiles = diGames.GetFiles("*.ckgx");
        ckcore.tl.writeLog("File info games length: " + ltCkgFiles.Length.ToString());
        foreach (FileInfo ckg in ltCkgFiles) {
          DateTime dtGame;

          string[] sFilenameData = Path.GetFileNameWithoutExtension(ckg.Name).Split('_');
          if (sFilenameData.Length < 3) continue;

          if (!DateTime.TryParseExact(sFilenameData[0], "yyyyMMdd-HHmm", CultureInfo.InvariantCulture, DateTimeStyles.None, out dtGame)) continue;
          if (dtGame.CompareTo(dtLoadCk) < 0) continue; // If game was already present when ck was started

          string sFileGameSave = Path.Combine(sHomeDir, "save", "games", ckg.Name);
          as3.uploadFile(sFileGameSave, "save/games/" + ckg.Name, "application/zip");
        }
      }
#endif

      // save log dir
      if (System.IO.Directory.Exists(sHomeDir + "/log")) {
        string sFileZipLog = sHomeDir + "/log.zip";

#if !DEBUG
        // Delete existing zip file
        if (System.IO.File.Exists(sFileZipLog)) {
          try {
            System.IO.File.Delete(sFileZipLog);
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
          as3.uploadFile(sFileZipLog, "ckLog", "application/zip");
#endif
        } catch {
#if _USE_BLOB
          ckcore.tl.writeLog("ERROR: could not upload log file to blob", ckcore.sErrorFile);
#endif
#if _USE_AMAZON_S3
          ckcore.tl.writeLog("ERROR: could not upload log file to amazon s3", ckcore.sErrorFile);
#endif
        }
#endif
      }
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

        fileSettings.Close();
      }

#if _USE_AMAZON_S3
      AmazonS3FileTransfer as3 = new AmazonS3FileTransfer();
      as3.uploadFile(sFileSettings, "laststate");
#endif
    }

    internal static bool load()
    {
      string sHomeDir = getHomeDir();

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
          ckcore.tl.writeLog("ERROR: Unable to download file " + sFilenameSave + " to: " + sFileLoad, ckcore.sErrorFile);
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

      try {
        as3.downloadAllFiles("save/games/", sHomeDir, null, ".ckgx");
      } catch {
        ckcore.tl.writeLog("ERROR: Unable to download games", ckcore.sErrorFile);
      }

      // Download log
      as3.downloadFile("ckLog", sHomeDir + "/log.zip");
#endif
#endif

      // Load ck state
      if (ckcore.io.load(sFileLoad)) {
        ckcore.tl.writeLog("File " + sFileLoad + " loaded. Elapsed time: ");

        // Set admin user to CPU
        if (ckcore.ltClubs.Count > 0) ckcore.ltClubs[0].user = null;

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
            double fInterval = 0.0; // Calendar interval [s]

            NumberStyles style = NumberStyles.Number | NumberStyles.AllowDecimalPoint;
            fInterval = double.Parse(sStateFileContent[0], style, CultureInfo.InvariantCulture);

            bool bCalendarRunning = false;
            bool.TryParse(sStateFileContent[1], out bCalendarRunning);

            if (fInterval > 10.0 && bCalendarRunning && DateTime.TryParse(sStateFileContent[2], out dtLast)) {
              Controllers.AdminController adminController = new Controllers.AdminController();

              adminController.setGameSpeedToAllUsers(0);

              double fTotalMin = (DateTime.Now - dtLast).TotalMinutes;
              int nSteps = (int)(fTotalMin / (fInterval / 60f));

              ckcore.tl.writeLog("Last step was at " + dtLast.ToString("s", CultureInfo.InvariantCulture) + " (now: " + DateTime.Now.ToString("s", CultureInfo.InvariantCulture) + ")");
              if (nSteps > 0) {
                ckcore.tl.writeLog("Performing " + nSteps.ToString() + " calendar steps");
                for (int iS = 0; iS < nSteps; iS++) performCalendarStep(false);
              }

              int iGameSpeed = 0; // Calendar interval [s]
              int.TryParse(sStateFileContent[3], out iGameSpeed);

              if (iGameSpeed > 0) adminController.setGameSpeedToAllUsers(iGameSpeed);

              timerCkCalender.Interval = fInterval * 1000.0; // Convert [s] to [ms]
              timerCkCalender.Enabled  = bCalendarRunning;

              ckcore.tl.writeLog("Calendar Interval set to " + timerCkCalender.Interval.ToString() + " ms");
            }

            if (sStateFileContent.Length > 5) {
              bool.TryParse(sStateFileContent[4], out settings.bEmailCertification);
              bool.TryParse(sStateFileContent[5], out settings.bRegisterDuringGame);
            }
          }
        } else {
          ckcore.tl.writeLog("laststate file '" + sFileLastState + "' does not exist");
        }

        // Download emblems
#if _USE_AMAZON_S3
        as3.downloadAllFiles("emblems/", sHomeDir + "/../Content/Uploads/", null, ".png");
#endif
#endif

        return true;
      }

      return false;
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
#if DEBUG
      return "C:\\Users\\Jan\\Documents\\Visual Studio 2017\\Projects\\Cornerkick.git\\CornerkickWebMvc\\";
#else

      if (HttpContext.Current == null) return "./App_Data";

#if _DEPLOY_ON_APPHB
      return Path.Combine(HttpContext.Current.Server.MapPath("~"), "App_Data");
#endif

      return HttpContext.Current.Server.MapPath("~");
#endif
    }
  }
}
