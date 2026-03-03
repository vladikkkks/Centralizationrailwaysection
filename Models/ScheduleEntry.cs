using System;

namespace DncApp.Models
{
    public class ScheduleEntry
    {
        public string TrainNumber { get; set; }
        public string DepartureStation { get; set; }
        public string ArrivalStation { get; set; }
        public TimeSpan DepartureTime { get; set; }
        public TimeSpan ArrivalTime { get; set; }
        public bool IsWeekendOnly { get; set; }

        public ScheduleEntry(string trainNumber, string depStation, string arrStation, int depHour, int depMinute, int arrHour, int arrMinute, bool isWeekendOnly = false)
        {
            TrainNumber = trainNumber;
            DepartureStation = depStation;
            ArrivalStation = arrStation;
            DepartureTime = new TimeSpan(depHour, depMinute, 0);
            ArrivalTime = new TimeSpan(arrHour, arrMinute, 0);
            IsWeekendOnly = isWeekendOnly;
        }

        public string GetInfo()
        {
            string days = IsWeekendOnly ? "(Тільки вихідні)" : "(Щоденно)";
            return $"Поїзд №{TrainNumber} {days}: {DepartureStation} {DepartureTime:hh\\:mm} -> {ArrivalStation} {ArrivalTime:hh\\:mm}";
        }
    }
}