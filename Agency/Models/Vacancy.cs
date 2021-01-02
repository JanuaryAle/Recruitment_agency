using MongoDB.Bson;
using System.Collections.Generic;


namespace Agency.Models
{
    public class Vacancy
    {
        public object _id { get; set; }
        public Specialization specialization { get; set; }
        public Conditions conditions { get; set; }
        public Requirements requirements { get; set; }
        public Location location { get; set; }
        public List<ObjectId> views { get; set; }
    }
}