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
        public void MonitorKeypress(Kil killer)
        {
            killer.Killables.Add("League", new List<String> { "League", "Riot" });

            ConsoleKeyInfo cki = new ConsoleKeyInfo();
            do
            {
                // true hides the pressed character from the console
                cki = Console.ReadKey(true);

                try
                {
                    if (cki.Key == ConsoleKey.Z)
                    {
                        killer.Killables.Add("Zoom", new List<String> { "Zoom" });
                        
                    }
                    else if (cki.Key == ConsoleKey.M)
                    {
                        killer.Killables.Add("Mtg", new List<String> { "MTGA" });
                        
                    }
                    else if (cki.Key == ConsoleKey.O)
                    {
                        killer.Killables.Add("OBS Studio", new List<String> { "obs" });

                    }
                    else if (cki.Key == ConsoleKey.D)
                    {
                        killer.Killables.Add("Discord", new List<String> { "Discord" });
                       
                    }
                    else if (cki.Key == ConsoleKey.S)
                    {
                        killer.Killables.Add("Slack", new List<String> { "slack" });

                    }
                    else if (cki.Key == ConsoleKey.T)
                    {
                        killer.Killables.Add("Teams", new List<String> { "Teams" });

                    }
                }
                catch
                {
                    // do nothing
                    // it's a duplicate entry error caused by holding down the button.
                }                

            // Wait for spacebar
            } while (cki.Key != ConsoleKey.Spacebar);

            Environment.Exit(0);
        }
    }
    
    public class Kil
    {
        public Dictionary<String, IEnumerable<String>> Killables = new Dictionary<string, IEnumerable<string>>();
        public void KillKillables(double seconds)
        {

            List<Process> processes = new List<Process>();

            for (int i = 0; i < (seconds * 10); i++)
            {
                processes = Process.GetProcesses().ToList();

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
                    foreach (KeyValuePair<string,IEnumerable<String>> kvp in Killables)
                    {
                        foreach (string s in kvp.Value)
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

            //foreach (ManagementObject mo in moc)
            //{
            //    KillProcessAndChildren(Convert.ToInt32(mo["ProcessID"]));
            //}

            try
            {
                Process proc = Process.GetProcessById(pid);
                proc.Kill();
            }
            catch (ArgumentException e)
            {
                var x = e;
                // Process already exited. 
            }
        }
    }
    internal class Program
    {
        static async Task Main(string[] args)
        {

            Console.WriteLine("Press Z to kill Zoom");
            Console.WriteLine("Press M to kill MtG Arena");
            Console.WriteLine("Press O to kill OBS Studio");
            Console.WriteLine("Press S Slack");
            Console.WriteLine("Press T Teams");
            Console.WriteLine("Press spacebar any time to quit" + Environment.NewLine);

            Kil killer = new Kil();
            Mon monitor = new Mon();

            Task monitorKeyPressTask = Task.Run(() => { monitor.MonitorKeypress(killer); });
            Task killLeagueProcsTask = Task.Run(() => { killer.KillKillables(10); });

            await killLeagueProcsTask;
            
        }
    }
}
