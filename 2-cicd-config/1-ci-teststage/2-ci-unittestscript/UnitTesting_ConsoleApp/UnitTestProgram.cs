using System;
using System.Linq;
using System.Threading.Tasks;

using RockwellAutomation.FactoryTalkLogixEcho.Api.Client;
using RockwellAutomation.FactoryTalkLogixEcho.Api.Interfaces;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("=== CI ECHO TEST START ===");

        // Step 1: Create client
        var serviceClient = ClientFactory.GetServiceApiClientV2("CI Demo");

        // Step 2: List chassis
        var chassisList = await serviceClient.ListChassis();
        var chassis = chassisList.First();

        Console.WriteLine($"Found chassis: {chassis.Name}");

        // Step 3: List controllers
        var controllers = await serviceClient.ListControllers(chassis.ChassisGuid);

        Console.WriteLine($"Controllers found: {controllers.Count()}");

        Console.WriteLine("=== CI ECHO TEST PASS ===");
    }
}