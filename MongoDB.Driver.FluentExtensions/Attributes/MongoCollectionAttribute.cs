using System;

namespace MongoDB.Driver.FluentExtensions
{
    [AttributeUsage(AttributeTargets.Class)]
    public class MongoCollectionAttribute : Attribute
    {
        public MongoCollectionAttribute(string collection)
            => Collection = collection;

        public string Collection { get; }
    }
}