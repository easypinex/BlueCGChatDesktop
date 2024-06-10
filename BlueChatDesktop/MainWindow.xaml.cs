﻿using System;
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
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;
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
        private int _defaultFontSize = 16;
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
                    }

                    if (settings.TextColor != null)
                    {
                        _textBrush.Color = (Color)ColorConverter.ConvertFromString(settings.TextColor);
                    }

                    // 添加字体大小设置
                    if (settings.FontSize > 0)
                    {
                        foreach (var child in ChatPanel.Children)
                        {
                            if (child is TextBlock textBlock)
                            {
                                textBlock.FontSize = settings.FontSize;
                            }
                        }
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
                            line = line.Replace('', '　');
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
                FontSize = _defaultFontSize,
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

                    // 获取当前字体大小
                    double currentFontSize = _defaultFontSize; // 
                    if (ChatPanel.Children.Count > 0 && ChatPanel.Children[0] is TextBlock textBlock)
                    {
                        currentFontSize = textBlock.FontSize;
                    }

                    // 调用保存设置方法，传递字体大小参数
                    SaveSettings(selectedPath, _backgroundBrush.Color.ToString(), _textBrush.Color.ToString(), currentFontSize);
                }
            }
        }

        private void SaveSettings(string folderPath, string backgroundColor, string textColor, double fontSize)
        {
            var settings = new Settings
            {
                BluecgFolder = folderPath,
                BackgroundColor = backgroundColor,
                TextColor = textColor,
                FontSize = fontSize // 保存字体大小
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


        private void ClearChatButton_Click(object sender, RoutedEventArgs e)
        {
            ChatPanel.Children.Clear();
            _filePath = GetLatestFile(_folderPath);
            InitializeFilePosition();
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            var currentFontSize = _defaultFontSize; // 默認字體大小

            var settingsWindow = new SettingsWindow(_backgroundBrush.Color, _textBrush.Color, currentFontSize);

            // 获取屏幕工作区的大小
            var screenWidth = SystemParameters.WorkArea.Width;
            var screenHeight = SystemParameters.WorkArea.Height;

            // 计算设置窗口的位置，使其尽可能靠近主窗口但不重叠
            double left = this.Left + this.Width;
            double top = this.Top;

            // 如果设置窗口在屏幕右边缘之外，则将其放在主窗口左边
            if (left + settingsWindow.Width > screenWidth)
            {
                left = this.Left - settingsWindow.Width;
            }

            // 如果设置窗口在屏幕下边缘之外，则将其放在主窗口上边
            if (top + settingsWindow.Height > screenHeight)
            {
                top = this.Top - settingsWindow.Height;
            }

            // 确保设置窗口在屏幕范围内
            if (left < 0) left = 0;
            if (top < 0) top = 0;

            settingsWindow.Left = left;
            settingsWindow.Top = top;

            if (settingsWindow.ShowDialog() == true)
            {
                _backgroundBrush.Color = settingsWindow.SelectedBackgroundColor;
                MainGrid.Background = _backgroundBrush;
                _textBrush.Color = settingsWindow.SelectedTextColor;

                foreach (var child in ChatPanel.Children)
                {
                    if (child is TextBlock tb)
                    {
                        tb.Foreground = _textBrush;
                        tb.FontSize = settingsWindow.SelectedFontSize; // 更新字体大小
                    }
                }
                _defaultFontSize = (int)settingsWindow.SelectedFontSize;
                // 保存字体大小和其他设置
                SaveSettings(_folderPath, _backgroundBrush.Color.ToString(), _textBrush.Color.ToString(), settingsWindow.SelectedFontSize);
            }
        }



        private class Settings
        {
            public string BluecgFolder { get; set; }
            public string BackgroundColor { get; set; }
            public string TextColor { get; set; }
            public double FontSize { get; set; } // 添加字体大小属性
        }

    }
}
