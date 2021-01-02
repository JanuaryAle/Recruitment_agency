using MongoDB.Bson;


namespace Agency.Models
{
    public class Staffer
    {
        public ObjectId _id { get; set; }
        public string name { get; set; }
        public Location location { get; set; }
        public string position { get; set; }
        public Contacts contacts { get; set; }
        public double salary { get; set; }
        public double experience { get; set; }
    }
}