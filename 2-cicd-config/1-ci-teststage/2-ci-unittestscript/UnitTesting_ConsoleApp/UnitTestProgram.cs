using RockwellAutomation.FactoryTalkLogixEcho.Api.Client;
using RockwellAutomation.FactoryTalkLogixEcho.Api.Interfaces;
using System;
using System.Linq;
using System.Threading.Tasks;

Console.WriteLine("=== ACD -> CONTROLLER -> DOWNLOAD TEST START ===");

string acdPath = @"C:\CI-Pipeline-Files\test.ACD";
var serviceClient = ClientFactory.GetServiceApiClientV2("CI_Demo", 46520);

try
{
    // 1. Get chassis
    var chassis = (await serviceClient.ListChassis()).First();
    Console.WriteLine($"Using chassis: {chassis.Name}");
    Console.WriteLine($"ChassisGuid: {chassis.ChassisGuid}");

    // 2. Share ACD and read metadata from it
    using var acdFileHandle = await serviceClient.SendFile(acdPath);
    var controllerUpdate = await serviceClient.GetControllerInfoFromAcd(acdFileHandle);

    Console.WriteLine();
    Console.WriteLine("--- FROM ACD ---");
    Console.WriteLine($"Name: {controllerUpdate.Name}");
    Console.WriteLine($"Slot (ACD): {controllerUpdate.Slot}");
    Console.WriteLine($"HasPartner: {controllerUpdate.HasPartner}");
    Console.WriteLine($"Firmware GUID: {controllerUpdate.FirmwarePackageGuid}");

    // 3. Find available slots in target chassis
    var availableSlots = await serviceClient.ListAvailableSlotNumbers(
        chassis.ChassisGuid,
        null,
        controllerUpdate.HasPartner
    );

    Console.WriteLine();
    Console.WriteLine("--- AVAILABLE SLOTS ---");
    foreach (var slot in availableSlots)
    {
        Console.WriteLine($"Slot: {slot}");
    }

    if (!availableSlots.Any())
    {
        throw new Exception("No available slots found in chassis.");
    }

    // 4. Pick slot
    uint finalSlot;
    if (availableSlots.Contains((int)controllerUpdate.Slot))
    {
        finalSlot = controllerUpdate.Slot;
        Console.WriteLine($"\nUsing ACD slot: {finalSlot}");
    }
    else
    {
        finalSlot = (uint)availableSlots.First();
        Console.WriteLine($"\nACD slot occupied -> switching to slot: {finalSlot}");
    }

    controllerUpdate.ChassisGuid = chassis.ChassisGuid;
    controllerUpdate.Slot = finalSlot;

    Console.WriteLine();
    Console.WriteLine("--- FINAL CONFIG ---");
    Console.WriteLine($"ChassisGuid: {controllerUpdate.ChassisGuid}");
    Console.WriteLine($"Assigned Slot: {controllerUpdate.Slot}");

    // 5. Create controller
    var controller = await serviceClient.CreateController(controllerUpdate);

    Console.WriteLine();
    Console.WriteLine("--- CREATED CONTROLLER ---");
    Console.WriteLine($"ControllerName: {controller.ControllerName}");
    Console.WriteLine($"ControllerGuid: {controller.ControllerGuid}");

    // 6. Send file again and download
    Console.WriteLine();
    Console.WriteLine("--- STARTING DOWNLOAD ---");

    using (var downloadHandle = await serviceClient.SendFile(acdPath))
    {
        await serviceClient.Download(controller.ControllerGuid, downloadHandle);
    }

    // 7. Poll feedback
    DownloadFeedback feedback;
    do
    {
        feedback = await serviceClient.GetDownloadFeedback(controller.ControllerGuid);

        foreach (var msg in feedback.Messages)
        {
            Console.WriteLine(msg);
        }

        await Task.Delay(500);
    }
    while (feedback.State == OperationState.InProgress);

    Console.WriteLine();
    Console.WriteLine($"Final download state: {feedback.State}");

    if (feedback.State != OperationState.Done)
    {
        throw new Exception($"Download failed: {feedback.State}");
    }

    Console.WriteLine("=== DOWNLOAD COMPLETE ===");
}
catch (Exception ex)
{
    Console.WriteLine("=== ERROR ===");
    Console.WriteLine(ex.ToString());
    return 1;
}

return 0;