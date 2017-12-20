using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CornerkickWebMvc.Models
{
  public class LeagueModels
  {
    public ushort iSaison { get; set; }
    public int iLand { get; set; }
    public byte iSpKl { get; set; }
    public byte iSpTg { get; set; }

    // List<CornerkickCore.Core.Tabellenplatz> ltTbpl = cr.getTabelleLiga(iSaison, iLand, iSpielklasse, iSpieltag, 0);
    public List<List<int[]>> ltErg { get; set; }
    public List<CornerkickCore.Core.Tabellenplatz> ltTbl { get; set; }
  }
}