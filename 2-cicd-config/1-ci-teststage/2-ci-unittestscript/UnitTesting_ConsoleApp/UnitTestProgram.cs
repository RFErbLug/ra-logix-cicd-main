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

            int failureCount = 0;
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
                // PART 3: BOILER LOGIC TESTS
                //
                Console.WriteLine("\n--- BOILER LOGIC TESTS ---");

                string tank1ValveInStatus = CreateTagPathFromName("Tank1_ValveInStatus");
                string tank1ValveInCmd = CreateTagPathFromName("Tank1_ValveInCmd");
                string tank1ValveOutStatus = CreateTagPathFromName("Tank1_ValveOutStatus");
                string tank1ValveOutCmd = CreateTagPathFromName("Tank1_ValveOutCmd");
                string tank1Level = CreateTagPathFromName("Tank1_Level");
                string tank1SetPoint = CreateTagPathFromName("Tank1_SetPoint");

                string tank2ValveInStatus = CreateTagPathFromName("Tank2_ValveInStatus");
                string tank2ValveInCmd = CreateTagPathFromName("Tank2_ValveInCmd");
                string tank2ValveOutStatus = CreateTagPathFromName("Tank2_ValveOutStatus");
                string tank2ValveOutCmd = CreateTagPathFromName("Tank2_ValveOutCmd");
                string tank2Level = CreateTagPathFromName("Tank2_Level");
                string tank2SetPoint = CreateTagPathFromName("Tank2_SetPoint");

                await ResetAllTags(logixProject,
                    tank1ValveInCmd, tank1ValveOutCmd, tank1Level, tank1SetPoint,
                    tank2ValveInCmd, tank2ValveOutCmd, tank2Level, tank2SetPoint);

                Console.WriteLine("\nTEST 1: Tank1 inlet opens below setpoint");
                await logixProject.SetTagValueREALAsync(tank1Level, LogixProject.OperationMode.Online, 0.0f);
                await logixProject.SetTagValueREALAsync(tank1SetPoint, LogixProject.OperationMode.Online, 50.0f);
                await logixProject.SetTagValueBOOLAsync(tank1ValveOutCmd, LogixProject.OperationMode.Online, false);
                await logixProject.SetTagValueBOOLAsync(tank1ValveInCmd, LogixProject.OperationMode.Online, true);
                await Task.Delay(250);
                bool t1 = await logixProject.GetTagValueBOOLAsync(tank1ValveInStatus, LogixProject.OperationMode.Online);
                failureCount += CompareExpected("Tank1_ValveInStatus", true, t1);

                Console.WriteLine("\nTEST 2: Tank1 inlet blocked at setpoint");
                await logixProject.SetTagValueREALAsync(tank1Level, LogixProject.OperationMode.Online, 50.0f);
                await logixProject.SetTagValueREALAsync(tank1SetPoint, LogixProject.OperationMode.Online, 50.0f);
                await logixProject.SetTagValueBOOLAsync(tank1ValveInCmd, LogixProject.OperationMode.Online, true);
                await Task.Delay(250);
                bool t2 = await logixProject.GetTagValueBOOLAsync(tank1ValveInStatus, LogixProject.OperationMode.Online);
                failureCount += CompareExpected("Tank1_ValveInStatus", false, t2);

                Console.WriteLine("\nTEST 3: Tank1 outlet opens above zero");
                await logixProject.SetTagValueBOOLAsync(tank1ValveInCmd, LogixProject.OperationMode.Online, false);
                await logixProject.SetTagValueREALAsync(tank1Level, LogixProject.OperationMode.Online, 10.0f);
                await logixProject.SetTagValueBOOLAsync(tank1ValveOutCmd, LogixProject.OperationMode.Online, true);
                await Task.Delay(250);
                bool t3 = await logixProject.GetTagValueBOOLAsync(tank1ValveOutStatus, LogixProject.OperationMode.Online);
                failureCount += CompareExpected("Tank1_ValveOutStatus", true, t3);

                Console.WriteLine("\nTEST 4: Tank1 outlet blocked at zero");
                await logixProject.SetTagValueREALAsync(tank1Level, LogixProject.OperationMode.Online, 0.0f);
                await logixProject.SetTagValueBOOLAsync(tank1ValveOutCmd, LogixProject.OperationMode.Online, true);
                await Task.Delay(250);
                bool t4 = await logixProject.GetTagValueBOOLAsync(tank1ValveOutStatus, LogixProject.OperationMode.Online);
                failureCount += CompareExpected("Tank1_ValveOutStatus", false, t4);

                Console.WriteLine("\nTEST 5: Tank2 inlet opens below setpoint");
                await logixProject.SetTagValueREALAsync(tank2Level, LogixProject.OperationMode.Online, 0.0f);
                await logixProject.SetTagValueREALAsync(tank2SetPoint, LogixProject.OperationMode.Online, 50.0f);
                await logixProject.SetTagValueBOOLAsync(tank2ValveOutCmd, LogixProject.OperationMode.Online, false);
                await logixProject.SetTagValueBOOLAsync(tank2ValveInCmd, LogixProject.OperationMode.Online, true);
                await Task.Delay(250);
                bool t5 = await logixProject.GetTagValueBOOLAsync(tank2ValveInStatus, LogixProject.OperationMode.Online);
                failureCount += CompareExpected("Tank2_ValveInStatus", true, t5);

                Console.WriteLine("\nTEST 6: Tank2 inlet blocked at setpoint");
                await logixProject.SetTagValueREALAsync(tank2Level, LogixProject.OperationMode.Online, 50.0f);
                await logixProject.SetTagValueREALAsync(tank2SetPoint, LogixProject.OperationMode.Online, 50.0f);
                await logixProject.SetTagValueBOOLAsync(tank2ValveInCmd, LogixProject.OperationMode.Online, true);
                await Task.Delay(250);
                bool t6 = await logixProject.GetTagValueBOOLAsync(tank2ValveInStatus, LogixProject.OperationMode.Online);
                failureCount += CompareExpected("Tank2_ValveInStatus", false, t6);

                Console.WriteLine("\nTEST 7: Tank2 outlet opens above zero");
                await logixProject.SetTagValueBOOLAsync(tank2ValveInCmd, LogixProject.OperationMode.Online, false);
                await logixProject.SetTagValueREALAsync(tank2Level, LogixProject.OperationMode.Online, 10.0f);
                await logixProject.SetTagValueBOOLAsync(tank2ValveOutCmd, LogixProject.OperationMode.Online, true);
                await Task.Delay(250);
                bool t7 = await logixProject.GetTagValueBOOLAsync(tank2ValveOutStatus, LogixProject.OperationMode.Online);
                failureCount += CompareExpected("Tank2_ValveOutStatus", true, t7);

                Console.WriteLine("\nTEST 8: Tank2 outlet blocked at zero");
                await logixProject.SetTagValueREALAsync(tank2Level, LogixProject.OperationMode.Online, 0.0f);
                await logixProject.SetTagValueBOOLAsync(tank2ValveOutCmd, LogixProject.OperationMode.Online, true);
                await Task.Delay(250);
                bool t8 = await logixProject.GetTagValueBOOLAsync(tank2ValveOutStatus, LogixProject.OperationMode.Online);
                failureCount += CompareExpected("Tank2_ValveOutStatus", false, t8);

                await ResetAllTags(logixProject,
                    tank1ValveInCmd, tank1ValveOutCmd, tank1Level, tank1SetPoint,
                    tank2ValveInCmd, tank2ValveOutCmd, tank2Level, tank2SetPoint);

                Console.WriteLine("\n=== FINAL RESULT ===");
                if (failureCount > 0)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"FAIL | {failureCount} issue(s) found.");
                    Console.ResetColor();
                    return 1;
                }

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("PASS | All boiler logic tests passed.");
                Console.ResetColor();
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

        private static int CompareExpected(string tagName, bool expected, bool actual)
        {
            if (expected != actual)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"FAIL: {tagName} expected '{expected}' actual '{actual}'");
                Console.ResetColor();
                return 1;
            }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"PASS: {tagName} expected '{expected}' actual '{actual}'");
            Console.ResetColor();
            return 0;
        }

        private static async Task ResetAllTags(
            LogixProject logixProject,
            string tank1ValveInCmd, string tank1ValveOutCmd, string tank1Level, string tank1SetPoint,
            string tank2ValveInCmd, string tank2ValveOutCmd, string tank2Level, string tank2SetPoint)
        {
            await logixProject.SetTagValueBOOLAsync(tank1ValveInCmd, LogixProject.OperationMode.Online, false);
            await logixProject.SetTagValueBOOLAsync(tank1ValveOutCmd, LogixProject.OperationMode.Online, false);
            await logixProject.SetTagValueREALAsync(tank1Level, LogixProject.OperationMode.Online, 0.0f);
            await logixProject.SetTagValueREALAsync(tank1SetPoint, LogixProject.OperationMode.Online, 50.0f);

            await logixProject.SetTagValueBOOLAsync(tank2ValveInCmd, LogixProject.OperationMode.Online, false);
            await logixProject.SetTagValueBOOLAsync(tank2ValveOutCmd, LogixProject.OperationMode.Online, false);
            await logixProject.SetTagValueREALAsync(tank2Level, LogixProject.OperationMode.Online, 0.0f);
            await logixProject.SetTagValueREALAsync(tank2SetPoint, LogixProject.OperationMode.Online, 50.0f);

            await Task.Delay(250);
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