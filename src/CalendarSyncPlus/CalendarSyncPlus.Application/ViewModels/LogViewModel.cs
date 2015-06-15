using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Waf.Applications;
using System.Waf.Foundation;
using System.Windows.Data;
using CalendarSyncPlus.Application.Views;
using CalendarSyncPlus.Common;
using CalendarSyncPlus.Common.Log.Parser;
using log4net.Core;

namespace CalendarSyncPlus.Application.ViewModels
{
    [Export]
    public class LogViewModel : ViewModel<ILogView>
    {
        private ObservableCollection<LogItem> _logItems=new ObservableCollection<LogItem>();
        private ObservableCollection<LogItem> _filteredLogItemsView= new ObservableCollection<LogItem>();

        private ObservableCollection<LogFilter> _logFilters = new ObservableCollection<LogFilter>();
        private ObservableCollection<LogFilter> _appliedFilterList = new ObservableCollection<LogFilter>(); 
        private LogItem _selectedLogItem;
        private bool _isLoading;
        private DelegateCommand _modifyFitlerCommand;
        private DelegateCommand _loadLogCommand;


        [ImportingConstructor]
        public LogViewModel(ILogView view)
            : base(view)
        {
            CreateFilters();
            CreateDefaultFilter();
        }

        private void CreateDefaultFilter()
        {
           var uiFilter =  LogFilters.FirstOrDefault(filter => filter.FilterType == LogLevel.Error);
            if (uiFilter!=null)
            {
                uiFilter.IsSelected = true;
                AppliedFilterList.Add(uiFilter);

            }
        }

        private void CreateFilters()
        {
            foreach (LogLevel logLevel in Enum.GetValues(typeof(LogLevel)).Cast<LogLevel>())
            {
                LogFilters.Add(new LogFilter
                {
                    FilterType=logLevel,
                    IsEnabled=true
                });
            }
        }

        public ObservableCollection<LogItem> LogItems
        {
            get { return _logItems; }
            set { SetProperty(ref _logItems, value); }
        }

        public LogItem SelectedLogItem
        {
            get { return _selectedLogItem; }
            set { SetProperty(ref _selectedLogItem, value); }
        }

        public bool IsLoading
        {
            get { return _isLoading; }
            set { SetProperty(ref _isLoading, value); }
        }

        public ObservableCollection<LogFilter> LogFilters
        {
            get { return _logFilters; }
            set { SetProperty(ref _logFilters, value); }
        }

        public DelegateCommand ModifyFitlerCommand
        {
            get { return _modifyFitlerCommand ?? new DelegateCommand(ModifyFilter); }
        }

        public ObservableCollection<LogFilter> AppliedFilterList
        {
            get { return _appliedFilterList; }
            set { SetProperty(ref _appliedFilterList, value); }
        }

        public DelegateCommand LoadLogCommand
        {
            get { return _loadLogCommand ?? new DelegateCommand(LoadLog); }
        }

        public ObservableCollection<LogItem> FilteredLogItemsView
        {
            get { return _filteredLogItemsView; }
            set { SetProperty(ref _filteredLogItemsView, value); }
        }


        private void LoadLog()
        {
            IsLoading = true;
            Task<IEnumerable<LogItem>>.Factory.StartNew(GetLogItems).ContinueWith(FillLogItems);
        }

        private void FillLogItems(Task<IEnumerable<LogItem>> task)
        {
            LogItems = new ObservableCollection<LogItem>(task.Result);
            ApplyFilters();
            IsLoading = false;
        }

        private IEnumerable<LogItem> GetLogItems()
        {
            var parser = new LogParser();
            var items =
                parser.Parse(Environment.ExpandEnvironmentVariables(@"%APPDATA%\CalendarSyncPlus\Log\CalSyncPlusLog.xml"));
            return items;
        }

        private void ModifyFilter(object selectedFilter)
        {
            var filter = selectedFilter as LogFilter;
            if (filter ==null)
            {
                return;
            }
            if (filter.IsSelected)
            {
                if (AppliedFilterList.Contains(filter)) return;
                AppliedFilterList.Add(filter);
            }
            else
            {
                if (!AppliedFilterList.Contains(filter)) return;
                AppliedFilterList.Remove(filter);
            }
        }

        public void ApplyFilters()
        {
            DispatcherHelper.CheckBeginInvokeOnUI(() =>
            {
                FilteredLogItemsView.Clear();
                FilteredLogItemsView = new ObservableCollection<LogItem>(LogItems.Where(GetFilteredItem));
                if (FilteredLogItemsView.Any())
                {
                    SelectedLogItem = FilteredLogItemsView.Last();
                }
            });
        }

        private bool GetFilteredItem(LogItem logItem)
        {
            return AppliedFilterList.Any(filter => filter.FilterType == logItem.Level);
        }
    }

    public class LogFilter : Model
    {
        private LogLevel filterType;
        private bool isSelected;
        private bool isEnabled;

        public LogLevel FilterType
        {
            get { return filterType; }
            set { SetProperty(ref filterType, value); }
        }

        public bool IsSelected
        {
            get { return isSelected; }
            set { SetProperty(ref isSelected, value); }
        }

        public bool IsEnabled
        {
            get { return isEnabled; }
            set { SetProperty(ref isEnabled, value); }
        }
    }
}
