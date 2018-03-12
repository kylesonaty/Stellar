using System;

namespace Stellar.Serialization
{
    public interface ISerializer
    {
        T Deserailize<T>(string json);
        object Deserialize(string json, Type type);
        string Serialize(object entity);
    }
}