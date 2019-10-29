using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CornerkickWebMvc.Models
{
  public class StadiumSurroundingsModel
  {
    public List<SelectListItem> ddlTrainingsgel { get; set; }
    public int iTrainingsgel { get; set; }
    public int iTrainingNew  { get; set; }

    public List<SelectListItem> ddlGym { get; set; }
    public int iGym { get; set; }
    public int iGymNew { get; set; }

    public List<SelectListItem> ddlSpa { get; set; }
    public int iSpa { get; set; }
    public int iSpaNew { get; set; }

    public List<SelectListItem> ddlJouthInternat { get; set; }
    public int iJouthInternat    { get; set; }
    public int iJouthInternatNew { get; set; }

    public List<SelectListItem> ddlClubHouse { get; set; }
    public int iClubHouse    { get; set; }
    public int iClubHouseNew { get; set; }

    public List<SelectListItem> ddlClubMuseum { get; set; }
    public int iClubMuseum    { get; set; }
    public int iClubMuseumNew { get; set; }

    public int iCarpark { get; set; }
    public int iCarparkNew { get; set; }
    public int iCounter { get; set; }
    public int iCounterNew { get; set; }
  }
}