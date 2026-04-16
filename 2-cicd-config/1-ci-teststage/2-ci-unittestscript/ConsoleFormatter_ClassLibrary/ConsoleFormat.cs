// ------------------------------------------------------------------------------------------------------------------------------------------------------------
//
// FileName:    ConsoleFormat.cs
// FileType:    Visual C# Source File
// Author:      Rockwell Automation
// Created:     2024
// Description: This class provides methods to write standardized, custom messages to the console.
//
// ------------------------------------------------------------------------------------------------------------------------------------------------------------

using System.Text;

namespace ConsoleFormatter_ClassLibrary
{
    /// <summary>
    /// Class containing custom methods for printing messages to the console for CI/CD execution.
    /// </summary>
    public class ConsoleFormatter
    {
        // Static variable for the character length limit of each line printed to the console.
        public static readonly int consoleCharLengthLimit = 110;

        /// <summary>
        /// Standardized method to print messages of varying categories to the console.
        /// </summary>
        /// <param name="messageContents">The contents of the message to be written to the console.</param>
        /// <param name="messageCategory">
        /// The name of the message category. Options include:<br/>
        /// 1. 'ERROR', 'FAILURE', 'FAIL'<br/>
        /// 2. 'SUCCESS', 'PASS'<br/>
        /// 3. 'STATUS'<br/>
        /// 4. 'NEWSECTION'<br/>
        /// 5. (no category)
        /// </param>
        /// <param name="newLineForSection">
        /// A boolean input that determines whether to space a new section with the characters '---'.<br/>
        /// (Note: only applicable if messageCateogry = "NEWSECTION")
        /// </param>
        public static void ConsoleMessage(string messageContents, string messageCategory = "", bool newLineForSection = true)
        {
            messageCategory = messageCategory.ToUpper().Trim();

            if ((messageCategory == "ERROR") || (messageCategory == "FAILURE") || (messageCategory == "FAIL"))
            {
                messageCategory = messageCategory.PadLeft(9, ' ') + ": ";
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write(messageCategory);
                Console.ForegroundColor = ConsoleColor.Gray;
            }
            else if ((messageCategory == "SUCCESS") || (messageCategory == "PASS"))
            {
                messageCategory = messageCategory.PadLeft(9, ' ') + ": ";
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write(messageCategory);
                Console.ForegroundColor = ConsoleColor.Gray;
            }
            else if (messageCategory == "STATUS")
            {
                messageCategory = messageCategory.PadLeft(9, ' ') + ": ";
                Console.Write(messageCategory);
            }
            else if (messageCategory == "NEWSECTION")
            {
                if (newLineForSection)
                {
                    Console.Write($"---\n[{DateTime.Now.ToString("HH:mm:ss")}] ");
                }
                else
                {
                    Console.Write($"[{DateTime.Now.ToString("HH:mm:ss")}] ");
                }
            }
            else
            {
                messageCategory = messageCategory.PadLeft(9, ' ') + "  ";
                Console.Write(messageCategory);
            }

            messageContents = WrapText(messageContents, 11, consoleCharLengthLimit);
            Console.WriteLine(messageContents);
        }

