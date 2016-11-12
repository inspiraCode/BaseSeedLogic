using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using static Reusable.BaseEntity;

namespace Reusable
{
    public abstract class BaseLogic<Entity> : IBaseLogic<Entity> where Entity : BaseEntity, new()
    {
        public int? byUserId { get; set; }

        protected DbContext context;
        protected IRepository<Entity> repository;

        public BaseLogic(DbContext context, IRepository<Entity> repository)//, int? byUserId)
        {
            this.context = context;
            this.repository = repository;
            //this.byUserId = byUserId;
        }

        protected abstract void loadNavigationProperties(DbContext context, params Entity[] entities);

        protected static EntityState GetEntityState(EF_EntityState state)
        {
            switch (state)
            {
                case EF_EntityState.Unchanged:
                    return EntityState.Unchanged;
                case EF_EntityState.Added:
                    return EntityState.Added;
                case EF_EntityState.Modified:
                    return EntityState.Modified;
                case EF_EntityState.Deleted:
                    return EntityState.Deleted;
                default:
                    return EntityState.Detached;
            }
        }

        protected virtual void onSaving(DbContext context, Entity entity, BaseEntity parent = null) { }
        protected virtual void onCreate(Entity entity) { }

        public virtual CommonResponse Add(Entity entity)
        {
            CommonResponse response = new CommonResponse();
            try
            {
                using (var transaction = context.Database.BeginTransaction())
                {
                    try
                    {
                        //var repository = RepositoryFactory.Create<Entity>(context, byUserId);

                        repository.byUserId = byUserId;
                        repository.Add(entity);
                        onSaving(context, entity);

                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        return response.Error(ex.ToString());
                    }
                }
            }
            catch (Exception e)
            {
                return response.Error("ERROR: " + e.ToString());
            }

            return response.Success(entity);
        }

        public virtual CommonResponse GetAll()
        {
            CommonResponse response = new CommonResponse();
            IList<Entity> entities;
            try
            {
                //var repository = RepositoryFactory.Create<Entity>(context, byUserId);

                repository.byUserId = byUserId;
                entities = repository.GetAll();

                //loadNavigationProperties(context, entities);
            }
            catch (Exception e)
            {
                return response.Error("ERROR: " + e.ToString());
            }

            return response.Success(entities);
        }

        public virtual CommonResponse GetByID(int ID)
        {
            CommonResponse response = new CommonResponse();
            List<Entity> entities = new List<Entity>();
            try
            {

                //var repository = RepositoryFactory.Create<Entity>(context, byUserId);

                Entity entity = repository.GetByID(ID);
                if (entity != null)
                {
                    repository.byUserId = byUserId;
                    entities.Add(entity);
                    loadNavigationProperties(context, entities.ToArray());
                    return response.Success(entity);
                }
                else
                {
                    return response.Error("Entity not found.");
                }
            }
            catch (Exception e)
            {
                return response.Error("ERROR: " + e.ToString());
            }
        }

        public virtual CommonResponse Remove(int id)
        {
            CommonResponse response = new CommonResponse();
            try
            {
                using (var transaction = context.Database.BeginTransaction())
                {
                    try
                    {
                        //var repository = RepositoryFactory.Create<Entity>(context, byUserId);
                        repository.byUserId = byUserId;
                        repository.Delete(id);

                        transaction.Commit();
                    }
                    catch (Exception e)
                    {
                        transaction.Rollback();
                        return response.Error("ERROR: " + e.ToString());
                    }
                }

            }
            catch (Exception e)
            {
                return response.Error("ERROR: " + e.ToString());
            }
            return response.Success(id, repository.EntityName + " removed successfully.");
        }

        public virtual CommonResponse Activate(int id)
        {
            CommonResponse response = new CommonResponse();
            try
            {
                using (var transaction = context.Database.BeginTransaction())
                {
                    try
                    {
                        repository.byUserId = byUserId;
                        repository.Activate(id);

                        transaction.Commit();
                    }
                    catch (Exception e)
                    {
                        transaction.Rollback();
                        return response.Error("ERROR: " + e.ToString());
                    }
                }

            }
            catch (Exception e)
            {
                return response.Error("ERROR: " + e.ToString());
            }
            return response.Success(id);
        }

        public virtual CommonResponse Update(Entity entity)
        {
            CommonResponse response = new CommonResponse();
            try
            {
                using (var transaction = context.Database.BeginTransaction())
                {
                    try
                    {
                        //var repository = RepositoryFactory.Create<Entity>(context, byUserId);

                        repository.byUserId = byUserId;
                        repository.Update(entity);
                        onSaving(context, entity);

                        transaction.Commit();
                    }
                    catch (Exception e)
                    {
                        transaction.Rollback();
                        return response.Error("ERROR: " + e.ToString());
                    }
                }

            }
            catch (Exception e)
            {

                return response.Error("ERROR: " + e.ToString());
            }

            return response.Success(entity);
        }

