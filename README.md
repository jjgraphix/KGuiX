# KGuiX

Advanced GUI for Karhu RAM Test with additional settings and features.

**This is not a standalone application. A copy of [Karhu](https://www.karhusoftware.com/ramtest) licensed to the to user is required.**

![preview](/.resources/KGuiX-errors1.png)

*Credit to [RaINi_](https://github.com/LeagueRaINi/KGuiV2) for creating the initial concept.*

## Release Notes - V2.1.x
- Test history with log file
- Command line support *(see KGuiX.exe -help)*
- New dark theme toggle
- Expandable error history
- Test size can be set as percentage of free memory
- Stop on error count option
- Adjustable max speed delay
- Adjustable UI refresh interval
- Windows scheduling priority option
- Keyboard and mousewheel support
- Memory usage visualizer
- Displays test usage per thread
- Advanced settings shown in status
- Tool tip descriptions for most settings
- Options to save window position and set topmost window
- Custom title bar and app icon
- Automatic backup to restore user config if corrupted.
- Various bug fixes and optimizations
- *Experimental:*
  - Button to increase available memory by clearing working set of other active processes.

### Command Line Support
KGuiX does not run as a console application but settings can be applied from the command line for automation.

***Run 'KGuiX.exe --help' for full list of available options.***

Examples:

    KGuiX -size 96 -threads auto -coverage 50000 -errors 2
    KGuiX -size 28000M -threads 16 -coverage 0 -errors 1 --fpu
    KGuiX -size 27G -threads auto -cache 3 -errors 0 --default

Switches are controlled as ':on' or ':off', with ':on' as the default. For example, '--fpu:off' would disable the feature if previously enabled.

The '--default' switch applies default values for all undefined settings. KGuiX features such as maximum test coverage are disabled by default.

For example, the following ensures only a test size of 90% will be applied:

    KGuiX -size 90 --default

*Test always begins immediately when run from command line unless the '--no-start' switch is included.*

## Requirements
* .NET 6
* Karhu RAM Test (v1.1.0.0)

## How to Use
* Follow initial prompt to install .NET 6, if necessary.
* Copy 'ramtest.dll' from Karhu './x64/' directory into KGuiX root directory.
* Run KGuiX.exe or see 'KGuiX.exe -help' for command line syntax.

## To Build from Source:
* Ensure .NET 6 SDK is installed.
* Build the project from the master directory: 'dotnet build KGuiX.csproj'
* KGuiX.exe will be compiled in the 'bin' directory under 'Debug'.

## Contact

You can contact me directly at jjfx.contact@gmail.com for additional questions or feedback.

*If you wish to support my work, buying me a coffee on [Ko-Fi](https://ko-fi.com/jjjfx) is appreciated.*