# PSBootstrap

## Introduction

This module has been written to provide a stable base for every PowerShell script project. It has been created because I kept rebuilding almost the same file structure and validation checks for every new project, without ever turning that pattern into something stable enough to actually depend on. With PSBootstrap as its base, it's easy to configure what a script should automatically import and what checks it needs to execute.

The project philosophy is "A script needs to have a reliable way to be initialized and stupidly simple to make that possible".

## Onboarding

To start using PSBootstrap, you can download the module via the Releases tab in Github or via a command in PowerShell:
``` PowerShell
iwr (iwr https://api.github.com/repos/92flash/PSBootstrap/releases/latest | ConvertFrom-Json).assets.browser_download_url -OutFile PSBootstrap.dll
```

To be able to use the module, you need to import the module first.

``` PowerShell
Import-Module .\PSBootstrap.dll
```

To create a new project structure, you can run the `New-BootstrapStructure` cmdlet. This makes sure a proper file structure with the main script exists so that you only have to worry about the script functionality itself

Before running your script, open Bootstrap.xml and configure it for your project. Most sections are enabled by default and expect values. The logging is enabled by default, but needs a variable or else it will throw a terminating exception at the start of the script.

## Available Cmdlets

- Invoke-Bootstrap: Actual orchestrator of the checks and returns the Json domain config
- New-BootstrapStructure: Creates a new file structure into the directory the cmdlet is run from, with which the project can immediately start
- Remove-BootstrapStructure: Removes the created file structure
- Write-Log: Adds a message with date, username, computername, time and type to a log file that is configured in `Bootstrap.xml` under Log.Path
- Clear-Log: Empties the log file
- Get-Log: Convert an existing log to a usable PowerShell object
- Remove-Log: Deletes the entire log file

## Xml Config

The `Bootstrap.xml` file holds all the properties for the script to properly load the environment. Adding functions to the Function section or modules to the Module section allows these required dependencies to be checked and automatically loaded. It also makes sure to add logging, enable/disable verbose or debugging and returns a configuration file for the script to use.

Worth noting is that if the Xml gets duplicated and renamed to `Bootstrap_local.xml`, the config will be used over the normal config file and won't be included in the Git repository. This is especially useful when the script needs to be tested with different values over the production script.

## Bootstrap file structure explained

```
Project
├─ Config                       
│ ├─ properties.json            JSON domain config that lets you control how the script behaves without editing the script's own code
│ └─ properties_schema.json     Validation contract for checking if all the properties in the main Json file are present and the values are correct
├─ Function                     A place where functions can be placed without having to clutter the main script file
│ ├─ Private                    
│ └─ Public                       
├─ Module                       Add custom PowerShell modules that are automatically imported when added to the `Bootstrap.xml` config file and allow for more advanced separation of concerns and scripting
├─ Test                         Add Pester tests to test PowerShell's own code
├─ .gitignore                   Set of excluded files and folders that won't be included by default in the Git repository
├─ Bootstrap.xml                This is where the script properties live and can be changed
├─ Project.ps1                  The main orchestrator PowerShell file, gets named after the main folder
└─ PSBootstrap.dll              The orchestrator module where the cmdlets live in
```

## Logging

Once Invoke-Bootstrap has run and logging is enabled, Write-Log picks up the configured LogPath automatically so there is no need to pass it on every call.

Write-Log can be used in the following way:
``` PowerShell
Write-Log <Message> [-Type <Type>] [-ShowShellOutput]
PS C:\users\user> Write-Log "Add this to the log" -Type Information -ShowShellOutput
```

Types that can be used: Success, Information, Warning, Attention, Error, Fatal, Verbose and Debug.

### Preference action

Write-Log is respecting the default PowerShell preference variables like `$ErrorActionPreference` and `$WarningPreference` when using the -ShowShellOutput parameter or ShowShellOutput config value. That means that when `$ErrorActionPreference` is set to SilentlyContinue, the message won't be written to the shell, even if ShowShellOutput is set to true.

For Verbose and Debug, the same applies. But when writing to the log file, if the preference action is set to SilentlyContinue or Ignore, the message also won't be written to the log file. This is to prevent the log being cluttered too much when the script is in production.

Worth noting is that Information, Success and Attention don't respect the preference action variables because, unlike Warning/Error/Verbose/Debug, they aren't backed by a real PowerShell stream so there's no corresponding preference variable for them to respect.

## Git ignored by default

The following files and folders are ignored by default, but can be added if required by editing the `.gitignore` file.
- All the Json files except the schema files as these are the contracts to which the script validates if every value is present and is in the correct state
- Everything in the Module folder as the modules have their own development cycle, but adding a version number to the module manifest file and the `Bootstrap.xml` Module.Required section, makes sure that the new module is tested with the main script
- The `PSBootstrap.dll` is excluded because the module needs to be easily upgradable

