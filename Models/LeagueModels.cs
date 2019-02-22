using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CornerkickWebMvc.Models
{
  public class LeagueModels
  {
    public ushort iSaison { get; set; }
    public int iLand { get; set; }
    public byte iSpKl { get; set; }
    [Display(Name = "Spieltag: ")]
    public int iSpTg { get; set; }

    // List<CornerkickCore.Core.Tabellenplatz> ltTbpl = cr.getTabelleLiga(iSaison, iLand, iSpielklasse, iSpieltag, 0);
    public List<List<int[]>> ltErg { get; set; }
    public List<CornerkickCore.Tool.TableItem> ltTbl { get; set; }
    public List<CornerkickCore.UI.Scorer> ltScorer { get; set; }
    public List<SelectListItem> ltDdlSpTg { get; set; }

    public static List<System.Web.Mvc.SelectListItem> ltSasn = new List<System.Web.Mvc.SelectListItem>();
    //public static List<System.Web.Mvc.SelectListItem> ltLand = new List<System.Web.Mvc.SelectListItem>();
    //public static List<System.Web.Mvc.SelectListItem> ltSpKl = new List<System.Web.Mvc.SelectListItem>();

    public LeagueModels()
    {
      ltDdlSpTg = new List<SelectListItem>();

      // Spieltage zu Dropdown Menü hinzufügen
      if (MvcApplication.ckcore.ltLiga[iLand].Count > iSpKl) {
        while (ltDdlSpTg.Count < (MvcApplication.ckcore.ltLiga[iLand][iSpKl].Count - 1) * 2) {
          int iSpTg = ltDdlSpTg.Count + 1;
          ltDdlSpTg.Add(new SelectListItem { Text = iSpTg.ToString().PadLeft(2), Value = iSpTg.ToString() });
        }
      }
    }

  }
}