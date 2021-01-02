using MongoDB.Bson;
using System;


namespace Agency.Models
{
    public class Event
    {
        public string name { get; set; }
        public Specialization specialization { get; set; }
        public Object result { get; set; }
        public DateTime date { get; set; }
        public ObjectId consultant { get; set; }
    }
}