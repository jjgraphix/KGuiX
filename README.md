# KGuiX

Advanced GUI for Karhu RAM Test with additional settings and features.

**This is not a standalone application. A copy of [Karhu](https://www.karhusoftware.com/ramtest) licensed to the to user is required.**

![preview](/.resources/KGuiX-errors+hist.png)

*Credit to [RaINi_](https://github.com/LeagueRaINi/KGuiV2) for the initial concept.*

## Release Notes - V2.1.x
- Added test history with log file
- Command line support *(see KGuiX.exe -help)*
- New dark theme toggle
- Expandable error history in test status
- Test size can be set as percentage of free memory
- Stop on error count option
- Adjustable max speed delay
- Adjustable UI refresh interval
- Windows scheduling priority option
- Keyboard and mousewheel control
- Memory usage visualizer
- Test usage displayed per thread
- Advanced settings shown in status
- Tool tip descriptions for most settings
- Options to save window position and set topmost window
- Custom title bar and app icon
- Automatic backup to restore user config if corrupted
- Various bug fixes and optimizations
- *Experimental:*
  - Button to increase available memory by clearing working set of other active processes.

### Command Line Support
KGuiX does not run as a console application but settings can be applied from the command line for automation.

***Run 'KGuiX.exe --help' to see all available options.***

Examples:

    KGuiX -size 96 -threads auto -coverage 50000 -errors 3
    KGuiX -size 27500M -threads 16 -coverage 0 -errors 1 --fpu
    KGuiX -size 27G -threads auto -cache 3 -errors 1 --default

Test `-size` indicates a percentage by default unless unit `M` or `G` is specified, or value is greater than 99.

Switches are controlled using `:ON` or `:OFF`, with on as the default state. For example, `--fpu:off` would disable the feature if previously enabled.

The `--default` switch applies default test values for all undefined settings. Added features such as maximum test coverage are disabled by default.

For example, the following would ensure only a test size of 90% will be applied:

    KGuiX -size 90 --default

*Test always begins immediately unless '--no-start' is included.*

## Requirements
* .NET 6
* Karhu RAM Test (v1.1.0.0)
* Currently only supported on Windows 10/11

## How to Use
* Follow initial prompt to install .NET 6, if necessary.
* Copy 'ramtest.dll' from Karhu './x64/' directory into KGuiX root directory.
* Run KGuiX.exe or see 'KGuiX.exe -help' for command line syntax.<br>
  *Run as admin recommended for full functionality but is not required.*

## To Build from Source:
* Ensure .NET 6 SDK is also installed.
* Build the project from the master directory: 'dotnet build KGuiX.csproj'
* KGuiX.exe will be compiled in the 'bin' directory under 'Debug'.

## Contact

Feel free to contact me directly at jjfx.contact@gmail.com for additional questions or feedback.

*If you wish to support my work, buying me a coffee on [Ko-Fi](https://ko-fi.com/jjjfx) is appreciated.*
