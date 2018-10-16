using System;
using System.Linq;
using System.Threading.Tasks;
using static DataInterface.Base;

namespace DataInterface
{
    /// <summary>
    ///  Defines basic Read operations on a datasource
    /// </summary>
    public  interface IDataReadOnlyOperator:IDataOperator  
    {
        

        /// <summary>
        /// Gets the records.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="filters">The filters.</param>
        /// <param name="Order">The order.</param>
        /// <param name="PageSize">Size of the page.</param>
        /// <param name="Page">The page.</param>
        /// <returns></returns>
        EntityCollection<T> GetRecords<T>(RequestFilters filters=null,Base.DataOrder Order=null,int PageSize=0,int Page=0) where T : class, IDataEntity;


        /// <summary>
        /// Gets a collection of entitiews by Entity Key value.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="Key">The key.</param>
        /// <param name="ValueOfKey">The value of key.</param>
        /// <returns></returns>
        EntityCollection<T> GetByEntityKey<T>(string Key,string ValueOfKey) where T : class, IDataEntity;

        /// <summary>
        /// Gets a collection of entitiews by Entity Key value.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="Key">The key.</param>
        /// <param name="ValueOfKey">The value of key.</param>
        /// <returns></returns>
        Task<EntityCollection<T>> GetByEntityKey_Async<T>(string Key,string ValueOfKey) where T : class, IDataEntity;



        /// <summary>
        /// Gets the records asynchronous.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="filters">The filters.</param>
        /// <param name="Order">The order.</param>
        /// <param name="PageSize"></param>
        /// <param name="Page"></param>
        /// <returns></returns>
        Task<EntityCollection<T>> GetRecords_Async<T>(RequestFilters filters = null, Base.DataOrder Order = null, int PageSize = 0, int Page = 0) where T : class, IDataEntity;

    }
}
