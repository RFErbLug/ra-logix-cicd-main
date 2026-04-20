using System;
using System.Threading.Tasks;
using RockwellAutomation.FactoryTalkLogixEcho.Api.Client;
using RockwellAutomation.FactoryTalkLogixEcho.Api.Client.Models;
using RockwellAutomation.FactoryTalkLogixEcho.Api;

namespace UnitTesting_ConsoleApp
{
    class StartUnitTest
    {
        static async Task<int> Main(string[] args)
        {
            Console.WriteLine("=== CI ECHO TEST START ===");

            try
            {
                string acdPath = @"C:\CI-Pipeline-Files\test.ACD";

                // 🔑 AUTH (this is new and REQUIRED)
                var login = new FactoryTalkServicesPlatformLogin();
                string token = login.GetTokenForCurrentUser();

                // 🔌 CLIENT
                var serviceClient = ClientFactory.GetServiceApiClientV2("CI_Demo", token);

                Console.WriteLine("Creating chassis...");
                var chassis = await serviceClient.CreateChassis(new ChassisUpdate
                {
                    Name = "CI_Chassis",
                    Description = "CI Demo"
                });

                Console.WriteLine("Reading controller from ACD...");
                using var file = await serviceClient.SendFile(acdPath);
                var controller = await serviceClient.GetControllerInfoFromAcd(file);

                controller.ChassisGuid = chassis.ChassisGuid;

                Console.WriteLine("Creating controller...");
                var created = await serviceClient.CreateController(controller);

                string commPath = @"EmulateEthernet\" + created.IPConfigurationData.Address;

                Console.WriteLine("Controller Path: " + commPath);

                Console.WriteLine("=== CI ECHO TEST PASS ===");
                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine("=== CI ECHO TEST FAIL ===");
                Console.WriteLine(ex.ToString());
                return 1;
            }
        }
    }
}