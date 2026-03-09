using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Controls;
using System.Windows.Threading;

namespace DncApp
{
    public partial class MainWindow : Window
    {
        private bool _isSwitchPlus = true;
        private DispatcherTimer _trainTimer;

        public MainWindow()
        {
            InitializeComponent();
            LogAction("Система МПЦ ініціалізована.");

            _trainTimer = new DispatcherTimer();
            _trainTimer.Interval = TimeSpan.FromMilliseconds(50);
            _trainTimer.Tick += TrainTimer_Tick;
        }

        private void LogAction(string message)
        {
            string time = DateTime.Now.ToString("HH:mm:ss");
            if (LogListBox != null)
            {
                LogListBox.Items.Insert(0, $"[{time}] {message}");
            }
        }

        protected internal void SwitchStation_Click(object sender, RoutedEventArgs e)
        {
            if (sender == BtnStationCentral)
            {
                CentralCanvas.Visibility = Visibility.Visible;
                OzernaCanvas.Visibility = Visibility.Collapsed;
                BtnStationCentral.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3f3f46"));
                BtnStationOzerna.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2d2d30"));
            }
            else
            {
                CentralCanvas.Visibility = Visibility.Collapsed;
                OzernaCanvas.Visibility = Visibility.Visible;
                BtnStationCentral.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2d2d30"));
                BtnStationOzerna.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3f3f46"));
            }
        }

        protected internal void Switch_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _isSwitchPlus = !_isSwitchPlus;

            if (_isSwitchPlus)
            {
                Switch1Line.X2 = 250;
                Switch1Line.Y2 = 180;
                LogAction("Стрілку 1 переведено в ПЛЮС (маршрут на 2 колію).");
            }
            else
            {
                Switch1Line.X2 = 250;
                Switch1Line.Y2 = 120;
                LogAction("Стрілку 1 переведено в МІНУС (маршрут на 1 колію).");
            }
        }

        protected internal void Signal_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is Ellipse clickedSignal && clickedSignal.ContextMenu != null)
            {
                clickedSignal.ContextMenu.IsOpen = true;
            }
        }

        protected internal void SignalN2_SetRed(object sender, RoutedEventArgs e) { SignalN2.Fill = Brushes.Red; LogAction("Сигнал Н2: Червоний"); }
        protected internal void SignalN2_SetGreen(object sender, RoutedEventArgs e) { SignalN2.Fill = Brushes.LimeGreen; LogAction("Сигнал Н2: Зелений"); }
        
        protected internal void SignalCh_SetRed(object sender, RoutedEventArgs e) { SignalCh.Fill = Brushes.Red; LogAction("Сигнал Ч: Червоний"); }
        protected internal void SignalCh_SetYellow(object sender, RoutedEventArgs e) { SignalCh.Fill = Brushes.Yellow; LogAction("Сигнал Ч: Жовтий"); }
        protected internal void SignalCh_SetWhite(object sender, RoutedEventArgs e) { SignalCh.Fill = Brushes.White; LogAction("Сигнал Ч: Запрошувальний (Білий)"); }

        protected internal void BtnAddTrain_Click(object sender, RoutedEventArgs e)
        {
            TrainsListBox.Items.Add("Поїзд №531 - Машиніст: Прус");
            TrainVisual.Visibility = Visibility.Visible;
            TrainLabel.Visibility = Visibility.Visible;
            
            Canvas.SetLeft(TrainVisual, 50);
            Canvas.SetLeft(TrainLabel, 65);
            
            LogAction("Поїзд №531 прийнято на 2 колію.");
        }

        protected internal void BtnMoveTrain_Click(object sender, RoutedEventArgs e)
        {
            if (SignalN2.Fill == Brushes.Red)
            {
                MessageBox.Show("Відправлення заборонено! Сигнал Н2 червоний.", "Увага", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            LogAction("Поїзд №531 відправляється на перегін...");
            _trainTimer.Start();
        }

        private void TrainTimer_Tick(object sender, EventArgs e)
        {
            double currentLeft = Canvas.GetLeft(TrainVisual);
            
            Canvas.SetLeft(TrainVisual, currentLeft + 2);
            Canvas.SetLeft(TrainLabel, currentLeft + 2 + 15);

            if (currentLeft > 800)
            {
                _trainTimer.Stop();
                TrainVisual.Visibility = Visibility.Hidden;
                TrainLabel.Visibility = Visibility.Hidden;
                SignalN2.Fill = Brushes.Red;
                LogAction("Поїзд №531 прослідував станцію. Сигнал Н2 перекрито.");
            }
        }

        protected internal void BtnEditTrain_Click(object sender, RoutedEventArgs e)
        {
            LogAction("Відкрито меню редагування вагонів.");
        }
    }
}