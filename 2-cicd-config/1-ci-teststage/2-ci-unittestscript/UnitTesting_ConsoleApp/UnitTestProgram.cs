using RockwellAutomation.FactoryTalkLogixEcho.Api.Client;
using RockwellAutomation.FactoryTalkLogixEcho.Api.Interfaces;
using System;
using System.Threading.Tasks;

Console.WriteLine("=== SAMPLE-STYLE DOWNLOAD TEST START ===");

// CHANGE THESE TWO VALUES
string acdFilePath = @"C:\CI-Pipeline-Files\test.ACD";
Guid controllerGuid = Guid.Parse("c8503cca-41e7-4a04-9352-5541488a7840");

// Use your Echo port explicitly
var serviceClient = ClientFactory.GetServiceApiClientV2("CI_Demo", 46520);

try
{
    Console.WriteLine($"ACD Path: {acdFilePath}");
    Console.WriteLine($"Controller GUID: {controllerGuid}");

    // Confirm controller exists
    var controller = await serviceClient.ReadController(controllerGuid);
    Console.WriteLine($"Controller found: {controller.ControllerName}");
    Console.WriteLine($"Controller ID: {controller.ControllerGuid}");

    // Send file and start download
    using (var fileHandle = await serviceClient.SendFile(acdFilePath))
    {
        Console.WriteLine("File sent to service.");
        Console.WriteLine("Starting download...");
        await serviceClient.Download(controller.ControllerGuid, fileHandle);
    }

    // Poll feedback
    DownloadFeedback downloadFeedback;
    do
    {
        downloadFeedback = await serviceClient.GetDownloadFeedback(controller.ControllerGuid);

        foreach (var message in downloadFeedback.Messages)
        {
            Console.WriteLine(message);
        }

        await Task.Delay(500);
    }
    while (downloadFeedback.State == OperationState.InProgress);

    Console.WriteLine($"Final state: {downloadFeedback.State}");

    if (downloadFeedback.State == OperationState.Done)
    {
        Console.WriteLine("=== DOWNLOAD SUCCEEDED ===");
        return 0;
    }
    else
    {
        Console.WriteLine("=== DOWNLOAD FAILED ===");
        return 1;
    }
}
catch (Exception ex)
{
    Console.WriteLine("=== ERROR ===");
    Console.WriteLine(ex.ToString());
    return 1;
}