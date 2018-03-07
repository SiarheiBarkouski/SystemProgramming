using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Sync
{
    class Program
    {
        private static readonly object _locker1 = new object();
        private static readonly object _locker2 = new object();

        static void Main(string[] args)
        {
            var s = new List<int>();
            var r = new List<long>();
            int counter = 0;

            var a = new Thread(() =>
            {
                while (true)
                {
                    lock (_locker1)
                    {
                        s.Add(++counter);
                    }
                }
            });

            var b = new Thread(() =>
            {
                int x;
                while (true)
                {
                    if (s.Count == 0)
                        Thread.Sleep(1000);
                    else
                    {
                        lock (_locker1)
                        {
                            Thread.Sleep(1500);
                            lock (_locker2)
                            {
                                x = s.Last();
                                r.Add(x * x);
                            }
                        }
                    }
                }
            });

            var c = new Thread(() =>
            {
                int x;
                while (true)
                {
                    if (s.Count == 0)
                    {
                        Thread.Sleep(1000);
                    }
                    else
                    {
                        lock (_locker2)
                        {
                            Thread.Sleep(1500);
                            lock (_locker1)
                            {
                                x = s.Last();
                                r.Add(x / 3);
                            }
                        }

                    }
                }
            });

            var d = new Thread(() =>
            {
                while (true)
                {
                    if (r.Count == 0)
                    {
                        Console.WriteLine("List R is empty.");
                        Thread.Sleep(1000);
                    }
                    else
                    {
                        lock (_locker2)
                        {
                            Console.WriteLine(r.Last());
                        }
                    }
                }
            });

            new Thread(() =>
            {
                a.Start();
                b.Start();
                c.Start();
                d.Start();
            }).Start();

            Console.ReadKey();
        }
    }
}