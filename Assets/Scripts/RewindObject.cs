using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Unity.VisualScripting;
using UnityEngine;

namespace RewindProject
{
    public class RewindObject : RewindNotifier_MonoBehaviour, IRewindObject
    {
        public IRewindController Controller { get => _controllerAbstract;}
        [SerializeField] private ARewindController _controllerAbstract;
        public ARewindController ControllerAbstract
        {
            get => _controllerAbstract;
            set { _controllerAbstract = value; _controllerAbstract.AddRewindObject(this); }
        }

        [SerializeField] public bool TrackActiveState = true;

        //May work unstable (trying to track all fields and properties in component).
        [SerializeField] public List<UnityEngine.Component> CustomComponents;

        private Rigidbody _rigidbody;
        private Collider[] _colliders;
        private bool _isUsingPhysics;

        private List<IRewindableComponent> Components = new List<IRewindableComponent>();
        private List<IRewindableComponent> MainThreadComponents = new List<IRewindableComponent>();

        #region ActiveStateTrack
        [RewindableProperty]
        private bool _activeState
        {
            get => gameObject.activeSelf;
            set
            {
                gameObject.SetActive(value);
                if (!Controller.IsPaused)
                    NotifyRewindablePropertyChanged();
            }
        }

        private void OnEnable()
        {
            if (!TrackActiveState) return;
            if (Controller.IsPaused) return;
            _activeState = true;
        }
        private void OnDisable()
        {
            if (!TrackActiveState) return;
            if (Controller.IsPaused) return;
            _activeState = false;
        }
        #endregion

        private void Awake()
        {
            if (ControllerAbstract == null) ControllerAbstract = SceneRewindController.Instance;
            else _controllerAbstract.AddRewindObject(this);
            FindRewindables();
        }

        void Start()
        {
            _isUsingPhysics = gameObject.TryGetComponent(out _rigidbody);
            _colliders = gameObject.GetComponents<Collider>();
        }

        public int GetSizeOfEventProps()
        {
            int sum = 0;
            foreach (var component in Components)
            {
                sum += component.GetSizeOfEventProps();
            }
            foreach (var component in MainThreadComponents)
            {
                sum += component.GetSizeOfEventProps();
            }
            return sum;
        }
        public void DisablePhysics()
        {
            ChangePhysicsStateTo(false);
        }
        public void EnablePhysics()
        {
            ChangePhysicsStateTo(true);
        }
        private void ChangePhysicsStateTo(bool isActive)
        {
            if (_isUsingPhysics) 
            {
                _rigidbody.isKinematic = !isActive; 
                foreach (var collider in _colliders)
                {
                    collider.enabled = isActive;
                }
            }
        }
        public void Tick(in int tickID)
        {
            foreach (var component in Components)
            {
                component.Tick(tickID);
            }
        }
        public void SetFrameToMainThreadComponents(in int frameID)
        {
            foreach (var component in MainThreadComponents)
            {
                component.SetFrame(frameID);
            }
        } 
        public void SetFrame(in int frameID)
        {
            foreach (var component in Components)
            {
                component.SetFrame(frameID);
            }
        }
        public void ClearInfo(in int frameID)
        {
            foreach(var component in Components)
            {
                component.ClearInfo(frameID);
            }
        }

        /// <summary>
        /// Поиск всех Rewindable-компонентов, прикреплённых к объекту.
        /// </summary>
        private void FindRewindables()
        { 
            //Finding all other components with RewindAttributes
            var components = this.gameObject.GetComponents(typeof(UnityEngine.Component));
            //Debug.Log("Len =" + components.Length);
            foreach (UnityEngine.Component component in components)
            {
                //If we found a IRewindableComponent component
                if (component is IRewindableComponent)
                {
                    var rewidableComponent = (component as IRewindableComponent);
                    rewidableComponent.RecordCounter = Controller.RecordCounter;
                    if (rewidableComponent.IsMainThreadComponent)
                    {
                        MainThreadComponents.Add(rewidableComponent);
                    }
                    else
                    {
                        Components.Add(rewidableComponent);
                    }
                    continue;
                }

                //Finding State-based properties
                var stateProperties = component.GetType().GetProperties()
                    .Where(p => p.GetCustomAttributes(typeof(RewindStateBasedAttribute), false).Length > 0 && p.CanWrite);
                //Finding State-based fields
                var stateFields = component.GetType().GetFields()
                    .Where(p => p.GetCustomAttributes(typeof(RewindStateBasedAttribute), false).Length > 0 && p.CanWrite());
                //Finding Event-based properties
                var eventProperties = component.GetType().GetProperties()
                    .Where(p => p.GetCustomAttributes(typeof(RewindablePropertyAttribute), false).Length > 0 && p.CanWrite);

                if (stateProperties.Count() > 0 || stateFields.Count() > 0 || eventProperties.Count() > 0)
                {
                    var newRC = new RewindableComponent(component, ref stateProperties, ref stateFields, ref eventProperties);
                    newRC.RecordCounter = Controller.RecordCounter;
                    Components.Add(newRC);
                }
                //Adding Fields and Properties from CustomComponents
                else if (CustomComponents.Any(comp => comp.Equals(component)))
                {
                    var ccFields = component.GetType().GetFields().Where(p => p.CanWrite()).AsEnumerable();
                    var ccProperties = component.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public).Where(p => p.CanWrite).AsEnumerable();
                    var newRC = new RewindableComponent(component, ref ccProperties, ref ccFields);
                    newRC.RecordCounter = Controller.RecordCounter;
                    Components.Add(newRC);
                }
            }
           
        }

    }
}




