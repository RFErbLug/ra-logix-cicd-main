// ------------------------------------------------------------------------------------------------------------------------------------------------------------
//
// FileName:    LogixDesigner.cs
// FileType:    Visual C# Source File
// Author:      Rockwell Automation
// Created:     2024
// Description: This class provides methods to get/set tags, read/change controller mode, and download to controller using the Studio 5000 Logix Designer SDK.
//
// ------------------------------------------------------------------------------------------------------------------------------------------------------------


using Google.Protobuf;
using RockwellAutomation.LogixDesigner;
using System.Collections;
using System.Text;
using System.Text.RegularExpressions;
using static ConsoleFormatter_ClassLibrary.ConsoleFormatter;
using static RockwellAutomation.LogixDesigner.LogixProject;

namespace LogixDesigner_ClassLibrary
{
    /// <summary>
    /// The "AOI Parameter" structure houses all the information required to read and use a single parameter of an AOI.<br/>
    /// (Note that this structure will always be used in a list, wherein each element pertains to an AOI parameter.)
    /// </summary>
    public struct AOIParameter
    {
        public string? Name { get; set; }       // The AOI parameter's name.
        public string? DataType { get; set; }   // Currently supported data types: BOOL/SINT/INT/DINT/LINT/REAL
        public string? Usage { get; set; }      // The 3 parameter usage types: Input/Output/InOut
        public bool? Required { get; set; }     // Is the parameter required in an instruction? (true/false)
        public bool? Visible { get; set; }      // Is the parameter visible in an instruction? (true/false)
        public string? Value { get; set; }      // The AOI parameter's value.
        public int BytePosition { get; set; }   // Used to track the position of the parameter in the incoming AOI byte string.
        public int BoolPosition { get; set; }   // Used to track the position of booleans in the incoming AOI byte string.
        public string? XPath { get; set; }      // The Studio 5000 tag's XPath.

        public AOIParameter() // Set default values.
        {
            Name = "";
            DataType = "";
            Usage = "";
            Required = false;
            Visible = false;
            Value = "";
            BytePosition = 0;
            BoolPosition = 0;
            XPath = "";
        }
    }

    /// <summary>
    /// The "Studio 5000 Logix Designer Tag" structure houses all the information required to read and use a single tag.
    /// </summary>
    public struct S5kAtomicTag
    {
        public string? Name { get; set; }       // The Studio 5000 tag's name.
        public string? DataType { get; set; }   // Currently supported data types: BOOL/SINT/INT/DINT/LINT/REAL/STRING
        public string? Usage { get; set; }      // Defines how this tag will be used during testing: INPUT/OUTPUT
        public string? Value { get; set; }      // The Studio 5000 tag's value (can be either Online or Offline tag value depending on struct initialization).
        public string? XPath { get; set; }      // The Studio 5000 tag's XPath.

        public S5kAtomicTag() // Set default values.
        {
            Name = "";
            DataType = "";
            Usage = "";
            Value = "";
            XPath = "";
        }
    }

    /// <summary>
    /// This class utilized the LDSDK to get/set tags, read/change controller mode, and download to controller.
    /// </summary>
    public class LogixDesigner
    {
        #region METHODS: get/set basic data type tags
        /// <summary>
        /// Asynchronously get the tag information of a Studio 5000 Logix Designer tag.<br/>
        /// (basic data types handled: boolean, single integer, integer, double integer, long integer, real, string)
        /// </summary>
        /// <param name="XPath">
        /// The XPath (tag path) specifying the tag's scope and location in the Studio 5000 Logix Designer project.<br/>
        /// The XPath for a tag is based on the XML filetype (L5X) encapsulation of elements.
        /// </param>
        /// <param name="type">The data type of the tag whose value will be returned.</param>
        /// <param name="project">An instance of the LogixProject class.</param>
        /// <param name="printout">A boolean that, if True, prints updates to the console.</param>
        /// <returns>A Task that results in a S5kTag structure containing tag information (Name, Online Value, Offline Value, XPath).</returns>
        public static async Task<S5kAtomicTag> GetTagValue_Async(string XPath, DataType type, LogixProject project, bool printout = false)
        {
            S5kAtomicTag tag = new();
            string tagName = GetNameFromXPath(XPath)!;
            tag.XPath = XPath;
            tag.Name = tagName;

            try
            {
                if (type == DataType.BOOL)
                {
                    var tagValue_online = await project.GetTagValueBOOLAsync(XPath, OperationMode.Online);
                    tag.Value = (tagValue_online == true) ? "1" : "0";
                }
                else if (type == DataType.SINT)
                {
                    var tagValue_online = await project.GetTagValueSINTAsync(XPath, OperationMode.Online);
                    tag.Value = $"{tagValue_online}";
                }
                else if (type == DataType.INT)
                {
                    var tagValue_online = await project.GetTagValueINTAsync(XPath, OperationMode.Online);
                    tag.Value = $"{tagValue_online}";
                }
                else if (type == DataType.DINT)
                {
                    var tagValue_online = await project.GetTagValueDINTAsync(XPath, OperationMode.Online);
                    tag.Value = $"{tagValue_online}";
                }
                else if (type == DataType.LINT)
                {
                    var tagValue_online = await project.GetTagValueLINTAsync(XPath, OperationMode.Online);
                    tag.Value = $"{tagValue_online}";
                }
                else if (type == DataType.REAL)
                {
                    var tagValue_online = await project.GetTagValueREALAsync(XPath, OperationMode.Online);
                    tag.Value = $"{tagValue_online}";
                }
                else if (type == DataType.STRING)
                {
                    var tagValue_online = await project.GetTagValueSTRINGAsync(XPath, OperationMode.Online);
                    tag.Value = (tagValue_online == "") ? "<empty_string>" : $"{tagValue_online}";
                }
                else
                {
                    ConsoleMessage($"Data type '{type}' not supported.", "ERROR");
                }
            }
            catch (LogixSdkException e)
            {
                ConsoleMessage($"Could not get tag '{tagName}'.", "ERROR");
                Console.WriteLine(e.Message);
            }

            if (printout)
            {
                string online_message = $"online value: {tag.Value}";
                ConsoleMessage($"{tagName,-40}{online_message,-35}", "SUCCESS");
            }

            return tag;
        }

