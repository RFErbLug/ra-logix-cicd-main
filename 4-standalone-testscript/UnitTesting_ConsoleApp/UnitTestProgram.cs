// ------------------------------------------------------------------------------------------------------------------------------------------------------------
//
// FileName:     UnitTestProgram.cs
// FileType:     Visual C# Source file
// Author:       Rockwell Automation Engineering
// Created:      2024
// Description:  This script conducts unit testing utilizing Studio 5000 Logix Designer SDK and Factory Talk Logix Echo SDK.
//               Valid unit test target objects: Add-On Instructions (AOI) Definition L5X or Application ACD files
//                                               (Future considerations for Program L5X, Routine L5X, & Rung L5X)
//               Script outputs: detailed console updates, generated files needed to execute unit testing, and generated excel report
//
// The main program of this script takes 3 inputs:
//   Input 1. The file path to the input excel sheet that defines the test target object and test cases.
//   Input 2. The file path to the output excel sheet that contains the test results. (If no file path is provided, this scripts default behavior is to create
//            a new excel file. The test results are programmatically added to the a new worksheet in that excel workbook. 
//
// Example 1:
// .\UnitTesting_ConsoleApp.exe "C:\Users\ASYost\Desktop\20240816_AOIUnitTestProgress\1_InputExcelFiles\WetBulbTemperature_ControllerFaultCase.xlsx"
//            In this example, only 1 input was specified so the default values for inputs 2 and 3 are used.
//            Default input 2: The output excel file is created at the input excel file's parent directory, within a new 'X_UnitTestResults' folder.
//            Default input 2 for this example: "C:\Users\ASYost\Desktop\20240816_AOIUnitTestProgress\X_UnitTestResults\20240816171211_UnitTestReport.xlsx"
//
// Example 2:
// .\UnitTesting_ConsoleApp.exe "C:\Users\ASYost\Desktop\20240816_AOIUnitTestProgress\1_InputExcelFiles\WetBulbTemperature_ControllerFaultCase.xlsx"
// "C:\Users\ASYost\Desktop\GeneratedTestResults"
//            In this example, only 2 inputs are specified so a default value for input 3 is used.
//            Note for input 2: If the output excel file exists at the file path provided, add the report of test results within a new worksheet.
//                              If the output excel file does not exist at the file path provided, create a new workbook and add the test results worksheet.
//
// ------------------------------------------------------------------------------------------------------------------------------------------------------------

using OfficeOpenXml;
using UnitTesting_ConsoleApp.UnitTestScripts;
using static ConsoleFormatter_ClassLibrary.ConsoleFormatter;

namespace UnitTesting_ConsoleApp
{
    /// <summary>
    /// This class contains the methods and logic to programmatically conduct unit testing for Studio 5000 Logix Designer Add-On Instructions (AOIs) and ACDs.
    /// </summary>
    public class UnitTestMethods
    {
        /// <summary>
        /// This unit test example has the following steps.<br/>
        /// 1. The "input excel sheet" is parsed. This excel sheet contains the following information:<br/>
        ///    -  The Studio 5000 component is being unit tested (options: AOI_Definition.L5X, Routine.L5X, Rung.L5X, Program.L5X, Application.ACD).<br/>
        ///    -  The file path of the Studio 5000 component being tested.<br/>
        ///    -  A boolean value whether or not to retain generated ACD files.<br/>
        ///    -  A boolean value whether or not to retain generated L5X files.<br/>
        ///    -  Test cases specifying what inputs to change and what outputs to test (1 test case per excel column).<br/>
        ///    -  The number of controller clock cycles to progress each test case before verifying the outputs.<br/>
        /// 2. Create an emulated controller and chassis using the Echo SDK if one doesn't already exist.<br/>
        /// 3. A Studio 5000 Logix Designer ACD application file is created to host unit testing for L5X test inputs.<br/>
        ///    (Note: If testing ACD application, skip this section.)<br/>
        ///    -  An L5X file containing a fault handler program (contents stored within this c# solution) is converted into an ACD file.<br/>
        ///    -  If testing an AOI definition, the AOI's definition L5X is programmatically converted into a Studio 5000 rung containing a<br/>
        ///    populated instance of the AOI instruction (all required/visible instruction inputs are populated). It is then import to the ACD file.<br/>
        ///    -  If testing a rung/routine/program L5X, import the L5X component to the ACD file.<br/>
        /// 4. Commence testing. While online with the emulated controller, the LDSDK is used to change the input parameters/tags,<br/>
        ///    then verify expected output vs. actual parameter/tag value results.<br/>
        /// 5. Put unit test results into a worksheet of an excel workbook.<br/>
        ///    If the excel workbook specified in the input command does not yet exist, the workbook is created.<br/>
        ///    If the excel workbook specified in the input command exists, a new worksheet is added to the workbook.<br/>
        ///    (Note for potential future modifications of this unit test script: the output excel sheet containing the results of the<br/>
        ///     unit test was programmatically created and modified at 4 separate locations of this script.)        
        /// </summary>
        /// <param name="args">
        /// args[0] = The file path to the input excel sheet that defines the test target object and test cases.<br/>
        /// args[1] = The file path to the output excel sheet that contains the test results.
        /// </param>
        /// <returns></returns>
        static async Task Main(string[] args)
        {
            // The input excel workbook file path. (This file defines the test cases and how the unit test is conducted).
            string inputArg_inputExcelFilePath = args[0];

            // This variable will be populated from the input excel file and contains the type of target object to be unit tested. 
            string iExcel_testObjectType;

            // Parse excel sheet for test object type.
            using (ExcelPackage package = new ExcelPackage(new FileInfo(inputArg_inputExcelFilePath)))
            {
                ExcelWorksheet inputExcelWorksheet = package.Workbook.Worksheets.FirstOrDefault()!;
                iExcel_testObjectType = inputExcelWorksheet.Cells[9, 2].Value.ToString()!.Trim()!.ToUpper()!;
            }

            // Run the test script for the specific input target object.
            if (iExcel_testObjectType == "APPLICATION.ACD")
                await UnitTestScript_ACD.RunTest(args);
            else if (iExcel_testObjectType == "AOI_DEFINITION.L5X")
                await UnitTestScript_AOI.RunTest(args);
            else if (iExcel_testObjectType == "RUNG.L5X")
                ConsoleMessage("Rung unit test has yet to be developed.", "ERROR");
            else if (iExcel_testObjectType == "ROUTINE.L5X")
                ConsoleMessage("Routine unit test has yet to be developed.", "ERROR");
            else if (iExcel_testObjectType == "PROGRAM.L5X")
                ConsoleMessage("Program unit test has yet to be developed.", "ERROR");
            else
            {
                ConsoleMessage($"Test object type '{iExcel_testObjectType}' not supported. Select either AOI_Definition.L5X, Rung.L5X, Program.L5X, or " +
                    $"Application.ACD in the input excel workbook '{inputArg_inputExcelFilePath}'.", "ERROR");
            }
        }
    }
}