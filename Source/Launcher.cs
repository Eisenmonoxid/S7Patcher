using System;
using System.Diagnostics;
using System.Threading;

namespace S7Patcher.Source
{
    internal class Launcher
    {
        private const string ProcessName = "Settlers7R";
        private const string ProcessWindow = "Settlers 7 Window";
        public Launcher(string Path, IntPtr AffinityMask)
        {
            Process Settlers = new()
            {
                StartInfo = new ProcessStartInfo()
                {
                    FileName = Path,
                    UseShellExecute = false,
                },
            };

            Settlers.Start();
            WaitForProcess(AffinityMask);
        }

        private void WaitForProcess(IntPtr AffinityMask)
        {
            do
            {
                Console.WriteLine("Tick: Waiting for Processes ...");
                Process[] Processes = Process.GetProcessesByName(ProcessName);
                if (Processes.Length > 0)
                {
                    foreach (Process Element in Processes)
                    {
                        if (Element.MainWindowTitle == ProcessWindow)
                        {
                            Console.WriteLine("Found Process: " + Element.MainWindowTitle);

                            Element.PriorityClass = ProcessPriorityClass.High;
                            Element.ProcessorAffinity = AffinityMask;
                            Element.PriorityBoostEnabled = true;
                            return;
                        }
                    }
                }

                Thread.Sleep(500);
            }
            while (true);
        }
    }
}
