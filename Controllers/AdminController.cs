using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using Microsoft.Azure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace CornerkickWebMvc.Controllers
{
  public class AdminController : Controller
  {
    // GET: Admin
    public ActionResult Index()
    {
      return View();
    }

    public ActionResult Settings(Models.AdminModel modelAdmin)
    {
      modelAdmin.bCk = MvcApplication.ckcore != null;
      modelAdmin.bTimer = MvcApplication.timerCkCalender.Enabled;
      modelAdmin.fCalenderInterval = MvcApplication.timerCkCalender.Interval / 1000;

      modelAdmin.sStartHour = "";
      if (MvcApplication.iStartHour >= 0) modelAdmin.sStartHour = MvcApplication.iStartHour.ToString();

      if (MvcApplication.ckcore.ltUser.Count > 0) modelAdmin.iGameSpeed = MvcApplication.ckcore.ltUser[0].nextGame.iGameSpeed;

      modelAdmin.nClubs  = MvcApplication.ckcore.ltClubs .Count;
      modelAdmin.nUser   = MvcApplication.ckcore.ltUser  .Count;
      modelAdmin.nPlayer = MvcApplication.ckcore.ltPlayer.Count;

      string sHomeDir = getHomeDir();
      modelAdmin.bLogExist      = System.IO.File.Exists(sHomeDir + "log/ck.log");

      //DirectoryInfo d = new DirectoryInfo(sHomeDir + "save");
      //FileInfo[] ltCkxFiles = d.GetFiles("*.ckx");
      modelAdmin.bAutosaveExist = System.IO.File.Exists(sHomeDir + "save/.autosave.ckx");

      return View(modelAdmin);
    }

    public ActionResult StartCalendar(Models.AdminModel modelAdmin)
    {
      /*
      if (MvcApplication.ckcore.iSaisonCount == 0) {
        MvcApplication.ltLog.Clear();
        MvcApplication.ckcore.setNeueSaison();
      }
      */
      MvcApplication.iStartHour = -1;
      if (!string.IsNullOrEmpty(modelAdmin.sStartHour)) {
        int.TryParse(modelAdmin.sStartHour, out MvcApplication.iStartHour);
      }

      if (modelAdmin.fCalenderInterval < 1E-6) {
        MvcApplication.timerCkCalender.Enabled = false;
        return View(modelAdmin);
      }

      for (int iU = 0; iU < MvcApplication.ckcore.ltUser.Count; iU++) {
        CornerkickCore.Core.User user = MvcApplication.ckcore.ltUser[iU];
        user.nextGame.iGameSpeed = modelAdmin.iGameSpeed;
        MvcApplication.ckcore.ltUser[iU] = user;
      }

      // Do one step now
      MvcApplication.ckcore.next(true);

      // Start the timer
      MvcApplication.timerCkCalender.Interval = modelAdmin.fCalenderInterval * 1000;
      MvcApplication.timerCkCalender.Enabled = true;

      return RedirectToAction("Settings");
    }

    public ActionResult StopCalendar(Models.AdminModel modelAdmin)
    {
      MvcApplication.timerCkCalender.Enabled = false;

      return RedirectToAction("Settings");
      //return View("Settings", "");
      //return View(modelAdmin);
    }

    public ActionResult RestartCk(Models.AdminModel modelAdmin)
    {
      MvcApplication.timerCkCalender.Enabled = false;

      MvcApplication.newCk();

      return RedirectToAction("Settings");
      //return View("Settings", "");
      //return View(modelAdmin);
    }

    public ActionResult DeleteAutosave()
    {
      if (System.IO.File.Exists(getHomeDir() + "save/.autosave.ckx")) System.IO.File.Delete(getHomeDir() + "save/.autosave.ckx");

      return RedirectToAction("Settings");
    }

    [HttpPost]
    public ActionResult LoadAutosave(Models.AdminModel modelAdmin)
    {
      if (System.IO.File.Exists(getHomeDir() + "save/" + modelAdmin.sSelectedAutosaveFile)) {
        MvcApplication.timerCkCalender.Enabled = false;

        MvcApplication.newCk();

        MvcApplication.ckcore.io.load(getHomeDir() + "save/" + modelAdmin.sSelectedAutosaveFile);
      }

      return RedirectToAction("Settings");
    }

    public ActionResult Log(Models.AdminModel modelAdmin)
    {
      modelAdmin.sLog = "";

      /*
      if (MvcApplication.ckcore != null) {
        if (MvcApplication.ckcore.log != null) {
          if (MvcApplication.ckcore.log.Count > 0) {

            //ltLog = new List<string>(MvcApplication.ckcore.log);
            MvcApplication.ltLog.AddRange(MvcApplication.ckcore.log);
          }
        }
      }

      foreach (string s in MvcApplication.ltLog) modelAdmin.sLog += s + '\n';
      */

      try {
        // Create an instance of StreamReader to read from a file.
        // The using statement also closes the StreamReader.
        using (StreamReader sr = new StreamReader(getHomeDir() + "ck.log")) {
          string sLine;
          // Read and display lines from the file until the end of 
          // the file is reached.
          while ((sLine = sr.ReadLine()) != null) {
            modelAdmin.sLog += sLine + '\n';
          }
        }
      } catch (Exception e) {
        // Let the user know what went wrong.
        Console.WriteLine("The file could not be read:");
        Console.WriteLine(e.Message);
      }

      return View(modelAdmin);
    }

    public ActionResult DeleteLog(Models.AdminModel modelAdmin)
    {
      MvcApplication.ckcore.log.Clear();

      var diLog = new DirectoryInfo(getHomeDir() + "log");
      foreach (var file in diLog.EnumerateFiles("*.log")) {
        file.Delete();
      }
      if (System.IO.File.Exists(getHomeDir() + "log.zip")) System.IO.File.Delete(getHomeDir() + "log.zip");

      return RedirectToAction("Settings");
    }

    public string getHomeDir()
    {
#if DEBUG
      return "C:\\Users\\Jan\\Documents\\Visual Studio 2017\\Projects\\Cornerkick.git\\CornerkickWebMvc\\";
#endif
      return System.Web.HttpContext.Current.Server.MapPath("~/");
    }

    public ActionResult SaveAutosave()
    {
      MvcApplication.save(true);

      return RedirectToAction("Settings");
    }

    public ActionResult CreateBlobContainer()
    {
      CloudBlobContainer container = MvcApplication.GetCloudBlobContainer();
      ViewBag.Success = container.CreateIfNotExists();
      ViewBag.BlobContainerName = container.Name;

      return View();
    }

  }
}