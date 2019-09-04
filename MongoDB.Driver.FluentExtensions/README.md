# MongoDB.Driver.FluentExtensions

Adding some fluency to [MongoDB.Driver](https://github.com/mongodb/mongo-csharp-driver).

## Primary Features

### Upsert

`Task UpsertAsync<T>(this IMongoDatabase database, T entity)`

### Get by key

`Task<T> GetAsync<T>(this IMongoDatabase database, string key)`

### Get all

`Task<ICollection<T>> GetAllAsync<T>(this IMongoDatabase database, Expression<Func<T, bool>> where)`

`Task<ICollection<T>> GetAllAsync<T>(this IMongoDatabase database, FilterDefinition<T> filter = null)`

Get all entities with an optional filter, either with a simple expression on the entity or with the old fashioned `FilterDefinition`.

### Get vector

`Task<ICollection<T>> GetVectorAsync<T>(this IMongoDatabase database, string target, Expression<Func<T, bool>> where)`

`Task<ICollection<T>> GetVectorAsync<T>(this IMongoDatabase database, string target, FilterDefinition<T> filter = null)`

Similar to `GetAllAsync`, but only projects a single property collection (vector) of the entity.

### Set field

`Task<bool> SetFieldAsync<TEntity, TField>(this IMongoDatabase database, string key, string field, TField value)`

Set the value of some entity field given the entity unique key.

## Supporting Features

The following functionality is added:

### Mongo Collection

`IMongoCollection<T> GetCollection<T>(this IMongoDatabase database)`

Simply use `_mongoDatabase.GetCollection<T>()`, that is assuming you are have only one collection per model type.

This method also asserts that your class is annotated with `MongoCollectionAnnotation`, to specify the collection name.

### Mongo Indexes

When you annotate any property with `MongoIndexAttribute`, a matching index will be created (if it doesn't exist) upon first access to the relevant collection.

### Id Field

Shortcut to creating a `FilterDefinition` by the unique id field, either by using an entity instance or only the id value.

## Example

See [Examples](./Examples).
