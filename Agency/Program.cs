using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agency
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Program started...");
            Queries.searchVacancy();
            Console.ReadLine();
        }
    }
}
