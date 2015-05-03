using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using CalendarSyncPlus.Application.Views;
using CalendarSyncPlus.Services;
using MahApps.Metro.SimpleChildWindow;

namespace CalendarSyncPlus.Presentation.Views
{
    /// <summary>
    /// Interaction logic for ChildView.xaml
    /// </summary>
    public partial class ChildView : ChildWindow
    {
        public ChildView()
        {
            InitializeComponent();
        }
        public object ChildContentView
        {
            get { return (object)GetValue(ChildContentViewProperty); }
            set { SetValue(ChildContentViewProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ChildContentView.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ChildContentViewProperty =
            DependencyProperty.Register("ChildContentView", typeof (object), typeof (ChildView),
                new PropertyMetadata(null,OnChildViewContentChanged));

        private static void OnChildViewContentChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var childView = (ChildView) dependencyObject;
            childView.ChildViewContentHolder.Content = dependencyPropertyChangedEventArgs.NewValue;
        }



        public string ChildContentTitle
        {
            get { return (string)GetValue(ChildContentTitleProperty); }
            set { SetValue(ChildContentTitleProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ChildContentTitle.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ChildContentTitleProperty =
            DependencyProperty.Register("ChildContentTitle", typeof(string), typeof(ChildView), new PropertyMetadata(string.Empty,OnChildContentChanged));

        private static void OnChildContentChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs eventArgs)
        {
             var childView = (ChildView) dependencyObject;
            childView.Title = (string) eventArgs.NewValue;
        }
    }
}