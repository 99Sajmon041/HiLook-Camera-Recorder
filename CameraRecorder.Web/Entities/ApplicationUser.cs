using Microsoft.AspNetCore.Identity;

namespace CameraRecorder.Web.Entities;

public sealed class ApplicationUser : IdentityUser
{
    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;
}
