using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataInterface.Attribbutes
{
    /// <summary>
    /// Defines that a property will map to the specified field of the datasource during db operations
    /// </summary>
    /// <seealso cref="System.Attribute" />
    public class Field:Attribute 
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Field"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <exception cref="System.ArgumentNullException">name</exception>
        public Field(string name)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
        }

        protected Field()
        {
        }

        public string Name { get; set; }
    }
}
