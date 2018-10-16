using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataInterface
{
    /// <summary>
    /// 
    /// </summary>
    public class OperatorEventArguments:EventArgs 
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OperatorEventArguments"/> class.
        /// </summary>
        public OperatorEventArguments()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OperatorEventArguments"/> class.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <exception cref="System.ArgumentNullException">data</exception>
        public OperatorEventArguments(object data)
        {
            Data = data ?? throw new ArgumentNullException(nameof(data));
        }

        /// <summary>
        /// Gets or sets the data.
        /// </summary>
        /// <value>
        /// The data.
        /// </value>
        public object Data { get; set; }
    }
}
