using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace CybersecurityChatbot
{
    public partial class MainWindow : Window
    {
        private ChatBot _chatBot;

        public MainWindow()
        {
            InitializeComponent();
            LoadAsciiArt();
            PlayVoiceGreeting();
            _chatBot = new ChatBot();
            string greeting = _chatBot.GetGreeting();
            AppendBotMessage(greeting);

            UserInputTextBox.GotFocus += (s, e) =>
            {
                if (UserInputTextBox.Text == "Type your message here...")
                    UserInputTextBox.Text = "";
            };
        }

        private void LoadAsciiArt()
        {
            string asciiArt = @"
   ____      _                  _                  _   
  / ___|   _| |__   ___  _ __  | |__   ___  _ __  | |_ 
 | |  | | | | '_ \ / _ \| '__| | '_ \ / _ \| '__| | __|
 | |__| |_| | |_) | (_) | |    | |_) | (_) | |    | |_ 
  \____\__, |_.__/ \___/|_|    |_.__/ \___/|_|     \__|
       |___/                                            
";
            AsciiArtTextBlock.Text = asciiArt;
        }

        private void PlayVoiceGreeting()
        {
            try
            {
                var psi = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "powershell",
                    Arguments = "-windowstyle hidden -c \"(New-Object Media.SoundPlayer 'greeting.wav').PlaySync()\"",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden
                };
                System.Diagnostics.Process.Start(psi);
            }
            catch
            {
                // Silent fail
            }
        }

        private void PlayVoiceGoodbye()
        {
            try
            {
                var psi = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "powershell",
                    Arguments = "-windowstyle hidden -c \"(New-Object Media.SoundPlayer 'goodbye.wav').PlaySync()\"",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden
                };
                System.Diagnostics.Process.Start(psi);
            }
            catch
            {
                // Silent fail
            }
        }

        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            SendMessage();
        }

        private void UserInputTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SendMessage();
            }
        }

        private void SendMessage()
        {
            string input = UserInputTextBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(input) || input == "Type your message here...")
                return;

            AppendUserMessage(input);
            UserInputTextBox.Text = "";

            string response = _chatBot.ProcessInput(input);

            // Play goodbye voice if bot is saying farewell
            if (response.Contains("Goodbye"))
            {
                PlayVoiceGoodbye();
            }

            AppendBotMessage(response);

            ChatScrollViewer.ScrollToBottom();
        }

        private void AppendUserMessage(string message)
        {
            var border = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(26, 31, 75)),
                CornerRadius = new CornerRadius(10, 10, 0, 10),
                Padding = new Thickness(12),
                Margin = new Thickness(50, 5, 5, 5),
                HorizontalAlignment = HorizontalAlignment.Right,
                MaxWidth = 600
            };

            var textBlock = new TextBlock
            {
                Text = message,
                Foreground = Brushes.White,
                FontSize = 13,
                TextWrapping = TextWrapping.Wrap
            };

            border.Child = textBlock;
            ChatStackPanel.Children.Add(border);
        }

        private void AppendBotMessage(string message)
        {
            var border = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(13, 18, 53)),
                BorderBrush = new SolidColorBrush(Color.FromRgb(0, 212, 170)),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(10, 10, 10, 0),
                Padding = new Thickness(12),
                Margin = new Thickness(5, 5, 50, 5),
                HorizontalAlignment = HorizontalAlignment.Left,
                MaxWidth = 600
            };

            var textBlock = new TextBlock
            {
                Text = message,
                Foreground = new SolidColorBrush(Color.FromRgb(0, 212, 170)),
                FontSize = 13,
                TextWrapping = TextWrapping.Wrap,
                LineHeight = 20
            };

            border.Child = textBlock;
            ChatStackPanel.Children.Add(border);
        }
    }
}