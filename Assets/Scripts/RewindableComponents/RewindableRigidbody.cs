using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using TMPro;
using UnityEngine;

namespace RewindProject
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(Collider))]
    [RequireComponent(typeof(RewindObject))]
    public sealed class RewindableRigidbody : ARewindableComponent
    {
        public override bool IsMainThreadComponent => true;
        private Rigidbody _rigidbody;

        private bool _rewindableTransformAttached;

        private int _lastSettedFrameID = 0;
        private bool _isWaitingToSet = true;

        void Start()
        {
            _rigidbody = GetComponent<Rigidbody>();
            _Component = _rigidbody;
            _rewindableTransformAttached = TryGetComponent<RewindableTransform>(out var rewindableTranform);
            if (_rewindableTransformAttached) rewindableTranform.TransformChanged += EventTick;
            Init();
        }
        protected override void Init()
        {
            _TrackingProps.Add(typeof(Rigidbody).GetProperty(nameof(_rigidbody.velocity)), new RewindFrames<Vector3>(InterpolationMethod, HowManyFramesSkip));
            _TrackingProps.Add(typeof(Rigidbody).GetProperty(nameof(_rigidbody.angularVelocity)), new RewindFrames<Vector3>(InterpolationMethod, HowManyFramesSkip));
            foreach (var prop in _TrackingProps.Keys)
            {
                _TrackingProps[prop].Add(0, prop.GetValue(_Component));
            }
        }

        private void Update()
        {
            if (_isWaitingToSet && !_rigidbody.isKinematic)
            {
                base.SetFrame(_lastSettedFrameID);
                _isWaitingToSet = false;
            }
        }
        void FixedUpdate()
        {
            if (IsPaused) return;
            if (!_rewindableTransformAttached && TrackEventBased && transform.hasChanged)
            {
                EventTick();
            }
        }

        protected override void EventTick()
        {
            base.EventTick();
            if (!_rewindableTransformAttached) transform.hasChanged = false;
        }

        public override void SetFrame(in int frameID)
        {
            if (_rigidbody.isKinematic)
            {
                _lastSettedFrameID = frameID;
                _isWaitingToSet = true;
            }
            else
            {
                base.SetFrame(frameID);
            }
        }


    }
}