        /// <summary>
        /// Asynchronously set either the online or offline value of a basic data type tag.<br/>
        /// (basic data types handled: boolean, single integer, integer, double integer, long integer, real, string)
        /// </summary>
        /// <param name="XPath">
        /// The XPath (tag path) specifying the tag's scope and location in the Studio 5000 Logix Designer project.<br/>
        /// The XPath for a tag is based on the XML filetype (L5X) encapsulation of elements.
        /// </param>
        /// <param name="newTagValue">The value of the tag that will be set.</param>
        /// <param name="mode">This specifies whether the 'Online' or 'Offline' value of the tag is the one to set.</param>
        /// <param name="type">The data type of the tag whose value will be set.</param>

        /// <param name="project">An instance of the LogixProject class.</param>
        /// <param name="printout">A boolean that, if True, prints the online and offline values to the console.</param>
        /// <returns>A Task that will set the online or offline value of a basic data type tag.</returns>
        public static async Task SetTagValue_Async(string XPath, string newTagValue, OperationMode mode, DataType type, LogixProject project,
            bool printout = false)
        {
            string tagName = GetNameFromXPath(XPath)!;
            S5kAtomicTag oldTag = await GetTagValue_Async(XPath, type, project);

            try
            {
                if (type == DataType.BOOL)
                {
                    string newBoolTagValue = "";
                    if (newTagValue == "1")
                        newBoolTagValue = "True";
                    else if (newTagValue == "0")
                        newBoolTagValue = "False";
                    else if ((newTagValue.ToUpper() == "TRUE") || (newTagValue.ToUpper() == "FALSE"))
                        newBoolTagValue = newTagValue;
                    else
                        ConsoleMessage($"Cannot set new boolean tag value using '{newTagValue}'. Input either 'True'/'1' or 'False'/'0'.", "ERROR");

                    await project.SetTagValueBOOLAsync(XPath, mode, bool.Parse(newBoolTagValue));
                }
                else if (type == DataType.SINT)
                    await project.SetTagValueSINTAsync(XPath, mode, sbyte.Parse(newTagValue));
                else if (type == DataType.INT)
                    await project.SetTagValueINTAsync(XPath, mode, short.Parse(newTagValue));
                else if (type == DataType.DINT)
                    await project.SetTagValueDINTAsync(XPath, mode, int.Parse(newTagValue));
                else if (type == DataType.LINT)
                    await project.SetTagValueLINTAsync(XPath, mode, long.Parse(newTagValue));
                else if (type == DataType.REAL)
                    await project.SetTagValueREALAsync(XPath, mode, float.Parse(newTagValue));
                else if (type == DataType.STRING)
                    await project.SetTagValueSTRINGAsync(XPath, mode, newTagValue);
                else
                    ConsoleMessage($"Data type '{type}' not supported.", "ERROR");
            }
            catch (LogixSdkException e)
            {
                ConsoleMessage("Unable to set tag value.", "ERROR");
                Console.WriteLine(e.Message);
            }

            try
            {
                await project.SaveAsync();
            }
            catch (LogixSdkException e)
            {
                ConsoleMessage("Unable to save project", "ERROR");
                Console.WriteLine(e.Message);
            }

            S5kAtomicTag newTag = await GetTagValue_Async(XPath, type, project);
            string newTagValueCheck = newTag.Value!;

            if (newTagValueCheck.ToUpper() != newTagValue.ToUpper())
                ConsoleMessage($"Tried to change '{tagName}' value to '{newTagValue}' but was '{newTagValueCheck}'.", "ERROR");

            if (printout)
            {
                string outputMessage = $"{oldTag.Name,-40} {oldTag.Value!,20} -> {newTagValueCheck,-20}";
                ConsoleMessage(outputMessage);
            }
        }


        /// <summary>
        /// Asynchronously toggle the value of a boolean tag in Studio 5000 Logix Designer.
        /// </summary>
        /// <param name="XPath">
        /// The XPath (tag path) specifying the tag's scope and location in the Studio 5000 Logix Designer project.<br/>
        /// The XPath for a tag is based on the XML filetype (L5X) encapsulation of elements.
        /// </param>
        /// <param name="toggleOnToOff">If True, toggle the tag on to off. If False, toggle the tag off to on.</param>
        /// <param name="mode">Specify online or offline operation.</param>
        /// <param name="project">An instance of the LogixProject class.</param>
        /// <param name="printout">A boolean that, if True, prints updates to the console.</param>
        /// <returns>A Task that asynchronously toggles a specified BOOL tag.</returns>
        public static async Task ToggleBOOLTagValue_Async(string XPath, bool toggleOnToOff, OperationMode mode, LogixProject project, bool printout = false)
        {
            string tagName = GetNameFromXPath(XPath)!;
            if (toggleOnToOff)
            {
                await SetTagValue_Async(XPath, "1", mode, DataType.BOOL, project, printout);
                await SetTagValue_Async(XPath, "0", mode, DataType.BOOL, project, printout);
            }
            else
            {
                await SetTagValue_Async(XPath, "0", mode, DataType.BOOL, project, printout);
                await SetTagValue_Async(XPath, "1", mode, DataType.BOOL, project, printout);
            }
            if (printout)
                ConsoleMessage($"Done toggling '{tagName}'.", "STATUS");
        }

