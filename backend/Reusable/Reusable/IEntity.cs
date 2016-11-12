using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reusable
{
    public interface IEntity : ICloneable
    {
        int id { get; }

        string AAA_EntityName { get; }
    }
}
