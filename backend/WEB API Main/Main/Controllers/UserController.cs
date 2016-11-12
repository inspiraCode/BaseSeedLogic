using Reusable;
using System.Web.Http;

namespace ReusableWebAPI.Controllers
{
    [RoutePrefix("api/User")]
    public class UserController : BaseController<User>
    {
        public UserController(IUserLogic logic) : base(logic) { }
    }
}