using DataInterface;
using Encryptor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MySqlDataStore
{
    class DataOperator : IDataOperator, IDataWriterOperator, IDataReadOnlyOperator
    {
        public string Source { get; set; }

        private Encryptor.IAesEncryptor _Encryptor { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DataOperator"/> class.
        /// 
        /// </summary>
        public DataOperator()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DataOperator"/> class.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <exception cref="ArgumentNullException">source</exception>
        public DataOperator(string source)
        {
            Source = source ?? throw new ArgumentNullException(nameof(source));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DataOperator"/> class.
        /// </summary>
        /// <param name="Encryptor">The encryptor.</param>
        /// <param name="source">The source.</param>
        /// <exception cref="ArgumentNullException">source</exception>
        public DataOperator(IAesEncryptor Encryptor, string source)
        {
            _Encryptor = Encryptor ?? null;
            Source = source ?? throw new ArgumentNullException(nameof(source));
        }


      public  event EventHandler<OperatorEventArguments> RecordInserted;


        public event EventHandler<OperatorEventArguments> RecordInsertFailed;


        public event EventHandler<OperatorEventArguments> BeforeInsert;


        /// <summary>
        /// Raises the <see cref="E:RecordInsertFailed" /> event.
        /// </summary>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected virtual void OnRecordInsertFailed(OperatorEventArguments e)
        {
            RecordInsertFailed?.Invoke(this, e);
        }

        private static void CreateInsertQuery(IDataEntity Record, StringBuilder Query, List<PropertyInfo> properties, Dictionary<string, string> FieldsNames)
        {

            List<string> FieldsHolders = new List<string>();
            foreach (PropertyInfo property in properties)
            {
                //first we dfo not want to add in the insert statement the Key
                var KeyName = (from attr in property.GetCustomAttributes(true) where attr.GetType() == typeof(DataInterface.Attribbutes.EntityKey) select attr).FirstOrDefault();
                var NotInsert = (from attr in property.GetCustomAttributes(true) where attr.GetType() == typeof(DataInterface.Attribbutes.NotInsert) select attr).FirstOrDefault();
                if (NotInsert == null)
                {
                    if (KeyName == null)
                    {

                        //check if the property has mapped field attribute
                        //if the attribute is missing we use the property name as field
                        DataInterface.Attribbutes.Field FieldAttribute = (DataInterface.Attribbutes.Field)(from attr in property.GetCustomAttributes(true) where attr.GetType() == typeof(DataInterface.Attribbutes.Field) select attr).FirstOrDefault();
                        if (FieldAttribute == null)
                        {
                            FieldAttribute = new DataInterface.Attribbutes.Field(property.Name);
                        }
                        FieldsNames.Add(property.Name, FieldAttribute.Name);
                        FieldsHolders.Add($"@{FieldAttribute.Name}");
                    }
                }

            }
            Query.Append($"{String.Join(",", FieldsNames.Values.ToArray()) }) values ({String.Join(",", FieldsHolders.ToArray())});Select Scope_Identity()");
        }



        /// <summary>
        /// Sets the inset parameters.
        /// </summary>
        /// <param name="Record">The record.</param>
        /// <param name="properties">The properties.</param>
        /// <param name="FieldsNames">The fields names.</param>
        /// <param name="com">The COM.</param>
        private static void SetInsetParameters(IDataEntity Record, List<PropertyInfo> properties, Dictionary<string, string> FieldsNames, MySql.Data.MySqlClient.MySqlCommand com)
        {
            foreach (PropertyInfo property in properties)
            {
                if (FieldsNames.Keys.Contains(property.Name) == true)
                {
                    if (property.GetValue(Record) == null)
                    {
                        com.Parameters.AddWithValue($"@{FieldsNames[property.Name]}", DBNull.Value);
                    }
                    else
                    {
                        com.Parameters.AddWithValue($"@{FieldsNames[property.Name]}", property.GetValue(Record));
                    }
                }
            }
        }
        /// <summary>
        /// Raises the <see cref="E:BeforeInsert" /> event.
        /// </summary>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected virtual void OnBeforeInsert(OperatorEventArguments e)
        {
            BeforeInsert?.Invoke(this, e);
        }

        /// <summary>
        /// Updates the key propery.
        /// </summary>
        /// <param name="Record">The record.</param>
        /// <param name="properties">The properties.</param>
        /// <param name="result">The result.</param>
        private static void UpdateKeyPropery(IDataEntity Record, List<PropertyInfo> properties, int result)
        {
            foreach (PropertyInfo property in properties)
            {
                DataInterface.Attribbutes.EntityKey KeyProperty = (DataInterface.Attribbutes.EntityKey)(from attr in property.GetCustomAttributes(true) where attr.GetType() == typeof(DataInterface.Attribbutes.EntityKey) select attr).FirstOrDefault();
                if (KeyProperty != null)
                {
                    property.SetValue(Record, result);
                }
            }
        }
        bool IDataWriterOperator.AddRecord(IDataEntity Record)
        {
            OnBeforeInsert(new OperatorEventArguments() { Data = Record });

            //Check if the class has NotInsert attribute
            DataInterface.Attribbutes.NotInsert NotInsertAttribute = (DataInterface.Attribbutes.NotInsert)(from attr in Record.GetType().GetCustomAttributes(true) where attr.GetType() == typeof(DataInterface.Attribbutes.NotInsert) select attr).FirstOrDefault();

            if (NotInsertAttribute != null)
            {
                throw new Exception("A Class marked with attribute NotInsert cannot perform write opperations");
            }


            //first find the table attribute to obtain the maped db table
            DataInterface.Attribbutes.Table TableAttribute = (DataInterface.Attribbutes.Table)(from attr in Record.GetType().GetCustomAttributes(true) where attr.GetType() == typeof(DataInterface.Attribbutes.Table) select attr).FirstOrDefault();


            using (MySql .Data .MySqlClient .MySqlConnection  connection = new MySql.Data.MySqlClient.MySqlConnection(Source))
            {
                connection.Open();
                try
                {
                    StringBuilder Query = new StringBuilder();
                    Query.Append($"Insert INTO {TableAttribute.Name} (");

                    //get the properties of the entity
                    List<PropertyInfo> properties = Record.GetType().GetProperties().ToList();
                    Dictionary<string, string> FieldsNames = new Dictionary<string, string>();
                    List<string> FieldsHolders = new List<string>();
                    // create the insert query
                    CreateInsertQuery(Record, Query, properties, FieldsNames);
                    var com = new MySql .Data .MySqlClient .MySqlCommand (Query.ToString(), connection);
                    //Create the sql parameters
                    if (_Encryptor != null)
                    {
                        if (Record is IEncryptableClass)
                        {
                            _Encryptor.EncryptProperties((IEncryptableClass)Record);
                        }
                    }
                    SetInsetParameters(Record, properties, FieldsNames, com);

                    int result = Convert.ToInt32(com.ExecuteScalar());
                    //find the Key property of the entity
                    UpdateKeyPropery(Record, properties, result);
                    connection.Close();
                    if (_Encryptor != null)
                    {
                        if (Record is IEncryptableClass)
                        {
                            _Encryptor.Decryptproperties((IEncryptableClass)Record);
                        }
                    }
                }
                catch (Exception ex)
                {
                    if (connection.State == System.Data.ConnectionState.Open)
                    {
                        connection.Close();
                    }
                    OnRecordInsertFailed(new OperatorEventArguments() { Data = Record });
                    return false;
                }
            }
            OnRecordInserted(new OperatorEventArguments() { Data = Record });
            return true;
        }

        /// <summary>
        /// Raises the <see cref="E:RecordInserted" /> event.
        /// </summary>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected virtual void OnRecordInserted(OperatorEventArguments e)
        {
            RecordInserted?.Invoke(this, e);
        }
        public async Task<bool> AddRecord_Async(IDataEntity Record)
        {
            OnBeforeInsert(new OperatorEventArguments() { Data = Record });


            DataInterface.Attribbutes.NotInsert NotInsertAttribute = (DataInterface.Attribbutes.NotInsert)(from attr in Record.GetType().GetCustomAttributes(true) where attr.GetType() == typeof(DataInterface.Attribbutes.NotInsert) select attr).FirstOrDefault();

            if (NotInsertAttribute != null)
            {
                throw new Exception("A Class marked with attribute NotInsert cannot perform write opperations");
            }


            //first find the table attribute to obtain the maped db table
            DataInterface.Attribbutes.Table TableAttribute = (DataInterface.Attribbutes.Table)(from attr in Record.GetType().GetCustomAttributes(true) where attr.GetType() == typeof(DataInterface.Attribbutes.Table) select attr).FirstOrDefault();

            using (MySql.Data.MySqlClient.MySqlConnection connection = new MySql.Data.MySqlClient.MySqlConnection(this.Source))
            {
                connection.Open();
                try
                {
                    StringBuilder Query = new StringBuilder();
                    if (TableAttribute != null)
                    {
                        Query.Append($"Insert INTO {TableAttribute.Name} (");
                    }
                    else
                    {
                        Query.Append($"Insert INTO {Record.GetType().Name} (");
                    }


                    //get the properties of the entity
                    List<PropertyInfo> properties = Record.GetType().GetProperties().ToList();
                    Dictionary<string, string> FieldsNames = new Dictionary<string, string>();
                    List<string> FieldsHolders = new List<string>();
                    // create the insert query
                    CreateInsertQuery(Record, Query, properties, FieldsNames);
                    var com = new MySql .Data .MySqlClient .MySqlCommand (Query.ToString(), connection);
                    //Create the sql parameters
                    if (_Encryptor != null)
                    {
                        if (Record is IEncryptableClass)
                        {
                            _Encryptor.EncryptProperties((IEncryptableClass)Record);
                        }
                    }

                    SetInsetParameters(Record, properties, FieldsNames, com);

                    int result = Convert.ToInt32(await com.ExecuteScalarAsync());
                    //find the Key property of the entity
                    UpdateKeyPropery(Record, properties, result);

                    if (_Encryptor != null)
                    {
                        if (Record is IEncryptableClass)
                        {
                            _Encryptor.Decryptproperties((IEncryptableClass)Record);
                        }
                    }
                    connection.Close();
                }
                catch (Exception ex)
                {
                    if (connection.State == System.Data.ConnectionState.Open)
                    {
                        connection.Close();
                    }
                    OnRecordInsertFailed(new OperatorEventArguments() { Data = Record });
                    return false;
                }
            }
            OnRecordInserted(new OperatorEventArguments() { Data = Record });
            return true;
        }

        /// <summary>
        /// Gets a collection of entitiews by Entity Key value.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="Key">The key.</param>
        /// <param name="ValueOfKey">The value of key.</param>
        /// <returns></returns>
        public EntityCollection<T> GetByEntityKey<T>(string Key, string ValueOfKey) where T : class, IDataEntity
        {
            var Filters = new Base.RequestFilters
            {
                Filters = new List<Base.DataFilter> {
                    new Base.DataFilter {
                         Comparer ="=",
                          Field =Key,
                           Value =ValueOfKey
                    }
                }
            };

            return GetRecords<T>(Filters);
        }

        public async   Task<EntityCollection<T>> GetByEntityKey_Async<T>(string Key, string ValueOfKey) where T :class,IDataEntity 
        {
            var Filters = new Base.RequestFilters
            {
                Filters = new List<Base.DataFilter> {
                    new Base.DataFilter {
                         Comparer ="=",
                          Field =Key,
                           Value =ValueOfKey
                    }
                }
            };

            return await GetRecords_Async<T>(Filters);
        }


        /// <summary>
        /// Gets the records.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="filters">The filters.</param>
        /// <param name="Order">The order.</param>
        /// <param name="PageSize">Size of the page.</param>
        /// <param name="Page">The page.</param>
        /// <returns></returns>
        public EntityCollection<T> GetRecords<T>(Base.RequestFilters filters = null, Base.DataOrder Order = null, int PageSize = 0, int Page = 0) where T : class, IDataEntity
        {
            var currentType = typeof(T);
            var records = new EntityCollection<T>();

            //first set the source where we get the data
            //if there is not any Table attribute we set the source as the type name
            DataInterface.Attribbutes.Table TableAttribute = (DataInterface.Attribbutes.Table)(from attr in currentType.GetCustomAttributes(true) where attr.GetType() == typeof(DataInterface.Attribbutes.Table) select attr).FirstOrDefault();

            using (MySql .Data .MySqlClient .MySqlConnection  connection = new MySql.Data.MySqlClient.MySqlConnection(Source))
            {
                connection.Open();
                try
                {
                    StringBuilder Query = new StringBuilder();
                    if (TableAttribute == null)
                    {
                        Query.Append($"Select * from {currentType.Name } where 1=1 ");
                    }
                    else
                    {
                        Query.Append($"Select * from {TableAttribute.Name} where 1=1 ");
                    }
                    MySql .Data .MySqlClient .MySqlCommand  com = new MySql.Data.MySqlClient.MySqlCommand();

                    BuildWhereStaTement(filters, Query, com);

                    if (Order != null)
                    {
                        Query.Append($" order by {Order.Field} {Order.Order}");
                    }
                    if (Page != 0)
                    {
                        Query.Append($" OFFSET (({Page} - 1) * {PageSize }) ROWS FETCH NEXT {PageSize} ROWS ONLY; ");

                        BuildCountPagingQuery(filters, PageSize, currentType, records, TableAttribute, connection);
                    }

                    com.Connection = connection;
                    com.CommandText = Query.ToString();

                    var result = com.ExecuteReader(System.Data.CommandBehavior.CloseConnection);
                    if (result.HasRows)
                    {
                        FillresultsNew(currentType, records, result);
                    }
                    result.Close();
                }
                catch (Exception ex)
                {
                    if (connection.State == System.Data.ConnectionState.Open)
                    {
                        connection.Close();
                    }
                    throw;
                }
            }
            if (Page != 0)
            {
                records.Pager.CurrentPage = Page;
            }
            else
            {
                records.Pager.TotalRecords = records.Count;

            }
            records.Pager.PageRecords = records.Count;
            return records;
        }

        public async   Task<EntityCollection<T>> GetRecords_Async<T>(Base.RequestFilters filters=null, Base.DataOrder Order=null, int PageSize=0, int Page=0) where T : class, IDataEntity
        {
            var currentType = typeof(T);
            var records = new EntityCollection<T>();

            //first set the source where we get the data
            //if there is not any Table attribute we set the source as the type name
            DataInterface.Attribbutes.Table TableAttribute = (DataInterface.Attribbutes.Table)(from attr in currentType.GetCustomAttributes(true) where attr.GetType() == typeof(DataInterface.Attribbutes.Table) select attr).FirstOrDefault();

            using (MySql .Data .MySqlClient .MySqlConnection  connection = new MySql.Data.MySqlClient.MySqlConnection(Source))
            {
                await connection.OpenAsync();
                try
                {
                    StringBuilder Query = new StringBuilder();
                    if (TableAttribute == null)
                    {
                        Query.Append($"Select * from {currentType.Name } where 1=1 ");
                    }
                    else
                    {
                        Query.Append($"Select * from {TableAttribute.Name} where 1=1 ");
                    }
               MySql.Data.MySqlClient .MySqlCommand       com = new MySql.Data.MySqlClient.MySqlCommand();

                    BuildWhereStaTement(filters, Query, com);

                    if (Order != null)
                    {
                        Query.Append($" order by {Order.Field} {Order.Order}");
                    }




                    if (Page != 0)
                    {
                        Query.Append($" OFFSET (({Page} - 1) * {PageSize }) ROWS FETCH NEXT {PageSize} ROWS ONLY; ");

                        BuildCountPagingQuery(filters, PageSize, currentType, records, TableAttribute, connection);
                    }
                    com.Connection = connection;
                    com.CommandText = Query.ToString();
                    var result = await com.ExecuteReaderAsync(System.Data.CommandBehavior.CloseConnection);
                    if (result.HasRows)
                    {
                        FillresultsNew(currentType, records, result);
                    }
                    result.Close();
                }
                catch (Exception ex)
                {
                    if (connection.State == System.Data.ConnectionState.Open)
                    {
                        connection.Close();
                    }
                    throw;
                }
            }

            if (Page != 0)
            {
                records.Pager.CurrentPage = Page;
            }
            else
            {
                records.Pager.TotalRecords = records.Count;

            }
            records.Pager.PageRecords = records.Count;
            return records;
        }
        private static void CreateUpdateQuery(IDataEntity Record, StringBuilder Query, List<PropertyInfo> properties, Dictionary<string, string> FieldsNames)
        {

            List<string> FieldsHolders = new List<string>();
            List<string> QueryParts = new List<string>();
            foreach (PropertyInfo property in properties)
            {
                //first we dfo not want to add in the insert statement the Key
                var KeyName = (from attr in property.GetCustomAttributes(true) where attr.GetType() == typeof(DataInterface.Attribbutes.EntityKey) select attr).FirstOrDefault();
                var NotInsert = (from attr in property.GetCustomAttributes(true) where attr.GetType() == typeof(DataInterface.Attribbutes.NotInsert) select attr).FirstOrDefault();
                if (NotInsert == null)
                {
                    if (KeyName == null)
                    {

                        //check if the property has mapped field attribute
                        //if the attribute is missing we use the property name as field
                        DataInterface.Attribbutes.Field FieldAttribute = (DataInterface.Attribbutes.Field)(from attr in property.GetCustomAttributes(true) where attr.GetType() == typeof(DataInterface.Attribbutes.Field) select attr).FirstOrDefault();
                        if (FieldAttribute == null)
                        {
                            FieldAttribute = new DataInterface.Attribbutes.Field(property.Name);
                        }
                        FieldsNames.Add(property.Name, FieldAttribute.Name);
                        FieldsHolders.Add($"@{FieldAttribute.Name}");
                        QueryParts.Add($"{FieldAttribute.Name}=@{FieldAttribute.Name}");

                    }
                }
            }
            Query.Append(String.Join(",", QueryParts.ToArray()));

            foreach (PropertyInfo property in properties)
            {
                DataInterface.Attribbutes.EntityKey KeyProperty = (DataInterface.Attribbutes.EntityKey)(from attr in property.GetCustomAttributes(true) where attr.GetType() == typeof(DataInterface.Attribbutes.EntityKey) select attr).FirstOrDefault();
                if (KeyProperty != null)
                {
                    //find if the property has a field attribute
                    //if not we map to the property;'s name
                    DataInterface.Attribbutes.Field FieldProperty = (DataInterface.Attribbutes.Field)(from attr in property.GetCustomAttributes(true) where attr.GetType() == typeof(DataInterface.Attribbutes.Field) select attr).FirstOrDefault();
                    if (FieldProperty != null)
                    {
                        Query.Append($" where   {FieldProperty.Name }=@{FieldProperty.Name}");
                    }
                    else
                    {
                        Query.Append($" where   {property.Name}=@{property.Name}");
                    }
                }
            }



        }
        /// <summary>
        /// Updates the record asynchronous.
        ///   If there is a property marked as EntityKey the update will consider that field as Unique Key    
        ///  If there is not any property marked as EntityKey the function will throw an exception
        /// </summary>
        /// <param name="record">The record.</param>
        /// <returns></returns>
        /// <exception cref="System.Exception">
        /// There is not any property marked as EntityKey
        /// or
        /// A Class marked with attribute NotInsert cannot perform write opperations
        /// </exception>
        public async Task<bool> UpdateRecordAsync(IDataEntity record)
        {
            OnBeforeInsert(null);
            //find if there is an EntityKey property. If there is not we abort the operation
            DataInterface.Attribbutes.EntityKey EntityKey = (DataInterface.Attribbutes.EntityKey)(from attr in record.GetType().GetCustomAttributes(true) where attr.GetType() == typeof(DataInterface.Attribbutes.EntityKey) select attr).FirstOrDefault();
            if (EntityKey != null)
            {
                throw new Exception("There is not any property marked as EntityKey");
            }


            DataInterface.Attribbutes.NotInsert NotInsertAttribute = (DataInterface.Attribbutes.NotInsert)(from attr in record.GetType().GetCustomAttributes(true) where attr.GetType() == typeof(DataInterface.Attribbutes.NotInsert) select attr).FirstOrDefault();

            if (NotInsertAttribute != null)
            {
                throw new Exception("A Class marked with attribute NotInsert cannot perform write opperations");
            }


            //first find the table attribute to obtain the maped db table
            DataInterface.Attribbutes.Table TableAttribute = (DataInterface.Attribbutes.Table)(from attr in record.GetType().GetCustomAttributes(true) where attr.GetType() == typeof(DataInterface.Attribbutes.Table) select attr).FirstOrDefault();

            using (MySql .Data .MySqlClient .MySqlConnection  connection = new MySql.Data.MySqlClient.MySqlConnection(this.Source))
            {
                connection.Open();
                try
                {
                    StringBuilder Query = new StringBuilder();
                    if (TableAttribute != null)
                    {
                        Query.Append($"Update {TableAttribute.Name} set ");
                    }
                    else
                    {
                        Query.Append($"Update  {record.GetType().Name} set");
                    }

                    //get the properties of the entity
                    List<PropertyInfo> properties = record.GetType().GetProperties().ToList();
                    Dictionary<string, string> FieldsNames = new Dictionary<string, string>();
                    // create the insert query                  

                    CreateUpdateQuery(record, Query, properties, FieldsNames);

                    var com = new MySql .Data .MySqlClient .MySqlCommand (Query.ToString(), connection);
                    //Create the sql parameters
                    if (_Encryptor != null)
                    {
                        if (record is IEncryptableClass)
                        {
                            _Encryptor.EncryptProperties((IEncryptableClass)record);
                        }
                    }
                    SetInsetParameters(record, properties, FieldsNames, com);

                    foreach (PropertyInfo property in properties)
                    {
                        DataInterface.Attribbutes.EntityKey KeyProperty = (DataInterface.Attribbutes.EntityKey)(from attr in property.GetCustomAttributes(true) where attr.GetType() == typeof(DataInterface.Attribbutes.EntityKey) select attr).FirstOrDefault();
                        if (KeyProperty != null)
                        {
                            DataInterface.Attribbutes.Field FieldProperty = (DataInterface.Attribbutes.Field)(from attr in property.GetCustomAttributes(true) where attr.GetType() == typeof(DataInterface.Attribbutes.Field) select attr).FirstOrDefault();
                            if (FieldProperty != null)
                            {
                                com.Parameters.AddWithValue($"@{FieldProperty.Name}", property.GetValue(record));
                            }
                            else
                            {
                                com.Parameters.AddWithValue($"@{property.Name}", property.GetValue(record));
                            }
                        }
                    }

                    await com.ExecuteNonQueryAsync();
                    //find the Key property of the entity                 

                    if (_Encryptor != null)
                    {
                        if (record is IEncryptableClass)
                        {
                            _Encryptor.Decryptproperties((IEncryptableClass)record);
                        }
                    }
                    connection.Close();
                }
                catch (Exception ex)
                {
                    if (connection.State == System.Data.ConnectionState.Open)
                    {
                        connection.Close();
                    }
                    throw;
                }
            }
            return true;
        }


        private static void BuildCountPagingQuery<T>(Base.RequestFilters filters, int PageSize, Type currentType, EntityCollection<T> records, DataInterface.Attribbutes.Table TableAttribute, MySql .Data .MySqlClient .MySqlConnection  connection) where T : class, IDataEntity
        {
            //count the records also
            var CountQuery = new StringBuilder();
            if (TableAttribute == null)
            {
                CountQuery.Append($"Select count(*) from {currentType.Name } where 1=1 ");
            }
            else
            {
                CountQuery.Append($"Select count(*) from {TableAttribute.Name} where 1=1 ");
            }
           var  countcom = new MySql.Data.MySqlClient.MySqlCommand();
            BuildWhereStaTement(filters, CountQuery, countcom);
            countcom.Connection = connection;
            countcom.CommandText = CountQuery.ToString();
            var Totalrecords = Convert.ToInt32(countcom.ExecuteScalar());
            records.Pager.TotalRecords = Totalrecords;
            records.Pager.TotalPages = (Totalrecords / PageSize) + 1;
        }

        private void FillresultsNew<T>(Type currentType, EntityCollection<T> records, System.Data .Common .DbDataReader   result) where T : class, IDataEntity
        {
            var CurrentTypeProperties = currentType.GetProperties().ToList();

            while (result.Read())
            {
                //now iterate through the properties
                //if the property has a maped field with Field attribute defined
                // we set the property value from that field
                //otherwise we se the property value from the field with name same as property's name
                T newEntity = (T)Activator.CreateInstance(typeof(T));

                for (int i = 0; i < CurrentTypeProperties.Count; i++)
                {
                    var property = CurrentTypeProperties[i];

                    DataInterface.Attribbutes.Field FieldAttribute = (DataInterface.Attribbutes.Field)(from attr in property.GetCustomAttributes(true) where attr.GetType() == typeof(DataInterface.Attribbutes.Field) select attr).FirstOrDefault();
                    string _fieldName;
                    if (FieldAttribute != null)
                    {
                        _fieldName = FieldAttribute.Name;
                    }
                    else
                    {
                        _fieldName = property.Name;
                    }


                    try
                    {
                        if (result[_fieldName] != DBNull.Value)
                        {
                            if (result[_fieldName].GetType() == typeof(Int64))
                            {
                                if (property.PropertyType == typeof(string))
                                {
                                    property.SetValue(newEntity, result[_fieldName].ToString());
                                }
                                else
                                {
                                    property.SetValue(newEntity, Convert.ToInt32(result[_fieldName]));
                                }
                            }
                            else
                            {
                                if (result[_fieldName].GetType() == typeof(Int32))
                                {
                                    if (property.PropertyType == typeof(string))
                                    {
                                        property.SetValue(newEntity, result[_fieldName].ToString());
                                    }
                                    else
                                    {
                                        property.SetValue(newEntity, result[_fieldName]);
                                    }
                                }
                                else
                                {
                                    property.SetValue(newEntity, result[_fieldName]);
                                }

                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        var aa = ex;
                    }
                }

                if (_Encryptor != null)
                {
                    if (newEntity is IEncryptableClass)
                    {
                        _Encryptor.Decryptproperties((IEncryptableClass)newEntity);
                    }

                }
                records.Add(newEntity);
            }
        }

        private static void BuildWhereStaTement(Base.RequestFilters filters, StringBuilder Query, MySql.Data .MySqlClient .MySqlCommand  com)
        {
            if (filters != null)
            {
                if (filters.Filters != null)
                {

                    for (int i = 0; i < filters.Filters.Count; i++)
                    {
                        if (filters.Filters[i].Field.ToLower().Contains("date"))
                        {
                            Query.Append($" and convert(date,{filters.Filters[i].Field}){filters.Filters[i].Comparer}@{filters.Filters[i].Field}_{i}");
                            com.Parameters.Add(new MySql .Data .MySqlClient .MySqlParameter  ($"@{filters.Filters[i].Field}_{i}", DateTime.Parse(filters.Filters[i].Value, System.Globalization.CultureInfo.InvariantCulture)));
                        }
                        else
                        {
                            Query.Append($" and {filters.Filters[i].Field}{filters.Filters[i].Comparer}@{filters.Filters[i].Field}_{i}");
                            com.Parameters.Add(new MySql.Data.MySqlClient.MySqlParameter($"@{filters.Filters[i].Field}_{i}", filters.Filters[i].Value));
                        }
                    }
                }



                if (filters.OrFilters != null)
                {
                    for (int i = 0; i < filters.OrFilters.Count; i++)
                    {
                        var currentFilters = filters.OrFilters[i].Filters;
                        Query.Append($" and ( 1=1 ");
                        for (int x = 0; x < currentFilters.Count; x++)
                        {
                            if (currentFilters[x].Field.ToLower().Contains("date"))
                            {
                                Query.Append($" or convert(date, {currentFilters[x].Field}){currentFilters[x].Comparer}@{currentFilters[x].Field}{x}_Or{x}");
                                com.Parameters.Add(new MySql.Data.MySqlClient.MySqlParameter($"@{currentFilters[x].Field}{x}_Or{x}", DateTime.Parse(currentFilters[x].Value, System.Globalization.CultureInfo.InvariantCulture)));
                            }
                            else
                            {
                                Query.Append($" or {currentFilters[x].Field}{currentFilters[x].Comparer}@{currentFilters[x].Field}{x}_Or{x}");
                                com.Parameters.Add(new MySql.Data.MySqlClient.MySqlParameter($"@{currentFilters[x].Field}{x}_Or{x}", currentFilters[x].Value));
                            }
                        }

                        Query.Append($" ) ");
                    }
                }
            }
        }
    }
}
