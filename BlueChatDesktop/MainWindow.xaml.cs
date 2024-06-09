using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using Newtonsoft.Json;
using System.Windows.Shell;
using System.Windows.Forms;

namespace BlueChatDesktop
{
    public partial class MainWindow : Window
    {
        private DispatcherTimer _timer;
        private string _filePath;
        private DateTime _lastReadTime = DateTime.MinValue;
        private long _lastReadPosition = 0;
        private string _lastLineRead = string.Empty;
        private const string SettingFileName = "setting.json";
        private bool _autoScroll = true;

        public MainWindow()
        {
            InitializeComponent();
            LoadSettings();
            SetupTimer();
        }

        private void LoadSettings()
        {
            if (File.Exists(SettingFileName))
            {
                var json = File.ReadAllText(SettingFileName);
                var settings = JsonConvert.DeserializeObject<Settings>(json);
                if (settings != null && !string.IsNullOrEmpty(settings.BluecgFolder))
                {
                    _filePath = GetLatestFile(settings.BluecgFolder);
                    InitializeFilePosition();
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
                Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.White),
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
            // 用户是否在最底部
            if (e.ExtentHeightChange == 0)
            {
                _autoScroll = ChatScrollViewer.VerticalOffset == ChatScrollViewer.ScrollableHeight;
            }

            // 新内容到达底部
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
                    SaveSettings(selectedPath);
                }
            }
        }

        private void SaveSettings(string folderPath)
        {
            var settings = new Settings
            {
                BluecgFolder = folderPath
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

            return latestFile?.FullName;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }

        private void DragWindow(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }

        private class Settings
        {
            public string BluecgFolder { get; set; }
        }
    }
}
