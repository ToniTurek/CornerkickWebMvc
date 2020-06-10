using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CornerkickWebMvc.Models
{
  public class JouthModel
  {
    public static List<CornerkickGame.Player> ltPlayerJouth { get; set; }

    public class DatatableJouth
    {
      public int iId { get; set; }
      public float fAge { get; set; }
      public string sName { get; set; }
      public string sPos { get; set; }
      public float fSkillAve { get; set; }
      public int iTalent { get; set; }
      public string sNat { get; set; }
    }
  }
}