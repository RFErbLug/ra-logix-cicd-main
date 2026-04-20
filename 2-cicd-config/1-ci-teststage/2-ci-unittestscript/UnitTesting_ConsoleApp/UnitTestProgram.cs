using System;

namespace UnitTesting_ConsoleApp
{
    class StartUnitTest
    {
        static int Main(string[] args)
        {
            Console.WriteLine("=== CI SMOKE TEST START ===");

            try
            {
                Console.WriteLine("Starting Echo test...");

                var client = new RockwellAutomation.FactoryTalkLogixEcho.Api.Client.EchoClient();

                Console.WriteLine("Echo client created.");

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