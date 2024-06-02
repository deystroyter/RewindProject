using RewindProject;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using Unity.VisualScripting;
using UnityEngine;

namespace RewindProject
{
    public abstract class ARewindableComponent : MonoBehaviour, IRewindableComponent
    {
        public RecordCounter RecordCounter { get => _recordCounter; set => _recordCounter = value; }
        private RecordCounter _recordCounter;

        protected int CurrentFrameID => RecordCounter.CurrentFrameID;
        protected bool IsPaused => RecordCounter.IsPaused;


        public virtual bool IsMainThreadComponent => false;
        [SerializeField] protected bool TrackEventBased = true;
        [SerializeField] protected ERewindInterpolationMethod InterpolationMethod = ERewindInterpolationMethod.None;

        public bool SkipFrames = false;
        public int HowManyFramesSkip = 4;
        protected int _LastWrittenFrameID = 0;

        protected UnityEngine.Component _Component;
        protected Dictionary<PropertyInfo, IRewindFrames> _TrackingProps = new Dictionary<PropertyInfo, IRewindFrames>();

        protected abstract void Init();
        protected virtual void EventTick()
        {
            if (SkipFrames && CurrentFrameID < _LastWrittenFrameID + HowManyFramesSkip) return;
            foreach (var prop in _TrackingProps.Keys)
            {
                _TrackingProps[prop].Add(CurrentFrameID, prop.GetValue(_Component));
            }
            _LastWrittenFrameID = CurrentFrameID;
        }
        public virtual void Tick(in int frameID)
        {
            if (TrackEventBased) { return; }
            if (SkipFrames && frameID < _LastWrittenFrameID + HowManyFramesSkip) return;
            foreach (var prop in _TrackingProps.Keys)
            {
                _TrackingProps[prop].Add(frameID, prop.GetValue(_Component));
            }
            _LastWrittenFrameID = frameID;
        }
        public virtual void SetFrame(in int frameID)
        {
            foreach (var prop in _TrackingProps.Keys)
            {
                prop.SetValue(_Component, _TrackingProps[prop].Get(frameID));
            }
        }
        public virtual void ClearInfo(in int frameID)
        {
            foreach (var prop in _TrackingProps.Keys)
            {
                _TrackingProps[prop].RemoveAllAfterFrame(frameID, _TrackingProps[prop].Get(frameID));
            }

        }
        public virtual int GetSizeOfEventProps()
        {
            int sum = 0;
            foreach (var prop in _TrackingProps.Keys)
            {
                sum += _TrackingProps[prop].GetDictSize();
            }
            return sum;
        }
    }
}

