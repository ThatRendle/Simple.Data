﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Simple.Data
{
    /// <summary>
    /// The entry class for Simple.Data. Provides static methods for opening databases,
    /// and implements runtime dynamic functionality for resolving database-level objects.
    /// </summary>
    public partial class Database : DataStrategy
    {
        private static readonly IDatabaseOpener DatabaseOpener;
        private readonly ConcurrentDictionary<string, dynamic> _members = new ConcurrentDictionary<string, dynamic>();
        private readonly Adapter _adapter;

        static Database()
        {
            DatabaseOpener = new DatabaseOpener();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Database"/> class.
        /// </summary>
        /// <param name="adapter">The adapter to use for data access.</param>
        internal Database(Adapter adapter)
        {
            _adapter = adapter;
        }

        protected override Adapter GetAdapter()
        {
            return _adapter;
        }

        public static IDatabaseOpener Opener
        {
            get { return DatabaseOpener; }
        }

        /// <summary>
        /// Gets a default instance of the Database. This connects to an ADO.NET data source
        /// specified in the 'Simple.Data.Properties.Settings.ConnectionString' config ConnectionStrings setting.
        /// </summary>
        /// <value>The default database.</value>
        public static dynamic Default
        {
            get { return DatabaseOpener.OpenDefault(); }
        }

        /// <summary>
        ///  Finds data from the specified "table".
        ///  </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="criteria">The criteria. This may be <c>null</c>, in which case all records should be returned.</param>
        /// <returns>The list of records matching the criteria. If no records are found, return an empty list.</returns>
        public override IEnumerable<IDictionary<string, object>> Find(string tableName, SimpleExpression criteria)
        {
            return _adapter.Find(tableName, criteria);
        }

        /// <summary>
        ///  Inserts a record into the specified "table".
        ///  </summary><param name="tableName">Name of the table.</param><param name="data">The values to insert.</param><returns>If possible, return the newly inserted row, including any automatically-set values such as primary keys or timestamps.</returns>
        public override IDictionary<string, object> Insert(string tableName, IDictionary<string, object> data)
        {
            return _adapter.Insert(tableName, data);
        }

        /// <summary>
        ///  Updates the specified "table" according to specified criteria.
        ///  </summary><param name="tableName">Name of the table.</param><param name="data">The new values.</param><param name="criteria">The expression to use as criteria for the update operation.</param><returns>The number of records affected by the update operation.</returns>
        public override int Update(string tableName, IDictionary<string, object> data, SimpleExpression criteria)
        {
            return _adapter.Update(tableName, data, criteria);
        }

        /// <summary>
        ///  Deletes from the specified table.
        ///  </summary><param name="tableName">Name of the table.</param><param name="criteria">The expression to use as criteria for the delete operation.</param><returns>The number of records which were deleted.</returns>
        public override int Delete(string tableName, SimpleExpression criteria)
        {
            return _adapter.Delete(tableName, criteria);
        }

    	public override object Max(string tableName, string columnName, SimpleExpression criteria)
    	{
    		return _adapter.Max(tableName, columnName, criteria);
    	}

		public override object Min(string tableName, string columnName, SimpleExpression criteria)
		{
			return _adapter.Min(tableName, columnName, criteria);
		}
		
		public SimpleTransaction BeginTransaction()
        {
            return SimpleTransaction.Begin(this);
        }

        public SimpleTransaction BeginTransaction(string name)
        {
            return SimpleTransaction.Begin(this, name);
        }

        protected override Database GetDatabase()
        {
            return this;
        }
    }
}