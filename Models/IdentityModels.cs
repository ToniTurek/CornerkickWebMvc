using System.Data.Entity;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace CornerkickWebMvc.Models
{
  // Sie können Profildaten für den Benutzer hinzufügen, indem Sie der ApplicationUser-Klasse weitere Eigenschaften hinzufügen. Weitere Informationen finden Sie unter https://go.microsoft.com/fwlink/?LinkID=317594.
  public class ApplicationUser : IdentityUser
  {
    public string Vorname { get; set; } 
    public string Nachname { get; set; }
    public string Vereinsname { get; set; }

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
    public ApplicationDbContext()
        : base("DefaultConnection", throwIfV1Schema: false)
    {
    }

    public static ApplicationDbContext Create()
    {
      return new ApplicationDbContext();
    }
  }
}