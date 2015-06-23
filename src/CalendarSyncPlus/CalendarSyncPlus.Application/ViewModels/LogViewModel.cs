using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;
using System.Waf.Applications;
using System.Waf.Applications.Services;
using System.Waf.Foundation;
using CalendarSyncPlus.Application.Views;
using CalendarSyncPlus.Common;
using CalendarSyncPlus.Common.Log.Parser;
using IFileDialogService = CalendarSyncPlus.Services.Interfaces.IFileDialogService;

namespace CalendarSyncPlus.Application.ViewModels
{
    [Export]
    public class LogViewModel : ViewModel<ILogView>
    {
        private ObservableCollection<LogFilter> _appliedFilterList = new ObservableCollection<LogFilter>();
        private ObservableCollection<LogItem> _filteredLogItemsView = new ObservableCollection<LogItem>();
        private bool _isCurrentFileNew;
        private bool _isLoading;
        private DelegateCommand _loadLogCommand;
        private ObservableCollection<LogFilter> _logFilters = new ObservableCollection<LogFilter>();
        private ObservableCollection<LogItem> _logItems = new ObservableCollection<LogItem>();
        private DelegateCommand _modifyFitlerCommand;
        private LogItem _selectedLogItem;
        private DelegateCommand _selectLogFileCommand;
        private string currentFileName = @"%APPDATA%\CalendarSyncPlus\Log\CalSyncPlusLog.xml";

        [ImportingConstructor]
        public LogViewModel(ILogView view, IFileDialogService fileDialogService)
            : base(view)
        {
            FileDialogService = fileDialogService;
            CreateFilters();
            CreateDefaultFilter();
        }

        public IFileDialogService FileDialogService { get; set; }

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

        public DelegateCommand ModifyFilterCommand
        {
            get { return _modifyFitlerCommand ?? new DelegateCommand(ModifyFilter); }
        }

        public DelegateCommand SelectLogFileCommand
        {
            get { return _selectLogFileCommand ?? new DelegateCommand(SelectLogFile); }
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

        public bool IsCurrentFileNew
        {
            get { return _isCurrentFileNew; }
            set { SetProperty(ref _isCurrentFileNew, value); }
        }

        public string CurrentFileName
        {
            get { return currentFileName; }
            set { SetProperty(ref currentFileName, value); }
        }

        private void CreateDefaultFilter()
        {
            var uiFilter = LogFilters.FirstOrDefault(filter => filter.FilterType == LogLevel.Error);
            if (uiFilter != null)
            {
                uiFilter.IsSelected = true;
                AppliedFilterList.Add(uiFilter);
            }

            uiFilter = LogFilters.FirstOrDefault(filter => filter.FilterType == LogLevel.Fatal);
            if (uiFilter != null)
            {
                uiFilter.IsSelected = true;
                AppliedFilterList.Add(uiFilter);
            }
        }

        private void CreateFilters()
        {
            foreach (var logLevel in Enum.GetValues(typeof (LogLevel)).Cast<LogLevel>())
            {
                LogFilters.Add(new LogFilter
                {
                    FilterType = logLevel,
                    IsEnabled = true
                });
            }
        }

        private void SelectLogFile()
        {
            var result = FileDialogService.ShowOpenFileDialog(new[] {new FileType("Log4j Xml Schema File", ".xml")},
                new FileType("Log4j Xml Schema File", ".xml"), CurrentFileName);
            if (result.IsValid)
            {
                CurrentFileName = result.FileName;
                IsCurrentFileNew = true;
            }
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
            if (IsCurrentFileNew)
            {
                IsCurrentFileNew = false;
            }
            IsLoading = false;
        }

        private IEnumerable<LogItem> GetLogItems()
        {
            var parser = new LogParser();
            var items =
                parser.Parse(Environment.ExpandEnvironmentVariables(CurrentFileName));
            return items;
        }

        private void ModifyFilter(object selectedFilter)
        {
            var filter = selectedFilter as LogFilter;
            if (filter == null)
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
        private bool isEnabled;
        private bool isSelected;

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