        /// <summary>
        /// Asynchronously get the online tag information for an array of basic data type tags.<br/>
        /// (basic data types handled: boolean, single integer, integer, double integer, long integer, real, string)
        /// </summary>
        /// <param name="s5KAtomicTags">The S5kAtomicTag structure array containing the atomic tags.</param>
        /// <param name="testCaseValues">A dictionary containing the tag name (dict key) and new tag value (dict value).</param>
        /// <param name="project">An instance of the LogixProject class.</param>
        /// <param name="printout">A boolean that, if True, prints updates to the console.</param>
        /// <returns></returns>
        public static async Task<S5kAtomicTag[]?> GetTagValuesPerTestCase(S5kAtomicTag[]? s5KAtomicTags, Dictionary<string, string> testCaseValues,
            LogixProject project, bool printout = false)
        {
            // Get the number of input tags in the current test case.
            int numberOfOutputs = 0;
            for (int i = 0; i < s5KAtomicTags!.Length; i++)
            {
                if ((!string.IsNullOrEmpty(testCaseValues[s5KAtomicTags[i].Name!])) && (s5KAtomicTags[i].Usage == "OUTPUT"))
                {
                    numberOfOutputs++;
                }
            }
            S5kAtomicTag[]? returnTags = new S5kAtomicTag[numberOfOutputs];

            // Copy the output information contained in the s5kAtomicTags array into the new array.
            int outputCount = 0;
            for (int i = 0; i < s5KAtomicTags!.Length; i++)
            {
                if ((!string.IsNullOrEmpty(testCaseValues[s5KAtomicTags[i].Name!])) && (s5KAtomicTags[i].Usage == "OUTPUT"))
                {
                    returnTags[outputCount] = s5KAtomicTags[i];
                    outputCount++;
                }
            }

            // Get the S5kAtomicTag info for each output tag and pack it into the return array.
            for (int i = 0; i < returnTags!.Length; i++)
            {
                if ((!string.IsNullOrEmpty(testCaseValues[returnTags[i].Name!])) && (returnTags[i].Usage == "OUTPUT"))
                {
                    var currentTag = await GetTagValue_Async(returnTags[i].XPath!, GetDataType(returnTags[i].DataType!), project, printout);
                    returnTags[i].Value = currentTag.Value;
                }
            }

            return returnTags;
        }

        /// <summary>
        /// Asynchronously set the online values of an array of basic data type tags.<br/>
        /// (basic data types handled: boolean, single integer, integer, double integer, long integer, real, string)
        /// </summary>
        /// <param name="s5KAtomicTags">The S5kAtomicTag structure array containing the atomic tags.</param>
        /// <param name="testCaseValues">A dictionary containing the tag name (dict key) and new tag value (dict value).</param>
        /// <param name="project">An instance of the LogixProject class.</param>
        /// <param name="printout">A boolean that, if True, prints updates to the console.</param>
        /// <returns></returns>
        public static async Task SetTagValuesPerTestCase(S5kAtomicTag[]? s5KAtomicTags, Dictionary<string, string> testCaseValues, LogixProject project,
            bool printout = false)
        {
            for (int i = 0; i < s5KAtomicTags!.Length; i++)
            {
                if ((!string.IsNullOrEmpty(testCaseValues[s5KAtomicTags[i].Name!])) && (s5KAtomicTags[i].Usage == "INPUT"))
                {
                    await SetTagValue_Async(s5KAtomicTags[i].XPath!, testCaseValues[s5KAtomicTags[i].Name!], OperationMode.Online,
                        GetDataType(s5KAtomicTags[i].DataType!), project, printout);
                }
            }
        }

        /// <summary>
        /// Print all the structure information of an S5kAtomicTag array to the console.
        /// </summary>
        /// <param name="s5kAtomicTags">The S5kAtomicTag structure array to be printed.</param>
        /// <param name="printAll">If True, print the structure subcomponent XPath and Value.</param>
        public static void PrintS5kAtomicTags(S5kAtomicTag[]? s5kAtomicTags, bool printAll)
        {
            // Console formatting: Get the max character length of the below 4 AOIParameter structure subcomponents within the input array.
            int[] S5kStructComponentCharLimits = new int[5];
            for (int i = 0; i < s5kAtomicTags!.Length; i++)
            {
                if (s5kAtomicTags[i].Value == null)
                    s5kAtomicTags[i].Value = "";
                if (s5kAtomicTags[i].XPath == null)
                    s5kAtomicTags[i].XPath = "";

                if (s5kAtomicTags[i].Name!.Length > S5kStructComponentCharLimits[0])
                    S5kStructComponentCharLimits[0] = s5kAtomicTags[i].Name!.Length;
                if (s5kAtomicTags[i].DataType!.Length > S5kStructComponentCharLimits[1])
                    S5kStructComponentCharLimits[1] = s5kAtomicTags[i].DataType!.Length;
                if (s5kAtomicTags[i].Usage!.Length > S5kStructComponentCharLimits[2])
                    S5kStructComponentCharLimits[2] = s5kAtomicTags[i].Usage!.Length;
                if (s5kAtomicTags[i].Value!.Length > S5kStructComponentCharLimits[3])
                    S5kStructComponentCharLimits[3] = s5kAtomicTags[i].Value!.Length;
                if (s5kAtomicTags[i].XPath!.Length > S5kStructComponentCharLimits[4])
                    S5kStructComponentCharLimits[4] = s5kAtomicTags[i].XPath!.Length;
            }

            for (int i = 0; i < s5kAtomicTags.Length; i++)
            {
                // Add the parameter formatted information to the current line.
                if (printAll)
                {
                    Console.WriteLine($"           Name: {s5kAtomicTags[i].Name!.PadRight(S5kStructComponentCharLimits[0], ' ')} | " +
                        $"Data Type: {s5kAtomicTags[i].DataType!.PadRight(S5kStructComponentCharLimits[1], ' ')} | " +
                        $"Usage: {s5kAtomicTags[i].Usage!.PadRight(S5kStructComponentCharLimits[2], ' ')} | " +
                        $"Value: {s5kAtomicTags[i].Value!.PadRight(S5kStructComponentCharLimits[3], ' ')} | " +
                        $"XPath: {s5kAtomicTags[i].XPath!.PadRight(S5kStructComponentCharLimits[4], ' ')}");
                }
                else
                {
                    Console.WriteLine($"           Name: {s5kAtomicTags[i].Name!.PadRight(S5kStructComponentCharLimits[0], ' ')} | " +
                        $"Data Type: {s5kAtomicTags[i].DataType!.PadRight(S5kStructComponentCharLimits[1], ' ')} | " +
                        $"Usage: {s5kAtomicTags[i].Usage!.PadRight(S5kStructComponentCharLimits[2], ' ')} |  " +
                        $"Value: {s5kAtomicTags[i].Value!.PadRight(S5kStructComponentCharLimits[3], ' ')}");
                }
            }
        }
        #endregion

