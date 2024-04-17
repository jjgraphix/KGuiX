// Copyright (c) 2024 J.Jarrard / JJFX
// KGuiX is released under the terms of the AGPLv3 or higher.
using KGuiX.Interop;
using KGuiX.Helpers;

using System;
using System.IO;
using System.Linq;
using System.Diagnostics;
using System.ComponentModel;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

#nullable disable warnings
namespace KGuiX.ViewModels
{
    internal class AppViewModel : BindableBase
    {
        Window _mainWindow = Application.Current.MainWindow;
        string _rootDirectory = AppDomain.CurrentDomain.BaseDirectory;
        string _logFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "history.log");

        long _ramtestStartTick;
        bool _startOnLaunch = false;

        /// <summary>
        /// Used to obtain system memory information from kernel32.dll.
        /// </summary>
        Interops.MemoryStatusEx _memoryStatusEx = new Interops.MemoryStatusEx();

        /// <summary>
        /// The background update timer.
        /// </summary>
        readonly DispatcherTimer _updateTimer;

        /// <summary>
        /// Command for starting the ramtest.
        /// </summary>
        public ICommand RamtestStartCommand { get; }
        /// <summary>
        /// Command for stopping the ramtest.
        /// </summary>
        public ICommand RamtestStopCommand { get; }
        /// <summary>
        /// Command for applying the <see cref="DispatcherTimer"> update interval.
        /// </summary>
        public ICommand SetPollingCommand { get; }
        /// <summary>
        /// Command for clearing all data from <see cref="HistoryLog">.
        /// </summary>
        public ICommand ClearLogCommand { get; }
        /// <summary>
        /// Command for removing last test entry from <see cref="HistoryLog">.
        /// </summary>
        public ICommand ClearLogEntryCommand { get; }
        /// <summary>
        /// Command for clearing all data from <see cref="HistoryLog">.
        /// </summary>
        public ICommand OpenLogCommand { get; }
        /// <summary>
        /// Command for resetting all user setting properties to default values.
        /// </summary>
        public ICommand ResetSettingsCommand { get; }
        /// <summary>
        /// Command for freeing available system memory by clearing working set of running processes.
        /// </summary>
        public ICommand FreeSystemMemoryCommand { get; }

        /// <summary>
        /// Create new instance of <see cref="AppViewModel"/>.
        /// </summary>
        /// <param name="eArgs"></param>
        public AppViewModel(string[]? eArgs)
        {
            LoadUserSettings();

            BackgroundUpdater(null, EventArgs.Empty);   // Initializes updater

            SystemMemoryTotal = _memoryStatusEx.TotalPhys / 1024 / 1024;    // Physical memory size in MB

            _updateTimer = new DispatcherTimer(DispatcherPriority.Background);
            _updateTimer.Interval = TimeSpan.FromMilliseconds((uint)UiPollingRate);
            _updateTimer.Tick += BackgroundUpdater;     // Starts background updater
            _updateTimer.Start();

            RamtestStartCommand = new RelayCommand(StartRamtest, CanStartRamtest);
            RamtestStopCommand = new RelayCommand(StopRamtest, CanStopRamtest);

            SetPollingCommand = new RelayCommand(SetPollingRate, CanSetPolling);
            ClearLogCommand = new RelayCommand(ClearLog, CanClearLog);
            ClearLogEntryCommand = new RelayCommand(ClearLastLogEntry, CanClearLog);
            OpenLogCommand = new RelayCommand(OpenLogFile, CanOpenLog);
            ResetSettingsCommand = new RelayCommand(ResetDefaultSettings);
            FreeSystemMemoryCommand = new RelayCommand(EmptyAllProcesses);

            if (eArgs.Count() > 0)                      // Checks for provided startup arguments
                SetStartupArguments(eArgs);

            RamtestHasStopped = true;
        }

