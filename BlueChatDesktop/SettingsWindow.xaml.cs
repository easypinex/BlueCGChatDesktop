using System.Windows;
using System.Windows.Media;

namespace BlueChatDesktop
{
    public partial class SettingsWindow : Window
    {
        public Color SelectedBackgroundColor { get; private set; }
        public Color SelectedTextColor { get; private set; }
        public double SelectedFontSize { get; private set; } // 添加字体大小属性

        public SettingsWindow(Color currentBackgroundColor, Color currentTextColor, double currentFontSize)
        {
            InitializeComponent();
            BackgroundColorPicker.SelectedColor = currentBackgroundColor;
            TextColorPicker.SelectedColor = currentTextColor;
            FontSizeSlider.Value = currentFontSize; // 设置初始字体大小
            FontSizeValueText.Text = currentFontSize.ToString(); // 显示初始字体大小
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

        private void FontSizeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (e != null)
            {
                SelectedFontSize = e.NewValue;
                if (FontSizeValueText != null)
                    FontSizeValueText.Text = ((int)e.NewValue).ToString(); // 更新选择的字体大小并显示
            }

        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}
