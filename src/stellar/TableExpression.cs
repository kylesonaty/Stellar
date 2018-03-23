using System;
using System.Linq.Expressions;

namespace Stellar
{
    internal class TableExpression : Expression
    {
        internal TableExpression(Type type, string alias, string name) : base((ExpressionType)DbExpressionType.Table, type)
        {
            Alias = alias;
            Name = name;
        }

        internal string Alias { get; private set; }
        internal string Name { get; private set; }
    }
}
