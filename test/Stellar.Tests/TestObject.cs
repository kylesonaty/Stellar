using System;
using System.Collections.Generic;

namespace Tests
{
    public class TestObject
    {
        public string Name { get; set; }
        public int SomeIntProperty { get; set; }
        public Guid GuidId { get; set; }
        public Nullable<Guid> NullableGuid { get; set; }
        public Nullable<int> NullableInt { get; set; }
        public GenericObject<int> IdName { get; set; }
        public List<int> Ids { get; set; }
        public IEnumerable<int> Ints { get; set; }
        public DateTime DateTime { get; set; }
        public DateTimeOffset DateTimeOffset { get; set; }
        public DateTime? PossibleDateTime { get; set; }
        public DateTimeOffset? PossibleDateTimeOffset { get; set; }
        public bool IsBool { get; set; }
    }

    public class GenericObject<T>
    {
        public T Id { get; set; }
        public string Name { get; set; }
    }
}
