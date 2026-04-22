using LogixEcho_ClassLibrary;
using RockwellAutomation.FactoryTalkLogixEcho.Api.Client;
using RockwellAutomation.FactoryTalkLogixEcho.Api.Interfaces;
using RockwellAutomation.LogixDesigner;
using System.Globalization;

namespace UnitTesting_ConsoleApp
{
    internal class Program
    {
        static async Task<int> Main(string[] args)
        {
            string acdFilePath = @"C:\CI-Pipeline-Files\BoilerDemo.ACD";
            string chassisName = "DemoChassis";
            bool cleanupOnExit = true;

            Console.WriteLine("=== BOILER DEMO TEST START ===");

            LogixProject? logixProject = null;

            try
            {
                //
                // PART 1: ECHO SETUP
                //
                Console.WriteLine("\n--- ECHO SETUP ---");

                var serviceClient = ClientFactory.GetServiceApiClientV2("CI_Demo", 46520);
                serviceClient.Culture = new CultureInfo("en-US");

                var chassisList = (await serviceClient.ListChassis()).ToList();
                ChassisData chassis;

                var existingChassis = chassisList.FirstOrDefault(c => c.Name == chassisName);
                if (existingChassis == null)
                {
                    Console.WriteLine($"Creating chassis '{chassisName}'...");
                    var chassisUpdate = new ChassisUpdate
                    {
                        Name = chassisName,
                        Description = "Boiler demo chassis"
                    };
                    chassis = await serviceClient.CreateChassis(chassisUpdate);
                }
                else
                {
                    Console.WriteLine($"Using existing chassis '{chassisName}'...");
                    chassis = existingChassis;
                }

                Console.WriteLine($"ChassisGuid: {chassis.ChassisGuid}");

                using var fileHandle = await serviceClient.SendFile(acdFilePath);
                ControllerUpdate controllerUpdate = await serviceClient.GetControllerInfoFromAcd(fileHandle);

                Console.WriteLine("\n--- FROM ACD ---");
                Console.WriteLine($"Name: {controllerUpdate.Name}");
                Console.WriteLine($"Slot (ACD): {controllerUpdate.Slot}");
                Console.WriteLine($"HasPartner: {controllerUpdate.HasPartner}");
                Console.WriteLine($"Firmware GUID: {controllerUpdate.FirmwarePackageGuid}");
                Console.WriteLine($"IP1: {controllerUpdate.IPConfigurationData?.Address}");
                Console.WriteLine($"Netmask1: {controllerUpdate.IPConfigurationData?.Netmask}");

                var existingControllers = (await serviceClient.ListControllers(chassis.ChassisGuid)).ToList();
                ControllerData controllerData;

                var existingController = existingControllers.FirstOrDefault(c => c.ControllerName == controllerUpdate.Name);
                if (existingController != null)
                {
                    Console.WriteLine($"\nUsing existing controller '{existingController.ControllerName}'...");
                    controllerData = existingController;
                }
                else
                {
                    var availableSlots = await serviceClient.ListAvailableSlotNumbers(
                        chassis.ChassisGuid,
                        null,
                        controllerUpdate.HasPartner
                    );

                    Console.WriteLine("\n--- AVAILABLE SLOTS ---");
                    foreach (var slot in availableSlots)
                    {
                        Console.WriteLine($"Slot: {slot}");
                    }

                    if (!availableSlots.Any())
                    {
                        throw new Exception("No available slots found in target chassis.");
                    }

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
                    controllerUpdate.Description = "Boiler demo controller";

                    Console.WriteLine("\n--- FINAL CONTROLLER CONFIG ---");
                    Console.WriteLine($"ChassisGuid: {controllerUpdate.ChassisGuid}");
                    Console.WriteLine($"Assigned Slot: {controllerUpdate.Slot}");

                    controllerData = await serviceClient.CreateController(controllerUpdate);

                    Console.WriteLine("\n--- CREATED CONTROLLER ---");
                    Console.WriteLine($"ControllerName: {controllerData.ControllerName}");
                    Console.WriteLine($"ControllerGuid: {controllerData.ControllerGuid}");
                    Console.WriteLine($"IP1: {controllerData.IPConfigurationData?.Address}");
                }

                if (controllerData.IPConfigurationData?.Address == null)
                {
                    throw new Exception("Controller IP address is null. Cannot build communication path.");
                }

                string commPath = @"EmulateEthernet\" + controllerData.IPConfigurationData.Address;
                Console.WriteLine($"\nCOMMPATH = {commPath}");

                //
                // PART 2: LOGIX SDK ONLINE / DOWNLOAD / RUN
                //
                Console.WriteLine("\n--- LOGIX SDK SETUP ---");

                logixProject = await LogixProject.OpenLogixProjectAsync(acdFilePath);
                await logixProject.SetCommunicationsPathAsync(commPath);

                Console.WriteLine("Changing controller to PROGRAM...");
                await ChangeControllerMode_Async(commPath, "PROGRAM", logixProject);

                Console.WriteLine("Downloading ACD...");
                await DownloadProject_Async(commPath, logixProject);

                Console.WriteLine("Changing controller to RUN...");
                await ChangeControllerMode_Async(commPath, "RUN", logixProject);

                //
                // PART 3: TANK1TEST PROBE
                //
                Console.WriteLine("\n--- BOILER LOGIC TESTS ---");

                string tank1TestPath = CreateTagPathFromName("Tank1Test");

                Console.WriteLine($"Trying tag path: {tank1TestPath}");

                try
                {
                    Console.WriteLine("Trying BOOL write/read...");
                    await logixProject.SetTagValueBOOLAsync(tank1TestPath, LogixProject.OperationMode.Online, true);
                    await Task.Delay(250);
                    bool boolVal = await logixProject.GetTagValueBOOLAsync(tank1TestPath, LogixProject.OperationMode.Online);
                    Console.WriteLine($"SUCCESS BOOL: Tank1Test = {boolVal}");
                }
                catch (Exception boolEx)
                {
                    Console.WriteLine("BOOL probe failed.");
                    Console.WriteLine(boolEx.Message);

                    try
                    {
                        Console.WriteLine("Trying DINT write/read...");
                        await logixProject.SetTagValueDINTAsync(tank1TestPath, LogixProject.OperationMode.Online, 123);
                        await Task.Delay(250);
                        long dintVal = await logixProject.GetTagValueLINTAsync(tank1TestPath, LogixProject.OperationMode.Online);
                        Console.WriteLine($"SUCCESS DINT/LINT: Tank1Test = {dintVal}");
                    }
                    catch (Exception dintEx)
                    {
                        Console.WriteLine("DINT probe failed.");
                        Console.WriteLine(dintEx.Message);

                        try
                        {
                            Console.WriteLine("Trying REAL write/read...");
                            await logixProject.SetTagValueREALAsync(tank1TestPath, LogixProject.OperationMode.Online, 12.5f);
                            await Task.Delay(250);
                            float realVal = await logixProject.GetTagValueREALAsync(tank1TestPath, LogixProject.OperationMode.Online);
                            Console.WriteLine($"SUCCESS REAL: Tank1Test = {realVal}");
                        }
                        catch (Exception realEx)
                        {
                            Console.WriteLine("REAL probe failed.");
                            Console.WriteLine(realEx.Message);
                        }
                    }
                }

                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine("=== ERROR ===");
                Console.WriteLine(ex.ToString());
                return 1;
            }
            finally
            {
                if (logixProject != null)
                {
                    try
                    {
                        await logixProject.GoOfflineAsync();
                    }
                    catch
                    {
                    }
                }

                if (cleanupOnExit)
                {
                    try
                    {
                        Console.WriteLine($"\nCleaning up Echo chassis '{chassisName}'...");
                        await LogixEchoMethods.DeleteChassis_Async(chassisName);
                        Console.WriteLine("Echo cleanup complete.");
                    }
                    catch (Exception cleanupEx)
                    {
                        Console.WriteLine("Echo cleanup failed.");
                        Console.WriteLine(cleanupEx.ToString());
                    }
                }
            }
        }

