//#define _USE_BLOB
#define _DEPLOY_ON_APPHB

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
      if (MvcApplication.ckcore == null) {
        modelAdmin.bCk = false;
        return View(modelAdmin);
      }
      modelAdmin.bCk = true;

      // Timer
      if (MvcApplication.timerCkCalender != null) {
        modelAdmin.bTimer            = MvcApplication.timerCkCalender.Enabled;
        modelAdmin.fCalendarInterval = MvcApplication.timerCkCalender.Interval / 1000;
      }

      if (MvcApplication.timerSave != null) {
        modelAdmin.bTimerSave = MvcApplication.timerSave.Enabled;
      }

      // Settings
      modelAdmin.sStartHour = "";
      if (MvcApplication.settings.iStartHour >= 0) modelAdmin.sStartHour = MvcApplication.settings.iStartHour.ToString();
      if (MvcApplication.ckcore.ltUser.Count > 0 && MvcApplication.ckcore.ltUser[0].nextGame != null) modelAdmin.iGameSpeed = MvcApplication.ckcore.ltUser[0].nextGame.iGameSpeed;
      modelAdmin.bLoginPossible      = MvcApplication.settings.bLoginPossible;
      modelAdmin.bEmailCertification = MvcApplication.settings.bEmailCertification;
      modelAdmin.bRegisterDuringGame = MvcApplication.settings.bRegisterDuringGame;
      modelAdmin.sHomeDir    = MvcApplication.getHomeDir();
      modelAdmin.sHomeDirCk = MvcApplication.ckcore.settings.sHomeDir;

      // Statistics
      modelAdmin.nClubs  = MvcApplication.ckcore.ltClubs .Count;
      modelAdmin.nUser   = MvcApplication.ckcore.ltUser  .Count;
      modelAdmin.nPlayer = MvcApplication.ckcore.ltPlayer.Count;

      modelAdmin.dtCkCurrent = MvcApplication.ckcore.dtDatum;
      modelAdmin.dtCkApproach = MvcApplication.getCkApproachDate();
      modelAdmin.fIntervalAveToApproachTarget = MvcApplication.getIntervalAve();

      // Files
      string sHomeDir = getHomeDir();
      modelAdmin.bLogExist = System.IO.File.Exists(sHomeDir + "/log/ck.log");

      //DirectoryInfo d = new DirectoryInfo(sHomeDir + "save");
      //FileInfo[] ltCkxFiles = d.GetFiles("*.ckx");
      modelAdmin.bAutosaveExist = System.IO.File.Exists(sHomeDir + "/save/.autosave.ckx");
      modelAdmin.bSaveDirExist  = System.IO.Directory.Exists(sHomeDir + "/save");

      if (MvcApplication.clubAdmin != null) modelAdmin.iSelectedClubAdmin = MvcApplication.clubAdmin.iId;

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
      MvcApplication.settings.iStartHour = -1;
      if (!string.IsNullOrEmpty(modelAdmin.sStartHour)) {
        int.TryParse(modelAdmin.sStartHour, out MvcApplication.settings.iStartHour);
      }

      if (modelAdmin.fCalendarInterval < 1E-6) {
        MvcApplication.timerCkCalender.Enabled = false;
        return View(modelAdmin);
      }

      /*
      // If first step: Add CPU teams
      if (MvcApplication.ckcore.dtDatum.Date.Equals(MvcApplication.ckcore.dtSeasonStart.Date) && MvcApplication.ckcore.iSaisonCount == 0) {
      }
      */

      setGameSpeedToAllUsers(modelAdmin.iGameSpeed);

      // Do one step now
      //MvcApplication.ckcore.next(true);

      // Start the timer
      MvcApplication.timerCkCalender.Interval = modelAdmin.fCalendarInterval * 1000;
      MvcApplication.timerCkCalender.Enabled = true;

      // Save last state
      MvcApplication.saveLaststate(MvcApplication.ckcore.settings.sHomeDir);

      return RedirectToAction("Settings");
    }

    internal static void setGameSpeedToAllUsers(int iGameSpeed)
    {
      foreach (CornerkickManager.User user in MvcApplication.ckcore.ltUser) {
        if (user.nextGame != null) user.nextGame.iGameSpeed = iGameSpeed;
      }
    }

    public ActionResult StopCalendar(Models.AdminModel modelAdmin)
    {
      MvcApplication.timerCkCalender.Enabled = false;
      MvcApplication.timerSave.Enabled = true;

      // Save last state
      MvcApplication.saveLaststate(MvcApplication.ckcore.settings.sHomeDir);

      return RedirectToAction("Settings");
      //return View("Settings", "");
      //return View(modelAdmin);
    }

    public ActionResult OneStep()
    {
      // Do one step now
      MvcApplication.performCalendarStep(bSave: false);

      return RedirectToAction("Settings");
    }

    public ActionResult RestartCk(Models.AdminModel modelAdmin)
    {
      if (MvcApplication.timerCkCalender != null) MvcApplication.timerCkCalender.Enabled = false;

      MvcApplication.newCk();

      return RedirectToAction("Settings");
      //return View("Settings", "");
      //return View(modelAdmin);
    }

    public ActionResult SaveAutosave()
    {
      MvcApplication.save(MvcApplication.timerCkCalender, true);

      return RedirectToAction("Settings");
    }

    public ActionResult DeleteAutosave()
    {
      if (System.IO.File.Exists(getHomeDir() + "/save/.autosave.ckx")) System.IO.File.Delete(getHomeDir() + "/save/.autosave.ckx");

      return RedirectToAction("Settings");
    }

    public ActionResult DeleteSaveFolder()
    {
      if (System.IO.Directory.Exists(getHomeDir() + "/save"))          System.IO.Directory.Delete(getHomeDir() + "/save", true);
      if (System.IO.File     .Exists(getHomeDir() + "/laststate.txt")) System.IO.File     .Delete(getHomeDir() + "/laststate.txt");

      return RedirectToAction("Settings");
    }

    [HttpPost]
    public ActionResult LoadAutosave(Models.AdminModel modelAdmin)
    {
      if (System.IO.File.Exists(getHomeDir() + "/save/" + modelAdmin.sSelectedAutosaveFile)) {
        MvcApplication.timerCkCalender.Enabled = false;

        MvcApplication.newCk(bLoadGame: false);

        MvcApplication.ckcore.io.load(getHomeDir() + "/save/" + modelAdmin.sSelectedAutosaveFile);
      }

      return RedirectToAction("Settings");
    }

    public void setSettings(bool bEmailCertification, bool bRegisterDuringGame, bool bLoginPossible, bool bMaintenance)
    {
      MvcApplication.settings.bEmailCertification = bEmailCertification;
      MvcApplication.settings.bRegisterDuringGame = bRegisterDuringGame;
      MvcApplication.settings.bLoginPossible      = bLoginPossible;
      MvcApplication.settings.bMaintenance        = bMaintenance;
    }

    public ActionResult Log(Models.AdminModel modelAdmin)
    {
      modelAdmin.ltLog = new List<string>();
      modelAdmin.ltErr = new List<string>();

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

      // Log
      try {
        // Create an instance of StreamReader to read from a file.
        // The using statement also closes the StreamReader.
        using (StreamReader sr = new StreamReader(getHomeDir() + "/log/ck.log")) {
          string sLine;
          // Read and display lines from the file until the end of 
          // the file is reached.
          while ((sLine = sr.ReadLine()) != null) {
            modelAdmin.ltLog.Add(sLine);
          }
        }
      } catch (Exception e) {
        // Let the user know what went wrong.
        Console.WriteLine("The file could not be read:");
        Console.WriteLine(e.Message);
      }

      // Error
      try {
        // Create an instance of StreamReader to read from a file.
        // The using statement also closes the StreamReader.
        using (StreamReader sr = new StreamReader(getHomeDir() + "/log/error.log")) {
          string sLine;
          // Read and display lines from the file until the end of 
          // the file is reached.
          while ((sLine = sr.ReadLine()) != null) {
            modelAdmin.ltErr.Add(sLine);
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
      var diLog = new DirectoryInfo(getHomeDir() + "/log");
      foreach (var file in diLog.EnumerateFiles("*.log")) {
        file.Delete();
      }
      if (System.IO.File.Exists(getHomeDir() + "/log.zip")) System.IO.File.Delete(getHomeDir() + "/log.zip");

      return RedirectToAction("Settings");
    }

    public ActionResult getFilesInDirectory(string sDir = "")
    {
      //if (string.IsNullOrEmpty(sDir) || sDir.Equals(".")) sDir = Server.MapPath("~");
      sDir = Path.Combine(Server.MapPath("~"), sDir);

      DirectoryInfo d = new DirectoryInfo(sDir);
      if (!d.Exists) {
        //Response.StatusCode = 1;
        return Json(new string[] { "Directory does not exist!" }, JsonRequestBehavior.AllowGet);
      }

      string sContent = ".." + '\n';

      // First get directories
      foreach (string sSubDir in Directory.GetDirectories(sDir)) {
        sContent += "<DIR> " + Path.GetFileName(sSubDir) + '\n';
      }

      // then get files
      //return Json(d.GetFiles("*").ToArray(), JsonRequestBehavior.AllowGet);
      foreach (FileInfo fi in d.GetFiles("*")) {
        sContent += fi.Name + " (" + fi.Length.ToString() + "b)\n";
      }

      return Json(sContent, JsonRequestBehavior.AllowGet);
    }

    public void SetAdminClub(int iClubIx)
    {
      MvcApplication.clubAdmin = null;

      if (iClubIx <                                    0) return;
      if (iClubIx >= MvcApplication.ckcore.ltClubs.Count) return;

      MvcApplication.clubAdmin = MvcApplication.ckcore.ltClubs[iClubIx];
    }

    public class UnfinishedGame
    {
      public int iCupId1 { get; set; }
      public int iCupId2 { get; set; }
      public int iCupId3 { get; set; }
      public string sCupName { get; set; }
      public int iMd { get; set; }
      public int iClubIdH { get; set; }
      public int iClubIdA { get; set; }
      public string sClubNameH { get; set; }
      public string sClubNameA { get; set; }
    }
    public JsonResult GetUnfinishedGames()
    {
      List<UnfinishedGame> ltUg = new List<UnfinishedGame>();

      foreach (CornerkickManager.Cup cup in MvcApplication.ckcore.ltCups) {
        if (Math.Abs(cup.iId) == MvcApplication.iCupIdTestgame) continue; // No test-games

        for (int iMd = 0; iMd < cup.ltMatchdays.Count; iMd++) {
          if (cup.ltMatchdays[iMd].dt.CompareTo(MvcApplication.ckcore.dtDatum) < 0) { // Game is in past
            for (int iGd = 0; iGd < cup.ltMatchdays[iMd].ltGameData.Count; iGd++) {
              if (cup.ltMatchdays[iMd].ltGameData[iGd].team[0].iGoals < 0 ||
                  cup.ltMatchdays[iMd].ltGameData[iGd].team[1].iGoals < 0) {
                ltUg.Add(
                  new UnfinishedGame() {
                    iCupId1 = cup.iId,
                    iCupId2 = cup.iId2,
                    iCupId3 = cup.iId3,
                    sCupName = cup.sName,
                    iMd = iMd,
                    iClubIdH = cup.ltMatchdays[iMd].ltGameData[iGd].team[0].iTeamId,
                    iClubIdA = cup.ltMatchdays[iMd].ltGameData[iGd].team[1].iTeamId,
                    sClubNameH = CornerkickManager.Tool.getClubFromId(cup.ltMatchdays[iMd].ltGameData[iGd].team[0].iTeamId, MvcApplication.ckcore.ltClubs).sName,
                    sClubNameA = CornerkickManager.Tool.getClubFromId(cup.ltMatchdays[iMd].ltGameData[iGd].team[1].iTeamId, MvcApplication.ckcore.ltClubs).sName
                  }
                );
              }
            }
          }
        }
      }

      return Json(new { games = ltUg }, JsonRequestBehavior.AllowGet);
    }

    public JsonResult setUnfinishedGameResult(int iCupId1, int iCupId2, int iCupId3, int iMd, int iClubIdH, int iClubIdA, int iGoalsH, int iGoalsA)
    {
      if (iGoalsH < 0 || iGoalsA < 0) return Json(new { result = "ERROR", Message = "Goals not valid!" }, JsonRequestBehavior.AllowGet);

      CornerkickManager.Cup cup = MvcApplication.ckcore.tl.getCup(iCupId1, iCupId2, iCupId3);
      if (cup == null) return Json(new { result = "ERROR", Message = "Cup not found!" }, JsonRequestBehavior.AllowGet);

      if (cup.ltMatchdays == null || iMd >= cup.ltMatchdays.Count) return Json(new { result = "ERROR", Message = "Matchday not valid" }, JsonRequestBehavior.AllowGet);

      for (int iGd = 0; iGd < cup.ltMatchdays[iMd].ltGameData.Count; iGd++) {
        if (cup.ltMatchdays[iMd].ltGameData[iGd].team[0].iTeamId == iClubIdH &&
            cup.ltMatchdays[iMd].ltGameData[iGd].team[1].iTeamId == iClubIdA) {
          // Reset cup ids
          cup.ltMatchdays[iMd].ltGameData[iGd].iGameType  = cup.iId;
          cup.ltMatchdays[iMd].ltGameData[iGd].iGameType2 = cup.iId2;
          cup.ltMatchdays[iMd].ltGameData[iGd].iGameType3 = cup.iId3;

          // Set new result
          cup.ltMatchdays[iMd].ltGameData[iGd].team[0].iGoals = iGoalsH;
          cup.ltMatchdays[iMd].ltGameData[iGd].team[1].iGoals = iGoalsA;

          return Json(new { result = "OK" }, JsonRequestBehavior.AllowGet);
        }
      }

      return Json(new { result = "ERROR", Message = "Clubs not found!" }, JsonRequestBehavior.AllowGet);
    }

    public string getHomeDir()
    {
#if DEBUG
      return "C:\\Users\\Jan\\Documents\\Visual Studio 2017\\Projects\\Cornerkick.git\\CornerkickWebMvc\\";
#endif
#if _DEPLOY_ON_APPHB
      return Path.Combine(System.Web.HttpContext.Current.Server.MapPath("~"), "App_Data");
#endif

      return System.Web.HttpContext.Current.Server.MapPath("~");
    }

#if _USE_BLOB
    public ActionResult CreateBlobContainer()
    {
      CloudBlobContainer container = MvcApplication.GetCloudBlobContainer();
      ViewBag.Success = container.CreateIfNotExists();
      ViewBag.BlobContainerName = container.Name;

      return View();
    }

#endif
  }
}