        #region METHODS: get/set AOI tags (complex data type)
        /// <summary>
        /// Asynchronously set (and verify the change of) multiple AOI parameter values at the same time.
        /// </summary>
        /// <param name="XPath">
        /// The XPath (tag path) specifying the tag's scope and location in the Studio 5000 Logix Designer project.<br/>
        /// The XPath for a tag is based on the XML filetype (L5X) encapsulation of elements.
        /// </param>
        /// <param name="newParameterValues">A dictionary where the keys are parameter names and where the values are the new parameter values.</param>
        /// <param name="AOIParameters">An array of the AOIParameter structure that contains required parameter name, usage, and parsing data info.</param>
        /// <param name="mode">This specifies whether the 'Online' or 'Offline' value of the tag is the one to set.</param>
        /// <param name="project">An instance of the LogixProject class.</param>
        /// <param name="printOut">A boolean that, if True, prints updates to the console.</param>
        /// <returns>A Task that will set multiple AOI parameters at the same time.</returns>
        /// <exception cref="Exception">Studio 5000 Logix Designer exceptions.</exception>
        public static async Task SetMultipleAOIParamVals_Async(string XPath, Dictionary<string, string> newParameterValues,
            AOIParameter[] AOIParameters, OperationMode mode, LogixProject project, bool printOut = false)
        {
            ByteString oldByteString = await project.GetTagValueAsync(XPath, mode, DataType.BYTE_ARRAY);
            byte[] modifiedByteArray = oldByteString.ToByteArray();

            string oldParameterValue;
            string newParameterValue;
            int numberOfInputs = 0;

            // Rotate through all the AOI parameters.
            for (int i = 0; i < AOIParameters.Length; i++)
            {
                if (AOIParameters[i].Usage! == "Input")
                {
                    numberOfInputs++;
                }
            }

            // Rotate through all the AOI parameters.
            for (int i = 0; i < AOIParameters.Length; i++)
            {
                // While rotating through AOI parameters, only make changes if they are not an output parameter.
                if (AOIParameters[i].Usage == "Input")
                {
                    DataType dataType = GetDataType(AOIParameters[i].DataType!);
                    int bytePosition = AOIParameters[i].BytePosition;

                    oldParameterValue = AOIParameters[i].Value!;
                    newParameterValue = newParameterValues[AOIParameters[i].Name!];

                    // Update the value of the current input parameter in the byte string (currently in array format) using the data type & byte position
                    // information from AOIParameters.
                    if (dataType == DataType.BOOL)
                    {
                        byte[] bools_byteArray = new byte[4];
                        Array.ConstrainedCopy(modifiedByteArray, bytePosition, bools_byteArray, 0, 4);
                        var bitArray = new BitArray(bools_byteArray);

                        int boolPosition = AOIParameters[i].BoolPosition;
                        bool bool_newTagValue = newParameterValue == "1";
                        bitArray[boolPosition] = bool_newTagValue;
                        bitArray.CopyTo(bools_byteArray, 0);

                        for (int j = 0; j < 4; ++j)
                            modifiedByteArray[j + bytePosition] = bools_byteArray[j];
                    }
                    else if (dataType == DataType.SINT)
                    {
                        modifiedByteArray[bytePosition] = BitConverter.GetBytes(int.Parse(newParameterValue))[0];
                    }
                    else if (dataType == DataType.INT)
                    {
                        byte[] int_byteArray = BitConverter.GetBytes(int.Parse(newParameterValue));
                        for (int j = 0; j < 2; ++j)
                            modifiedByteArray[j + bytePosition] = int_byteArray[j];
                    }
                    else if (dataType == DataType.DINT)
                    {
                        byte[] dint_byteArray = BitConverter.GetBytes(long.Parse(newParameterValue));
                        for (int j = 0; j < 4; ++j)
                            modifiedByteArray[j + bytePosition] = dint_byteArray[j];
                    }
                    else if (dataType == DataType.LINT)
                    {
                        byte[] lint_byteArray = BitConverter.GetBytes(long.Parse(newParameterValue));
                        for (int j = 0; j < 8; ++j)
                            modifiedByteArray[j + bytePosition] = lint_byteArray[j];
                    }
                    else if (dataType == DataType.REAL)
                    {
                        byte[] real_byteArray = BitConverter.GetBytes(float.Parse(newParameterValue));
                        for (int j = 0; j < 4; ++j)
                            modifiedByteArray[j + bytePosition] = real_byteArray[j];
                    }
                    else
                    {
                        ConsoleMessage($"Data type '{dataType}' not supported by 'SetMultipleAOIParamVals_Async' method.", "ERROR");
                    }

                    // If specified using the method input, print the current parameter changed to the console.
                    if (printOut)
                    {
                        string setParamIntro = $"{AOIParameters[i].Name} value:".PadRight(40, ' ');

                        // Write the first formatting component of the current parameter to the console.
                        if (i == 0)
                        {
                            setParamIntro = "┌── " + setParamIntro;
                        }
                        else if (i < numberOfInputs)
                        {
                            setParamIntro = "├── " + setParamIntro;
                        }
                        else
                        {
                            setParamIntro = "└── " + setParamIntro;
                        }

                        ConsoleMessage($"{setParamIntro} {oldParameterValue,20} -> {newParameterValue,-20}");
                    }
                }
            }

            // Push the newly modified byte string (containing the AOI tag information) to Studio 5000 Logix Designer. 
            try
            {
                await project.SetTagValueAsync(XPath, mode, modifiedByteArray, DataType.BYTE_ARRAY);
            }
            catch (LogixSdkException e)
            {
                ConsoleMessage("Unable to set tag values.", "ERROR");
                Console.WriteLine(e.Message);
            }

            // Verify that the tag change was actually implemented.
            try
            {
                ByteString newByteString = await project.GetTagValueAsync(XPath, mode, DataType.BYTE_ARRAY);
                AOIParameter[] newAOIParameters = GetAOIParameterValues(AOIParameters, newByteString);

                foreach (var kvp in newParameterValues)
                {
                    foreach (var param in newAOIParameters)
                    {
                        if ((kvp.Key == param.Name) && (param.Usage == "Input"))
                        {
                            if (kvp.Value != param.Value)
                            {
                                throw new Exception("Method 'SetMultipleAOIParamVals_Async' did not properly set tag values.");
                            }
                        }
                    }
                }
            }
            catch (LogixSdkException e)
            {
                ConsoleMessage("Unable to get new tag values to verify tag change.", "ERROR");
                Console.WriteLine(e.Message);
            }

            // Save the project.
            try
            {
                await project.SaveAsync();
            }
            catch (LogixSdkException e)
            {
                ConsoleMessage("Unable to save project.", "ERROR");
                Console.WriteLine(e.Message);
            }
        }

