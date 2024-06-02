using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace RewindProject
{    
    public abstract class RewindNotifier
    {
        public bool IsPaused => _isPaused;
        private bool _isPaused = false;

        public void Start() { _isPaused = false; }
        public void Pause() { _isPaused = true; }

        public event PropertyChangedEventHandler RewindablePropertyChanged;
        protected void NotifyRewindablePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (IsPaused && RewindablePropertyChanged != null)
            {
                RewindablePropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}