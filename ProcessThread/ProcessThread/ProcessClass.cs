using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProcessThread
{
    internal class ProcessClass
    {
        public Process[] GetAllProcesses()
        {
            var processes = Process.GetProcesses();
            Console.Clear();
            foreach (var proc in processes)
            {
                Console.WriteLine($"{proc.Id} {proc.ProcessName} {proc.StartTime}");
            }
            Console.ReadKey();
            Console.Clear();

            return processes;
        }
        public ProcessThreadCollection GetProcessThreads(Process proc)
        {
            var threads = proc.Threads;

            Console.Clear();
            foreach (System.Diagnostics.ProcessThread thread in threads)
            {
                Console.WriteLine($"{thread.Id} {thread.PriorityLevel} {thread.StartTime}");
            }

            Console.ReadKey();
            Console.Clear();

            return threads;
        }
        public ProcessThreadCollection GetProcessThreads(int processId)
        {
            var proc = Process.GetProcessById(processId);
            var threads = proc.Threads;

            Console.Clear();
            foreach (System.Diagnostics.ProcessThread thread in threads)
            {
                Console.WriteLine($"{thread.Id} {thread.PriorityLevel} {thread.StartTime}");
            }

            Console.ReadKey();
            Console.Clear();

            return threads;
        }
        public Process GetProcessById(int processId)
        {
            var proc = Process.GetProcessById(processId);
            Console.WriteLine($"{proc.Id} {proc.ProcessName} {proc.StartTime}");
            return proc;
        }
        public Process StartProcess(string path)
        {
            var proc = Process.Start(path);
            return proc;
        }
        public void KillProcess(Process proc)
        {
            proc.Kill();
            Console.WriteLine($"Process {proc.ProcessName} was killed.");
        }
        public void KillProcess(int processId)
        {
            var proc = Process.GetProcessById(processId);
            proc.Kill();
            Console.WriteLine($"Process {proc.ProcessName} was killed.");
        }
        public ProcessModuleCollection ShowModulesInfo(Process proc)
        {
            var modules = proc.Modules;
            Console.Clear();
            foreach (ProcessModule module in modules)
            {
                Console.WriteLine($"{module.ModuleName} {module.FileVersionInfo}");
            }
            Console.ReadKey();
            Console.Clear();

            return modules;
        }
        public ProcessModuleCollection ShowModulesInfo(int processId)
        {
            var proc = Process.GetProcessById(processId);
            var modules = proc.Modules;
            Console.Clear();
            foreach (ProcessModule module in modules)
            {
                Console.WriteLine($"{module.ModuleName} {module.FileVersionInfo}");
            }
            Console.ReadKey();
            Console.Clear();

            return modules;
        }
    }
}
