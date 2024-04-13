using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Threading;
using System.Threading.Tasks;

namespace LeagueDelete
{
    public class KeyMonitor
    {
        public void Monitor(ProcessHitman killer, CancellationTokenSource cts)
        {
            ConsoleKeyInfo cki = new ConsoleKeyInfo();
            do
            {
                cki = Console.ReadKey(true);

                // Cancel if spacebar is hit.

                // Personal note,
                // Using cancellation token is redundant here but
                // will be useful if the app supports multiple tasks that need
                // to be notified of cancellation
		// MOD twb 20240413@1257
                if (cki.Key == ConsoleKey.Spacebar)
                {
                    cts.Cancel();
                    Environment.Exit(0);
                }

                // Kill the process corresponding to keyboard key
                try
                {
                    killer.TryAddProcessByKey(cki.Key.ToString());
                }
                catch
                {
                    // duplicate entry or other exceptions
                    // do nothing
                }

                if (cts.Token.IsCancellationRequested)
                {
                    break;
                }

            } while (true);
        }
    }


    public class ProcessHitman
    {
        public Dictionary<string, IEnumerable<string>> ActiveHitList = new Dictionary<string, IEnumerable<string>>();
        public Dictionary<string, IEnumerable<string>> InactiveHitList = new Dictionary<string, IEnumerable<string>>();
        private readonly Dictionary<string, string> KeyToProcessMap = new Dictionary<string, string>();

        /// <summary>
        /// Inactive hitlist items are added in Main(). League is Active by default,
        /// 
        /// </summary>
        /// <param name="processName"></param>
        /// <param name="key"></param>
        /// <param name="processList"></param>
        /// <param name="isActive"></param>
        public void AddToHitList(string processName, string key, List<string> processList, bool isActive = false)
        {
            if (isActive)
            {
                ActiveHitList.Add(processName, processList);
            }
            else
            {
                InactiveHitList.Add(processName, processList);
            }

            KeyToProcessMap.Add(key, processName);
        }
        /// <summary>
        /// "Press S to kill Slack,
        ///  Press Spacebar to end program" 
        /// </summary>
        public void DisplayInstructions()
        {
            ////Debugging
            //Process[] runningProcesses = Process.GetProcesses();
            //Console.WriteLine("Currently running processes:");
            //foreach (var process in runningProcesses)
            //{
            //    Console.WriteLine($"- {process.ProcessName}");
            //}

            /* Instructions */
            foreach (var key in KeyToProcessMap.Keys)
            {
                Console.WriteLine($"Press {key} to exit {KeyToProcessMap[key]}");
            }

            Console.WriteLine("Press spacebar any time to quit" + Environment.NewLine);
        }


        public void TryAddProcessByKey(string key)
        {
            if (InactiveHitList.ContainsKey(KeyToProcessMap[key]))
            {
                string processName = KeyToProcessMap[key];
                ActiveHitList.Add(processName, InactiveHitList[processName]);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="seconds">Number of seconds the program should run</param>
        /// <param name="cancellationToken"></param>
        public void Hit(double seconds, CancellationToken cancellationToken)
        {

            List<Process> processes = new List<Process>();

            for (int i = 0; i < (seconds * 10); i++)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }

                processes = Process.GetProcesses().ToList();

                foreach (Process p in processes)
                {
                    foreach (KeyValuePair<string, IEnumerable<string>> kvp in ActiveHitList) // Updated to ActiveHitList
                    {
                        foreach (string s in kvp.Value)
                        {
                            if (p.ProcessName.IndexOf(s, StringComparison.OrdinalIgnoreCase) >= 0 && p.Id != Process.GetCurrentProcess().Id)
                            {
                                try
                                {
                                    Console.WriteLine($"Killing {p.ProcessName}");
                                    KillProcessAndChildren(p.Id);
                                }
                                catch
                                {
                                    // do nothing
                                }
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

            try
            {
                Process proc = Process.GetProcessById(pid);
                proc.Kill();
            }
            catch (ArgumentException e)
            {
                // Process already exited. 
            }
        }
    }
    internal class Program
    {
        static async Task Main(string[] args)
        {
            ProcessHitman hitman = new ProcessHitman();

            // Add League to ActiveHitList so it starts being terminated right away
            hitman.AddToHitList("League", "L", new List<string> { "League", "Riot" }, true);

            // Add others to InactiveHitList
            hitman.AddToHitList("Teams", "T", new List<string> { "Teams" });
            hitman.AddToHitList("Discord", "D", new List<string> { "Discord" });
            hitman.AddToHitList("Slack", "S", new List<string> { "Slack" });


            hitman.DisplayInstructions();

            KeyMonitor keyMonitor = new KeyMonitor();

            CancellationTokenSource cts = new CancellationTokenSource();

            Task keyMon = Task.Run(() => { keyMonitor.Monitor(hitman, cts); }, cts.Token);
            Task hitProcs = Task.Run(() => { hitman.Hit(10, cts.Token); }, cts.Token);

            await hitProcs;
        }
    }
}