        /// <summary>
        /// Load all user settings.
        /// </summary>
        /// <param name="setDefaults"></param>
        void LoadUserSettings(bool setDefaults = false)
        {
            if (setDefaults)
                Properties.Settings.Default.Reset();
            else
            {
                // Do not reset with default settings
                UiIsDarkTheme = Properties.Settings.Default.UiIsDarkTheme;
                HistoryLog = Properties.Settings.Default.HistoryLog;

                // Update log status if previous ramtest did not complete
                if (!string.IsNullOrEmpty(HistoryLog) && !HistoryLogLines.Last().Contains("STOPPED"))
                    UpdateLog(" ► STOPPED:	! CRASH !");
            }
            // Get ramtest properties
            RamtestMegabytes = RamtestSize = (uint)Properties.Settings.Default.RamtestMegabytes;
            RamtestSizeIsAuto = Properties.Settings.Default.RamtestSizeIsAuto;
            RamtestSizePercent = Properties.Settings.Default.RamtestSizePercent;
            RamtestThreads = Properties.Settings.Default.RamtestThreads;
            RamtestStopOnTaskScope = Properties.Settings.Default.RamtestStopOnTaskScope;
            RamtestTaskScope = Properties.Settings.Default.RamtestTaskScope;
            RamtestStopOnError = Properties.Settings.Default.RamtestStopOnError;
            RamtestBeepOnError = Properties.Settings.Default.RamtestBeepOnError;
            RamtestErrorLimit = Properties.Settings.Default.RamtestErrorLimit;
            RamtestRngMode = (Ramtest.RngMode)Properties.Settings.Default.RamtestRngMode;
            RamtestCpuCacheMode = (Ramtest.CpuCacheMode)Properties.Settings.Default.RamtestCpuCacheMode;
            RamtestStressFpu = Properties.Settings.Default.RamtestStressFpu;
            // Get kguix properties
            UiPollingRate = Properties.Settings.Default.UiPollingRate;
            UiMaxSpeedDelay = Properties.Settings.Default.UiMaxSpeedDelay;
            UiSaveWindowPos = Properties.Settings.Default.UiSaveWindowPos;
            UiHistoryEnabled = Properties.Settings.Default.UiHistoryEnabled;
            UiTopmostEnabled = Properties.Settings.Default.UiTopmostEnabled;
            UiToolTipEnabled = Properties.Settings.Default.UiToolTipEnabled;
        }

        /// <summary>
        /// Reset user settings to default values.
        /// </summary>
        void ResetDefaultSettings(object? param)
        {
            LoadUserSettings(setDefaults: true);
            Properties.Settings.Default.HistoryLog = HistoryLog;  // Preserves history on reset defaults

            SetPollingCommand.Execute(null);                      // Applies the default update interval

        }

        // TODO
        /// <summary>
        /// Get the default value of an individual settings property.
        /// </summary>
        /// <param name="propertyName"></param>
        static dynamic GetDefault(string propertyName)
        {
            var settingsProperty = Properties.Settings.Default.Properties[propertyName];

            if (settingsProperty != null)
            {
                if (settingsProperty.DefaultValue != null)
                {
                    var defaultValue = TypeDescriptor.GetConverter(settingsProperty.PropertyType).ConvertFromString(settingsProperty.DefaultValue as string);
                    Console.WriteLine($"{propertyName} Default: {defaultValue.GetType()}");    // DEBUG

                    return defaultValue;
                }
            }

            return null;    // Property not found or does not have a default value
        }

        /// <summary>
        /// Called when <see cref="MainWindow"/> is closing.
        /// </summary>
        public void OnWindowClosing(object sender, CancelEventArgs e)       // Settings are saved on property changed.
        {
            if (RamtestIsRunning)
            {
                if (MessageBox.Show(
                    "Stop test and close?", "RAM Test is Running",
                    MessageBoxButton.OKCancel, MessageBoxImage.Warning) != MessageBoxResult.OK)
                {
                    e.Cancel = true;
                    return;
                }
            }

            if (RamtestStopCommand.CanExecute(null))
                RamtestStopCommand.Execute(null);
        }

        /// <summary>
        /// Called when <see cref="MainWindow"/> is activated.
        /// </summary>
        public void OnWindowActivated(object sender, EventArgs e)
        {
            if (!UiTopmostEnabled)      // Ensures activated window is in the foreground
            {
                UiTopmostEnabled = true;
                UiTopmostEnabled = false;
            }
        }

