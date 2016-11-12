using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Reflection;

namespace Reusable
{
    public class Repository<T> : IRepository<T> where T : BaseEntity
    {
        protected readonly DbContext context;
        public int? byUserId { get; set; }

        public string EntityName { get; set; }

        public Repository(DbContext context)
        {
            this.context = context;
            EntityName = typeof(T).Name;
        }

        public virtual void Add(params T[] items)
        {
            DbSet<T> dbSet = context.Set<T>();
            foreach (T item in items)
            {
                dbSet.Attach(item);
            }

            foreach (DbEntityEntry<BaseEntity> entry in context.ChangeTracker.Entries<BaseEntity>())
            {
                context.Entry(entry.Entity).State = EntityState.Unchanged;
            }

            foreach (T item in items)
            {
                context.Entry(item).State = EntityState.Added;
            }

            context.SaveChanges();

            /*DOCUMENT*/
            if (typeof(T).IsSubclassOf(typeof(BaseDocument)))
            {

                foreach (T entity in items)
                {
                    var document = entity as BaseDocument;
                    document.InfoTrack = new Track();
                    //(entity as Trackable).InfoTrack = trackRepository.GetSingle(context, t => t.Entity_ID == entity.ID && t.Entity_Kind == entity.AAA_EntityName);
                    document.InfoTrack.Date_CreatedOn = DateTime.Now;
                    document.InfoTrack.Entity_ID = document.id;
                    document.InfoTrack.Entity_Kind = document.AAA_EntityName;
                    document.InfoTrack.User_CreatedByKey = byUserId ?? 0;

                    context.Entry(document.InfoTrack).State = EntityState.Added;
                    context.SaveChanges();
                }
            }
        }

        public virtual IList<T> GetAll()
        {
            List<T> list;
            IQueryable<T> dbQuery = context.Set<T>();

            list = dbQuery
                .AsNoTracking()
                .ToList();

            /*DOCUMENT*/
            if (typeof(T).IsSubclassOf(typeof(BaseDocument)))
            {
                list = list.Where(d => (d as BaseDocument).sys_active == true).ToList();

                foreach (T item in list)
                {
                    var document = item as BaseDocument;

                    //document.InfoTrack = context.Database.SqlQuery<Track>("select * from Track where Entity_ID = @p0 and Entity_Kind = @p1",
                    //    document.ID, document.AAA_EntityName).FirstOrDefault();
                    document.InfoTrack = context.Set<Track>()
                                        .AsNoTracking()
                                        .FirstOrDefault(t => t.Entity_ID == document.id && t.Entity_Kind == document.AAA_EntityName);

                }
            }

            return list;
        }

        public virtual IList<T> GetList(Func<T, bool> where)
        {
            List<T> list;
            IQueryable<T> dbQuery = context.Set<T>();

            if (where != null)
            {
                list = dbQuery
                .AsNoTracking()
                .Where(where)
                .ToList();
            }
            else
            {
                list = dbQuery
                .AsNoTracking()
                .ToList();
            }

            /*DOCUMENT*/
            if (typeof(T).IsSubclassOf(typeof(BaseDocument)))
            {
                list = list.Where(d => (d as BaseDocument).sys_active == true).ToList();

                foreach (T item in list)
                {
                    var document = item as BaseDocument;

                    document.InfoTrack = context.Set<Track>()
                                        .AsNoTracking()
                                        .FirstOrDefault(t => t.Entity_ID == document.id && t.Entity_Kind == document.AAA_EntityName);

                }
            }
            return list;
        }

        public virtual T GetSingle(Func<T, bool> where)
        {
            T item = null;
            IQueryable<T> dbQuery = context.Set<T>();

            item = dbQuery
                .AsNoTracking()
                .FirstOrDefault(where);


            /*DOCUMENT*/
            if (typeof(T).IsSubclassOf(typeof(BaseDocument)))
            {
                var document = item as BaseDocument;
                if (document.sys_active == true)
                {
                    document.InfoTrack = context.Set<Track>()
                                        .AsNoTracking()
                                        .FirstOrDefault(t => t.Entity_ID == document.id && t.Entity_Kind == document.AAA_EntityName);
                }
                else
                {
                    return null;
                }
            }

            return item;
        }

