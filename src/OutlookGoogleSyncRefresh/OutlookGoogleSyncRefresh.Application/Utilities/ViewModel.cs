#region File Header

// /******************************************************************************
//  * 
//  *      Copyright (C) Ankesh Dave 2015 All Rights Reserved. Confidential
//  * 
//  ******************************************************************************
//  * 
//  *      Project:        OutlookGoogleSyncRefresh
//  *      SubProject:     OutlookGoogleSyncRefresh.Application
//  *      Author:         Dave, Ankesh
//  *      Created On:     06-02-2015 1:18 PM
//  *      Modified On:    06-02-2015 1:18 PM
//  *      FileName:       ViewModel.cs
//  * 
//  *****************************************************************************/

#endregion

using System;
using System.Threading;
using System.Waf.Applications;
using System.Windows.Threading;

namespace OutlookGoogleSyncRefresh.Application.Utilities
{
    /// <summary>
    ///     Abstract base class for a ViewModel implementation.
    /// </summary>
    public abstract class ViewModel : DataModel
    {
        private readonly IView view;


        /// <summary>
        ///     Initializes a new instance of the <see cref="ViewModel" /> class and
        ///     attaches itself as <c>DataContext</c> to the view.
        /// </summary>
        /// <param name="view">The view.</param>
        protected ViewModel(IView view)
        {
            if (view == null)
            {
                throw new ArgumentNullException("view");
            }
            this.view = view;

            // Check if the code is running within the WPF application model
            if (SynchronizationContext.Current is DispatcherSynchronizationContext)
            {
                // Set DataContext of the view has to be delayed so that the ViewModel can initialize the internal data (e.g. Commands)
                // before the view starts with DataBinding.
                Dispatcher.CurrentDispatcher.BeginInvoke((Action) delegate { this.view.DataContext = this; });
            }
            else
            {
                // When the code runs outside of the WPF application model then we set the DataContext immediately.
                view.DataContext = this;
            }
        }

        /// <summary>
        ///     Obsolete: Initializes a new instance of the <see cref="ViewModel" /> class.
        /// </summary>
        /// <param name="view">The view.</param>
        /// <param name="isChild">if set to <c>true</c> then this object is a child of another ViewModel.</param>
        [Obsolete("Please use the DataModel base class for child view models instead of using this constructor.")]
        protected ViewModel(IView view, bool isChild)
        {
            if (view == null)
            {
                throw new ArgumentNullException("view");
            }
            this.view = view;
            if (!isChild)
            {
                // Check if the code is running within the WPF application model
                if (SynchronizationContext.Current is DispatcherSynchronizationContext)
                {
                    // Set DataContext of the view has to be delayed so that the ViewModel can initialize the internal data (e.g. Commands)
                    // before the view starts with DataBinding.
                    Dispatcher.CurrentDispatcher.BeginInvoke((Action) delegate { this.view.DataContext = this; });
                }
                else
                {
                    // When the code runs outside of the WPF application model then we set the DataContext immediately.
                    view.DataContext = this;
                }
            }
        }


        /// <summary>
        ///     Gets the associated view.
        /// </summary>
        public object View
        {
            get { return view; }
        }
    }

    /// <summary>
    ///     Abstract base class for a ViewModel implementation.
    /// </summary>
    /// <typeparam name="TView">The type of the view. Do provide an interface as type and not the concrete type itself.</typeparam>
    public abstract class ViewModel<TView> : ViewModel where TView : IView
    {
        private readonly TView view;


        /// <summary>
        ///     Initializes a new instance of the <see cref="ViewModel&lt;TView&gt;" /> class and
        ///     attaches itself as <c>DataContext</c> to the view.
        /// </summary>
        /// <param name="view">The view.</param>
        protected ViewModel(TView view)
            : base(view)
        {
            this.view = view;
        }

        /// <summary>
        ///     Obsolete: Initializes a new instance of the <see cref="ViewModel&lt;TView&gt;" /> class.
        /// </summary>
        /// <param name="view">The view.</param>
        /// <param name="isChild">if set to <c>true</c> then the ViewModel is a child of another ViewModel.</param>
        [Obsolete("Please use the DataModel base class for child view models instead of using this constructor.")]
        protected ViewModel(TView view, bool isChild)
            : base(view, isChild)
        {
            this.view = view;
        }


        /// <summary>
        ///     Gets the associated view as specified view type.
        /// </summary>
        /// <remarks>
        ///     Use this property in a ViewModel class to avoid casting.
        /// </remarks>
        protected TView ViewCore
        {
            get { return view; }
        }
    }
}