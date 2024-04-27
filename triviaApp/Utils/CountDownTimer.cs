using System;
namespace triviaApp.Utils
{
    public static class CountdownTimer
    {
        private static Timer _timer;
        private static int _seconds;
        private static bool _timerActive = false;
        private static bool _timerEnd = false;

        public static void Start(int seconds)
        {
            if (_timerActive)
            {
                Dispose();
            }
            _seconds = seconds;
            _timer = new Timer(_ => TimerCallback(), null, 0, 1000);
            _timerActive = true;
            _timerEnd = false;
        }

        private static void TimerCallback()
        {
            if (_seconds > 0)
            {
                OnSecondPassed?.Invoke(_seconds);
                _seconds--;
            }
            else if(!_timerEnd)
            {
                _timerEnd = true;

                OnTimerCompleted?.Invoke();
                Dispose();
            }
        }

        public static void Dispose()
        {
            if (_timer != null)
            {
                _timer.Dispose();
                _timer = null;
                _timerActive = false;
            }
        }

        public static event Action<int> OnSecondPassed;
        public static event Action OnTimerCompleted;
    }
}

