using System.Windows;
using System.Windows.Controls;
using System.Windows.Interactivity;

namespace CalendarSyncPlus.Presentation.Behaviors
{
    public class TextBoxScrollBehavior : Behavior<TextBox>
    {
        private bool _isCleanup;

        protected override void OnAttached()
        {
            AssociatedObject.Loaded += AssociatedObjectOnLoaded;
            AssociatedObject.Unloaded += AssociatedObjectOnUnloaded;
            base.OnAttached();
        }

        protected override void OnDetaching()
        {
            CleanUp();
            base.OnDetaching();
        }

        private void AssociatedObjectOnUnloaded(object sender, RoutedEventArgs routedEventArgs)
        {
            CleanUp();
        }

        private void AssociatedObjectOnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            AssociatedObject.TextChanged += AssociatedObjectOnTextChanged;
        }

        private void AssociatedObjectOnTextChanged(object sender, TextChangedEventArgs textChangedEventArgs)
        {
            AssociatedObject.ScrollToEnd();
        }

        private void CleanUp()
        {
            if (_isCleanup)
            {
                return;
            }
            AssociatedObject.Loaded -= AssociatedObjectOnLoaded;
            AssociatedObject.Unloaded -= AssociatedObjectOnUnloaded;
            AssociatedObject.TextChanged -= AssociatedObjectOnTextChanged;
            _isCleanup = true;
        }
    }
}