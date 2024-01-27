﻿using Microsoft.Win32.SafeHandles;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Versioning;
using Windows.Win32;
using Windows.Win32.Security;
using Windows.Win32.System.JobObjects;

// The job related APIs are all annotated with as requiring "windows5.1.2600", which is Windows XP.
// Modern .NET does not even run on an operating system that old, so annotate our class as merely
// requiring "windows" and silence the warning.
#pragma warning disable CA1416

namespace KillMyChildren
{
    [SupportedOSPlatform("windows")]
    internal class ProcessManager
    {
        private static readonly object s_lock = new();
        private static SafeFileHandle? s_job;

        [MemberNotNull(nameof(s_job))]
        private static unsafe void EnsureJob()
        {
            lock (s_lock)
            {
                if (s_job is null)
                {
                    SECURITY_ATTRIBUTES secAttrs = default;
                    var job = PInvoke.CreateJobObject(secAttrs, null);
                    if (job.IsInvalid)
                        throw new Win32Exception();

                    JOBOBJECT_EXTENDED_LIMIT_INFORMATION limits = default;
                    limits.BasicLimitInformation.LimitFlags = JOB_OBJECT_LIMIT.JOB_OBJECT_LIMIT_KILL_ON_JOB_CLOSE;
                    if (!PInvoke.SetInformationJobObject(job, JOBOBJECTINFOCLASS.JobObjectExtendedLimitInformation, &limits, (uint)sizeof(JOBOBJECT_EXTENDED_LIMIT_INFORMATION)))
                    {
                        throw new Win32Exception();
                    }

                    s_job = job;
                }
            }
        }

        public static void KillAllMyChildrenWhenIExit()
        {
            EnsureJob();
            if (!PInvoke.AssignProcessToJobObject(s_job, Process.GetCurrentProcess().SafeHandle))
                throw new Win32Exception();
        }

        public static void KillProcessWhenIExit(Process p)
        {
            EnsureJob();
            if (!PInvoke.AssignProcessToJobObject(s_job, p.SafeHandle))
                throw new Win32Exception();
        }
    }
}