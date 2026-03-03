using DncApp.Interfaces;

namespace DncApp.Models
{
    public enum SignalColor { Red, Yellow, Green, White }

    public class Signal : RailwayObject
    {
        private SignalColor _currentColor;

        public SignalColor CurrentColor
        {
            get => _currentColor;
            set => SetProperty(ref _currentColor, value);
        }

        public Signal(string id) : base(id)
        {
            CurrentColor = SignalColor.Red;
        }

        public void AutoClose()
        {
            CurrentColor = SignalColor.Red;
        }

        public override string GetStatus() => $"Світлофор {Id}: Колір {CurrentColor}";
        
        public override void ResetToSafeState() => CurrentColor = SignalColor.Red;
    }
}