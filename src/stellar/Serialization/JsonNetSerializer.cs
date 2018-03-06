using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace Stellar.Serialization
{
    public class JsonNetSerializer : ISerializer
    {
        public T Deserailize<T>(string json)
        {
            return JsonConvert.DeserializeObject<T>(json);
        }

        public string Serialize(object entity)
        {
            var serializer = new JsonSerializer
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };
            
            var jo = JObject.FromObject(entity, serializer);
            jo.Add("_type", entity.GetType().FullName);
            
            return JsonConvert.SerializeObject(jo, Formatting.None);
        }
    }
}
