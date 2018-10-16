using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataInterface.Attribbutes
{
    /// <summary>
    /// Specify that a property will not maped to a datasource field in an update,insert operation
    /// </summary>
    /// <seealso cref="System.Attribute" />    
    public class NotInsert : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NotInsert"/> class.
        /// This attribute means that a proiperty will not map to a datasource for an update or insert operation
        /// If you use it as a class attribute will mean that an Entity cannot be insert or update to a datasource
        /// </summary>
        public NotInsert()
        {
        }
    }
}
