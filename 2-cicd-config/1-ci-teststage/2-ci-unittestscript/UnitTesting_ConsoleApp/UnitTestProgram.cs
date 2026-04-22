using RockwellAutomation.FactoryTalkLogixEcho.Api.Client;
using System;
using System.Linq;

Console.WriteLine("=== DELETE ALL CONTROLLERS START ===");

var serviceClient = ClientFactory.GetServiceApiClientV2("DeleteAllControllers", 46520);

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
            continue;
        }

        foreach (var controller in controllers)
        {
            Console.WriteLine($"  Deleting controller: {controller.ControllerName}");
            Console.WriteLine($"  ControllerGuid: {controller.ControllerGuid}");

            await serviceClient.DeleteController(controller.ControllerGuid);

            Console.WriteLine("  Deleted.");
        }
    }

    Console.WriteLine("=== DELETE ALL CONTROLLERS COMPLETE ===");
}
catch (Exception ex)
{
    Console.WriteLine("=== ERROR ===");
    Console.WriteLine(ex.ToString());
}