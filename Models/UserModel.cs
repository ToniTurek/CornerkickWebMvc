using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CornerkickWebMvc.Models
{
  public class UserModel
  {
    public List<CornerkickManager.Main.News> ltUserMail { get; set; }
    public List<SelectListItem> ltDdlUser { get; set; }
    public string sMailTo { get; set; }

    /*
    public struct Mail
    {
      public int iTo { get; set; }
      public int iFrom { get; set; }
      public DateTime dt { get; set; }
      public string sText { get; set; }
      public bool bRead { get; set; }
    }
    */

    public UserModel()
    {
      if (Controllers.AccountController.appUser == null) return;

      ltDdlUser = new List<SelectListItem>();

      ltDdlUser.Add(new SelectListItem { Text = "-", Value = "-1" });

      // Positionen zu Dropdown Menü hinzufügen
      for (int iU = 0; iU < MvcApplication.ckcore.ltUser.Count; iU++) {
        CornerkickManager.User user = MvcApplication.ckcore.ltUser[iU];
        if (user.id == Controllers.AccountController.appUser.Id) continue;

        string sName = user.sFirstname + " " + user.sSurname;

        sName += " (" + MvcApplication.ckcore.ltClubs[user.iTeam].sName + ")";

        ltDdlUser.Add(new SelectListItem { Text = sName, Value = iU.ToString() });
      }
    }
  }
}