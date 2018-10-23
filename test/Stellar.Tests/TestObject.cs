using System;

namespace Tests
{
    public class TestObject
    {
        public string Name { get; set; }
        public int SomeIntProperty { get; set; }
        public Guid GuidId { get; set; }
        public Nullable<Guid> NullableGuid { get; set; }
        public Nullable<int> NullableInt { get; set; }
    }
}
