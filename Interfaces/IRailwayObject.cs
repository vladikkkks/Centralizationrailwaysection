using System;
using System.ComponentModel;

namespace DncApp.Interfaces
{
    public interface IRailwayObject : INotifyPropertyChanged
    {
        string Id { get; }
        bool IsOccupied { get; set; }
        bool IsLocked { get; set; }
        bool IsOperational { get; set; }
        DateTime LastUpdated { get; }
        bool CanPerformAction();
        string GetStatus();
        void ResetToSafeState();
    }
}