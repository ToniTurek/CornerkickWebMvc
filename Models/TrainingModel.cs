using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Web.Mvc;

namespace CornerkickWebMvc.Models
{
  public class TrainingModel
  {
    public CornerkickManager.Main.TrainingPlan.Unit[][] ltTu { get; set; }
    public int   [] iTrainingCount    { get; set; }
    public string[] sTrainingCountRel { get; set; }
  }

  //DataContract for Serializing Data - required to serve in JSON format
  [DataContract]
  public class DataPointGeneral
  {
    public DataPointGeneral(long x, double y, string z = "")
    {
      this.x = x;
      this.y = y;
      this.z = z;
    }

    // Explicitly setting the name to be used while serializing to JSON.
    [DataMember(Name = "x")]
    public Nullable<long> x = null;

    // Explicitly setting the name to be used while serializing to JSON.
    [DataMember(Name = "y")]
    public Nullable<double> y = null;

    // Explicitly setting the name to be used while serializing to JSON.
    [DataMember(Name = "z")]
    public string z;
  }

  [DataContract]
  public class DataPointTeamFAve2
  {
    public DataPointTeamFAve2(string s, double fFAve)
    {
      this.s = s;
      this.f = fFAve;
    }

    public string s = null;

    //Explicitly setting the name to be used while serializing to JSON.
    [DataMember(Name = "f")]
    public Nullable<double> f = null;
  }
}