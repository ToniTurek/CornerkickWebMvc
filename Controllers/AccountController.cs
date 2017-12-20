using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using CornerkickWebMvc.Models;

namespace CornerkickWebMvc.Controllers
{
  [Authorize]
  public class AccountController : Controller
  {
#if _CONSOLE
    public static CornerkickConsole.CUI ckconsole;
#endif
    //public static System.Timers.Timer timerRefresh = new System.Timers.Timer(10000);
    public static CornerkickCore.Core.User ckUser;
    public static CornerkickCore.Core.Club ckClub;

    private ApplicationSignInManager _signInManager;
    private ApplicationUserManager _userManager;

    public AccountController()
    {

      //timerRefresh.Elapsed += new System.Timers.ElapsedEventHandler(timerRefresh_Elapsed);
      //timerRefresh.Enabled = true;
    }

    /*
    static void timerRefresh_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
    {
      var result = new MemberController().Console();
    }
    */

#if DEBUG
    public void startCkConsole()
#else
    private void startCkConsole()
#endif
    {
#if _CONSOLE
      //ckconsole = new CornerkickConsole.CUI(MvcApplication.ckcore);
#endif
      //MemberController.iniLtsFormationen();

      foreach (CornerkickCore.Core.User usr in MvcApplication.ckcore.ltUser) {
        if (usr.id.Equals(appUser.Id)) {
          ckUser = usr;
          ckClub = MvcApplication.ckcore.ltClubs[ckUser.iTeam];

          /*
          TrainingModel.ltTraining = new List<SelectListItem>();
          for (byte i = 0; i < MvcApplication.ckcore.sTraining.Length; i++) {
            TrainingModel.ltTraining.Add(
              new SelectListItem {
                Text = MvcApplication.ckcore.sTraining[i],
                Value = i.ToString()
              }
            );
          }
          */

          return;
        }
      }
    }

#if DEBUG
    public void addUserToCk(ApplicationUser applicationUser, RegisterViewModel registerViewModel)
#else
    private void addUserToCk(ApplicationUser applicationUser, RegisterViewModel registerViewModel)
#endif
    {
      int iLand = 0;
      int iLiga = 0;

      if (registerViewModel != null) {
        iLand = registerViewModel.Land - 1;
        iLiga = registerViewModel.Liga - 1;
      }

      if (iLand < 0) return;
      if (iLiga < 0) return;

      if (MvcApplication.ckcore.ltClubs.Count == 0) MvcApplication.ckcore.ltClubs.Add(new CornerkickCore.Core.Club());

#if DEBUG
      for (byte iU = 0; iU < 10; iU++) {
#endif
        CornerkickCore.Core.User usr = createUser(applicationUser);
        CornerkickCore.Core.Club clb = createClub(applicationUser);
        usr.iTeam = clb.iID;

#if DEBUG
        clb.sName += "_" + (iU + 1).ToString();
#endif

        MvcApplication.ckcore.ltUser .Add(usr);
        MvcApplication.ckcore.ltClubs.Add(clb);

        if (MvcApplication.ckcore.ltLiga[iLand] == null) MvcApplication.ckcore.ltLiga[iLand] = new List<List<int>>();
        while (MvcApplication.ckcore.ltLiga[iLand].Count <= iLiga) MvcApplication.ckcore.ltLiga[iLand].Add(new List<int>());
        MvcApplication.ckcore.ltLiga[iLand][iLiga].Add(clb.iID);

        while (RegisterViewModel.ltSpTg.Count < (MvcApplication.ckcore.ltLiga[iLand][iLiga].Count - 1) * 2) {
          int iSpTg = RegisterViewModel.ltSpTg.Count + 1;
          RegisterViewModel.ltSpTg.Add(new SelectListItem { Text = iSpTg.ToString().PadLeft(2), Value = iSpTg.ToString().PadLeft(2), Selected = false });
        }

        MvcApplication.ckcore.doAufstellungKI(clb.iID, true, true);
#if DEBUG
      }

      MvcApplication.ckcore.setNeueSaison();
      MvcApplication.ckcore.doAufstellungKI(0, true, true);
#endif
    }