        /// <summary>
        /// <br>Apply startup arguments included at runtime.</br>
        /// <br>Syntax: -arg param || --switch || --switch:off</br>
        /// </summary>
        /// <param name="startArgs"></param>
        /* TODO:
            Support priority classes
            Update log restore
            ? Support for alternate single letter parameters
            ? Do not make startup values persistent and support "--save" switch
            ? User presets?
        */
        void SetStartupArguments(string[] startArgs)
        {
            Dictionary<string, string> argsDict = new Dictionary<string, string>();

            for (int index = 0; index < startArgs.Length; index += 2)
            {
                string argIndex = startArgs[index].ToUpper();

                if (startArgs.Length == (index + 1) || startArgs[index + 1].StartsWith("-"))        // Handles switch with no parameters
                {
                    bool argSwitch = (!argIndex.EndsWith(":OFF") || argIndex.EndsWith(":ON"));      // Defaults parameter to true
                    argIndex = argIndex.TrimStart(' ', '-').Replace(":OFF", "").Replace(":ON", "");

                    argsDict.Add(argIndex, argSwitch.ToString());
                    index--;
                }

                if (startArgs.Length >= (index + 1) && !startArgs[index + 1].StartsWith("-"))       // Handles argument with parameter
                    argsDict.Add(argIndex.TrimStart(' ', '-'), startArgs[index + 1].ToUpper());
            }

            if (argsDict.Count() > 0)
            {
                if (argsDict.Remove("DEFAULT"))         // Checks for default parameter before setting test arguments
                    ResetSettingsCommand.Execute(null);

                if (argsDict.Remove("LOG-RESTORE"))     // TODO
                {
                    ReadLogFile();
                    return;
                }

                string argErrors = "";
                _startOnLaunch = true;                  // Starts test by default

                foreach(KeyValuePair<string, string> arg in argsDict)
                {
                    bool isBool = Boolean.TryParse(arg.Value, out bool argSwitch);
                    bool isNumber = uint.TryParse(arg.Value.Replace("%", "").Replace("M", "").Replace("G", ""), out uint argValue);
                    bool hasUnit = isNumber && (arg.Value.EndsWith("M") || arg.Value.EndsWith("G"));
                    bool isAuto = arg.Value.Equals("AUTO") || argValue is > 0 and < 100 && (!hasUnit);

                    switch (arg.Key)
                    {
                        case "SIZE" when !(RamtestSizeIsAuto = isAuto) && isNumber:     // Sets manual test size
                            RamtestSize = (arg.Value.EndsWith("G")) ? argValue * 1000 : argValue;
                            break;
                        case "SIZE" when (RamtestSizeIsAuto = isAuto):                  // Sets auto test size
                            RamtestSizePercent = (isNumber) ? argValue : GetDefault("RamtestSizePercent");
                            break;
                        case "THREADS" when isNumber || isAuto:
                            RamtestThreads = (argValue > 0) ? argValue : ((isAuto) ? SystemCpuThreads : RamtestThreads);
                            break;
                        case "COVERAGE" when isNumber || isBool:
                            RamtestTaskScope = (argValue > 0) ? argValue : RamtestTaskScope;
                            RamtestStopOnTaskScope = argValue > 0 || argSwitch;
                            break;
                        case "ERRORS" when isNumber || isBool:
                            RamtestErrorLimit = (argValue > 0) ? argValue : RamtestErrorLimit;
                            RamtestStopOnError = argValue > 0 || argSwitch;
                            break;
                        case "CACHE" when argValue <= 3:
                            RamtestCpuCacheMode = (Ramtest.CpuCacheMode)argValue;
                            break;
                        case "RNG" when argValue <= 1:
                            RamtestRngMode = (Ramtest.RngMode)argValue;
                            break;
                        case "FPU" when isBool:
                            RamtestStressFpu = argSwitch;
                            break;
                        case "DELAY" when isNumber:
                            UiMaxSpeedDelay = argValue;
                            break;
                        case "LOG" when isBool:
                            UiHistoryEnabled = argSwitch;
                            break;
                        case "NO-START" when isBool:
                            _startOnLaunch = !argSwitch;
                            break;
                        // case "LOG-CLEAR" when isBool:
                            // ClearLogCommand.Execute(null);
                            // break;

                        default:    // Sets invalid parameter error
                            argErrors += (string.IsNullOrEmpty(argErrors) ? "" : "\n") + $"Invalid Parameter: {arg.Key}";
                            break;
                    }
                }

                if (!string.IsNullOrEmpty(argErrors))
                {
                    _startOnLaunch = false;     // Does not start test on error

                    MessageBox.Show($"{argErrors}\n\n" +
                                     "See 'KGuiX -help' for command line usage.",
                                    "KGuiX Startup Arguments", MessageBoxButton.OK, MessageBoxImage.Error);
                }

            }
        }

