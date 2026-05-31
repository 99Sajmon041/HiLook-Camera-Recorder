using CameraRecorder.Web.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CameraRecorder.Web.Database;

public sealed class CameraRecorderDbContext(DbContextOptions<CameraRecorderDbContext> options) : IdentityDbContext<ApplicationUser>(options)
{

}
