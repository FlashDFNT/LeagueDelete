using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Threading;
using System.Threading.Tasks;

namespace LeagueDelete
{
    public class Mon
    {
        public void MonitorKeypress()
        {
            ConsoleKeyInfo cki = new ConsoleKeyInfo();
            do
            {
                // true hides the pressed character from the console
                cki = Console.ReadKey(true);

                // Wait for an ESC
            } while (cki.Key != ConsoleKey.Spacebar);

            Environment.Exit(0);
        }
    }
    public class Kil
    {
        public static readonly List<String> ForbiddenNames = new List<string> { "Riot", "League" };

        public void KillLeagueProcs(double seconds)
        {
            Console.WriteLine("Press spacebar any time to quit" + Environment.NewLine);
            List<Process> processes = new List<Process>();

            for (int i = 0; i < (seconds * 10); i++)
            {
                processes = Process.GetProcesses().ToList();

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
                            catch
                            {
                                // do nothing
                            }
                        }
                    }
                }
                System.Threading.Thread.Sleep(100);
            }
        }

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
                proc.Kill();
            }
            catch (ArgumentException)
            {
                // Process already exited. 
            }
        }
    }
    internal class Program
    {
        static async Task Main(string[] args)
        {
            Mon monitor = new Mon();
            Kil killer = new Kil();

            Task monitorKeyPressTask = Task.Run(() => { monitor.MonitorKeypress(); });
            Task killLeagueProcsTask = Task.Run(() => { killer.KillLeagueProcs(10); });

            await killLeagueProcsTask;
            await monitorKeyPressTask;
        }
    }
}