        public virtual void Delete(int id)
        {
            DbSet<T> tSet = context.Set<T>();
            T entity = tSet.Find(id);

            if (entity != null)
            {
                /*DOCUMENT*/
                if (typeof(T).IsSubclassOf(typeof(BaseDocument)))
                {
                    Deactivate(id);
                }
                else
                {
                    context.Entry(entity).State = EntityState.Deleted;
                }
                context.SaveChanges();
            }
            else
            {
                throw new Exception("Entity not found.");
            }
        }

        public virtual void Update(T item) //Update only parent Entity, not children
        {
            DbSet<T> dbSet = context.Set<T>();

            T originalEntity = dbSet.Find(item.id);
            if (originalEntity == null)
            {
                throw new Exception("Entity of type: [" + item.AAA_EntityName + "] with ID: [" + item.id + "] was not found.");
            }

            context.Entry(originalEntity).CurrentValues.SetValues(item);
            context.Entry(originalEntity).State = EntityState.Modified;

            /*DOCUMENT*/
            if (typeof(T).IsSubclassOf(typeof(BaseDocument)))
            {
                var document = originalEntity as BaseDocument;
                document.InfoTrack = context.Set<Track>()
                                    .AsNoTracking()
                                    .FirstOrDefault(t => t.Entity_ID == document.id && t.Entity_Kind == document.AAA_EntityName);

                if (document.InfoTrack != null)
                {
                    document.InfoTrack.User_LastEditedByKey = byUserId;
                    document.InfoTrack.Date_EditedOn = DateTime.Now;

                    context.Entry(document.InfoTrack).State = EntityState.Modified;
                }
            }

            context.SaveChanges();
        }

        public virtual T GetByID(int id)
        {
            DbSet<T> set = context.Set<T>();
            T item = set.Find(id);
            
            /*DOCUMENT*/
            if (typeof(T).IsSubclassOf(typeof(BaseDocument)))
            {
                var document = item as BaseDocument;
                if (document != null && document.sys_active == true)
                {
                    document.InfoTrack = context.Set<Track>()
                                        .AsNoTracking()
                                        .FirstOrDefault(t => t.Entity_ID == document.id && t.Entity_Kind == document.AAA_EntityName);
                }
                else
                {
                    return null;
                }
            }

            return item;
        }

        public virtual IList<T> GetListByParent<P>(int parentID) where P : class
        {
            List<T> list = new List<T>();

            DbSet<P> setParent = context.Set<P>();

            P parent = setParent.Find(parentID);

            if (parent == null)
            {
                throw new Exception("Parent non-existent.");
            }

            if (parent is BaseDocument)
            {
                if ((parent as BaseDocument).sys_active == false)
                {
                    throw new Exception("Parent non-existent.");
                }
            }

            string tName = typeof(T).Name + "s";
            list = context.Entry(parent).Collection<T>(tName)
                .Query()
                .AsNoTracking()
                .ToList<T>();

            /*DOCUMENT*/
            if (typeof(T).IsSubclassOf(typeof(BaseDocument)))
            {
                list = list.Where(d => (d as BaseDocument).sys_active == true).ToList();

                foreach (T item in list)
                {
                    var document = item as BaseDocument;

                    document.InfoTrack = context.Set<Track>()
                                        .AsNoTracking()
                                        .FirstOrDefault(t => t.Entity_ID == document.id && t.Entity_Kind == document.AAA_EntityName);

                }
            }

            //Removing Recurivity
            string navigationPropertyName = typeof(P).Name;
            foreach (T item in list)
            {
                //PropertyInfo prop = item.GetType().GetProperty(navigationPropertyName, BindingFlags.Public | BindingFlags.Instance);
                //if (null != prop && prop.CanWrite)
                //{
                //    prop.SetValue(item, new List<P>());
                //}

                try
                {
                    //Trying for collection
                    context.Entry(item).Collection<P>(navigationPropertyName + "s").CurrentValue.Clear();
                }
                catch (Exception)
                {
                    //Trying for reference
                    //context.Entry(item).Reference<P>(navigationPropertyName).CurrentValue.
                }
                
            }

            return list;
        }

