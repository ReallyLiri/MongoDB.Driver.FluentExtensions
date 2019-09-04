using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;

namespace MongoDB.Driver.FluentExtensions.Examples
{
    [MongoCollection("entities")]
    public class Entity
    {
        [BsonId]
        public string Key { get; set; }
        
        public string Name { get; set; }
        
        [BsonIgnore]
        public Dictionary<string, string> SomeCache { get; set; }
        
        [MongoIndex]
        public int Age { get; set; }
    }
}