using System.Diagnostics;

namespace KillMyChildren
{
    internal class Program
    {
        static unsafe void Main(string[] args)
        {
            // Uncomment to automatically kill ALL child processes.
            //if (OperatingSystem.IsWindows())
            //{
            //    ProcessManager.KillAllMyChildrenWhenIExit();
            //}

            var psi = new ProcessStartInfo("cmd.exe", "/k echo This CMD.exe instance should be killed when the parent process exits.")
            {
                UseShellExecute = true,
            };
            var p = Process.Start(psi);

            if (OperatingSystem.IsWindows())
            {
                ProcessManager.KillProcessWhenIExit(p);
            }

            Console.WriteLine("sleeping 5 seconds");
            Thread.Sleep(5000);
            Console.WriteLine("exiting");
        }
    }
}
