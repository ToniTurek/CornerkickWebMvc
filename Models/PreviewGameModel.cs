using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CornerkickWebMvc.Models
{
  public class PreviewGameModel
  {
    public List<SelectListItem> ddlGames { get; set; }
    public string[] sGames { get; set; }

    public string sTeamH { get; set; }
    public string sTeamA { get; set; }
    public string sCupName { get; set; }
    public string sMd { get; set; }
    public string sStadium { get; set; }
    public string sReferee { get; set; }

    public PreviewGameModel()
    {
      ddlGames = new List<SelectListItem>();
    }
  }
}