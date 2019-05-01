using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace CornerkickWebMvc.Models
{
  public class DeskModel
  {
    public CornerkickManager.User user { get; set; }
    public CornerkickManager.Club club { get; set; }
    public string sNews { get; set; }
    public string sNewsOld { get; set; }
    public string sTabellenplatz { get; set; }
    public string sPokalrunde { get; set; }
    public string sVDL { get; set; }
    public string sKFM { get; set; }
    public string sStrength { get; set; }
    public byte   iWeather { get; set; }
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