        /// <summary>
        /// Attempts to clear working set of every currently running process.
        /// </summary>
        /// <param name="param"></param>
        void EmptyAllProcesses(object? param)
        {
            Process.GetProcesses().ToList().ForEach(p =>
            {
                try
                {
                    if (!EmptyWorkingSet(p))    // Empty working set of all processes
                        Console.WriteLine($"Failed to Empty Working Set: {p.ProcessName}");    // DEBUG
                }
                catch (Win32Exception ex)
                {
                    Console.WriteLine($"[Exception] EmptyAllProcesses: {ex.Message}");    // DEBUG
                }
            });
        }

        /// <summary>
        /// Remove as many pages as possible from working set of a process.
        /// </summary>
        /// <param name="process">A handle to the process.</param>
        /// <returns>
        /// Returns true if working set successfully cleared for the given process.
        /// </returns>
        public bool EmptyWorkingSet(Process process)
            => Interops.SetWorkingSetSize(process, -1, -1);


        // TODO: Currently for debugging - modified log files could break UI history
        /// <summary>
        /// Retrieve test history from an existing log file.
        /// </summary>
        void ReadLogFile()
        {
            if (File.Exists(_logFilePath) && new FileInfo(_logFilePath).Length >= 6)
                HistoryLog = File.ReadAllText("history.log");
        }

        /// <summary>
        /// Update <see cref="HistoryLog"/> and log file with test events.
        /// </summary>
        /// <param name="logText"></param>
        /// <param name="newEntry">True indicates start of a new test.</param>
        /// <param name="testCancel">True removes test info from UI and updates log file.</param>
        public void UpdateLog(string logText = "", bool newEntry = false, bool testCancel = false)
        {
            bool logExists = File.Exists(_logFilePath) && new FileInfo(_logFilePath).Length > 6;

            if (!string.IsNullOrEmpty(logText))
            {
                File.AppendAllText(_logFilePath, (newEntry && logExists) ? $" \n\n{logText}" : logText);    // Updates log file

                HistoryLog += (newEntry && !string.IsNullOrWhiteSpace(HistoryLog)) ? $" \n\n{logText}" : logText;                       // Updates history tab
            }

            if (testCancel)
            {
                File.AppendAllText(_logFilePath, " ► STOPPED:	Cancelled");    // Updates log file that test was cancelled

                if (ClearLogEntryCommand.CanExecute(null))                      // Removes test from UI history
                    ClearLogEntryCommand.Execute(null);
            }
        }

        /// <summary>
        /// Check if <see cref="HistoryLog"/> has data to be cleared.
        /// </summary>
        /// <param name="param"></param>
        bool CanClearLog(object? param)
            => !string.IsNullOrEmpty(HistoryLog);

        // TODO: Clear/Save log files and manage size
        /// <summary>
        /// Clear <see cref="HistoryLog"/>.
        /// </summary>
        /// <param name="param"></param>
        void ClearLog(object? param)
        {
            if (MessageBox.Show(
                "Clear All Test History?", "KGuiX History",
                MessageBoxButton.OKCancel, MessageBoxImage.Question) == MessageBoxResult.OK)
            {
                HistoryLog = null;
                EmptyWorkingSet(Process.GetCurrentProcess());
            }
        }

        /// <summary>
        /// Remove last test entry from <see cref="HistoryLog"/>.
        /// </summary>
        /// <param name="param"></param>
        void ClearLastLogEntry(object? param)
        {
            int lastIndex = HistoryLog.LastIndexOf(" \n\n");

            // Removes lines following line break of last test entry
            HistoryLog = HistoryLog.Substring(0, ((lastIndex >= 0) ? lastIndex : 0));

            GC.Collect(0, GCCollectionMode.Forced, true);           // Encourages garbage collection
            EmptyWorkingSet(Process.GetCurrentProcess());
        }

