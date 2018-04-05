using System.Collections.Generic;
using System.Linq.Expressions;

namespace Stellar
{
    internal class RedundantSubqueryRemover : DbExpressionVisitor
    {
        internal Expression Remove(Expression expression)
        {
            return Visit(expression);
        }

        protected override Expression VisitSelect(SelectExpression select)
        {
            select = (SelectExpression)base.VisitSelect(select);

            // first remove all the purely redundant subqueries
            List<SelectExpression> redundant = new RedundantSubqueryGatherer().Gather(select.From);
            if (redundant != null)
            {
                select = (SelectExpression)new SubqueryRemover().Remove(select, redundant);
            }

            // next attempt to merge subqueries, can only merge if subquery is single select (not a join)
            SelectExpression fromSelect = select.From as SelectExpression;
            if (fromSelect != null)
            {
                // can only merge if subquery has simple-projections (no renames or complex expressions)
                if (HasSimpleProjection(fromSelect))
                {
                    // remove the subquery
                    select = (SelectExpression)new SubqueryRemover().Remove(select, fromSelect);
                    // merge where expressions
                    Expression where = select.Where;
                    if (fromSelect.Where != null)
                    {
                        if (where != null)
                        {
                            where = Expression.And(fromSelect.Where, where);
                        }
                        else
                        {
                            where = fromSelect.Where;
                        }
                    }

                    if (where != select.Where)
                    {
                        return new SelectExpression(select.Type, select.Alias, select.Columns, select.From, where, select.OrderBy);
                    }
                }
            }
            return select;
        }

        private static bool IsRedundantSubquery(SelectExpression select)
        {
            return HasSimpleProjection(select) && select.Where == null;
        }

        private static bool HasSimpleProjection(SelectExpression select)
        {
            foreach (var declaration in select.Columns)
            {
                var col = declaration.Expression as ColumnExpression;
                if (col == null || declaration.Name != col.Name)
                {
                    // column name change or column expression is more complex than reference to another column
                    return false;
                }
            }
            return true;
        }

        class RedundantSubqueryGatherer : DbExpressionVisitor
        {
            List<SelectExpression> _redundant;

            internal List<SelectExpression> Gather(Expression source)
            {
                Visit(source);
                return _redundant;
            }

            protected override Expression VisitSelect(SelectExpression select)
            {
                if(IsRedundantSubquery(select))
                {
                    if (_redundant == null)
                    {
                        _redundant = new List<SelectExpression>();
                    }
                    _redundant.Add(select);
                }
                return select;
            }
        }

    }
}
