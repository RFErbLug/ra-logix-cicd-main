using RockwellAutomation.LogixDesigner;
using LogixEcho_ClassLibrary;

namespace UnitTesting_ConsoleApp
{
    internal class Program
    {
        static async Task<int> Main(string[] args)
        {
            string acdFilePath = @"C:\CI-Pipeline-Files\BoilerDemo.ACD";
            string commPath = LogixEchoMethods.Main(acdFilePath, "Chassis", "iotesting").GetAwaiter().GetResult();
Console.WriteLine($"COMMPATH = {commPath}");

            Console.WriteLine("=== BOILER LOGIC ONLINE TEST START ===");

            int failureCount = 0;
            LogixProject? logixProject = null;

            try
            {
                logixProject = await LogixProject.OpenLogixProjectAsync(acdFilePath);

                Console.WriteLine("Setting communication path...");
                await logixProject.SetCommunicationsPathAsync(commPath);

                Console.WriteLine("Going online...");
                // If already online through SetCommunicationsPathAsync + project operations, this is enough.
                // We are not downloading here. Just testing live values.

                //
                // Candidate UDT member paths
                //
                string tank1Level = "Controller/Tags/Tag[@Name='Tank1']/Data[@Name='Level']";
                string tank1SetPoint = "Controller/Tags/Tag[@Name='Tank1']/Data[@Name='SetPoint']";
                string tank1ValveInCmd = "Controller/Tags/Tag[@Name='Tank1']/Data[@Name='ValveInCmd']";
                string tank1ValveInStatus = "Controller/Tags/Tag[@Name='Tank1']/Data[@Name='ValveInStatus']";
                string tank1ValveOutCmd = "Controller/Tags/Tag[@Name='Tank1']/Data[@Name='ValveOutCmd']";
                string tank1ValveOutStatus = "Controller/Tags/Tag[@Name='Tank1']/Data[@Name='ValveOutStatus']";

                string tank2Level = "Controller/Tags/Tag[@Name='Tank2']/Data[@Name='Level']";
                string tank2SetPoint = "Controller/Tags/Tag[@Name='Tank2']/Data[@Name='SetPoint']";
                string tank2ValveInCmd = "Controller/Tags/Tag[@Name='Tank2']/Data[@Name='ValveInCmd']";
                string tank2ValveInStatus = "Controller/Tags/Tag[@Name='Tank2']/Data[@Name='ValveInStatus']";
                string tank2ValveOutCmd = "Controller/Tags/Tag[@Name='Tank2']/Data[@Name='ValveOutCmd']";
                string tank2ValveOutStatus = "Controller/Tags/Tag[@Name='Tank2']/Data[@Name='ValveOutStatus']";

                //
                // Reset to known state
                //
                Console.WriteLine("--- RESETTING TAGS TO KNOWN STATE ---");

                await logixProject.SetTagValueBOOLAsync(tank1ValveInCmd, LogixProject.OperationMode.Online, false);
                await logixProject.SetTagValueBOOLAsync(tank1ValveOutCmd, LogixProject.OperationMode.Online, false);
                await logixProject.SetTagValueBOOLAsync(tank2ValveInCmd, LogixProject.OperationMode.Online, false);
                await logixProject.SetTagValueBOOLAsync(tank2ValveOutCmd, LogixProject.OperationMode.Online, false);

                await Task.Delay(250);

                //
                // TEST 1
                // Tank1 inlet should open when level below setpoint and ValveInCmd = 1
                //
                Console.WriteLine("\n--- TEST 1: Tank1 inlet opens below setpoint ---");
                await logixProject.SetTagValueDINTAsync(tank1Level, LogixProject.OperationMode.Online, 0);
                await logixProject.SetTagValueDINTAsync(tank1SetPoint, LogixProject.OperationMode.Online, 50);
                await logixProject.SetTagValueBOOLAsync(tank1ValveOutCmd, LogixProject.OperationMode.Online, false);
                await logixProject.SetTagValueBOOLAsync(tank1ValveInCmd, LogixProject.OperationMode.Online, true);

                await Task.Delay(250);

                bool test1Actual = await logixProject.GetTagValueBOOLAsync(tank1ValveInStatus, LogixProject.OperationMode.Online);
                failureCount += CompareExpected("Tank1.ValveInStatus", true, test1Actual);

                //
                // TEST 2
                // Tank1 inlet should stay closed at setpoint
                //
                Console.WriteLine("\n--- TEST 2: Tank1 inlet blocked at setpoint ---");
                await logixProject.SetTagValueDINTAsync(tank1Level, LogixProject.OperationMode.Online, 50);
                await logixProject.SetTagValueDINTAsync(tank1SetPoint, LogixProject.OperationMode.Online, 50);
                await logixProject.SetTagValueBOOLAsync(tank1ValveInCmd, LogixProject.OperationMode.Online, true);

                await Task.Delay(250);

                bool test2Actual = await logixProject.GetTagValueBOOLAsync(tank1ValveInStatus, LogixProject.OperationMode.Online);
                failureCount += CompareExpected("Tank1.ValveInStatus", false, test2Actual);

                //
                // TEST 3
                // Tank1 outlet should open when level > 0 and ValveOutCmd = 1
                //
                Console.WriteLine("\n--- TEST 3: Tank1 outlet opens above zero ---");
                await logixProject.SetTagValueBOOLAsync(tank1ValveInCmd, LogixProject.OperationMode.Online, false);
                await logixProject.SetTagValueDINTAsync(tank1Level, LogixProject.OperationMode.Online, 10);
                await logixProject.SetTagValueBOOLAsync(tank1ValveOutCmd, LogixProject.OperationMode.Online, true);

                await Task.Delay(250);

                bool test3Actual = await logixProject.GetTagValueBOOLAsync(tank1ValveOutStatus, LogixProject.OperationMode.Online);
                failureCount += CompareExpected("Tank1.ValveOutStatus", true, test3Actual);

                //
                // TEST 4
                // Tank1 outlet should stay closed when level = 0
                //
                Console.WriteLine("\n--- TEST 4: Tank1 outlet blocked at zero ---");
                await logixProject.SetTagValueDINTAsync(tank1Level, LogixProject.OperationMode.Online, 0);
                await logixProject.SetTagValueBOOLAsync(tank1ValveOutCmd, LogixProject.OperationMode.Online, true);

                await Task.Delay(250);

                bool test4Actual = await logixProject.GetTagValueBOOLAsync(tank1ValveOutStatus, LogixProject.OperationMode.Online);
                failureCount += CompareExpected("Tank1.ValveOutStatus", false, test4Actual);

                //
                // TEST 5
                // Tank2 inlet should open when level below setpoint and ValveInCmd = 1
                //
                Console.WriteLine("\n--- TEST 5: Tank2 inlet opens below setpoint ---");
                await logixProject.SetTagValueDINTAsync(tank2Level, LogixProject.OperationMode.Online, 0);
                await logixProject.SetTagValueDINTAsync(tank2SetPoint, LogixProject.OperationMode.Online, 50);
                await logixProject.SetTagValueBOOLAsync(tank2ValveOutCmd, LogixProject.OperationMode.Online, false);
                await logixProject.SetTagValueBOOLAsync(tank2ValveInCmd, LogixProject.OperationMode.Online, true);

                await Task.Delay(250);

                bool test5Actual = await logixProject.GetTagValueBOOLAsync(tank2ValveInStatus, LogixProject.OperationMode.Online);
                failureCount += CompareExpected("Tank2.ValveInStatus", true, test5Actual);

                //
                // TEST 6
                // Tank2 outlet should open when level > 0 and ValveOutCmd = 1
                //
                Console.WriteLine("\n--- TEST 6: Tank2 outlet opens above zero ---");
                await logixProject.SetTagValueBOOLAsync(tank2ValveInCmd, LogixProject.OperationMode.Online, false);
                await logixProject.SetTagValueDINTAsync(tank2Level, LogixProject.OperationMode.Online, 10);
                await logixProject.SetTagValueBOOLAsync(tank2ValveOutCmd, LogixProject.OperationMode.Online, true);

                await Task.Delay(250);

                bool test6Actual = await logixProject.GetTagValueBOOLAsync(tank2ValveOutStatus, LogixProject.OperationMode.Online);
                failureCount += CompareExpected("Tank2.ValveOutStatus", true, test6Actual);

                //
                // Reset commands at end
                //
                Console.WriteLine("\n--- RESETTING COMMANDS OFF ---");
                await logixProject.SetTagValueBOOLAsync(tank1ValveInCmd, LogixProject.OperationMode.Online, false);
                await logixProject.SetTagValueBOOLAsync(tank1ValveOutCmd, LogixProject.OperationMode.Online, false);
                await logixProject.SetTagValueBOOLAsync(tank2ValveInCmd, LogixProject.OperationMode.Online, false);
                await logixProject.SetTagValueBOOLAsync(tank2ValveOutCmd, LogixProject.OperationMode.Online, false);

                await Task.Delay(250);

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
                        // Ignore shutdown cleanup failures
                    }
                }
            }
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
    }
}