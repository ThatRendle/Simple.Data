﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Linq;
using System.Text;

namespace Simple.Data
{
    /// <summary>
    /// Provides an abstraction over the underlying data adapter, if it is transaction-capable.
    /// </summary>
    public sealed class SimpleTransaction : DataStrategy, IDisposable
    {
        private readonly Database _database;

        private readonly IAdapterWithTransactions _adapter;
        private IAdapterTransaction _adapterTransaction;

        private SimpleTransaction(IAdapterWithTransactions adapter, Database database)
        {
            if (adapter == null) throw new ArgumentNullException("adapter");
            if (database == null) throw new ArgumentNullException("database");
            _adapter = adapter;
            _database = database;
        }

        private void Begin()
        {
            _adapterTransaction = _adapter.BeginTransaction();
        }

        private void Begin(string name)
        {
            _adapterTransaction = _adapter.BeginTransaction(name);
        }

        internal static SimpleTransaction Begin(Database database)
        {
            SimpleTransaction transaction = CreateTransaction(database);
            transaction.Begin();
            return transaction;
        }

        internal static SimpleTransaction Begin(Database database, string name)
        {
            SimpleTransaction transaction = CreateTransaction(database);
            transaction.Begin(name);
            return transaction;
        }

        private static SimpleTransaction CreateTransaction(Database database)
        {
            var adapterWithTransactions = database.Adapter as IAdapterWithTransactions;
            if (adapterWithTransactions == null) throw new NotSupportedException();
            return new SimpleTransaction(adapterWithTransactions, database);
        }


        internal Database Database
        {
            get { return _database; }
        }

        /// <summary>
        /// Gets the name assigned to the transaction.
        /// </summary>
        /// <value>The name.</value>
        public string Name
        {
            get { return _adapterTransaction.Name; }
        }

        public IAdapterTransaction AdapterTransaction
        {
            get { return _adapterTransaction; }
        }

        /// <summary>
        /// Commits all changes to the database and cleans up resources associated with the transaction.
        /// </summary>
        public void Commit()
        {
            _adapterTransaction.Commit();
        }

        /// <summary>
        /// Rolls back all changes to the database and cleans up resources associated with the transaction.
        /// </summary>
        public void Rollback()
        {
            _adapterTransaction.Rollback();
        }

        public override IEnumerable<IDictionary<string, object>> Find(string tableName, SimpleExpression criteria)
        {
            return _adapter.Find(tableName, criteria, AdapterTransaction);
        }

        public override IDictionary<string, object> Insert(string tableName, IDictionary<string, object> data)
        {
            return _adapter.Insert(tableName, data, AdapterTransaction);
        }

        public override int Update(string tableName, IDictionary<string, object> data, SimpleExpression criteria)
        {
            return _adapter.Update(tableName, data, criteria, AdapterTransaction);
        }

        public override int Delete(string tableName, SimpleExpression criteria)
        {
            return _adapter.Delete(tableName, criteria, AdapterTransaction);
        }

		public override object Max(string tableName, string columnName, SimpleExpression criteria)
    	{
    		return _adapter.Max(tableName, columnName, criteria);
    	}

		public override object Min(string tableName, string columnName, SimpleExpression criteria)
		{
			return _adapter.Min(tableName, columnName, criteria);
		}

    	/// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            try
            {
                _adapterTransaction.Dispose();
            }
            catch (Exception ex)
            {
                Trace.WriteLine("IAdapterTransaction Dispose threw exception: " + ex.Message);
            }
        }

        protected override Adapter GetAdapter()
        {
            return _adapter as Adapter;
        }

        protected override Database GetDatabase()
        {
            return _database;
        }
    }
}
