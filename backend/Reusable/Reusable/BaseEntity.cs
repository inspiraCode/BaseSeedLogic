using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Reusable
{
    public abstract class BaseEntity : IEntity
    {
        [NotMapped]
        public abstract int id { get; }

        [NotMapped]
        public string AAA_EntityName { get { return GetType().Name.Split('_')[0]; } }

        [NotMapped]
        public EF_EntityState EF_State { get; set; }
        
        public enum EF_EntityState
        {
            Unchanged,
            Added,
            Modified,
            Deleted
        }

        public override bool Equals(object obj)
        {
            //Check whether the compared object is null.
            if (Object.ReferenceEquals(obj, null)) return false;

            //Check whether the compared object is same type.
            if (!this.GetType().Name.Split('_')[0].Equals(obj.GetType().Name.Split('_')[0])) return false;

            //Check whether the compared object references the same data.
            if (Object.ReferenceEquals(this, obj)) return true;

            //Check whether the IEntity' ids are equal.
            return id.Equals(((BaseEntity)obj).id);
        }
        public override int GetHashCode()
        {
            //Get hash code for the id field if it is not null.
            int hashID = id.GetHashCode();

            return hashID;
        }

        public virtual object Clone()
        {
            return this.MemberwiseClone();
        }
    }
}
