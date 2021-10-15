using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;

namespace ExclamationPing
{
    internal class Program
    {
        
        private static List<int> res = new List<int>();

        private static int ProceededCount = 0;

        static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("% Bad IP address");
                return;
            }

            string addr = args[0];

            int bytelen = 32;
            int timeout = 2000;
            int count = 5;

            for (int i = 1; i + 1 < args.Length; i += 2)
            {
                string optn = args[i];
                string data = args[i + 1];

                bool isint = int.TryParse(data, out int intdata);
                
                switch (optn)
                {
                    case "timeout":
                        if (!isint) continue;
                        timeout = intdata;
                        break;

                    case "repeat":
                        if (!isint) continue;
                        count = intdata;
                        break;

                    case "size":
                        if (!isint) continue;
                        bytelen = intdata;
                        break;
                }
            }

            Console.WriteLine("Type escape sequence to abort.");
            Console.WriteLine($"Sending {count}, {bytelen}-byte ICMP Echos to {addr}, timeout is {timeout} ms:");

            Console.CancelKeyPress += Console_CancelKeyPress;

            byte[] sendbyte = new byte[bytelen];
            Ping sender = new Ping();
            for (int i = 0; i < count; i++)
            {
                PingReply reply = sender.Send(addr, timeout, sendbyte);
                switch (reply.Status)
                {
                    case IPStatus.Success:
                        Console.Write('!');
                        res.Add((int)reply.RoundtripTime);
                        break;

                    case IPStatus.TimedOut:
                        Console.Write('.');
                        break;

                    case IPStatus.DestinationUnreachable:
                        Console.Write('U');
                        break;

                    case IPStatus.Unknown:
                        Console.Write('?');
                        break;

                    case IPStatus.TtlExpired:
                        Console.Write('&');
                        break;
                }
                ProceededCount++;
            }

            Console_CancelKeyPress(null, null);
        }

        private static void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            Console.WriteLine();
            Console.WriteLine($"Success rate is {res.Count * 100 / ProceededCount} percent ({res.Count}/{ProceededCount}), round-trip min/avg/max = {res.Min()}/{(int)res.Average()}/{res.Max()} ms");
        }
    }
}
