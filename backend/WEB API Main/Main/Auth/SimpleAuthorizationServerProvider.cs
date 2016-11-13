using System.Threading.Tasks;
using Microsoft.Owin.Security.OAuth;
using System.Security.Claims;
using Reusable;
using BusinessSpecificLogic;

namespace ReusableWebAPI.Auth
{
    internal class SimpleAuthorizationServerProvider : OAuthAuthorizationServerProvider
    {
        private UserLogic userLogic;

        public SimpleAuthorizationServerProvider()
        {
        }
        public override async Task ValidateClientAuthentication(OAuthValidateClientAuthenticationContext context)
        {
            context.Validated();
        }

        public override async Task GrantResourceOwnerCredentials(OAuthGrantResourceOwnerCredentialsContext context)
        {
            context.OwinContext.Response.Headers.Add("Access-Control-Allow-Origin", new[] { "*" });

            //using (AuthRepository _repo = new AuthRepository())
            //{
            //    IdentityUser user = await _repo.FindUser(context.UserName, context.Password);

            //    if (user == null)
            //    {
            //        context.SetError("invalid_grant", "The user name or password is incorrect.");
            //        return;
            //    }
            //}


            using (var mainContext = new MainContext())
            {
                IRepository<User> userRepo = new Repository<User>(mainContext);
                userLogic = new UserLogic(mainContext, userRepo);

                CommonResponse response = userLogic.GetByName(context.UserName);
                if (response.ErrorThrown)
                {
                    context.SetError(response.ResponseDescription);
                    return;
                }

                User theUser = (User)response.Result;

                var identity = new ClaimsIdentity(context.Options.AuthenticationType);
                identity.AddClaim(new Claim("role", theUser.Role));
                identity.AddClaim(new Claim("userID", theUser.id.ToString()));
                identity.AddClaim(new Claim("userName", theUser.UserName.ToString()));

                context.Validated(identity);
            }
        }
    }
}