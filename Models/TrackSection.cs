using System;
using DncApp.Interfaces;

namespace DncApp.Models
{
    public class TrackSection : RailwayObject
    {
        public event EventHandler<bool>? OccupancyChanged;

        public TrackSection(string id) : base(id) { }

        public new bool IsOccupied
        {
            get => base.IsOccupied;
            set
            {
                if (base.IsOccupied != value)
                {
                    base.IsOccupied = value;
                    OccupancyChanged?.Invoke(this, value);
                }
            }
        }

        public override string GetStatus() => $"Колійна секція {Id}: {(IsOccupied ? "Зайнята" : "Вільна")}";

        public override void ResetToSafeState()
        {
            IsOccupied = true;
        }
    }
}