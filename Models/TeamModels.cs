using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace CornerkickWebMvc.Models
{
  public class TeamModels
  {
    public static List<CalendarModels> ltCalendar = new List<CalendarModels>();

    public static List<CornerkickGame.Game.Spieler> ltPlayer { get; set; }
    public int iPlayerDetails { get; set; }
    public int iPlayerIndTr { get; set; }
    public string sPlayerIndTr { get; set; }
    public bool bPlayerIndTr { get; set; }

    [DataType(DataType.Text)]
    [Display(Name = "Formation")]
    public string sFormation { get; set; }
    public List<System.Web.Mvc.SelectListItem> ltsFormations { get; set; }
  }
}