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
        public void Monitor(ProcessHitman killer)
        {
            killer.AddToHitList("League", "L", new List<string> { "League", "Riot" });

            ConsoleKeyInfo cki = new ConsoleKeyInfo();
            do
            {
                cki = Console.ReadKey(true);

                try
                {
                    killer.TryAddProcessByKey(cki.Key.ToString());
                }
                catch
                {
                    // Handle duplicate entry or other exceptions
                }

            } while (cki.Key != ConsoleKey.Spacebar);

            Environment.Exit(0);
        }
    }


    public class ProcessHitman
    {
        public Dictionary<string, IEnumerable<string>> HitList = new Dictionary<string, IEnumerable<string>>();
        private readonly Dictionary<string, string> KeyToProcessMap = new Dictionary<string, string>();

        public void AddToHitList(string processName, string key, List<string> processList)
        {
            HitList.Add(processName, processList);
            KeyToProcessMap.Add(key, processName);
        }

        public void DisplayInstructions()
        {
            foreach (var key in KeyToProcessMap.Keys)
            {
                Console.WriteLine($"Press {key} to exit {KeyToProcessMap[key]}");
            }
        }

        public void TryAddProcessByKey(string key)
        {
            if (KeyToProcessMap.ContainsKey(key))
            {
                string processName = KeyToProcessMap[key];
                HitList.Add(processName, new List<string> { processName });
            }
        }

        public void Hit(double seconds)
        {

            List<Process> processes = new List<Process>();

            for (int i = 0; i < (seconds * 10); i++)
            {
                processes = Process.GetProcesses().ToList();

                ////Debugging:
                ////show the list of killable processes
                //foreach (var k in Killables)
                //{
                //    foreach (var kk in k.Value)
                //    {
                //        Console.WriteLine(kk);
                //    }
                //}

                ////show list of current processes
                //foreach (var k in processes)
                //{
                //    Console.WriteLine(k.Id + " " + k.ProcessName);
                //}

                foreach (Process p in processes)
                {
                    foreach (KeyValuePair<string, IEnumerable<string>> kvp in HitList)
                    {
                        foreach (string s in kvp.Value)
                        {
                            if (p.ProcessName.Contains(s) && p.Id != Process.GetCurrentProcess().Id)
                            {
                                try
                                {
                                    Console.WriteLine($"Killing {string.Join(", ", kvp.Value)}");

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
            Console.WriteLine("Press spacebar any time to quit" + Environment.NewLine);

            ProcessHitman hitman = new ProcessHitman();
            hitman.AddToHitList("Teams", "T", new List<string> { "Teams" });
            hitman.AddToHitList("Discord", "D", new List<string> { "Discord" });
            hitman.AddToHitList("Slack", "S", new List<string> { "Slack" });

            hitman.DisplayInstructions();

            KeyMonitor keyMonitor = new KeyMonitor();

            Task keyMon = Task.Run(() => { keyMonitor.Monitor(hitman); });
            Task hitProcs = Task.Run(() => { hitman.Hit(10); });

            await hitProcs;
        }
    }
}
