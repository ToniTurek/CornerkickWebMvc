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
    public int[] iSeatType { get; set; }
    public int[] iSeatsBuild { get; set; }
    public bool bOberring { get; set; }
    public static int iKosten { get; set; }

    public CornerkickGame.Stadium stadion    { get; set; }
    public CornerkickGame.Stadium stadionNew { get; set; }

    public List<SelectListItem> ltDdlStadionSeatType { get; set; }

    public List<SelectListItem> ltDdlVideo { get; set; }
    public byte iVideo { get; set; }

    public StadionModel()
    {
      ltDdlStadionSeatType = new List<SelectListItem>();
      ltDdlStadionSeatType.Add(new SelectListItem { Text = "Steh", Value = "0" });
      ltDdlStadionSeatType.Add(new SelectListItem { Text = "Sitz", Value = "1" });
      ltDdlStadionSeatType.Add(new SelectListItem { Text = "VIP",  Value = "2" });

      ltDdlVideo = new List<SelectListItem>();
      for (byte iV = 0; iV < MvcApplication.ckcore.st.sVideo.Length; iV++) ltDdlVideo.Add(new SelectListItem { Text = MvcApplication.ckcore.st.sVideo[iV], Value = iV.ToString() });
    }
  }
}