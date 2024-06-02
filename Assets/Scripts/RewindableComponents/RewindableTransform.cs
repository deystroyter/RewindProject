using System;
using UnityEngine;

namespace RewindProject
{
    [RequireComponent(typeof(Transform))]
    public sealed class RewindableTransform : ARewindableComponent 
    {
        public event Action TransformChanged;
        public override bool IsMainThreadComponent => true;
        [Header("Transform Tracking")]
        [SerializeField] private bool _trackScale = false;

        void Start()
        {
            _Component = this.transform;
            Init();
        }
        protected override void Init()
        {
            _TrackingProps.Add(typeof(Transform).GetProperty(nameof(_Component.transform.position)), new RewindFrames<Vector3>(InterpolationMethod, HowManyFramesSkip));
            _TrackingProps.Add(typeof(Transform).GetProperty(nameof(_Component.transform.rotation)), new RewindFrames<Quaternion>(InterpolationMethod, HowManyFramesSkip));
            if (_trackScale) _TrackingProps.Add(typeof(Transform).GetProperty(nameof(_Component.transform.localScale)), new RewindFrames<Vector3>(InterpolationMethod, HowManyFramesSkip));
            foreach (var prop in _TrackingProps.Keys)
            {
                _TrackingProps[prop].Add(0, prop.GetValue(_Component));
            }
        }

        void FixedUpdate()
        {
            if (TrackEventBased && transform.hasChanged)
            {
                if (IsPaused) return;
                EventTick();
            }
        }
        protected override void EventTick()
        {
            TransformChanged?.Invoke();
            base.EventTick();
            transform.hasChanged = false;
        }
    }

}