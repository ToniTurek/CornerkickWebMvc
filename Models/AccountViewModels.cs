using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Drawing;

namespace CornerkickWebMvc.Models
{
    public class ExternalLoginConfirmationViewModel
    {
        [Required]
        [Display(Name = "E-Mail")]
        public string Email { get; set; }
    }

    public class ExternalLoginListViewModel
    {
        public string ReturnUrl { get; set; }
    }

  public class SendCodeViewModel
  {
    public string SelectedProvider { get; set; }
    public ICollection<System.Web.Mvc.SelectListItem> Providers { get; set; }
    public string ReturnUrl { get; set; }
    public bool RememberMe { get; set; }
  }

  public class VerifyCodeViewModel
  {
    [Required]
    public string Provider { get; set; }

    [Required]
    [Display(Name = "Code")]
    public string Code { get; set; }
    public string ReturnUrl { get; set; }

    [Display(Name = "Diesen Browser merken?")]
    public bool RememberBrowser { get; set; }

    public bool RememberMe { get; set; }
  }

  public class ForgotViewModel
  {
    [Required]
    [Display(Name = "E-Mail")]
    public string Email { get; set; }
  }

  public class LoginViewModel
  {
    [Required]
    [Display(Name = "E-Mail")]
    [EmailAddress]
    public string Email { get; set; }

    [Required]
    [DataType(DataType.Password)]
    [Display(Name = "Kennwort")]
    public string Password { get; set; }

    [Display(Name = "Speichern?")]
    public bool RememberMe { get; set; }
  }

  public class RegisterViewModel
  {
    public static List<System.Web.Mvc.SelectListItem> ltLand  = new List<System.Web.Mvc.SelectListItem>();
    public static List<System.Web.Mvc.SelectListItem> ltSpKl  = new List<System.Web.Mvc.SelectListItem>();
    public List<System.Web.Mvc.SelectListItem> ltClubs = new List<System.Web.Mvc.SelectListItem>();

    [Required]
    [EmailAddress]
    [Display(Name = "E-Mail")]
    public string Email { get; set; }

#if !DEBUG
    [Required]
#endif
    [DataType(DataType.Text)]
    [Display(Name = "Vorname")]
    public string Vorname { get; set; }

#if !DEBUG
    [Required]
#endif
    [DataType(DataType.Text)]
    [Display(Name = "Nachname")]
    public string Nachname { get; set; }

#if !DEBUG
    [Required]
    [System.Web.Mvc.Remote("CheckExistingTeamName", "Account", HttpMethod = "POST", ErrorMessage = "Vereinsname bereits vorhanden!")]
#endif
    [DataType(DataType.Text)]
    [Display(Name = "Vereinsname")]
    public string Verein { get; set; }

    //[Required]
    [DataType(DataType.Text)]
    [Display(Name = "Land")]
    public int Land { get; set; }

    //[Required]
    [DataType(DataType.Text)]
    [Display(Name = "Liga")]
    public int Liga { get; set; }

    //[Required]
    [DataType(DataType.Text)]
    [Display(Name = "Verein")]
    public int iClubIx { get; set; }

    //[Required]
    public Color cl1 = System.Drawing.Color.White;
    public Color cl2 = System.Drawing.Color.Blue;
    public Color cl3 = System.Drawing.Color.White;
    public Color cl4 = System.Drawing.Color.Red;

#if !DEBUG
    [Required]
    [StringLength(100, ErrorMessage = "\"{0}\" muss mindestens {2} Zeichen lang sein.", MinimumLength = 4)]
    [DataType(DataType.Password)]
#endif
    [Display(Name = "Kennwort")]
    public string Password { get; set; }

    [DataType(DataType.Password)]
    [Display(Name = "Kennwort bestätigen")]
    [Compare("Password", ErrorMessage = "Das Kennwort entspricht nicht dem Bestätigungskennwort.")]
    public string ConfirmPassword { get; set; }

    public RegisterViewModel()
    {
      iClubIx = -1;
    }
  }

  public class ResetPasswordViewModel
  {
    [Required]
    [EmailAddress]
    [Display(Name = "E-Mail")]
    public string Email { get; set; }

    [Required]
    [StringLength(100, ErrorMessage = "\"{0}\" muss mindestens {2} Zeichen lang sein.", MinimumLength = 4)]
    [DataType(DataType.Password)]
    [Display(Name = "Kennwort")]
    public string Password { get; set; }

    [DataType(DataType.Password)]
    [Display(Name = "Kennwort bestätigen")]
    [Compare("Password", ErrorMessage = "Das Kennwort stimmt nicht mit dem Bestätigungskennwort überein.")]
    public string ConfirmPassword { get; set; }

    public string Code { get; set; }
  }

  public class ForgotPasswordViewModel
  {
    [Required]
    [EmailAddress]
    [Display(Name = "E-Mail")]
    public string Email { get; set; }
  }

  public class RemoveUserViewModel
  {
    /*
    [Required]
    [EmailAddress]
    [Display(Name = "E-Mail")]
    public string Email { get; set; }
    */
  }

}
