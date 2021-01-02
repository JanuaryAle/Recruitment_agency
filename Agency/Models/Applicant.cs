using MongoDB.Bson;
using System.Collections.Generic;


namespace Agency.Models
{
    public class Applicant
    {
        public ObjectId _id { get; set; }
        public string name { get; set; }
        public int age { get; set; }
        public string password { get; set; }
        public Location location { get; set; }
        public Contacts contacts { get; set; }
        public List<Education> education { get; set; }
        public List<Specialization> specialization { get; set; }
        public List<Experience> experience { get; set; }
        public List<Event> history { get; set; }
        public List<ObjectId> views { get; set; }
    }
}