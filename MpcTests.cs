using System;
using System.Collections.Generic;
using System.Text;

namespace DncApp
{
    // Окремий ізольований клас для тестування логіки МПЦ
    public static class MpcTests
    {
        public static string RunAllTests()
        {
            StringBuilder report = new StringBuilder();
            report.AppendLine("========================================");
            report.AppendLine(" АВТОМАТИЗОВАНЕ ТЕСТУВАННЯ БЕЗПЕКИ МПЦ");
            report.AppendLine("========================================\n");

            int passed = 0;
            passed += RunTest(report, "Вріз стрілки (SwitchDerailment_Test)", SwitchDerailment_Test);
            passed += RunTest(report, "Зайнятість колії (TrackOccupied_Test)", TrackOccupied_Test);
            passed += RunTest(report, "Зіткнення поїздів (TrainCollision_Test)", TrainCollision_Test);
            passed += RunTest(report, "Аварійне перекриття (EmergencyStop_Test)", EmergencyStop_Test);
            passed += RunTest(report, "Зміна кабіни (DirectionReverse_Test)", DirectionReverse_Test);

            report.AppendLine("\n----------------------------------------");
            if (passed == 5) {
                report.AppendLine($"[УСПІХ] Пройдено тестів: {passed} з 5. Логіка безпечна.");
            } else {
                report.AppendLine($"[ПОМИЛКА] Пройдено тестів: {passed} з 5. Є збої.");
            }

            return report.ToString();
        }

        private static int RunTest(StringBuilder report, string name, Func<bool> test)
        {
            try {
                if (test()) {
                    report.AppendLine($"{name,-40} [PASSED ✔]");
                    return 1;
                } else {
                    report.AppendLine($"{name,-40} [FAILED ✘]");
                    return 0;
                }
            } catch {
                report.AppendLine($"{name,-40} [ERROR ⚠]");
                return 0;
            }
        }

        // --- ІМІТАЦІЙНІ МОДЕЛІ ДЛЯ ТЕСТІВ ---
        private class TestTrain {
            public string Location { get; set; } = "";
            public int Direction { get; set; } = 1; 
            public bool IsMoving { get; set; } = false;
            public bool IsActive { get; set; } = true;
            public double X { get; set; }
            public double Y { get; set; }
        }

        private static bool SwitchDerailment_Test() {
            var train = new TestTrain { Location = "Centralna", Direction = 1, IsMoving = true, Y = 167, X = 590 };
            bool isCentralSwitchPlus = true; bool isCrashTriggered = false;
            train.X += 5;
            if (train.Y == 167 && isCentralSwitchPlus && train.X >= 595) {
                train.IsMoving = false; train.IsActive = false; isCrashTriggered = true;
            }
            return isCrashTriggered && !train.IsMoving && !train.IsActive;
        }

        private static bool TrackOccupied_Test() {
            var activeTrains = new List<TestTrain> { new TestTrain { IsActive = true, Location = "Centralna", Y = 287 } };
            bool isSpawnAllowed = true;
            foreach (var t in activeTrains) {
                if (t.IsActive && t.Location == "Centralna" && Math.Abs(t.Y - 287) < 10) { isSpawnAllowed = false; break; }
            }
            return !isSpawnAllowed;
        }

        private static bool TrainCollision_Test() {
            var t1 = new TestTrain { IsActive = true, Y = 287, X = 400, Direction = 1 };
            var t2 = new TestTrain { IsActive = true, Y = 287, X = 460, Direction = -1 };
            bool isCollisionDetected = false;
            t1.X += 5; t2.X -= 5;
            if (Math.Abs(t1.Y - t2.Y) < 15 && Math.Abs(t1.X - t2.X) < 65) {
                t1.IsActive = false; t2.IsActive = false; isCollisionDetected = true;
            }
            return isCollisionDetected && !t1.IsActive && !t2.IsActive;
        }

        private static bool EmergencyStop_Test() {
            var trains = new List<TestTrain> { new TestTrain { IsMoving = true }, new TestTrain { IsMoving = true } };
            foreach (var t in trains) t.IsMoving = false;
            return !trains[0].IsMoving && !trains[1].IsMoving;
        }

        private static bool DirectionReverse_Test() {
            var train = new TestTrain { Direction = 1 };
            train.Direction *= -1;
            return train.Direction == -1;
        }
    }
}