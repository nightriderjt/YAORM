using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataInterface.Attribbutes
{
    /// <summary>
    /// Defines that a class property is the KEY 
    /// </summary>
    /// <seealso cref="Attribute" />     
    public class EntityKey : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EntityKey"/> class.
        /// </summary>
        public EntityKey()
        {
        }
    }
}
