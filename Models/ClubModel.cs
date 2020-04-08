﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CornerkickWebMvc.Models
{
  public class ClubModel
  {
    public CornerkickManager.Club club { get; set; }

    // Emblem
    public string sEmblem { get; set; }

    public bool bEmblemEditable { get; set; }

    // Record games
    // League
    public string sRecordLWinH;
    public string sRecordLWinA;
    public string sRecordLDefH;
    public string sRecordLDefA;

    // Cup
    public string sRecordCWinH;
    public string sRecordCWinA;
    public string sRecordCDefH;
    public string sRecordCDefA;
    public string sAttrFc { get; set; }
  }
}