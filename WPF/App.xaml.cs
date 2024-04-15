// Copyright (c) 2024 J.Jarrard / JJFX
// KGuiX is released under the terms of the AGPLv3 or higher.

// #define DEBUG_CON    // Enables attached debug console
using KGuiX.Interop;
using KGuiX.ViewModels;

using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Reflection;
using System.Configuration;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;

using Microsoft.Win32;  // Open file dialog

namespace KGuiX
{
    public partial class App : Application
    {
#if DEBUG_CON
        Process commandProcess;
        StreamWriter inputWriter;
        StreamReader outputReader;
        StreamReader errorReader;
#endif

        /// <summary>
        /// <br>Application startup events.</br>
        /// <br>Validate required files and attach console for optional command line help.</br>
        /// <br>Send any included startup arguments to ViewModel.</br>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void App_OnStartup(object sender, StartupEventArgs e)
        {
            string requiredFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ramtest.dll");

            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CrashHandler);

            // Check for console help argument
            if (e.Args.Any(s => new[] {"HELP", "H", "?"}.Contains(s.Trim('-').ToUpper())))
            {
                ShowConsoleHelp();  // Display command help in console window and close application
                return;
            }

#if DEBUG_CON
            OpenDebugConsole();
#endif

            using (var mutex = new Mutex(true, "Local\\KGuiXRamTest"))
            {
                if (!mutex.WaitOne(0, false))   // Tests if mutex is owned by a local thread
                {
                    // Broadcast message to activate the window of existing instance
                    Interops.PostMessage((IntPtr)Interops.HWND_BROADCAST, Interops.WM_SHOWME, IntPtr.Zero, IntPtr.Zero);
                    Current.Shutdown();
                    return;
                }

                RequiredFileDialog(requiredFilePath, "RAM Test");       // Checks for required library

                UserConfigBackup();                                     // Validate and backup user config file

                if (!KGuiX.Properties.Settings.Default.UiIsDarkTheme)   // Sets the saved UI theme
                {
                    Resources.MergedDictionaries.Clear();
                    Resources.MergedDictionaries.Add(
                        new ResourceDictionary() {Source = new Uri("Themes/Light.xaml", UriKind.Relative)}
                    );
                }

                // Do not proceed unless required file exists in root directory

                MainWindow mainWindow = new MainWindow();

                mainWindow.DataContext = new AppViewModel(e.Args);
                mainWindow.ThemeSwitched += MainWindow_ThemeSwitched;
                mainWindow.ShowDialog();
            }
        }

        /// <summary>
        /// Load theme resources on the <see cref="ThemeSwitched"/> event of the main window.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void MainWindow_ThemeSwitched(object? sender, EventArgs e)
        {
            // Get current theme value from user property setting
            bool isDarkTheme = KGuiX.Properties.Settings.Default.UiIsDarkTheme;

            ResourceDictionary themeDictionary = new ResourceDictionary();

            themeDictionary.Source = new Uri(!isDarkTheme ? "Themes/Light.xaml" : "Themes/Dark.xaml", UriKind.Relative);
            Resources.MergedDictionaries.Clear();
            Resources.MergedDictionaries.Add(themeDictionary);
        }

        /// <summary>
        /// Write unhandled exceptions to debug log.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        static void CrashHandler(object sender, UnhandledExceptionEventArgs args)
        {
            string debugLog = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "debug.log");
            Exception ex = (Exception) args.ExceptionObject;

