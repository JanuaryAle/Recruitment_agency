using Agency.Models;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agency
{
    class Queries
    {
        public static void getCollections()
        {
            MongoHelper.ConnectToMongoService();
            MongoHelper.applicant_collection = MongoHelper.database.GetCollection<Models.Applicant>("applicants");
            MongoHelper.employer_collection = MongoHelper.database.GetCollection<Models.Employer>("employers");
            MongoHelper.task_collection = MongoHelper.database.GetCollection<Models.Task>("tasks");
            MongoHelper.staffer_collection = MongoHelper.database.GetCollection<Models.Staffer>("staffers");
        }

        //	Формирование топ 3 компаний, просмотревших наибольшее количество резюме соискателей.

        public static void top()
        {
            getCollections();

            var projection = Builders<Applicant>.Projection.Include("views").Exclude("_id");
            var result = MongoHelper.applicant_collection.Aggregate()
                .Project(projection)
                .Unwind("views")
                .Group(new BsonDocument { { "_id", "$views" }, { "count", new BsonDocument("$sum", 1) } })
                .Group(new BsonDocument { { "_id", "$count" }, { "employers", new BsonDocument("$push","$_id" )} })
                .SortByDescending(r => r["_id"])
                .Limit(3)
                .ToList();

            var place = 1;
            foreach (BsonDocument document in result)
            {
                Console.WriteLine(place++ + " место:");
                List<ObjectId> employers = BsonSerializer.Deserialize<List<ObjectId>>(document[1].ToJson());
                foreach (ObjectId s in employers)
                {
                    var employer_filter = Builders<Employer>.Filter.Eq("_id", s);
                    var employer = MongoHelper.employer_collection.Find(employer_filter).FirstOrDefault();
                    Console.WriteLine("\t" + employer.name);
                }
            }
        }

        //	Вывод средней заработной платы востребованных специалистов в различных сферах деятельности.
        public async static void avgFieldSalary()
        {
            getCollections();

            var projection = Builders<Employer>.Projection.Exclude("_id").Include("vacancies");
            var result = MongoHelper.employer_collection.Aggregate()
                .Project(projection)
                .Unwind("vacancies")
                .Group(new BsonDocument { { "_id", "$vacancies.specialization.field" }
                    , { "salary", new BsonDocument("$avg", "$vacancies.conditions.salary")} })
                .ToList();
            foreach (BsonDocument bd in result)
            {
                Console.WriteLine(bd);
            }
        }

        //	Вывод количества вакансий по каждой сфере деятельности.
        public async static void countFieldVacancy()
        {
            getCollections();

            var result = MongoHelper.employer_collection.Aggregate()
                .Unwind("vacancies")
                .Group(new BsonDocument { { "_id", "$vacancies.specialization.field" }
                    , { "count", new BsonDocument("$sum", 1) }})
                .ToList();

            foreach (BsonDocument bd in result)
            {
                Console.WriteLine(bd);
            }
        }

        //      Отчет об активности консультантов
        public async static void activeConsultants()
        {
            getCollections();

            var result = MongoHelper.task_collection.Aggregate()
                .Group(new BsonDocument { { "_id", "$consultant" }
                    , { "count", new BsonDocument("$sum", 1) }})
                .SortByDescending(r => r["count"])
                .ToList();

            foreach (BsonDocument document in result)
            {
                ObjectId id = BsonSerializer.Deserialize<ObjectId>(document[0].ToJson());
                var consultant = MongoHelper.staffer_collection
                    .Aggregate().Match(x => x._id == id).FirstOrDefault();
                Console.WriteLine("Имя: " + consultant.name + "; число контрактов: " + document[1]);
            }
        }

        //   	Вывод сколько компания заработала на контрактах за декабрь.
        public async static void revenue()
        {
            getCollections();

            var result = MongoHelper.task_collection.Aggregate()
                 .Match(r => r.status == "Завершена")
                 .Match(r => r.date.CompareTo(new DateTime(2020,12,1)) >= 0)
                 .Group(r => r.status, g => new
                 {
                     Key = g.Key,
                     Sum = g.Sum(x => x.price),
                 })
                 .FirstOrDefault();

            Console.WriteLine("За последний месяц компания заработала на контрактах " + result.Sum + " рублей.");


        }

        //      Создание
        public static void createApplicant()
        {
            getCollections();
            var age = 30;
            var applicant = new Applicant
            {
                name = "Павел Чехов",
                age = age,
                password = Fabric.faker.Internet.Password(),
                location = Fabric.getLocation(),
                contacts = Fabric.getContacts(),
                education = Fabric.getListEducation(DateTime.Now.AddYears(18 - age)),
                experience = Fabric.getListExperience(DateTime.Now.AddYears(18 - age)),
                specialization = Fabric.getListSpecialization(),
                views = Fabric.getViewsVacancy()
            };

            MongoHelper.applicant_collection.InsertOne(applicant);
        }

        //   	Поиск вакансии по фильтрам.
        public async static void searchVacancy()
        {
            getCollections();
            var salaryStart = 40000;
            var salaryEnd = 60000;
            var field = "IT";

            var projection = Builders<Applicant>.Projection.Include("vacancies");
            var aggragation = MongoHelper.employer_collection.Aggregate()
                .Unwind("vacancies")
                .Match(new BsonDocument { { "vacancies.specialization.field", field },
                    { "vacancies.conditions.salary", new BsonDocument("$gt", salaryStart) }})
                .Match(new BsonDocument { { "vacancies.conditions.salary", new BsonDocument("$lt", salaryEnd) } })
                .ToList();

            foreach (BsonDocument document in aggragation)
            {
                Vacancy vacancy = BsonSerializer.Deserialize<Vacancy>(document["vacancies"].ToJson());
                Console.WriteLine("Имя работодателя: " + document["name"]
                                   + "\nid Вакансии: " + vacancy._id
                                   + "\nОбласть деятельности: " + vacancy.specialization.field
                                   + "\nЗаработная плата: " + vacancy.conditions.salary + "\n\n" );
            }

        }

        //      Изменение
        public static void updateApplicant(string id)
        {
            getCollections();

            var filter = Builders<Applicant>.Filter.Eq("_id", new ObjectId(id));
            var applicant = MongoHelper.applicant_collection.Find(filter).FirstOrDefault();

            var update = Builders<Applicant>.Update
                .Set("history", Fabric.getListEvent(applicant))
                .Set("age", 25)
                .Set("contacts.email", "pavel_chekhov@example.ru");

            MongoHelper.applicant_collection.UpdateOne(filter, update);
        }

        //      Идентификация пользователей
        public static void indentify(string id)
        {
            getCollections();

            var filter = Builders<Applicant>.Filter.Eq("_id", new ObjectId(id));
            var result = MongoHelper.applicant_collection.Find(filter).FirstOrDefault();
            if (result == null)
            {
                Console.WriteLine("Пользователь с данным id не найден");
            }else
            {
                Console.WriteLine("Ваше имя: " + result.name);
            }
        }

        //	Вывод средней заработной платы, которую предлагают работодатели по вакансиям.
        public static void eighthQuery()
        {
            getCollections();

            var result = MongoHelper.employer_collection.Aggregate()
                .Unwind("vacancies")
                .Group(new BsonDocument { { "_id", "$name" }
                    , {"salary", new BsonDocument("$avg", "$vacancies.conditions.salary") }})
                .ToList();

            foreach (BsonDocument bd in result)
            {
                Console.WriteLine(bd);
            }
        }

        //      Вывод направленных в компанию соискателей.
        public static void fourteenthQuery()
        {
            getCollections();

            var result = MongoHelper.applicant_collection.Aggregate()
                .Unwind("history")
                .Match(new BsonDocument { { "history.name", "Рекомендация" } })
                .Group(new BsonDocument { { "_id", "$_id" } })
                .ToList();

            foreach (BsonDocument document in result)
            {
                ObjectId id = BsonSerializer.Deserialize<ObjectId>(document[0].ToJson());
                var applicant = MongoHelper.applicant_collection.Aggregate().Match(x => x._id == id).FirstOrDefault();
                Console.WriteLine(applicant.name);
            }
        }
    }
}

/*
  { "$project" : {
    "vacancies" : "vacancies"
  } },
  { "$unwind" : "$vacancies" },
  { "$group" : {
    "name" : "$vacancies.specialization.field",
    "salary" : { "$avg" : "$vacancies.conditions.salary"}
     }
  }*/
