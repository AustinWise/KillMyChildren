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

            using (var p = Process.Start(psi))
            {
                if (p is null)
                    throw new Exception("Failed to start process.");

                // Note that there is a gap between when we start the process and when we add it to the job.
                // In that time it could start it's own subprocesses that are not part of the job.
                // To avoid this race condition, we need the CREATE_SUSPENDED flag for CreateProcess,
                // which is currently not exposed in an offical .NET API:
                // https://github.com/dotnet/runtime/issues/94127

                if (OperatingSystem.IsWindows())
                {
                    ProcessManager.KillProcessWhenIExit(p);
                }
            }

            Console.WriteLine("sleeping 5 seconds");
            Thread.Sleep(5000);
            Console.WriteLine("exiting");
        }
    }
}
