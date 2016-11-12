using Microsoft.AspNet.Identity.EntityFramework;

namespace ReusableWebAPI.Auth
{
    public class AuthContext : IdentityDbContext<IdentityUser>
    {
        public AuthContext() : base("AuthContext") { }
    }
}