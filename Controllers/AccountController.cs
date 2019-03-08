using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
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

    private Random random = new Random();
    private ApplicationSignInManager _signInManager;
    private ApplicationUserManager _userManager;
    public static CultureInfo ciUser;
    string[] sCultureInfo = new string[3] { "de-DE", "en-GB", "fr-FR"};

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
    public void iniCk()
#else
    private void iniCk()
#endif
    {
#if _CONSOLE
      //ckconsole = new CornerkickConsole.CUI(MvcApplication.ckcore);
#endif

      CornerkickCore.Core.User usr = ckUser();
      if (usr.iTeam >= MvcApplication.ckcore.ltClubs.Count) return;

      CornerkickCore.Core.Club clb = ckClub();
      int iLandUser = clb.iLand;
      if (iLandUser >= 0 && iLandUser < sCultureInfo.Length) ciUser = new CultureInfo(sCultureInfo[iLandUser]);

      if (usr.ltNews != null) {
        for (int iN = 0; iN < usr.ltNews.Count; iN++) {
          CornerkickCore.Core.News news = usr.ltNews[iN];
          if (news.bRead2) news.bRead = true;

          news.bRead2 = true;

          usr.ltNews[iN] = news;
        }
      }
    }

    public static CornerkickCore.Core.User ckUser()
    {
      if (appUser != null) {
        foreach (CornerkickCore.Core.User usr in MvcApplication.ckcore.ltUser) {
          if (usr.id.Equals(appUser.Id)) {
            return usr;
          }
        }
      }

      return new CornerkickCore.Core.User();
    }

    public static int getiUser(CornerkickCore.Core.User usr)
    {
      for (int iU = 0; iU < MvcApplication.ckcore.ltUser.Count; iU++) {
        if (MvcApplication.ckcore.ltUser[iU].id.Equals(usr.id)) {
          return iU;
        }
      }

      return -1;
    }

    public static void setCkUser(CornerkickCore.Core.User usr)
    {
      int iU = getiUser(usr);
      if (iU >= 0 && iU < MvcApplication.ckcore.ltUser.Count) {
        MvcApplication.ckcore.ltUser[iU] = usr;
      }
    }

    public static CornerkickCore.Core.Club ckClub()
    {
      CornerkickCore.Core.User usr = ckUser();
      if (usr.iTeam < MvcApplication.ckcore.ltClubs.Count) {
        return MvcApplication.ckcore.ltClubs[usr.iTeam];
      }

      return new CornerkickCore.Core.Club();
    }

#if DEBUG
    public void addUserToCk(ApplicationUser applicationUser, RegisterViewModel registerViewModel, bool bAdmin = false)
#else
    private void addUserToCk(ApplicationUser applicationUser, RegisterViewModel registerViewModel, bool bAdmin = false)
