using System.Linq.Expressions;

namespace Stellar
{
    internal class OrderExpression
    {
        internal OrderExpression(OrderType orderType, Expression expression)
        {
            OrderType = OrderType;
            Expression = expression;
        }

        internal OrderType OrderType { get; private set; }
        internal Expression Expression { get; private set; }
    }
}
