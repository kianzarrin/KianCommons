using System;
using System.IO;
using System.Reflection;

namespace experiments
{
    class Program
    {
        static void Main(string[] args)
        {
            using (Stream s = Assembly.GetExecutingAssembly().GetManifestResourceStream("kian.com")) {
                if (s == null)
                    Console.WriteLine("s is null");
                else
                    Console.WriteLine("s is not null");
                Console.WriteLine("inside using block");
            }
            Console.WriteLine("after using block");
        }
    }
}
