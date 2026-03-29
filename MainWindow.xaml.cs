using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Controls;
using System.Windows.Threading;
using System.Collections.Generic;
using System.Linq;

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

        // --- МОДЕЛЬ ПОЇЗДА ---
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

        // --- СИСТЕМА КВЕСТІВ ---
        private class GameQuest {
            public string Title { get; set; } = "";
            public string Description { get; set; } = "";
            public Func<List<TrainData>, bool> CheckCompletion { get; set; } = _ => false;
        }
        private List<GameQuest> _allQuests = new List<GameQuest>();
        private GameQuest? _currentQuest = null;
        private bool _isQuestCompleted = false;

        public MainWindow()
        {
            InitializeComponent();
            LogAction("МПЦ РДЗ ініціалізована. Готовність до роботи.");
            
            _trains.Add(new TrainData { Id = 0, Grid = TrainGrid1, DirText = TrainDir1, NumText = TrainNum1 });
            _trains.Add(new TrainData { Id = 1, Grid = TrainGrid2, DirText = TrainDir2, NumText = TrainNum2 });

            _trainTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(40) };
            _trainTimer.Tick += TrainTimer_Tick;
            _trainTimer.Start();

            _clockTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            _clockTimer.Tick += (s, e) => { if (ClockText != null) ClockText.Text = DateTime.Now.ToString("HH:mm:ss"); };
            _clockTimer.Start();

            InitializeQuests();
        }

        private void InitializeQuests() {
            _allQuests = new List<GameQuest> {
                new GameQuest {
                    Title = "🚆 Ранковий рейс",
                    Description = "Завдання: Відправте будь-який поїзд зі ст. Центральна та безпечно доставте його на ст. Озерна.",
                    CheckCompletion = (trains) => trains.Any(t => t.IsActive && t.Location == "Ozerna" && !t.IsMoving)
                },
                new GameQuest {
                    Title = "🔀 Маневрові роботи",
                    Description = "Завдання: Прийміть поїзд на бокову колію ст. Центральна (1П), після чого змініть його напрямок руху (реверс).",
                    CheckCompletion = (trains) => trains.Any(t => t.IsActive && t.Location == "Centralna" && Math.Abs(Canvas.GetTop(t.Grid!) - 167) < 5 && t.Direction == -1)
                },
                new GameQuest {
                    Title = "⚠️ Екстрена ситуація",
                    Description = "Завдання: Згенеруйте поїзд. Під час його руху активуйте систему 'Аварійне перекриття' для перевірки гальм.",
                    CheckCompletion = (trains) => trains.Any(t => t.IsActive && !t.IsMoving && SignalCentralCh?.Fill == Brushes.Red && SignalN?.Fill == Brushes.Red)
                },
                new GameQuest {
                    Title = "⚔️ Складне схрещення",
                    Description = "Завдання: Організуйте одночасне перебування двох поїздів на станції Озерна на різних коліях.",
                    CheckCompletion = (trains) => trains.Count(t => t.IsActive && t.Location == "Ozerna") == 2
                }
            };
        }

        protected internal void BtnGenerateQuest_Click(object sender, RoutedEventArgs e) {
            _currentQuest = _allQuests[_rnd.Next(_allQuests.Count)];
            _isQuestCompleted = false;
            
            if(QuestTitleText != null) QuestTitleText.Text = _currentQuest.Title;
            if(QuestDescText != null) QuestDescText.Text = _currentQuest.Description;
            if(QuestStatusText != null) { 
                QuestStatusText.Text = "🟡 ЗАВДАННЯ ВИКОНУЄТЬСЯ..."; 
                QuestStatusText.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#facc15")); 
            }
            
            System.Media.SystemSounds.Asterisk.Play();
            LogAction($"Отримано нове завдання: {_currentQuest.Title}");
        }

        private void LogAction(string message) {
            if (LogListBox != null) {
                LogListBox.Items.Insert(0, $"[{DateTime.Now:HH:mm:ss}] {message}");
            }
        }

        // --- БЕЗПЕКА СТРІЛОК (ЗАХИСТ ВІД ПЕРЕВЕДЕННЯ ПІД ПОЇЗДОМ) ---
        private bool IsTrainOnSwitch(string stationLoc) {
            foreach (var t in _trains) {
                if (!t.IsActive || t.Grid == null || t.Location != stationLoc) continue;
                double x = Canvas.GetLeft(t.Grid);
                // Зона стрілки Центральна (Х від 580 до 720)
                if (stationLoc == "Centralna" && x >= 580 && x <= 720) return true;
                // Зона стрілки Озерна (Х від 280 до 420)
                if (stationLoc == "Ozerna" && x >= 280 && x <= 420) return true;
            }
            return false;
        }

        protected internal void CentralSwitch_Click(object sender, MouseButtonEventArgs e) {
            if (_isCentralSwitchLocked) { MessageBox.Show("Стрілка замкнена (ШЗ)!", "МПЦ", MessageBoxButton.OK, MessageBoxImage.Warning); return; }
            
            // ПЕРЕВІРКА ЗАЙНЯТОСТІ
            if (IsTrainOnSwitch("Centralna")) {
                System.Media.SystemSounds.Hand.Play();
                MessageBox.Show("УВАГА! Стрілочна ділянка зайнята рухомим складом. Переведення заблоковано СЦБ!", "БЕЗПЕКА МПЦ", MessageBoxButton.OK, MessageBoxImage.Error);
                LogAction("⚠ Спроба переведення стрілки під поїздом (Центральна). Блокування.");
                return;
            }

            _isCentralSwitchPlus = !_isCentralSwitchPlus;
            if (CentralSwitch != null) { CentralSwitch.X1 = _isCentralSwitchPlus ? 700 : 600; CentralSwitch.Y1 = _isCentralSwitchPlus ? 300 : 180; }
            LogAction($"Центральна: Стрілка №1 -> {(_isCentralSwitchPlus ? "ПЛЮС (на 2П)" : "МІНУС (на 1П)")}.");
        }

        protected internal void OzernaSwitch_Click(object sender, MouseButtonEventArgs e) {
            if (_isOzernaSwitchLocked) { MessageBox.Show("Стрілка замкнена (ШЗ)!", "МПЦ", MessageBoxButton.OK, MessageBoxImage.Warning); return; }
            
            // ПЕРЕВІРКА ЗАЙНЯТОСТІ
            if (IsTrainOnSwitch("Ozerna")) {
                System.Media.SystemSounds.Hand.Play();
                MessageBox.Show("УВАГА! Стрілочна ділянка зайнята рухомим складом. Переведення заблоковано СЦБ!", "БЕЗПЕКА МПЦ", MessageBoxButton.OK, MessageBoxImage.Error);
                LogAction("⚠ Спроба переведення стрілки під поїздом (Озерна). Блокування.");
                return;
            }

            _isOzernaSwitchPlus = !_isOzernaSwitchPlus;
            if (OzernaSwitch1 != null) { OzernaSwitch1.X2 = 400; OzernaSwitch1.Y2 = _isOzernaSwitchPlus ? 300 : 180; }
            LogAction($"Озерна: Стрілка №1 -> {(_isOzernaSwitchPlus ? "ПЛЮС (на 1П)" : "МІНУС (на 2П)")}.");
        }

        protected internal void Switch_RightClick(object sender, MouseButtonEventArgs e) {
            if (sender == CentralSwitch && CentralSwitch != null) { 
                _isCentralSwitchLocked = !_isCentralSwitchLocked; 
                CentralSwitch.Stroke = _isCentralSwitchLocked ? Brushes.Yellow : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#e2e8f0")); 
            } 
            else if (sender == OzernaSwitch1 && OzernaSwitch1 != null) { 
                _isOzernaSwitchLocked = !_isOzernaSwitchLocked; 
                OzernaSwitch1.Stroke = _isOzernaSwitchLocked ? Brushes.Yellow : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#e2e8f0")); 
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
            if (BtnStationCentral != null && BtnStationOzerna != null) SwitchStation_Click(loc == "Centralna" ? BtnStationCentral : BtnStationOzerna, null!);
            LogAction($"Додано поїзд №{freeTrain.Number} ({freeTrain.LocoType}) на ст. {loc}.");
        }

        protected internal void Train_Click(object sender, MouseButtonEventArgs e) {
            if (e.ChangedButton == MouseButton.Left) { 
                var t = _trains.Find(tr => tr.Grid == sender); 
                if (t != null) { t.IsMoving = !t.IsMoving; LogAction($"№{t.Number}: {(t.IsMoving ? "РУХ" : "ЗУПИНКА")}"); } 
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
        private void ReverseTrain(TrainData t) { t.Direction *= -1; if(t.DirText != null) t.DirText.Text = t.Direction == 1 ? "▶" : "◀"; LogAction($"Поїзд №{t.Number}: Зміна кабіни."); }
        protected internal void MenuDelete1_Click(object sender, RoutedEventArgs e) { DeleteTrain(_trains[0]); }
        protected internal void MenuDelete2_Click(object sender, RoutedEventArgs e) { DeleteTrain(_trains[1]); }
        private void DeleteTrain(TrainData t) { t.IsActive = false; t.IsMoving = false; t.Location = "None"; if(t.Grid != null) t.Grid.Visibility = Visibility.Hidden; LogAction($"Поїзд №{t.Number} знято з дільниці."); }

        private void TrainTimer_Tick(object? sender, EventArgs e) {
            
            // ПЕРЕВІРКА КВЕСТУ
            if (_currentQuest != null && !_isQuestCompleted) {
                if (_currentQuest.CheckCompletion(_trains)) {
                    _isQuestCompleted = true;
                    if(QuestStatusText != null) {
                        QuestStatusText.Text = "🟢 ЗАВДАННЯ ВИКОНАНО!";
                        QuestStatusText.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#10b981"));
                    }
                    LogAction($"🏆 УСПІХ: Сценарій '{_currentQuest.Title}' пройдено!");
                }
            }

            // ПЕРЕВІРКА ЗІТКНЕНЬ
            var t1 = _trains[0]; var t2 = _trains[1];
            if (t1.IsActive && t2.IsActive && t1.Location == t2.Location && t1.Grid != null && t2.Grid != null) {
                if (Math.Abs(Canvas.GetTop(t1.Grid) - Canvas.GetTop(t2.Grid)) < 15 && Math.Abs(Canvas.GetLeft(t1.Grid) - Canvas.GetLeft(t2.Grid)) < 65) { 
                    t1.IsActive = false; t2.IsActive = false; 
                    TriggerCrash(t1, "Зіткнення на перегоні"); TriggerCrash(t2, "Зіткнення на перегоні"); return;
                }
            }

            foreach (var t in _trains) {
                if (!t.IsActive || !t.IsMoving || t.Grid == null) continue;

                double x = Canvas.GetLeft(t.Grid); double y = Canvas.GetTop(t.Grid);
                bool stopForRed = false;

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

                if (t.Location == "Centralna" && t.Direction == 1) {
                    if (Math.Abs(y - 287) < 5 && !_isCentralSwitchPlus && x >= 695) { t.IsActive = false; TriggerCrash(t, "Вріз стрілки №1"); return; }
                    if (Math.Abs(y - 167) < 5 && _isCentralSwitchPlus && x >= 595) { t.IsActive = false; TriggerCrash(t, "Схід у тупик (стрілка по прямій)"); return; }
                }
                if (t.Location == "Ozerna" && t.Direction == -1) {
                    if (Math.Abs(y - 287) < 5 && !_isOzernaSwitchPlus && x <= 405) { t.IsActive = false; TriggerCrash(t, "Вріз стрілки №1"); return; }
                    if (Math.Abs(y - 167) < 5 && _isOzernaSwitchPlus && x <= 505) { t.IsActive = false; TriggerCrash(t, "Схід у тупик (стрілка по прямій)"); return; }
                }

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

        // --- ВИКЛИК ЮНІТ-ТЕСТІВ ---
        protected internal void BtnRunTests_Click(object sender, RoutedEventArgs e) {
            try {
                string testReport = MpcTests.RunAllTests();
                MessageBox.Show(testReport, "Звіт про модульне тестування (Unit Tests)", MessageBoxButton.OK, MessageBoxImage.Information);
                LogAction("Виконано автоматизоване тестування безпеки МПЦ.");
            } catch (Exception ex) {
                MessageBox.Show($"Помилка запуску тестів. Переконайтеся, що файл MpcTests.cs існує в проєкті.\nДеталі: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}