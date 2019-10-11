using System;
using System.Collections;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Notifications.Wpf.Controls
{
    public class NotificationArea : Control
    {
        public NotificationPosition Position
        {
            get { return (NotificationPosition)GetValue(PositionProperty); }
            set { SetValue(PositionProperty, value); }
        }

        public static readonly DependencyProperty PositionProperty =
            DependencyProperty.Register("Position", typeof(NotificationPosition), typeof(NotificationArea), new PropertyMetadata(NotificationPosition.BottomRight));

        public int MaxItems
        {
            get { return (int)GetValue(MaxItemsProperty); }
            set { SetValue(MaxItemsProperty, value); }
        }
        
        public static readonly DependencyProperty MaxItemsProperty =
            DependencyProperty.Register("MaxItems", typeof(int), typeof(NotificationArea), new PropertyMetadata(int.MaxValue));

        private IList _items;

        public NotificationArea()
        {
            NotificationManager.AddArea(this);
        }

        static NotificationArea()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(NotificationArea),
                new FrameworkPropertyMetadata(typeof(NotificationArea)));
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            var itemsControl = GetTemplateChild("PART_Items") as Panel;

            _items = itemsControl?.Children;
        }

        public async void Show(FrameworkElement content, TimeSpan expirationTime, Action onClick, Action onClose)
        {
            var notification = new Notification
            {
                Content = content
            };

            notification.MouseLeftButtonDown += (sender, args) =>
            {
                if (onClick != null)
                {
                    onClick.Invoke();

                    (sender as Notification)?.Close();
                }
            };

            notification.NotificationClosed += (sender, args) =>
            {
                onClose?.Invoke();
            };

            notification.NotificationClosed += OnNotificationClosed;

            if (!IsLoaded)
            {
                return;
            }

            var w = Window.GetWindow(this);
            var x = PresentationSource.FromVisual(w);
            if (x == null)
            {
                return;
            }

            lock (_items)
            {
                _items.Add(notification);

                if (_items.OfType<Notification>().Count(i => !i.IsClosing) > MaxItems)
                {
                    _items.OfType<Notification>().First(i => !i.IsClosing).Close();
                }
            }

            if (expirationTime == TimeSpan.MaxValue)
            {
                return;
            }

            await Task.Delay(expirationTime);

            notification.Close();
        }

        private void OnNotificationClosed(object sender, RoutedEventArgs routedEventArgs)
        {
            var notification = sender as Notification;

            var window = Window.GetWindow(this);

            window.Height = window.Height + notification.Height;
            window.Top = SystemParameters.FullPrimaryScreenHeight - notification.Height;

            _items.Remove(notification);

            if(_items.Count == 0)
            {
                NotificationManager.CloseNotificationArea();
            }
        }
    }

    public enum NotificationPosition
    {
        TopLeft,
        TopRight,
        BottomLeft,
        BottomRight
    }
}