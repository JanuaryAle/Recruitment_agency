using System;
using MongoDB.Driver;

namespace Agency
{
    public class MongoHelper
    {

        public static IMongoCollection<Models.Employer> employer_collection { get; set; }
        public static IMongoCollection<Models.Applicant> applicant_collection { get; set; }
        public static IMongoCollection<Models.Staffer> staffer_collection { get; set; }
        public static IMongoCollection<Models.Task> task_collection { get; set; }
        public static IMongoClient client { get; set; }
        public static IMongoDatabase database { get; set; }
        public static string MongoConnection = "mongodb://<user>:<password>@cluster-shard-00-00.3tjnw.mongodb.net:27017,cluster-shard-00-01.3tjnw.mongodb.net:27017,cluster-shard-00-02.3tjnw.mongodb.net:27017/<dbname>?ssl=true&replicaSet=atlas-9x4vfv-shard-0&authSource=admin&retryWrites=true&w=majority";
        public static string MongoDatabase = "Agency";
        internal static void ConnectToMongoService()
        {
            try
            {
                client = new MongoClient(MongoConnection);
                database = client.GetDatabase(MongoDatabase);
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}













