using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Stellar
{
    public class CosmosQueryable<T> : ICosmosQueryable<T>, IOrderedQueryable<T>, IOrderedQueryable
    {
        public Type ElementType => typeof(T);

        public Expression Expression { get; private set; }

        public IQueryProvider Provider { get; private set; }
        
        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return ((IEnumerable<T>)Provider.Execute(Expression)).GetEnumerator();
        }

        public IEnumerator GetEnumerator()
        {
            return ((IEnumerable)Provider.Execute(Expression)).GetEnumerator();
        }

        public CosmosQueryable(QueryProvider provider)
        {
            Provider = provider ?? throw new ArgumentNullException(nameof(provider));
            Expression = Expression.Constant(this);
        }

        public CosmosQueryable(QueryProvider provider, Expression expression)
        {
            Provider = provider ?? throw new ArgumentNullException(nameof(provider));
            Expression = expression ?? throw new ArgumentNullException(nameof(expression));

            if (!typeof(IQueryable<T>).IsAssignableFrom(expression.Type))
            {
                throw new ArgumentOutOfRangeException(nameof(expression));
            }
        }

        public override string ToString()
        {
            if (Provider is CosmosQuery p)
                return p.GetQueryText(Expression);
            return base.ToString();
        }
    }
}