        public P AddToParent<P>(int parentId, T entity) where P : class
        {
            //string sParentPropID = typeof(P).Name + "Key";
            //P parent = context.Database.SqlQuery<P>("select * from " + typeof(P).Name + " where " + sParentPropID + " = @p0", parentId).FirstOrDefault();

            DbSet<P> parentSet = context.Set<P>();
            P parent = parentSet.Find(parentId);
            if (parent == null)
            {
                throw new Exception("Non-existent Parent Entity.");
            }
            if (parent is BaseDocument)
            {
                if ((parent as BaseDocument).sys_active == false)
                {
                    throw new Exception("Non-existent Parent Entity.");
                }
            }

            //parentSet.Attach(parent);
            //context.Entry(parent).State = EntityState.Unchanged;

            string navigationPropertyName = typeof(T).Name + "s";

            //DbSet<T> entitySet = context.Set<T>();
            //entitySet.Attach(entity);

            //PropertyInfo navigationProperty = parent.GetType().GetProperty(navigationPropertyName, BindingFlags.Public | BindingFlags.Instance);
            //ICollection<T> childrenList = (ICollection<T>)navigationProperty.GetValue(entity);


            //childrenList.Add(entity);


            DbCollectionEntry<P, T> childrenCollection = context.Entry(parent).Collection<T>(navigationPropertyName);
            childrenCollection.Load();

            if (!childrenCollection.CurrentValue.Contains(entity))
            {
                string sPropID = typeof(T).Name + "Key";
                int id = (int)context.Entry(entity).Property(sPropID).CurrentValue;

                childrenCollection.CurrentValue.Add(entity);
                if (id > 0)
                {
                    context.Entry(entity).State = EntityState.Unchanged;

                    /*DOCUMENT*/
                    if (typeof(T).IsSubclassOf(typeof(BaseDocument)))
                    {
                        var document = entity as BaseDocument;
                        document.InfoTrack = context.Set<Track>()
                                            .AsNoTracking()
                                            .FirstOrDefault(t => t.Entity_ID == document.id && t.Entity_Kind == document.AAA_EntityName);

                        if (document.InfoTrack != null)
                        {
                            document.InfoTrack.User_LastEditedByKey = byUserId;
                            document.InfoTrack.Date_EditedOn = DateTime.Now;

                            context.Entry(document.InfoTrack).State = EntityState.Modified;
                        }
                    }
                }
                else
                {
                    context.Entry(entity).State = EntityState.Added;

                    /*DOCUMENT*/
                    if (typeof(T).IsSubclassOf(typeof(BaseDocument)))
                    {
                        var document = entity as BaseDocument;
                        document.InfoTrack = new Track();
                        //(entity as Trackable).InfoTrack = trackRepository.GetSingle(context, t => t.Entity_ID == entity.ID && t.Entity_Kind == entity.AAA_EntityName);
                        document.InfoTrack.Date_CreatedOn = DateTime.Now;
                        document.InfoTrack.Entity_ID = document.id;
                        document.InfoTrack.Entity_Kind = document.AAA_EntityName;
                        document.InfoTrack.User_CreatedByKey = byUserId ?? 0;

                        context.Entry(document.InfoTrack).State = EntityState.Added;
                    }
                }

                context.SaveChanges();
            }

            return parent;
        }

