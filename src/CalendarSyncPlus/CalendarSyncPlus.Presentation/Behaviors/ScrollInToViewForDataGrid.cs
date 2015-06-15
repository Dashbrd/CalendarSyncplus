using System;
using System.Windows.Controls;
using System.Windows.Interactivity;

namespace CalendarSyncPlus.Presentation.Behaviors
{
    public class ScrollIntoViewForDataGrid : Behavior<DataGrid>
    {
        /// <summary>
        ///  When Beahvior is attached
        /// </summary>
        protected override void OnAttached()
        {
            base.OnAttached();
            this.AssociatedObject.SelectionChanged += AssociatedObject_SelectionChanged;
        }

        /// <summary>
        /// On Selection Changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void AssociatedObject_SelectionChanged(object sender,
                                               SelectionChangedEventArgs e)
        {
            DataGrid dataGrid = sender as DataGrid;
            if (dataGrid == null) return;
            if (dataGrid.SelectedItem != null)
            {
                dataGrid.Dispatcher.BeginInvoke(
                    (Action)(() =>
                    {
                        dataGrid.UpdateLayout();
                        if (dataGrid.SelectedItem !=
                            null)
                            dataGrid.ScrollIntoView(
                                dataGrid.SelectedItem);
                    }));
            }
        }

        /// <summary>
        /// When behavior is detached
        /// </summary>
        protected override void OnDetaching()
        {
            base.OnDetaching();
            this.AssociatedObject.SelectionChanged -=
                AssociatedObject_SelectionChanged;

        }
    }
}