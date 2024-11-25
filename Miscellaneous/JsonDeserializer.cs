using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace MachineClassLibrary.Miscellaneous
{
    public class JsonDeserializer<TObject>
    {
        private List<Type> _knownTypes;
        public JsonDeserializer()
        {
            _knownTypes = new();
            _knownTypes.Add(typeof(TObject));
        }
        public JsonDeserializer<TObject> SetKnownType<TKnown>()
        {
            _knownTypes.Add(typeof(TKnown));
            return this;
        }
        public TObject Deserialize(string jsonTree)
        {
            TObject result;
            try
            {
                result = JsonConvert.DeserializeObject<TObject>(jsonTree, new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.Objects,
                    SerializationBinder = new TypesBinder
                    {
                        KnownTypes = _knownTypes
                    }
                }) ?? throw new ArgumentException($"Can not deserialize {nameof(jsonTree)}");
            }
            catch (Exception)
            {
                throw;
            }
            return result;
        }

        public TObject DeserializeFromFile(string jsonpath) => Deserialize(File.ReadAllText(jsonpath));
    }

}
