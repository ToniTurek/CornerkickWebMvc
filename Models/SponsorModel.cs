using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CornerkickWebMvc.Models
{
  public class SponsorModel
  {
    public List<CornerkickManager.Finance.Sponsor> ltSponsorOffers { get; set; }
    public List<CornerkickManager.Finance.Sponsor> ltSponsorBoards { get; set; }
    public      CornerkickManager.Finance.Sponsor  sponsorMain { get; set; }
    public List<int> ltSponsorBoardIds { get; set; }
    public List<string> ltSponsorNames { get; set; }

    public string sEmblem { get; set; }
    public string sColorJersey   { get; set; }
  }

  public class DatatableEntrySponsorBoard
  {
    public bool   bOffer { get; set; }
    public int    iIndex { get; set; }
    public byte   iId { get; set; }
    public string sName { get; set; }
    public int    iMoneyVicHome { get; set; }
    public byte   nBoards { get; set; }
    public byte   iYears { get; set; }
  }

  public class DatatableEntrySponsorSpecial
  {
    public bool bOffer { get; set; }
    public int iIndex { get; set; }
    public byte iId { get; set; }
    public string sName { get; set; }
    public int iMoney { get; set; }
    public string sCondition { get; set; }
  }
}