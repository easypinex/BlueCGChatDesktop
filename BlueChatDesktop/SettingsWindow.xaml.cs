using System.Windows;
using System.Windows.Media;
using Xceed.Wpf.Toolkit;

namespace BlueChatDesktop
{
    public partial class SettingsWindow : Window
    {
        public Color SelectedBackgroundColor { get; private set; }
        public Color SelectedTextColor { get; private set; }

        public SettingsWindow(Color currentBackgroundColor, Color currentTextColor)
        {
            InitializeComponent();
            BackgroundColorPicker.SelectedColor = currentBackgroundColor;
            TextColorPicker.SelectedColor = currentTextColor;
        }

        private void BackgroundColorPicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (e.NewValue.HasValue)
            {
                SelectedBackgroundColor = e.NewValue.Value;
            }
        }

        private void TextColorPicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (e.NewValue.HasValue)
            {
                SelectedTextColor = e.NewValue.Value;
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}
