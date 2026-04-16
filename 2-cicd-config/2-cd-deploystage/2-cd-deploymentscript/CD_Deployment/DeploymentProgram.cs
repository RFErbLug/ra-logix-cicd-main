// ------------------------------------------------------------------------------------------------------------------------------------------------------------
//
// FileName:     DeploymentProgram.cs
// FileType:     Visual C# Source file
// Author:       Rockwell Automation Engineering
// Created:      2024
// Description:  This script programmatically downloads specified ACD applications to specified controllers.
//
// ------------------------------------------------------------------------------------------------------------------------------------------------------------

using LogixEcho_ClassLibrary;
using OfficeOpenXml;
using RockwellAutomation.LogixDesigner;
using RockwellAutomation.LogixDesigner.Logging;
using static ConsoleFormatter_ClassLibrary.ConsoleFormatter;
using static LogixDesigner_ClassLibrary.LogixDesigner;
using static RockwellAutomation.LogixDesigner.LogixProject;

namespace CD_Deployment
{
    /// <summary>
    /// This class contains the methods and scripts required to perform scripted PLC downloads.
    /// </summary>
    public class AutomatedDownload
    {
        // "STATIC VARIABLES" - Use to configure downloading setup as desired.
        public static readonly int consoleCharLengthLimit = 110; /* ---------------------------------- The character length limit of each line printed to the console.*/
        public static readonly bool showFullEventLog = false; /* ------------------------------------- Capture and print event logger information to the console.
                                                                                                         (Useful during troubleshooting.) */
        public static readonly bool deleteEchoChassis = true; /* ------------------------------------- Choose whether to keep or delete emulated chassis (including 
                                                                                                         its controllers) at the end of testing. */
        public static readonly DateTime testStartTime = DateTime.Now; /* ----------------------------- The time during which this test was first initiated. 
                                                                                                         (Used at end of test to calculate unit test length.) */
        public static readonly string currentDateTime = testStartTime.ToString("yyyyMMddHHmmss"); /* - Time during which test was first initiated.
                                                                                                         (Used to name generated files & test reports.) */

