using System;
using System.ComponentModel.Composition;
using CalendarSyncPlus.Domain.Models;
using CalendarSyncPlus.Services.Sync.Interfaces;
using Microsoft.AspNet.SignalR.Client;

namespace CalendarSyncPlus.Services.Sync
{
    [Export(typeof(IAnnouncementService))]
    public class AnnouncementService : IAnnouncementService
    {
        public async void Start()
        {
            var hubConnection = new HubConnection("http://www.contoso.com/");
            var stockTickerHubProxy = hubConnection.CreateHubProxy("StockTickerHub");
            stockTickerHubProxy.On<Announcement>("GetAnnouncement",
                announcement => Console.WriteLine("Announcement received"));
            await hubConnection.Start();
        }
    }
}