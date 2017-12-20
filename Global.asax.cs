using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace CornerkickWebMvc
{
  public class MvcApplication : System.Web.HttpApplication
  {
    public static CornerkickCore.Core ckcore;
    public static System.Timers.Timer timerCkCalender = new System.Timers.Timer(10000);

    protected void Application_Start()
    {
      AreaRegistration.RegisterAllAreas();
      FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
      RouteConfig.RegisterRoutes(RouteTable.Routes);
      BundleConfig.RegisterBundles(BundleTable.Bundles);

      Models.RegisterViewModel.ltSasn.Clear();
      Models.RegisterViewModel.ltSasn.Add(new SelectListItem { Text = "Saison 1", Value = "1", Selected = true });

      Models.RegisterViewModel.ltLand.Clear();
      Models.RegisterViewModel.ltLand.Add(new SelectListItem { Text = "Land 1", Value = "1", Selected = true });

      Models.RegisterViewModel.ltSpKl.Clear();
      Models.RegisterViewModel.ltSpKl.Add(new SelectListItem { Text = "Liga 1", Value = "1", Selected = true });

      Models.RegisterViewModel.ltSpTg.Clear();
      Models.RegisterViewModel.ltSpTg.Add(new SelectListItem { Text = "1", Value = "1", Selected = true });

      ckcore = new CornerkickCore.Core();
      ckcore.dtDatum = new DateTime(DateTime.Now.Year, ckcore.dtDatum.Month, ckcore.dtDatum.Day);

      /*
#if DEBUG
      Models.ApplicationUser appUsr = new Models.ApplicationUser();
      appUsr.Vorname = "Toni";
      appUsr.Nachname = "Turek";
      appUsr.Vereinsname = "Team";
      appUsr.Email = "s.jan@web.de";

      Models.RegisterViewModel regModel = new Models.RegisterViewModel();
      regModel.Email = "s.jan@web.de";
      regModel.Password = "test";
      regModel.Land = 1;
      regModel.Liga = 1;


      Controllers.AccountController account = new Controllers.AccountController();
      Controllers.AccountController.appUser = appUsr;
      var result = account.SignInManager.PasswordSignInAsync(regModel.Email, regModel.Password, false, shouldLockout: false);
      account.addUserToCk(appUsr, regModel);
      account.startCkConsole();
#endif
      */

      timerCkCalender.Elapsed += new System.Timers.ElapsedEventHandler(timerCkCalender_Elapsed);
      timerCkCalender.Enabled = false;
    }

    static void timerCkCalender_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
    {
      if (ckcore.ltUser.Count == 0) {
        timerCkCalender.Enabled = false;
        return;
      }

      ckcore.next(true);
    }
  }
}
