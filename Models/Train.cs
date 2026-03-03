using DncApp.Interfaces;

namespace DncApp.Models
{
    public class Train : RailwayObject
    {
        public string TrainNumber { get; set; }
        public string LocomotiveType { get; set; }
        public double LocomotiveWeight { get; set; }
        public string CarType { get; set; }
        public int CarCount { get; set; }
        public double CarWeight { get; set; }
        public string DriverName { get; set; }
        public double CurrentSpeed { get; set; }
        public int BrakePosition { get; private set; }

        public double TotalWeight => LocomotiveWeight + (CarCount * CarWeight);

        public Train(string id, string trainNumber, string driverName) : base(id)
        {
            TrainNumber = trainNumber;
            LocomotiveType = "ТУ2";
            LocomotiveWeight = 32.0;
            CarType = "ПВ-40";
            CarCount = 3;
            CarWeight = 9.5;
            DriverName = driverName;
            BrakePosition = 2;
        }

        public void ApplyBrakes(int steps)
        {
            BrakePosition += steps;
            if (BrakePosition > 7)
            {
                BrakePosition = 7;
            }
        }

        public void ReleaseBrakes()
        {
            BrakePosition = 2;
        }

        public override string GetStatus() 
            => $"Поїзд №{TrainNumber} ({LocomotiveType}), Машиніст: {DriverName}\n" +
               $"Склад: {CarCount} вагонів {CarType}\n" +
               $"Вага локомотива: {LocomotiveWeight} т, Вага вагонів: {CarCount * CarWeight} т (Загальна: {TotalWeight} т)\n" +
               $"Швидкість: {CurrentSpeed} км/год";

        public override void ResetToSafeState()
        {
            ApplyBrakes(7);
            CurrentSpeed = 0;
        }
    }
}