using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Stellar
{
    internal class SubqueryRemover : DbExpressionVisitor
    {
        HashSet<SelectExpression> _selectsToRemove;
        Dictionary<string, Dictionary<string, Expression>> _map;

        public Expression Remove(SelectExpression outerSelect, params SelectExpression[] selectsToRemove)
        {
            return Remove(outerSelect, (IEnumerable<SelectExpression>)selectsToRemove);
        }

        public Expression Remove(SelectExpression outerSelect, IEnumerable<SelectExpression> selectsToRemove)
        {
            _selectsToRemove = new HashSet<SelectExpression>(selectsToRemove);
            _map = selectsToRemove.ToDictionary(d => d.Alias, d => d.Columns.ToDictionary(d2 => d2.Name, d2 => d2.Expression));
            return Visit(outerSelect);
        }

        protected override Expression VisitSelect(SelectExpression select)
        {
            if (_selectsToRemove.Contains(select))
            {
                return Visit(select.From);
            }
            else
            {
                return base.VisitSelect(select);
            }
        }

        protected override Expression VisitColumn(ColumnExpression column)
        {
            Dictionary<string, Expression> nameMap;
            if (_map.TryGetValue(column.Alias, out nameMap))
            {
                Expression expression;
                if (nameMap.TryGetValue(column.Name, out expression))
                {
                    return Visit(expression);
                }
                throw new Exception("Reference to undefined column");
            }
            return column;
        }
    }
}
