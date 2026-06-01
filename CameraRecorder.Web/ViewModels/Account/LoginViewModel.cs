using System.ComponentModel.DataAnnotations;

namespace CameraRecorder.Web.ViewModels.Account;

public sealed class LoginViewModel
{
    [Display(Name = "E-mail")]
    [Required(ErrorMessage = "E-mail je povinný.")]
    [EmailAddress(ErrorMessage = "Zadejte e-mail ve správném formátu.")]
    public string Email { get; set; } = default!;

    [Display(Name = "Heslo")]
    [Required(ErrorMessage = "Heslo je povinné.")]
    public string Password { get; set; } = default!;
}