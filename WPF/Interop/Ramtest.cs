// Credit: RaINi_
using System;
using System.Runtime.InteropServices;

namespace KGuiX.Interop
{
    public static class Ramtest
    {
        public enum CpuCacheMode
        {
            Disabled     = 0,
            WriteCombine = 1,
            Default      = 2,
            Enabled      = 3,
        }

        public enum RngMode
        {
            Default = 0,
            XORWOW  = 1,
        }

        /// <summary>
        /// Starts the stresstest.
        /// </summary>
        /// <param name="numMegabytes">The number of megabytes to test with a minimum value of 1</param>
        /// <param name="numThreads">The number of threads to use for the test between 1 and 64.</param>
        /// <returns></returns>
        [DllImport("ramtest.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool StartTest(
            uint numMegabytes,
            uint numThreads
        );

        /// <summary>
        /// Sets the cpu cache mode.
        /// </summary>
        /// <param name="mode"></param>
        [DllImport("ramtest.dll", SetLastError = true)]
        public static extern void SetCpuCache(
            CpuCacheMode mode
        );

        /// <summary>
        /// Sets the test data random generation mode.
        /// </summary>
        /// <param name="mode"></param>
        [DllImport("ramtest.dll", SetLastError = true)]
        public static extern void SetRng(
            RngMode mode
        );

        /// <summary>
        /// Enables fpu stress testing.
        /// </summary>
        /// <param name="option"></param>
        [DllImport("ramtest.dll", SetLastError = true)]
        public static extern void SetStressFpu(
            bool option
        );

        /// <summary>
        /// Stops the ramtest if it is running.
        /// </summary>
        /// <returns></returns>
        [DllImport("ramtest.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool StopTest();

        /// <summary>
        /// The current test coverage.
        /// </summary>
        /// <returns></returns>
        [DllImport("ramtest.dll", SetLastError = true)]
        public static extern double GetCoverage();

        /// <summary>
        /// Gets the amount of errors that where detected during the test.
        /// </summary>
        /// <returns></returns>
        [DllImport("ramtest.dll", SetLastError = true)]
        public static extern uint GetErrorCount();
    }
}
