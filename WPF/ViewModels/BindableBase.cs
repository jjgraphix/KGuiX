using KGuiX.Interop;

using System;
using System.Linq;
using System.Reflection;
using System.Diagnostics;
using System.Globalization;
using System.ComponentModel;
using System.Security.Principal;    // To check for admin rights
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

#nullable disable warnings
namespace KGuiX.ViewModels
{
    internal abstract class BindableBase : INotifyPropertyChanged
    {
        /// <summary>
        /// Indicates the application has administrator privledges.
        /// </summary>
        internal static bool IsAdministrator => new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator);

        /// <summary>
        /// The application version information.
        /// </summary>
        public static string AppVersion => GetAppVersion("-beta");

        /// <summary>
        /// Current culture info for XAML binding data.
        /// </summary>
        public static readonly CultureInfo CurrentCulture = CultureInfo.CurrentCulture;

        /// <summary>
        /// Collection of priority classes available to set <see cref="UiPriorityLevel">.
        /// </summary>
        public ObservableCollection<ProcessPriorityClass> SystemPriorityLevels { get; } = GetPriorityLevels();

        /// <summary>
        /// The total number of logical cores in the system.
        /// </summary>
        public uint SystemCpuThreads => (uint)Environment.ProcessorCount;


        /// <inheritdoc/>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// The total amount of system memory.
        /// </summary>
        public double SystemMemoryTotal { get; set; }
        /// <summary>
        /// The amount of currently unused system memory.
        /// </summary>
        public double SystemMemoryFree { get; set; }
        /// <summary>
        /// The percentage of currently unused system memory.
        /// </summary>
        public double SystemMemoryFreePercent => Math.Round(100 * (SystemMemoryFree / SystemMemoryTotal), 1);


        // TEST PROPERTIES //

        /// <summary>
        /// The ramtest cpu cache modes.
        /// </summary>
        public IEnumerable<Ramtest.CpuCacheMode> RamtestCpuCacheModes { get; } = Enum.GetValues(typeof(Ramtest.CpuCacheMode)).Cast<Ramtest.CpuCacheMode>();
        /// <summary>
        /// The ramtest rng function modes.
        /// </summary>
        public IEnumerable<Ramtest.RngMode> RamtestRngModes { get; } = Enum.GetValues(typeof(Ramtest.RngMode)).Cast<Ramtest.RngMode>();
        /// <summary>
        /// The cpu cache mode to use for the ramtest.
        /// </summary>
        public Ramtest.CpuCacheMode RamtestCpuCacheMode { get; set; } = Ramtest.CpuCacheMode.Default;
        /// <summary>
        /// The rng mode to use for the ramtest.
        /// </summary>
        public Ramtest.RngMode RamtestRngMode { get; set; } = Ramtest.RngMode.Default;
        /// <summary>
        /// Indicates if the ramtest is currently running.
        /// </summary>
        public bool RamtestIsRunning { get; set; }
        /// <summary>
        /// <br>The amount of memory in MB to test, defined explicitly as <see cref="RamtestMegabytes"/> or</br>
        /// <br>implicitly calculated from <see cref="RamtestSizePercent"/> when <see cref="RamtestSizeIsAuto"/>.</br>
        /// </summary>
        public uint? RamtestSize { get; set; } = 1000;
        /// <summary>
        /// The amount of memory in MB to test when <see cref="RamtestSizeIsAuto"/> is disabled.
        /// </summary>
        public uint? RamtestMegabytes { get; set; } = 1000;
        /// <summary>
        /// The percentage of <see cref="SystemMemoryFree"/> to set <see cref="RamtestSize"/> when <see cref="RamtestSizeIsAuto"/>.
        /// </summary>
        public uint? RamtestSizePercent { get; set; } = 95;
        /// <summary>
        /// The amount of CPU threads to use for the test. (Max: 64)
        /// </summary>
        public uint? RamtestThreads { get; set; } = 1;      // Max value set in property settings by default
        /// <summary>
        /// The maximum ramtest coverage if <see cref="RamtestStopOnTaskScope"/>.
        /// </summary>
        public uint? RamtestTaskScope { get; set; } = 5000;
        /// <summary>
        /// The maximum ramtest errors allowed if <see cref="RamtestStopOnError"/>.
        /// </summary>
        public uint? RamtestErrorLimit { get; set; } = 1;
        /// <summary>
        /// Indicates the ramtest will stress the cpu fpu.
        /// </summary>
        public bool RamtestStressFpu { get; set; }  = false;
        /// <summary>
        /// Indicates <see cref="RamtestSize"/> is calculated from percentage of <see cref="SystemMemoryFree"/>.
        /// </summary>
        public bool RamtestSizeIsAuto { get; set; } = false;
        /// <summary>
        /// Indicates ramtest will stop when <see cref="RamtestTaskScope"/> is reached.
        /// </summary>
        public bool RamtestStopOnTaskScope { get; set; } = true;
        /// <summary>
        /// Indicates ramtest will stop when <see cref="RamtestErrorLimit"/> is reached.
        /// </summary>
        public bool RamtestStopOnError { get; set; } = true;
        /// <summary>
        /// Indicates a <see cref="Console.Beep"/> tone is played when ramtest encounters an error.
        /// </summary>
        public bool RamtestBeepOnError { get; set; } = true;


