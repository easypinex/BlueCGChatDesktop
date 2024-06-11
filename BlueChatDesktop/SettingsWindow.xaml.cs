using System.Windows;
using System.Windows.Media;

namespace BlueChatDesktop
{
    public partial class SettingsWindow : Window
    {
        public Color SelectedBackgroundColor { get; private set; }
        public Color SelectedTextColor { get; private set; }
        public double SelectedFontSize { get; private set; } // 添加字体大小属性
        public Color? SelectedGPColor { get; private set; }
        public Color? SelectedWorldColor { get; private set; }
        public Color? SelectedFamilyColor { get; private set; }
        public bool AutoHideControlBar { get; private set; }

        public SettingsWindow(Color currentBackgroundColor, Color currentTextColor, double currentFontSize, Color? gpColor, Color? worldColor, Color? familyColor, bool autoHideControlBar)
        {
            InitializeComponent();
            BackgroundColorPicker.SelectedColor = currentBackgroundColor;
            TextColorPicker.SelectedColor = currentTextColor;
            FontSizeSlider.Value = currentFontSize; // 设置初始字体大小
            FontSizeValueText.Text = currentFontSize.ToString(); // 显示初始字体大小

            GPColorPicker.SelectedColor = gpColor ?? Colors.Blue;
            GPColorPicker.IsEnabled = gpColor.HasValue;
            GPColorCheckBox.IsChecked = gpColor.HasValue;

            WorldColorPicker.SelectedColor = worldColor ?? Colors.Yellow;
            WorldColorPicker.IsEnabled = worldColor.HasValue;
            WorldColorCheckBox.IsChecked = worldColor.HasValue;

            FamilyColorPicker.SelectedColor = familyColor ?? Colors.Purple;
            FamilyColorPicker.IsEnabled = familyColor.HasValue;
            FamilyColorCheckBox.IsChecked = familyColor.HasValue;

            AutoHideControlBarCheckBox.IsChecked = autoHideControlBar;
            AutoHideControlBar = autoHideControlBar;
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

        private void GPColorPicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (GPColorCheckBox.IsChecked == true && e.NewValue.HasValue)
            {
                SelectedGPColor = e.NewValue.Value;
            }
        }

        private void GPColorCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            GPColorPicker.IsEnabled = true;
            SelectedGPColor = GPColorPicker.SelectedColor;
        }

        private void GPColorCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            GPColorPicker.IsEnabled = false;
            SelectedGPColor = null;
        }

        private void WorldColorPicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (WorldColorCheckBox.IsChecked == true && e.NewValue.HasValue)
            {
                SelectedWorldColor = e.NewValue.Value;
            }
        }

        private void WorldColorCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            WorldColorPicker.IsEnabled = true;
            SelectedWorldColor = WorldColorPicker.SelectedColor;
        }

        private void WorldColorCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            WorldColorPicker.IsEnabled = false;
            SelectedWorldColor = null;
        }

        private void FamilyColorPicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (FamilyColorCheckBox.IsChecked == true && e.NewValue.HasValue)
            {
                SelectedFamilyColor = e.NewValue.Value;
            }
        }

        private void FamilyColorCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            FamilyColorPicker.IsEnabled = true;
            SelectedFamilyColor = FamilyColorPicker.SelectedColor;
        }

        private void FamilyColorCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            FamilyColorPicker.IsEnabled = false;
            SelectedFamilyColor = null;
        }

        private void AutoHideControlBarCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            AutoHideControlBar = true;
        }

        private void AutoHideControlBarCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            AutoHideControlBar = false;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}
