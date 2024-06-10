using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Newtonsoft.Json;
using Xceed.Wpf.Toolkit;
using Application = System.Windows.Application;
using Button = System.Windows.Controls.Button;
using Color = System.Windows.Media.Color;
using HorizontalAlignment = System.Windows.HorizontalAlignment;
using TextBox = System.Windows.Controls.TextBox;

namespace BlueChatDesktop
{
    public partial class MainWindow : Window
    {
        private DispatcherTimer _timer;
        private string _filePath;
        private string _folderPath;
        private DateTime _lastReadTime = DateTime.MinValue;
        private long _lastReadPosition = 0;
        private string _lastLineRead = string.Empty;
        private const string SettingFileName = "setting.json";
        private bool _autoScroll = true;
        private SolidColorBrush _backgroundBrush;
        private SolidColorBrush _textBrush;

        public MainWindow()
        {
            InitializeComponent();
            _backgroundBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#AA87CEFA"));
            _textBrush = new SolidColorBrush(Colors.White);
            MainGrid.Background = _backgroundBrush;
            LoadSettings();
            SetupTimer();
        }

        private void LoadSettings()
        {
            if (File.Exists(SettingFileName))
            {
                var json = File.ReadAllText(SettingFileName);
                var settings = JsonConvert.DeserializeObject<Settings>(json);
                if (settings != null)
                {
                    if (!string.IsNullOrEmpty(settings.BluecgFolder))
                    {
                        _filePath = GetLatestFile(settings.BluecgFolder);
                        InitializeFilePosition();
                    }

                    if (settings.BackgroundColor != null)
                    {
                        _backgroundBrush.Color = (Color)ColorConverter.ConvertFromString(settings.BackgroundColor);
                        MainGrid.Background = _backgroundBrush;
                        BackgroundColorPicker.SelectedColor = _backgroundBrush.Color;
                    }

                    if (settings.TextColor != null)
                    {
                        _textBrush.Color = (Color)ColorConverter.ConvertFromString(settings.TextColor);
                        TextColorPicker.SelectedColor = _textBrush.Color;
                    }
                }
            }
        }

        private void SetupTimer()
        {
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += Timer_Tick;
            _timer.Start();
        }

        private void InitializeFilePosition()
        {
            if (!string.IsNullOrEmpty(_filePath) && File.Exists(_filePath))
            {
                _lastReadPosition = new FileInfo(_filePath).Length;
            }
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            ReadAndDisplayChatLog();
        }

        private void ReadAndDisplayChatLog()
        {
            if (string.IsNullOrEmpty(_filePath) || !File.Exists(_filePath))
                return;

            try
            {
                using (var fileStream = new FileStream(_filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    fileStream.Seek(_lastReadPosition, SeekOrigin.Begin);
                    using (var streamReader = new StreamReader(fileStream, Encoding.GetEncoding("Big5")))
                    {
                        string line;
                        while ((line = streamReader.ReadLine()) != null)
                        {
                            if (line != _lastLineRead)
                            {
                                AddChatMessage(line);
                                _lastLineRead = line;
                            }
                        }

                        _lastReadPosition = fileStream.Position;
                    }
                }
            }
            catch (IOException ex)
            {
                // 处理文件访问异常
                Console.WriteLine($"Error reading file: {ex.Message}");
            }
        }

        private void AddChatMessage(string message)
        {
            var textBlock = new TextBlock
            {
                Text = message,
                Foreground = _textBrush,
                Margin = new Thickness(5),
                FontSize = 16,
                FontWeight = FontWeights.Bold,
                TextWrapping = TextWrapping.Wrap
            };

            ChatPanel.Children.Add(textBlock);

            if (_autoScroll)
            {
                ScrollToBottom();
            }
        }

        private void ScrollToBottom()
        {
            ChatScrollViewer.ScrollToEnd();
        }

        private void ChatScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (e.ExtentHeightChange == 0)
            {
                _autoScroll = ChatScrollViewer.VerticalOffset == ChatScrollViewer.ScrollableHeight;
            }

            if (_autoScroll && e.ExtentHeightChange != 0)
            {
                ChatScrollViewer.ScrollToVerticalOffset(ChatScrollViewer.ExtentHeight);
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var desktopWorkingArea = SystemParameters.WorkArea;
            this.Left = desktopWorkingArea.Right - this.Width;
            this.Top = desktopWorkingArea.Bottom - this.Height;

            ChatScrollViewer.ScrollChanged += ChatScrollViewer_ScrollChanged;
        }

        private void SelectFolderButton_Click(object sender, RoutedEventArgs e)
        {
            using (var dialog = new FolderBrowserDialog())
            {
                dialog.Description = "選擇你魔力寶貝的「Log」資料夾";
                dialog.ShowNewFolderButton = false;

                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    var selectedPath = dialog.SelectedPath;
                    _filePath = GetLatestFile(selectedPath);
                    InitializeFilePosition();
                    SaveSettings(selectedPath, _backgroundBrush.Color.ToString(), _textBrush.Color.ToString());
                }
            }
        }

        private void SaveSettings(string folderPath, string backgroundColor, string textColor)
        {
            var settings = new Settings
            {
                BluecgFolder = folderPath,
                BackgroundColor = backgroundColor,
                TextColor = textColor
            };
            var json = JsonConvert.SerializeObject(settings, Formatting.Indented);
            File.WriteAllText(SettingFileName, json);
        }

        private string GetLatestFile(string folderPath)
        {
            var directoryInfo = new DirectoryInfo(folderPath);
            var latestFile = directoryInfo.GetFiles()
                                          .OrderByDescending(f => f.LastWriteTime)
                                          .FirstOrDefault();
            _folderPath = folderPath;

            return latestFile?.FullName;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void DragWindow(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }

        private void BackgroundColorPicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (e.NewValue.HasValue)
            {
                var selectedColor = e.NewValue.Value;
                _backgroundBrush.Color = selectedColor;
                MainGrid.Background = _backgroundBrush;
                SaveSettings(_folderPath, _backgroundBrush.Color.ToString(), _textBrush.Color.ToString());
            }
        }

        private void TextColorPicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (e.NewValue.HasValue)
            {
                var selectedColor = e.NewValue.Value;
                _textBrush.Color = selectedColor;
                foreach (var child in ChatPanel.Children)
                {
                    if (child is TextBlock textBlock)
                    {
                        textBlock.Foreground = _textBrush;
                    }
                }
                SaveSettings(_folderPath, _backgroundBrush.Color.ToString(), _textBrush.Color.ToString());
            }
        }

        private void ClearChatButton_Click(object sender, RoutedEventArgs e)
        {
            ChatPanel.Children.Clear();
        }


        private class Settings
        {
            public string BluecgFolder { get; set; }
            public string BackgroundColor { get; set; }
            public string TextColor { get; set; }
        }
    }
}