    private CornerkickCore.Core.User createUser(ApplicationUser applicationUser)
    {
      //Models.ApplicationUser appUser
      //Models.RegisterViewModel registerViewModel = new Models.RegisterViewModel();

      CornerkickCore.Core.User usr = new CornerkickCore.Core.User();
      MvcApplication.ckcore.ini.iniUser(ref usr);

      if (applicationUser == null) return usr;

      //Models.RegisterViewModel m;
      //usr.sVorname = User.Identity.Name;
      usr.id = applicationUser.Id;

      usr.sVorname  = applicationUser.Vorname;
      usr.sNachname = applicationUser.Nachname;

      return usr;
    }

    private CornerkickCore.Core.Club createClub(ApplicationUser applicationUser)
    {
      CornerkickCore.Core.Club clb = MvcApplication.ckcore.ini.newClub();
      clb.sName = applicationUser.Vereinsname;
      if (string.IsNullOrEmpty(clb.sName)) clb.sName = "Team";
      clb.iID = MvcApplication.ckcore.ltClubs.Count;

      addPlayerToClub(ref clb);

      return clb;
    }

    private void addPlayerToClub(ref CornerkickCore.Core.Club club)
    {
      Random rnd = new Random();
      for (byte iPos = 1; iPos < 12; iPos++) {
        for (byte iPl = 0; iPl < 2; iPl++) {
          CornerkickGame.Game.Spieler sp = MvcApplication.ckcore.sp.createSpieler(iPos, false, 0, club);
#if DEBUG
          sp.fFrische = 1f;
#endif
          sp.vertrag.iDauer = (byte)rnd.Next(1, 4);
          //sp.vertrag.iGehalt = MvcApplication.ckcore.sp.getGefGehaltSpieler(sp, sp.vertrag.iDauer, 100000);
          sp.iNr = iPos + (11 * iPl);
          MvcApplication.ckcore.ltSpieler[sp.iID] = sp;
          club.ltSpielerID.Add(sp.iID);
        }
      }
    }

    public AccountController(ApplicationUserManager userManager, ApplicationSignInManager signInManager )
    {
      UserManager = userManager;
      SignInManager = signInManager;
    }

    public ApplicationSignInManager SignInManager
    {
        get
        {
            return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
        }
        private set 
        { 
            _signInManager = value; 
        }
    }

    public ApplicationUserManager UserManager
    {
        get
        {
            return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
        }
        private set
        {
            _userManager = value;
        }
    }

      //
      // GET: /Account/Login
      [AllowAnonymous]
      public ActionResult Login(string returnUrl)
      {
          ViewBag.ReturnUrl = returnUrl;
          return View();
      }