        private static string CreateTagPathFromName(string tagName)
        {
            return $"Controller/Tags/Tag[@Name='{tagName}']";
        }

        private static async Task ChangeControllerMode_Async(string commPath, string mode, LogixProject project)
        {
            mode = mode.ToUpper().Trim();

            var requestedControllerMode = default(LogixProject.RequestedControllerMode);
            if (mode == "PROGRAM")
                requestedControllerMode = LogixProject.RequestedControllerMode.Program;
            else if (mode == "RUN")
                requestedControllerMode = LogixProject.RequestedControllerMode.Run;
            else if (mode == "TEST")
                requestedControllerMode = LogixProject.RequestedControllerMode.Test;
            else
                throw new Exception($"Unsupported mode '{mode}'.");

            await project.SetCommunicationsPathAsync(commPath);
            await project.ChangeControllerModeAsync(requestedControllerMode);
        }

        private static async Task DownloadProject_Async(string commPath, LogixProject project)
        {
            await project.SetCommunicationsPathAsync(commPath);

            LogixProject.ControllerMode controllerMode = await project.ReadControllerModeAsync();
            if (controllerMode != LogixProject.ControllerMode.Program)
                throw new Exception($"Controller mode is {controllerMode}. Download requires Program mode.");

            await project.DownloadAsync();
            await project.SaveAsync();
        }
    }
}