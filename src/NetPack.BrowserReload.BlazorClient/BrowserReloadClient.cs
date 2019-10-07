using BlazorSignalR;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.JSInterop;
using System;
using System.Threading.Tasks;

namespace NetPack.BrowserReload.BlazorClient
{
    public class BrowserReloadClient : IDisposable
    {

        private HubConnection _connection;

        public event EventHandler Reload;

        public BrowserReloadClient(IJSRuntime jsRuntime, NavigationManager navManager, string hubPath)
        {

            _connection = new HubConnectionBuilder()
                .WithUrlBlazor(hubPath, jsRuntime, navManager, options: opt =>
                   {
                       //opt.AccessTokenProvider = () =>
                       //{
                       //    return "some token for example";
                       //};
                   }).Build();           

            _connection.Closed += Connection_Closed;

            _connection.On("Reload", () =>
            {
                OnReload();
            });
        }

        private async Task Connection_Closed(Exception arg)
        {
            // Attempt reconnect after 2 seconds.
            Console.WriteLine("stopping will attempt reconnect..");
            await Task.Delay(2000);
            await _connection.StartAsync();
        }

        public void OnReload()
        {
            Reload?.Invoke(this, EventArgs.Empty);
        }

        public async Task StartListening()
        {
            if (_connection.State == HubConnectionState.Disconnected)
            {
                await _connection.StartAsync();
            }
        }

        public void Dispose()
        {
            _connection.Closed -= Connection_Closed;
            _connection.DisposeAsync();
        }
    }
}
