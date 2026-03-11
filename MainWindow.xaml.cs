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
        
        private DispatcherTimer? _trainTimer;
        private DispatcherTimer? _clockTimer;
        private int _trainCounter = 531;
        private Random _rnd = new Random();

        // Цей клас чітко визначає всі властивості поїзда
        private class TrainData {
            public int Id;
            public int Number;
            public string LocoType = "";
            public bool IsActive = false;
            public string Location = "None"; 
            public int Direction = 1; 
            public bool IsMoving = false;
            public FrameworkElement? Grid;
            public TextBlock? DirText;
            public TextBlock? NumText;
        }
        
        private List<TrainData> _trains = new List<TrainData>();

        public MainWindow()
        {
            InitializeComponent();
            LogAction("МПЦ РДЗ ініціалізована. Повний контроль увімкнено.");
            
            _trains.Add(new TrainData { Id = 0, Grid = TrainGrid1, DirText = TrainDir1, NumText = TrainNum1 });
            _trains.Add(new TrainData { Id = 1, Grid = TrainGrid2, DirText = TrainDir2, NumText = TrainNum2 });

            _trainTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(40) };
            _trainTimer.Tick += TrainTimer_Tick;
            _trainTimer.Start();

            _clockTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            _clockTimer.Tick += (s, e) => { if (ClockText != null) ClockText.Text = DateTime.Now.ToString("HH:mm:ss"); };
            _clockTimer.Start();
        }

        private void LogAction(string message) {
            if (LogListBox != null) {
                LogListBox.Items.Insert(0, $"[{DateTime.Now:HH:mm:ss}] {message}");
                if (LogListBox.Items.Count > 0) {
                    LogListBox.ScrollIntoView(LogListBox.Items[LogListBox.Items.Count - 1]);
                }
            }
        }

        // --- СТРІЛКИ ТА СИГНАЛИ ---
        protected internal void CentralSwitch_Click(object sender, MouseButtonEventArgs e) {
            if (_isCentralSwitchLocked) { MessageBox.Show("Стрілка замкнена (ШЗ)!", "МПЦ", MessageBoxButton.OK, MessageBoxImage.Warning); return; }
            _isCentralSwitchPlus = !_isCentralSwitchPlus;
            if (CentralSwitch != null) { CentralSwitch.X1 = _isCentralSwitchPlus ? 700 : 600; CentralSwitch.Y1 = _isCentralSwitchPlus ? 300 : 180; }
            LogAction($"Центральна: Стрілка №1 -> {(_isCentralSwitchPlus ? "ПЛЮС (на 2П)" : "МІНУС (на 1П)")}.");
        }

        protected internal void OzernaSwitch_Click(object sender, MouseButtonEventArgs e) {
            if (_isOzernaSwitchLocked) { MessageBox.Show("Стрілка замкнена (ШЗ)!", "МПЦ", MessageBoxButton.OK, MessageBoxImage.Warning); return; }
            _isOzernaSwitchPlus = !_isOzernaSwitchPlus;
            if (OzernaSwitch1 != null) { OzernaSwitch1.X2 = 400; OzernaSwitch1.Y2 = _isOzernaSwitchPlus ? 300 : 180; }
            LogAction($"Озерна: Стрілка №1 -> {(_isOzernaSwitchPlus ? "ПЛЮС (на 1П)" : "МІНУС (на 2П)")}.");
        }

        protected internal void Switch_RightClick(object sender, MouseButtonEventArgs e) {
            if (sender == CentralSwitch && CentralSwitch != null) { 
                _isCentralSwitchLocked = !_isCentralSwitchLocked; 
                CentralSwitch.Stroke = _isCentralSwitchLocked ? Brushes.Yellow : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#e2e8f0")); 
                LogAction($"Центральна: Стр.1 {(_isCentralSwitchLocked ? "ЗАМКНЕНА" : "РОЗІМКНЕНА")}."); 
            } 
            else if (sender == OzernaSwitch1 && OzernaSwitch1 != null) { 
                _isOzernaSwitchLocked = !_isOzernaSwitchLocked; 
                OzernaSwitch1.Stroke = _isOzernaSwitchLocked ? Brushes.Yellow : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#e2e8f0")); 
                LogAction($"Озерна: Стр.1 {(_isOzernaSwitchLocked ? "ЗАМКНЕНА" : "РОЗІМКНЕНА")}."); 
            }
        }

        protected internal void BtnEmergencyStop_Click(object sender, RoutedEventArgs e) {
            if(SignalCentralN1 != null) SignalCentralN1.Fill = Brushes.Red; 
            if(SignalCentralN2 != null) SignalCentralN2.Fill = Brushes.Red; 
            if(SignalCentralCh != null) SignalCentralCh.Fill = Brushes.Red;
            if(SignalN != null) SignalN.Fill = Brushes.Red; 
            if(SignalCh1 != null) SignalCh1.Fill = Brushes.Red; 
            if(SignalCh2 != null) SignalCh2.Fill = Brushes.Red;
            foreach (var t in _trains) t.IsMoving = false;
            System.Media.SystemSounds.Exclamation.Play();
            LogAction("⚠ АВАРІЯ! Перекриття сигналів. Рух зупинено.");
        }

        protected internal void SwitchStation_Click(object sender, RoutedEventArgs e) {
            bool isCentral = sender == BtnStationCentral;
            if(CentralCanvas != null) CentralCanvas.Visibility = isCentral ? Visibility.Visible : Visibility.Collapsed;
            if(OzernaCanvas != null) OzernaCanvas.Visibility = !isCentral ? Visibility.Visible : Visibility.Collapsed;
            if(BtnStationCentral != null) { BtnStationCentral.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(isCentral ? "#334155" : "#1e293b")); BtnStationCentral.Foreground = isCentral ? Brushes.White : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#94a3b8")); }
            if(BtnStationOzerna != null) { BtnStationOzerna.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(!isCentral ? "#334155" : "#1e293b")); BtnStationOzerna.Foreground = !isCentral ? Brushes.White : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#94a3b8")); }
            UpdateTrainVisibility();
        }

        private void UpdateTrainVisibility() {
            foreach(var t in _trains) {
                if (t.IsActive && t.Grid != null) {
                    t.Grid.Visibility = (t.Location == "Centralna" && CentralCanvas?.Visibility == Visibility.Visible) || (t.Location == "Ozerna" && OzernaCanvas?.Visibility == Visibility.Visible) ? Visibility.Visible : Visibility.Hidden;
                }
            }
        }

        protected internal void Signal_Click(object sender, MouseButtonEventArgs e) { if (sender is Ellipse s && s.ContextMenu != null) s.ContextMenu.IsOpen = true; }
        protected internal void SignalCentralN1_SetRed(object sender, RoutedEventArgs e) { if(SignalCentralN1!=null) SignalCentralN1.Fill = Brushes.Red; } protected internal void SignalCentralN1_SetGreen(object sender, RoutedEventArgs e) { if(SignalCentralN1!=null) SignalCentralN1.Fill = Brushes.Lime; }
        protected internal void SignalCentralN2_SetRed(object sender, RoutedEventArgs e) { if(SignalCentralN2!=null) SignalCentralN2.Fill = Brushes.Red; } protected internal void SignalCentralN2_SetGreen(object sender, RoutedEventArgs e) { if(SignalCentralN2!=null) SignalCentralN2.Fill = Brushes.Lime; }
        protected internal void SignalCentralCh_SetRed(object sender, RoutedEventArgs e) { if(SignalCentralCh!=null) SignalCentralCh.Fill = Brushes.Red; } protected internal void SignalCentralCh_SetGreen(object sender, RoutedEventArgs e) { if(SignalCentralCh!=null) SignalCentralCh.Fill = Brushes.Lime; } protected internal void SignalCentralCh_SetYellow(object sender, RoutedEventArgs e) { if(SignalCentralCh!=null) SignalCentralCh.Fill = Brushes.Yellow; }
        protected internal void SignalN_SetRed(object sender, RoutedEventArgs e) { if(SignalN!=null) SignalN.Fill = Brushes.Red; } protected internal void SignalN_SetGreen(object sender, RoutedEventArgs e) { if(SignalN!=null) SignalN.Fill = Brushes.Lime; } protected internal void SignalN_SetYellow(object sender, RoutedEventArgs e) { if(SignalN!=null) SignalN.Fill = Brushes.Yellow; }
        protected internal void SignalCh1_SetRed(object sender, RoutedEventArgs e) { if(SignalCh1!=null) SignalCh1.Fill = Brushes.Red; } protected internal void SignalCh1_SetGreen(object sender, RoutedEventArgs e) { if(SignalCh1!=null) SignalCh1.Fill = Brushes.Lime; }
        protected internal void SignalCh2_SetRed(object sender, RoutedEventArgs e) { if(SignalCh2!=null) SignalCh2.Fill = Brushes.Red; } protected internal void SignalCh2_SetGreen(object sender, RoutedEventArgs e) { if(SignalCh2!=null) SignalCh2.Fill = Brushes.Lime; }

        // --- СПАВН ПОЇЗДІВ ---
        protected internal void BtnAddTrainCentral_Click(object sender, RoutedEventArgs e) { TryAddTrain("Centralna", 1, 50, _isCentralSwitchPlus ? 287 : 167); }
        protected internal void BtnAddTrainOzerna_Click(object sender, RoutedEventArgs e) { TryAddTrain("Ozerna", -1, 850, _isOzernaSwitchPlus ? 287 : 167); }

        private void TryAddTrain(string loc, int dir, double startX, double targetY) {
            var freeTrain = _trains.Find(t => !t.IsActive);
            if (freeTrain == null) { MessageBox.Show("Ліміт рухомого складу (макс. 2).", "МПЦ", MessageBoxButton.OK, MessageBoxImage.Information); return; }

            freeTrain.Number = _trainCounter++;
            freeTrain.LocoType = _rnd.Next(2) == 0 ? "ТУ2" : "ТУ7А";
            freeTrain.IsActive = true; 
            freeTrain.Location = loc; 
            freeTrain.Direction = dir; 
            freeTrain.IsMoving = false;
            
            if (freeTrain.Grid != null && freeTrain.NumText != null && freeTrain.DirText != null) {
                var rect = ((Grid)freeTrain.Grid).Children[0] as Rectangle;
                if (rect != null) rect.Fill = freeTrain.Id == 0 ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3b82f6")) : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#f97316"));
                freeTrain.NumText.Text = $"№{freeTrain.Number}";
                freeTrain.DirText.Text = dir == 1 ? "▶" : "◀";
                
                Canvas.SetLeft(freeTrain.Grid, startX); Canvas.SetTop(freeTrain.Grid, targetY);
            }
            if (BtnStationCentral != null && BtnStationOzerna != null) {
                SwitchStation_Click(loc == "Centralna" ? BtnStationCentral : BtnStationOzerna, null!);
            }
            LogAction($"Додано поїзд №{freeTrain.Number} ({freeTrain.LocoType}) на ст. {loc}.");
        }

        // --- УПРАВЛІННЯ ---
        protected internal void Train_Click(object sender, MouseButtonEventArgs e) {
            if (e.ChangedButton == MouseButton.Left) { 
                var t = _trains.Find(tr => tr.Grid == sender); 
                if (t != null) { 
                    t.IsMoving = !t.IsMoving; 
                    LogAction($"№{t.Number}: {(t.IsMoving ? "РУХ" : "ЗУПИНКА")}"); 
                } 
            }
        }
        
        protected internal void MenuInfo1_Click(object sender, RoutedEventArgs e) { ShowInfo(_trains[0]); }
        protected internal void MenuInfo2_Click(object sender, RoutedEventArgs e) { ShowInfo(_trains[1]); }
        private void ShowInfo(TrainData t) { 
            if (InfoTitle != null) InfoTitle.Text = $"Поїзд №{t.Number}"; 
            if (InfoLoco != null) InfoLoco.Text = t.LocoType;
            if (InfoStation != null) InfoStation.Text = t.Location == "Centralna" ? "ст. Центральна" : "ст. Озерна";
            if (InfoDirection != null) InfoDirection.Text = t.Direction == 1 ? "Вправо" : "Вліво";
            if (InfoState != null) { 
                InfoState.Text = t.IsMoving ? "◉ У РУСІ" : "◉ ЗУПИНЕНО"; 
                InfoState.Foreground = t.IsMoving ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#10b981")) : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#ef4444")); 
            }
            if (InfoPopup != null) InfoPopup.Visibility = Visibility.Visible; 
        }

        protected internal void BtnCloseInfo_Click(object sender, RoutedEventArgs e) { if (InfoPopup != null) InfoPopup.Visibility = Visibility.Collapsed; }

        protected internal void MenuMove1_Click(object sender, RoutedEventArgs e) { _trains[0].IsMoving = !_trains[0].IsMoving; }
        protected internal void MenuMove2_Click(object sender, RoutedEventArgs e) { _trains[1].IsMoving = !_trains[1].IsMoving; }
        protected internal void MenuReverse1_Click(object sender, RoutedEventArgs e) { ReverseTrain(_trains[0]); }
        protected internal void MenuReverse2_Click(object sender, RoutedEventArgs e) { ReverseTrain(_trains[1]); }
        private void ReverseTrain(TrainData t) { 
            t.Direction *= -1; 
            if(t.DirText != null) t.DirText.Text = t.Direction == 1 ? "▶" : "◀";
            LogAction($"Поїзд №{t.Number}: Зміна кабіни."); 
        }

        protected internal void MenuDelete1_Click(object sender, RoutedEventArgs e) { DeleteTrain(_trains[0]); }
        protected internal void MenuDelete2_Click(object sender, RoutedEventArgs e) { DeleteTrain(_trains[1]); }
        private void DeleteTrain(TrainData t) {
            t.IsActive = false; t.IsMoving = false; t.Location = "None"; 
            if(t.Grid != null) t.Grid.Visibility = Visibility.Hidden;
            LogAction($"Поїзд №{t.Number} знято з дільниці.");
        }

        // --- ЛОГІКА РУХУ ТА АВАРІЙ ---
        private void TrainTimer_Tick(object? sender, EventArgs e) {
            
            // 1. ПЕРЕВІРКА ЗІТКНЕНЬ
            var t1 = _trains[0]; var t2 = _trains[1];
            if (t1.IsActive && t2.IsActive && t1.Location == t2.Location && t1.Grid != null && t2.Grid != null) {
                if (Math.Abs(Canvas.GetTop(t1.Grid) - Canvas.GetTop(t2.Grid)) < 15) { 
                    if (Math.Abs(Canvas.GetLeft(t1.Grid) - Canvas.GetLeft(t2.Grid)) < 65) { 
                        t1.IsActive = false; t2.IsActive = false; 
                        TriggerCrash(t1, "Зіткнення"); 
                        TriggerCrash(t2, "Зіткнення"); 
                        return;
                    }
                }
            }

            foreach (var t in _trains) {
                if (!t.IsActive || !t.IsMoving || t.Grid == null) continue;

                double x = Canvas.GetLeft(t.Grid); double y = Canvas.GetTop(t.Grid);
                bool stopForRed = false;

                // СИГНАЛИ
                if (t.Direction == 1 && t.Location == "Centralna") { 
                    if (Math.Abs(y - 167) < 5 && x >= 550 && x < 570 && SignalCentralN1?.Fill == Brushes.Red) stopForRed = true;
                    if (Math.Abs(y - 287) < 5 && x >= 650 && x < 670 && SignalCentralN2?.Fill == Brushes.Red) stopForRed = true;
                } else if (t.Direction == -1 && t.Location == "Centralna") {
                    if (x <= 840 && x > 820 && SignalCentralCh?.Fill == Brushes.Red) stopForRed = true;
                } else if (t.Direction == 1 && t.Location == "Ozerna") {
                    if (x >= 150 && x < 170 && SignalN?.Fill == Brushes.Red) stopForRed = true;
                } else if (t.Direction == -1 && t.Location == "Ozerna") {
                    if (Math.Abs(y - 287) < 5 && x <= 480 && x > 460 && SignalCh1?.Fill == Brushes.Red) stopForRed = true;
                    if (Math.Abs(y - 167) < 5 && x <= 480 && x > 460 && SignalCh2?.Fill == Brushes.Red) stopForRed = true;
                }
                if (stopForRed) continue;

                // ВРІЗ СТРІЛКИ
                if (t.Location == "Centralna" && t.Direction == 1) {
                    if (Math.Abs(y - 287) < 5 && !_isCentralSwitchPlus && x >= 695) { t.IsActive = false; TriggerCrash(t, "Вріз стрілки №1"); return; }
                    if (Math.Abs(y - 167) < 5 && _isCentralSwitchPlus && x >= 595) { t.IsActive = false; TriggerCrash(t, "Схід у тупик (стрілка по прямій)"); return; }
                }
                if (t.Location == "Ozerna" && t.Direction == -1) {
                    if (Math.Abs(y - 287) < 5 && !_isOzernaSwitchPlus && x <= 405) { t.IsActive = false; TriggerCrash(t, "Вріз стрілки №1"); return; }
                    if (Math.Abs(y - 167) < 5 && _isOzernaSwitchPlus && x <= 505) { t.IsActive = false; TriggerCrash(t, "Схід у тупик (стрілка по прямій)"); return; }
                }

                // РУХ ТА ДІАГОНАЛІ
                x += 5 * t.Direction;
                Canvas.SetLeft(t.Grid, x);

                if (t.Direction == 1) {
                    if (t.Location == "Centralna") {
                        if (x >= 670 && x < 680 && SignalCentralN1 != null && SignalCentralN2 != null) { SignalCentralN1.Fill = Brushes.Red; SignalCentralN2.Fill = Brushes.Red; }
                        if (x >= 600 && x <= 700 && !_isCentralSwitchPlus && Math.Abs(y - 287) > 10) { double p = (x - 600) / 100.0; Canvas.SetTop(t.Grid, 167 + (120 * p)); }
                        if (x > 1050) { t.Location = "Ozerna"; Canvas.SetLeft(t.Grid, 20); Canvas.SetTop(t.Grid, 287); UpdateTrainVisibility(); }
                    } else if (t.Location == "Ozerna") {
                        if (x >= 300 && x <= 400 && !_isOzernaSwitchPlus) { double p = (x - 300) / 100.0; Canvas.SetTop(t.Grid, 287 - (120 * p)); }
                        if (x >= 170 && x < 180 && SignalN != null && SignalN.Fill != Brushes.Red) SignalN.Fill = Brushes.Red;
                        if (x > 840) t.IsMoving = false;
                    }
                } else {
                    if (t.Location == "Ozerna") {
                        if (x <= 460 && x > 450) { if(SignalCh1!=null)SignalCh1.Fill=Brushes.Red; if(SignalCh2!=null)SignalCh2.Fill=Brushes.Red; }
                        if (x >= 300 && x <= 400 && !_isOzernaSwitchPlus && Math.Abs(y - 287) > 10) { double p = (400 - x) / 100.0; Canvas.SetTop(t.Grid, 167 + (120 * p)); }
                        if (x < 20) { t.Location = "Centralna"; Canvas.SetLeft(t.Grid, 950); Canvas.SetTop(t.Grid, 287); UpdateTrainVisibility(); }
                    } else if (t.Location == "Centralna") {
                        if (x <= 820 && x > 810 && SignalCentralCh != null && SignalCentralCh.Fill != Brushes.Red) SignalCentralCh.Fill = Brushes.Red;
                        if (x >= 600 && x <= 700 && !_isCentralSwitchPlus) { double p = (700 - x) / 100.0; Canvas.SetTop(t.Grid, 287 - (120 * p)); }
                        if (x < 50) t.IsMoving = false;
                    }
                }
            }
        }

        private void TriggerCrash(TrainData t, string reason) {
            t.IsMoving = false; 
            var rect = (t.Grid as Grid)?.Children[0] as Rectangle;
            if(rect != null) rect.Fill = Brushes.DarkRed; 
            System.Media.SystemSounds.Hand.Play();
            LogAction($"АВАРІЯ! {reason}. Поїзд №{t.Number}.");
            MessageBox.Show($"КАТАСТРОФА!\n{reason}.\nПоїзд №{t.Number} зійшов з рейок/пошкоджено.", "БЕЗПЕКА", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}