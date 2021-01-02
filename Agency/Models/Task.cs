using MongoDB.Bson;
using System;


namespace Agency.Models
{
    public class Task
    {
        public ObjectId _id { get; set; }
        public ObjectId employer { get; set; }
        public object vacancy { get; set; }
        public double price { get; set; }
        public DateTime date { get; set; }
        public string status { get; set; }
        public ObjectId consultant { get; set; }
    }
}