        /// <summary>
        /// Check if <see cref="HistoryLog"/> has data to be cleared.
        /// </summary>
        /// <param name="param"></param>
        bool CanOpenLog(object? param)
            => File.Exists(_logFilePath)
            && new FileInfo(_logFilePath).Length > 6;

        /// <summary>
        /// Open history log with notepad.
        /// </summary>
        /// <param name="param"></param>
        void OpenLogFile(object? param)
        {
            Process.Start(@"notepad.exe", _logFilePath);
        }

        /// <summary>
        /// Check if dispatcher timer interval can be set.
        /// </summary>
        /// <param name="param"></param>
        bool CanSetPolling(object? param)
            => UiPollingRate is >= 10 and <= 1000
            && UiPollingRate != _updateTimer.Interval.TotalMilliseconds;

        /// <summary>
        /// Set update interval of the dispatcher timer.
        /// </summary>
        /// <param name="param"></param>
        void SetPollingRate(object? param)
        {
            _updateTimer.Interval = TimeSpan.FromMilliseconds(Convert.ToUInt32(UiPollingRate));

            Properties.Settings.Default.UiPollingRate = UiPollingRate;    // Sets updated interval as user default
        }

        /// <summary>
        /// Check if ramtest can be stopped.
        /// </summary>
        /// <param name="param"></param>
        bool CanStopRamtest(object? param)
            => RamtestIsRunning;

        /// <summary>
        /// Check if ramtest can be started.
        /// </summary>
        /// <param name="param"></param>
        bool CanStartRamtest(object? param)
            => !RamtestIsRunning
            && RamtestSizeIsValid
            && RamtestThreads > 0
            && RamtestThreads <= SystemCpuThreads
            && !(RamtestStopOnTaskScope && RamtestTaskScope < 1);

        /// <summary>
        /// Stop the ramtest and update <see cref="HistoryLog"/> when enabled.
        /// </summary>
        /// <param name="param"></param>
        void StopRamtest(object? param)
        {
            RamtestIsRunning = !Ramtest.StopTest();
            RamtestHasStopped = true;   // Sets test stopped flag for 1 tick

            if (UiHistoryEnabled)
            {
                // Do not keep test log with no defined max speed unless errors exist 
                if (RamtestMaxSpeed == 0 && RamtestErrorCount == 0)
                    UpdateLog(testCancel: true);
                else
                {
                    UpdateLog(String.Format( " ► STOPPED:	{0}\n" +
                                            $"    MemUsage:	{RamtestMemoryUsagePercent} %\n" +
                                            $"    Duration:	{RamtestDuration.ToString("d\\:hh\\:mm\\:ss")}\n" +
                                            $"    Coverage:	{RamtestCoveragePercent:0} %\n" +
                                            $"    MaxSpeed:	{RamtestMaxSpeed:N3}",
                                            RamtestErrorCount > 0 ? RamtestErrorCount + (RamtestErrorCount > 1 ? " Errors" : " Error") : "** PASS **" ));
                }
            }
        }

        /// <summary>
        /// Start the ramtest and set new <see cref="HistoryLog"/> entry when enabled.
        /// </summary>
        /// <param name="param"></param>
        void StartRamtest(object? param)
        {
            EmptyWorkingSet(Process.GetCurrentProcess());

            Ramtest.SetStressFpu(RamtestStressFpu);
            Ramtest.SetCpuCache(RamtestCpuCacheMode);
            Ramtest.SetRng(RamtestRngMode);

            _startOnLaunch = false;

            var started = RamtestIsRunning = Ramtest.StartTest(RamtestSize ?? 0, RamtestThreads ?? 0);

            if (started)
            {
                _ramtestStartTick = Stopwatch.GetTimestamp();
                RamtestErrorLog = "";

                if (UiHistoryEnabled)
                {
                    UpdateLog( $"〚 {DateTime.Now.ToString()} 〛\n" +
                               $" ► TESTING:	{RamtestSize:D} / {RamtestThreads:D}\n", newEntry: true );
                }
            }
        }

