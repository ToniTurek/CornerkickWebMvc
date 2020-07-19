using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Web.Mvc;

namespace CornerkickWebMvc.Models
{
  public class DeskModel
  {
    public CornerkickManager.User user { get; set; }
    public CornerkickManager.Club club { get; set; }
    public string sDtNextGame { get; set; }
    public string sTabellenplatz { get; set; }
    public string sPokalrunde { get; set; }
    public string sGoldCupRound { get; set; }
    public string sSilverCupRound { get; set; }
    public string sVDL { get; set; }
    public string sKFM { get; set; }
    public string sStrength { get; set; }
    public byte   iWeather { get; set; }

    public bool bEmblemExist { get; set; }

    public List<SelectListItem> ddlDeleteLog { get; set; }
    public int iDeleteLog { get; set; }

    public DeskModel()
    {
      ddlDeleteLog = new List<SelectListItem>();
      ddlDeleteLog.Add(new SelectListItem { Text = "7 Tagen",  Value = "1" });
      ddlDeleteLog.Add(new SelectListItem { Text = "14 Tagen", Value = "2" });
      ddlDeleteLog.Add(new SelectListItem { Text = "1 Monat",  Value = "3" });
      ddlDeleteLog.Add(new SelectListItem { Text = "Nie",      Value = "0"});
    }
  }

  //DataContract for Serializing Data - required to serve in JSON format
  [DataContract]
  public class DataPointLastGames2
  {
    public DataPointLastGames2(int x, double y)
    {
      this.X = x;
      this.Y = y;
    }

    //Explicitly setting the name to be used while serializing to JSON.
    [DataMember(Name = "x")]
    public Nullable<int> X = null;

    //Explicitly setting the name to be used while serializing to JSON.
    [DataMember(Name = "y")]
    public Nullable<double> Y = null;
  }

  //DataContract for Serializing Data - required to serve in JSON format
  [DataContract]
  public class DataPointLastGames
  {
    public DataPointLastGames(string sX, int iY)
    {
      this.X = sX;
      this.Y = iY;
    }

    //Explicitly setting the name to be used while serializing to JSON.
    [DataMember(Name = "Game")]
    public string X = "";

    //Explicitly setting the name to be used while serializing to JSON.
    [DataMember(Name = "Result")]
    public Nullable<int> Y = null;
  }
}