using System.ComponentModel.Composition;
using CalendarSyncPlus.Application.Views;
using CalendarSyncPlus.Services;

namespace CalendarSyncPlus.Presentation.Services
{
    [Export(typeof(IChildViewService))]
    public class ChildViewService : IChildViewService
    {
        private readonly IShellView _shellView;

        [ImportingConstructor]
        public ChildViewService(IShellView shellView)
        {
            _shellView = shellView;
        }

        #region IChildViewService Members

        public void ShowChildView(object childViewContentView)
        {
            _shellView.ShowChildWindow(childViewContentView);
        }

        #endregion
    }
}