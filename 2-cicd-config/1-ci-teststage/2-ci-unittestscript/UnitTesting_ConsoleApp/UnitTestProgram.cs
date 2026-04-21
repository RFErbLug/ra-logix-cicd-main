using RockwellAutomation.FactoryTalkLogixEcho.Api.Client;
using RockwellAutomation.FactoryTalkLogixEcho.Api.Interfaces;

Console.WriteLine("=== ECHO DIAGNOSTIC START ===");

// Create client
var serviceClient = ClientFactory.GetServiceApiClientV2("CI_Demo", 46520);

// --------------------
// A — Firmware list
// --------------------
Console.WriteLine("\n--- FIRMWARE PACKAGES ---");

var firmwares = await serviceClient.ListFirmwarePackages();

foreach (var fw in firmwares)
{
    Console.WriteLine($"{fw.Name} | {fw.Uuid}");
}

// --------------------
// B — Chassis list
// --------------------
Console.WriteLine("\n--- CHASSIS LIST ---");

var chassisList = await serviceClient.ListChassis();

foreach (var ch in chassisList)
{
    Console.WriteLine($"{ch.Name} | {ch.ChassisGuid}");
}

// --------------------
// Pick first chassis
// --------------------
var chassisOne = chassisList.First();

Console.WriteLine($"\nUsing chassis: {chassisOne.Name}");

// --------------------
// C — Slot list
// --------------------
Console.WriteLine("\n--- AVAILABLE SLOTS ---");

var slots = await serviceClient.ListAvailableSlotNumbers(
    chassisOne.ChassisGuid,
    null,
    false
);

foreach (var s in slots)
{
    Console.WriteLine($"Slot: {s}");
}

Console.WriteLine("\n=== ECHO DIAGNOSTIC END ===");