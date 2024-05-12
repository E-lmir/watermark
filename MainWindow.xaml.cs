using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WoterMark
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool _isDataDirty = true;
        TextBox documentTextBox = new TextBox();
        TextBox documentTextBox2 = new TextBox();
        Window w = new Window();
        Window closeWindow;
        Button closeButton;
        Window opacityWindow;
        Slider slider;
        private static double sliderValue = 0;
        int x = 0;
        private string password = string.Empty;
        private void documentTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            password = documentTextBox.Text;
        }

        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = slider;
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            closeButton = new Button();
            closeButton.Click += CloseButton_Click;

            closeWindow = new Window();
            closeWindow.Width = 200;
            closeWindow.Height = 30;
            closeWindow.VerticalAlignment = VerticalAlignment.Top;
            closeWindow.HorizontalAlignment = HorizontalAlignment.Right;
            closeWindow.AllowsTransparency = true;
            closeWindow.WindowStyle = WindowStyle.None;
            closeWindow.Content = closeButton;
            closeWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;

            slider = new Slider();
            slider.Value = 0.2;
            sliderValue = slider.Value;
            slider.Maximum = 1;
            slider.Minimum = 0;
            slider.ValueChanged += Slider_ValueChanged1;
            opacityBG.Opacity = slider.Value;


            opacityWindow = new Window();
            opacityWindow.Width = 190;
            opacityWindow.Height = 30;
            opacityWindow.VerticalAlignment = VerticalAlignment.Top;
            opacityWindow.HorizontalAlignment = HorizontalAlignment.Right;
            opacityWindow.AllowsTransparency = true;
            opacityWindow.WindowStyle = WindowStyle.None;
            opacityWindow.Content = slider;
            opacityWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;

            w = new Window();
            w.Closing += SubWindow_Closing;
            documentTextBox.TextChanged += documentTextBox_TextChanged;
            w.Height = 100;
            w.Width = 100;
            w.VerticalAlignment = VerticalAlignment.Top;
            w.HorizontalAlignment = HorizontalAlignment.Right;
            w.Content = documentTextBox;
            password = documentTextBox.Text;
            w.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            w.Show();
        }

        private void Slider_ValueChanged1(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (slider.Value < 0.05)
                slider.Value = 0.05;
            opacityBG.Opacity = slider.Value;
        }

        private void SubWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {

            closeButton.Width = 190;
            closeButton.Height = 30;
            closeButton.Content = "Закрыть";
            closeButton.VerticalAlignment = VerticalAlignment.Bottom;
            closeButton.HorizontalAlignment = HorizontalAlignment.Center;
            closeWindow.Show();
            opacityWindow.Show();

        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // If data is dirty, prompt user and ask for a response
            if (_isDataDirty)
            {
                e.Cancel = true;
                w = new Window();
                w.Height = 100;
                w.Width = 100;
                w.Content = documentTextBox2;
                w.Show();
            }
            else
            {
                closeWindow.Close();
                w.Close();             
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            foreach (Window wind in App.Current.Windows)
                wind.Close();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            opacityBG.Opacity = slider.Value;
            if (documentTextBox2.Text == password)
            {
                _isDataDirty = false;
            }

            this.Close();
        }


        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            var hwnd = new WindowInteropHelper(this).Handle;
            WindowsServices.SetWindowExTransparent(hwnd);
        }
    }
    public static class WindowsServices
    {
        const int WS_EX_TRANSPARENT = 0x00000020;
        const int GWL_EXSTYLE = (-20);

        [DllImport("user32.dll")]
        static extern int GetWindowLong(IntPtr hwnd, int index);

        [DllImport("user32.dll")]
        static extern int SetWindowLong(IntPtr hwnd, int index, int newStyle);

        public static void SetWindowExTransparent(IntPtr hwnd)
        {
            var extendedStyle = GetWindowLong(hwnd, GWL_EXSTYLE);
            SetWindowLong(hwnd, GWL_EXSTYLE, extendedStyle | WS_EX_TRANSPARENT);
        }
    }
}
