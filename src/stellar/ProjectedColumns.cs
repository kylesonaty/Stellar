using System.Collections.ObjectModel;
using System.Linq.Expressions;

namespace Stellar
{
    internal sealed class ProjectedColumns
    {
        internal ProjectedColumns(Expression projector, ReadOnlyCollection<ColumnDeclaration> columns)
        {
            Projector = projector;
            Columns = columns;
        }

        internal Expression Projector { get; private set; }
        internal ReadOnlyCollection<ColumnDeclaration> Columns { get; private set; }
    }
}
