﻿using System;
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
    public string sTraining { get; set; }
    public List<SelectListItem> ltDdlTraining { get; set; }

    public TrainingModel()
    {
      ltDdlTraining = new List<SelectListItem>();

      for (byte i = 0; i < MvcApplication.ckcore.sTraining.Length; i++) {
        ltDdlTraining.Add(
          new SelectListItem {
            Text = MvcApplication.ckcore.sTraining[i],
            Value = i.ToString()
          }
        );
      }
    }
  }

  //DataContract for Serializing Data - required to serve in JSON format
  [DataContract]
  public class DataPointGeneral
  {
    public DataPointGeneral(long x, double y)
    {
      this.x = x;
      this.y = y;
    }

    //Explicitly setting the name to be used while serializing to JSON.
    [DataMember(Name = "x")]
    public Nullable<long> x = null;
    //[DataMember(Name = "x")]
    //public Nullable<DateTime> X = null;

    //Explicitly setting the name to be used while serializing to JSON.
    [DataMember(Name = "y")]
    public Nullable<double> y = null;
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