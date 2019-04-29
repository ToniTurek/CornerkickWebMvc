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
    public static int iStartHour = -1;
    private static Random random = new Random();

    //const string sSaveZip = "ckSave.zip";
    const string sFilenameSave = ".autosave.ckx";

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

      // Load autosave
      if (!load()) {
        // New game
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
          cup.sName = "Liga " + ckcore.sLand[iLand];
          ckcore.ltCups.Add(league);

          // Only GER (remove to use full iNations)
          break;
        }

        MvcApplication.ckcore.calcMatchdays();
        MvcApplication.ckcore.dtDatum = MvcApplication.ckcore.dtSeasonStart;
      }

      // If no calendar timer --> enable save timer to save every 15 min.
      timerSave.Enabled = !timerCkCalender.Enabled;
    }

    private static void timerCkCalender_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
    {
      // Disable save timer (not needed if calendar timer is on)
      timerSave.Enabled = false;

      if (timerCkCalender.Interval < 1000) timerCkCalender.Enabled = false;

      performCalendarStep();

      timerCkCalender.Enabled = true;
    }

    private static void timerSave_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
    {
      save(timerCkCalender, true);
    }

    private static void performCalendarStep(bool bSave = true)
    {
      if (ckcore.ltUser.Count == 0) return;

      if (iStartHour >= 0 && iStartHour <= 24) {
        if (DateTime.Now.Hour != iStartHour && DateTime.Now.Hour > 13) {
          if (MvcApplication.ckcore.dtDatum.Equals(MvcApplication.ckcore.dtSeasonStart) ||
             ((int)MvcApplication.ckcore.dtDatum.DayOfWeek == 1 && MvcApplication.ckcore.dtDatum.Hour == 0 && MvcApplication.ckcore.dtDatum.Minute == 0)) {
            timerCkCalender.Enabled = true;
            return;
          }
        }
      }

      // Check if new jouth player and put on transferlist
      if (MvcApplication.ckcore.dtDatum.Hour == 0 && MvcApplication.ckcore.dtDatum.Minute == 0) {
        CornerkickManager.Club club0 = MvcApplication.ckcore.ltClubs[0];

        CornerkickGame.Player plNew = MvcApplication.ckcore.plr.newPlayer(club0);
        MvcApplication.ckcore.ui.putPlayerOnTransferlist(plNew.iId, 0);

        // Player jouth
        foreach (int iPlJouthId in club0.ltJugendspielerID) {
          MvcApplication.ckcore.ui.putPlayerOnTransferlist(iPlJouthId, 0);
        }

        if (countCpuPlayerOnTransferlist() > 200) {
          for (int iT = 0; iT < MvcApplication.ckcore.ltTransfer.Count; iT++) {
            CornerkickManager.csTransfer.Transfer transfer = MvcApplication.ckcore.ltTransfer[iT];
            if (club0.ltPlayerId.IndexOf(transfer.iPlayerId) >= 0) {
              MvcApplication.ckcore.tr.removePlayerFromTransferlist(MvcApplication.ckcore.ltPlayer[transfer.iPlayerId]);
              break;
            }
          }
        }

        // retire cpu player
        if (club0.ltPlayerId.Count > 500) {
          MvcApplication.ckcore.plr.retirePlayer(MvcApplication.ckcore.ltPlayer[club0.ltPlayerId[0]]);
        }

        //checkCpuJouth();
      }

      // Save .autosave
      if (bSave && ckcore.dtDatum.Minute == 0 && ckcore.dtDatum.Hour % 2 == 0) {
        foreach (CornerkickManager.Club clb in MvcApplication.ckcore.ltClubs) {
          if (clb.iUser < 0) {
            clb.ltTrainingHist.Clear();
            clb.ltAccount     .Clear();
          }
        }

        save(timerCkCalender);
      }

      if ((MvcApplication.ckcore.dtDatum.Equals(MvcApplication.ckcore.dtSeasonStart) ||
          MvcApplication.ckcore.dtDatum.Year < 1900) &&
          MvcApplication.ckcore.iSaisonCount == 0) {
        MvcApplication.ltLog.Clear();
        MvcApplication.ckcore.setNewSeason();
      }

      int iRetCk = ckcore.next(true);
      if (iRetCk == 99) return; // Saisonende

      // Remove testgame requests if in past
      List<CornerkickManager.Cup> ltCupsTmp = new List<CornerkickManager.Cup>(MvcApplication.ckcore.ltCups);
      foreach (CornerkickManager.Cup cup in ltCupsTmp) {
        if (cup == null) continue;

        if (cup.iId == -5) {
          if (cup.ltMatchdays.Count < 1) continue;

          if (cup.ltMatchdays[0].dt.CompareTo(MvcApplication.ckcore.dtDatum) <= 0) { // if request in past or now ...
            MvcApplication.ckcore.ltCups.Remove(cup); // ... remove cup
          }
        }
      }
    }

    private static int countCpuPlayerOnTransferlist()
    {
      int nPl = 0;
      foreach (CornerkickManager.csTransfer.Transfer transfer in MvcApplication.ckcore.ltTransfer) {
        if (MvcApplication.ckcore.ltClubs[0].ltPlayerId.IndexOf(transfer.iPlayerId) >= 0) nPl++;
      }

      return nPl;
    }

    /*
    private static void checkCpuJouth()
    {
      while (MvcApplication.ckcore.ltClubs[0].ltJugendspielerID.Count > 0) {
        int iPlId = MvcApplication.ckcore.ltClubs[0].ltJugendspielerID[0];
        MvcApplication.ckcore.ltClubs[0].ltJugendspielerID.RemoveAt(0);
        MvcApplication.ckcore.ltClubs[0].ltPlayerId.Add(iPlId);
        MvcApplication.ckcore.ui.putPlayerOnTransferlist(iPlId, 0);
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
        MvcApplication.ckcore.tl.writeLog("save: HttpException: " + exp.Message.ToString());
#if _DEPLOY_ON_AZURE
        sHomeDir = "D:\\home\\site\\wwwroot";
#endif
      } catch {
        MvcApplication.ckcore.tl.writeLog("save: unable to create sHomeDir from Server.MapPath", MvcApplication.ckcore.sErrorFile);
#if _DEPLOY_ON_AZURE
        sHomeDir = "D:\\home\\site\\wwwroot";
#endif
      }
#endif

#if _USE_AMAZON_S3
      AmazonS3FileTransfer as3 = new AmazonS3FileTransfer();
#endif

      string sFilenameSave2 = ".autosave_" + MvcApplication.ckcore.dtDatum.ToString("yyyy-MM-dd_HH-mm") + ".ckx";
      string sFileSave2 = Path.Combine(sHomeDir, "save", sFilenameSave2);
      MvcApplication.ckcore.tl.writeLog("save file: " + sFileSave2);

      try {
        MvcApplication.ckcore.io.save(sFileSave2);
        as3.uploadFile(sFileSave2, sFilenameSave2, "application/zip");
      } catch {
        MvcApplication.ckcore.tl.writeLog("ERROR: could not save to file " + sFileSave2, MvcApplication.ckcore.sErrorFile);
      }

      // Copy autosave file with datum to basic one (could use file link)
      string sFileSave = sHomeDir + "/save/" + sFilenameSave;
      if (System.IO.File.Exists(sFileSave)) {
        try {
          System.IO.File.Delete(sFileSave);
        } catch {
        }
      }

      System.IO.File.Copy(sFileSave2, sFileSave);
      as3.uploadFile(sFileSave, sFilenameSave, "application/zip");

      /*
      if (System.IO.Directory.Exists(sHomeDir + "/save")) {
#if _USE_AMAZON_S3
        string sFileZipSave = sHomeDir + "/" + sSaveZip;
#else
        string sFileZipSave = sHomeDir + "/" + sSaveZip;
#endif

        // Delete existing zip file
        if (System.IO.File.Exists(sFileZipSave)) {
          try {
            System.IO.File.Delete(sFileZipSave);
          } catch {
          }
        }

        ZipFile.CreateFromDirectory(sHomeDir + "/save", sFileZipSave);

#if !DEBUG
        try {
#if _USE_BLOB
        CornerkickWebMvc.Controllers.BlobsController bcontr = new Controllers.BlobsController();
        bcontr.uploadBlob("blobSave", sFileSave2);
#endif
#if _USE_AMAZON_S3
          as3.uploadFile(sFileZipSave, "ckSave", "application/zip");
#endif
        } catch {
#if _USE_BLOB
        MvcApplication.ckcore.tl.writeLog("ERROR: could not upload file " + sFileSave2 + " to blob", MvcApplication.ckcore.sErrorFile);
#endif
#if _USE_AMAZON_S3
          MvcApplication.ckcore.tl.writeLog("ERROR: could not upload file " + sFileZipSave + " to amazon s3", MvcApplication.ckcore.sErrorFile);
#endif
        }
#endif
      }
      */

      // Write last ck state to file
      saveLaststate(sHomeDir);

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
          MvcApplication.ckcore.tl.writeLog("ERROR: could not upload log file to blob", MvcApplication.ckcore.sErrorFile);
