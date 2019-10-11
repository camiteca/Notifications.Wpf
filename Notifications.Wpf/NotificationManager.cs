using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Threading;
using Notifications.Wpf.Controls;

namespace Notifications.Wpf
{
    public class NotificationManager 
        : INotificationManager
    {
        private readonly Dispatcher _dispatcher;
        private static readonly List<NotificationArea> Areas = new List<NotificationArea>();
        private static NotificationsOverlayWindow _window;

        public NotificationManager(Dispatcher dispatcher = null)
        {
            if (dispatcher == null)
            {
                dispatcher = Application.Current?.Dispatcher ?? Dispatcher.CurrentDispatcher;
            }

            _dispatcher = dispatcher;
        }

        public void Show(FrameworkElement content, string areaName = "", TimeSpan? expirationTime = null, Action onClick = null, Action onClose = null, Window parent = null, double width = 350, double height = 120)
        {
            if(content == null)
            {
                throw new ArgumentNullException(nameof(content));
            }

            if (!_dispatcher.CheckAccess())
            {
                _dispatcher.BeginInvoke(new Action(() => Show(content, areaName, expirationTime, onClick, onClose)));
                return;
            }

            if (expirationTime == null)
            {
                expirationTime = TimeSpan.FromSeconds(5);
            }

            if (areaName == string.Empty && _window == null)
            {
                _window = new NotificationsOverlayWindow
                {
                    Owner = parent ?? Application.Current.MainWindow,
                    Width = width,
                    Height = height,
                };

                _window.Left = SystemParameters.FullPrimaryScreenWidth - _window.Width;
                _window.Top = SystemParameters.FullPrimaryScreenHeight - _window.Height;

                _window.Show();
            }
            else
            {
                _window.Height = _window.Height + height;
                _window.Top = SystemParameters.FullPrimaryScreenHeight - _window.Height;
            }

            foreach (var area in Areas.Where(a => a.Name == areaName))
            {
                area.Show(content, (TimeSpan) expirationTime, onClick, onClose);
            }
        }

        internal static void AddArea(NotificationArea area)
        {
            Areas.Add(area);
        }

        internal static void CloseNotificationArea()
        {
            Areas.Clear();

            _window.Close();

            _window = null;
        }
    }

    public class NotificationSize
    {
        public NotificationSize(double width, double height)
        {
            Width = width;
            Height = height;
        }

        public double Width { get; set; }

        public double Height { get; set; }
    }
}