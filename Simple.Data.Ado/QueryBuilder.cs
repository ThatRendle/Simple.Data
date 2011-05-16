﻿using System;
using System.Data;
using System.Linq;
using System.Collections.Generic;
using Simple.Data.Ado.Schema;

namespace Simple.Data.Ado
{
    public class QueryBuilder
    {
        private readonly AdoAdapter _adoAdapter;
        private readonly DatabaseSchema _schema;

        private ObjectName _tableName;
        private Table _table;
        private SimpleQuery _query;
        private CommandBuilder _commandBuilder;

        public QueryBuilder(AdoAdapter adoAdapter)
        {
            _adoAdapter = adoAdapter;
            _schema = _adoAdapter.GetSchema();
        }

        public ICommandBuilder Build(SimpleQuery query)
        {
            SetQueryContext(query);

            HandleQueryCriteria();
            HandleOrderBy();
            HandlePaging();

            return _commandBuilder;
        }

        private void SetQueryContext(SimpleQuery query)
        {
            _query = query;
            _tableName = ObjectName.Parse(query.TableName);
            _table = _schema.FindTable(_tableName);
            _commandBuilder = new CommandBuilder(GetSelectClause(_tableName), _schema.SchemaProvider);
        }

        private void HandleQueryCriteria()
        {
            if (_query.Criteria == null) return;

            var joins = new Joiner(JoinType.Inner, _schema).GetJoinClauses(_tableName, _query.Criteria);
            if (!string.IsNullOrWhiteSpace(joins))
            {
                _commandBuilder.Append(" " + joins);
            }
            _commandBuilder.Append(" WHERE " + new ExpressionFormatter(_commandBuilder, _schema).Format(_query.Criteria));
        }

        private void HandleOrderBy()
        {
            if (_query.Order == null) return;

            var orderNames = _query.Order.Select(ToOrderByDirective);
            _commandBuilder.Append(" ORDER BY " + string.Join(", ", orderNames));
        }

        private void HandlePaging()
        {
            if (_query.SkipCount != null || _query.TakeCount != null)
            {
                var queryPager = _adoAdapter.ProviderHelper.GetCustomProvider<IQueryPager>(_adoAdapter.ConnectionProvider);
                if (queryPager == null)
                {
                    throw new NotSupportedException("Paging is not supported by the current ADO provider.");
                }

                var skipTemplate = _commandBuilder.AddParameter("skip", DbType.Int32, _query.SkipCount ?? 0);
                var takeTemplate = _commandBuilder.AddParameter("take", DbType.Int32, _query.TakeCount ?? int.MaxValue - _query.SkipCount);
                _commandBuilder.SetText(queryPager.ApplyPaging(_commandBuilder.Text, skipTemplate.Name, takeTemplate.Name));
            }
        }

        private string ToOrderByDirective(SimpleOrderByItem item)
        {
            var col = _table.FindColumn(item.Reference.GetName());
            var direction = item.Direction == OrderByDirection.Descending ? " DESC" : string.Empty;
            return col.QuotedName + direction;

        }

        private string GetSelectClause(ObjectName tableName)
        {
            var table = _schema.FindTable(tableName);
            return string.Format("select {0} from {1}",
                string.Join(",", table.Columns.Select(c => string.Format("{0}.{1}", table.QualifiedName, c.QuotedName))),
                table.QualifiedName);
        }
    }
}