        /// <summary>
        /// Modify the input string to wrap the text to the next line after a certain length.<br/>
        /// The input string is seperated per word and then each line is incrementally added to per word.<br/>
        /// Start a new line when the character count of a line exceeds the specified line limit.
        /// </summary>
        /// <param name="inputString">The input string to be wrapped.</param>
        /// <param name="indentLength">An integer that defines the length of the characters in the indent starting each new line.</param>
        /// <param name="lineLimit">An integer that defines the maximum number of characters per line before a new line is created.</param>
        /// <returns>A modified string that wraps to the next line after a specified length of characters.</returns>
        public static string WrapText(string inputString, int indentLength, int lineLimit)
        {
            // Variables containing formatting information:
            StringBuilder newSentence = new StringBuilder(); // The properly formatted string to be returned.
            string[] words = inputString.Split(' ');         // An array where each element contains each word in an input string. 
            string indent = new string(' ', indentLength);   // An empty string to be used for indenting.
            string line = "";                                // The variable that will be modified and appended to the returned StringBuilder for each line.

            // Variables informing formatting logic:
            bool newLongWord = true;
            int numberOfNewLines = 0;
            int numberOfSplitWords = 0;
            int indentedLineLimit = lineLimit - indentLength;

            // Cycle through each word in the input string.
            foreach (string word in words)
            {
                // The word (short or long) has any excess spaces removed. 
                string trimmedWord = word.Trim();
                //Console.WriteLine("trimmedWord: " + trimmedWord);

                // Required for "Long Word Splitting" Logic: This variable is used to wrap long words at the indentLength specified with indenting.
                int partLengthLimit = lineLimit - (indentLength + line.Length);

                // Required for "Long Word Splitting" Logic: The # of long words determine how a long word component is added to the console.
                // Long words for this method are defined as words that are above the character number of line limit minus indent length.
                if (trimmedWord.Length >= partLengthLimit)
                    numberOfSplitWords++;

                // "Long Word Splitting" Logic
                // If the word is longer than the line limit # of characters, split it & wrap to the next line keeping indents.
                while ((trimmedWord.Length > partLengthLimit) && (trimmedWord.Length > 15))
                {
                    string part = trimmedWord.Substring(0, partLengthLimit); // A peice of the long word to add to the existing line. 
                    trimmedWord = trimmedWord.Substring(partLengthLimit);    // The long word part is removed from trimmedWord.

                    // Long Word Scenario 1: This should only ever run once the first time a long word goes through the while loop.
                    if (((numberOfSplitWords == 1) || (numberOfNewLines == 0)) && (newLongWord))
                    {
                        newSentence.AppendLine(line + part);         // Add line & part to return string. No indent b/c either the long word starts the message
                                                                     // or because the long word part gets added to the current line that already has words.
                        line = "";                                   // Reset the line string.
                        numberOfNewLines++;                          // Count up for number of new lines.
                        newLongWord = false;                         // Lock this if statement (Scenario 1) from being run again.
                        partLengthLimit = indentedLineLimit;
                    }
                    // Long Word Scenario 2: All other subsequent lines with long words (or long word components) need to be indented.
                    else
                    {
                        newSentence.AppendLine(indent + line + part);  // Add indented current line with part. (line could be 0 chars if part is long enough)
                        line = "";                                     // Reset the line string.
                        numberOfNewLines++;                            // Count up for number of new lines.
                        partLengthLimit = indentedLineLimit;
                    }
                }

                // Required for "Long Word Splitting" Logic: Determines how a long word component is added to the console.
                newLongWord = true;

                // "Adding Line" Logic
                // Check if the current line plus the next word (or the remaining part of a long word) exceeds the line limit (accounting for indenting).
                if ((line + trimmedWord).Length > indentedLineLimit)
                {
                    // Line Scenario 1: If not the first line, add indented current line to return string. 
                    if (numberOfNewLines > 0)
                    {
                        newSentence.AppendLine(indent + line.TrimEnd());
                    }
                    // Line Scenario 2: If the first line, add the current line without indents to return string.
                    else
                    {
                        newSentence.AppendLine(line.TrimEnd());
                    }
                    line = "";           // Reset the line string.
                    numberOfNewLines++;  // Count up for number of new lines.
                }

                // Add the word (or the remaining part of a long word) to the current line.
                line += trimmedWord + " ";
            }

            // Same as "Adding Line" Logic where the line contents are the remaining input string contents under the line limit. 
            if (line.Length > 0)
            {
                if (numberOfNewLines > 0)
                    newSentence.Append(indent + line.TrimEnd());
                else
                    newSentence.Append(line.TrimEnd());
            }

            return newSentence.ToString();
        }

        /// <summary>
        /// Return a string numerical variable in a specified data type.
        /// </summary>
        /// <param name="number">The string formatted number to be returned in the specified data type.</param>
        /// <param name="dataType">The data type to return the variable as.</param>
        /// <returns>A variable of specified data type.</returns>
        public static object? GetVariableByDataType(string? number, string dataType)
        {
            if ((number == "") || (string.IsNullOrEmpty(number)))
                return null;
            else if (dataType == "BOOL")
                return int.Parse(number);
            else if (dataType == "SINT")
                return sbyte.Parse(number);
            else if (dataType == "INT")
                return int.Parse(number);
            else if (dataType == "DINT")
                return double.Parse(number);
            else if (dataType == "LINT")
                return long.Parse(number);
            else if (dataType == "REAL")
                return float.Parse(number);
            else if (dataType == "STRING")
                return number;
            else
            {
                ConsoleMessage($"Input data type '{dataType}' unsupported.", "ERROR");
                return null;
            }
        }

        /// <summary>
        /// Create a banner used to identify the portion of the test being executed and write it to console.
        /// </summary>
        /// <param name="bannerName">The name displayed in the console banner.</param>
        public static void CreateBanner(string bannerName)
        {
            string final_banner = "-=[" + bannerName + "]=---";
            final_banner = final_banner.PadLeft(consoleCharLengthLimit, '-');
            Console.WriteLine(final_banner);
        }
    }
}
