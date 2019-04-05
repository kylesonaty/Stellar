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
        public GenericObject<int> IdName { get; set; }
    }

    public class GenericObject<T>
    {
        public T Id { get; set; }
        public string Name { get; set; }
    }
}
