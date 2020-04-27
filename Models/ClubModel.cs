using System;
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

    public string sAttrFc { get; set; }
  }
}