            File.AppendAllText(debugLog, String.Format("{0}" +
                $"[{DateTime.Now}]\r\n" +
                $"{Assembly.GetExecutingAssembly().FullName}\r\n" +     // App info
                $"{Environment.OSVersion}\r\n" +                        // OS version
                $"{typeof(AppDomain).Assembly.FullName}\r\n\n{ex}",     // Runtime and exception info
                File.Exists(debugLog) ? "\r\n\n" : ""));
        }

        /// <summary>
        /// <br>Open file dialog to select missing file then copy to root directory.</br>
        /// <br>Close application if file is invalid or unable to copy.</br>
        /// </summary>
        /// <param name="filePath">Path to the required file.</param>
        /// <param name="fileInfo">File description to verify.</param>
        void RequiredFileDialog(string filePath, string fileInfo)
        {
            string filename = Path.GetFileName(filePath);
            bool overwriteFile = false;

            if (File.Exists(filePath))
            {
                if (FileVersionInfo.GetVersionInfo(filePath).FileDescription != fileInfo)   // Validates file description
                    overwriteFile = true;
                else
                    return;     // Required file exists
            }

            try
            {
                if (MessageBox.Show(
                    $"    Copy of '{filename}' licensed to the user required in KGuiX directory.\n\n" +
                     "                                   Press OK to Select File Location",
                     "KGuiX Required File", MessageBoxButton.OKCancel) != MessageBoxResult.OK)
                {
                    throw new Exception("Missing File");
                }

                Microsoft.Win32.OpenFileDialog openDialog = new Microsoft.Win32.OpenFileDialog
                {
                    Title = $"Locate Required File: {filename}",
                    InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    FileName = $"{Path.GetFileNameWithoutExtension(filename)}", DefaultExt = ".dll",
                    Filter = "Dynamic Link Libraries (*.dll)|*.dll"
                };

                if (openDialog.ShowDialog() != true)
                    throw new Exception("Missing File");                // No File Selected

                if (FileVersionInfo.GetVersionInfo(openDialog.FileName).FileDescription != fileInfo)       // Validates file description
                    throw new Exception("Invalid File Selected");

                if (overwriteFile)
                {
                    if (MessageBox.Show("Overwrite Existing File?", "Filename Exists", MessageBoxButton.YesNo) != MessageBoxResult.Yes)
                        throw new Exception("Missing File");
                }

                File.Copy(openDialog.FileName, filePath, true);         // Copies file to root directory
            }

            catch (Exception ex)
            {
                if (ex.Message != "Missing File")
                    MessageBox.Show($"{ex.Message}", "KGuiX File Error", MessageBoxButton.OK, MessageBoxImage.Error);

                Current.Shutdown();     // Does not proceed
                return;
            }
        }

        /// <summary>
        /// Create a backup of the user config file and restore backup if config is corrupted.
        /// </summary>
        void UserConfigBackup()
        {
            try
            {
                Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.PerUserRoamingAndLocal);
                config.SaveAs(config.FilePath + ".bak", ConfigurationSaveMode.Full, true);      // Creates config backup
            }
            catch (ConfigurationErrorsException ex)
            {
                if (MessageBox.Show(
                    "The user config file is corrupted.\n\n" +
                    "Saved settings may be lost if backup file fails.", "KGuiX Config Error",
                    MessageBoxButton.OKCancel, MessageBoxImage.Warning) != MessageBoxResult.OK)
                {
                    Current.Shutdown();
                    return;
                }

                string configBackup = ex.Filename + ".bak";

                if (File.Exists(ex.Filename))
                {
                    File.Delete(ex.Filename);

                    if (File.Exists(configBackup))
                    {
                        File.Copy(configBackup, ex.Filename, true);

                        try
                        {
                            ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.PerUserRoamingAndLocal);
                        }
                        catch (ConfigurationErrorsException ex2)
                        {
                            Console.WriteLine($"[Exception] User config backup failed: {ex2.Message}");    // DEBUG
                            File.Delete(ex.Filename);
                        }
                    }
                }
                // Restore or create default config file
                KGuiX.Properties.Settings.Default.Reload();
            }
        }

        /// <summary>
        /// Display application command line help in console window.
        /// </summary>
        void ShowConsoleHelp()
        {
            try
            {
                const int ATTACH_PARENT_PROCESS = -1;
                if (AttachConsole(ATTACH_PARENT_PROCESS))
                {
                    // Ensures help fits in current console
                    if (Console.WindowHeight < 50)
                        Console.WindowHeight = 50;
                    if (Console.WindowWidth < 100)
                        Console.WindowWidth = 100;

                    if (Console.BackgroundColor != ConsoleColor.Green)
                        Console.ForegroundColor = ConsoleColor.Green;   // Because green.

                    Console.SetCursorPosition(0, Console.CursorTop);
                    Console.Write(new string(' ', Console.WindowWidth));    // Clears command path
                    Console.SetCursorPosition(0, Console.CursorTop);

                    Console.WriteLine("******************************************************");
                    Console.WriteLine("**            KGuiX Command Line Options            **");
                    Console.WriteLine("******************************************************");
                    Console.WriteLine("KGuiX Command line syntax for RAM test automation. \n");
                    Console.WriteLine("Usage: ");
                    Console.WriteLine("  KGuiX [parameters][switches] \n");
                    Console.WriteLine("Examples:");
                    Console.WriteLine("  KGuiX -size 96 -threads auto -errors 2 -cache 3 --fpu ");
                    Console.WriteLine("  KGuiX -size 28000M -threads 16 -coverage 10000 --no-start --default \n");
                    Console.WriteLine("Parameters: ");
                    Console.WriteLine("  -size <1-99> ................. Test size set as a percentage of free memory. ");
                    Console.WriteLine("        <1-999999>M ............ Test size set in megabytes not exceeding free memory. ");
                    Console.WriteLine("        auto ................... Calculates test size using the default percentage. \n");
                    Console.WriteLine("  -threads <1-64> .............. Number of cpu threads used for the test. ");
                    Console.WriteLine("           auto ................ Sets maximum cpu threads available. \n");
                    Console.WriteLine("  -cache <0-3> ................. The CPU cache mode used for the test. ");
                    Console.WriteLine("                                 0:Disabled, 1:WriteCombine, 2:Default, 3:Enabled \n");
                    Console.WriteLine("  -rng <0-1> ................... The RNG function mode used for the test. ");
                    Console.WriteLine("                                 0:Default, 1:XORWOW \n");
                    Console.WriteLine("  -coverage <0-999999> ......... Maximum percentage of coverage the test will complete. ");
                    Console.WriteLine("                                 Value of '0' will disable max coverage.* \n");
                    Console.WriteLine("  -errors <0-100> .............. Maximum errors allowed until test is stopped. ");
                    Console.WriteLine("                                 Value of '0' will disable stop on error.* \n");
                    Console.WriteLine("  -delay <0-1000> .............. Delay in ms until MaxSpeed starts recording. ");
                    Console.WriteLine("                                 Test history will not save until MaxSpeed is set. \n");
                    Console.WriteLine(" * Limit parameters can also be used as a switch. \n");
                    Console.WriteLine("Switches: ");
                    Console.WriteLine("  --fpu, --fpu:off ..............Enable/Disable floating point workload. ");
                    Console.WriteLine("  --log, --log:off ............. Enable/Disable recording test history. ");
                    Console.WriteLine("  --no-start ................... Do not start test automatically. ");
                    Console.WriteLine("  --default .................... Reset all settings except history to default values. ");
                    Console.WriteLine("  --help, -h, -? ............... Show KGuiX command line help. \n");
                    Console.WriteLine(" * Switches indicate enabled by default but ':on/:off' can be specified. \n");

                    Console.ResetColor();   // Restore console color state
                }
            }
            finally
            {
                FreeConsole();
                Shutdown();
            }

            [DllImport("kernel32")]
            static extern bool AttachConsole(int dwProcessId);
        }


#if DEBUG_CON
        void OpenDebugConsole()
        {
            try
            {
                AllocConsole(); // Creates new console window

                commandProcess = new Process();
                commandProcess.StartInfo.FileName = "cmd.exe";
                commandProcess.StartInfo.UseShellExecute = false;
                commandProcess.StartInfo.RedirectStandardInput = true;  // Input stream sent to console
                commandProcess.StartInfo.RedirectStandardOutput = true; // Output stream of application
                commandProcess.StartInfo.RedirectStandardError = true;  // Error stream of application
                commandProcess.StartInfo.CreateNoWindow = true;

                commandProcess.Start();

                inputWriter = commandProcess.StandardInput;
                inputWriter.AutoFlush = true; // Enables auto-flushing of the input stream

                outputReader = commandProcess.StandardOutput;
                errorReader = commandProcess.StandardError;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Debug Console: {ex.Message}", "KGuiX Exception", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            [DllImport("kernel32")]
            static extern bool AllocConsole();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
            FreeConsole(); // Close the console window
            inputWriter?.Close();
            outputReader?.Close();
            commandProcess?.Close();
        }
#endif

        [DllImport("kernel32")]
        static extern bool FreeConsole();
    }
}