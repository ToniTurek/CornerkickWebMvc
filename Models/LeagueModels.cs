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

    [Display(Name = "Land: ")]
    public int iLand { get; set; }

    public byte iSpKl { get; set; }

    [Display(Name = "Spieltag: ")]
    public int iMd { get; set; }

    // List<CornerkickCore.Core.Tabellenplatz> ltTbpl = cr.getTabelleLiga(iSaison, iLand, iSpielklasse, iSpieltag, 0);
    public CornerkickCore.Cup league { get; set; }
    public List<CornerkickCore.Tool.TableItem> ltTbl { get; set; }
    public List<CornerkickCore.Tool.TableItem> ltTblLast { get; set; } // Table last matchday
    public List<CornerkickCore.UI.Scorer> ltScorer { get; set; }
    public int iLeagueSize { get; set; }

    public List<SelectListItem> ddlLand { get; set; }
    public List<SelectListItem> ltDdlSpTg { get; set; }

    public static List<System.Web.Mvc.SelectListItem> ltSasn = new List<System.Web.Mvc.SelectListItem>();
    //public static List<System.Web.Mvc.SelectListItem> ltLand = new List<System.Web.Mvc.SelectListItem>();
    //public static List<System.Web.Mvc.SelectListItem> ltSpKl = new List<System.Web.Mvc.SelectListItem>();

    public LeagueModels()
    {
      ddlLand   = new List<SelectListItem>();
      ltDdlSpTg = new List<SelectListItem>();
    }

  }
}