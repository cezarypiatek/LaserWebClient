using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using SocketIOClient;

namespace SmartLaserCutting
{


    public class ServerConfig
    {
        public int webPort { get; set; }
        public string IP { get; set; }
        public string serverVersion { get; set; }
        public string apiVersion { get; set; }
        public int verboseLevel { get; set; }
        public int logLevel { get; set; }
        public int resetOnConnect { get; set; }
        public int mpgType { get; set; }
        public int socketMaxDataSize { get; set; }
        public string socketCorsOrigin { get; set; }
        public int socketPingTimeout { get; set; }
        public int socketPingInterval { get; set; }
        public int posDecimals { get; set; }
        public int firmwareWaitTime { get; set; }
        public int grblWaitTime { get; set; }
        public int smoothieWaitTime { get; set; }
        public int tinygWaitTime { get; set; }
        public int grblBufferSize { get; set; }
        public int smoothieBufferSize { get; set; }
        public int tinygBufferSize { get; set; }
        public int reprapBufferSize { get; set; }
        public string jobOnStart { get; set; }
        public string jobOnFinish { get; set; }
        public string jobOnAbort { get; set; }
        public string uipath { get; set; }
    }


    public class Port
    {
        public string manufacturer { get; set; }
        public string pnpId { get; set; }
        public string vendorId { get; set; }
        public string productId { get; set; }
        public string path { get; set; }
    }


    public class GetCommand<T>
    {
        private readonly SocketIO _client;
        private readonly string _commandName;
        private readonly string _responseName;
        private readonly string? _parameter;

        public GetCommand(SocketIO client, string commandName, string responseName, string? parameter)
        {
            _client = client;
            _commandName = commandName;
            _responseName = responseName;
            _parameter = parameter;
        }

        public async Task<T> Execute()
        {
            var ts = new TaskCompletionSource<T>();
            _client.On(_responseName, response =>
            {
                var x = this;
                var result = response.GetValue<T>();
                _client.Off(_responseName);
                ts.SetResult(result);
            });
            if (_parameter != null)
                await _client.EmitAsync(_commandName, _parameter);
            else
            {
                await _client.EmitAsync(_commandName);
            }
            return await ts.Task;
        }
    }  
    
    public class GetCommand
    {
        private readonly SocketIO _client;
        private readonly string _commandName;
        private readonly string? _parameter;

        public GetCommand(SocketIO client, string commandName, string? parameter)
        {
            _client = client;
            _commandName = commandName;
            _parameter = parameter;
        }

        public async Task Execute()
        {
            if (_parameter != null)
                await _client.EmitAsync(_commandName, _parameter);
            else
            {
                await _client.EmitAsync(_commandName);
            }
        }
    }

    public class LaserWebClient
    {
        private readonly string _serverUrl;
        private readonly SocketIO _client;

        public LaserWebClient(string serverUrl)
        {
            _serverUrl = serverUrl;
            _client = new SocketIO(serverUrl);
            _client.OnAny((name, response) =>
            {

            });
            
            _client.OnConnected += (sender, args) =>
            {
                Console.WriteLine("Connected");
                if (_connectTask != null)
                {
                    _connectTask.SetResult(true);
                }
            };
            _client.OnError += (sender, s) =>
            {
                Console.WriteLine("Error");
            };
        }

        public async Task Connect()
        {
            _connectTask = new TaskCompletionSource<bool>();
            await _client.ConnectAsync();
            await _connectTask.Task;
            _connectTask = null;
        }

        private TaskCompletionSource<bool>? _connectTask;
        public async Task<ServerConfig> GetServerConfig() => await Execute<ServerConfig>("getServerConfig", "serverConfig");
        public async Task<string[]> GetInterfaces() => await Execute<string[]>("getInterfaces", "interfaces");
        public async Task<Port[]> GetPorts() => await Execute<Port[]>("getPorts", "ports");
        public async Task<string> GetConnectStatus() => await Execute<string>("getConnectStatus", "connectStatus");
        public async Task Jog(Axis axis, int distance, int speed) => await Execute("jog",  $"{axis},{distance},{speed}");
        public async Task DisconnectPort() => await Execute("closePort");

        public async Task<string> ConnectToPort(Port p, int baudRate) =>
            await Execute<string>("connectTo", "data", $"USB,{p.path},{baudRate}");


        private async Task<T> Execute<T>(string commandName, string responseName, string? parameter = null)
        {
            var command = new GetCommand<T>(this._client, commandName, responseName, parameter);
            return await command.Execute();
        }
        private async Task Execute(string commandName, string? parameter = null)
        {
            var command = new GetCommand(this._client, commandName,  parameter);
            await command.Execute();
        }
    }

    public enum Axis
    {
        X,
        Y,
        Z
    }
}
