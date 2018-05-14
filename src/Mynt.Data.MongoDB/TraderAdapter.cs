using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace Mynt.Data.MongoDB
{
    public class TraderAdapter
    {
        [BsonId]
        [BsonIgnoreIfDefault]
        public ObjectId Id { get; set; }
        public string Identifier { get; set; }
        public double StakeAmount { get; set; }
        public double CurrentBalance { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime LastUpdated { get; set; }
        public bool IsBusy { get; set; }
        public bool IsArchived { get; set; }
    }
}