#endif
#if _USE_AMAZON_S3
          MvcApplication.ckcore.tl.writeLog("ERROR: could not upload log file to amazon s3", MvcApplication.ckcore.sErrorFile);
#endif
        }
#endif
      }
    }

    internal static void saveLaststate(string sTargetDir)
    {
      string sFileLastState = Path.Combine(sTargetDir, "laststate.txt");
      using (System.IO.StreamWriter fileLastState = new System.IO.StreamWriter(sFileLastState)) {
        fileLastState.WriteLine((timerCkCalender.Interval / 1000.0).ToString("g", CultureInfo.InvariantCulture));
        fileLastState.WriteLine(timerCkCalender.Enabled.ToString());
        fileLastState.WriteLine(DateTime.Now.ToString("s", CultureInfo.InvariantCulture));

        int iGameSpeed = 0;
        if (MvcApplication.ckcore.ltUser.Count > 0 && MvcApplication.ckcore.ltUser[0].nextGame != null) iGameSpeed = MvcApplication.ckcore.ltUser[0].nextGame.iGameSpeed;
        fileLastState.WriteLine(iGameSpeed.ToString());

        fileLastState.Close();
      }

#if _USE_AMAZON_S3
      AmazonS3FileTransfer as3 = new AmazonS3FileTransfer();
      as3.uploadFile(sFileLastState, "laststate");
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
          MvcApplication.ckcore.tl.writeLog("ERROR: Unable to download file " + sFilenameSave + " to: " + sFileLoad, MvcApplication.ckcore.sErrorFile);
        }
        /*
        if (Directory.Exists(sHomeDir + "save")) {
          try {
            Directory.Delete(sHomeDir + "save", true);
          } catch {
            MvcApplication.ckcore.tl.writeLog("ERROR: unable to delete existing temp. load directory: " + sHomeDir + "save", MvcApplication.ckcore.sErrorFile);
          }
        }

        Directory.CreateDirectory(sHomeDir + "save");
        if (System.IO.File.Exists(sHomeDir + sSaveZip)) ZipFile.ExtractToDirectory(sHomeDir + sSaveZip, sHomeDir + "save");
        */
      }

      // Download log
      as3.downloadFile("ckLog", sHomeDir + "/log.zip");
