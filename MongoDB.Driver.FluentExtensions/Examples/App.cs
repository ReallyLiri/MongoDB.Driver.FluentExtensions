namespace MongoDB.Driver.FluentExtensions.Examples
{
    public static class App
    {
        public static async void Main(string[] args)
        {
            var client = new MongoClient("mongodb://localhost:27017");
            var database = client.GetDatabase("foo");
            var collection = database.GetCollection<Entity>();

            await database.UpsertAsync(new Entity
            {
                Key = "k1",
                Name = "Test 1",
                Age = 17
            });
            await database.UpsertAsync(new Entity
            {
                Key = "k1",
                Name = "Test 1.1",
                Age = 17
            });
            await database.UpsertAsync(new Entity
            {
                Key = "k2",
                Name = "Test 2",
                Age = 71
            });

            var allEntities = database.GetAllAsync<Entity>();
            var allOld = database.GetAllAsync<Entity>(entity => entity.Age > 70);
            var entity1 = database.GetAsync<Entity>("k1");
            var allAges = database.GetVectorAsync<Entity, int>(nameof(Entity.Age));
            var youngAges = database.GetVectorAsync<Entity, int>(nameof(Entity.Age), entity => entity.Age < 20);
            await database.SetFieldAsync<Entity, int>(nameof(Entity.Age), "k1", 18);
        }
    }
}