using MongoDB.Bson;
using System;
using System.Collections.Generic;


namespace Agency.Models
{
    public class Employer
    {
        public ObjectId _id { get; set; }
        public string name { get; set; }
        public string tin { get; set; }
        public Contacts contacts { get; set; }
        public Location location { get; set; }
        public Double balance { get; set; }
        public string password { get; set; }
        public List<Vacancy> vacancies { get; set; }
    }
}