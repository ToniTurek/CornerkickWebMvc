using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using CornerkickWebMvc.Models;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace CornerkickWebMvc
{
  public class EmailService : IIdentityMessageService
  {
    public async Task SendAsync(IdentityMessage message)
    {
      await configSendGridasync(message);
    }

    // Use NuGet to install SendGrid (Basic C# client lib) 
    private async Task configSendGridasync(IdentityMessage message)
    {
      var apiKey = "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";
      var client = new SendGridClient(apiKey);

      var from = new EmailAddress("contact@cornerkick.net", "Cornerkick");
      var subject = message.Subject;
      var to = new EmailAddress(message.Destination);
      var plainTextContent = message.Body;
      var htmlContent = message.Body;

      var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);

      var response = await client.SendEmailAsync(msg);
    }
  }

  public class SmsService : IIdentityMessageService
  {
    public Task SendAsync(IdentityMessage message)
    {
      // Hier den SMS-Dienst einfügen, um eine Textnachricht zu senden.
      return Task.FromResult(0);
    }
  }

  // Konfigurieren des in dieser Anwendung verwendeten Anwendungsbenutzer-Managers. UserManager wird in ASP.NET Identity definiert und von der Anwendung verwendet.
  public class ApplicationUserManager : UserManager<ApplicationUser>
  {
      public ApplicationUserManager(IUserStore<ApplicationUser> store)
          : base(store)
      {
      }

      public static ApplicationUserManager Create(IdentityFactoryOptions<ApplicationUserManager> options, IOwinContext context) 
      {
          var manager = new ApplicationUserManager(new UserStore<ApplicationUser>(context.Get<ApplicationDbContext>()));

          // Konfigurieren der Überprüfungslogik für Benutzernamen.
          manager.UserValidator = new UserValidator<ApplicationUser>(manager)
          {
              AllowOnlyAlphanumericUserNames = false,
              RequireUniqueEmail = true
          };

          // Konfigurieren der Überprüfungslogik für Kennwörter.
          manager.PasswordValidator = new PasswordValidator
          {
              RequiredLength = 4,
              RequireNonLetterOrDigit = false,
              RequireDigit = false,
              RequireLowercase = false,
              RequireUppercase = false,
          };

          // Standardeinstellungen für Benutzersperren konfigurieren
          manager.UserLockoutEnabledByDefault = true;
          manager.DefaultAccountLockoutTimeSpan = TimeSpan.FromMinutes(5);
          manager.MaxFailedAccessAttemptsBeforeLockout = 5;

          // Registrieren von Anbietern für zweistufige Authentifizierung. Diese Anwendung verwendet telefonische und E-Mail-Nachrichten zum Empfangen eines Codes zum Überprüfen des Benutzers.
          // Sie können Ihren eigenen Anbieter erstellen und hier einfügen.
          manager.RegisterTwoFactorProvider("Telefoncode", new PhoneNumberTokenProvider<ApplicationUser>
          {
              MessageFormat = "Ihr Sicherheitscode lautet {0}"
          });
          manager.RegisterTwoFactorProvider("E-Mail-Code", new EmailTokenProvider<ApplicationUser>
          {
              Subject = "Sicherheitscode",
              BodyFormat = "Ihr Sicherheitscode lautet {0}"
          });
          manager.EmailService = new EmailService();
          manager.SmsService = new SmsService();
          var dataProtectionProvider = options.DataProtectionProvider;
          if (dataProtectionProvider != null)
          {
              manager.UserTokenProvider = 
                  new DataProtectorTokenProvider<ApplicationUser>(dataProtectionProvider.Create("ASP.NET Identity"));
          }
          return manager;
      }
  }

  // Anwendungsanmelde-Manager konfigurieren, der in dieser Anwendung verwendet wird.
  public class ApplicationSignInManager : SignInManager<ApplicationUser, string>
  {
      public ApplicationSignInManager(ApplicationUserManager userManager, IAuthenticationManager authenticationManager)
          : base(userManager, authenticationManager)
      {
      }

      public override Task<ClaimsIdentity> CreateUserIdentityAsync(ApplicationUser user)
      {
          return user.GenerateUserIdentityAsync((ApplicationUserManager)UserManager);
      }

      public static ApplicationSignInManager Create(IdentityFactoryOptions<ApplicationSignInManager> options, IOwinContext context)
      {
          return new ApplicationSignInManager(context.GetUserManager<ApplicationUserManager>(), context.Authentication);
      }
  }
}
