using Stellar.Serialization;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;

namespace Stellar
{
    internal class ProjectionReader<T> : IEnumerable<T>, IEnumerable
    {
        Enumerator enumerator;

        internal ProjectionReader(ISerializer serialier, Func<ProjectionRow, T> projector)
        {
            enumerator = new Enumerator(serialier, projector);
        }

        public IEnumerator<T> GetEnumerator()
        {
            Enumerator e = enumerator;
            if (e == null)
            {
                throw new InvalidOperationException("Cannot enumerate more than once");
            }

            enumerator = null;
            return e;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        class Enumerator : ProjectionRow, IEnumerator<T>, IEnumerator, IDisposable
        {
            ISerializer serializer;
            DbDataReader dbDataReader;
            T current;
            Func<ProjectionRow, T> projector;

            internal Enumerator(ISerializer serializer, Func<ProjectionRow, T> projector)
            {
                this.serializer = serializer;
                this.projector = projector;
            }

            public override object GetValue(int index)
            {
                if (index >= 0)
                {
                    return string.Empty;
                    // TODO: this is from sql not from json 
                    // here it was using DbDataReader
                    // We have a serializer so i don't think we need to get each row. 
                    // more like we need to project is row for the collection


                    return null;
                }
                return null;
            }

            public T Current
            {
                get { return this.current; }
            }

            object IEnumerator.Current
            {
                get { return this.current; }
            }

            public void Reset()
            {

            }

            public bool MoveNext()
            {
                if (this.dbDataReader.Read())
                {
                    this.current = this.projector(this);
                    return true;
                }
                return false;
                // TODO: Not sure yet what to do here, again this is the DbDataReader.read
            }

            public void Dispose()
            {
                // TODO: might not need this guy.
            }
        }
    }
}