        public void RemoveFromParent<P>(int parentId, T entity) where P : class
        {
            DbSet<P> parentSet = context.Set<P>();
            P parent = parentSet.Find(parentId);
            if (parent == null)
            {
                throw new Exception("Non-existent Parent Entity.");
            }
            if (parent is BaseDocument)
            {
                if ((parent as BaseDocument).sys_active == false)
                {
                    throw new Exception("Non-existent Parent Entity.");
                }
            }

            string navigationPropertyName = typeof(T).Name + "s";
            
            DbCollectionEntry<P, T> childrenCollection = context.Entry(parent).Collection<T>(navigationPropertyName);
            childrenCollection.Load();

            if (childrenCollection.CurrentValue.Contains(entity))
            {
                childrenCollection.CurrentValue.Remove(entity);
                context.SaveChanges();

                /*DOCUMENT*/
                if (typeof(T).IsSubclassOf(typeof(BaseDocument)))
                {
                    var document = entity as BaseDocument;

                    document.InfoTrack = context.Set<Track>()
                                        .AsNoTracking()
                                        .FirstOrDefault(t => t.Entity_ID == document.id && t.Entity_Kind == document.AAA_EntityName);

                    if (document.InfoTrack != null)
                    {
                        document.InfoTrack.User_LastEditedByKey = byUserId;
                        document.InfoTrack.Date_EditedOn = DateTime.Now;

                        context.Entry(document.InfoTrack).State = EntityState.Modified;
                    }
                    
                    context.SaveChanges();
                }
            }
        }

        public virtual T GetSingleByParent<P>(int parentID) where P : class
        {
            T entity = null;

            DbSet<P> setParent = context.Set<P>();

            P parent = setParent.Find(parentID);

            if (parent == null)
            {
                throw new Exception("Parent non-existent.");
            }

            if (parent is BaseDocument)
            {
                if ((parent as BaseDocument).sys_active == false)
                {
                    throw new Exception("Parent non-existent.");
                }
            }

            string tName = typeof(T).Name;
            entity = context.Entry(parent).Reference<T>(tName).Query().FirstOrDefault();

            /*DOCUMENT*/
            if (typeof(T).IsSubclassOf(typeof(BaseDocument)))
            {
                if (entity != null)
                {
                    var document = entity as BaseDocument;
                    document.InfoTrack = context.Set<Track>()
                                        .AsNoTracking()
                                        .FirstOrDefault(t => t.Entity_ID == document.id && t.Entity_Kind == document.AAA_EntityName);
                }
            }

            return entity;
        }

        public void Activate(int id)
        {
            DbSet<T> tSet = context.Set<T>();

            T entity = tSet.Find(id);

            if (entity != null)
            {
                if (entity is BaseDocument)
                {
                    var document = entity as BaseDocument;
                    document.sys_active = true;
                    context.Entry(document).State = EntityState.Modified;

                    document.InfoTrack = context.Set<Track>()
                                        .AsNoTracking()
                                        .FirstOrDefault(t => t.Entity_ID == document.id && t.Entity_Kind == document.AAA_EntityName);

                    if (document.InfoTrack != null)
                    {
                        document.InfoTrack.Date_EditedOn = DateTime.Now;
                        document.InfoTrack.User_LastEditedByKey = byUserId;

                        document.InfoTrack.Date_RemovedOn = null;
                        document.InfoTrack.User_RemovedByKey = null;

                        context.Entry(document.InfoTrack).State = EntityState.Modified;
                    }

                    context.SaveChanges();
                }
                else
                {
                    throw new Exception("Entity is not a document.");
                }
            }
            else
            {
                throw new Exception("Entity not found.");
            }
        }

        public void Deactivate(int id)
        {
            DbSet<T> tSet = context.Set<T>();

            T entity = tSet.Find(id);

            if (entity != null)
            {
                if (entity is BaseDocument)
                {
                    var document = entity as BaseDocument;

                    document.sys_active = false;

                    context.Entry(document).State = EntityState.Modified;

                    document.InfoTrack = context.Set<Track>()
                                        .AsNoTracking()
                                        .FirstOrDefault(t => t.Entity_ID == document.id && t.Entity_Kind == document.AAA_EntityName);

                    if (document.InfoTrack != null)
                    {
                        document.InfoTrack.Date_RemovedOn = DateTime.Now;
                        document.InfoTrack.User_RemovedByKey = byUserId;

                        context.Entry(document.InfoTrack).State = EntityState.Modified;
                    }

                    context.SaveChanges();
                }
                else
                {
                    throw new Exception("Entity is not a document.");
                }
            }
            else
            {
                throw new Exception("Entity not found.");
            }
        }
    }
}