#endif
    {
      int  iLand = 0;
      byte iLiga = 0;

      MvcApplication.ckcore.tl.writeLog("  add User to Ck: " + applicationUser.Vorname + " " + applicationUser.Nachname);

      if (bAdmin) {
        MvcApplication.ckcore.ltClubs.Add(MvcApplication.ckcore.ini.newClub());
        CornerkickCore.Core.User usrAdmin = createUser(applicationUser);
        MvcApplication.ckcore.ltUser.Add(usrAdmin);

        return;
      }

      if (registerViewModel != null) {
        iLand = registerViewModel.Land - 1;
        iLiga = (byte)(registerViewModel.Liga - 1);
      }

      if (iLand < 0) return;
      if (iLiga < 0) return;

#if DEBUG
      for (byte iU = 0; iU < 8; iU++) {
#endif
      CornerkickCore.Core.User usr = createUser(applicationUser);
      CornerkickCore.Core.Club clb = createClub(applicationUser, iLand, iLiga);
      usr.iLevel = 1;
      usr.iTeam = clb.iId;
      usr.nextGame.iGameSpeed = 250;
#if DEBUG
      if (iU == 0) {
#endif
      clb.iUser = MvcApplication.ckcore.ltUser.Count;
#if DEBUG
      }
#endif

#if DEBUG
        clb.sName += "_" + clb.iId.ToString();
#endif

#if DEBUG
      if (iU == 0) {
#endif
      MvcApplication.ckcore.ltUser .Add(usr);
#if DEBUG
      }
#endif
      MvcApplication.ckcore.ltClubs.Add(clb);

      if (MvcApplication.ckcore.ltLiga[iLand] == null) MvcApplication.ckcore.ltLiga[iLand] = new List<List<int>>();
      while (MvcApplication.ckcore.ltLiga[iLand].Count <= iLiga) MvcApplication.ckcore.ltLiga[iLand].Add(new List<int>());
      MvcApplication.ckcore.ltLiga[iLand][iLiga].Add(clb.iId);

      // do initial formation
      MvcApplication.ckcore.doFormationKI(clb.iId, true);

#if DEBUG
      if (iU == 0) {
#endif
      MvcApplication.ckcore.Info(clb.iUser, usr.sVorname + " " + usr.sNachname + ", herzlich Willkommen bei Ihrem neuen Verein " + clb.sName + "!",  2, usr.iTeam, 1);
      MvcApplication.ckcore.Info(clb.iUser, usr.sVorname + " " + usr.sNachname + ", herzlich Willkommen bei Ihrem neuen Verein " + clb.sName + "!", 99, usr.iTeam, 1, System.DateTime.Now, -1);
#if DEBUG
      }
#endif

#if DEBUG
      }
#endif

      if (!MvcApplication.timerCkCalender.Enabled) MvcApplication.ckcore.calcSpieltage();
    }

    private CornerkickCore.Core.User createUser(ApplicationUser applicationUser)
    {
      CornerkickCore.Core.User usr = MvcApplication.ckcore.ini.newUser();

      if (applicationUser == null) return usr;

      usr.id = applicationUser.Id;

      usr.sVorname  = applicationUser.Vorname;
      usr.sNachname = applicationUser.Nachname;
      
      return usr;
    }

    internal static bool checkUserIsAdmin(string sEmail, string sPw = "")
    {
      if (!sEmail.Equals("admin@ck.com")) return false;

      if (!string.IsNullOrEmpty(sPw)) {
        if (!sPw.Equals("!Cornerkick1")) return false;
      }

      return true;
    }

    internal static bool checkUserIsAdmin(ApplicationUser appUserAdmin)
    {
      if (appUserAdmin == null) return false;

      return checkUserIsAdmin(appUserAdmin.Email);
    }

    internal static bool checkUserIsAdmin(RegisterViewModel regModel)
    {
      return checkUserIsAdmin(regModel.Email, regModel.Password);
    }

    private CornerkickCore.Core.Club createClub(ApplicationUser applicationUser, int iLand, byte iLiga)
    {
      CornerkickCore.Core.Club clb = MvcApplication.ckcore.ini.newClub();
      clb.sName = applicationUser.Vereinsname;
      if (string.IsNullOrEmpty(clb.sName)) clb.sName = "Team";
      clb.iId = MvcApplication.ckcore.ltClubs.Count;
      clb.stadium.sName = clb.sName +  " Stadion";

      clb.iLand = iLand;
      clb.iDivision = iLiga;
      clb.iPokalrunde = 0;

      MvcApplication.ckcore.tl.setFormationToClub(ref clb, MvcApplication.ckcore.ltFormationen[12]);

#if DEBUG
      clb.training.iTraining[1] = 2;
      clb.training.iTraining[2] = 3;
      clb.training.iTraining[3] = 4;
      clb.training.iTraining[4] = 6;
      clb.training.iTraining[5] = 9;
      clb.training.iTraining[6] = 1;
#endif

      for (byte iB = 0; iB < 3; iB++) {
        clb.stadium.blocks[iB].iSeats = 2000;
        clb.stadium.blocks[iB].iType = 1;
      }
      for (byte iB = 3; iB < 8; iB++) clb.stadium.blocks[iB].iSeats = 1000;

      clb.ltSponsorOffers.Add(createDefaultSponsor());

      clb.iAdmissionPrice[0] =  10;
      clb.iAdmissionPrice[1] =  30;
      clb.iAdmissionPrice[2] = 100;

      addPlayerToClub(ref clb);

      return clb;
    }

    private CornerkickCore.Finance.Sponsor createDefaultSponsor()
    {
      CornerkickCore.Finance.Sponsor sponUser = MvcApplication.ckcore.fz.newSponsor();

      sponUser.bMain = true;
      sponUser.iId = 1;
      sponUser.iYears = 2;
      sponUser.iGeldJahr     = 8000000; // 8 mio.
      sponUser.iGeldMeister  = 1000000; // 1 mio.
      sponUser.iMoneyVicHome =   10000;

      return sponUser;
    }

    private void addPlayerToClub(ref CornerkickCore.Core.Club club)
    {
      int   iSpeed  = 0;
      int   iTalent = 0;
      float fAlter  = 0f;
      Random rnd = new Random();

      for (byte iPos = 1; iPos < 12; iPos++) {
        for (byte iPl = 0; iPl < 2; iPl++) {
          CornerkickGame.Player pl = MvcApplication.ckcore.plr.newPlayer(club, iPos);
#if DEBUG
          pl.fFresh = 1f;
#endif
          pl.contract.iLength = (byte)rnd.Next(1, 4);
          pl.contract.iSalary = MvcApplication.ckcore.plr.getSalary(pl, pl.contract.iLength, 0);

          pl.iNr = (byte)(iPos + (11 * iPl));

          // Count speed
          iSpeed  += pl.iSkill[0];

          // Count talent
          iTalent += pl.iTalent;

          // Count age
          float fAge = pl.getAge(MvcApplication.ckcore.dtDatum);
          fAlter += fAge;
        }
      }

      // Equalize player speed
      byte iCount7 = 0;
      byte iCount5 = 0;
      for (int iPl = 0; iPl < club.ltPlayerId.Count; iPl++) {
        CornerkickGame.Player pl = MvcApplication.ckcore.ltPlayer[club.ltPlayerId[iPl]];
        pl.iSkill[0] = 6;
        MvcApplication.ckcore.ltPlayer[club.ltPlayerId[iPl]] = pl;
      }

      while (iCount7 < 2 || iCount5 < 1) {
        int iPlId = club.ltPlayerId[random.Next(club.ltPlayerId.Count)];
        CornerkickGame.Player pl = MvcApplication.ckcore.ltPlayer[iPlId];

        if (!MvcApplication.ckcore.game.tl.bPlayerMainPos(pl, 1)) { // if not keeper
          if (iCount7 < 2) {
            pl.iSkill[0] = 7;
            iCount7++;
          } else if (iCount5 < 1) {
            pl.iSkill[0] = 5;
            iCount5++;
          }
        }
      }
      /*
      while (iSpeed < 6 * club.ltPlayerId.Count) {
        for (byte iPl = 0; iPl < club.ltPlayerId.Count; iPl++) {
          CornerkickGame.Player pl = MvcApplication.ckcore.ltPlayer[club.ltPlayerId[iPl]];
          if (pl.iF[0] < 6) {
            pl.iF[0]++;
            MvcApplication.ckcore.ltPlayer[club.ltPlayerId[iPl]] = pl;

            iSpeed++;
            break;
          }
        }
      }

      while (iSpeed > 6 * club.ltPlayerId.Count) {
        for (byte iPl = 0; iPl < club.ltPlayerId.Count; iPl++) {
          CornerkickGame.Player pl = MvcApplication.ckcore.ltPlayer[club.ltPlayerId[iPl]];
          if (pl.iF[0] > 6) {
            pl.iF[0]--;
            MvcApplication.ckcore.ltPlayer[club.ltPlayerId[iPl]] = pl;

            iSpeed++;
            break;
          }
        }
      }
      */

      // Equalize player talent
      while (iTalent > 4.51 * club.ltPlayerId.Count) {
        int iPlId = club.ltPlayerId[random.Next(club.ltPlayerId.Count)];
        CornerkickGame.Player pl = MvcApplication.ckcore.ltPlayer[iPlId];
        if (pl.iTalent > 2) {
          pl.iTalent--;

          iTalent--;
        }
      }

      while (iTalent < 4.49 * club.ltPlayerId.Count) {
        int iPlId = club.ltPlayerId[random.Next(club.ltPlayerId.Count)];
        CornerkickGame.Player pl = MvcApplication.ckcore.ltPlayer[iPlId];
        if (pl.iTalent < 7) {
          pl.iTalent++;

          iTalent++;
        }
      }

      // Equalize player age
      while (fAlter > 25.5f * club.ltPlayerId.Count) {
        int iPlId = club.ltPlayerId[random.Next(club.ltPlayerId.Count)];
        CornerkickGame.Player pl = MvcApplication.ckcore.ltPlayer[iPlId];

        if (pl.getAge(MvcApplication.ckcore.dtDatum) > 22) {
          try {
            pl.dtBirthday = new DateTime(pl.dtBirthday.Year + 1, pl.dtBirthday.Month, pl.dtBirthday.Day); // make younger
          } catch {
            continue;
          }

          fAlter -= 1f;
        }
      }

      while (fAlter < 25.5f * club.ltPlayerId.Count) {
        int iPlId = club.ltPlayerId[random.Next(club.ltPlayerId.Count)];
        CornerkickGame.Player pl = MvcApplication.ckcore.ltPlayer[iPlId];

        if (pl.getAge(MvcApplication.ckcore.dtDatum) < 30) {
          try {
            pl.dtBirthday = new DateTime(pl.dtBirthday.Year - 1, pl.dtBirthday.Month, pl.dtBirthday.Day); // make older
          } catch {
            continue;
          }

          fAlter += 1f;
        }
      }

      // Equalize factor of player (age - 10) * talent 
      // --> (25.5 - 10) * 4.5 = 69.75
      int iBreak = 0;
      float fAgeTalent = getAgeTalent(club);
      while (fAgeTalent > 70.75 || fAgeTalent < 68.75) {
        int iPlId1 = club.ltPlayerId[random.Next(club.ltPlayerId.Count)];
        CornerkickGame.Player pl1 = MvcApplication.ckcore.ltPlayer[iPlId1];

        int iPlId2 = club.ltPlayerId[random.Next(club.ltPlayerId.Count)];
        CornerkickGame.Player pl2 = MvcApplication.ckcore.ltPlayer[iPlId2];

        if (pl1.iTalent > 2 && pl1.iTalent < 7 &&
            pl2.iTalent > 2 && pl2.iTalent < 7) {
          int iDeltaTalent = +1;
          if ((fAgeTalent > 70.75 && pl1.dtBirthday.Year > pl2.dtBirthday.Year) ||
              (fAgeTalent < 68.75 && pl1.dtBirthday.Year < pl2.dtBirthday.Year)) iDeltaTalent = -1;

          pl1.iTalent -= iDeltaTalent;
          pl2.iTalent += iDeltaTalent;

          fAgeTalent = getAgeTalent(club);
        } else if (pl1.getAge(MvcApplication.ckcore.dtDatum) > 22 &&
                   pl2.getAge(MvcApplication.ckcore.dtDatum) < 30) {
          try {
            pl1.dtBirthday = new DateTime(pl1.dtBirthday.Year + 1, pl1.dtBirthday.Month, pl1.dtBirthday.Day); // make younger
          } catch {
            MvcApplication.ckcore.tl.writeLog("ERROR: cannot make player " + pl1.sName + " younger. Player birthday: " + pl1.dtBirthday.ToShortDateString());
          }

          try {
            pl2.dtBirthday = new DateTime(pl2.dtBirthday.Year - 1, pl2.dtBirthday.Month, pl2.dtBirthday.Day); // make older
          } catch {
            MvcApplication.ckcore.tl.writeLog("ERROR: cannot make player " + pl2.sName + " older. Player birthday: " + pl2.dtBirthday.ToShortDateString());
          }

          fAgeTalent = getAgeTalent(club);
        }

        iBreak++;
        if (iBreak > 1000) {
          break;
        }
      }
    }

    private float getAgeTalent(CornerkickCore.Core.Club club)
    {
      float fAgeTalent = 0f;

      foreach (int iPlId in club.ltPlayerId) {
        CornerkickGame.Player pl = MvcApplication.ckcore.ltPlayer[iPlId];

        // Count age
        fAgeTalent += (pl.getAge(MvcApplication.ckcore.dtDatum) - 10f) * pl.iTalent;
      }

      return fAgeTalent / club.ltPlayerId.Count;
    }

    [HttpPost]
    public bool uploadFile(HttpPostedFileBase file, int iClub)
    {
      try {
        if (file.ContentLength > 0) {
          file.SaveAs(Path.Combine(Server.MapPath("~/App_Data/Uploads"), iClub.ToString() + ".png"));

          AmazonS3FileTransfer as3 = new AmazonS3FileTransfer();
          as3.uploadFile(file.FileName, "ckEmblem_" + iClub.ToString());
        }

        return true;
      } catch {
        return false;
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
      MvcApplication.ckcore.tl.writeLog("Login of user: " + model.Email);

      if (!ModelState.IsValid)
      {
        return View(model);
      }

      // Anmeldefehler werden bezüglich einer Kontosperre nicht gezählt.
      // Wenn Sie aktivieren möchten, dass Kennwortfehler eine Sperre auslösen, ändern Sie in "shouldLockout: true".
      var result = await SignInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, shouldLockout: false);
      MvcApplication.ckcore.tl.writeLog("  Login result: " + result.ToString());

      switch (result)
      {
        case SignInStatus.Success:
          //appUser = new ApplicationUser { UserName = model.Email, Email = model.Email };
          appUser = await UserManager.FindByNameAsync(model.Email);
          if (appUser == null/* || !(await UserManager.IsEmailConfirmedAsync(appUser.Id))*/) {
            // Don't reveal that the user does not exist or is not confirmed

            ModelState.AddModelError("", "Ungültiger Anmeldeversuch.");

            return View("ForgotPasswordConfirmation");
          }

          iniCk();

          return RedirectToAction("Desk", "Member");
          //return RedirectToAction("Index", "Home");
          //return View(model);
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
    public async Task<ActionResult> Register(RegisterViewModel model, HttpPostedFileBase file)
    {
      if (file != null) {
        List<string> sImageExtensions = new List<string> { ".JPG", ".JPE", ".BMP", ".GIF", ".PNG" };
        if (!sImageExtensions.Contains(Path.GetExtension(file.FileName).ToUpperInvariant())) {
          ModelState.AddModelError("", "Nur Bilddateien möglich!");
          return View(model);
        }
      }

      DebugInfo("Register");

      MvcApplication.ckcore.tl.writeLog("Register new user: " + model.Vorname + " " + model.Nachname + ", Team: " + model.Verein);

      if (ModelState.IsValid) {
        //appUser = new ApplicationUser { UserName = model.Vorname + " " + model.Nachname, Email = model.Email, Vorname = model.Vorname, Nachname = model.Nachname, Vereinsname = model.Verein };
        appUser = new ApplicationUser { UserName = model.Email, Email = model.Email, Vorname = model.Vorname, Nachname = model.Nachname, Vereinsname = model.Verein };

        // first user must be admin
        if (MvcApplication.ckcore.ltUser.Count == 0) {
          if (!checkUserIsAdmin(model)) return View(model);
        }

        var result = await UserManager.CreateAsync(appUser, model.Password);
        MvcApplication.ckcore.tl.writeLog("  Register succeeded: " + result.Succeeded.ToString());

        if (result.Succeeded) {
          await SignInManager.SignInAsync(appUser, isPersistent:false, rememberBrowser:false);

          // Weitere Informationen zum Aktivieren der Kontobestätigung und Kennwortzurücksetzung finden Sie unter https://go.microsoft.com/fwlink/?LinkID=320771
          // E-Mail-Nachricht mit diesem Link senden
          // string code = await UserManager.GenerateEmailConfirmationTokenAsync(usr.Id);
          // var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = usr.Id, code = code }, protocol: Request.Url.Scheme);
          // await UserManager.SendEmailAsync(usr.Id, "Konto bestätigen", "Bitte bestätigen Sie Ihr Konto. Klicken Sie dazu <a href=\"" + callbackUrl + "\">hier</a>");

          rvmDEBUG = model;

          if (MvcApplication.ckcore.ltUser.Count == 0/* && MvcApplication.ckcore.ltClubs.Count == 0*/) { // admin user
            CornerkickCore.Core.User usrAdmin = createUser(appUser);
            MvcApplication.ckcore.ltUser.Add(usrAdmin);

            // Initialize dummy club
            CornerkickCore.Core.Club club0 = MvcApplication.ckcore.ini.newClub();
            club0.iId = 0;
            club0.sName = "Computer";

            club0.personal.iJugendScouting = 10;

            for (int iPl = 0; iPl < 100; iPl++) {
              CornerkickGame.Player sp = MvcApplication.ckcore.plr.newPlayer(club0);
              MvcApplication.ckcore.ui.putPlayerOnTransferlist(sp.iId, 0);
            }

            MvcApplication.ckcore.ltClubs.Add(club0);
            // END Initialize dummy club
          } else { // no admin
            addUserToCk(appUser, model);
            uploadFile(file, ckClub().iId);
            iniCk();
          }

          MvcApplication.save();
          /*
#if DEBUG
          MvcApplication.ckcore.io.save("C:\\Users\\Jan\\Documents\\Visual Studio 2017\\Projects\\Cornerkick.git\\CornerkickWebMvc\\save\\.autosave.ck");
#else
          var path = Server.MapPath("~/save");
          if (!System.IO.Directory.Exists(path)) {
            System.IO.Directory.CreateDirectory(path);
          }
          MvcApplication.ckcore.io.save(System.Web.HttpContext.Current.Server.MapPath("~/save/.autosave.ck"));
#endif
          */

          return RedirectToAction("Desk", "Member");
          //return RedirectToAction("Index", "Home");
        }

        AddErrors(result);
      }

      // Wurde dieser Punkt erreicht, ist ein Fehler aufgetreten; Formular erneut anzeigen.
      return View(model);
    }

    [AllowAnonymous]
    [HttpPost]
    public ActionResult CheckExistingTeamName(string Verein)
    {
      foreach (CornerkickCore.Core.Club clubExist in MvcApplication.ckcore.ltClubs) {
        if (clubExist.sName.Equals(Verein)) return Json(false, JsonRequestBehavior.AllowGet);
      }
      return Json(true, JsonRequestBehavior.AllowGet);
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