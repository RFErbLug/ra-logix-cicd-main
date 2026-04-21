using RockwellAutomation.FactoryTalkLogixEcho.Api.Client;
using System;
using System.Linq;

var serviceClient = ClientFactory.GetServiceApiClientV2("ListControllers", 46520);

try
{
    var chassisList = await serviceClient.ListChassis();

    foreach (var chassis in chassisList)
    {
        Console.WriteLine($"Chassis: {chassis.Name}");
        Console.WriteLine($"ChassisGuid: {chassis.ChassisGuid}");

        var controllers = await serviceClient.ListControllers(chassis.ChassisGuid);

        if (!controllers.Any())
        {
            Console.WriteLine("  No controllers found.");
        }
        else
        {
            foreach (var controller in controllers)
            {
                Console.WriteLine($"  ControllerName: {controller.ControllerName}");
                Console.WriteLine($"  ControllerGuid: {controller.ControllerGuid}");
            }
        }

        Console.WriteLine();
    }
}
catch (Exception ex)
{
    Console.WriteLine(ex.ToString());
}