    //
    // POST: /Account/Login
    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult> Login(LoginViewModel model, string returnUrl)
    {
      if (!ModelState.IsValid)
      {
        return View(model);
      }

      // Anmeldefehler werden bezüglich einer Kontosperre nicht gezählt.
      // Wenn Sie aktivieren möchten, dass Kennwortfehler eine Sperre auslösen, ändern Sie in "shouldLockout: true".
      var result = await SignInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, shouldLockout: false);
      switch (result)
      {
        case SignInStatus.Success:
          //return RedirectToLocal(returnUrl);
          //return View("Members");
          //return Redirect("~/MemberPages/Saison");
          appUser = new ApplicationUser { UserName = model.Email, Email = model.Email };

#if DEBUG
          addUserToCk(appUser, rvmDEBUG);
#endif
          startCkConsole();

          //return RedirectToAction("Console", "Member");
          return View(model);
        case SignInStatus.LockedOut:
          return View("Lockout");
        case SignInStatus.RequiresVerification:
          return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = model.RememberMe });
        case SignInStatus.Failure:
        default:
          ModelState.AddModelError("", "Ungültiger Anmeldeversuch.");
          return View(model);
      }
    }

      //
      // GET: /Account/VerifyCode
      [AllowAnonymous]
      public async Task<ActionResult> VerifyCode(string provider, string returnUrl, bool rememberMe)
      {
          // Verlangen, dass sich der Benutzer bereits mit seinem Benutzernamen/Kennwort oder einer externen Anmeldung angemeldet hat.
          if (!await SignInManager.HasBeenVerifiedAsync())
          {
              return View("Error");
          }
          return View(new VerifyCodeViewModel { Provider = provider, ReturnUrl = returnUrl, RememberMe = rememberMe });
      }

      //
      // POST: /Account/VerifyCode
      [HttpPost]
      [AllowAnonymous]
      [ValidateAntiForgeryToken]
      public async Task<ActionResult> VerifyCode(VerifyCodeViewModel model)
      {
          if (!ModelState.IsValid)
          {
              return View(model);
          }

          // Der folgende Code schützt vor Brute-Force-Angriffen der zweistufigen Codes. 
          // Wenn ein Benutzer in einem angegebenen Zeitraum falsche Codes eingibt, wird das Benutzerkonto 
          // für einen bestimmten Zeitraum gesperrt. 
          // Sie können die Einstellungen für Kontosperren in "IdentityConfig" konfigurieren.
          var result = await SignInManager.TwoFactorSignInAsync(model.Provider, model.Code, isPersistent:  model.RememberMe, rememberBrowser: model.RememberBrowser);
          switch (result)
          {
              case SignInStatus.Success:
                  return RedirectToLocal(model.ReturnUrl);
              case SignInStatus.LockedOut:
                  return View("Lockout");
              case SignInStatus.Failure:
              default:
                  ModelState.AddModelError("", "Ungültiger Code.");
                  return View(model);
          }
      }

      //
      // GET: /Account/Register
      [AllowAnonymous]
      public ActionResult Register()
      {
          return View();
      }

    //
    // POST: /Account/Register
    public static ApplicationUser appUser;
    private RegisterViewModel rvmDEBUG;
    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult> Register(RegisterViewModel model)
    {
      DebugInfo("Register");

      if (ModelState.IsValid)
      {
        //appUser = new ApplicationUser { UserName = model.Vorname + " " + model.Nachname, Email = model.Email, Vorname = model.Vorname, Nachname = model.Nachname, Vereinsname = model.Verein };
        appUser = new ApplicationUser { UserName = model.Email, Email = model.Email, Vorname = model.Vorname, Nachname = model.Nachname, Vereinsname = model.Verein };
        var result = await UserManager.CreateAsync(appUser, model.Password);
        if (result.Succeeded)
        {
          await SignInManager.SignInAsync(appUser, isPersistent:false, rememberBrowser:false);

          // Weitere Informationen zum Aktivieren der Kontobestätigung und Kennwortzurücksetzung finden Sie unter https://go.microsoft.com/fwlink/?LinkID=320771
          // E-Mail-Nachricht mit diesem Link senden
          // string code = await UserManager.GenerateEmailConfirmationTokenAsync(usr.Id);
          // var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = usr.Id, code = code }, protocol: Request.Url.Scheme);
          // await UserManager.SendEmailAsync(usr.Id, "Konto bestätigen", "Bitte bestätigen Sie Ihr Konto. Klicken Sie dazu <a href=\"" + callbackUrl + "\">hier</a>");

          rvmDEBUG = model;
          addUserToCk(appUser, model);
          startCkConsole();

          return RedirectToAction("Index", "Home");
        }
        AddErrors(result);
      }

      // Wurde dieser Punkt erreicht, ist ein Fehler aufgetreten; Formular erneut anzeigen.
      return View(model);
    }

    private string DebugInfo(string sText)
    {
      return HttpUtility.HtmlEncode(sText);
    }

    //
    // GET: /Account/ConfirmEmail
    [AllowAnonymous]
      public async Task<ActionResult> ConfirmEmail(string userId, string code)
      {
          if (userId == null || code == null)
          {
              return View("Error");
          }
          var result = await UserManager.ConfirmEmailAsync(userId, code);
          return View(result.Succeeded ? "ConfirmEmail" : "Error");
      }

      //
      // GET: /Account/ForgotPassword
      [AllowAnonymous]
      public ActionResult ForgotPassword()
      {
          return View();
      }

      //
      // POST: /Account/ForgotPassword
      [HttpPost]
      [AllowAnonymous]
      [ValidateAntiForgeryToken]
      public async Task<ActionResult> ForgotPassword(ForgotPasswordViewModel model)
      {
          if (ModelState.IsValid)
          {
              var usr = await UserManager.FindByNameAsync(model.Email);
              if (usr == null || !(await UserManager.IsEmailConfirmedAsync(usr.Id)))
              {
                  // Nicht anzeigen, dass der Benutzer nicht vorhanden ist oder nicht bestätigt wurde.
                  return View("ForgotPasswordConfirmation");
              }

              // Weitere Informationen zum Aktivieren der Kontobestätigung und Kennwortzurücksetzung finden Sie unter https://go.microsoft.com/fwlink/?LinkID=320771
              // E-Mail-Nachricht mit diesem Link senden
              // string code = await UserManager.GeneratePasswordResetTokenAsync(usr.Id);
              // var callbackUrl = Url.Action("ResetPassword", "Account", new { userId = usr.Id, code = code }, protocol: Request.Url.Scheme);		
              // await UserManager.SendEmailAsync(usr.Id, "Kennwort zurücksetzen", "Bitte setzen Sie Ihr Kennwort zurück. Klicken Sie dazu <a href=\"" + callbackUrl + "\">hier</a>");
              // return RedirectToAction("ForgotPasswordConfirmation", "Account");
          }

          // Wurde dieser Punkt erreicht, ist ein Fehler aufgetreten; Formular erneut anzeigen.
          return View(model);
      }

      //
      // GET: /Account/ForgotPasswordConfirmation
      [AllowAnonymous]
      public ActionResult ForgotPasswordConfirmation()
      {
          return View();
      }

      //
      // GET: /Account/ResetPassword
      [AllowAnonymous]
      public ActionResult ResetPassword(string code)
      {
          return code == null ? View("Error") : View();
      }

      //
      // POST: /Account/ResetPassword
      [HttpPost]
      [AllowAnonymous]
      [ValidateAntiForgeryToken]
      public async Task<ActionResult> ResetPassword(ResetPasswordViewModel model)
      {
          if (!ModelState.IsValid)
          {
              return View(model);
          }
          var usr = await UserManager.FindByNameAsync(model.Email);
          if (usr == null)
          {
              // Nicht anzeigen, dass der Benutzer nicht vorhanden ist.
              return RedirectToAction("ResetPasswordConfirmation", "Account");
          }
          var result = await UserManager.ResetPasswordAsync(usr.Id, model.Code, model.Password);
          if (result.Succeeded)
          {
              return RedirectToAction("ResetPasswordConfirmation", "Account");
          }
          AddErrors(result);
          return View();
      }

      //
      // GET: /Account/ResetPasswordConfirmation
      [AllowAnonymous]
      public ActionResult ResetPasswordConfirmation()
      {
          return View();
      }

      //
      // POST: /Account/ExternalLogin
      [HttpPost]
      [AllowAnonymous]
      [ValidateAntiForgeryToken]
      public ActionResult ExternalLogin(string provider, string returnUrl)
      {
          // Umleitung an den externen Anmeldeanbieter anfordern
          return new ChallengeResult(provider, Url.Action("ExternalLoginCallback", "Account", new { ReturnUrl = returnUrl }));
      }

      //
      // GET: /Account/SendCode
      [AllowAnonymous]
      public async Task<ActionResult> SendCode(string returnUrl, bool rememberMe)
      {
          var userId = await SignInManager.GetVerifiedUserIdAsync();
          if (userId == null)
          {
              return View("Error");
          }
          var userFactors = await UserManager.GetValidTwoFactorProvidersAsync(userId);
          var factorOptions = userFactors.Select(purpose => new SelectListItem { Text = purpose, Value = purpose }).ToList();
          return View(new SendCodeViewModel { Providers = factorOptions, ReturnUrl = returnUrl, RememberMe = rememberMe });
      }

      //
      // POST: /Account/SendCode
      [HttpPost]
      [AllowAnonymous]
      [ValidateAntiForgeryToken]
      public async Task<ActionResult> SendCode(SendCodeViewModel model)
      {
          if (!ModelState.IsValid)
          {
              return View();
          }

          // Token generieren und senden
          if (!await SignInManager.SendTwoFactorCodeAsync(model.SelectedProvider))
          {
              return View("Error");
          }
          return RedirectToAction("VerifyCode", new { Provider = model.SelectedProvider, ReturnUrl = model.ReturnUrl, RememberMe = model.RememberMe });
      }

      //
      // GET: /Account/ExternalLoginCallback
      [AllowAnonymous]
      public async Task<ActionResult> ExternalLoginCallback(string returnUrl)
      {
        var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync();
        if (loginInfo == null)
        {
          return RedirectToAction("Login");
        }

        // Benutzer mit diesem externen Anmeldeanbieter anmelden, wenn der Benutzer bereits eine Anmeldung besitzt
        var result = await SignInManager.ExternalSignInAsync(loginInfo, isPersistent: false);
        switch (result)
        {
            case SignInStatus.Success:
        //return RedirectToLocal(returnUrl);
        return RedirectToAction("Login");
      case SignInStatus.LockedOut:
                return View("Lockout");
            case SignInStatus.RequiresVerification:
                return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = false });
            case SignInStatus.Failure:
            default:
                // Benutzer auffordern, ein Konto zu erstellen, wenn er kein Konto besitzt
                ViewBag.ReturnUrl = returnUrl;
                ViewBag.LoginProvider = loginInfo.Login.LoginProvider;
                return View("ExternalLoginConfirmation", new ExternalLoginConfirmationViewModel { Email = loginInfo.Email });
        }
      }

      //
      // POST: /Account/ExternalLoginConfirmation
      [HttpPost]
      [AllowAnonymous]
      [ValidateAntiForgeryToken]
      public async Task<ActionResult> ExternalLoginConfirmation(ExternalLoginConfirmationViewModel model, string returnUrl)
      {
          if (User.Identity.IsAuthenticated)
          {
              return RedirectToAction("Index", "Manage");
          }

          if (ModelState.IsValid)
          {
              // Informationen zum Benutzer aus dem externen Anmeldeanbieter abrufen
              var info = await AuthenticationManager.GetExternalLoginInfoAsync();
              if (info == null)
              {
                  return View("ExternalLoginFailure");
              }
              var usr = new ApplicationUser { UserName = model.Email, Email = model.Email };
              var result = await UserManager.CreateAsync(usr);
              if (result.Succeeded)
              {
                  result = await UserManager.AddLoginAsync(usr.Id, info.Login);
                  if (result.Succeeded)
                  {
                      await SignInManager.SignInAsync(usr, isPersistent: false, rememberBrowser: false);
                      return RedirectToLocal(returnUrl);
                  }
              }
              AddErrors(result);
          }

          ViewBag.ReturnUrl = returnUrl;
          return View(model);
      }

      //
      // POST: /Account/LogOff
      [HttpPost]
      [ValidateAntiForgeryToken]
      public ActionResult LogOff()
      {
          AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
          return RedirectToAction("Index", "Home");
      }

      //
      // GET: /Account/ExternalLoginFailure
      [AllowAnonymous]
      public ActionResult ExternalLoginFailure()
      {
          return View();
      }

      protected override void Dispose(bool disposing)
      {
          if (disposing)
          {
              if (_userManager != null)
              {
                  _userManager.Dispose();
                  _userManager = null;
              }

              if (_signInManager != null)
              {
                  _signInManager.Dispose();
                  _signInManager = null;
              }
          }

          base.Dispose(disposing);
      }

