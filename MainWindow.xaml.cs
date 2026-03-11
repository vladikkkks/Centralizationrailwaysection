using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Controls;
using System.Windows.Threading;
using System.Collections.Generic;

namespace DncApp
{
    public partial class MainWindow : Window
    {
        private bool _isCentralSwitchPlus = true;
        private bool _isCentralSwitchLocked = false;
        private bool _isOzernaSwitchPlus = true;
        private bool _isOzernaSwitchLocked = false;
        
        private DispatcherTimer _trainTimer;
        private int _trainCounter = 531;
        private Random _rnd = new Random();

        private class TrainData {
            public int Id;
            public int Number;
            public string LocoType;
            public string FullName;
            public bool IsActive = false;
            public string Location = "None"; 
            public int Direction = 1; 
            public bool IsMoving = false;
            public Rectangle Visual;
            public TextBlock Label;
        }
        
        private List<TrainData> _trains = new List<TrainData>();

        public MainWindow()
        {
            InitializeComponent();
            LogAction("МПЦ РДЗ ініціалізована. Тренажер готовий.");
            
            _trains.Add(new TrainData { Id = 0, Visual = TrainVisual1, Label = TrainLabel1 });
            _trains.Add(new TrainData { Id = 1, Visual = TrainVisual2, Label = TrainLabel2 });

            _trainTimer = new DispatcherTimer();
            _trainTimer.Interval = TimeSpan.FromMilliseconds(50);
            _trainTimer.Tick += TrainTimer_Tick;
            _trainTimer.Start();
        }

        private void LogAction(string message) {
            string time = DateTime.Now.ToString("HH:mm:ss");
            if (LogListBox != null) LogListBox.Items.Insert(0, $"[{time}] {message}");
            LogListBox.ScrollIntoView(LogListBox.Items[LogListBox.Items.Count - 1]); // Автоскрол
        }

        // --- СТРІЛКИ ТА СИГНАЛИ ---
        protected internal void CentralSwitch_Click(object sender, MouseButtonEventArgs e) {
            if (_isCentralSwitchLocked) { MessageBox.Show("Стрілка замкнена (ШЗ)!", "МПЦ", MessageBoxButton.OK, MessageBoxImage.Warning); return; }
            _isCentralSwitchPlus = !_isCentralSwitchPlus;
            CentralSwitch.X1 = _isCentralSwitchPlus ? 700 : 600; CentralSwitch.Y1 = _isCentralSwitchPlus ? 300 : 180;
            LogAction($"Центральна: Стрілка №1 -> {(_isCentralSwitchPlus ? "ПЛЮС" : "МІНУС")}.");
        }
        protected internal void OzernaSwitch_Click(object sender, MouseButtonEventArgs e) {
            if (_isOzernaSwitchLocked) { MessageBox.Show("Стрілка замкнена (ШЗ)!", "МПЦ", MessageBoxButton.OK, MessageBoxImage.Warning); return; }
            _isOzernaSwitchPlus = !_isOzernaSwitchPlus;
            OzernaSwitch1.X2 = 400; OzernaSwitch1.Y2 = _isOzernaSwitchPlus ? 300 : 180;
            LogAction($"Озерна: Стрілка №1 -> {(_isOzernaSwitchPlus ? "ПЛЮС" : "МІНУС")}.");
        }
        protected internal void Switch_RightClick(object sender, MouseButtonEventArgs e) {
            if (sender == CentralSwitch) { _isCentralSwitchLocked = !_isCentralSwitchLocked; CentralSwitch.Stroke = _isCentralSwitchLocked ? Brushes.Yellow : Brushes.LightGray; LogAction($"Центральна: Стр.1 {(_isCentralSwitchLocked ? "ЗАМКНЕНА" : "РОЗІМКНЕНА")}."); } 
            else if (sender == OzernaSwitch1) { _isOzernaSwitchLocked = !_isOzernaSwitchLocked; OzernaSwitch1.Stroke = _isOzernaSwitchLocked ? Brushes.Yellow : Brushes.LightGray; LogAction($"Озерна: Стр.1 {(_isOzernaSwitchLocked ? "ЗАМКНЕНА" : "РОЗІМКНЕНА")}."); }
        }

