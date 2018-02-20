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


        static void Main(string[] args)
        {
            var counter = 0;
            var s = new List<long>();
            var r = new List<long>();

            var locker1 = new object();
            var locker2 = new object();


            var a = new Thread(() =>
                    {
                        while (true)
                        {
                            lock (locker1)
                            {
                                s.Add(counter++);
                            }
                        }
                    });

            var b = new Thread(() =>
            {
                while (true)
                {
                    long result;
                    long count;

                    lock (locker1)
                    {
                        count = s.Count;
                    }

                    if (count != 0)
                    {
                        lock (locker1)
                        {
                            result = s.Last() * s.Last();
                        }
                        lock (locker2)
                        {
                            r.Add(result);
                        }
                    }
                    else
                        Thread.Sleep(1000);
                }
            });

            var c = new Thread(() =>
            {
                while (true)
                {
                    long result;
                    long count;

                    lock (locker1)
                    {
                        count = s.Count;
                    }

                    if (count != 0)
                    {
                        lock (locker1)
                        {
                            result = s.Last() / 3;
                        }
                        lock (locker2)
                        {
                            r.Add(result);
                        }
                    }
                    else
                        Thread.Sleep(1000);
                }
            });

            var d = new Thread(() =>
            {
                while (true)
                {
                    long count;

                    lock (locker1)
                    {
                        count = s.Count;
                    }

                    if (count != 0)
                    {
                        lock (locker1)
                        {
                            Console.WriteLine(s.Last());
                        }
                    }
                    else
                    {
                        Console.WriteLine("List R is empty.");
                        Thread.Sleep(1000);
                    }
                }
            });

            a.Start();
            b.Start();
            c.Start();
            d.Start();

            Console.ReadKey();
        }
    }
}
