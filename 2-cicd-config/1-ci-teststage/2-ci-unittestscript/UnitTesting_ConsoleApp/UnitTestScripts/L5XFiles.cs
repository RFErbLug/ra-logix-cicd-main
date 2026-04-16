// ------------------------------------------------------------------------------------------------------------------------------------------------------------
//
// FileName:    L5XFiles.cs
// FileType:    Visual C# Source File
// Author:      Rockwell Automation
// Created:     2024
// Description: This class provides methods to create the full L5X contents of a Studio 5000 Logix Designer application. This application will have components
//              imported to it depending on the unit test target object For example, when testing a program or an AOI, the target L5X components will be 
//              programmatically imported to the Studio 5000 Logix Designer application shown below.
//
// ------------------------------------------------------------------------------------------------------------------------------------------------------------

namespace UnitTesting_ConsoleApp.UnitTestScripts
{
    /// <summary>
    /// This class contains the foundational Studio 5000 Logix Designer L5X application contents needed to begin unit testing.
    /// </summary>
    internal class L5XFiles
    {
        /// <summary>
        /// Get the string contents of a full Studio 5000 Logix Designer application in L5X form.<br/>
        /// The application sets up the environment needed to conduct AOI unit testing.<br/>
        /// (application contents: Controller Fault Handler program and 1 task containing 1 program containing 1 routine).
        /// </summary>
        /// <param name="routineName_Cont">The name of the first (main) routine - this routine gets controller fault info and executes the second routine.</param>
        /// <param name="routineName_Event">
		/// The name of the second routine - this routine does not yet exist in this L5X application but will be imported in subsequent test set-up steps.
		/// </param>
        /// <param name="programName_Cont">The name of the program.</param>
        /// <param name="taskName_Cont">The name of the task.</param>
        /// <param name="routineName_FaultHandler">The name of the routine within the 'Controller Fault Handler' in the controller organizer.</param>
        /// <param name="programName_FaultHandler">The name of the program within the 'Controller Fault Handler' in the controller organizer.</param>
        /// <param name="controllerName">The name of the controller to be emulated during unit testing.</param>
        /// <param name="processorType">The type of controller to use during unit testing.</param>
        /// <param name="softwareRevision">The software revision to use during unit testing.</param>
        /// <returns></returns>
        public static string GetFaultHandlingApplicationL5XContents_ForRoutine(string routineName_Cont, string routineName_Event, string programName_Cont,
            string programName_Event, string taskName_Cont, string taskName_Event, string routineName_FaultHandler, string programName_FaultHandler,
            string controllerName, string processorType, string softwareRevision)
        {
            return @"<?xml version=""1.0"" encoding=""UTF-8"" standalone=""yes""?>
<RSLogix5000Content SchemaRevision=""1.0"" SoftwareRevision=""" + softwareRevision + @""" TargetName=""" + controllerName + @""" TargetType=""Controller"" 
ContainsContext=""false"" ExportDate=""" + DateTime.Now.ToString("ddd MMM dd HH:mm:ss yyyy") + @""" ExportOptions=""NoRawData L5KData DecoratedData 
ForceProtectedEncoding AllProjDocTrans"">
	<Controller Use=""Target"" Name=""" + controllerName + @""" ProcessorType=""" + processorType + @""" MajorRev=""" + GetStringPart(softwareRevision, "LEFT")
    + @""" MinorRev=""" + GetStringPart(softwareRevision, "RIGHT") + @""" MajorFaultProgram=""" + programName_FaultHandler + @""" 
	ProjectCreationDate=""Mon Jul 29 14:17:13 2024"" LastModifiedDate=""Wed Jul 31 21:12:30 2024"" SFCExecutionControl=""CurrentActive"" 
	SFCRestartPosition=""MostRecent"" SFCLastScan=""DontScan"" CommPath="""" ProjectSN=""16#0000_0000"" MatchProjectToController=""false"" 
	CanUseRPIFromProducer=""false"" InhibitAutomaticFirmwareUpdate=""0"" PassThroughConfiguration=""EnabledWithAppend"" 
	DownloadProjectDocumentationAndExtendedProperties=""true"" DownloadProjectCustomProperties=""true"" ReportMinorOverflow=""false"" 
	AutoDiagsEnabled=""true"" WebServerEnabled=""false"">
		<RedundancyInfo Enabled=""false"" KeepTestEditsOnSwitchOver=""false""/>
		<Security Code=""0"" ChangesToDetect=""16#ffff_ffff_ffff_ffff""/>
		<SafetyInfo/>
		<DataTypes>
			<DataType Name=""FAULTRECORD"" Family=""NoFamily"" Class=""User"">
				<Members>
					<Member Name=""Time_Low"" DataType=""DINT"" Dimension=""0"" Radix=""Decimal"" Hidden=""false"" ExternalAccess=""Read/Write"">
						<Description>
							<![CDATA[Lower 32 bits of the fault timestamp value]]>
						</Description>
					</Member>
					<Member Name=""Time_High"" DataType=""DINT"" Dimension=""0"" Radix=""Decimal"" Hidden=""false"" ExternalAccess=""Read/Write"">
						<Description>
							<![CDATA[Upper 32 bits of the fault timestamp value]]>
						</Description>
					</Member>
					<Member Name=""Type"" DataType=""INT"" Dimension=""0"" Radix=""Decimal"" Hidden=""false"" ExternalAccess=""Read/Write"">
						<Description>
							<![CDATA[Fault type (program, I/O, and so forth)]]>
						</Description>
					</Member>
					<Member Name=""Code"" DataType=""INT"" Dimension=""0"" Radix=""Decimal"" Hidden=""false"" ExternalAccess=""Read/Write"">
						<Description>
							<![CDATA[Unique code for the fault]]>
						</Description>
					</Member>
					<Member Name=""Info"" DataType=""DINT"" Dimension=""8"" Radix=""Decimal"" Hidden=""false"" ExternalAccess=""Read/Write"">
						<Description>
							<![CDATA[Fault specific information]]>
						</Description>
					</Member>
				</Members>
			</DataType>
		</DataTypes>
		<Modules>
			<Module Name=""Local"" CatalogNumber=""1756-L85E"" Vendor=""1"" ProductType=""14"" ProductCode=""168"" Major=""36"" Minor=""11"" 
			ParentModule=""Local"" ParentModPortId=""1"" Inhibited=""false"" MajorFault=""true"">
				<EKey State=""Disabled""/>
				<Ports>
					<Port Id=""1"" Address=""0"" Type=""ICP"" Upstream=""false"">
						<Bus Size=""17""/>
					</Port>
					<Port Id=""2"" Type=""Ethernet"" Upstream=""false"">
						<Bus/>
					</Port>
				</Ports>
			</Module>
		</Modules>
		<AddOnInstructionDefinitions/>
		<Tags>
			<Tag Name=""AT_ClearFault"" TagType=""Base"" DataType=""BOOL"" Radix=""Decimal"" Constant=""false"" ExternalAccess=""Read/Write"" 
			OpcUaAccess=""None"">
				<Description>
					<![CDATA[Automated Testing -------------------- clear the fault  type & code]]>
				</Description>
				<Data Format=""L5K"">
					<![CDATA[0]]>
				</Data>
				<Data Format=""Decorated"">
					<DataValue DataType=""BOOL"" Radix=""Decimal"" Value=""0""/>
				</Data>
			</Tag>
			<Tag Name=""AT_EnableIn"" TagType=""Base"" DataType=""BOOL"" Radix=""Decimal"" Constant=""false"" ExternalAccess=""Read/Write"" 
			OpcUaAccess=""None"">
				<Description>
					<![CDATA[Automated Testing -------------------- helps set the EnableIn AOI input parameter]]>
				</Description>
				<Data Format=""L5K"">
					<![CDATA[0]]>
				</Data>
				<Data Format=""Decorated"">
					<DataValue DataType=""BOOL"" Radix=""Decimal"" Value=""0""/>
				</Data>
			</Tag>
			<Tag Name=""AT_FaultCode"" TagType=""Base"" DataType=""DINT"" Radix=""Decimal"" Constant=""false"" ExternalAccess=""Read/Write"" 
			OpcUaAccess=""None"">
				<Description>
					<![CDATA[Automated Testing -------------------- contains fault code number]]>
				</Description>
				<Data Format=""L5K"">
					<![CDATA[0]]>
				</Data>
				<Data Format=""Decorated"">
					<DataValue DataType=""DINT"" Radix=""Decimal"" Value=""0""/>
				</Data>
			</Tag>
			<Tag Name=""AT_FaultedIndicator"" TagType=""Base"" DataType=""BOOL"" Radix=""Decimal"" Constant=""false"" ExternalAccess=""Read/Write"" 
			OpcUaAccess=""None"">
				<Description>
					<![CDATA[Automated Testing -------------------- boolean HIGH if controller faulted]]>
				</Description>
				<Data Format=""L5K"">
					<![CDATA[0]]>
				</Data>
				<Data Format=""Decorated"">
					<DataValue DataType=""BOOL"" Radix=""Decimal"" Value=""0""/>
				</Data>
			</Tag>
			<Tag Name=""AT_FaultType"" TagType=""Base"" DataType=""DINT"" Radix=""Decimal"" Constant=""false"" ExternalAccess=""Read/Write"" 
			OpcUaAccess=""None"">
				<Description>
					<![CDATA[Automated Testing -------------------- contains fault type number]]>
				</Description>
				<Data Format=""L5K"">
					<![CDATA[0]]>
				</Data>
				<Data Format=""Decorated"">
					<DataValue DataType=""DINT"" Radix=""Decimal"" Value=""0""/>
				</Data>
			</Tag>
			<Tag Name=""AT_ToggleTest"" TagType=""Base"" DataType=""BOOL"" Radix=""Decimal"" Constant=""false"" ExternalAccess=""Read/Write"" 
			OpcUaAccess=""None"">
				<Description>
					<![CDATA[Automated Testing -------------------- toggle to progress AOI testing by 1 clock cycle]]>
				</Description>
				<Data Format=""L5K"">
					<![CDATA[0]]>
				</Data>
				<Data Format=""Decorated"">
					<DataValue DataType=""BOOL"" Radix=""Decimal"" Value=""0""/>
				</Data>
			</Tag>
			<Tag Name=""major_fault_record"" TagType=""Base"" DataType=""FAULTRECORD"" Constant=""false"" ExternalAccess=""Read/Write"" 
			OpcUaAccess=""None"">
				<Data Format=""L5K"">
					<![CDATA[[0,0,0,0,[0,0,0,0,0,0,0,0]]]]>
				</Data>
				<Data Format=""Decorated"">
					<Structure DataType=""FAULTRECORD"">
						<DataValueMember Name=""Time_Low"" DataType=""DINT"" Radix=""Decimal"" Value=""0""/>
						<DataValueMember Name=""Time_High"" DataType=""DINT"" Radix=""Decimal"" Value=""0""/>
						<DataValueMember Name=""Type"" DataType=""INT"" Radix=""Decimal"" Value=""0""/>
						<DataValueMember Name=""Code"" DataType=""INT"" Radix=""Decimal"" Value=""0""/>
						<ArrayMember Name=""Info"" DataType=""DINT"" Dimensions=""8"" Radix=""Decimal"">
							<Element Index=""[0]"" Value=""0""/>
							<Element Index=""[1]"" Value=""0""/>
							<Element Index=""[2]"" Value=""0""/>
							<Element Index=""[3]"" Value=""0""/>
							<Element Index=""[4]"" Value=""0""/>
							<Element Index=""[5]"" Value=""0""/>
							<Element Index=""[6]"" Value=""0""/>
							<Element Index=""[7]"" Value=""0""/>
						</ArrayMember>
					</Structure>
				</Data>
			</Tag>
			<Tag Name=""ONS"" TagType=""Base"" DataType=""BOOL"" Radix=""Decimal"" Constant=""false"" ExternalAccess=""Read/Write"" OpcUaAccess=""None"">
				<Description>
					<![CDATA[Automated Testing -------------------- make the routine with AOI being tested update only 1 clock cycle]]>
				</Description>
				<Data Format=""L5K"">
					<![CDATA[0]]>
				</Data>
				<Data Format=""Decorated"">
					<DataValue DataType=""BOOL"" Radix=""Decimal"" Value=""0""/>
				</Data>
			</Tag>
		</Tags>
		<Programs>
			<Program Name=""" + programName_Event + @""" TestEdits=""false"" MainRoutineName=""" + routineName_Event + @""" Disabled=""false"" UseAsFolder=""false"">
			<Tags/>
			<Routines>
				<Routine Name=""" + routineName_Event + @""" Type=""RLL"">
					<RLLContent>
						<Rung Number=""0"" Type=""N"">
							<Text>
								<![CDATA[;]]>
							</Text>
						</Rung>
					</RLLContent>
				</Routine>
			</Routines>
			</Program>
			<Program Name=""" + programName_Cont + @""" TestEdits=""false"" MainRoutineName=""" + routineName_Cont + @""" Disabled=""false"" UseAsFolder=""false"">
				<Tags/>
				<Routines>
					<Routine Name=""" + routineName_Cont + @""" Type=""RLL"">
						<RLLContent>
							<Rung Number=""0"" Type=""N"">
								<Comment>
									<![CDATA[AUTOMATED TESTING | FAULT HANDLING
- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - 
Clear the tags storing the fault Type and Code information.]]>
								</Comment>
								<Text>
									<![CDATA[XIC(AT_ClearFault)CLR(AT_FaultType)CLR(AT_FaultCode);]]>
								</Text>
							</Rung>
							<Rung Number=""1"" Type=""N"">
								<Comment>
									<![CDATA[AUTOMATED TESTING | PROGRESS UNIT TESTING FORWARD BY ONE CLOCK CYCLE
- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
Progress the test object under test by one controller clock cycle.]]>
								</Comment>
								<Text>
									<![CDATA[XIC(AT_ToggleTest)ONS(ONS)EVENT(" + taskName_Event + @");]]>
                                </Text>
                            </Rung>
                        </RLLContent>
                    </Routine>
                </Routines>
            </Program>
            <Program Name = """ + programName_FaultHandler + @""" TestEdits = ""false"" MainRoutineName = """ + routineName_FaultHandler + @""" Disabled = ""false""
            UseAsFolder = ""false"">
                <Tags/>
                <Routines>
                    <Routine Name = """ + routineName_FaultHandler + @""" Type = ""RLL"">
                        <RLLContent>
                            <Rung Number = ""0"" Type = ""N"">
                                <Comment>
                                    <![CDATA[FAULT HANDLER
- - - - - - - - - - - - - - -
Get the fault (GSV) and store the fault Type and Code (MOVEs) in two tags that can be programmatically obtained during testing to provide fault information.
Clear the fault Type and Code (SSV) using the major_fault_record tag for the FAULTRECORD user - defined data type.
Keep an indicator of fault state(OTE) for automated testing logic.

(Rung created with the assistance of https://literature.rockwellautomation.com/idc/groups/literature/documents/pm/1756-pm014_-en-p.pdf)
(Fault code spreadsheet link: https://literature.rockwellautomation.com/idc/groups/literature/documents/rd/1756-rd001_-en-p.xlsx)
									]]>
                                </Comment>
                                <Text>
                                    <![CDATA[GSV(Program,THIS,MajorFaultRecord,major_fault_record.Time_Low)[MOVE(major_fault_record.Type,AT_FaultType)
MOVE(major_fault_record.Code,AT_FaultCode) ,CLR(major_fault_record.Type) CLR(major_fault_record.Code)
,SSV(Program,THIS,MajorFaultRecord,major_fault_record.Time_Low) ,OTE(AT_FaultedIndicator) ];]]>
                                </Text>
                            </Rung>
                        </RLLContent>
                    </Routine>
                </Routines>
            </Program>
        </Programs>
        <Tasks>
            <Task Name = """ + taskName_Cont + @""" Type = ""CONTINUOUS"" Priority = ""10"" Watchdog = ""500"" DisableUpdateOutputs = ""false"" InhibitTask = ""false"">
                <ScheduledPrograms>
                    <ScheduledProgram Name = """ + programName_Cont + @"""/>
                </ScheduledPrograms>
            </Task>
			<Task Name=""" + taskName_Event + @""" Type=""EVENT"" Rate=""10"" Priority=""10"" Watchdog=""500"" DisableUpdateOutputs=""true"" InhibitTask=""false"">
				<EventInfo EventTrigger=""EVENT Instruction Only"" EnableTimeout=""false""/>
				<ScheduledPrograms>
					<ScheduledProgram Name=""" + programName_Event + @"""/>
				</ScheduledPrograms>
			</Task>
        </Tasks>
        <CST MasterID = ""0""/>
        <WallClockTime LocalTimeAdjustment = ""0"" TimeZone = ""0""/>
        <Trends/>
        <DataLogs/>
        <TimeSynchronize Priority1 = ""128"" Priority2 = ""128"" PTPEnable = ""false""/>
        <EthernetPorts>
            <EthernetPort Port = ""1"" Label = ""1"" PortEnabled = ""true""/>
        </EthernetPorts>
    </Controller>
</RSLogix5000Content> ";
        }

        /// <summary>
        /// Helper method to split the software revision string as needed.
        /// </summary>
        /// <param name="inputString">The string to be separated.</param>
        /// <param name="side">A string specifying the 'LEFT' or 'RIGHT' side of string to be returned.</param>
        /// <returns>A string that returns the 'LEFT' or 'RIGHT' side of the input string, split at the first period.</returns>
        private static string GetStringPart(string inputString, string side)
        {
            int periodIndex = inputString.IndexOf('.');
            side = side.ToUpper().Trim();

            if (side == "LEFT")
                return inputString.Substring(0, periodIndex);
            else if (side == "RIGHT")
                return inputString.Substring(periodIndex + 1);
            else
                return inputString;
        }
    }
}