        /// <summary>
        /// Parse any AOI ByteString into its relevant parameter values.<br/>
        /// To parse the input ByteString properly, the following conditions must be provided in the input AOIParameter array:<br/>
        /// 1. The data types of all the AOI parameters must already be included.<br/>
        /// 2. The AOI parameter data types must be ordered how they were added during AOI creation (as shown in the AOI instruction window in S5k).
        /// </summary>
        /// <param name="AOIParameters">An array of the AOIParameter structure that contains required parameter name, usage, and parsing data info.</param>
        /// <param name="aoiByteString">The byte string from which to get values for the AOIParameters input.</param>
        /// <param name="printOut">A boolean that, if True, prints updates to the console.</param>
        /// <returns>An updated instance of the AOIParameter structure with parameter values from the input ByteString.</returns>
        public static AOIParameter[] GetAOIParameterValues(AOIParameter[] AOIParameters, ByteString aoiByteString, bool printOut = false)
        {
            // Split ByteSTring into its individual bytes.
            byte[] inputByteArray = aoiByteString.ToByteArray();

            // To properly parse the byte array, the below variables track and/or create the logic determining which bytes to get information from.
            int byteStartPosition = 0; // Increases by different amounts based on the data type of the parameter (ex. DINTs = 4 bytes, LINTs = 8 bytes).
            int boolStartPosition = 0; // Store the boolean's byte location because they are stored together.
            int boolCount = 0;         // Tracks the # of booleans. This helps determine when to update the boolStartPosition.

            // Rotate through all the AOI parameters.
            for (int i = 0; i < AOIParameters.Length; i++)
            {
                // Get the data type of the current element (AOI parameter) of the AOIParameters array.
                string paramDataType = AOIParameters[i].DataType!;

                // Based on the current AOI parameter's data type, convert a specific number of bytes into the parameter value. Then, update the byte trackers.
                if (paramDataType == "BOOL")
                {
                    // Update the "boolean host member" location of the input byte array that is being checked every 32 booleans.
                    if (((boolCount % 32 == 1) && (boolCount > 1)) || (boolCount == 0))
                    {
                        boolStartPosition = byteStartPosition;
                        byteStartPosition += 4;
                    }

                    // Booleans are stored in clusters of 4 bytes.
                    byte[] bools_bytearray = new byte[4];
                    Array.ConstrainedCopy(inputByteArray, boolStartPosition, bools_bytearray, 0, 4);

                    // Reverse the elements of the byte array & put them all in one string. This helps to get a specific boolean's value later.
                    StringBuilder sb = new StringBuilder();
                    for (int j = bools_bytearray.Length - 1; j >= 0; j--)
                        sb.Append(Convert.ToString(bools_bytearray[j], 2).PadLeft(8, '0'));
                    string bools_string = sb.ToString();

                    // Update parameter value & tracker information to the "final result array". 
                    AOIParameters[i].Value = bools_string[31 - boolCount].ToString();
                    AOIParameters[i].BytePosition = boolStartPosition;
                    AOIParameters[i].BoolPosition = boolCount;

                    boolCount++;
                }
                else if (paramDataType == "SINT")
                {
                    // Single integers are 1 byte in length.
                    byte[] sint_bytearray = new byte[1];
                    Array.ConstrainedCopy(inputByteArray, byteStartPosition, sint_bytearray, 0, 1);
                    string sint_string = Convert.ToString(unchecked((sbyte)sint_bytearray[0]));

                    // Update parameter value & tracker information to the "final result array". 
                    AOIParameters[i].Value = sint_string;
                    AOIParameters[i].BytePosition = byteStartPosition;
                    byteStartPosition += 1;
                }
                else if (paramDataType == "INT")
                {
                    // Integers are not stored at "odd valued" byte locations. If odd, make the tracker even. Consider the skipped, empty byte a "buffer byte".
                    if ((byteStartPosition % 2) > 0)
                        byteStartPosition += 1;

                    // Integers are 1 byte in length.
                    byte[] int_bytearray = new byte[2];
                    Array.ConstrainedCopy(inputByteArray, byteStartPosition, int_bytearray, 0, 2);
                    string int_string = Convert.ToString(BitConverter.ToInt16(int_bytearray));

                    // Update parameter value & tracker information to the "final result array". 
                    AOIParameters[i].Value = int_string;
                    AOIParameters[i].BytePosition = byteStartPosition;
                    byteStartPosition += 2;
                }
                else if (paramDataType == "DINT")
                {
                    if ((byteStartPosition % 4) > 0)
                        byteStartPosition += 4 - (byteStartPosition % 4);

                    byte[] dint_bytearray = new byte[4];
                    Array.ConstrainedCopy(inputByteArray, byteStartPosition, dint_bytearray, 0, 4);
                    string dint_string = Convert.ToString(BitConverter.ToInt32(dint_bytearray));

                    // Update parameter value & tracker information to the "final result array". 
                    AOIParameters[i].Value = dint_string;
                    AOIParameters[i].BytePosition = byteStartPosition;
                    byteStartPosition += 4;
                }
                else if (paramDataType == "LINT")
                {
                    if ((byteStartPosition % 8) > 0)
                        byteStartPosition += 8 - (byteStartPosition % 8);

                    byte[] lint_bytearray = new byte[8];
                    Array.ConstrainedCopy(inputByteArray, byteStartPosition, lint_bytearray, 0, 8);
                    string lint_string = Convert.ToString(BitConverter.ToInt64(lint_bytearray));

                    // Update parameter value & tracker information to the "final result array". 
                    AOIParameters[i].Value = lint_string;
                    AOIParameters[i].BytePosition = byteStartPosition;
                    byteStartPosition += 8;
                }
                else if (paramDataType == "REAL")
                {
                    if ((byteStartPosition % 4) > 0)
                        byteStartPosition += 4 - (byteStartPosition % 4);

                    byte[] real_bytearray = new byte[4];
                    Array.ConstrainedCopy(inputByteArray, byteStartPosition, real_bytearray, 0, 4);
                    string real_string = Convert.ToString(BitConverter.ToSingle(real_bytearray));

                    // Update parameter value & tracker information to the "final result array". 
                    AOIParameters[i].Value = real_string;
                    AOIParameters[i].BytePosition = byteStartPosition;
                    byteStartPosition += 4;
                }
                else
                {
                    ConsoleMessage($"The GetAOIParameterValues method cannot handle process the data type '{paramDataType}'.", "STATUS");
                    AOIParameters[i].BytePosition = byteStartPosition;
                }
            }

            // Print all the AOI parameter structure components to the console.
            if (printOut)
            {
                PrintAOIParameters(AOIParameters, true);
            }

            return AOIParameters;
        }

