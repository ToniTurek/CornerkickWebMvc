using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace CornerkickWebMvc.Models
{
  public class StadionModel
  {
    public string sName { get; set; }

    public int[] iSeats { get; set; }
    public int[] iSeatsConstr { get; set; }

    public string[] sBlocksConstrName { get; set; }
    public int[] iBlocksConstrSeats { get; set; }
    public string[] sBlocksConstrType { get; set; }
    public int[] iBlocksConstrDays { get; set; }

    public bool bTopring { get; set; }

    public bool bBlockConstructions { get; set; }
    public bool bEditable { get; set; } // Stadium name editable

    public CornerkickGame.Stadium stadion { get; set; }

    public List<SelectListItem> ltDdlStadionSeatType { get; set; }

    public List<SelectListItem> ltDdlVideo { get; set; }
    public byte iVideo { get; set; }
    public byte iSnackbarNew { get; set; }
    public byte iToiletsNew { get; set; }
    public byte iSnackbarReq { get; set; }
    public byte iToiletsReq { get; set; }

    public bool bSound { get; set; }

    public StadionModel()
    {
      ltDdlStadionSeatType = new List<SelectListItem>();
      ltDdlStadionSeatType.Add(new SelectListItem { Text = "Steh", Value = "0" });
      ltDdlStadionSeatType.Add(new SelectListItem { Text = "Sitz", Value = "1" });
      ltDdlStadionSeatType.Add(new SelectListItem { Text = "VIP",  Value = "2" });

      ltDdlVideo = new List<SelectListItem>();
      for (byte iV = 0; iV < CornerkickManager.Stadium.sVideo.Length; iV++) ltDdlVideo.Add(new SelectListItem { Text = CornerkickManager.Stadium.sVideo[iV], Value = iV.ToString() });
    }
  }
}