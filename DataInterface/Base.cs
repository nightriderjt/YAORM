using Newtonsoft.Json;
using System.Collections.Generic;
using System.Configuration;
namespace DataInterface
{
    /// <summary>
    /// 
    /// </summary>
    public class Base
    {
        /// <summary>
        /// 
        /// </summary>
        public class Pager
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="Pager"/> class.
            /// </summary>
            public Pager()
            {
            }

            /// <summary>
            /// Gets or sets the current page.
            /// </summary>
            /// <value>
            /// The current page.
            /// </value>
            public int CurrentPage { get; set; }
            /// <summary>
            /// Gets or sets the total pages.
            /// </summary>
            /// <value>
            /// The total pages.
            /// </value>
            public int TotalPages { get; set; }
            /// <summary>
            /// Gets or sets the page records.
            /// </summary>
            /// <value>
            /// The page records.
            /// </value>
            public int PageRecords { get; set; }
            /// <summary>
            /// Gets or sets the total records.
            /// </summary>
            /// <value>
            /// The total records.
            /// </value>
            public int TotalRecords { get; set; }
        }



        /// <summary>
        /// 
        /// </summary>
        public class RequestFilters
    {
        private List<DataFilter> _Filters = new List<DataFilter>();

        private List<DataFilters> _OrFilters = new List<DataFilters>();
            /// <summary>
            /// Gets or sets the filters.
            /// </summary>
            /// <value>
            /// The filters.
            /// </value>
            [JsonProperty("filters")]
        public List<DataFilter> Filters
        {
            get
            {
                return _Filters;
            }
            set
            {
                _Filters = value;
            }
        }
            /// <summary>
            /// Gets or sets the or filters.
            /// </summary>
            /// <value>
            /// The or filters.
            /// </value>
            [DefaultSettingValue(null/* TODO Change to default(_) if this is not a reference type */)]
        [JsonProperty("OrFilters")]
        public List<DataFilters> OrFilters
        {
            get
            {
                return _OrFilters;
            }
            set
            {
                _OrFilters = value;
            }
        }

            /// <summary>
            /// Initializes a new instance of the <see cref="RequestFilters"/> class.
            /// </summary>
            public RequestFilters()
        {
        }
    }

    public class DataFilter
    {
        public string Field { get; set; }

        public string Value { get; set; }

        public string Comparer { get; set; }

            /// <summary>
            /// Initializes a new instance of the <see cref="DataFilter"/> class.
            /// </summary>
            public DataFilter()
        {
        }
    }

    public class DataFilters
    {
        [JsonProperty("filters")]
        private List<DataFilter> _Filters = new List<DataFilter>();

            /// <summary>
            /// Gets or sets the filters.
            /// </summary>
            /// <value>
            /// The filters.
            /// </value>
            public List<DataFilter> Filters
        {
            get
            {
                return _Filters;
            }
            set
            {
                _Filters = value;
            }
        }

            /// <summary>
            /// Initializes a new instance of the <see cref="DataFilters"/> class.
            /// </summary>
            public DataFilters()
        {
        }
    }



    /// <summary>
    /// 
    /// </summary>
    public class DataOrder
        {
            /// <summary>
            /// Gets or sets the field.
            /// </summary>
            /// <value>
            /// The field.
            /// </value>
            public string Field { get; set; }
            /// <summary>
            /// Gets or sets the order.
            /// </summary>
            /// <value>
            /// The order.
            /// </value>
            public string Order { get; set; }

        }

    }
   
}