        /// <summary>
        /// Print all the AOI information within an AOIParameter structure array to the console.
        /// </summary>
        /// <param name="AOIParameters">The AOIParameter structure array to be printed.</param>
        /// <param name="printAll">If True, print the internally used structure subcomponents, BytePosition and BoolPosition.</param>
        public static void PrintAOIParameters(AOIParameter[]? AOIParameters, bool printAll)
        {
            // Console formatting: Get the max character length of the below 4 AOIParameter structure subcomponents within the input array.
            int[] AOIParameterLimits = new int[5];
            AOIParameterLimits[3] = 5;
            AOIParameterLimits[4] = 20;
            for (int i = 0; i < AOIParameters!.Length; i++)
            {
                if (AOIParameters[i].Value == null)
                    AOIParameters[i].Value = "";
                if (AOIParameters[i].XPath == null)
                    AOIParameters[i].XPath = "";

                if (AOIParameters[i].Name!.Length > AOIParameterLimits[0])
                    AOIParameterLimits[0] = AOIParameters[i].Name!.Length;
                if (AOIParameters[i].DataType!.Length > AOIParameterLimits[1])
                    AOIParameterLimits[1] = AOIParameters[i].DataType!.Length;
                if (AOIParameters[i].Usage!.Length > AOIParameterLimits[2])
                    AOIParameterLimits[2] = AOIParameters[i].Usage!.Length;
                if (AOIParameters[i].Value!.Length > AOIParameterLimits[3])
                    AOIParameterLimits[3] = AOIParameters[i].Value!.Length;
                if (AOIParameters[i].XPath!.Length > AOIParameterLimits[4])
                    AOIParameterLimits[4] = AOIParameters[i].XPath!.Length;
            }

            for (int i = 0; i < AOIParameters.Length; i++)
            {
                // Write the first formatting component of a new parameter to the console.
                if (i == 0)
                {
                    Console.Write(" ┌── ");
                }
                else if (i < AOIParameters.Length - 1)
                {
                    Console.Write(" ├── ");
                }
                else
                {
                    Console.Write(" └── ");
                }

                // Add the parameter formatted information to the current line.
                if (printAll)
                {
                    Console.WriteLine($"Name: {AOIParameters[i].Name!.PadRight(AOIParameterLimits[0], ' ')} | " +
                        $"Data Type: {AOIParameters[i].DataType!.PadRight(AOIParameterLimits[1], ' ')} | " +
                        $"Usage: {AOIParameters[i].Usage!.PadRight(AOIParameterLimits[2], ' ')} | Required: {AOIParameters[i].Required,-5} | " +
                        $"Visible: {AOIParameters[i].Visible,-5} |  Value: {AOIParameters[i].Value!.PadRight(AOIParameterLimits[3], ' ')} | " +
                        $"Byte Position: {AOIParameters[i].BytePosition,-3} | Bool Position: {AOIParameters[i].BoolPosition} | " +
                        $"XPath: {AOIParameters[i].XPath!.PadRight(AOIParameterLimits[4], ' ')}");
                }
                else
                {
                    Console.WriteLine($"Name: {AOIParameters[i].Name!.PadRight(AOIParameterLimits[0], ' ')} | " +
                        $"Data Type: {AOIParameters[i].DataType!.PadRight(AOIParameterLimits[1], ' ')} | " +
                        $"Usage: {AOIParameters[i].Usage!.PadRight(AOIParameterLimits[2], ' ')} |  " +
                        $"Value: {AOIParameters[i].Value!.PadRight(AOIParameterLimits[3], ' ')}");
                }
            }
        }

        /// <summary>
        /// Get a specific subcomponent value from a specific element of the AOIParameter structure array. 
        /// </summary>
        /// <param name="parameterName">The name of the AOI parameter that determines which element of the AOIParameter array to search within.</param>
        /// <param name="AOIParameterField">The target field (structure subcomponent name) to get the value of.<br/>
        /// (Name, DataType, Usage, Required, Visible, Value, BytePosition, BoolPosition)</param>
        /// <param name="AOIParameters">An array of the AOIParameter structure that contains the parameter characteristics and parsing data info.</param>
        /// <returns>The value of the target field (structure subcomponent name) as a string.</returns>
        public static string GetAOIParameterComponentValue(string parameterName, string AOIParameterField, AOIParameter[] AOIParameters)
        {
            AOIParameterField = AOIParameterField.Trim().ToUpper();
            string returnString = "";
            for (int i = 0; i < AOIParameters.Length; i++)
            {
                if (AOIParameters[i].Name == parameterName)
                {
                    if (AOIParameterField == "NAME")
                    {
                        returnString = AOIParameters[i].Name!;
                    }
                    if (AOIParameterField == "DATATYPE")
                    {
                        returnString = AOIParameters[i].DataType!;
                    }
                    if (AOIParameterField == "USAGE")
                    {
                        returnString = AOIParameters[i].Usage!;
                    }
                    if (AOIParameterField == "REQUIRED")
                    {
                        returnString = AOIParameters[i].Required.ToString()!;
                    }
                    if (AOIParameterField == "VISIBLE")
                    {
                        returnString = AOIParameters[i].Visible.ToString()!;
                    }
                    if (AOIParameterField == "VALUE")
                    {
                        returnString = AOIParameters[i].Value!;
                    }
                    if (AOIParameterField == "BYTEPOSITION")
                    {
                        returnString = AOIParameters[i].BytePosition.ToString();
                    }
                    if (AOIParameterField == "BOOLPOSITION")
                    {
                        returnString = AOIParameters[i].BoolPosition.ToString();
                    }
                    if (AOIParameterField == "XPATH")
                    {
                        returnString = AOIParameters[i].XPath!;
                    }
                }
            }
            return returnString;
        }

