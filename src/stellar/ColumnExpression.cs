using System;
using System.Linq.Expressions;

namespace Stellar
{
    internal class ColumnExpression : Expression
    {
        internal ColumnExpression(Type type, string alias, string name, int ordinal) : base((ExpressionType)DbExpressionType.Column, type)
        {
            Alias = alias;
            Name = name;
            Ordinal = ordinal;
        }

        internal string Alias { get; private set; }
        internal string Name { get; private set; }
        internal int Ordinal { get; private set; }
    }
}
