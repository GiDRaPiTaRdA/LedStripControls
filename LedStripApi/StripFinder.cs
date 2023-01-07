using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using System.Collections.Concurrent;

namespace LedStripApi
{
    public class StripFinder
    {
        Ping PingTool { get; }

        int timeout = 4000;

        public StripFinder()
        {
            this.PingTool = new Ping();
        }

        public string[] GetIps(IPAddress gateway, Range range) =>
             GetIps(gateway, (byte)range.Start.Value, (byte)range.End.Value);

        public string[] GetIps(IPAddress gateway, byte start, byte end)
        {
            string gatePart = GetGateWayPart(gateway);

            List<string> awailableIps = new List<string>();

            for (int i = start; i < end; i++)
            {
                string ip = CreateIp(gatePart, i);

                PingReply repl = PingTool.Send(ip, timeout);

                if (repl.Status == IPStatus.Success)
                    awailableIps.Add(ip);
            }

            return awailableIps.ToArray();

            string CreateIp(string ipPart, int i) => $"{ipPart}.{i}";
        }

        public string[] GetIps1(IPAddress gateway, Range range) =>
             GetIps1(gateway, (byte)range.Start.Value, (byte)range.End.Value);

        public string[] GetIps1(IPAddress gateway, byte start, byte end)
        {
            string gatePart = GetGateWayPart(gateway);

            ParallelQuery<string>? a = Enumerable.Range(start, end)
                .AsParallel()
                .Select(i => new Ping().Send(CreateIp(gatePart, i), timeout))
                .Where(rpl => rpl.Status == IPStatus.Success)
                .Select(r => r.Address.ToString());

            return a.ToList().ToArray();
        }

        public string[] GetIpsParralel(IPAddress gateway, Range range) =>
             GetIpsParralel(gateway, (byte)range.Start.Value, (byte)range.End.Value);

        public string[] GetIpsParralel(IPAddress gateway, byte start, byte end)
        {
            string gatePart = GetGateWayPart(gateway);

            int paralelism = Convert.ToInt32(Math.Ceiling(Environment.ProcessorCount * 0.75 * 2.0));

            Task[] tasks = new Task[paralelism];

            ConcurrentStack<byte> stack = new(RangeByte(start, end));

            ConcurrentBag<string> clients = new();

            for (int i = 0; i < tasks.Length; i++)
            {
                tasks[i] = Task.Factory.StartNew(() =>
                {
                    using (Ping ping = new())
                    {
                        while (stack.TryPop(out byte value))
                        {
                            string ip = CreateIp(gatePart, value);

                            PingReply reply = ping.Send(ip, timeout);

                            if (reply.Status == IPStatus.Success)
                            {
                                clients.Add(ip);
                            }
                        }
                    }
                });
            }

            Task.WaitAll(tasks);

            return clients.ToArray();
        }


        public IEnumerable<TCPConnection> ConnectTcpClients(string[] ips, int port)
        {
            List<TCPConnection> clients = new();

            foreach (string ip in ips)
            {
                TCPConnection connection = new();

                try
                {
                    if (connection.Connect(ip, port))
                        clients.Add(connection);
                }
                catch (Exception)
                {
                    connection.Dispose();
                }
            }

            return clients;
        }


        static IEnumerable<byte> RangeByte(int start, int end)
        {
            int count = end - start + 1;

            List<byte> arr = new List<byte>(count);

            for (int i = 0; i < count; i++)
            {
                arr.Add((byte)(i + start));
            }

            return arr;
        }

        public static string CreateIp(string ipPart, int i) => $"{ipPart}.{i}";

        public static string GetGateWayPart(IPAddress gateway) =>
            string.Join(".", gateway.ToString().Split('.').Take(3));

        static void DisplayGatewayAddresses()
        {
            Console.WriteLine("Gateways");
            NetworkInterface[] adapters = NetworkInterface.GetAllNetworkInterfaces();
            foreach (NetworkInterface adapter in adapters)
            {
                IPInterfaceProperties adapterProperties = adapter.GetIPProperties();
                GatewayIPAddressInformationCollection addresses = adapterProperties.GatewayAddresses;
                if (addresses.Count > 0)
                {
                    Console.WriteLine(adapter.Description);
                    foreach (GatewayIPAddressInformation address in addresses)
                    {
                        Console.WriteLine("  Gateway Address ......................... : {0}",
                            address.Address.ToString());
                    }
                    Console.WriteLine();
                }
            }
        }

        public GatewayIPAddressInformation? GetGateway()
        {
            NetworkInterface[] adapters = NetworkInterface.GetAllNetworkInterfaces();

            GatewayIPAddressInformation? a = adapters
                .Select(a => a.GetIPProperties().GatewayAddresses)
                .FirstOrDefault(ga => ga.Count > 0)?
                .FirstOrDefault();

            return a;
        }
    }
}
