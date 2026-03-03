using System;
using DncApp.Models;
using DncApp.Services;

namespace DncApp
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=== ТЕСТУВАННЯ СИСТЕМИ РДЗ ===\n");

            Train testTrain = new Train("T1", "601", "Прус");
            Console.WriteLine("--- Стан поїзда ---");
            Console.WriteLine(testTrain.GetStatus());

            Console.WriteLine("\n--- Тест гальм ---");
            Console.WriteLine($"Початкова позиція крана: {testTrain.BrakePosition} (Поїзне)");
            
            testTrain.ApplyBrakes(3);
            Console.WriteLine($"Позиція крана при гальмуванні: {testTrain.BrakePosition}");
            
            testTrain.ReleaseBrakes();
            Console.WriteLine($"Позиція крана після відпуску: {testTrain.BrakePosition} (Поїзне)");

            Console.WriteLine("\n--- Тест стрілки ---");
            Switch testSwitch = new Switch("C1", "Вхідна");
            Console.WriteLine(testSwitch.GetStatus());
            
            testSwitch.Toggle();
            Console.WriteLine("Після переведення:");
            Console.WriteLine(testSwitch.GetStatus());

            Console.WriteLine("\n--- Тест розкладу руху ---");
            ScheduleManager scheduleManager = new ScheduleManager();
            TimeSpan testTime = new TimeSpan(11, 15, 0);
            Console.WriteLine($"Поточний час для тесту: {testTime:hh\\:mm}");

            var nextTrain = scheduleManager.GetNextTrain(testTime, false);
            if (nextTrain != null)
            {
                Console.WriteLine("Автоматично знайдено наступний поїзд:");
                Console.WriteLine(nextTrain.GetInfo());
            }

            Console.WriteLine("\nТестування завершено.");
        }
    }
}