        protected internal void BtnEmergencyStop_Click(object sender, RoutedEventArgs e) {
            SignalCentralN1.Fill = Brushes.Red; SignalCentralN2.Fill = Brushes.Red; SignalCentralCh.Fill = Brushes.Red;
            SignalN.Fill = Brushes.Red; SignalCh1.Fill = Brushes.Red;
            foreach (var t in _trains) t.IsMoving = false;
            System.Media.SystemSounds.Exclamation.Play();
            LogAction("АВАРІЯ! Перекриття сигналів. Рух зупинено.");
            MessageBox.Show("Застосовано аварійне перекриття сигналів!", "БЕЗПЕКА МПЦ", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        protected internal void SwitchStation_Click(object sender, RoutedEventArgs e) {
            bool isCentral = sender == BtnStationCentral;
            CentralCanvas.Visibility = isCentral ? Visibility.Visible : Visibility.Collapsed;
            OzernaCanvas.Visibility = !isCentral ? Visibility.Visible : Visibility.Collapsed;
            BtnStationCentral.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(isCentral ? "#333333" : "#222222")); BtnStationCentral.Foreground = isCentral ? Brushes.White : Brushes.Gray;
            BtnStationOzerna.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(!isCentral ? "#333333" : "#222222")); BtnStationOzerna.Foreground = !isCentral ? Brushes.White : Brushes.Gray;
            UpdateTrainVisibility();
        }

        private void UpdateTrainVisibility() {
            foreach(var t in _trains) {
                if (t.IsActive) {
                    t.Visual.Visibility = (t.Location == "Centralna" && CentralCanvas.Visibility == Visibility.Visible) || (t.Location == "Ozerna" && OzernaCanvas.Visibility == Visibility.Visible) ? Visibility.Visible : Visibility.Hidden;
                    t.Label.Visibility = t.Visual.Visibility;
                }
            }
        }

        protected internal void Signal_Click(object sender, MouseButtonEventArgs e) { if (sender is Ellipse s && s.ContextMenu != null) s.ContextMenu.IsOpen = true; }
        protected internal void SignalCentralN1_SetRed(object sender, RoutedEventArgs e) { SignalCentralN1.Fill = Brushes.Red; } protected internal void SignalCentralN1_SetGreen(object sender, RoutedEventArgs e) { SignalCentralN1.Fill = Brushes.Lime; }
        protected internal void SignalCentralN2_SetRed(object sender, RoutedEventArgs e) { SignalCentralN2.Fill = Brushes.Red; } protected internal void SignalCentralN2_SetGreen(object sender, RoutedEventArgs e) { SignalCentralN2.Fill = Brushes.Lime; }
        protected internal void SignalCentralCh_SetRed(object sender, RoutedEventArgs e) { SignalCentralCh.Fill = Brushes.Red; } protected internal void SignalCentralCh_SetGreen(object sender, RoutedEventArgs e) { SignalCentralCh.Fill = Brushes.Lime; } protected internal void SignalCentralCh_SetYellow(object sender, RoutedEventArgs e) { SignalCentralCh.Fill = Brushes.Yellow; }
        protected internal void SignalN_SetRed(object sender, RoutedEventArgs e) { SignalN.Fill = Brushes.Red; } protected internal void SignalN_SetGreen(object sender, RoutedEventArgs e) { SignalN.Fill = Brushes.Lime; } protected internal void SignalN_SetYellow(object sender, RoutedEventArgs e) { SignalN.Fill = Brushes.Yellow; }
        protected internal void SignalCh1_SetRed(object sender, RoutedEventArgs e) { SignalCh1.Fill = Brushes.Red; } protected internal void SignalCh1_SetGreen(object sender, RoutedEventArgs e) { SignalCh1.Fill = Brushes.Lime; }

        // --- СПАВН ПОЇЗДІВ ---
        private bool IsTrackOccupied(string location, double targetY) {
            foreach(var t in _trains) if (t.IsActive && t.Location == location && Canvas.GetTop(t.Visual) == targetY) return true;
            return false;
        }

        protected internal void BtnAddTrainCentral_Click(object sender, RoutedEventArgs e) { TryAddTrain("Centralna", 1, 50, _isCentralSwitchPlus ? 290 : 170); }
        protected internal void BtnAddTrainOzerna_Click(object sender, RoutedEventArgs e) { TryAddTrain("Ozerna", -1, 850, _isOzernaSwitchPlus ? 290 : 170); }

        private void TryAddTrain(string location, int direction, double startX, double targetY) {
            if (IsTrackOccupied(location, targetY)) { MessageBox.Show("Колія вже зайнята іншим поїздом.", "Контроль", MessageBoxButton.OK, MessageBoxImage.Warning); return; }
            var freeTrain = _trains.Find(t => !t.IsActive);
            if (freeTrain == null) { MessageBox.Show("Ліміт рухомого складу (макс. 2).", "МПЦ", MessageBoxButton.OK, MessageBoxImage.Information); return; }

            freeTrain.Number = _trainCounter++;
            freeTrain.LocoType = _rnd.Next(2) == 0 ? "ТУ2" : "ТУ7А";
            freeTrain.FullName = $"№{freeTrain.Number} ({freeTrain.LocoType})";
            
            // Відновлюємо колір, якщо він був змінений після аварії
            freeTrain.Visual.Fill = freeTrain.Id == 0 ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2196F3")) : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF5722"));
            
