﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using Simple.Data.Ado.Schema;

namespace Simple.Data.Ado
{
    class AdoAdapterFinder
    {
        private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, CommandTemplate>> _commandCaches =
            new ConcurrentDictionary<string, ConcurrentDictionary<string, CommandTemplate>>();
        private readonly AdoAdapter _adapter;
        private readonly IDbTransaction _transaction;
        private readonly IDbConnection _connection;
        private readonly DatabaseSchema _schema;

        public AdoAdapterFinder(AdoAdapter adapter) : this(adapter, null)
        {
        }

        public AdoAdapterFinder(AdoAdapter adapter, IDbTransaction transaction)
        {
            if (adapter == null) throw new ArgumentNullException("adapter");
            _adapter = adapter;

            if (transaction != null)
            {
                _transaction = transaction;
                _connection = transaction.Connection;
            }
        }

        public IDictionary<string, object> FindOne(string tableName, SimpleExpression criteria)
        {
            if (criteria == null) return FindAll(_adapter.GetSchema().BuildObjectName(tableName)).FirstOrDefault();
            var commandTemplate = GetCommandTemplate(tableName, criteria);
            return ExecuteSingletonQuery(commandTemplate, criteria.GetValues());
        }

        public Func<object[],IDictionary<string,object>> CreateFindOneDelegate(string tableName, SimpleExpression criteria)
        {
            if (criteria == null)
            {
                return _ => FindAll(_adapter.GetSchema().BuildObjectName(tableName)).FirstOrDefault();
            }
            var commandTemplate = GetCommandTemplate(tableName, criteria);
            return args => ExecuteSingletonQuery(commandTemplate, args);
        }

        public IEnumerable<IDictionary<string, object>> Find(string tableName, SimpleExpression criteria)
        {
            if (criteria == null) return FindAll(_adapter.GetSchema().BuildObjectName(tableName));
            var commandTemplate = GetCommandTemplate(tableName, criteria);
            return ExecuteQuery(commandTemplate, criteria.GetValues());
        }

        private CommandTemplate GetCommandTemplate(string tableName, SimpleExpression criteria)
        {
            var tableCommandCache = _commandCaches.GetOrAdd(tableName,
                                                            _ => new ConcurrentDictionary<string, CommandTemplate>());

            var hash = new ExpressionHasher().Format(criteria);
            return tableCommandCache.GetOrAdd(hash,
                                              _ =>
                                              new FindHelper(_adapter.GetSchema())
                                                  .GetFindByCommand(_adapter.GetSchema().BuildObjectName(tableName), criteria)
                                                  .GetCommandTemplate(_adapter.GetSchema().FindTable(_adapter.GetSchema().BuildObjectName(tableName))));
        }

        private IEnumerable<IDictionary<string, object>> FindAll(ObjectName tableName)
        {
            return ExecuteQuery("select * from " + _adapter.GetSchema().FindTable(tableName).QualifiedName);
        }

        private IEnumerable<IDictionary<string, object>> ExecuteQuery(CommandTemplate commandTemplate, IEnumerable<object> parameterValues)
        {
            var connection = _connection ?? _adapter.CreateConnection();
            var command = commandTemplate.GetDbCommand(connection, parameterValues);
            command.Transaction = _transaction;
            return TryExecuteQuery(connection, command, commandTemplate.Index);
        }

        private IDictionary<string, object> ExecuteSingletonQuery(CommandTemplate commandTemplate, IEnumerable<object> parameterValues)
        {
            var connection = _connection ?? _adapter.CreateConnection();
            var command = commandTemplate.GetDbCommand(connection, parameterValues);
            command.Transaction = _transaction;
            return TryExecuteSingletonQuery(connection, command, commandTemplate.Index);
        }

        private IEnumerable<IDictionary<string, object>> ExecuteQuery(string sql, params object[] values)
        {
            var connection = _connection ?? _adapter.CreateConnection();
            var command = new CommandHelper(_adapter).Create(connection, sql, values);
            command.Transaction = _transaction;
            return TryExecuteQuery(connection, command);
        }

        private static IEnumerable<IDictionary<string, object>> TryExecuteQuery(IDbConnection connection, IDbCommand command)
        {
            try
            {
                return command.ToEnumerable(connection);
            }
            catch (DbException ex)
            {
                throw new AdoAdapterException(ex.Message, command);
            }
        }

        private static IEnumerable<IDictionary<string, object>> TryExecuteQuery(IDbConnection connection, IDbCommand command, IDictionary<string, int> index)
        {
            try
            {
                return command.ToEnumerable(connection, index);
            }
            catch (DbException ex)
            {
                throw new AdoAdapterException(ex.Message, command);
            }
        }

        private static IDictionary<string, object> TryExecuteSingletonQuery(IDbConnection connection, IDbCommand command, IDictionary<string, int> index)
        {
            command.WriteTrace();
            using (connection)
            using (command)
            {
                try
                {
                    if (connection.State != ConnectionState.Open)
                        connection.Open();
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return reader.ToDictionary(index);
                        }
                    }
                }
                catch (DbException ex)
                {
                    throw new AdoAdapterException(ex.Message, command);
                }
            }
            return null;
        }
    }
}
