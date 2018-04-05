using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq.Expressions;

namespace Stellar
{
    internal class OrderByRewritter : DbExpressionVisitor
    {
        IEnumerable<OrderExpression> _gatheredOrderings;
        bool _isOuterMostSelect;

        public Expression Rewrite(Expression expression)
        {
            _isOuterMostSelect = true;
            return Visit(expression);
        }

        protected override Expression VisitSelect(SelectExpression select)
        {
            var saveIsOuterMostSelect = _isOuterMostSelect;
            try
            {
                _isOuterMostSelect = false;
                select = (SelectExpression)base.VisitSelect(select);
                var hasOrderBy = select.OrderBy != null && select.OrderBy.Count > 0;
                if (hasOrderBy)
                    PrependOrderings(select.OrderBy);

                var canHaveOrderBy = saveIsOuterMostSelect;
                var canPassOnOrderings = !saveIsOuterMostSelect;
                var orderings = (canHaveOrderBy) ? _gatheredOrderings : null;
                var columns = select.Columns;
                if (_gatheredOrderings != null)
                {
                    if (canPassOnOrderings)
                    {
                        var producedAliases = new AliasesProduced().Gather(select.From);
                        // reproject order expressions using this select's alias so the other select will have properly formed expressions
                        var project = RebindOrderings(_gatheredOrderings, select.Alias, producedAliases, select.Columns);
                        _gatheredOrderings = project.Orderings;
                        columns = project.Columns;
                    }
                    else
                    {
                        _gatheredOrderings = null;
                    }
                }
                if(orderings != select.OrderBy || columns != select.Columns)
                {
                    select = new SelectExpression(select.Type, select.Alias, columns, select.From, select.Where, orderings);
                }
                return select;
            }
            finally
            {
                _isOuterMostSelect = saveIsOuterMostSelect;
            }
        }

        private void PrependOrderings(IEnumerable<OrderExpression> newOrderings)
        {
            if (newOrderings != null)
            {
                if (_gatheredOrderings == null)
                {
                    _gatheredOrderings = newOrderings;
                }
                else
                {
                    var list = _gatheredOrderings as List<OrderExpression>;
                    if (list == null)
                    {
                        _gatheredOrderings = list = new List<OrderExpression>(_gatheredOrderings);
                    }
                    list.InsertRange(0, newOrderings);
                }
            }
        }

        protected class BindResult
        {
            public BindResult(IEnumerable<ColumnDeclaration> columns, IEnumerable<OrderExpression> orderings)
            {
                Columns = columns as ReadOnlyCollection<ColumnDeclaration>;
                Orderings = orderings as ReadOnlyCollection<OrderExpression>;

                if (Columns == null)
                    Columns = new List<ColumnDeclaration>(columns).AsReadOnly();

                if (Orderings == null)
                    Orderings = new List<OrderExpression>(orderings).AsReadOnly();
            }

            public ReadOnlyCollection<ColumnDeclaration> Columns { get; private set; }
            public ReadOnlyCollection<OrderExpression> Orderings { get; private set; }
        }

        protected virtual BindResult RebindOrderings(IEnumerable<OrderExpression> orderings, string alias, HashSet<string> existingAliases, IEnumerable<ColumnDeclaration> existingColumns)
        {
            List<ColumnDeclaration> newColumns = null;
            var newOrderings = new List<OrderExpression>();
            foreach (var ordering in orderings)
            {
                var expr = ordering.Expression;
                var column = expr as ColumnExpression;
                if (column == null || (existingAliases != null && existingAliases.Contains(column.Alias)))
                {
                    // check to see if a declared column already contains a similar expression
                    var ordinal = 0;
                    foreach (var declaration in existingColumns)
                    {
                        var columnExpression = declaration.Expression as ColumnExpression;
                        if (declaration.Expression == ordering.Expression || (column != null && column.Alias == columnExpression.Alias && column.Name == columnExpression.Name))
                        {
                            // found i, so make a reference to this column
                            expr = new ColumnExpression(column.Type, alias, declaration.Name, ordinal);
                            break;
                        }
                        ordinal++;
                    }
                    // if not already projectd, add a new column declaration for it
                    if (expr == ordering.Expression)
                    {
                        if (newColumns == null)
                        {
                            newColumns = new List<ColumnDeclaration>(existingColumns);
                            existingColumns = newColumns;
                        }
                        var columnName = column != null ? column.Name : "c" + ordinal;
                        newColumns.Add(new ColumnDeclaration(columnName, ordering.Expression));
                        expr = new ColumnExpression(expr.Type, alias, columnName, ordinal);
                    }
                    newOrderings.Add(new OrderExpression(ordering.OrderType, expr));
                }
            }
            return new BindResult(existingColumns, newOrderings);
        }
    }
}
