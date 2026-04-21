using RockwellAutomation.FactoryTalkLogixEcho.Api.Client;
using RockwellAutomation.FactoryTalkLogixEcho.Api.Interfaces;

Console.WriteLine("=== CI ECHO CREATE + DOWNLOAD TEST START ===");

// Create client (async API)
var serviceClient = ClientFactory.GetServiceApiClientV2("CI_Demo", 50051);

// 1. Get chassis
var chassis = (await serviceClient.ListChassis()).First();

Console.WriteLine($"Using chassis: {chassis.Name}");

// 2. Get firmware
var firmwareGuid = (await serviceClient.ListFirmwarePackages()).First().Uuid;

// 3. Get available slot
var slots = await serviceClient.ListAvailableSlotNumbers(chassis.ChassisGuid, null, false);
var slot = slots.First();

// 4. Create controller
var controllerUpdate = new ControllerUpdate
{
    FirmwarePackageGuid = firmwareGuid,
    Name = "CI_Controller",
    Description = "Created by CI",
    ChassisGuid = chassis.ChassisGuid,
    Slot = slot,
    IPConfigurationData = new IP4ConfigurationData
    {
        Address = System.Net.IPAddress.Parse("127.0.0.1"),
        Netmask = System.Net.IPAddress.Parse("255.255.255.0")
    },
    KeySwitchPosition = KeySwitchPosition.Remote,
    IsEnabled = true,
    IsSdCardAttached = false,
    ProjectPath = @"C:\CI-Pipeline-Files\test.ACD",
    HasPartner = false
};

var controller = await serviceClient.CreateController(controllerUpdate);

Console.WriteLine($"Created controller: {controller.Name}");

// 5. Download project
string acdPath = @"C:\CI-Pipeline-Files\3-generatedfiles\CI_Project.ACD";

using (var fileHandle = await serviceClient.SendFile(acdPath))
{
    await serviceClient.Download(controller.ControllerGuid, fileHandle);
}

// 6. Monitor download
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

Console.WriteLine("Download complete");

// 7. Verify file exists
if (!File.Exists(acdPath))
{
    throw new Exception("ACD file not found");
}

Console.WriteLine("File verified");

Console.WriteLine("=== CI ECHO TEST PASS ===");