using System.Linq.Expressions;

namespace Stellar
{
    internal class ColumnDeclaration
    {
        internal ColumnDeclaration(string name, Expression expression)
        {
            Name = name;
            Expression = expression;
        }

        internal string Name { get; private set; }
        internal Expression Expression { get; private set; }
    }
}
