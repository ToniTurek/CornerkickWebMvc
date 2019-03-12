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
    public double fCalendarInterval { get; set; }
    public bool bTimer { get; set; }
    public string sStartHour { get; set; }
    public string sLog { get; set; }
    public string sErr { get; set; }
    public int nClubs { get; set; }
    public int nUser { get; set; }
    public int nPlayer { get; set; }
    public int iGameSpeed { get; set; }

    public bool bLogExist { get; set; }
    public bool bAutosaveExist { get; set; }

    public string sSelectedAutosaveFile { get; set; }
    public int iAutosaveFile { get; set; }
    public List<SelectListItem> ddlAutosaveFiles { get; set; }

    public AdminModel()
    {
      ddlAutosaveFiles = new List<SelectListItem>();

      DirectoryInfo d = new DirectoryInfo(MvcApplication.getHomeDir() + "/App_Data/save");
      if (d.Exists) {
        FileInfo[] ltCkxFiles = d.GetFiles("*.ckx");
        int i = 0;
        foreach (FileInfo ckx in ltCkxFiles) {
          ddlAutosaveFiles.Add(
            new SelectListItem {
              Text = ckx.Name,
              Value = ckx.Name
            }
          );
        }
      }
    }
  }
}