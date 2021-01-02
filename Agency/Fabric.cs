using Agency.Models;
using Bogus;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Agency
{
    class Fabric
    {
        public static Faker faker = new Faker("ru");

        public static Random random = new Random();

        public static List<Applicant> applicants;
        public static List<Employer> employers;
        public static List<Task> tasks;
        public static List<Staffer> staffers;

        public static int emplIndex = 100;
        public async static void loadCollections()
        {
            MongoHelper.ConnectToMongoService();
            MongoHelper.employer_collection = MongoHelper.database.GetCollection<Models.Employer>("employers");
            MongoHelper.applicant_collection = MongoHelper.database.GetCollection<Models.Applicant>("applicants");
            MongoHelper.staffer_collection = MongoHelper.database.GetCollection<Models.Staffer>("staffers");
            MongoHelper.task_collection = MongoHelper.database.GetCollection<Models.Task>("tasks");

            var filter = Builders<Applicant>.Filter.Ne("_id", "");
            var result = await MongoHelper.applicant_collection.FindAsync(filter);
            applicants = result.ToList();

            var filter1 = Builders<Employer>.Filter.Ne("_id", "");
            var result1 = await MongoHelper.employer_collection.FindAsync(filter1);
            employers = result1.ToList();

            var filter2 = Builders<Task>.Filter.Ne("_id", "");
            var result2 = await MongoHelper.task_collection.FindAsync(filter2);
            tasks = result2.ToList();

            var filter3 = Builders<Staffer>.Filter.Ne("_id", "");
            var result3 = await MongoHelper.staffer_collection.FindAsync(filter3);
            staffers = result3.ToList();

            Console.WriteLine("Got all collections : applicant = " + applicants.Count + " vs employers = " + employers.Count
                + "\ntasks = " + tasks.Count + " vs staffers = " + staffers.Count);
        }
        public async static void generateMain()
        {
            var employers = new List<Employer>();
            var staffers = new List<Staffer>();
            var applicants = new List<Applicant>();

            for (int i = 0; i < 100; i++)
            {
                employers.Add(generateEmployer());
                applicants.Add(generateApplicant());
            }

            for (int i = 0; i < 25; i++)
            {
                staffers.Add(generateStaffer());
            }

            MongoHelper.ConnectToMongoService();
            MongoHelper.applicant_collection = MongoHelper.database.GetCollection<Models.Applicant>("applicants");
            MongoHelper.employer_collection = MongoHelper.database.GetCollection<Models.Employer>("employers");
            MongoHelper.staffer_collection = MongoHelper.database.GetCollection<Models.Staffer>("staffers");

            await MongoHelper.applicant_collection.InsertManyAsync(applicants);
            
            await MongoHelper.employer_collection.InsertManyAsync(employers);
            await MongoHelper.staffer_collection.InsertManyAsync(staffers);
            Console.WriteLine("Data loaded...");
        }
        public static void generateVacancies()
        {
            loadCollections();

            foreach (Employer e in employers)
            {
                var filter = Builders<Employer>.Filter.Eq("_id", e._id);

                var update = Builders<Employer>.Update
                    .Set("vacancies", getListVacancy(e));

                MongoHelper.employer_collection.UpdateOneAsync(filter, update);
            }
        }
        public async static void generateHistory()
        {
            //loadCollections();
            foreach (Applicant a in applicants)
            {
                var filter = Builders<Applicant>.Filter.Eq("_id", a._id);

                var update = Builders<Applicant>.Update
                    .Set("history", getListEvent(a))
                    .Set("views", getListEmployer());

                await MongoHelper.applicant_collection.UpdateOneAsync(filter, update);
            }
        }

        private static int p = 0;
        public static void generateTask()
        {
            

            var employersL = new HashSet<Employer>();
            var list = new List<Task>();
            for (int i = 0; i < 100; i += p)
            {
                var empl = employers[random.Next(employers.Count)];
                if (employersL.Contains(empl)) continue;
                var t = getTask(empl);
                p = t.Count;
                foreach (Task task in t)
                {
                    list.Add(task);
                }
            }
            MongoHelper.task_collection.InsertManyAsync(list);

        }
        public static List<Task> getTask(Employer empl)
        {
            var list = new List<Task>();
            var count = random.Next(3);
           
            for (int i = 0; i < count; i++)
            {
                if (i >= empl.vacancies.Count) break;
                list.Add(new Task
                {
                    employer = empl._id,
                    vacancy = empl.vacancies[i]._id,
                    price = random.Next(1, 50) * 1000,
                    date = getDate(new DateTime(2020, 11, 2)),
                    status = random.Next(2) == 0 ? "Актуальна" : "Завершена",
                    consultant = staffers[random.Next(staffers.Count)]._id
                });
            }
            return list;
        }
        public static List<Event> getListEvent(Applicant a)
        {
            MongoHelper.staffer_collection = MongoHelper.database.GetCollection<Models.Staffer>("staffers");
            var filter3 = Builders<Staffer>.Filter.Ne("_id", "");
            var result3 =  MongoHelper.staffer_collection.Find(filter3);
            staffers = result3.ToList();
            var list = new List<Event>();
            int count = random.Next(3);
            for (int i = 0; i < count; i++)
            {
                list.Add(getEvent(a));
            }
            return list;
        }
        public static Event getEvent(Applicant a)
        {
            var spec = random.Next(a.specialization.Count);
            var names = new string[] { "Собеседование", "Профориентация", "Рекомендация" };
            var results = new string[] { "отлично", "хорошо", "замечательно" };
            var name = names[random.Next(names.Length)];
            return new Event
            {
                specialization = a.specialization[spec],
                name = name,
                result = name == "Рекомендация" ? (object)employers[random.Next(employers.Count)]._id : name == "Профориентация" ? "Соискателю предложено рассмотреть..." : results[random.Next(results.Length)],
                date = getDate(new DateTime(2020, 11, 2)),
                consultant = staffers[random.Next(staffers.Count)]._id
            };
        }
        public static List<ObjectId> getListEmployer()
        {
            var list = new HashSet<ObjectId>();
            int count = random.Next(10);

            for (int i = 0; i <= count; i++)
            {
                int index = random.Next(employers.Count);
                list.Add(employers[index]._id);
            }

            return list.ToList<ObjectId>();
        }
        public static Employer generateEmployer()
        {
            return new Employer
            {
                name = Source.places[emplIndex++],
                tin = faker.Finance.Account(10),
                contacts = getContacts(),
                location = getLocation(),
                balance = random.Next(1, 100) * 1000,
                password = faker.Internet.Password()
            };
        }
        public static Staffer generateStaffer()
        {
            var experiences = new double[5] { 0, 1, 2, 3, 1.5 };
            int index = random.Next(experiences.Length);
            return new Staffer
            {
                name = faker.Name.FullName(),
                contacts = getContacts(),
                location = getLocation(),
                position = "Консультант",
                salary = 40000,
                experience = experiences[index]
            };
        }
        public static Applicant generateApplicant()
        {
            var age = random.Next(19, 50);
            return new Applicant
            {
                name = faker.Name.FullName(),
                age = age,
                password = faker.Internet.Password(),
                location = getLocation(),
                contacts = getContacts(),
                education = getListEducation(DateTime.Now.AddYears(18 - age)),
                experience = getListExperience(DateTime.Now.AddYears(18 - age)),
                specialization = getListSpecialization(),
            };
        }
        public static Vacancy getVacancy(Employer employer)
        {
            return new Vacancy
            {
                _id = GenerateRandomId(24),
                specialization = getSpecialization(),
                conditions = getConditions(),
                requirements = getRequirements(),
                location = employer.location,
                views = getViewsVacancy()
            };
        }
        public static List<Vacancy> getListVacancy(Employer employer)
        {
            var list = new List<Vacancy>();
            int count = random.Next(5);
            for (int i = 0; i < count; i++)
            {
                list.Add(getVacancy(employer));
            }
            return list;
        }
        public static List<ObjectId> getViewsVacancy()
        {
            int count = random.Next(15);
            var filter = Builders<Applicant>.Filter.Ne("_id", "");
            var result =  MongoHelper.applicant_collection.Find(filter);
            applicants = result.ToList();
            var list = new HashSet<ObjectId>();
            for (int i = 0; i < count; i++)
            {
                int index = random.Next(applicants.Count);
                list.Add(applicants[index]._id);
            }
            return list.ToList<ObjectId>();
        }
        public static Conditions getConditions()
        {
            var empl = new List<string> { "полный", "полный", "полный", "частичный", "контракт" };
            var sced = new List<string> { "полный", "гибкий", "сменный" };
            return new Conditions
            {
                salary = random.Next(15, 100) * 1000,
                emplType = empl[random.Next(empl.Count)],
                scedule = sced[random.Next(sced.Count)]
            };
        }
        public static Contacts getContacts()
        {
            return new Contacts
            {
                email = faker.Internet.Password()+"@example.ru",
                phone = faker.Phone.PhoneNumber(),
            };
        }
        public static Requirements getRequirements()
        {
            var experiences = new double[5] { 0, 1, 2, 3, 1.5 };
            int index = random.Next(experiences.Length);

            return new Requirements {
                experience = experiences[index],
                education = getEducationType()
            };
        }
        public static string getEducationType()
        {
            int index = random.Next(Source.eduType.Count);
            return Source.eduType.ElementAt(index);
        }
        public static Experience getExperience(DateTime start)
        {
            int gen = random.Next(4);
            DateTime end = start.AddYears(gen).CompareTo(DateTime.Now) > 0 ? DateTime.Now : start.AddYears(gen);
            int index = random.Next(Source.places.Count);

            return new Experience
            {
                place = Source.places[index],
                specialization = getSpecialization(),
                start = start,
                end = end
            };
        }
        public static List<Experience> getListExperience(DateTime start)
        {
            var list = new List<Experience>();
            int count = random.Next(3);
            for (int i = 0; i <= count; i++)
            {
                var edu = getExperience(start);
                start = getDate(edu.end);
                list.Add(edu);
                if (start.CompareTo(DateTime.Now) == 0) break;
            }
            return list;
        }
        public static Education getEducation(DateTime start)
        {
            int index = random.Next(Source.institutions.Count);
            string place = Source.institutions.ElementAt(index);
            string type = getEducationType();
            int gen = random.Next(1, 4);
            DateTime end = start.AddYears(gen).CompareTo(DateTime.Now) > 0 ? DateTime.Now : start.AddYears(gen);

            return new Education
            {
                type = type,
                place = place,
                specialization = getSpecialization(),
                start = start,
                end = end
            };
        }
        public static List<Education> getListEducation(DateTime start)
        {
            var list = new List<Education>();
            int count = random.Next(3);
            for (int i = 0; i <= count; i++)
            {
                var edu = getEducation(start);
                start = getDate(edu.end);
                list.Add(edu);
                if (start.CompareTo(DateTime.Now) == 0) break;
            }
            return list;
        }

        public static List<Specialization> specs = convertSpecializations();
        public static List<Specialization> convertSpecializations()
        {
            var list = new List<Specialization>();

            foreach(string key in Source.specializations.Keys)
            {
                var value = Source.specializations[key];
                foreach (string element in value)
                {
                    list.Add(new Specialization
                    {
                        field = key,
                        name = element
                    });
                }
            }
            return list;
        }
        public static Specialization getSpecialization()
        {
            int index = random.Next(specs.Count);
            return specs[index];
        }
        public static List<Specialization> getListSpecialization()
        {
            var list = new HashSet<Specialization>();
            int count = random.Next(3);
            for (int i = 0; i <= count; i++)
            {
                var spec = getSpecialization();
                list.Add(spec);
            }
            return list.ToList<Specialization>();
        }    
        public static DateTime getDate()
        {
            return getDate(new DateTime(2005, 1, 1));
        }
        public static DateTime getDate(DateTime start )
        {
            int range = (DateTime.Today - start).Days;
            return start.AddDays(random.Next(range));
        }
        public static Location getLocation()
        {
            return Source.location[random.Next(2)];
        }
        public static object GenerateRandomId(int v)
        {
            string strarray = "abcdefjhijklmnopqrstuvwxyz123456789";
            return new string(Enumerable.Repeat(strarray, v).Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}
