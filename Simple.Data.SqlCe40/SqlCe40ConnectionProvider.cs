﻿using System;
using System.ComponentModel.Composition;
using System.Data;
using System.Data.Common;
using System.Data.SqlServerCe;
using System.Linq;
using Shitty.Data.Ado;
using Shitty.Data.Ado.Schema;

namespace Shitty.Data.SqlCe40
{
    [Export(typeof(IConnectionProvider))]
    [Export("sdf", typeof(IConnectionProvider))]
    [Export("System.Data.SqlServerCe", typeof(IConnectionProvider))]
    [Export("System.Data.SqlServerCe.4.0", typeof(IConnectionProvider))]
    public class SqlCe40ConnectionProvider : IConnectionProvider
    {
        private string _connectionString;
        private bool _checked;

        public void SetConnectionString(string connectionString)
        {
            _connectionString = connectionString;
        }

        public IDbConnection CreateConnection()
        {
            if (!_checked) CheckVersion();
            return new SqlCeConnection(_connectionString);
        }

        private void CheckVersion()
        {
            try
            {
                using (var cn = new SqlCeConnection(_connectionString))
                {
                    cn.Open();
                }
            }
            catch (SqlCeInvalidDatabaseFormatException)
            {
                new SqlCeEngine(_connectionString).Upgrade();
            }
            _checked = true;
        }

        public ISchemaProvider GetSchemaProvider()
        {
            return new SqlCe40SchemaProvider(this);
        }

        public string ConnectionString
        {
            get { return _connectionString; }
        }

        public string GetIdentityFunction()
        {
            return "@@IDENTITY";
        }

        public bool SupportsCompoundStatements
        {
            get { return false; }
        }

        public bool SupportsStoredProcedures
        {
            get { return false; }
        }

        public IProcedureExecutor GetProcedureExecutor(AdoAdapter adapter, ObjectName procedureName)
        {
            throw new NotSupportedException("SQL Server Compact Edition does not support stored procedures.");
        }
    }
}