        // UI PROPERTIES //

        /// <summary>
        /// The update interval for <see cref="DispatcherTimer"> (Default: 150 ms).
        /// </summary>
        public uint? UiPollingRate { get; set; } = 150;
        /// <summary>
        /// The ammount of memory coverage completed until <see cref="RamtestMaxSpeed"> begins recording.
        /// </summary>
        public uint? UiMaxSpeedDelay { get; set; } = 15;
        /// <summary>
        /// Indicates if tool tip help is enabled in the UI.
        /// </summary>
        public bool UiToolTipEnabled { get; set; } = true;
        /// <summary>
        /// Indicates if logging ramtest results to <see cref="HistoryLog"> is enabled.
        /// </summary>
        public bool UiHistoryEnabled { get; set; } = true;
        /// <summary>
        /// Indicates if the application will stay as the topmost window.
        /// </summary>
        public bool UiTopmostEnabled { get; set; } = false;
        /// <summary>
        /// Indicates if the application window position should be saved and restored.
        /// </summary>
        public bool UiSaveWindowPos { get; set; } = false;
        /// <summary>
        /// Indicates if active UI theme is dark mode.
        /// </summary>
        public bool UiIsDarkTheme { get; set; } = false;
        /// <summary>
        /// The priority class currently defined for the application.
        /// </summary>
        public ProcessPriorityClass UiPriorityLevel { get; set; } = Process.GetCurrentProcess().PriorityClass;


        // STATUS PROPERTIES //

        /// <summary>
        /// The current ramtest test duration.
        /// </summary>
        public TimeSpan RamtestDuration { get; set; }
        /// <summary>
        /// The current ramtest coverage.
        /// </summary>
        public double RamtestCoverage { get; set; } = 0;
        /// <summary>
        /// The current ramtest coverage in percent.
        /// </summary>
        public double RamtestCoveragePercent => 100 * RamtestCoverage;
        /// <summary>
        /// The next full ramtest coverage in percent.
        /// </summary>
        public double RamtestNextFullCoveragePercent => 100 * Math.Floor(RamtestCoverage + 1.0);
        /// <summary>
        /// The approximate time it will take to reach the next full coverage percentage.
        /// </summary>
        public TimeSpan RamtestNextFullCoverageIn
            => TimeSpan.FromSeconds((int)(0.01 * (RamtestNextFullCoveragePercent - RamtestCoveragePercent) * (RamtestSize ?? 0) / RamtestSpeed));
        /// <summary>
        /// The approximate time it will take to finish the test with the given <see cref="RamtestTaskScope"/>.
        /// </summary>
        public TimeSpan RamtestFinishedIn
            => TimeSpan.FromSeconds((int)(0.01 * ((uint)RamtestTaskScope - RamtestCoveragePercent) * (RamtestSize ?? 0) / RamtestSpeed));
        /// <summary>
        /// The speed at which the ramtest is running.
        /// </summary>
        public double RamtestSpeed { get; set; } = 0;
        /// <summary>
        /// The maximum recorded <see cref="RamtestSpeed"/> during the ramtest.
        /// </summary>
        public double RamtestMaxSpeed { get; set; } = 0;
        /// <summary>
        /// The amount of ramtest errors detected.
        /// </summary>
        public uint RamtestErrorCount { get; set; }
        /// <summary>
        /// The ramtest error history displayed in status.
        /// </summary>
        public string RamtestErrorLog { get; set; } = "";
        /// <summary>
        /// The ramtest size divided by <see cref="RamtestThreads"/>.
        /// </summary>
        public double RamtestSizePerThread => (double)Math.Round((RamtestSize ?? .0) / (RamtestThreads ?? .0), 1);
        /// <summary>
        /// The percentage of total system memory used by <see cref="RamtestSize"/>.
        /// </summary>
        public double RamtestMemoryUsagePercent { get; set; } = 0;
        /// <summary>
        /// Indicates valid ramtest size is less than <see cref="SystemMemoryFree"/>.
        /// </summary>
        public bool RamtestSizeIsValid => (RamtestSize < SystemMemoryFree && RamtestSize > 0);
        /// <summary>
        /// Indicates the ramtest has just been stopped.
        /// </summary>
        public bool RamtestHasStopped { get; set; } = false;
        /// <summary>
        /// Indicates when <see cref="RamtestErrorCount"/> has increased.
        /// </summary>
        public bool RamtestHasNewError { get; set; } = false;

