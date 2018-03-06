namespace Stellar.Serialization
{
    public interface ISerializer
    {
        T Deserailize<T>(string json);
        string Serialize(object entity);
    }
}