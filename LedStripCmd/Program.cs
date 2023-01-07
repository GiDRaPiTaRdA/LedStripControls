




using LedStripApi;
using System.Net;
using System.Net.NetworkInformation;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;


Range range = new Range(1, 255);
int port = 5045;

var b = Dns.GetHostName();

var a = Dns.GetHostEntry(b);

StripFinder finder = new StripFinder();

var address = finder.GetGateway().Address;

Console.WriteLine($"Default gateway {address}");

Console.WriteLine($"Start LAN search " +
    $"{StripFinder.CreateIp(StripFinder.GetGateWayPart(address), range.Start.Value)}-" +
    $"{StripFinder.CreateIp(StripFinder.GetGateWayPart(address), range.End.Value)}");



// Search 
string[] ips;


//ips = new string[] { "192.168.0.114" };

{
    Stopwatch s = Stopwatch.StartNew();
    ips = finder.GetIpsParralel(address, range);
    s.Stop();

    ips?.ForEach(Console.WriteLine);
    Console.WriteLine($"Hosts up : {ips.Length}");
    Console.WriteLine($"Elapsed {s.ElapsedMilliseconds / 1000} seconds");
}


// Connect
IEnumerable<TCPConnection> clients;
{
    Console.WriteLine($"Connecting TCP IPs...");
    clients = finder.ConnectTcpClients(ips, port);
    Console.WriteLine($"Connected : { clients.Count()}");

    clients.ForEach(c=>Console.WriteLine($"{c.Host}:{c.Port}"));
}

Console.ReadLine();

IEnumerable<LedStrip>? apis = clients.Where(c => c != null).Select(c => new LedStrip(c));
while (true)
{
    string cmd = Console.ReadLine();
    if (cmd == "exit")
        return;
    else if (cmd?.Any() ?? false)
    {
        apis.ForEach(api => api.SendCommand(cmd));
    }
}


void TrafficLight(LedStrip api)
{
    Thread.Sleep(5000);
    api.SendCommand("color Blue");
    Thread.Sleep(2000);
    api.SendCommand("color Red");
    Thread.Sleep(2000);
    api.SendCommand("color Yellow");
}



Console.ReadLine();

// Dispose
clients.ForEach(c => c.Dispose());
Console.WriteLine($"Disposed : { clients.Count()}");

Console.ReadLine();