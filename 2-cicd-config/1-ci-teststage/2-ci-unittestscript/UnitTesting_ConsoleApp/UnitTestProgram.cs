using RockwellAutomation.FactoryTalkLogixEcho.Api.Client;
using RockwellAutomation.FactoryTalkLogixEcho.Api.Interfaces;

Console.WriteLine("=== ACD → CONTROLLER CREATION TEST START ===");

// Create client
var serviceClient = ClientFactory.GetServiceApiClientV2("CI_Demo", 46520);

// Path to your ACD
string acdPath = @"C:\CI-Pipeline-Files\test.ACD";

// 1. Get chassis (we KNOW this exists from earlier)
var chassis = (await serviceClient.ListChassis()).First();
Console.WriteLine($"Using chassis: {chassis.Name}");

// 2. Send ACD to Echo
using (var fileHandle = await serviceClient.SendFile(acdPath))
{
    // 3. Extract controller config FROM ACD (this is the key)
    var controllerUpdate = await serviceClient.GetControllerInfoFromAcd(fileHandle);

    Console.WriteLine("\n--- FROM ACD ---");
    Console.WriteLine($"Name: {controllerUpdate.Name}");
    Console.WriteLine($"Slot (ACD): {controllerUpdate.Slot}");
    Console.WriteLine($"HasPartner: {controllerUpdate.HasPartner}");
    Console.WriteLine($"Firmware GUID: {controllerUpdate.FirmwarePackageGuid}");

    // 4. Attach to chassis (THIS is the only thing we override)
    controllerUpdate.ChassisGuid = chassis.ChassisGuid;

    Console.WriteLine("\n--- FINAL CONFIG ---");
    Console.WriteLine($"ChassisGuid: {controllerUpdate.ChassisGuid}");
    Console.WriteLine($"Slot: {controllerUpdate.Slot}");

    // 5. Create controller
    var controller = await serviceClient.CreateController(controllerUpdate);

    Console.WriteLine($"\nCreated controller: {controller.ControllerGuid}");
}

Console.WriteLine("\n=== TEST COMPLETE ===");