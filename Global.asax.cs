//#define _USE_BLOB
#define _USE_AMAZON_S3
//#define _DEPLOY_ON_AZURE
#define _DEPLOY_ON_APPHB

using System;
using System.Collections.Generic;
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
    public static CornerkickCore.Core ckcore;
    public static System.Timers.Timer timerCkCalender = null;
    public static List<string> ltLog = new List<string>();
    public static int iStartHour = -1;
    private static Random random = new Random();

    //const string sSaveZip = "ckSave.zip";
    const string sFilenameSave = ".autosave.ckx";

    protected void Application_Start()
    {
      AreaRegistration.RegisterAllAreas();
      FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
      RouteConfig.RegisterRoutes(RouteTable.Routes);
      BundleConfig.RegisterBundles(BundleTable.Bundles);

      newCk();
    }

    internal static void newCk()
    {
      string sHomeDir = getHomeDir();

      ckcore = new CornerkickCore.Core();

#if !DEBUG
      ckcore.sHomeDir = sHomeDir;
#endif

      ckcore.tl.writeLog("WebMvc START");

      Models.RegisterViewModel.ltLand.Clear();
      Models.RegisterViewModel.ltLand.Add(new SelectListItem { Text = ckcore.sLand[ckcore.iNatUmrechnung[1]], Value = "1", Selected = true });

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

      // Load autosave
      load();
    }

    private static void timerCkCalender_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
    {
      if (timerCkCalender.Interval < 1000) timerCkCalender.Enabled = false;

      performCalendarStep();

      timerCkCalender.Enabled = true;
    }

    private static void performCalendarStep(bool bSave = true)
    {
      if (ckcore.ltUser.Count == 0) return;

      if (iStartHour >= 0 && iStartHour <= 24) {
        if (DateTime.Now.Hour != iStartHour && DateTime.Now.Hour > 13) {
          if (MvcApplication.ckcore.dtDatum.Equals(MvcApplication.ckcore.dtSaisonstart) ||
             ((int)MvcApplication.ckcore.dtDatum.DayOfWeek == 1 && MvcApplication.ckcore.dtDatum.Hour == 0 && MvcApplication.ckcore.dtDatum.Minute == 0)) {
            timerCkCalender.Enabled = true;
            return;
          }
        }
      }

      // Check if new jouth player and put on transferlist
      if (MvcApplication.ckcore.dtDatum.Hour == 0 && MvcApplication.ckcore.dtDatum.Minute == 0) {
        CornerkickCore.Core.Club club0 = MvcApplication.ckcore.ltClubs[0];

        CornerkickGame.Player plNew = MvcApplication.ckcore.plr.newPlayer(club0);
        MvcApplication.ckcore.ui.putPlayerOnTransferlist(plNew.iId, 0);

        // Player jouth
        foreach (int iPlJouthId in club0.ltJugendspielerID) {
          MvcApplication.ckcore.ui.putPlayerOnTransferlist(iPlJouthId, 0);
        }

        if (countCpuPlayerOnTransferlist() > 200) {
          for (int iT = 0; iT < MvcApplication.ckcore.ltTransfer.Count; iT++) {
            CornerkickCore.csTransfer.Transfer transfer = MvcApplication.ckcore.ltTransfer[iT];
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
      if (bSave) {
        MvcApplication.ckcore.ltClubs[0].ltTrainingHist.Clear();
        MvcApplication.ckcore.ltClubs[0].ltAccount.Clear();
        save(timerCkCalender);
      }

      if ((MvcApplication.ckcore.dtDatum.Equals(MvcApplication.ckcore.dtSaisonstart) ||
          MvcApplication.ckcore.dtDatum.Year < 1900) &&
          MvcApplication.ckcore.iSaisonCount == 0) {
        MvcApplication.ltLog.Clear();
        MvcApplication.ckcore.setNewSeason();
      }

      int iRetCk = ckcore.next(true);
      if (iRetCk == 99) return; // Saisonende
    }

    private static int countCpuPlayerOnTransferlist()
    {
      int nPl = 0;
      foreach (CornerkickCore.csTransfer.Transfer transfer in MvcApplication.ckcore.ltTransfer) {
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
      string sFileLastState = Path.Combine(sHomeDir, "laststate.txt");
      using (System.IO.StreamWriter fileLastState = new System.IO.StreamWriter(sFileLastState)) {
        fileLastState.WriteLine((timerCkCalender.Interval / 1000.0).ToString("g", System.Globalization.CultureInfo.InvariantCulture));
        fileLastState.WriteLine(timerCkCalender.Enabled.ToString());
        fileLastState.WriteLine(DateTime.Now.ToString("s", System.Globalization.CultureInfo.InvariantCulture));
        fileLastState.WriteLine(MvcApplication.ckcore.ltUser[0].nextGame.iGameSpeed.ToString());
        fileLastState.Close();
      }

#if _USE_AMAZON_S3
      as3.uploadFile(sFileLastState, "laststate");
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
          MvcApplication.ckcore.tl.writeLog("ERROR: could not upload log file to blob", MvcApplication.ckcore.sErrorFile);
#endif
#if _USE_AMAZON_S3
          MvcApplication.ckcore.tl.writeLog("ERROR: could not upload log file to amazon s3", MvcApplication.ckcore.sErrorFile);
#endif
        }
#endif
      }
    }

    internal static void load()
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
            sStateFileContent[0].Replace(',', '.');
            double fInterval = 0; // Calendar interval [s]
            double.TryParse(sStateFileContent[0], out fInterval);

            bool bCalendarRunning = false;
            bool.TryParse(sStateFileContent[1], out bCalendarRunning);

            if (fInterval > 0.0 && bCalendarRunning && DateTime.TryParse(sStateFileContent[2], out dtLast)) {
              Controllers.AdminController adminController = new Controllers.AdminController();

              adminController.setGameSpeedToAllUsers(0);

              double fTotalMin = (DateTime.Now - dtLast).TotalMinutes;
              int nSteps = (int)(fTotalMin / (fInterval / 60f));

              MvcApplication.ckcore.tl.writeLog("Last step was at " + dtLast.ToString("s", System.Globalization.CultureInfo.InvariantCulture) + " (now: " + DateTime.Now.ToString("s", System.Globalization.CultureInfo.InvariantCulture) + ")");
              if (nSteps > 0) {
                MvcApplication.ckcore.tl.writeLog("Performing " + nSteps.ToString() + " calendar steps");
                for (int iS = 0; iS < nSteps; iS++) performCalendarStep(false);
              }

              int iGameSpeed = 0; // Calendar interval [s]
              int.TryParse(sStateFileContent[3], out iGameSpeed);

              if (iGameSpeed > 0) adminController.setGameSpeedToAllUsers(iGameSpeed);

              timerCkCalender.Interval = fInterval * 1000;
              timerCkCalender.Enabled  = bCalendarRunning;
            }
          }
        } else {
          MvcApplication.ckcore.tl.writeLog("laststate file '" + sFileLastState + "' does not exist");
        }
#endif
      } else {
        MvcApplication.ckcore.calcSpieltage();
        MvcApplication.ckcore.dtDatum = MvcApplication.ckcore.dtSaisonstart;
      }

#if !DEBUG
#if _USE_AMAZON_S3
      // Download emblems
      for (int iC = 0; iC < MvcApplication.ckcore.ltClubs.Count; iC++) as3.downloadFile("ckEmblem_" + iC.ToString(), sHomeDir + "/../Content/Uploads/" + iC.ToString() + ".png");
#endif
#endif
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
