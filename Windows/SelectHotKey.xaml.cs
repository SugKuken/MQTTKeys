using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using Application = System.Windows.Application;
using CheckBox = System.Windows.Controls.CheckBox;
using MessageBox = System.Windows.MessageBox;

namespace mqtt_hotkeys_test.Windows
{
    /// <summary>
    ///     Interaction logic for Window1.xaml
    /// </summary>
    public partial class SelectHotKey : Window
    {
        private readonly List<ModifierKeys> _outKeys = new List<ModifierKeys>();
        private bool IsSub = false;

        public SelectHotKey(bool isSub)
        {
            IsSub = isSub;
            InitializeComponent();
            CmbBoxKeyList.ItemsSource = (Key[])Enum.GetValues(typeof(Key));
        }

        public Key HotKey { get; set; }
        public ModifierKeys ModKeys { get; set; }

        //private void TxtKey_OnGotFocus(object sender, RoutedEventArgs e)
        //{
        //    TxtKey.Clear();
        //}

        //private void TxtKey_OnLostFocus(object sender, RoutedEventArgs e)
        //{
        //    if (TxtKey.Text == "")
        //        TxtKey.Text = "Key...";
        //}

        private void SelectHotKey_OnLoaded(object sender, RoutedEventArgs e)
        {
            var curApp = Application.Current;
            var mainWindow = curApp.MainWindow;
            Left = mainWindow.Left + (mainWindow.Width - ActualWidth) / 2;
            Top = mainWindow.Top + (mainWindow.Height - ActualHeight) / 2;
        }

        private void BtnOk_OnClick(object sender, RoutedEventArgs e)
        {
            // TODO: this will probably be replaced if keyboard interface is implemented
            {
                if (IsSub)
                {
                    // TODO: If keyboard interface is implemented, enable space
                }
                var parsedMKeys = string.Join(",", _outKeys);
                if (Enum.TryParse(parsedMKeys, true, out ModifierKeys tempKeys))
                    ModKeys = tempKeys;
                if (ModKeys == ModifierKeys.None)
                {
                    MessageBox.Show("No modifier keys selected.");
                    return;
                }
                HotKey = (Key)CmbBoxKeyList.SelectedItem;
                DialogResult = true;
                Close();
            }
        }

        private void BtnCancel_OnClick(object sender, MouseButtonEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void CheckBox_OnChecked(object sender, RoutedEventArgs e)
        {
            var s = (CheckBox) sender;
            var name = s.Content.ToString();
            if (name == "Win")
                name = "Windows";
            if (Enum.TryParse(name, true, out ModifierKeys mKey))
                _outKeys.Add(mKey);
        }

        private void CheckBox_OnUnChecked(object sender, RoutedEventArgs e)
        {
            var s = (CheckBox) sender;
            var name = s.Content.ToString();
            if (name == "Win")
                name = "Windows";
            if (Enum.TryParse(name, true, out ModifierKeys mKey))
                _outKeys.Remove(mKey);
        }
    }
}