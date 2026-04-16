// ------------------------------------------------------------------------------------------------------------------------------------------------------------
//
// FileName:    LogixDesigner.cs
// FileType:    Visual C# Source File
// Author:      Rockwell Automation
// Created:     2024
// Description: This class provides methods to get/set tags, read/change controller mode, and download to controller using the Studio 5000 Logix Designer SDK.
//
// ------------------------------------------------------------------------------------------------------------------------------------------------------------


using RockwellAutomation.LogixDesigner;
using static ConsoleFormatter_ClassLibrary.ConsoleFormatter;

namespace LogixDesigner_ClassLibrary
{
    /// <summary>
    /// This class utilized the LDSDK to get/set tags, read/change controller mode, and download to controller.
    /// </summary>
    public class LogixDesigner
    {
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
    }
}
