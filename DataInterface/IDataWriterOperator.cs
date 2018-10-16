using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DataInterface
{
    /// <summary>
    /// Performs Write operation on the datasource
    /// </summary>
    /// <seealso cref="DataInterface.IDataOperator" />
    public interface IDataWriterOperator: IDataOperator
    {
        /// <summary>
        /// Adds the record to the datasource
        /// </summary>
        /// <param name="Record">The record.</param>
        /// <returns>True or False</returns>
        bool AddRecord(IDataEntity Record);


        /// <summary>
        /// Adds the record asynchronous.
        /// </summary>
        /// <param name="record">The record.</param>
        /// <returns>True or false</returns>
        Task<bool> AddRecord_Async(IDataEntity record);


        /// <summary>
        /// Updates the record asynchronous.
        /// </summary>
        /// <param name="record">The record.</param>
        /// <returns></returns>
        Task<bool> UpdateRecordAsync(IDataEntity record); 

        /// <summary>
        /// Occurs when [record inserted].
        /// </summary>
        event EventHandler<OperatorEventArguments> RecordInserted;

        /// <summary>
        /// Occurs when [record insert failed].
        /// </summary>
        event EventHandler<OperatorEventArguments> RecordInsertFailed;

        /// <summary>
        /// Occurs when [before insert].
        /// </summary>
        event EventHandler<OperatorEventArguments> BeforeInsert;
    }
}
