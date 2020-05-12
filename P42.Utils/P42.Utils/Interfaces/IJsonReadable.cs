using System.Collections.Generic;
using Newtonsoft.Json;
namespace P42.Utils
{
    public interface IJsonReadable
    {
        void PropertiesFrom(JsonReader reader);
    }

    public interface IJsonReadable<T> : IJsonReadable, ICopiable<T>
    {
    }

}
