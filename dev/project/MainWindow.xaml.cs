using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Windowing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Diagnostics;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics;
using System.Runtime.InteropServices; 
using Microsoft.Web.WebView2.Core;
using Windows.UI.ViewManagement;

namespace AppGenerated
{
    public sealed partial class MainWindow : Window
    {
        private OverlappedPresenter _presenter;
        private DispatcherTimer _splashTimer;

        public MainWindow()
        {
            InitializeComponent();
            Window window = this;
            window.ExtendsContentIntoTitleBar = true;
            window.SetTitleBar(AppTitleBar);
            window.AppWindow.TitleBar.PreferredHeightOption = TitleBarHeightOption.Collapsed;
            
            _presenter = window.AppWindow.Presenter as OverlappedPresenter;
            window.AppWindow.Changed += AppWindow_Changed;

            MyWebView2.NavigationStarting += MyWebView2_NavigationStarting;
            MyWebView2.NavigationCompleted += MyWebView2_NavigationCompleted;

            // Настраиваем окно для заставки (блокируем изменение размера)
            SetupSplashScreenWindow();
            SetTaskbarIcon();
            
            // Запускаем таймер заставки
            StartSplashScreen();
        }

        private void SetupSplashScreenWindow()
        {
            // Блокируем изменение размеров и кнопку "развернуть" во время заставки
            if (_presenter != null)
            {
                _presenter.IsResizable = false;
                _presenter.IsMaximizable = false;
            }

            // Устанавливаем размер 700x500 и центрируем окно на экране
            var appWindow = this.AppWindow;
            int width = 700;
            int height = 500;
            
            var displayArea = DisplayArea.GetFromWindowId(appWindow.Id, DisplayAreaFallback.Nearest);
            if (displayArea != null)
            {
                var centeredPosition = new PointInt32(
                    ((displayArea.WorkArea.Width - width) / 2),
                    ((displayArea.WorkArea.Height - height) / 2)
                );
                appWindow.MoveAndResize(new RectInt32(centeredPosition.X, centeredPosition.Y, width, height));
            }
            else
            {
                appWindow.Resize(new SizeInt32(width, height));
            }
        }

        private void StartSplashScreen()
        {
            _splashTimer = new DispatcherTimer();
            _splashTimer.Interval = TimeSpan.FromSeconds(5);
            _splashTimer.Tick += SplashTimer_Tick;
            _splashTimer.Start();
        }

        private void SplashTimer_Tick(object sender, object e)
        {
            _splashTimer.Stop();
            
            // 1. Скрываем заставку и показываем основное приложение
            SplashUI.Visibility = Visibility.Collapsed;
            MainAppUI.Visibility = Visibility.Visible;

            // 2. Откатываем ограничения окна (разрешаем менять размер)
            if (_presenter != null)
            {
                _presenter.IsResizable = true;
                _presenter.IsMaximizable = true;
                
                // 3. Открываем окно на весь экран (режим окна во весь экран, с панелью задач)
                _presenter.Maximize();
            }

            // 4. Освобождаем ресурсы WebView2 заставки
            SplashWebView.Close();
        }

        private async void MyWebView2_NavigationCompleted(WebView2 sender, CoreWebView2NavigationCompletedEventArgs args)
        {
            if (args.IsSuccess)
            {
                var uiSettings = new UISettings();
                var color = uiSettings.GetColorValue(UIColorType.Accent);
                string hexAccent = $"#{color.R:X2}{color.G:X2}{color.B:X2}";
                await MyWebView2.ExecuteScriptAsync($"if (typeof accentcolor === 'function') {{ accentcolor('{hexAccent}'); }}");
            }
        }

        private void MyWebView2_NavigationStarting(WebView2 sender, CoreWebView2NavigationStartingEventArgs args)
        {
            if (args.Uri == "http://127.0.0.1:6024/closeide.html")
            {
                Application.Current.Exit();
            }
        }

        private void AppWindow_Changed(AppWindow sender, AppWindowChangedEventArgs args)
        {
            if (args.DidPresenterChange && _presenter != null)
            {
                if (_presenter.State == OverlappedPresenterState.Maximized)
                {
                    MaximizeBtn.Content = "\uE923"; 
                }
                else
                {
                    MaximizeBtn.Content = "\uE922"; 
                }
            }
        }

        private void MinimizeBtn_Click(object sender, RoutedEventArgs e)
        {
            _presenter?.Minimize();
        }

        private void MaximizeBtn_Click(object sender, RoutedEventArgs e)
        {
            if (_presenter == null) return;

            if (_presenter.State == OverlappedPresenterState.Maximized)
            {
                _presenter.Restore();
            }
            else
            {
                _presenter.Maximize();
            }
        }

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr LoadImage(IntPtr hInst, string lpszName, uint uType, int cxDesired, int cyDesired, uint fuLoad);

        private const uint WM_SETICON = 0x0080;
        private const uint IMAGE_ICON = 1;
        private const uint LR_LOADFROMFILE = 0x0010;

        private void SetTaskbarIcon()
        {
            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
            string iconPath = System.IO.Path.Combine(AppContext.BaseDirectory, "Assets", "AppIcon.ico");
            IntPtr hIcon = LoadImage(IntPtr.Zero, iconPath, IMAGE_ICON, 0, 0, LR_LOADFROMFILE);
            SendMessage(hwnd, WM_SETICON, (IntPtr)1, hIcon);
            SendMessage(hwnd, WM_SETICON, (IntPtr)0, hIcon);
        }
    }
}