using System.Windows;
using MahApps.Metro.SimpleChildWindow;

namespace CalendarSyncPlus.Presentation.Views.Main
{
    /// <summary>
    ///     Interaction logic for ChildView.xaml
    /// </summary>
    public partial class ChildView : ChildWindow
    {
        public ChildView()
        {
            InitializeComponent();
        }

        public object ChildContentView
        {
            get { return GetValue(ChildContentViewProperty); }
            set { SetValue(ChildContentViewProperty, value); }
        }

        public string ChildContentTitle
        {
            get { return (string) GetValue(ChildContentTitleProperty); }
            set { SetValue(ChildContentTitleProperty, value); }
        }

        private static void OnChildViewContentChanged(DependencyObject dependencyObject,
            DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var childView = (ChildView) dependencyObject;
            childView.ChildViewContentHolder.Content = dependencyPropertyChangedEventArgs.NewValue;
        }

        private static void OnChildContentChanged(DependencyObject dependencyObject,
            DependencyPropertyChangedEventArgs eventArgs)
        {
            var childView = (ChildView) dependencyObject;
            childView.Title = (string) eventArgs.NewValue;
        }

        // Using a DependencyProperty as the backing store for ChildContentView.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ChildContentViewProperty =
            DependencyProperty.Register("ChildContentView", typeof (object), typeof (ChildView),
                new PropertyMetadata(null, OnChildViewContentChanged));

        // Using a DependencyProperty as the backing store for ChildContentTitle.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ChildContentTitleProperty =
            DependencyProperty.Register("ChildContentTitle", typeof (string), typeof (ChildView),
                new PropertyMetadata(string.Empty, OnChildContentChanged));
    }
}