        public virtual CommonResponse AddToParent<ParentType>(int parentID, Entity entity) where ParentType : BaseEntity
        {
            CommonResponse response = new CommonResponse();
            try
            {

                using (var transaction = context.Database.BeginTransaction())
                {
                    try
                    {
                        //var repository = RepositoryFactory.Create<Entity>(context, byUserId);

                        //var parentRepoType = typeof(BaseEntityRepository<>);
                        //Type[] parentRepositoryArgs = { typeof(ParentType) };
                        //var makeme = parentRepoType.MakeGenericType(parentRepositoryArgs);
                        //object parentRepository = Activator.CreateInstance(makeme);

                        //PropertyInfo propContext = parentRepository.GetType().GetProperty("context", BindingFlags.Public | BindingFlags.Instance);
                        //propContext.SetValue(parentRepository, context);

                        //PropertyInfo propByUser = parentRepository.GetType().GetProperty("byUserID", BindingFlags.Public | BindingFlags.Instance);
                        //propByUser.SetValue(parentRepository, byUserID);

                        //MethodInfo method = parentRepository.GetType().GetMethod("GetByID");
                        //BaseEntity parent = (Entity)method.Invoke(parentRepository, new object[] { parentID });
                        //if (parent == null)
                        //{
                        //    return response.Error("Non-existent Parent Entity.");
                        //}

                        repository.byUserId = byUserId;
                        ParentType parent = repository.AddToParent<ParentType>(parentID, entity);
                        onSaving(context, entity, parent);

                        transaction.Commit();
                    }
                    catch (Exception e)
                    {
                        transaction.Rollback();
                        return response.Error("ERROR: " + e.ToString());
                    }
                }

            }
            catch (Exception e)
            {
                return response.Error("ERROR: " + e.ToString());
            }

            return response.Success(entity);
        }

        public virtual CommonResponse GetAllByParent<ParentType>(int parentID) where ParentType : BaseEntity
        {
            CommonResponse response = new CommonResponse();
            IList<Entity> entities;

            try
            {

                //var repository = RepositoryFactory.Create<Entity>(context, byUserId);

                repository.byUserId = byUserId;
                entities = repository.GetListByParent<ParentType>(parentID);
                loadNavigationProperties(context, entities.ToArray());
                //MethodInfo method = repository.GetType().GetMethod("GetListByParent");
                //MethodInfo genericMethod = method.MakeGenericMethod(new Type[] { typeof(ParentType) });
                //entities = (IList<Entity>) genericMethod.Invoke(repository, new object[] { parentID });

            }
            catch (Exception e)
            {
                return response.Error("ERROR: " + e.ToString());
            }

            return response.Success(entities);
        }

        public virtual CommonResponse GetSingleByParent<ParentType>(int parentID) where ParentType : BaseEntity
        {
            CommonResponse response = new CommonResponse();
            Entity entity;

            try
            {

                repository.byUserId = byUserId;
                entity = repository.GetSingleByParent<ParentType>(parentID);
                loadNavigationProperties(context, entity);
                //MethodInfo method = repository.GetType().GetMethod("GetListByParent");
                //MethodInfo genericMethod = method.MakeGenericMethod(new Type[] { typeof(ParentType) });
                //entities = (IList<Entity>) genericMethod.Invoke(repository, new object[] { parentID });

            }
            catch (Exception e)
            {
                return response.Error("ERROR: " + e.ToString());
            }

            return response.Success(entity);
        }

        public virtual CommonResponse GetAvailableFor<ForEntity>(int id) where ForEntity : BaseEntity
        {
            CommonResponse response = new CommonResponse();
            IEnumerable<Entity> availableEntities;
            try
            {
                repository.byUserId = byUserId;

                IRepository<ForEntity> oRepository = new Repository<ForEntity>(context);
                oRepository.byUserId = byUserId;


                ForEntity forEntity = oRepository.GetByID(id);
                if (forEntity == null)
                {
                    throw new Exception("Entity " + forEntity.AAA_EntityName + " not found.");
                }

                IList<Entity> childrenInForEntity = repository.GetListByParent<ForEntity>(id);

                IList<Entity> allEntities = repository.GetAll();

                availableEntities = allEntities.Where(e => !childrenInForEntity.Any(o => o.id == e.id));

                loadNavigationProperties(context, availableEntities.ToArray());
            }
            catch (Exception e)
            {
                return response.Error("ERROR: " + e.ToString());
            }

            return response.Success(availableEntities);
        }

        public virtual CommonResponse RemoveFromParent<Parent>(int parentID, Entity entity) where Parent : BaseEntity
        {
            CommonResponse response = new CommonResponse();
            try
            {
                using (var transaction = context.Database.BeginTransaction())
                {
                    try
                    {
                        repository.byUserId = byUserId;
                        repository.RemoveFromParent<Parent>(parentID, entity);

                        transaction.Commit();
                    }
                    catch (Exception e)
                    {
                        transaction.Rollback();
                        return response.Error("ERROR: " + e.ToString());
                    }
                }
            }
            catch (Exception e)
            {
                return response.Error("ERROR: " + e.ToString());
            }

            return response.Success();
        }

        public CommonResponse CreateInstance()
        {
            CommonResponse response = new CommonResponse();
            Entity entity = new Entity();
            onCreate(entity);
            return response.Success(entity);
        }

        protected virtual ICatalogContainer LoadCatalogs() { return null; }

        public virtual CommonResponse GetCatalogs()
        {
            CommonResponse response = new CommonResponse();
            try
            {
                response.Success(LoadCatalogs());
            }
            catch (Exception e)
            {
                return response.Error("ERROR: " + e.ToString());
            }
            return response.Success();
        }
    }
}