#endif
#endif

      // Load ck state
      if (MvcApplication.ckcore.io.load(sFileLoad)) {
        MvcApplication.ckcore.tl.writeLog("File " + sFileLoad + " loaded");

        // Set admin user to CPU
        if (MvcApplication.ckcore.ltClubs.Count > 0) MvcApplication.ckcore.ltClubs[0].iUser = -1;

        // TEMP: Set cup names
        for (int iC = 0; iC < ckcore.ltCups.Count; iC++) {
          if      (ckcore.ltCups[iC].iId == 1) ckcore.ltCups[iC].sName = "Liga "  + ckcore.sLand[ckcore.ltCups[iC].iId2];
          else if (ckcore.ltCups[iC].iId == 2) ckcore.ltCups[iC].sName = "Pokal " + ckcore.sLand[ckcore.ltCups[iC].iId2];
        }

        // TEMP: Set stadium name for fiffi
        MvcApplication.ckcore.ltClubs[2].stadium.sName = "Stadion an der Busseallee";

        // TEMP: Clear sponsor offers
        for (int iC = 0; iC < ckcore.ltClubs.Count; iC++) {
          ckcore.ltClubs[iC].ltSponsorOffers.Clear();
        }

        string sFileLastState = Path.Combine(sHomeDir, "laststate.txt");
#if !DEBUG
#if _USE_AMAZON_S3
        if (!System.IO.File.Exists(sFileLastState)) as3.downloadFile("laststate", sFileLastState);
#endif

        if (System.IO.File.Exists(sFileLastState)) {
          MvcApplication.ckcore.tl.writeLog("Reading laststate from file: " + sFileLastState);

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

              MvcApplication.ckcore.tl.writeLog("Last step was at " + dtLast.ToString("s", CultureInfo.InvariantCulture) + " (now: " + DateTime.Now.ToString("s", CultureInfo.InvariantCulture) + ")");
              if (nSteps > 0) {
                MvcApplication.ckcore.tl.writeLog("Performing " + nSteps.ToString() + " calendar steps");
                for (int iS = 0; iS < nSteps; iS++) performCalendarStep(false);
              }

              int iGameSpeed = 0; // Calendar interval [s]
              int.TryParse(sStateFileContent[3], out iGameSpeed);

              if (iGameSpeed > 0) adminController.setGameSpeedToAllUsers(iGameSpeed);

              timerCkCalender.Interval = fInterval * 1000.0; // Convert [s] to [ms]
              timerCkCalender.Enabled  = bCalendarRunning;

              MvcApplication.ckcore.tl.writeLog("Calendar Interval set to " + timerCkCalender.Interval.ToString() + " ms");
            }
          }
        } else {
          MvcApplication.ckcore.tl.writeLog("laststate file '" + sFileLastState + "' does not exist");
        }
      
        // Download emblems
#if _USE_AMAZON_S3
        for (int iC = 0; iC < MvcApplication.ckcore.ltClubs.Count; iC++) as3.downloadFile("ckEmblem_" + iC.ToString(), sHomeDir + "/../Content/Uploads/" + iC.ToString() + ".png");
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
