using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CornerkickWebMvc.Models
{
  public class MerchandisingModel
  {
    public List<CornerkickManager.Club.MerchandisingItem> ltClubMerchandisingItems;

    public int iItem { get; set; }
    public List<SelectListItem> sliMerchandisingItems { get; set; }

    public CornerkickManager.Club.MerchandisingMarketer marketer { get; set; }
    public string sMarketerMoney { get; set; }

    public bool bFanshopsAvailable { get; set; }
    public bool bMarketer { get; set; }

    public class DatatableMerchandising
    {
      public int iIx { get; set; }
      public int iId { get; set; }
      public string sName { get; set; }
      public int iPresent { get; set; }
      public int iValuePresent { get; set; }
      public string sPricePresentBuyAve { get; set; }
      public int iSold { get; set; }
      public string sPriceBasic { get; set; }
      public string sPriceBuy { get; set; }
      public float fPriceSell { get; set; }
      public int iItemIncome { get; set; }
      public int iWinLoose { get; set; }
      public string sPriceSellAve { get; set; }
    }

    public MerchandisingModel()
    {
      sliMerchandisingItems = new List<SelectListItem>();

      foreach (CornerkickManager.Merchandising.Item mi in MvcApplication.ckcore.ltMerchandising) {
        sliMerchandisingItems.Add(new SelectListItem { Text = mi.sName, Value = mi.iId.ToString() });
      }
    }
  }
}