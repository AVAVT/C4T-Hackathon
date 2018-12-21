using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using Newtonsoft.Json;

public static class ObjectExtensions
{
  public static T Clone<T>(this T source)
  {
    if (!typeof(T).IsSerializable)
    {
      throw new ArgumentException("The type must be serializable.", "source");
    }

    // Don't serialize a null object, simply return the default for that object
    if (Object.ReferenceEquals(source, null))
    {
      return default(T);
    }
    
    var json = JsonConvert.SerializeObject(source);
    return JsonConvert.DeserializeObject<T>(json);
  }
}