This project provides a wrapper for the Windows Task Scheduler. It aggregates the multiple versions, provides an editor and allows for localization.  

**NuGet**  
This project's assemblies are available via NuGet.

*   Main Library: [Task Scheduler Managed Wrapper](http://www.nuget.org/packages/TaskScheduler/) (TaskScheduler)
*   UI Library: [Task Scheduler Managed Wrapper UI Library](http://www.nuget.org/packages/TaskSchedulerEditor/) (TaskSchedulerEditor)

**Main Library**  
Microsoft introduced version 2.0 (internally version 1.2) with a completely new object model with Windows Vista. The managed assembly closely resembles the new object model, but allows the 1.0 (internally version 1.1) COM objects to be manipulated. It will automatically choose the most recent version of the library found on the host system (up through 1.4). Core features include:

*   Separate, functionally identical, libraries for .NET 2.0 and 4.0\.
*   Unlike the base library, this wrapper helps to create and view tasks up and down stream.
*   Written in C#, but works with any .NET language including scripting languages (e.g. PowerShell).
*   Supports almost all V2 native properties, even under V1 systems.
*   Maintain EmailAction and ShowMessageAction using PowerShell scripts for systems after Win8 where these actions have been deprecated
*   Supports all action types (not just ExecAction) on V1 systems (XP/WS2003) and earlier (if PowerShell is installed).
*   Supports multiple actions on V1 systems (XP/WS2003). Native library only supports a single action.
*   Supports serialization to XML for both 1.0 and 2.0 tasks (base library only supports 2.0)
*   Supports task validation for targeted version.
*   Supports secure task reading and maintenance.
*   Fluent methods for task creation.
*   Cron syntax for trigger creation.
*   Supports reading "custom" triggers under Win8 and later.
*   Numerous work-arounds and checks to compensate for base library shortcomings.

The project supports a number of languages and, upon request, is ready to support others. The currently supported languages include: English, Spanish, Italian, French, Chinese (Simplified), German.  

The project is based on work the originator started in January 2002 with the 1.0 library that is currently hosted on [CodeProject](http://www.codeproject.com/KB/system/taskschedulerlibrary.aspx).  

**UI Library**  
There is a second library that includes localized and localizable GUI editors and a wizard for tasks which mimic the ones in Vista and later and adds optional pages for new properties. Following is the list of available UI controls:

*   A DropDownCheckList control that is very useful for selecting flag type enumerations.
*   A FullDateTimePicker control which allows both date and time selection in a single control.
*   A CredentialsDialog class for prompting for a password which wraps the Windows API.
*   Simplified classes for pulling events from the system event log.
*   Action editor dialog
*   Trigger editor dialog
*   Task editor dialog and tabbed control
*   Event viewer dialog
*   Task / task folder selection dialog
*   Task history viewer
*   Task run-times viewer
*   Task creation wizard
*   Task service connection dialog

**COM Handler Project Template**  
If you are writing your own custom Task Scheduler COM Handler based on the <span class="codeInline">ITaskHandler</span> interface, there is a [project template](http://taskscheduler.codeplex.com/releases/view/142773) available that will stub out the majority of the work to create it in C# and a [sample](http://taskscheduler.codeplex.com/releases/view/29938) to show a complete one. If running your own code, this model provides much better interaction, control and memory management than running an executable.  

**Sample Code**  
There is a help file included with the download that provides an overview of the various classes. There are numerous examples under the "Documentation" tab.  

You can perform a number of actions in a single line of code:  

<div style="color:Black; background-color:White">

<pre><span style="color:Green">// Run a program every day on the local machine</span>
TaskService.Instance.AddTask(<span style="color:#A31515">"Test"</span>, QuickTriggerType.Daily, <span style="color:#A31515">"myprogram.exe"</span>, <span style="color:#A31515">"-a arg"</span>);

<span style="color:Green">// Run a custom COM handler on the last day of every month</span>
TaskService.Instance.AddTask(<span style="color:#A31515">"Test"</span>, <span style="color:Blue">new</span> MonthlyTrigger { RunOnLastDayOfMonth = <span style="color:Blue">true</span> }, 
    <span style="color:Blue">new</span> ComHandlerAction(<span style="color:Blue">new</span> Guid(<span style="color:#A31515">"{CE7D4428-8A77-4c5d-8A13-5CAB5D1EC734}"</span>)));
</pre>

</div>

For many more options, use the library classes to build a complex task. Below is a brief example of how to use the library from C#.  

<div style="color:Black; background-color:White">

<pre><span style="color:Blue">using</span> System;
<span style="color:Blue">using</span> Microsoft.Win32.TaskScheduler;

<span style="color:Blue">class</span> Program
{
   <span style="color:Blue">static</span> <span style="color:Blue">void</span> Main(<span style="color:Blue">string</span>[] args)
   {
      <span style="color:Green">// Get the service on the remote machine</span>
      <span style="color:Blue">using</span> (TaskService ts = <span style="color:Blue">new</span> TaskService(<span style="color:#A31515">@"\\RemoteServer"</span>))
      {
         <span style="color:Green">// Create a new task definition and assign properties</span>
         TaskDefinition td = ts.NewTask();
         td.RegistrationInfo.Description = <span style="color:#A31515">"Does something"</span>;

         <span style="color:Green">// Create a trigger that will fire the task at this time every other day</span>
         td.Triggers.Add(<span style="color:Blue">new</span> DailyTrigger { DaysInterval = 2 });

         <span style="color:Green">// Create an action that will launch Notepad whenever the trigger fires</span>
         td.Actions.Add(<span style="color:Blue">new</span> ExecAction(<span style="color:#A31515">"notepad.exe"</span>, <span style="color:#A31515">"c:\\test.log"</span>, <span style="color:Blue">null</span>));

         <span style="color:Green">// Register the task in the root folder</span>
         ts.RootFolder.RegisterTaskDefinition(<span style="color:#A31515">@"Test"</span>, td);
      }
   }
}
</pre>

For extended examples on how to the use the library, look in the source code area or look at the [Examples Page](http://taskscheduler.codeplex.com/wikipage?title=Examples&referringTitle=Home). The library closely follows the Task Scheduler 2.0 Scripting classes. Microsoft has some examples on [MSDN](http://msdn2.microsoft.com/en-us/library/aa384006(VS.85).aspx) around it that may further help you understand how to use this library.  
</div>
