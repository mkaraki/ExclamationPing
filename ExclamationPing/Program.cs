using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace ExclamationPing
{
    internal class Program
    {
        
        private static readonly List<int> res = new();

        private static int ProceededCount = 0;

        static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("% Bad IP address");
                Environment.Exit(1);
                return;
            }

            string addr = args[0];

            int bytelen = 32;
            int timeout = 2000;
            int count = 5;
            bool no_fragment = false;

            for (int i = 1; i < args.Length; i += 2)
            {
                string optn = args[i];
                string data = null;
                if (i + 1 < args.Length)
                    data = args[i + 1];

                bool isint = int.TryParse(data, out int intdata);
                
                switch (optn)
                {
                    case "timeout-in-ms":
                        if (!isint) continue;
                        timeout = intdata;
                        break;

                    case "timeout":
                        if (!isint) continue;
                        timeout = intdata * 1000;
                        break;

                    case "repeat":
                        if (!isint) continue;
                        count = intdata;
                        break;

                    case "size":
                        if (!isint) continue;
                        bytelen = intdata;
                        break;

                    case "df-bit":
                        i--;
                        no_fragment = true;
                        break;
                }
            }

            if (!IPAddress.TryParse(addr, out var ipaddrobj))
            {
                if (Uri.CheckHostName(addr) == UriHostNameType.Unknown)
                {
                    Console.WriteLine($"% Unrecognized host or address.");
                }
                Console.Write($"Translating \"{addr}\"...");

                IPHostEntry host;
                try
                {
                    host = Dns.GetHostEntry(addr);
                }
                catch (SocketException)
                {
                    Console.WriteLine();
                    Console.WriteLine($"% Unrecognized host or address.");
                    Environment.Exit(2);
                    return;
                }

                Console.WriteLine(" [OK]");

                addr = host.AddressList.First().ToString();
            }

            Console.WriteLine("Type escape sequence to abort.");
            Console.WriteLine($"Sending {count}, {bytelen}-byte ICMP Echos to {addr}, timeout is {timeout / 1000f} seconds:");

            Console.CancelKeyPress += Console_CancelKeyPress;

            byte[] sendbyte = new byte[bytelen];
            Ping sender = new Ping();
            PingOptions options = new PingOptions() {
                DontFragment = no_fragment,
            };

            if (no_fragment)
                Console.WriteLine("Packet sent with the DF bit set");

            for (int i = 0; i < count; i++)
            {
                PingReply reply;
                try
                {
                    reply = sender.Send(addr, timeout, sendbyte, options);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("% " + ex.Message);
                    Environment.Exit(3);
                    return;
                }

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

                    case IPStatus.SourceQuench:
                        Console.Write('Q');
                        break;

                    case IPStatus.PacketTooBig:
                        Console.Write('M');
                        break;
                }
                ProceededCount++;
            }

            Console_CancelKeyPress(null, null);
        }

        private static void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            Console.WriteLine();
            Console.Write($"Success rate is {res.Count * 100 / ProceededCount} percent ({res.Count}/{ProceededCount})");
            if (res.Count > 0)
                Console.Write($", round-trip min/avg/max = {res.Min()}/{(int)res.Average()}/{res.Max()} ms");

            Console.WriteLine();
        }
    }
}
