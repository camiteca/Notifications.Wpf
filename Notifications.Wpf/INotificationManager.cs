using System;
using System.Windows;

namespace Notifications.Wpf
{
    public interface INotificationManager
    {
        void Show(object content, string areaName = "", TimeSpan? expirationTime = null, Action onClick = null, Action onClose = null, Window parent = null);
    }
}