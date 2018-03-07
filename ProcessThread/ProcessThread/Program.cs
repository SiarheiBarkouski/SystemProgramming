using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;

namespace ProcessThread
{
    class Program
    {
        static void Main(string[] args)
        {
            Menu(new ProcessClass());
        }



        private static void Menu(ProcessClass procClass)
        {
            while (true)
            {
                try
                {
                    Console.WriteLine("1. Get all processes.");
                    Console.WriteLine("2. Get process  by PID.");
                    Console.WriteLine("3. Get process threads.");
                    Console.WriteLine("4. Start process.");
                    Console.WriteLine("5. Kill process.");
                    Console.WriteLine("6. Show process modules.");
                    Console.Write("Enter your choice: ");

                    var result = Int32.TryParse(Console.ReadLine(), out int choice);
                    if (result)
                    {
                        switch (choice)
                        {
                            case 1:
                                procClass.GetAllProcesses();
                                break;
                            case 2:
                                procClass.GetProcessById(Convert.ToInt32(Console.ReadLine()));
                                break;
                            case 3:
                                procClass.GetProcessThreads(Convert.ToInt32(Console.ReadLine()));
                                break;
                            case 4:
                                procClass.StartProcess(Console.ReadLine());
                                break;
                            case 5:
                                procClass.KillProcess(Convert.ToInt32(Console.ReadLine()));
                                break;
                            case 6:
                                procClass.ShowModulesInfo(Convert.ToInt32(Console.ReadLine()));
                                break;
                            case 0:
                                Process.GetCurrentProcess().Kill();
                                break;
                            default: Console.Clear(); continue;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    Console.WriteLine("Press any key to continue...");
                    Console.ReadKey();
                    Console.Clear();
                }
            }
        }


    }
}
