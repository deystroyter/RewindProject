using System.ComponentModel;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace RewindProject
{
    public abstract class RewindNotifier_MonoBehaviour : MonoBehaviour
    {
        //[SerializeField] public IRewindController Controller;
        public event PropertyChangedEventHandler RewindablePropertyChanged;
        protected void NotifyRewindablePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (RewindablePropertyChanged != null) //!Controller.IsPaused && 
            {
                RewindablePropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
