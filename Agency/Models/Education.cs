using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;


namespace Agency.Models
{
    public class Education
    { 
        public string type { get; set; }
        public string place { get; set; }
        public Specialization specialization { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime start { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime end { get; set; }
    }
}