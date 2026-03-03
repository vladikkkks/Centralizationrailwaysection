using System;
using System.Collections.Generic;
using System.Linq;
using DncApp.Models;

namespace DncApp.Services
{
    public class ScheduleManager
    {
        private readonly List<ScheduleEntry> _schedule;

        public ScheduleManager()
        {
            _schedule = new List<ScheduleEntry>
            {
                new ScheduleEntry("529", "Центральна", "Озерна", 10, 00, 10, 10, true),
                new ScheduleEntry("530", "Озерна", "Центральна", 10, 20, 10, 30, true),
                
                new ScheduleEntry("531", "Центральна", "Озерна", 11, 00, 11, 10),
                new ScheduleEntry("532", "Озерна", "Центральна", 11, 20, 11, 30),
                
                new ScheduleEntry("533", "Центральна", "Озерна", 12, 00, 12, 10),
                new ScheduleEntry("534", "Озерна", "Центральна", 12, 20, 12, 30),
                
                new ScheduleEntry("535", "Центральна", "Озерна", 13, 00, 13, 10, true),
                new ScheduleEntry("536", "Озерна", "Центральна", 13, 20, 13, 30, true),
                
                new ScheduleEntry("537", "Центральна", "Озерна", 14, 00, 14, 10),
                new ScheduleEntry("538", "Озерна", "Центральна", 14, 20, 14, 30),
                
                new ScheduleEntry("539", "Центральна", "Озерна", 15, 00, 15, 10),
                new ScheduleEntry("540", "Озерна", "Центральна", 15, 20, 15, 30),
                
                new ScheduleEntry("541", "Центральна", "Озерна", 16, 00, 16, 10),
                new ScheduleEntry("542", "Озерна", "Центральна", 16, 20, 16, 30),
                
                new ScheduleEntry("543", "Центральна", "Озерна", 17, 00, 17, 10),
                new ScheduleEntry("544", "Озерна", "Центральна", 17, 20, 17, 30)
            };
        }

        public List<ScheduleEntry> GetAllTrains()
        {
            return _schedule;
        }

        public ScheduleEntry? GetNextTrain(TimeSpan currentTime, bool isWeekend)
        {
            return _schedule
                .Where(t => t.DepartureTime >= currentTime && (!t.IsWeekendOnly || isWeekend))
                .OrderBy(t => t.DepartureTime)
                .FirstOrDefault();
        }
    }
}