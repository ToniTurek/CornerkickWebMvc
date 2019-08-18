using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CornerkickWebMvc.Models
{
  public class UserModel
  {
    public CornerkickManager.User usr { get; set; }
    public List<CornerkickManager.Main.News> ltUserMail { get; set; }
    public List<SelectListItem> ltDdlUser { get; set; }
    public string sMailTo { get; set; }
  }
}