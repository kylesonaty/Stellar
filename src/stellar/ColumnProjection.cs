using System.Linq.Expressions;

namespace Stellar
{
    internal class ColumnProjection
    {
        internal string Columns;
        internal Expression Selector;
    }
}