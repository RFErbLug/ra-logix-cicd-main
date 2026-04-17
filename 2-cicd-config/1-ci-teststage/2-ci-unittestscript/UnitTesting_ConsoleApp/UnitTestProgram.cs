using System;

namespace UnitTesting_ConsoleApp
{
    class Program
    {
        static int Main(string[] args)
        {
            Console.WriteLine("=== CI SMOKE TEST START ===");

            try
            {
                Console.WriteLine("Hello from CI pipeline.");
                Console.WriteLine("Args count: " + args.Length);

                Console.WriteLine("=== CI SMOKE TEST PASS ===");
                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine("=== CI SMOKE TEST FAIL ===");
                Console.WriteLine(ex.Message);
                return 1;
            }
        }
    }
}