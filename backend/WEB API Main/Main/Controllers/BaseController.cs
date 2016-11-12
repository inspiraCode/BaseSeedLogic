using Newtonsoft.Json;
using Reusable;
using ReusableWebAPI.Auth;
using System;
using System.Reflection;
using System.Security.Claims;
using System.Web.Http;
using System.Web.Http.Cors;
using System.Web.Script.Serialization;

namespace ReusableWebAPI.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public abstract class BaseController<Entity> : ApiController where Entity : BaseEntity
    {
        protected IBaseLogic<Entity> _logic;

        public BaseController(IBaseLogic<Entity> logic)
        {
            _logic = logic;

            LoggedUser loggedUser = new LoggedUser((ClaimsIdentity)User.Identity);
            _logic.byUserId = loggedUser.UserID;
        }

        // GET: api/Base
        [HttpGet Route("")]
        virtual public CommonResponse Get()
        {
            return _logic.GetAll();
        }

        // GET: api/Base/5
        [HttpGet Route("")]
        virtual public CommonResponse Get(int id)
        {
            return _logic.GetByID(id);
        }

        // GET: api/Base
        [HttpGet Route("getCatalogs")]
        virtual public CommonResponse getCatalogs()
        {
            return _logic.GetCatalogs();
        }

        // POST: api/Base
        [HttpPost Route("")]
        virtual public CommonResponse Post([FromBody]string value)
        {
            CommonResponse response = new CommonResponse();
            Entity entity;

            try
            {
                entity = JsonConvert.DeserializeObject<Entity>(value);

                return _logic.Add(entity);
            }
            catch (Exception e)
            {
                return response.Error("ERROR: " + e.ToString());
            }
        }

        // POST: api/Base
        [HttpPost Route("AddToParent/{type}/{parentId}")]
        virtual public CommonResponse AddToParent(string type, int parentId, [FromBody]string value)
        {
            CommonResponse response = new CommonResponse();

            Entity entity;
            try
            {
                Type parentType = Type.GetType("BusinessSpecificLogic.EF." + type + ", BusinessSpecificLogic", true);
                entity = JsonConvert.DeserializeObject<Entity>(value);

                MethodInfo method = _logic.GetType().GetMethod("AddToParent");
                MethodInfo generic = method.MakeGenericMethod(parentType);
                response = (CommonResponse)generic.Invoke(_logic, new object[] { parentId, entity });
                return response;
            }
            catch (Exception e)
            {
                return response.Error("ERROR: " + e.ToString());
            }
        }

        [HttpPost Route("Create")]
        virtual public CommonResponse Create()
        {
            return _logic.CreateInstance();
        }

        // PUT: api/Base/5
        virtual public CommonResponse Put(int id, [FromBody]string value)
        {
            CommonResponse response = new CommonResponse();
            Entity entity;

            try
            {
                JavaScriptSerializer jsonSerializer = new JavaScriptSerializer();
                entity = jsonSerializer.Deserialize<Entity>(value);
                //entity = JsonConvert.DeserializeObject<Entity>(value);

                return _logic.Update(entity);
            }
            catch (Exception e)
            {
                return response.Error("ERROR: " + e.ToString());
            }
        }

        // DELETE: api/Base/5
        virtual public CommonResponse Delete(int id)
        {
            return _logic.Remove(id);
        }

        [HttpGet Route("GetAvailableForEntity/{sEntityType}/{id}")]
        virtual public CommonResponse GetAvailableForEntity(string sEntityType, int id)
        {
            CommonResponse response = new CommonResponse();

            try
            {
                Type forEntityType = Type.GetType("BusinessSpecificLogic.EF." + sEntityType + ", BusinessSpecificLogic", true);

                MethodInfo method = _logic.GetType().GetMethod("GetAvailableFor");
                MethodInfo generic = method.MakeGenericMethod(forEntityType);
                response = (CommonResponse)generic.Invoke(_logic, new object[] { id });
                return response;
            }
            catch (Exception e)
            {
                return response.Error("ERROR: " + e.ToString());
            }
        }

        [HttpPost Route("RemoveFromParent/{type}/{parentId}")]
        virtual public CommonResponse RemoveFromParent(string type, int parentId, [FromBody]string value)
        {
            CommonResponse response = new CommonResponse();

            Entity entity;
            try
            {
                Type parentType = Type.GetType("BusinessSpecificLogic.EF." + type + ", BusinessSpecificLogic", true);
                entity = JsonConvert.DeserializeObject<Entity>(value);

                MethodInfo method = _logic.GetType().GetMethod("RemoveFromParent");
                MethodInfo generic = method.MakeGenericMethod(parentType);
                response = (CommonResponse)generic.Invoke(_logic, new object[] { parentId, entity });
                return response;
            }
            catch (Exception e)
            {
                return response.Error("ERROR: " + e.Message, e);
            }
        }


        // POST: api/Base
        [HttpGet Route("GetSingleByParent/{parentType}/{parentId}")]
        virtual public CommonResponse GetSingleByParent(string parentType, int parentId)
        {
            CommonResponse response = new CommonResponse();

            try
            {
                Type parentClassName = Type.GetType("BusinessSpecificLogic.EF." + parentType + ", BusinessSpecificLogic", true);
                MethodInfo method = _logic.GetType().GetMethod("GetSingleByParent");
                MethodInfo generic = method.MakeGenericMethod(parentClassName);
                response = (CommonResponse)generic.Invoke(_logic, new object[] { parentId });
                return response;
            }
            catch (Exception e)
            {
                return response.Error("ERROR: " + e.ToString());
            }
        }
    }
}
