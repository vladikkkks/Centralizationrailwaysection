using DncApp.Interfaces;
using DncApp.Enums;

namespace DncApp.Models
{
    public class Switch : RailwayObject
    {
        private SwitchPosition _currentPosition;
private string _description = "";
        public SwitchPosition CurrentPosition
        {
            get => _currentPosition;
            set => SetProperty(ref _currentPosition, value);
        }

        public string Description
        {
            get => _description;
            private set => SetProperty(ref _description, value);
        }

        public Switch(string id, string description) : base(id)
        {
            Description = description;
            CurrentPosition = SwitchPosition.Plus;
        }

        public bool Toggle()
        {
            if (!CanPerformAction())
            {
                return false; 
            }

            CurrentPosition = (CurrentPosition == SwitchPosition.Plus) 
                ? SwitchPosition.Minus 
                : SwitchPosition.Plus;
            
            return true;
        }

        public override bool CanPerformAction()
        {
            return base.CanPerformAction();
        }

        public override string GetStatus()
        {
            string pos = CurrentPosition == SwitchPosition.Plus ? "Плюс" : "Мінус";
            string state = IsLocked ? "Замкнена" : "Вільна";
            return $"Стрілка {Id} ({Description}): Положення: {pos}. Статус: {state}.";
        }

        public override void ResetToSafeState()
        {
            IsLocked = true;
        }
    }
}