using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CornerkickWebMvc.Models
{
  public class WishListModel
  {
    public static List<Wish> ltWish { get; set; }
    public bool bAdmin { get; set; }

    public class WishJson
    {
      public List<Wish> Wish { get; set; }
    }
    public class Wish
    {
      public float ranking { get; set; }
      public string title { get; set; }
      public string description { get; set; }
      public string owner { get; set; }
      public int complexity { get; set; }
      public List<string> votes { get; set; }
      public string date { get; set; }
      public float progress { get; set; }
      public string dateRel { get; set; } // Date of release
      public string version { get; set; } // Version of release
    }
  }
}