        /// <summary>
        /// Asynchronously set (and verify the change of) a single value of an AOI tag in Studio 5000 Logix Designer.
        /// </summary>
        /// <param name="XPath">
        /// The XPath (tag path) specifying the tag's scope and location in the Studio 5000 Logix Designer project.<br/>
        /// The XPath for a tag is based on the XML filetype (L5X) encapsulation of elements.
        ///</param>
        /// <param name="newParameterValue">The new value of the AOI parameter as a string.</param>
        /// <param name="parameterName">The name of the parameter that will have its value changed.</param>
        /// <param name="mode">This specifies whether the 'Online' or 'Offline' value of the tag is the one to set.</param>
        /// <param name="AOIParameters">An array of the AOIParameter structure that contains required parameter name, usage, and parsing data info.</param>
        /// <param name="project">An instance of the LogixProject class.</param>
        /// <param name="printOut">A boolean that, if True, prints updates to the console.</param>
        /// <returns></returns>
        /// <exception cref="Exception">Studio 5000 Logix Designer exceptions.</exception>
        public static async Task SetSingleAOIParamValue_Async(string XPath, string newParameterValue, string parameterName, OperationMode mode,
            AOIParameter[] AOIParameters, LogixProject project, bool printOut = false)
        {
            ByteString oldByteString = await project.GetTagValueAsync(XPath, mode, DataType.BYTE_ARRAY);
            AOIParameter[] oldAOIParameters = GetAOIParameterValues(AOIParameters, oldByteString);

            byte[] modifiedByteArray = oldByteString.ToByteArray();

            string oldParameterValue = GetAOIParameterComponentValue(parameterName, "Value", oldAOIParameters);

            // Rotate through all the AOI parameters.
            for (int i = 0; i < AOIParameters.Length; i++)
            {
                // Stop rotating through the AOI parameters when on the parameter 
                if (AOIParameters[i].Name == parameterName)
                {
                    DataType dataType = GetDataType(AOIParameters[i].DataType!);
                    int bytePosition = AOIParameters[i].BytePosition;

                    if (dataType == DataType.BOOL)
                    {
                        byte[] bools_byteArray = new byte[4];
                        Array.ConstrainedCopy(modifiedByteArray, bytePosition, bools_byteArray, 0, 4);
                        var bitArray = new BitArray(bools_byteArray);

                        int boolPosition = AOIParameters[i].BoolPosition;
                        bool bool_newTagValue = bool.Parse(newParameterValue);
                        newParameterValue = (newParameterValue == "True") ? "1" : "0";
                        bitArray[boolPosition] = bool_newTagValue;
                        bitArray.CopyTo(bools_byteArray, 0);


                        for (int j = 0; j < 4; ++j)
                            modifiedByteArray[j + bytePosition] = bools_byteArray[j];
                    }
                    else if (dataType == DataType.SINT)
                    {
                        string sint_string = Convert.ToString(long.Parse(newParameterValue), 2);
                        sint_string = sint_string.Substring(sint_string.Length - 8);
                        modifiedByteArray[bytePosition] = Convert.ToByte(sint_string, 2);
                    }
                    else if (dataType == DataType.INT)
                    {
                        byte[] int_byteArray = BitConverter.GetBytes(int.Parse(newParameterValue));
                        for (int j = 0; j < 2; ++j)
                            modifiedByteArray[j + bytePosition] = int_byteArray[j];
                    }
                    else if (dataType == DataType.DINT)
                    {
                        byte[] dint_byteArray = BitConverter.GetBytes(long.Parse(newParameterValue));
                        for (int j = 0; j < 4; ++j)
                            modifiedByteArray[j + bytePosition] = dint_byteArray[j];
                    }
                    else if (dataType == DataType.LINT)
                    {
                        byte[] lint_byteArray = BitConverter.GetBytes(long.Parse(newParameterValue));
                        for (int j = 0; j < 8; ++j)
                            modifiedByteArray[j + bytePosition] = lint_byteArray[j];
                    }
                    else if (dataType == DataType.REAL)
                    {
                        byte[] real_byteArray = BitConverter.GetBytes(float.Parse(newParameterValue));
                        for (int j = 0; j < 4; ++j)
                            modifiedByteArray[j + bytePosition] = real_byteArray[j];
                    }
                    else
                    {
                        ConsoleMessage($"Data type '{dataType}' not supported by 'SetSingleAOIParamValue_Async' method.", "ERROR");
                    }
                }
            }

            // Push the newly modified byte string (containing the AOI tag information) to Studio 5000 Logix Designer. 
            try
            {
                await project.SetTagValueAsync(XPath, mode, modifiedByteArray, DataType.BYTE_ARRAY);
            }
            catch (LogixSdkException e)
            {
                ConsoleMessage("Unable to set tag value.", "ERROR");
                Console.WriteLine(e.Message);
            }

            // Verify that the tag value change was actually implemented.
            try
            {
                ByteString newByteString = await project.GetTagValueAsync(XPath, mode, DataType.BYTE_ARRAY);
                AOIParameter[] updatedAOIParameters = GetAOIParameterValues(AOIParameters, newByteString);
                string updatedParameterValue = GetAOIParameterComponentValue(parameterName, "Value", updatedAOIParameters);

                if (updatedAOIParameters != oldAOIParameters)
                {
                    throw new Exception("SetSingleAOIParamValue_Async method did not properly set tag value.");
                }
            }
            catch (LogixSdkException e)
            {
                ConsoleMessage("Unable to get new tag values to verify tag change.", "ERROR");
                Console.WriteLine(e.Message);
            }

            // Save the project.
            try
            {
                await project.SaveAsync();
            }
            catch (LogixSdkException e)
            {
                ConsoleMessage("Unable to save project.", "ERROR");
                Console.WriteLine(e.Message);
            }

            if (printOut)
            {
                string setParamIntro = $"{parameterName} value:".PadRight(40, ' ');
                ConsoleMessage($"{setParamIntro} {oldParameterValue,20} -> {newParameterValue,-20}");
            }
        }
        #endregion

