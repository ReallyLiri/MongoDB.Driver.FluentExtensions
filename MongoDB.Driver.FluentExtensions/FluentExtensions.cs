using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MongoDB.Driver.FluentExtensions
{
    public static class FluentExtensions
    {
        private static readonly ConcurrentDictionary<Type, object> CollectionsCache =
            new ConcurrentDictionary<Type, object>();

        private static readonly ConcurrentDictionary<Type, PropertyInfo> IdPropertiesCache =
            new ConcurrentDictionary<Type, PropertyInfo>();

        private static IEnumerable<PropertyInfo> IndexProperties<T>()
        {
            var props = typeof(T).GetProperties();
            foreach (var propertyInfo in props)
            {
                var indexAttribute = propertyInfo.GetCustomAttribute<MongoIndexAttribute>();
                if (indexAttribute != null)
                {
                    yield return propertyInfo;
                }
            }
        }

        private static void CreateAllIndexes<T>(this IMongoCollection<T> collection)
        {
            var createIndexModels = IndexProperties<T>()
                .Select(indexProperty => new CreateIndexModel<T>($"{{ {indexProperty.Name}: 1 }}"))
                .ToList();
            if (!createIndexModels.Any())
            {
                return;
            }

            collection.Indexes.CreateMany(createIndexModels);
        }

        public static IMongoCollection<T> GetCollection<T>(this IMongoDatabase database)
        {
            return (IMongoCollection<T>) CollectionsCache.GetOrAdd(
                typeof(T),
                type =>
                {
                    var collectionName = typeof(T).GetCustomAttribute<MongoCollectionAttribute>()
                        .Collection;
                    var collection = database.GetCollection<T>(collectionName);
                    collection.CreateAllIndexes();
                    return collection;
                }
            );
        }

        private static PropertyInfo IdProperty<T>()
        {
            return IdPropertiesCache.GetOrAdd(
                typeof(T),
                type =>
                {
                    var props = typeof(T).GetProperties();
                    PropertyInfo idProperty = null;
                    foreach (var propertyInfo in props)
                    {
                        var bsonIdAttribute = propertyInfo.GetCustomAttribute<BsonIdAttribute>();
                        if (bsonIdAttribute != null)
                        {
                            idProperty = propertyInfo;
                        }
                    }

                    if (idProperty == null)
                    {
                        throw new ApplicationException(
                            $"No {nameof(BsonIdAttribute)} attribute defined on {typeof(T)}");
                    }

                    return idProperty;
                }
            );
        }

        public static FilterDefinition<T> IdFilter<T>(this T entity)
            => IdFilter<T>(
                IdProperty<T>()
                    .GetValue(entity)
                    ?.ToString()
            );

        public static FilterDefinition<T> IdFilter<T>(this string key)
            => Builders<T>.Filter.Eq(
                IdProperty<T>()
                    .Name,
                key
            );

        public static string FixPropertyNameIfNeeded<T>(this string property)
            => property == IdProperty<T>()
                   .Name
                ? "_id"
                : property;

        public static Task UpsertAsync<T>(this IMongoDatabase database, T entity)
            => database.GetCollection<T>()
                .ReplaceOneAsync(
                    entity.IdFilter(),
                    entity,
                    new UpdateOptions {IsUpsert = true}
                );

        public static async Task<T> GetAsync<T>(this IMongoDatabase database, string key)
        {
            var cursor = await database.GetCollection<T>()
                .FindAsync(
                    key.IdFilter<T>()
                );
            return await cursor.FirstOrDefaultAsync();
        }

        public static Task<ICollection<T>> GetAllAsync<T>(this IMongoDatabase database, Expression<Func<T, bool>> where)
        {
            return database.GetAllAsync(Builders<T>.Filter.Where(where));
        }

        public static async Task<ICollection<T>> GetAllAsync<T>(this IMongoDatabase database, FilterDefinition<T> filter = null)
        {
            filter = filter ?? FilterDefinition<T>.Empty;
            var cursor = await database.GetCollection<T>()
                .FindAsync(
                    filter
                );
            return await cursor.ToListAsync();
        }

        public static Task<ICollection<TTarget>> GetVectorAsync<TEntity, TTarget>(this IMongoDatabase database,
            string target, Expression<Func<TEntity, bool>> where)
        {
            return database.GetVectorAsync<TEntity, TTarget>(target, Builders<TEntity>.Filter.Where(where));
        }

        public static async Task<ICollection<TTarget>> GetVectorAsync<TEntity, TTarget>(this IMongoDatabase database,
            string target, FilterDefinition<TEntity> filter = null)
        {
            filter = filter ?? FilterDefinition<TEntity>.Empty;
            target = target.FixPropertyNameIfNeeded<TEntity>();
            var cursor = await database.GetCollection<TEntity>()
                .FindAsync(
                    filter,
                    new FindOptions<TEntity, BsonDocument>
                    {
                        Projection = Builders<TEntity>.Projection.Include(target)
                    }
                );
            return (await cursor.ToListAsync()).Select(
                    document => (TTarget) Convert.ChangeType(document, typeof(TTarget))
                )
                .ToList();
        }

        public static async Task<bool> SetFieldAsync<TEntity, TField>(this IMongoDatabase database, string key, string field, TField value)
        {
            var result = await database.GetCollection<TEntity>()
                .UpdateOneAsync(
                    key.IdFilter<TEntity>(),
                    Builders<TEntity>.Update.Set(field, value)
                );
            return result.MatchedCount > 0;
        }
    }
}