## Development decisions

### ILRepack

In this project I chose to use ILRepack to include the necessary dependencies all in one file. I found this important because PSBootstrap.dll needs to be understood easily and also movable between projects and folders. Also adding different dependencies added too much complexity, so the only way to make sure that the onboarding is as smooth as possible is to include the dependencies in the module itself.

## Project file structure

```
PSBootstrap
├─ src                              
│ ├─ Resources                      Resources the module needs to set the files for New-BootstrapStructure
│ │ ├─ .gitignore                   Here do the exclusions for Git live
│ │ ├─ Bootstrap.xml                The Script Config to control the actions of the script
│ │ ├─ Main.ps1                     PowerShell script-template where the new script will be based on
│ │ └─ properties_schema.json       An empty Json Schema file
│ ├─ Service                        Actual implementation
│ │ ├─ BootstrapService.cs          Invoke-Bootstrap actions like checking and importing functions
│ │ ├─ ConfigService.cs             Actions for the domain Json config
│ │ ├─ FileStructureService.cs      Create a new file structure for New-BootstrapStructure
│ │ ├─ LogService.cs                Logging implementation
│ │ └─ ScriptConfigService.cs       Convert script config to a usable object
│ ├─ Shared
│ │ ├─ Context
│ │ │ └─ BootstrapContext.cs        Global context so different modules can use this context
│ │ ├─ Entity
│ │ │ ├─ Log.cs                     Entity form of a log entry
│ │ │ └─ ScriptConfig.cs            Usable entity to set and get the XML script properties
│ │ ├─ Enum
│ │ │ ├─ LogStreamState.cs          State of the log file like if it's already been opened
│ │ │ └─ LogType.cs                 Types like Information and Error that can individually be set by the user
│ │ ├─ Exception
│ │ │ └─ BootstrapException.cs      Custom Bootstrap exception that can be individually caught by a try-catch block
│ │ ├─ Interface
│ │ │ ├─ IBootstrapService.cs       Contract for the Bootstrap service
│ │ │ ├─ IConfigService.cs          Contract for the Config service
│ │ │ ├─ IFileStructureService.cs   Contract for the FileStructure service
│ │ │ ├─ ILogService.cs             Contract for the Log service
│ │ │ └─ IScriptConfigService.cs    Contract for the ScriptConfig service
│ │ ├─ Template
│ │ │ ├─ FileStructureTemplate.cs   Template for how the structure of New-BootstrapStructure should look like
│ │ │ └─ LogTemplate.cs             Template for how the log should look like
│ │ ├─ Utility
│ │ │ └─ ShellUtil.cs               Useful utilities like Write to host with a color
│ │ └─ Value object
│ │ │ ├─ CheckFile.cs               Extendable Value Object for checking a file
│ │ │ ├─ CheckFolder.cs             Extendable Value Object for checking a folder
│ │ │ ├─ ConfigPath.cs              Value Object for checking the config path
│ │ │ ├─ FileSystemEntry.cs         File system entry name
│ │ │ ├─ FunctionPath.cs            Value Object for checking the function path
│ │ │ ├─ JsonConfigPath.cs          Value Object for checking the Json path
│ │ │ ├─ JsonSchemaPath.cs          Value Object for checking the Json Schema path
│ │ │ ├─ LogPath.cs                 Value Object for checking the log path
│ │ │ ├─ ModulePath.cs              Value Object for checking the module path
│ │ │ ├─ RequiredModule.cs          A bundled required module name and its optional required version
│ │ │ ├─ TemplateFile.cs            File system abstraction for how a file looks like
│ │ │ ├─ TemplateFolder.cs          File system abstraction for how a folder looks like
│ │ │ └─ XmlPath.cs                 Value Object for checking the XML path
│ ├─ ClearLog.cs                    Cmdlet implementation for clearing a log file
│ ├─ GetLog.cs                      Cmdlet implementation for getting a log file
│ ├─ InvokeBootstrap.cs             Cmdlet for fail-fast base to setup script dependencies and logging
│ ├─ NewBootstrapStructure.cs       Cmdlet for creating a new project structure
│ ├─ RemoveBootstrapStructure.cs    Cmdlet for removing the project structure
│ ├─ RemoveLog.cs                   Cmdlet implementation for removing a log file
│ └─ WriteLog.cs                    Cmdlet implementation for writing an entry to a log file
├─ test
│ ├─ ConfigTest.cs                  Test the Json domain config implementation
│ ├─ LogTest.cs                     Test the logging implementation
│ └─ ScriptConigTest.cs             Test the XML script config implementation
└─ README.md                        User documentation
```