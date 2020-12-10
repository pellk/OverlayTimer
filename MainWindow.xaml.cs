using System;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.ComponentModel;
using System.Windows.Media;
using System.Windows.Threading;
using System.Media;

namespace OverlayTimer
{
    public partial class MainWindow : Window
    {
        readonly DispatcherTimer mainTimer, hoverTimer;
        //readonly BackgroundWorker worker;
        DateTime tStart, tPaused;
        readonly Color[] quarterColors = new Color[] {
            Color.FromRgb(33, 150, 243),
            Color.FromRgb(100, 221, 23),
            Color.FromRgb(255,215,64),
            Color.FromRgb(213, 0, 0)
        };
        int minutes;

        public MainWindow()
        {
            InitializeComponent();

            mainTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            mainTimer.Tick += MainTimer_Tick;

            hoverTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(1000)
            };
            hoverTimer.Tick += hoverTimer_Tick;
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            SizeToContent = SizeToContent.Width;
            Height = SystemParameters.WorkArea.Height;
            Top = 0;
            Left = -1;
            if (StartButton.Content.ToString() == "Start")
            {
                StartButton.Content = "Stop";
                SetTimePanel.Visibility = Visibility.Collapsed;
                if (TimeProgress.Value > 0)
                {
                    tStart += DateTime.Now.Subtract(tPaused);
                    tPaused = tStart;
                    mainTimer.Start();
                }
                else
                {
                    if (int.TryParse(TimeTextBox.Text, out minutes))
                    {
                        TimeProgress.Maximum = minutes * 60.0;
                        TimeProgress.Value = 0.0;
                        tStart = DateTime.Now;
                        tPaused = tStart;
                        mainTimer.Start();
                    }
                }
            }
            else
            {
                mainTimer.Stop();
                StartButton.Content = "Start";
                tPaused = DateTime.Now;
            }
        }

        private void MainTimer_Tick(object sender, EventArgs e)
        {
            double progres = DateTime.Now.Subtract(tStart).TotalSeconds;
            TimeProgress.Value = progres;
            if (DateTime.Now < tStart.AddMinutes(minutes))
            {
                TimeProgress.Foreground = new SolidColorBrush(quarterColors[(int)(TimeProgress.Value / (TimeProgress.Maximum + 1) * 4.0)]);
                int s = (int)(TimeProgress.Maximum - progres);
                Title = string.Format("{0}:{1:00}", s / 60, s % 60);
            }
            else
            {
                mainTimer.Stop();
                StartButton.Content = "Start";
                Title = "Overlay Timer";
                var player = new MediaPlayer();
                string number = new Random().Next(1, 10).ToString("00");
                player.Open(new Uri($@"C:\Windows\Media\Alarm{number}.wav"));
                player.Play();
            }
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            TimeProgress.Value = 0.0;
            StartButton.Content = "Start";
            mainTimer.Stop();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            hoverTimer.Stop();
            mainTimer.Stop();
        }

        private void TimeProgress_MouseMove(object sender, MouseEventArgs e)
        {
            if (SetTimePanel.Visibility == Visibility.Collapsed && !hoverTimer.IsEnabled)
                hoverTimer.Start();
        }

        private void TimeProgress_MouseLeave(object sender, MouseEventArgs e)
        {
            if (!SetTimePanel.IsMouseOver)
            {
                SetTimePanel.Visibility = Visibility.Collapsed;
            }
        }

        private void Grid_MouseLeave(object sender, MouseEventArgs e)
        {
            if (mainTimer.IsEnabled)
                SetTimePanel.Visibility = Visibility.Collapsed;
        }

        private void hoverTimer_Tick(object sender, EventArgs e)
        {
            hoverTimer.Stop();
            if (TimeProgress.IsMouseOver)
                SetTimePanel.Visibility = Visibility.Visible;
        }
    }
}
