﻿using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Data.Entity;

namespace CornerkickWebMvc.Models
{
  // You can add profile data for the user by adding more properties to your ApplicationUser
  // class, please visit http://go.microsoft.com/fwlink/?LinkID=317594 to learn more.
  public class ApplicationUser : IdentityUser
  {
    public string Vorname { get; set; }
    public string Nachname { get; set; }
    public string Vereinsname { get; set; }
    public bool   bAdmin { get; set; }

    public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager)
    {
      // Beachten Sie, dass der "authenticationType" mit dem in "CookieAuthenticationOptions.AuthenticationType" definierten Typ übereinstimmen muss.
      var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);

      // Benutzerdefinierte Benutzeransprüche hier hinzufügen
      /*
      var user = manager.Find(userName, password);
      userIdentity.AddClaim(new Claim(ClaimTypes.Surname, user.Email));
      userIdentity.AddClaim(new Claim("Vorname", ""));
      userIdentity.AddClaim(new Claim("Nachname", ""));
      userIdentity.AddClaim(new Claim("Vereinsname", ""));
      */

      return userIdentity;
    }
  }

  public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
  {
    static ApplicationDbContext()
    {
      Database.SetInitializer(new MySqlInitializer());
    }

    public ApplicationDbContext()
      : base("ckMySqlConnection")
    {
    }

    public static ApplicationDbContext Create()
    {
      return new ApplicationDbContext();
    }
  }
}