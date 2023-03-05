using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;

namespace LeagueDelete
{
    class InterlockedConsole
    {

        object lockObject = new object();

        void WriteLine(string value) { lock (lockObject) { System.Console.WriteLine(value); } }

        string ReadLine(string value) { lock (lockObject) { return System.Console.ReadLine(); } }

    }

    internal class Program
    {
        public static void showRunning()
        {
            List<Process> processes = Process.GetProcesses().ToList();
            foreach (Process p in processes)
                Console.WriteLine(p.ProcessName);

        }

        public static readonly List<String> ForbiddenNames = new List<string> { "Riot", "League" };

        static void Main(string[] args)
        {   // showRunning();

            // kill league procs 
                // run it for 10 seconds
                KillLeagueProcs(10);   
   
        }

        static void KillLeagueProcs(double seconds)
        {
            List<Process> processes = Process.GetProcesses().ToList();

            for (int i = 0; i < seconds; i++)
            {
                foreach (Process p in processes)
                {
                    foreach (String s in ForbiddenNames)
                    {
                        if (p.ProcessName.Contains(s) && p.Id != Process.GetCurrentProcess().Id)
                        {
                            try
                            {
                                Console.WriteLine("Killing " + p.ProcessName);
                                KillProcessAndChildren(p.Id);
                            }
                            catch (Exception ex)
                            {
                                // Console.WriteLine(ex.Message);
                                // Console.WriteLine(ex.ToString());
                            }

                        }
                    }
                }
                System.Threading.Thread.Sleep(1000);
            }
        }
        /// <summary>
        /// Kill a process, and all of its children, grandchildren, etc.
        /// </summary>
        /// <param name="pid">Process ID.</param>
        private static void KillProcessAndChildren(int pid)
        {
            // Cannot close 'system idle process'.
            if (pid == 0)
            {
                return;
            }
            ManagementObjectSearcher searcher = new ManagementObjectSearcher
                    ("Select * From Win32_Process Where ParentProcessID=" + pid);
            ManagementObjectCollection moc = searcher.Get();
            foreach (ManagementObject mo in moc)
            {
                KillProcessAndChildren(Convert.ToInt32(mo["ProcessID"]));
            }
            try
            {
                Process proc = Process.GetProcessById(pid);
                //Console.Write("Killing: ");
                //Console.WriteLine(proc.ProcessName);
                proc.Kill();
            }
            catch (ArgumentException)
            {
                // Process already exited. 
            }
        }
    }
}