        /// <summary>
        /// Programmatically download specified ACD file applications to specified controllers.<br/>
        /// The ACD file paths and controller commnication paths are specified in an excel workbook contained within the local GitHub folder (method input arg).
        /// </summary>
        /// <param name="args">
        /// args[0] = The file path to the local GitHub folder (example format: C:\Users\TestUser\Desktop\example-github-repo\).<br/>
        /// args[1] = The folder path to which generated files will be saved during testing (example format: C:\CI-Pipeline-Files\).
        ///</param>
        /// <returns>A Task that programmatically downloads ACDs to controllers.</returns>
        static async Task Main(string[] args)
        {
            // Handle incorrect number of arguments console output.
            if ((args.Length < 1) || (args.Length > 2))
            {
                CreateBanner("INCORRECT NUMBER OF INPUTS");
                Console.WriteLine("Correct Command: ".PadRight(20, ' ') + WrapText(@".\CD_Deployment.exe inputExcelFilePath reportAndGeneratedFilesFolderPath",
                    20, consoleCharLengthLimit));
                Console.WriteLine("Example Format: ".PadRight(20, ' ') + WrapText(@".\CD_Deployment.exe C:\Users\RAUser\Desktop\2-cicd-config\2-cd-deploystage\
                    3-cd-inputexcelworkbooks\DeployToControllersWorkbook.xlsx C:\CI-Pipeline-Files\", 20, consoleCharLengthLimit));
                CreateBanner("END");
            }

            // Parse the input excel sheet needed to determine which modules are to have their firmware verified & flashed if needed.
            string githubPath = args[0];                           // 1st incoming argument = GitHub folder path
            string reportAndGeneratedFilesFolderPath = args[1];    /* 2nd incoming argument = the folder path to the folder storing generated test files
                                                                      Note that this input argument is only included to demo downloading with no hardware present. 
                                                                      This input argument reference can be freely deleted if no emulated controllers are included
                                                                      in the DeployToControllersWorkbook excel workbook list of target download controllers.*/
            string inputExcelFilePath = githubPath + @"2-cicd-config\2-cd-deploystage\3-cd-inputexcelworkbooks\DeployToControllersWorkbook.xlsx";

            // Create the local folders that will contain the test reports and generated file contents.
            if (!Directory.Exists(reportAndGeneratedFilesFolderPath))
                Directory.CreateDirectory(reportAndGeneratedFilesFolderPath);

            // Print unit test banner to the console.
            Console.WriteLine("\n  ".PadRight(consoleCharLengthLimit - 2, '='));
            Console.WriteLine("".PadRight(consoleCharLengthLimit, '='));
            string bannerContents = "CD DEPLOYMENT STAGE | " + DateTime.Now + " " + TimeZoneInfo.Local;
            int padding = (consoleCharLengthLimit - bannerContents.Length) / 2;
            Console.WriteLine(bannerContents.PadLeft(bannerContents.Length + padding).PadRight(consoleCharLengthLimit));
            Console.WriteLine("".PadRight(consoleCharLengthLimit, '='));
            Console.WriteLine("  ".PadRight(consoleCharLengthLimit - 2, '=') + "\n");

            CreateBanner(".NET INFO");
            Console.WriteLine("LDSDK .NET Core version: ".PadRight(40, ' ') + "8.0");
            Console.WriteLine("EchoSDK .NET Core version: ".PadRight(40, ' ') + "6.0");

            CreateBanner("DEPLOYMENT INFO");
            ConsoleMessage("START downloading specified ACD application files to specified controllers...", "NEWSECTION", false);
            ConsoleMessage($"Input excel workbook used is '{inputExcelFilePath}'.", "STATUS");
            CreateBanner("BEGIN DOWNLOADS");

            int numberOfControllers = GetPopulatedCellsInColumnCount(inputExcelFilePath, 2) - 2;
            ExcelPackage package = new ExcelPackage(new FileInfo(inputExcelFilePath));
            ExcelWorksheet worksheet = package.Workbook.Worksheets.FirstOrDefault()!;

            // Download ACDs to each controller specified in the input excel workbook.
            for (int i = 0; i < numberOfControllers; i++)
            {
                int rowNumber = i + 7;
                string applicationFilePath = worksheet.Cells[rowNumber, 2].Value.ToString()!;
                string plcCommPath = worksheet.Cells[rowNumber, 3].Value.ToString()!;
                string acdFilePath = githubPath + applicationFilePath;
                string generatedACD = reportAndGeneratedFilesFolderPath + currentDateTime + "_" + Path.GetFileNameWithoutExtension(acdFilePath) + ".ACD";

                // If the application file is an L5X file type, convert it to an ACD.
                if ((acdFilePath.EndsWith("L5X", StringComparison.OrdinalIgnoreCase) || acdFilePath.EndsWith("l5x", StringComparison.OrdinalIgnoreCase)) && !File.Exists(generatedACD))
                {
                    if (i == 0)
                    {
                        ConsoleMessage($"Application file path '{acdFilePath}' is an L5X file type. Start converting L5X application file to ACD...", "NEWSECTION", false);
                    }
                    else
                    {
                        ConsoleMessage($"Application file path '{acdFilePath}' is an L5X file type. Start converting L5X application file to ACD...", "NEWSECTION");
                    }

                    // Open the target object L5X application.
                    LogixProject l5xProject = await OpenLogixProjectAsync(acdFilePath);
                    ConsoleMessage($"Opened L5X application file '{acdFilePath}'.", "STATUS");

                    // The file path to the generated ACD file.
                    string l5xFilePath = acdFilePath;
                    acdFilePath = generatedACD;

                    // Convert L5X to ACD.
                    await l5xProject.SaveAsAsync(acdFilePath, true);
                    ConsoleMessage($"Converted L5X application project at '{l5xFilePath}' to ACD file at '{acdFilePath}'.", "STATUS");
                }
                if ((acdFilePath.EndsWith("L5X", StringComparison.OrdinalIgnoreCase) || acdFilePath.EndsWith("l5x", StringComparison.OrdinalIgnoreCase)) && File.Exists(generatedACD))
                    acdFilePath = generatedACD;

                // If one of the input file paths specified are for an emulated controller, create that Echo controller if it did not previously exist.
                if (plcCommPath.StartsWith(@"EmulateEthernet\"))
                {

                    if (i == 0)
                        ConsoleMessage($"START creating emulated controller for '{plcCommPath}' in chassis 'GeneratedChassis-{i + 1}'.", "NEWSECTION", false);
                    else
                        ConsoleMessage($"START creating emulated controller for '{plcCommPath}' in chassis 'GeneratedChassis-{i + 1}'.", "NEWSECTION");

                    string justIP = plcCommPath.Replace(@"EmulateEthernet\", "");
                    await LogixEchoMethods.CreateChassisFromACD_Async(acdFilePath, justIP, $"GeneratedChassis-{i + 1}");
                    ConsoleMessage($"Done creating emulated controller for '{plcCommPath}' in chassis 'GeneratedChassis-{i + 1}'.", "STATUS");
                    ConsoleMessage($"Note that in a typical CI/CD system, the input excel workbook would not contain any emulated controller communication " +
                        $"paths that need to be emulated. Emulated file paths are included only for demo purposes.", "STATUS");
                }

                // Open instance of the LogixProject class.
                if ((i == 0) && (!plcCommPath.StartsWith(@"EmulateEthernet\")))
                    ConsoleMessage($"START opening ACD file '{acdFilePath}' for '{plcCommPath}'.", "NEWSECTION", false);
                else
                    ConsoleMessage($"START opening ACD file '{acdFilePath}' for '{plcCommPath}'.", "NEWSECTION");

                LogixProject logixProject = await OpenLogixProjectAsync(acdFilePath);

                // Capture and print event logger information to the console. (Useful during troubleshooting.)
                if (showFullEventLog)
                    logixProject.AddEventHandler(new StdOutEventLogger());

                // Change controller mode to program & verify.
                ConsoleMessage($"START changing controller '{plcCommPath}' to PROGRAM mode...", "NEWSECTION");
                await ChangeControllerMode_Async(plcCommPath, "PROGRAM", logixProject);
                if (ReadControllerMode_Async(plcCommPath, logixProject).GetAwaiter().GetResult() == "PROGRAM")
                    ConsoleMessage($"SUCCESS changing controller '{plcCommPath}' to PROGRAM mode.", "STATUS", false);
                else
                    ConsoleMessage($"FAILURE changing controller '{plcCommPath}' to PROGRAM mode.", "ERROR", false);

                // Download the specified ACD application to the controller.
                ConsoleMessage($"START downloading ACD file to '{plcCommPath}'.", "NEWSECTION");
                await DownloadProject_Async(plcCommPath, logixProject);
                ConsoleMessage($"SUCCESS downloading ACD file to '{plcCommPath}'.", "STATUS", false);

                // Change controller mode to test & verify.
                ConsoleMessage($"START changing controller '{plcCommPath}' to RUN mode...", "NEWSECTION");
                await ChangeControllerMode_Async(plcCommPath, "RUN", logixProject);
                if (ReadControllerMode_Async(plcCommPath, logixProject).GetAwaiter().GetResult() == "RUN")
                    ConsoleMessage($"SUCCESS changing controller '{plcCommPath}' to RUN mode.", "STATUS", false);
                else
                    ConsoleMessage($"FAILURE changing controller '{plcCommPath}' to RUN mode.", "ERROR", false);

                // Based on the static variable deleteEchoChassis, keep or delete the Logix Echo chassis (and its controllers) used to showcase deployment.
                if (deleteEchoChassis)
                {
                    await LogixEchoMethods.DeleteChassis_Async($"GeneratedChassis-{i + 1}");
                    ConsoleMessage($"Done deleting chassis 'GeneratedChassis-{i + 1}' and its controller '{plcCommPath}'.", "NEWSECTION");
                }
                else
                {
                    ConsoleMessage($"Retaining Logix Echo chassis 'GeneratedChassis-{i + 1}' and its controller '{plcCommPath}'.", "NEWSECTION");
                }
            }

            // Compute how long the test took to run and print final banner.
            DateTime testEndTime = DateTime.Now;
            TimeSpan testLength = testEndTime.Subtract(testStartTime);
            string formattedTestLength = testLength.ToString(@"hh\:mm\:ss");
            CreateBanner($"CD DEPLOYMENT SCRIPT COMPLETED IN {formattedTestLength} (HH:mm:ss)");
        }

        #region METHODS
        /// <summary>
        /// In the first worksheet of an Excel workbook, get the number of populated cells in the specified column.
        /// </summary>
        /// <param name="excelFilePath">The excel workbook file path.</param>
        /// <param name="columnNumber">The column in which the populated cell count is derived.</param>
        /// <returns>The number of populated cells in the specified column.</returns>
        private static int GetPopulatedCellsInColumnCount(string excelFilePath, int columnNumber)
        {
            int returnCellCount = 0;
            using (ExcelPackage package = new ExcelPackage(new FileInfo(excelFilePath)))
            {
                ExcelWorksheet worksheet = package.Workbook.Worksheets.FirstOrDefault()!;
                int maxRowNum = worksheet.Dimension.End.Row;

                for (int row = 1; row <= maxRowNum; row++)
                {
                    var cellValue = worksheet.Cells[row, columnNumber].Value;

                    if (cellValue != null && !string.IsNullOrWhiteSpace(cellValue.ToString()))
                        returnCellCount++;
                }
            }
            return returnCellCount;
        }
        #endregion
    }
}