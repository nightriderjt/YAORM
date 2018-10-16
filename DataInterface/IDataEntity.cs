using System;

namespace DataInterface
{
    /// <summary>
    /// Defines an entity , that's it a RECORD 
    /// </summary>
   public interface IDataEntity
    {

    }

   


    /// <summary>
    /// Defines events for an Entity
    /// </summary>
    public interface IDataEntityWithEvents
    {
           
        /// <summary>
        /// Occurs when [before insert].
        /// </summary>
        event EventHandler  BeforeInsert;
        /// <summary>
        /// Occurs when [record added].
        /// </summary>
        event EventHandler InsertRecord;

        /// <summary>
        /// Occurs when [record add failed].
        /// </summary>
        event EventHandler InsertRecordFailed;

        /// <summary>
        /// Occurs when [update record].
        /// </summary>
        event EventHandler UpdateRecord;
        /// <summary>
        /// Occurs when [updaterecord failed].
        /// </summary>
        event EventHandler UpdaterecordFailed;
    }
}
