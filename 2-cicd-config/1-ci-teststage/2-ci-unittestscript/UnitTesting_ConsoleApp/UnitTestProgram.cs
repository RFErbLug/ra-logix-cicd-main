using RockwellAutomation.FactoryTalkLogixEcho.Api.Client;
using RockwellAutomation.FactoryTalkLogixEcho.Api.Interfaces;

Console.WriteLine("=== ECHO ACD INSPECTION START ===");

// Create client
var serviceClient = ClientFactory.GetServiceApiClientV2("CI_Demo", 46520);

// Path to your ACD
string acdPath = @"C:\CI-Pipeline-Files\test.ACD";

Console.WriteLine($"Loading ACD: {acdPath}");

// Send file to Echo
using (var fileHandle = await serviceClient.SendFile(acdPath))
{
    Console.WriteLine("\n--- EXTRACTING CONTROLLER INFO FROM ACD ---");

    var controllerUpdate = await serviceClient.GetControllerInfoFromAcd(fileHandle);

    Console.WriteLine("\n--- CONTROLLER UPDATE FROM ACD ---");

    Console.WriteLine($"Name: {controllerUpdate.Name}");
    Console.WriteLine($"Description: {controllerUpdate.Description}");
    Console.WriteLine($"ChassisGuid: {controllerUpdate.ChassisGuid}");
    Console.WriteLine($"Slot: {controllerUpdate.Slot}");
    Console.WriteLine($"HasPartner: {controllerUpdate.HasPartner}");
    Console.WriteLine($"FirmwarePackageGuid: {controllerUpdate.FirmwarePackageGuid}");

    // Optional: dump anything else useful (safe introspection)
    Console.WriteLine("\n--- RAW OBJECT DUMP (for anything hidden) ---");
    Console.WriteLine(controllerUpdate.ToString());
}

Console.WriteLine("\n=== ECHO ACD INSPECTION END ===");