#region Hilfsprogramme
      // Wird für XSRF-Schutz beim Hinzufügen externer Anmeldungen verwendet
      private const string XsrfKey = "XsrfId";

      private IAuthenticationManager AuthenticationManager
      {
          get
          {
              return HttpContext.GetOwinContext().Authentication;
          }
      }

      private void AddErrors(IdentityResult result)
      {
          foreach (var error in result.Errors)
          {
              ModelState.AddModelError("", error);
          }
      }

      private ActionResult RedirectToLocal(string returnUrl)
      {
          if (Url.IsLocalUrl(returnUrl))
          {
              return Redirect(returnUrl);
          }
          return RedirectToAction("Index", "Home");
      }

      internal class ChallengeResult : HttpUnauthorizedResult
      {
          public ChallengeResult(string provider, string redirectUri)
              : this(provider, redirectUri, null)
          {
          }

          public ChallengeResult(string provider, string redirectUri, string userId)
          {
              LoginProvider = provider;
              RedirectUri = redirectUri;
              UserId = userId;
          }

          public string LoginProvider { get; set; }
          public string RedirectUri { get; set; }
          public string UserId { get; set; }

          public override void ExecuteResult(ControllerContext context)
          {
              var properties = new AuthenticationProperties { RedirectUri = RedirectUri };
              if (UserId != null)
              {
                  properties.Dictionary[XsrfKey] = UserId;
              }
              context.HttpContext.GetOwinContext().Authentication.Challenge(properties, LoginProvider);
          }
      }
#endregion
  }
}