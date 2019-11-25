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

    public class Buildings
    {
      public byte iGround { get; set; }
      public string sCostBuyGround { get; set; }
      public List<Building> ltBuildings { get; set; }
      public List<Building> ltBuildingsFree { get; set; }
    }

    public class Building
    {
      public string sCategory { get; set; }
      public int iLevel { get; set; }
      public byte iType { get; set; }
      public string sName { get; set; }
      public string sNameNext { get; set; }
      public int nDaysConstruct { get; set; }
      public int nDaysConstructTotal { get; set; }
      public string sCostConstructNext { get; set; }
      public bool bDispoOk { get; set; }
    }
  }
}