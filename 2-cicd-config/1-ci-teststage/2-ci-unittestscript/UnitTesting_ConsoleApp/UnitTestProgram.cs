using RockwellAutomation.FactoryTalkLogixEcho.Api.Client;
using RockwellAutomation.FactoryTalkLogixEcho.Api.Interfaces;
using RockwellAutomation.FactoryTalkLogixEcho.Api;

Console.WriteLine("=== ACD → CONTROLLER → DOWNLOAD TEST START ===");

var serviceClient = ClientFactory.GetServiceApiClientV2("CI_Demo", 46520);

// ADD THIS LINE ONLY
var ftLogin = new FactoryTalkServicesPlatformLogin();
string userToken = ftLogin.GetTokenForUser("desktop-c2jqv6k\\DevOps", "Rockwell1");

Console.WriteLine("Token acquired successfully.");

serviceClient.LoginFactoryTalkUser(userToken);

Console.WriteLine("Token login applied successfully.");


string acdPath = @"C:\CI-Pipeline-Files\test.ACD";

// 1. Get chassis
var chassis = (await serviceClient.ListChassis()).First();
Console.WriteLine($"Using chassis: {chassis.Name}");

// 2. Get available slots
var availableSlots = await serviceClient.ListAvailableSlotNumbers(
    chassis.ChassisGuid,
    null,
    false
);

// PRINT AVAILABLE SLOTS
Console.WriteLine("\n--- AVAILABLE SLOTS ---");
foreach (var s in availableSlots)
{
    Console.WriteLine($"Slot: {s}");
}

// 3. Send ACD
using (var fileHandle = await serviceClient.SendFile(acdPath))
{
    var controllerUpdate = await serviceClient.GetControllerInfoFromAcd(fileHandle);

    Console.WriteLine("\n--- FROM ACD ---");
    Console.WriteLine($"Name: {controllerUpdate.Name}");
    Console.WriteLine($"Slot (ACD): {controllerUpdate.Slot}");
    Console.WriteLine($"HasPartner: {controllerUpdate.HasPartner}");
    Console.WriteLine($"Firmware GUID: {controllerUpdate.FirmwarePackageGuid}");

    // 4. Slot decision logic (instrumented)
    uint finalSlot;

    if (availableSlots.Contains((int)controllerUpdate.Slot))
    {
        finalSlot = controllerUpdate.Slot;
        Console.WriteLine($"\nUsing ACD slot: {finalSlot}");
    }
    else
    {
        finalSlot = (uint)availableSlots.First();
        Console.WriteLine($"\nACD slot occupied → switching to slot: {finalSlot}");
    }

    // APPLY SLOT
    controllerUpdate.ChassisGuid = chassis.ChassisGuid;
    controllerUpdate.Slot = finalSlot;

    Console.WriteLine("\n--- FINAL CONFIG ---");
    Console.WriteLine($"ChassisGuid: {controllerUpdate.ChassisGuid}");
    Console.WriteLine($"Assigned Slot: {controllerUpdate.Slot}");

    // 5. Create controller
    var controller = await serviceClient.CreateController(controllerUpdate);
    Console.WriteLine($"\nCreated controller: {controller.ControllerGuid}");

    // 6. Download
    Console.WriteLine("\n--- STARTING DOWNLOAD ---");

    using (var downloadHandle = await serviceClient.SendFile(acdPath))
    {
        await serviceClient.Download(controller.ControllerGuid, downloadHandle);
    }

    // 7. Monitor
    DownloadFeedback feedback;

    do
    {
        feedback = await serviceClient.GetDownloadFeedback(controller.ControllerGuid);

        foreach (var msg in feedback.Messages)
        {
            Console.WriteLine(msg);
        }

    } while (feedback.State == OperationState.InProgress);

    if (feedback.State != OperationState.Done)
    {
        throw new Exception($"Download failed: {feedback.State}");
    }

    Console.WriteLine("\nDownload complete");
}

Console.WriteLine("\n=== TEST COMPLETE ===");