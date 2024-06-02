using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using UnityEngine;

namespace RewindProject
{
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(RewindObject))]
    public sealed class RewindableAnimator : ARewindableComponent
    {
        private struct AnimatorRecord
        {
            public int ShortNameHash;
            public float NormalizedTime;
            public AnimatorRecord(int shortNameHash, float normalizedTime)
            {
                ShortNameHash = shortNameHash;
                NormalizedTime = normalizedTime;
            }

        }
        private Animator _animator;
        private Dictionary<int, RewindFrames<AnimatorRecord>> _trackingProps = new Dictionary<int, RewindFrames<AnimatorRecord>>();

        void Awake()
        {
            if (!gameObject.TryGetComponent(out _animator))
            {
                Debug.LogError("Animator component not found.");
            }
            Init();
        }
        protected override void Init()
        {
            for (int i = 0; i < _animator.layerCount; i++)
            {
                AnimatorStateInfo animatorStateInfo = _animator.GetCurrentAnimatorStateInfo(i);
                _trackingProps.Add(i, new RewindFrames<AnimatorRecord>(InterpolationMethod, HowManyFramesSkip));
            }
        }

        public override void Tick(in int frameID)
        {
            foreach (var layer in _trackingProps.Keys)
            {
                _trackingProps[layer].Add(frameID, _animator.GetCurrentAnimatorStateInfo(layer));
            }
        }

        public override void SetFrame(in int frameID)
        {
            foreach (var layer in _trackingProps.Keys)
            {
                var record = (AnimatorRecord)_trackingProps[layer].Get(frameID);
                _animator.Play(record.ShortNameHash, layer, record.NormalizedTime);
            }
        }
        public override void ClearInfo(in int frameID)
        {
            foreach (var layer in _trackingProps.Keys)
            {
                _trackingProps[layer].RemoveAllAfterFrame(frameID, _animator.GetCurrentAnimatorStateInfo(layer));
            }
        }
        public override int GetSizeOfEventProps()
        {
            var count = 0;
            foreach (var rewindFrames in _trackingProps.Values)
            {
                count += rewindFrames.GetDictSize();
            }
            return count * 8;
        }
    }
}