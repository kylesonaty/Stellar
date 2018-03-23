using System.Linq.Expressions;

namespace Stellar
{
    internal class ProjectionExpression : Expression
    {
        internal ProjectionExpression(SelectExpression source, Expression projector) : base((ExpressionType)DbExpressionType.Projection, projector.Type)
        {
            Source = source;
            Projector = projector;
        }

        internal SelectExpression Source { get; private set; }
        internal Expression Projector { get; private set; }
    }
}