        #region METHODS: read/change controller mode & download
        /// <summary>
        /// Asynchronously get the current controller mode (FAULTED, PROGRAM, RUN, or TEST).
        /// </summary>
        /// <param name="commPath">The controller communication path.</param>
        /// <param name="project">An instance of the LogixProject class.</param>
        /// <returns>A Task that returns a string of the current controller mode.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if the returned controller mode is not FAULTED, PROGRAM, RUN, or TEST.</exception>
        public static async Task<string> ReadControllerMode_Async(string commPath, LogixProject project)
        {
            try
            {
                await project.SetCommunicationsPathAsync(commPath);
            }
            catch (LogixSdkException e)
            {
                ConsoleMessage($"Unable to set commpath to '{commPath}'.", "ERROR");
                Console.WriteLine(e.Message);
            }

            try
            {
                LogixProject.ControllerMode result = await project.ReadControllerModeAsync();
                switch (result)
                {
                    case LogixProject.ControllerMode.Faulted:
                        return "FAULTED";
                    case LogixProject.ControllerMode.Program:
                        return "PROGRAM";
                    case LogixProject.ControllerMode.Run:
                        return "RUN";
                    case LogixProject.ControllerMode.Test:
                        return "TEST";
                    default:
                        throw new ArgumentOutOfRangeException("Controller mode is unrecognized.");
                }
            }
            catch (LogixSdkException e)
            {
                ConsoleMessage("Unable to read controller mode.", "ERROR");
                Console.WriteLine(e.Message);
            }

            return "";
        }

        /// <summary>
        /// Asynchronously change the controller mode to either Program, Run, or Test mode.
        /// </summary>
        /// <param name="commPath">The controller communication path.</param>
        /// <param name="mode">The controller mode to switch to.</param>
        /// <param name="project">An instance of the LogixProject class.</param>
        /// <returns>A Task that changes the controller mode.</returns>
        public static async Task ChangeControllerMode_Async(string commPath, string mode, LogixProject project)
        {
            mode = mode.ToUpper().Trim();

            var requestedControllerMode = default(LogixProject.RequestedControllerMode);
            if (mode == "PROGRAM")
            {
                requestedControllerMode = LogixProject.RequestedControllerMode.Program;
            }
            else if (mode == "RUN")
            {
                requestedControllerMode = LogixProject.RequestedControllerMode.Run;
            }
            else if (mode == "TEST")
            {
                requestedControllerMode = LogixProject.RequestedControllerMode.Test;
            }
            else
            {
                ConsoleMessage($"Mode '{mode}' is not supported.", "ERROR");
            }

            try
            {
                await project.SetCommunicationsPathAsync(commPath);
            }
            catch (LogixSdkException e)
            {
                ConsoleMessage($"Unable to set communication path to '{commPath}'.", "ERROR");
                Console.WriteLine(e.Message);
            }

            try
            {
                await project.ChangeControllerModeAsync(requestedControllerMode);
            }
            catch (LogixSdkException e)
            {
                ConsoleMessage($"Unable to set mode. Requested mode was '{mode}'.", "ERROR");
                Console.WriteLine(e.Message);
            }
        }

        /// <summary>
        /// Asynchronously download to the specified controller.
        /// </summary>
        /// <param name="commPath">The controller communication path.</param>
        /// <param name="project">An instance of the LogixProject class.</param>
        /// <returns>An Task that downloads to the specified controller.</returns>
        public static async Task DownloadProject_Async(string commPath, LogixProject project)
        {
            try
            {
                await project.SetCommunicationsPathAsync(commPath);
            }
            catch (LogixSdkException e)
            {
                ConsoleMessage($"Unable to set communication path to '{commPath}'.", "ERROR");
                Console.WriteLine(e.Message);
            }

            try
            {
                LogixProject.ControllerMode controllerMode = await project.ReadControllerModeAsync();
                if (controllerMode != LogixProject.ControllerMode.Program)
                {
                    ConsoleMessage($"Controller mode is {controllerMode}. Downloading is possible only if the controller is in 'Program' mode.", "ERROR");
                }
            }
            catch (LogixSdkException e)
            {
                ConsoleMessage("Unable to read ControllerMode.", "ERROR");
                Console.WriteLine(e.Message);
            }

            try
            {
                await project.DownloadAsync();
            }
            catch (LogixSdkException e)
            {
                ConsoleMessage("Unable to download.", "ERROR");
                Console.WriteLine(e.Message);
            }

            // Download modifies the project.
            // Without saving, if used file will be opened again, commands which need correlation
            // between program in the controller and opened project like LoadImageFromSDCard or StoreImageOnSDCard
            // may not be able to succeed because project in the controller won't match opened project.
            try
            {
                await project.SaveAsync();
            }
            catch (LogixSdkException e)
            {
                ConsoleMessage("Unable to save project.", "ERROR");
                Console.WriteLine(e.Message);
            }
        }
        #endregion

        #region METHODS: helper methods
        /// <summary>
        /// Method to replace the string representation of a tag data type with the LDSDK provided DataType enumerator.
        /// </summary>
        /// <param name="dataType">The name of the data type to be returned.</param>
        /// <returns>The LDSDK provided DataType enumerator</returns>
        /// <exception cref="ArgumentException"></exception>
        public static DataType GetDataType(string dataType)
        {
            DataType type;
            switch (dataType)
            {
                case "BOOL":
                    type = DataType.BOOL;
                    break;
                case "SINT":
                    type = DataType.SINT;
                    break;
                case "INT":
                    type = DataType.INT;
                    break;
                case "DINT":
                    type = DataType.DINT;
                    break;
                case "REAL":
                    type = DataType.REAL;
                    break;
                case "LINT":
                    type = DataType.LINT;
                    break;
                case "STRING":
                    type = DataType.STRING;
                    break;
                default:
                    ConsoleMessage($"Data type '{dataType}' not supported.", "ERROR");
                    throw new ArgumentException();
            }
            return type;
        }

        /// <summary>
        /// Get the contents within the right-most brackets and apostrophes of the input XPath.<br/>
        /// </summary>
        /// <param name="XPath">
        /// The XPath (tag path) specifying the tag's scope and location in the Studio 5000 Logix Designer project.<br/>
        /// The XPath for a tag is based on the XML filetype (L5X) encapsulation of elements.
        /// </param>
        /// <returns>
        /// The contents of an XPath.<br/>
        /// Ex.) Controller/Tags/Tag[@Name='AT_ClearFault'] returns AT_ClearFault
        /// </returns>
        public static string? GetNameFromXPath(string XPath)
        {
            string pattern = @"'([^']*)'";
            MatchCollection matches = Regex.Matches(XPath, pattern);

            if (matches.Count > 0)
                return matches[matches.Count - 1].Groups[1].Value;
            else
            {
                ConsoleMessage($"Tag path '{XPath}' does not contain an [@Name=''] or [@Class=''] region.", "ERROR");
                return null;
            }
        }
        #endregion
    }
}
