using System;

namespace MongoDB.Driver.FluentExtensions
{
    [AttributeUsage(AttributeTargets.Property)]
    public class MongoIndexAttribute : Attribute
    {
    }
}