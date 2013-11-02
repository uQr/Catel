// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PleaseWaitService.cs" company="Catel development team">
//   Copyright (c) 2008 - 2013 Catel development team. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------


namespace Catel.MVVM.Services
{
    using System;
    using System.Diagnostics;
    using System.Threading;
    using System.Windows;
    using System.Windows.Threading;
    using Catel.Logging;
    using Catel.MVVM.Properties;
    using Catel.Windows;
    using Catel.Windows.Threading;

    /// <summary>
    /// Please wait service to show a please wait window during background activities. This service uses the <see cref="PleaseWaitWindow"/>
    /// for the actual displaying of the please wait status to the user.
    /// </summary>
    public class PleaseWaitService : IPleaseWaitService
    {
        /// <summary>
        /// The log.
        /// </summary>
        private static readonly ILog Log = LogManager.GetCurrentClassLogger();

        #region Fields
        private Thread _windowThread;
        private PleaseWaitWindow _pleaseWaitWindow;
        private int _showCounter;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="PleaseWaitService"/> class.
        /// </summary>
        public PleaseWaitService()
        {
            _windowThread = new Thread(() =>
            {
                _pleaseWaitWindow = CreatePleaseWaitWindow();

                // Run dispatcher to keep the separate thread alive
                Dispatcher.Run();

                Log.Warning("PleaseWaitService thread has been ended, this should only happen at application shutdown");
            });

            _windowThread.Name = "PleaseWaitWindowThread";
            _windowThread.SetApartmentState(ApartmentState.STA);
            _windowThread.IsBackground = true;

            _windowThread.Start();
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets the dispatcher of the please wait window.
        /// </summary>
        /// <value>The dispatcher.</value>
        protected Dispatcher Dispatcher { get { return _pleaseWaitWindow.Dispatcher; } }
        #endregion

        #region IPleaseWaitService Members
        /// <summary>
        /// Shows the please wait window with the specified status text.
        /// </summary>
        /// <param name="status">The status. When the string is <c>null</c> or empty, the default please wait text will be used.</param>
        /// <remarks>When this method is used, the <see cref="Hide" /> method must be called to hide the window again.</remarks>
        public virtual void Show(string status = "")
        {
            UpdateStatus(status);

            Show(-1);
        }

        /// <summary>
        /// Shows the please wait window with the specified status text and executes the work delegate (in a background thread). When the work is finished,
        /// the please wait window will be automatically closed.
        /// </summary>
        /// <param name="workDelegate">The work delegate.</param>
        /// <param name="status">The status.</param>
        public virtual void Show(PleaseWaitWorkDelegate workDelegate, string status = "")
        {
            UpdateStatus(status);

            Show(-1);

            if (workDelegate != null)
            {
                workDelegate();
            }

            Hide();
        }

        /// <summary>
        /// Updates the status text.
        /// </summary>
        /// <param name="status">The status.</param>
        public virtual void UpdateStatus(string status)
        {
            Dispatcher.BeginInvoke(() => _pleaseWaitWindow.Text = GetStatus(status));
        }

        /// <summary>
        /// Updates the status and shows a progress bar with the specified status text. The percentage will be automatically calculated.
        /// <para>
        /// </para>
        /// The busy indicator will automatically hide when the <paramref name="totalItems" /> is larger than <paramref name="currentItem" />.
        /// <para>
        /// </para>
        /// When providing the <paramref name="statusFormat" />, it is possible to use <c>{0}</c> (represents current item) and
        /// <c>{1}</c> (represents total items).
        /// </summary>
        /// <param name="currentItem">The current item.</param>
        /// <param name="totalItems">The total items.</param>
        /// <param name="statusFormat">The status format. Can be empty, but not <c>null</c>.</param>
        /// <exception cref="ArgumentOutOfRangeException">The <paramref name="currentItem" /> is smaller than zero.</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="statusFormat" /> is <c>null</c>.</exception>
        public virtual void UpdateStatus(int currentItem, int totalItems, string statusFormat = "")
        {
            if (currentItem > totalItems)
            {
                Hide();
                return;
            }

            UpdateStatus(string.Format(statusFormat, currentItem, totalItems));

            int percentage = (100 / totalItems) * currentItem;
            Show(percentage);
        }

        /// <summary>
        /// Hides this please wait window.
        /// </summary>
        public virtual void Hide()
        {
            Dispatcher.BeginInvoke(() =>
            {
                //_currentStatusText = GetStatus(string.Empty);
                //_currentWindowWidth = 0d;

                _pleaseWaitWindow.Visibility = Visibility.Hidden;
                //_pleaseWaitWindow.Hide();
                _pleaseWaitWindow.Text = GetStatus(string.Empty);

                _showCounter = 0;
            });
        }

        /// <summary>
        /// Increases the number of clients that show the please wait window. The implementing class is responsible for holding a counter
        /// internally which a call to this method will increase.
        /// <para>
        /// </para>
        /// As long as the internal counter is not zero (0), the please wait window will stay visible. To decrease the counter, make a
        /// call to <see cref="Pop" />.
        /// <para>
        /// </para>
        /// A call to <see cref="Show(string)" /> or one of its overloads will not increase the internal counter. A call to <see cref="Hide" />
        /// will reset the internal counter to zero (0) and thus hide the window.
        /// </summary>
        /// <param name="status">The status.</param>
        public virtual void Push(string status = "")
        {
            UpdateStatus(status);

            _showCounter++;

            if (_showCounter > 0)
            {
                Show(status);
            }
        }

        /// <summary>
        /// Decreases the number of clients that show the please wait window. The implementing class is responsible for holding a counter internally which a call to this method will decrease.
        /// <para>
        /// </para>
        /// As long as the internal counter is not zero (0), the please wait window will stay visible. To increase the counter, make a call to <see cref="Pop" />.
        /// <para>
        /// </para>
        /// A call to <see cref="Show(string)" /> or one of its overloads will not increase the internal counter. A call to <see cref="Hide" /> will reset the internal counter to zero (0) and thus hide the window.
        /// </summary>
        public virtual void Pop()
        {
            if (_showCounter > 0)
            {
                _showCounter--;
            }

            if (_showCounter <= 0)
            {
                Hide();
            }
        }
        #endregion

        /// <summary>
        /// Shows the window specified percentage.
        /// </summary>
        /// <param name="percentage">The percentage. If <c>-1</c>, the window is indeterminate.</param>
        protected virtual void Show(int percentage)
        {
            bool isIndeterminate = (percentage == -1);

            Dispatcher.BeginInvoke(() =>
            {
                _pleaseWaitWindow.IsIndeterminate = isIndeterminate;
                _pleaseWaitWindow.Percentage = percentage;

                //_pleaseWaitWindow.Text = _currentStatusText;
                //_pleaseWaitWindow.MinWidth = double.IsNaN(_currentWindowWidth) ? 0d : _currentWindowWidth;

                _pleaseWaitWindow.Visibility = Visibility.Visible;

                // Yes, check for PleaseWaitWindow (property). Sometimes the show immediately hides
                //while (!_pleaseWaitWindow.IsOwnerDimmed)
                //{
                //    // It's a bad practice to use this "equivalent" of DoEvents in WPF, but I don't see another choice
                //    // to wait until the animation of the ShowWindow has finished without blocking the UI
                //    _pleaseWaitWindow.Dispatcher.Invoke(DispatcherPriority.Background, (ThreadStart)delegate { });
                //    _pleaseWaitWindow.UpdateLayout();
                //}

                //lock (_visibleStopwatchLock)
                //{
                //    if (_visibleStopwatch == null)
                //    {
                //        _visibleStopwatch = new Stopwatch();
                //        _visibleStopwatch.Start();
                //    }
                //    else
                //    {
                //        _visibleStopwatch.Reset();
                //        _visibleStopwatch.Start();
                //    }
                //}
            });
        }

        /// <summary>
        /// Gets the status.
        /// </summary>
        /// <param name="status">The status.</param>
        /// <returns>System.String.</returns>
        protected virtual string GetStatus(string status)
        {
            if (string.IsNullOrEmpty(status))
            {
                status = Resources.PleaseWait;
            }

            return status;
        }

        /// <summary>
        /// Creates the please wait window.
        /// </summary>
        /// <returns>PleaseWaitWindow.</returns>
        protected virtual PleaseWaitWindow CreatePleaseWaitWindow()
        {
            return new PleaseWaitWindow();
        }
    }
}