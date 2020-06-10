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
    public int iClubId { get; set; }

    [Display(Name = "Saison: ")]
    public int iSeason { get; set; }

    [Display(Name = "Land: ")]
    public int iLand { get; set; }

    [Display(Name = "Spielklasse: ")]
    public int iDivision { get; set; }

    // List<CornerkickManager.Core.Tabellenplatz> ltTbpl = cr.getTabelleLiga(iSaison, iLand, iSpielklasse, iSpieltag, 0);
    public CornerkickManager.Cup league { get; set; }
    public List<CornerkickManager.Cup.TableItem> ltTbl { get; set; }
    public List<CornerkickManager.Cup.TableItem> ltTblLast { get; set; } // Table last matchday
    public int iLeagueSize { get; set; }

    public List<SelectListItem> ddlLand { get; set; }
    public List<SelectListItem> ddlSeason { get; set; }
    public List<SelectListItem> ddlDivision { get; set; }

    public LeagueModels()
    {
      ddlLand = new List<SelectListItem>();
      ddlDivision = new List<SelectListItem>();
      if (MvcApplication.iNations.Length > 0) iLand = MvcApplication.iNations[0];
    }

  }
}