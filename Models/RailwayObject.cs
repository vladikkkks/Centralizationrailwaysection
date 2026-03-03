using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using DncApp.Interfaces;

namespace DncApp.Models
{
    public abstract class RailwayObject : IRailwayObject
    {
        private bool _isOccupied;
        private bool _isLocked;
        private bool _isOperational = true;

        public string Id { get; protected set; }
        public DateTime LastUpdated { get; private set; }

        public bool IsOccupied 
        { 
            get => _isOccupied; 
            set { SetProperty(ref _isOccupied, value); UpdateTimestamp(); }
        }

        public bool IsLocked 
        { 
            get => _isLocked; 
            set { SetProperty(ref _isLocked, value); UpdateTimestamp(); }
        }

        public bool IsOperational 
        { 
            get => _isOperational; 
            set { SetProperty(ref _isOperational, value); UpdateTimestamp(); }
        }

        protected RailwayObject(string id)
        {
            Id = id;
            UpdateTimestamp();
        }

        public virtual bool CanPerformAction() => IsOperational && !IsLocked && !IsOccupied;

        public abstract string GetStatus();
        public abstract void ResetToSafeState();

        private void UpdateTimestamp() => LastUpdated = DateTime.Now;

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void SetProperty<T>(ref T storage, T value, [CallerMemberName] string? propertyName = null)
        {
            if (Equals(storage, value)) return;
            storage = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}