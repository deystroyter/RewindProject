using System.Collections.Generic;
using System.ComponentModel;
using Unity.VisualScripting;
using UnityEngine;

namespace RewindProject
{
    [RequireComponent(typeof(RewindObject))]
    public sealed class RewindableRig : ARewindableComponent
    {
        public override bool IsMainThreadComponent => true;

        [SerializeField] private Transform _rigRoot;
        private List<IRewindableComponent> _trackingTransforms = new List<IRewindableComponent>();

        void Awake()
        {
            if (_rigRoot == null )
            {
                Debug.LogError($"{gameObject.name} | RigRoot was not attached to RewindableRig.");
            }
            Init();
        }
        protected override void Init()
        {
            _trackingTransforms.Add(_rigRoot.AddComponent<RewindableTransform>());
            AddChildTransforms(_rigRoot);
            foreach ( var transform in _trackingTransforms ) transform.RecordCounter = RecordCounter;
        }
        private void AddChildTransforms(Transform parent)
        {
            foreach (Transform child in parent)
            {
                _trackingTransforms.Add(child.AddComponent<RewindableTransform>());
                AddChildTransforms(child);
            }
        }
        public override void Tick(in int tickID)
        {
            foreach (var component in _trackingTransforms)
            {
                component.Tick(tickID);
            }
        }
        public override void SetFrame(in int frameID)
        {
            foreach (var component in _trackingTransforms)
            {
                component.SetFrame(frameID);    
            }
        }
        public override void ClearInfo(in int frameID)
        {
            foreach (var component in _trackingTransforms)
            {
                component.ClearInfo(frameID);
            }
        }
        public override int GetSizeOfEventProps()
        {
            int sum = 0;
            foreach (var component in _trackingTransforms)
            {
                sum += component.GetSizeOfEventProps();
            }
            return sum;
        }
    }
}