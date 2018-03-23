using System.Linq.Expressions;

namespace Stellar
{
    internal class TranslateResult
    {
        internal string CommandText;
        internal LambdaExpression Projector;
    }
}