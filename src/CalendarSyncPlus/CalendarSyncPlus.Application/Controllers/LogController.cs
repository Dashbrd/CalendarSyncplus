using System.Collections.Specialized;
using System.ComponentModel.Composition;
using CalendarSyncPlus.Application.ViewModels;

namespace CalendarSyncPlus.Application.Controllers
{
    [Export(typeof (ILogController))]
    public class LogController : ILogController
    {
        [ImportingConstructor]
        public LogController(LogViewModel logViewModel)
        {
            LogViewModel = logViewModel;
        }

        public LogViewModel LogViewModel { get; set; }

        public void Initialize()
        {
            CollectionChangedEventManager.AddHandler(LogViewModel.AppliedFilterList, OnFilterChanged);
        }

        public void Run(bool startMinimized)
        {
        }

        public void Shutdown()
        {
            CollectionChangedEventManager.RemoveHandler(LogViewModel.AppliedFilterList, OnFilterChanged);
        }

        private void OnFilterChanged(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            LogViewModel.ApplyFilters();
        }
    }
}