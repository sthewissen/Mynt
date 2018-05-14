using System;
namespace Mynt.Data.MongoDB
{
    public class MongoDBOptions
    {
		public string MongoUrl { get; set; } = "mongodb://127.0.0.1:27017";
		public string MongoDatabaseName { get; set; } = "Mynt";
    }
}
