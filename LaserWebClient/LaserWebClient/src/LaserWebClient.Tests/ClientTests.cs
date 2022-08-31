using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using SmartLaserCutting;

namespace LaserWebClient.Tests
{
    public class ClientTests
    {
        [Test]
        public async Task should_connect_to_server()
        {
            var client = new SmartLaserCutting.LaserWebClient("ws://raspberrypi:8000");
            await client.Connect();
            var serverConfig = await client.GetServerConfig();
            var interfaces = await client.GetInterfaces();
            var ports = await client.GetPorts();
            var data = await client.ConnectToPort(ports[1], 115200);
            await client.GetConnectStatus();
            await client.Jog(Axis.X, 10, 1800);
            await client.Jog(Axis.Y, 10, 1800);
            await client.Jog(Axis.X, 10, 1800);
            await client.Jog(Axis.Y, 10, 1800);
            await client.DisconnectPort();
        }
    }
}
