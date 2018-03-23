using System.Collections.Generic;
using System.Linq.Expressions;

namespace Stellar
{
    /// <summary>
    ///  returns the set of all aliases produced by a query source
    /// </summary>
    internal class AliasesProduced : DbExpressionVisitor
    {
        HashSet<string> aliases;

        public HashSet<string> Gather(Expression source)
        {
            this.aliases = new HashSet<string>();
            this.Visit(source);
            return this.aliases;
        }

        protected override Expression VisitSelect(SelectExpression select)
        {
            this.aliases.Add(select.Alias);
            return select;
        }

        protected override Expression VisitTable(TableExpression table)
        {
            this.aliases.Add(table.Alias);
            return table;
        }
    }
}