        /// <summary>
        /// The ramtest history log string saved to user property settings.
        /// </summary>
        public string HistoryLog { get; set; }                     // TODO: Better solution
        /// <summary>
        /// The ramtest history log lines for virtualizing scroll viewer.
        /// </summary>
        public IEnumerable<string> HistoryLogLines
            => (string.IsNullOrWhiteSpace(HistoryLog)) ? new[] {"No History"} : HistoryLog?.Split(" \n\n");


        /// <summary>
        /// Update all property changes in view model or UI thread and save user setting property changes.
        /// </summary>
        /// <param name="propertyName"></param>
        public void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));  // Updates UI thread
            var propertyInfo = typeof(Properties.Settings).GetProperty(propertyName);   // Gets Reflection.PropertyInfo object

            if (propertyInfo != null)                                                   // Checks and updates value to Properties.Settings
            {
                // Console.WriteLine($" {propertyName}: {propertyInfo.GetValue(Properties.Settings.Default)}");         // DEBUG
                propertyInfo.SetValue(Properties.Settings.Default, GetType().GetProperty(propertyName).GetValue(this));
                Properties.Settings.Default.Save();
            }
        }

        /// <summary>
        /// Validate value and range for changes to unsigned integer properties.
        /// </summary>
        /// <param name="before">The previously set value</param>
        /// <param name="after">The new value to validate.</param>
        /// <param name="minValue">The minimum allowed value.</param>
        /// <param name="maxValue">The maximum allowed value.</param>
        /// <returns>Returns after value if valid or before value if invalid.</returns>
        uint ValidatePropertyRange(object? before, object after, uint minValue, uint maxValue)
        {
            if (maxValue < minValue)            // Ensures variables have initialized
                return Convert.ToUInt32(after);

            uint fallback = (Convert.ToUInt32(before) > minValue) ? Convert.ToUInt32(before) : minValue;    // Value when input is null
            uint newValue = Convert.ToUInt32(after);

            uint setValue = (Convert.ToString(after) is null or "") ? fallback :
                            (newValue > maxValue) ? maxValue :
                            (newValue < minValue) ? minValue : newValue;

            return setValue;
        }

        /// <summary>
        /// OnPropertyChanged: Validate when <see cref="RamtestStopOnTaskScope"/> is enabled while ramtest is running.
        /// </summary>
        void OnRamtestStopOnTaskScopeChanged()
        {
            if ( RamtestIsRunning && RamtestStopOnTaskScope
                && RamtestTaskScope < (uint)RamtestCoveragePercent )
            {
                RamtestStopOnTaskScope = false;

                MessageBox.Show("Coverage limit must be higher than active test coverage.",
                                "Test is Running", MessageBoxButton.OK);
            }
        }

        /// <summary>
        /// OnPropertyChanged: If <see cref="RamtestSizeIsAuto"> disabled set <see cref="RamtestSize"> to <see cref="RamtestMegabytes">.
        /// </summary>
        void OnRamtestSizeIsAutoChanged()
        {
            if (!RamtestSizeIsAuto)
                RamtestSize = RamtestMegabytes;
        }

        /// <summary>
        /// OnPropertyChanged: Ensure UI updates when <see cref="RamtestSizeIsValid"/> changes.
        /// </summary>
        void OnRamtestSizeIsValidChanged()
        {
            CommandManager.InvalidateRequerySuggested();
        }

        /// <summary>OnPropertyChanged: Validate <see cref="RamtestMegabytes"></summary>
        void OnRamtestSizeChanged(object before, object after)
        {
            RamtestSize = ValidatePropertyRange(before, after, 1, (uint)SystemMemoryTotal);

            if (!RamtestSizeIsAuto)     // Saves manual test size setting property
                RamtestMegabytes = Properties.Settings.Default.RamtestMegabytes = (uint)RamtestSize;
        }

        /// <summary>OnPropertyChanged: Validate <see cref="RamtestThreads"></summary>
        void OnRamtestThreadsChanged(object before, object after)
        {
            RamtestThreads = ValidatePropertyRange(before, after, 1, (uint)SystemCpuThreads);

            if (RamtestThreads > 64)
            {
                MessageBox.Show("Maximum of 64 threads supported by RAM Test",
                                "KGuiX Max Thread Count", MessageBoxButton.OK);

                RamtestThreads = 64;
            }
        }

        /// <summary>OnPropertyChanged: Validate <see cref="RamtestTaskScope"></summary>
        void OnRamtestTaskScopeChanged(object before, object after)
        {
            RamtestTaskScope = ValidatePropertyRange(before, after, 0, 1000000);

            if (RamtestIsRunning && RamtestStopOnTaskScope)
            {
                // Ensure max coverage is set higher than test coverage
                if ((int)RamtestTaskScope - 50 < RamtestCoveragePercent)
                    RamtestTaskScope = (uint)(Math.Ceiling((RamtestCoveragePercent + 50) / 100) * 100.0);
            }

            RamtestStopOnTaskScope = (RamtestStopOnTaskScope) ? (RamtestTaskScope != 0) :
                                    (Convert.ToUInt32(before) == 0 && RamtestTaskScope > 0);
        }

        /// <summary>OnPropertyChanged: Validate <see cref="RamtestSizePercent"></summary>
        void OnRamtestSizePercentChanged(object before, object after)
        {
            RamtestSizePercent = ValidatePropertyRange(before, after, 1, 99);
        }

        /// <summary>OnPropertyChanged: Validate <see cref="RamtestErrorLimit"></summary>
        void OnRamtestErrorLimitChanged(object before, object after)
        {
            RamtestErrorLimit = ValidatePropertyRange(before, after, 0, 100);

            RamtestStopOnError = (RamtestStopOnError) ? (RamtestErrorLimit != 0) :
                                    (Convert.ToUInt32(before) == 0 && RamtestErrorLimit > 0);

            CommandManager.InvalidateRequerySuggested();    // Ensures changes reflected in UI
        }

        /// <summary>
        /// OnPropertyChanged: Raise <see cref="RamtestHasNewError"/> flag when <see cref="RamtestErrorCount"/> increases.
        /// </summary>
        void OnRamtestErrorCountChanged(object before, object after)
        {
            RamtestHasNewError = (RamtestErrorCount > Convert.ToUInt32(before));
        }

        /// <summary>OnPropertyChanged: Validate <see cref="UiPollingRate"></summary>
        void OnUiPollingRateChanged(object before, object after)
        {
            UiPollingRate = ValidatePropertyRange(before, after, 10, 1000);
            CommandManager.InvalidateRequerySuggested();    // Ensures set button detects changes
        }

        /// <summary>OnPropertyChanged: Validate <see cref="UiMaxSpeedDelay"></summary>
        void OnUiMaxSpeedDelayChanged(object before, object after)
        {
            UiMaxSpeedDelay = ValidatePropertyRange(before, after, 0, 1000);
        }

        /// <summary>
        /// OnPropertyChanged: Apply selected <see cref="UiPriorityLevel"> of the application.
        /// </summary>
        void OnUiPriorityLevelChanged()
        {
            Process currentProcess = Process.GetCurrentProcess();

            try
            {
                currentProcess.PriorityClass = UiPriorityLevel;
                currentProcess.Refresh();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Exception] Priority Level Changed: {ex.Message}");         // DEBUG
                return;
            }

            UiPriorityLevel = currentProcess.PriorityClass;
        }

        /// <summary>
        /// Get collection of priority classes available to the current process in a specified order.
        /// </summary>
        /// <returns></returns>
        static ObservableCollection<ProcessPriorityClass> GetPriorityLevels()
        {
            var priorityClasses = new ObservableCollection<ProcessPriorityClass>();

            // Define the desired order of available priority classes
            ProcessPriorityClass[] priorityOrder = new ProcessPriorityClass[]
            {
                ProcessPriorityClass.Idle,
                // ProcessPriorityClass.BelowNormal,
                ProcessPriorityClass.Normal,
                // ProcessPriorityClass.AboveNormal,
                ProcessPriorityClass.High,
            };

            foreach (ProcessPriorityClass priority in priorityOrder)
            {
                priorityClasses.Add(priority);
            }

            if (IsAdministrator)
                priorityClasses.Add(ProcessPriorityClass.RealTime);

            return priorityClasses;
        }

        /// <summary>
        /// Get application version information.
        /// <param name="suffix"></param>
        /// </summary>
        /// <returns></returns>
        static string GetAppVersion(string? suffix = null)
        {
            var appInfo = Assembly.GetExecutingAssembly().GetName();  // Retrieves version from the assembly
            string appVersion = appInfo.Version.ToString();
#if DEBUG
            appVersion += "-debug";
            Console.WriteLine($"{appInfo.Name} Version: {appVersion}");
#else
            appVersion += $"{suffix}";
#endif
            return $"{appInfo.Name} v{appVersion}     JJFX 2024";
        }
    }
}
