using System.Linq;
using System.Linq.Expressions;

namespace Stellar
{
    internal class TypePredicateInjector
    {
        /// <summary>
        /// Add the expression (x => _type == "Type.FullName" to the expression tree.
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        internal static Expression Inject(Expression expression)
        {
            var type = TypeSystemHelper.GetElementType(expression.Type);
            var targetExpression = Expression.Parameter(type, "x");
            var namespaceExpression = Expression.Constant(type.FullName);
            var stellerTypeExpression = Expression.Constant("_type");
            var equalExp = Expression.Equal(stellerTypeExpression, namespaceExpression);
            var queryableType = typeof(Queryable);
            var whereMethod = queryableType.GetMethods().First(m =>
                {
                    var parameters = m.GetParameters().ToList();
                    return m.Name == "Where" && m.IsGenericMethodDefinition && parameters.Count == 2;
                });
            var whereClause = Expression.Lambda(equalExp, new ParameterExpression[] { targetExpression });
            var genericMethod = whereMethod.MakeGenericMethod(type);
            var exp = Expression.Call(genericMethod, expression, whereClause);
            return exp;
        }
    }
}