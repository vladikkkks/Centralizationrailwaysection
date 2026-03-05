using System;
using System.Collections.Generic;
using System.Linq;
using DncApp.Models;

namespace DncApp.Services
{
    public interface IRouteStrategy
    {
        bool ValidateAndLock(List<TrackSection> tracks, List<Switch> switches);
    }

    public class TrainRouteStrategy : IRouteStrategy
    {
        public bool ValidateAndLock(List<TrackSection> tracks, List<Switch> switches)
        {
            if (tracks.Any(t => t.IsOccupied)) return false;

            foreach (var sw in switches) sw.IsLocked = true;
            return true;
        }
    }

    public class ShuntingRouteStrategy : IRouteStrategy
    {
        public bool ValidateAndLock(List<TrackSection> tracks, List<Switch> switches)
        {
            foreach (var sw in switches) sw.IsLocked = true;
            return true;
        }
    }

    public class InterlockingEngine
    {
        private IRouteStrategy? _strategy;

        public void SetStrategy(IRouteStrategy strategy)
        {
            _strategy = strategy;
        }

        public bool TryBuildRoute(List<TrackSection> tracks, List<Switch> switches, Signal routeSignal)
        {
            if (_strategy == null) return false;

            bool isSafe = _strategy.ValidateAndLock(tracks, switches);
            
            if (isSafe)
            {
                routeSignal.CurrentColor = SignalColor.Green; 
                return true;
            }
            return false;
        }
    }
}