            freeTrain.IsActive = true; freeTrain.Location = location; freeTrain.Direction = direction; freeTrain.IsMoving = false;
            freeTrain.Label.Text = $"№{freeTrain.Number}";
            
            Canvas.SetLeft(freeTrain.Visual, startX); Canvas.SetLeft(freeTrain.Label, startX);
            Canvas.SetTop(freeTrain.Visual, targetY); Canvas.SetTop(freeTrain.Label, targetY - 20);

            SwitchStation_Click(location == "Centralna" ? BtnStationCentral : BtnStationOzerna, null);
            LogAction($"Додано: {freeTrain.FullName} на ст. {(location == "Centralna" ? "Центральна" : "Озерна")}.");
        }

        // --- УПРАВЛІННЯ ЧЕРЕЗ КЛІК ПО ПОЇЗДУ ---
        protected internal void Train_Click(object sender, MouseButtonEventArgs e) {
            if (sender is Rectangle r && r.ContextMenu != null) r.ContextMenu.IsOpen = true;
        }

        protected internal void MenuInfo1_Click(object sender, RoutedEventArgs e) { ShowInfo(_trains[0]); }
        protected internal void MenuInfo2_Click(object sender, RoutedEventArgs e) { ShowInfo(_trains[1]); }
        private void ShowInfo(TrainData t) { MessageBox.Show($"Поїзд: №{t.Number}\nЛокомотив: {t.LocoType}\nМашиніст: Прус В.\nЛокація: ст. {t.Location}\nНапрямок: {(t.Direction == 1 ? "Вправо" : "Вліво")}\nСтан: {(t.IsMoving ? "Їде" : "Стоїть")}", "Довідка ДНЦ", MessageBoxButton.OK, MessageBoxImage.Information); }

        protected internal void MenuMove1_Click(object sender, RoutedEventArgs e) { ToggleMove(_trains[0]); }
        protected internal void MenuMove2_Click(object sender, RoutedEventArgs e) { ToggleMove(_trains[1]); }
        private void ToggleMove(TrainData t) { t.IsMoving = !t.IsMoving; LogAction($"Поїзд №{t.Number}: {(t.IsMoving ? "Рух розпочато" : "Зупинено")}."); }

        protected internal void MenuReverse1_Click(object sender, RoutedEventArgs e) { ReverseTrain(_trains[0]); }
        protected internal void MenuReverse2_Click(object sender, RoutedEventArgs e) { ReverseTrain(_trains[1]); }
        private void ReverseTrain(TrainData t) { t.Direction *= -1; LogAction($"Поїзд №{t.Number}: Зміна кабіни."); }

        protected internal void MenuDelete1_Click(object sender, RoutedEventArgs e) { DeleteTrain(_trains[0]); }
        protected internal void MenuDelete2_Click(object sender, RoutedEventArgs e) { DeleteTrain(_trains[1]); }
        private void DeleteTrain(TrainData t) {
            t.IsActive = false; t.IsMoving = false; t.Location = "None"; // Повний скид
            t.Visual.Visibility = Visibility.Hidden; t.Label.Visibility = Visibility.Hidden;
            LogAction($"Поїзд №{t.Number} знято з дільниці.");
        }

        // --- ЛОГІКА РУХУ, ВРІЗУ ТА ЗІТКНЕНЬ ---
        private void TrainTimer_Tick(object? sender, EventArgs e) {
            
            // ПЕРЕВІРКА ЗІТКНЕНЬ
            var t1 = _trains[0]; var t2 = _trains[1];
            if (t1.IsActive && t2.IsActive && t1.Location == t2.Location) {
                if (Math.Abs(Canvas.GetTop(t1.Visual) - Canvas.GetTop(t2.Visual)) < 10) { 
                    if (Math.Abs(Canvas.GetLeft(t1.Visual) - Canvas.GetLeft(t2.Visual)) < 50) { 
                        t1.IsMoving = false; t2.IsMoving = false;
                        System.Media.SystemSounds.Hand.Play();
                        LogAction($"АВАРІЯ! Зіткнення №{t1.Number} та №{t2.Number}.");
                        MessageBox.Show($"КАТАСТРОФА!\nЗіткнення поїздів на ст. {t1.Location}.", "АВАРІЯ", MessageBoxButton.OK, MessageBoxImage.Error);
                        DeleteTrain(t1); DeleteTrain(t2); 
                        return;
                    }
                }
            }

            foreach (var t in _trains) {
                if (!t.IsActive || !t.IsMoving) continue;

                double x = Canvas.GetLeft(t.Visual); double y = Canvas.GetTop(t.Visual);
                bool stopForRed = false;

                // 1. СИГНАЛИ (Зупинка перед червоним)
                if (t.Direction == 1) { 
                    if (t.Location == "Centralna") {
                        if (y == 290 && x >= 590 && x < 670 && SignalCentralN1.Fill == Brushes.Red) stopForRed = true;
                        if (y == 170 && x >= 490 && x < 570 && SignalCentralN2.Fill == Brushes.Red) stopForRed = true;
                    } else if (t.Location == "Ozerna" && x >= 90 && x < 170 && SignalN.Fill == Brushes.Red) stopForRed = true;
                } else { 
                    if (t.Location == "Ozerna" && y == 290 && x <= 540 && x > 460 && SignalCh1.Fill == Brushes.Red) stopForRed = true;
                    else if (t.Location == "Centralna" && x <= 900 && x > 820 && SignalCentralCh.Fill == Brushes.Red) stopForRed = true;
                }
                if (stopForRed) continue;

                // 2. ВРІЗ СТРІЛКИ (Пошерсний рух)
                if (t.Direction == 1 && t.Location == "Centralna") {
                    if (y == 290 && !_isCentralSwitchPlus && x >= 680 && x < 710) { TriggerDerailment(t, "1 (Центральна)"); return; }
                    if (y == 170 && _isCentralSwitchPlus && x >= 580 && x < 610) { TriggerDerailment(t, "1 (Центральна)"); return; }
                }
                if (t.Direction == -1 && t.Location == "Ozerna") {
                    if (y == 290 && !_isOzernaSwitchPlus && x <= 420 && x > 390) { TriggerDerailment(t, "1 (Озерна)"); return; }
                    if (y == 170 && _isOzernaSwitchPlus && x <= 420 && x > 390) { TriggerDerailment(t, "1 (Озерна)"); return; }
                }

                // 3. РУХ
                x += 5 * t.Direction;
                Canvas.SetLeft(t.Visual, x); Canvas.SetLeft(t.Label, x + 5);

                // 4. МАРШРУТИЗАЦІЯ ТА АВТОПЕРЕКРИТТЯ
                if (t.Direction == 1) {
                    if (t.Location == "Centralna") {
                        if (x >= 670 && x < 680) { SignalCentralN1.Fill = Brushes.Red; SignalCentralN2.Fill = Brushes.Red; }
                        if (x > 950) { t.Location = "Ozerna"; Canvas.SetLeft(t.Visual, 20); Canvas.SetTop(t.Visual, 290); Canvas.SetTop(t.Label, 270); UpdateTrainVisibility(); }
                    } else if (t.Location == "Ozerna") {
                        if (x >= 300 && x <= 400 && !_isOzernaSwitchPlus) { double p = (x - 300) / 100.0; Canvas.SetTop(t.Visual, 290 - (120 * p)); Canvas.SetTop(t.Label, 270 - (120 * p)); }
                        if (x >= 170 && x < 180 && SignalN.Fill != Brushes.Red) SignalN.Fill = Brushes.Red;
                        if (x > 850) { t.IsMoving = false; }
                    }
                } else {
                    if (t.Location == "Ozerna") {
                        if (x >= 300 && x <= 400 && !_isOzernaSwitchPlus) { double p = (400 - x) / 100.0; Canvas.SetTop(t.Visual, 170 + (120 * p)); Canvas.SetTop(t.Label, 150 + (120 * p)); }
                        if (x <= 460 && x > 450 && SignalCh1.Fill != Brushes.Red) SignalCh1.Fill = Brushes.Red;
                        if (x < 20) { t.Location = "Centralna"; Canvas.SetLeft(t.Visual, 950); Canvas.SetTop(t.Visual, 290); Canvas.SetTop(t.Label, 270); UpdateTrainVisibility(); }
                    } else if (t.Location == "Centralna") {
                        if (x <= 820 && x > 810 && SignalCentralCh.Fill != Brushes.Red) SignalCentralCh.Fill = Brushes.Red;
                        if (x >= 600 && x <= 700 && !_isCentralSwitchPlus) { double p = (700 - x) / 100.0; Canvas.SetTop(t.Visual, 290 - (120 * p)); Canvas.SetTop(t.Label, 270 - (120 * p)); }
                        if (x < 50) { t.IsMoving = false; }
                    }
                }
            }
        }

        private void TriggerDerailment(TrainData t, string switchName) {
            t.IsMoving = false; t.Visual.Fill = Brushes.DarkRed; 
            System.Media.SystemSounds.Exclamation.Play();
            LogAction($"АВАРІЯ! Вріз стрілки №{switchName} поїздом №{t.Number}.");
            MessageBox.Show($"Поїзд №{t.Number} здійснив вріз стрілки №{switchName} та зійшов з рейок.", "СХОДЖЕННЯ РУХОМОГО СКЛАДУ", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}