        /// <summary>
        /// Called on <see cref="RamtestHasNewError"/>.
        /// </summary>
        /// <param name="isErrorLimit"></param>
        void SetRamtestError(bool isErrorLimit = false)
        {
            RamtestHasNewError = false;

            if (RamtestErrorCount <= 51)                // Limits excessive logging
            {
                string errorInfo = (RamtestErrorCount <= 50) ? 
                                    $"Error {RamtestErrorCount}:	{RamtestDuration.ToString("d\\:hh\\:mm\\:ss")}" : "Game Over ☠";

                if (UiHistoryEnabled)
                    UpdateLog($"    {errorInfo}\n");

                if (RamtestErrorCount <= 50)            // Adds coverage to error log popup
                    errorInfo = $"{errorInfo} ({RamtestCoveragePercent:0} %)";

                RamtestErrorLog += (string.IsNullOrEmpty(RamtestErrorLog) ? "" : "\n") + $"{errorInfo}";
                errorInfo = "";
            }

            if (RamtestBeepOnError)
                System.Threading.Tasks.Task.Run(() =>   // Executes on new thread to preserve timer interval
                {
                    if (!isErrorLimit)
                        Console.Beep(1550, 150);        // Higher frequency 150ms beep
                    else
                        Console.Beep(1000, 450);        // Final longer beep
                });

            if (RamtestStopOnError && isErrorLimit)
            {
                if (RamtestStopCommand.CanExecute(null))
                    RamtestStopCommand.Execute(null);
            }
        }

        /// <summary>
        /// Background updater for the application.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void BackgroundUpdater(object? sender, EventArgs e)
        {
            if (Interops.GlobalMemoryStatusEx(ref _memoryStatusEx))
            {
                if (Math.Abs(SystemMemoryFree - (_memoryStatusEx.AvailPhys / 1024 / 1024)) > 1)  // Minimizes excessive updates
                {
                    SystemMemoryFree = _memoryStatusEx.AvailPhys / 1024 / 1024;
                    GC.Collect(0, GCCollectionMode.Forced);
                }
            }

            if (!RamtestIsRunning)
            {
                if (RamtestHasStopped)
                {
                    _mainWindow.Activate();
                    EmptyWorkingSet(Process.GetCurrentProcess());
                    RamtestHasStopped = false;                      // Resets test stopped flag
                }

                if (RamtestSizeIsAuto)      // Updates calculated test size
                    RamtestSize = (uint)((SystemMemoryFree * RamtestSizePercent.GetValueOrDefault() / 100)) / RamtestThreads * RamtestThreads;      // Rounded to thread count

                RamtestMemoryUsagePercent = Math.Round((double)(RamtestSize / SystemMemoryTotal) * 100, 1);     // Updates test size as percentage of physical memory.

                // Start test at runtime if launched with startup arguments
                if (_startOnLaunch && RamtestStartCommand.CanExecute(null))
                    RamtestStartCommand.Execute(null);

                // TODO
                // Reduce memory allocation as working set increases
                if (Environment.WorkingSet / 1024 / 1024 > 80)
                    EmptyWorkingSet(Process.GetCurrentProcess());
            }

            if (RamtestIsRunning)
            {
                RamtestErrorCount = Ramtest.GetErrorCount();
                RamtestCoverage = Ramtest.GetCoverage();
                RamtestDuration = TimeSpan.FromTicks(Stopwatch.GetTimestamp() - _ramtestStartTick);

                RamtestSpeed = RamtestCoverage * ((RamtestSize ?? 0) / RamtestDuration.TotalSeconds);
                // Preserve highest recorded test speed when delay is reached
                RamtestMaxSpeed = (UiMaxSpeedDelay < (uint)RamtestCoveragePercent) ? Math.Max(RamtestSpeed, RamtestMaxSpeed) : 0;

                if (RamtestHasNewError)
                    SetRamtestError(RamtestErrorCount >= RamtestErrorLimit);

                // Check if maximum coverage has been reached
                if (RamtestStopOnTaskScope && RamtestTaskScope <= RamtestCoveragePercent)
                {
                    RamtestCoverage = (double)RamtestTaskScope / 100;      // Ensures final coverage doesn't overshoot the set value

                    if (RamtestStopCommand.CanExecute(null))
                        RamtestStopCommand.Execute(null);
                }
            }
        }
    }
}