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
      string sFileLoad = sHomeDir + "save/.autosave.ckx";

#if !DEBUG
      string sFileZipLog = sHomeDir + "log.zip";

#if _USE_BLOB
      CornerkickWebMvc.Controllers.BlobsController bcontr = new Controllers.BlobsController();
      if (!System.IO.File.Exists(sFileLoad)) bcontr.downloadBlob("blobSave", sFileLoad);
      bcontr.downloadBlob("blobLog", sFileZipLog);
#endif
#if _USE_AMAZON_S3
      AmazonS3FileTransfer as3 = new AmazonS3FileTransfer();
      if (!System.IO.File.Exists(sFileLoad)) as3.downloadFile("ckSave", sHomeDir + "save");
      as3.downloadFile("ckLog", sHomeDir + "log");
#endif
#endif

      //if (MvcApplication.ckcore.io.load(sFileLoad)) MvcApplication.ckcore.calcSpieltage();
      if (MvcApplication.ckcore.io.load(sFileLoad)) {
        MvcApplication.ckcore.tl.writeLog("File " + sFileLoad + " loaded");
#if !DEBUG
        if (!MvcApplication.ckcore.dtDatum.Equals(MvcApplication.ckcore.dtSaisonstart)) timerCkCalender.Enabled = true;
#endif
      } else {
        MvcApplication.ckcore.calcSpieltage();
        MvcApplication.ckcore.dtDatum = MvcApplication.ckcore.dtSaisonstart;
      }
    }

    static void timerCkCalender_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
    {
      if (timerCkCalender.Interval < 1000) {
        timerCkCalender.Enabled = false;
      }

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
      MvcApplication.ckcore.ltClubs[0].ltTrainingHist.Clear();
      MvcApplication.ckcore.ltClubs[0].ltAccount.Clear();
      save();

      if ((MvcApplication.ckcore.dtDatum.Equals(MvcApplication.ckcore.dtSaisonstart) ||
          MvcApplication.ckcore.dtDatum.Year < 1900) &&
          MvcApplication.ckcore.iSaisonCount == 0) {
        MvcApplication.ltLog.Clear();
        MvcApplication.ckcore.setNewSeason();
      }

      int iRetCk = ckcore.next(true);
      if (iRetCk == 99) return; // Saisonende

      timerCkCalender.Enabled = true;
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

    public static void save(bool bForce = false)
    {
      // Don't save if calendar to fast
      if (timerCkCalender.Interval < 10000 && !bForce) return;

      string path = getHomeDir();
#if DEBUG
      path = "C:\\Users\\Jan\\Documents\\Visual Studio 2017\\Projects\\Cornerkick.git\\CornerkickWebMvc";
#else
      try {
#if _DEPLOY_ON_AZURE
        path = HttpContext.Current.Server.MapPath("~");
#endif
        if (path.EndsWith("\\")) path = path.Remove(path.Length - 1);
      } catch (HttpException exp) {
        MvcApplication.ckcore.tl.writeLog("save: HttpException: " + exp.Message.ToString());
#if _DEPLOY_ON_AZURE
        path = "D:\\home\\site\\wwwroot";
#endif
      } catch {
        MvcApplication.ckcore.tl.writeLog("save: unable to create path from Server.MapPath", MvcApplication.ckcore.sErrorFile);
#if _DEPLOY_ON_AZURE
        path = "D:\\home\\site\\wwwroot";
#endif
      }
#endif

      string sFileSave = path + "/save/.autosave_" + MvcApplication.ckcore.dtDatum.ToString("yyyy-MM-dd_HH-mm") + ".ckx";
      MvcApplication.ckcore.tl.writeLog("save: filename: " + sFileSave);

      try {
        MvcApplication.ckcore.io.save(sFileSave);

#if !DEBUG
        try {
#if _USE_BLOB
          CornerkickWebMvc.Controllers.BlobsController bcontr = new Controllers.BlobsController();
          bcontr.uploadBlob("blobSave", sFileSave);
#endif
#if _USE_AMAZON_S3
          AmazonS3FileTransfer as3 = new AmazonS3FileTransfer();
          as3.uploadFile(sFileSave);
#endif
        } catch {
#if _USE_BLOB
          MvcApplication.ckcore.tl.writeLog("ERROR: could not upload file " + sFileSave + " to blob", MvcApplication.ckcore.sErrorFile);
#endif
#if _USE_AMAZON_S3
          MvcApplication.ckcore.tl.writeLog("ERROR: could not upload file " + sFileSave + " to amazon s3", MvcApplication.ckcore.sErrorFile);
#endif
        }
#endif

        // Copy autosave file with datum to basic one (could use file link)
        string sFileSaveBasic = path + "/save/.autosave.ckx";
        if (System.IO.File.Exists(sFileSaveBasic)) {
          try {
            System.IO.File.Delete(sFileSaveBasic);
          } catch {
          }
        }

        System.IO.File.Copy(sFileSave, sFileSaveBasic);
      } catch {
        MvcApplication.ckcore.tl.writeLog("ERROR: could not save to file " + sFileSave, MvcApplication.ckcore.sErrorFile);
      }

      // save log dir
      if (System.IO.Directory.Exists(path + "/log")) {
        string sFileZipLog = path + "/log.zip";

#if !DEBUG
        if (System.IO.File.Exists(sFileZipLog)) {
          try {
            System.IO.File.Delete(sFileZipLog);
          } catch {
          }
        }

        ZipFile.CreateFromDirectory(path + "/log", sFileZipLog);

        try {
#if _USE_BLOB
          CornerkickWebMvc.Controllers.BlobsController bcontr = new Controllers.BlobsController();
          bcontr.uploadBlob("blobLog", sFileZipLog);
#endif
#if _USE_AMAZON_S3
          AmazonS3FileTransfer as3 = new AmazonS3FileTransfer();
          as3.uploadFile(sFileZipLog, "ckLog");
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

#if _DEPLOY_ON_APPHB
      return Path.Combine(HttpContext.Current.Server.MapPath("~"), "App_Data");
#endif

      return HttpContext.Current.Server.MapPath("~/");
#endif
    }
  }
}
