using Microsoft.Xaml.Behaviors;
using System;
using System.Windows.Controls;

namespace CalendarSyncPlus.Presentation.Behaviors
{
    public class ScrollIntoViewForDataGrid : Behavior<DataGrid>
    {
        /// <summary>
        ///     When Beahvior is attached
        /// </summary>
        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.SelectionChanged += AssociatedObject_SelectionChanged;
        }

        /// <summary>
        ///     On Selection Changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AssociatedObject_SelectionChanged(object sender,
            SelectionChangedEventArgs e)
        {
            var dataGrid = sender as DataGrid;
            if (dataGrid == null) return;
            if (dataGrid.SelectedItem != null)
            {
                dataGrid.Dispatcher.BeginInvoke(
                    (Action) (() =>
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
        ///     When behavior is detached
        /// </summary>
        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.SelectionChanged -=
                AssociatedObject_SelectionChanged;
        }
    }
}