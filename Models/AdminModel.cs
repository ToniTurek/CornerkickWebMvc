using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CornerkickWebMvc.Models
{
  public class AdminModel
  {
    public bool bCk { get; set; }
    public bool bTimer { get; set; }
    public bool bTimerSave { get; set; }
    public List<string> ltLog { get; set; }
    public List<string> ltErr { get; set; }
    public int nClubs { get; set; }
    public int nUser { get; set; }
    public int nPlayer { get; set; }
    public string sHomeDir { get; set; }
    public string sHomeDirCk { get; set; }
    public string sHomeDirCk2 { get; set; }

    public DateTime dtCkCurrent { get; set; }
    public DateTime dtCkApproach { get; set; }
    public double fIntervalAveToApproachTarget { get; set; }

    // Settings
    public double fCalendarInterval { get; set; }
    public string sStartHour { get; set; }
    public int iGameSpeed { get; set; }
    public bool bEmailCertification { get; set; }
    public bool bRegisterDuringGame { get; set; }
    public bool bLoginPossible { get; set; }

    public bool bLogExist { get; set; }
    public bool bAutosaveExist { get; set; }
    public bool bSaveDirExist { get; set; }

    public string sSelectedAutosaveFile { get; set; }
    public List<SelectListItem> ddlAutosaveFiles { get; set; }

    // CPU Clubs to be selected by admin
    public List<SelectListItem> ddlClubsAdmin { get; set; }
    public int iSelectedClubAdmin { get; set; }

    public AdminModel()
    {
      ddlAutosaveFiles = new List<SelectListItem>();
      DirectoryInfo d = new DirectoryInfo(Path.Combine(MvcApplication.getHomeDir(), "save"));
      if (d.Exists) {
        FileInfo[] ltCkxFiles = d.GetFiles("*.ckx");
        int i = 0;
        foreach (FileInfo ckx in ltCkxFiles) {
          ddlAutosaveFiles.Add(
            new SelectListItem {
              Text  = ckx.Name,
              Value = ckx.Name
            }
          );
        }
      }

      ddlClubsAdmin = new List<SelectListItem>();
      for (int iC = 0; iC < MvcApplication.ckcore.ltClubs.Count; iC++) {
        CornerkickManager.Club clbCPU = MvcApplication.ckcore.ltClubs[iC];

        if (clbCPU.user == null) {
          ddlClubsAdmin.Add(
            new SelectListItem {
              Text  = clbCPU.sName,
              Value = iC.ToString()
            }
          );
        }
      }
    }

  }
}