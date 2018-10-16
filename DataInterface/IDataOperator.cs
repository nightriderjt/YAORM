namespace DataInterface
{
    /// <summary>
    /// Permorms Actions against a datasource
    /// </summary>
    public interface IDataOperator
    {
        /// <summary>
        ///Gets or sets the context source.It can be a connection string, a filepath,a url
        /// </summary>
        /// <value>
        /// The source.
        /// </value>
        string Source { get; set; }

    }
}
