using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Stellar
{
    internal class QueryBinder : ExpressionVisitor
    {
        ColumnProjector _columnProjector;
        Dictionary<ParameterExpression, Expression> _map;
        List<OrderExpression> _thenBys;
        int _aliasCount;

        internal QueryBinder()
        {
            _columnProjector = new ColumnProjector(CanBeColumn);
        }

        private bool CanBeColumn(Expression expression)
        {
            return expression.NodeType == (ExpressionType)DbExpressionType.Column;
        }

        internal Expression Bind(Expression expression)
        {
            _map = new Dictionary<ParameterExpression, Expression>();
            return Visit(expression);
        }

        private static Expression StripQuotes(Expression e)
        {
            while (e.NodeType == ExpressionType.Quote)
            {
                e = ((UnaryExpression)e).Operand;
            }
            return e;
        }

        private string GetNextAlias()
        {
            return "t" + (_aliasCount++);
        }

        private ProjectedColumns ProjectColumns(Expression expression, string newAlias, string existingAlias)
        {
            return _columnProjector.ProjectColumns(expression, newAlias, existingAlias);
        }

        protected override Expression VisitMethodCall(MethodCallExpression m)
        {
            if (m.Method.DeclaringType == typeof(Queryable) || m.Method.DeclaringType == typeof(Enumerable))
            {
                switch (m.Method.Name)
                {
                    case "Where":
                        return BindWhere(m.Type, m.Arguments[0], (LambdaExpression)StripQuotes(m.Arguments[1]));
                    case "Select":
                        return BindSelect(m.Type, m.Arguments[0], (LambdaExpression)StripQuotes(m.Arguments[1]));
                    case "OrderBy":
                        return BindOrderBy(m.Type, m.Arguments[0], (LambdaExpression)StripQuotes(m.Arguments[1]), OrderType.Ascending);
                    case "OrderByDescending":
                        return BindOrderBy(m.Type, m.Arguments[0], (LambdaExpression)StripQuotes(m.Arguments[1]), OrderType.Descending);
                    case "ThenBy":
                        return BindThenBy(m.Arguments[0], (LambdaExpression)StripQuotes(m.Arguments[1]), OrderType.Ascending);
                    case "ThenByDescending":
                        return BindThenBy(m.Arguments[0], (LambdaExpression)StripQuotes(m.Arguments[1]), OrderType.Descending);

                }
                throw new NotSupportedException($"The method '${m.Method.Name}' is not supported");
            }
            return base.VisitMethodCall(m);
        }

        private Expression BindThenBy(Expression source, LambdaExpression orderSelector, OrderType orderType)
        {
            if (_thenBys == null)
                _thenBys = new List<OrderExpression>();

            _thenBys.Add(new OrderExpression(orderType,orderSelector));
            return Visit(source);
        }

        private Expression BindOrderBy(Type resultType, Expression source, LambdaExpression orderSelector, OrderType orderType)
        {
            var thenBys = _thenBys;
            _thenBys = null;
            var projection = (ProjectionExpression)Visit(source);
            _map[orderSelector.Parameters[0]] = projection.Projector;
            var orderings = new List<OrderExpression>();
            orderings.Add(new OrderExpression(orderType, Visit(orderSelector.Body)));
            if (thenBys != null)
            {
                for (int i = 0; i < thenBys.Count; i++)
                {
                    var tb = thenBys[i];
                    var lambda = (LambdaExpression)tb.Expression;
                    _map[lambda.Parameters[0]] = projection.Projector;
                    orderings.Add(new OrderExpression(tb.OrderType, Visit(lambda.Body)));
                }
            }
            var alias = GetNextAlias();
            var pc = ProjectColumns(projection.Projector, alias, projection.Source.Alias);
            return new ProjectionExpression(new SelectExpression(resultType, alias, pc.Columns, projection.Source, null, orderings.AsReadOnly()), pc.Projector);
        }

        private Expression BindWhere(Type resultType, Expression source, LambdaExpression predicate)
        {
            var projection = (ProjectionExpression)Visit(source);
            _map[predicate.Parameters[0]] = projection.Projector;
            var where = Visit(predicate.Body);
            var alias = GetNextAlias();
            var pc = ProjectColumns(projection.Projector, alias, GetExistingAlias(projection.Source));
            return new ProjectionExpression(new SelectExpression(resultType, alias, pc.Columns, projection.Source, where, null), pc.Projector);
        }

        private Expression BindSelect(Type resultType, Expression source, LambdaExpression selector)
        {
            var projection = (ProjectionExpression)Visit(source);
            _map[selector.Parameters[0]] = projection.Projector;
            var expression = Visit(selector.Body);
            var alias = GetNextAlias();
            var pc = ProjectColumns(expression, alias, GetExistingAlias(projection.Source));
            return new ProjectionExpression(new SelectExpression(resultType, alias, pc.Columns, projection.Source, null, null), pc.Projector);
        }

        private static string GetExistingAlias(Expression source)
        {
            switch ((DbExpressionType)source.NodeType)
            {
                case DbExpressionType.Table:
                    return ((TableExpression)source).Alias;
                case DbExpressionType.Select:
                    return ((SelectExpression)source).Alias;
                default:
                    throw new InvalidOperationException($"Invalid source node type'${source.NodeType}'");
            }
        }

        private bool IsTable(object value)
        {
            return value is IQueryable q && q.Expression.NodeType == ExpressionType.Constant;
        }

        private string GetTableName(object table)
        {
            IQueryable tableQuery = (IQueryable)table;
            var rowType = tableQuery.ElementType;
            return rowType.Name;
        }

        private string GetColumnName(MemberInfo member)
        {
            return member.Name;
        }

        private Type GetColumnType(MemberInfo member)
        {
            FieldInfo fi = member as FieldInfo;
            if (fi != null)
            {
                return fi.FieldType;
            }

            var pi = (PropertyInfo)member;
            return pi.PropertyType;
        }

        private IEnumerable<MemberInfo> GetMappedMembers(Type rowType)
        {
            return rowType.GetFields().Cast<MemberInfo>();
        }

        private ProjectionExpression GetTableProjection(object value)
        {
            var table = (IQueryable)value;
            var tableAlias = GetNextAlias();
            var selectAlias = GetNextAlias();
            var bindings = new List<MemberBinding>();
            var columns = new List<ColumnDeclaration>();
            foreach (var mi in GetMappedMembers(table.ElementType))
            {
                var columnName = GetColumnName(mi);
                var columnType = GetColumnType(mi);
                int ordinal = columns.Count;
                bindings.Add(Expression.Bind(mi, new ColumnExpression(columnType, selectAlias, columnName, ordinal)));
                columns.Add(new ColumnDeclaration(columnName, new ColumnExpression(columnType, tableAlias, columnName, ordinal)));
            }

            var projector = Expression.MemberInit(Expression.New(table.ElementType), bindings);
            var resultType = typeof(IEnumerable<>).MakeGenericType(table.ElementType);
            return new ProjectionExpression(new SelectExpression(resultType, selectAlias, columns, new TableExpression(resultType, tableAlias, GetTableName(table)), null, null), projector);
        }

        protected override Expression VisitConstant(ConstantExpression c)
        {
            if (IsTable(c.Value))
                return GetTableProjection(c.Value);
            return c;
        }

        protected override Expression VisitParameter(ParameterExpression p)
        {
            if (_map.TryGetValue(p, out Expression e))
                return e;
            return p;
        }

        protected override Expression VisitMember(MemberExpression m)
        {
            var source = Visit(m.Expression);
            switch (source.NodeType)
            {
                case ExpressionType.MemberInit:
                    var min = (MemberInitExpression)source;
                    for (int i = 0; i < min.Bindings.Count; i++)
                    {
                        if (min.Bindings[i] is MemberAssignment assign && MembersMatch(assign.Member, m.Member))
                        {
                            return assign.Expression;
                        }
                    }
                    break;
                case ExpressionType.New:
                    var nex = (NewExpression)source;
                    if (nex.Members != null)
                    {
                        for (int i = 0; i < nex.Members.Count; i++)
                        {
                            if (MembersMatch(nex.Members[i], m.Member))
                            {
                                return nex.Arguments[i];
                            }
                        }
                    }
                    break;
            }

            if (source == m.Expression)
            {
                return m;
            }

            return MakeMemberAccess(source, m.Member);
        }

        private bool MembersMatch(MemberInfo a, MemberInfo b)
        {
            if (a == b)
            {
                return true;
            }

            if (a is MethodInfo && b is PropertyInfo)
            {
                return a == ((PropertyInfo)b).GetGetMethod();
            }

            else if (a is PropertyInfo && b is MethodInfo)
            {
                return ((PropertyInfo)a).GetGetMethod() == b;
            }
            return false;
        }

        private Expression MakeMemberAccess(Expression source, MemberInfo mi)
        {
            var fi = mi as FieldInfo;
            if (fi != null)
            {
                return Expression.Field(source, fi);
            }
            var pi = (PropertyInfo)mi;
            return Expression.Property(source, pi);
        }
    }
}
