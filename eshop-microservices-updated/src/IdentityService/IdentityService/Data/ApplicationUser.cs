using Microsoft.AspNetCore.Identity;

namespace IdentityService.Data;

public class ApplicationUser : IdentityUser
{
    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;
    public string Address { get; set; } = default!;
}
