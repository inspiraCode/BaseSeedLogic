using System;
using System.Collections.Generic;
using System.Data.Entity;

namespace Reusable
{
    public interface IUserLogic : IBaseLogic<User>
    {
    }

    public class UserLogic : BaseLogic<User>, IUserLogic
    {
        public UserLogic(DbContext context, IRepository<User> repository) : base(context, repository)
        {
        }

        protected override void loadNavigationProperties(DbContext context, params User[] entities)
        {
            //Empty
        }



        //Specific for UserLogic
        public CommonResponse GetByName(string sName)
        {
            CommonResponse response = new CommonResponse();
            List<User> entities = new List<User>();
            try
            {
                User entity = repository.GetSingle(e => e.UserName == sName);
                if (entity != null)
                {
                    entities.Add(entity);
                    loadNavigationProperties(context, entities.ToArray());
                }
                return response.Success(entity);

            }
            catch (Exception e)
            {
                return response.Error("ERROR: " + e.ToString());
            }
        }
    }
}
