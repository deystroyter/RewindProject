using RewindProject;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace RewindProject
{
    public sealed class RewindableComponent : IRewindableComponent
    {
        public bool IsMainThreadComponent => false;
        public RecordCounter RecordCounter { get => _recordCounter; set => _recordCounter = value; }
        private RecordCounter _recordCounter;
        private int _currentFrameID => RecordCounter.CurrentFrameID;

        private UnityEngine.Component _component; //Parent RewindObject
        private Dictionary<PropertyInfo, IRewindFrames> _stateProps = new Dictionary<PropertyInfo, IRewindFrames>();
        private Dictionary<FieldInfo, IRewindFrames> _stateFields = new Dictionary<FieldInfo, IRewindFrames>();
        private Dictionary<PropertyInfo, IRewindFrames> _eventProps = new Dictionary<PropertyInfo, IRewindFrames>();

        private bool _isNeedToTickStateProps = false;
        private bool _isNeedToTickStateFields = false;

        public RewindableComponent(UnityEngine.Component component, ref IEnumerable<PropertyInfo> eventProps)
        {
            _component = component;
            InitEventProps(ref eventProps);
        }       
        public RewindableComponent(UnityEngine.Component component, ref IEnumerable<PropertyInfo> stateProps, ref IEnumerable<FieldInfo> stateFields)
        {
            _component = component;
            InitStatePropsAndFields(ref stateProps, ref stateFields);
        }
        public RewindableComponent(UnityEngine.Component component, ref IEnumerable<PropertyInfo> stateProps, ref IEnumerable<FieldInfo> stateFields, ref IEnumerable<PropertyInfo> eventProps)
        {
            _component = component;
            InitStatePropsAndFields(ref stateProps, ref stateFields);
            InitEventProps(ref eventProps);
        }
        private void InitStatePropsAndFields(ref IEnumerable<PropertyInfo> stateProps, ref IEnumerable<FieldInfo> stateFields)
        {
            foreach (FieldInfo fInfo in stateFields)
            {
                this._stateFields.Add(fInfo, CreateRewindFrames(fInfo.GetValue(_component).GetType()));
                _stateFields[fInfo].Add(0, fInfo.GetValue(_component));
            }
            foreach (PropertyInfo pInfo in stateProps)
            {
                try
                {
                    this._stateProps.Add(pInfo, CreateRewindFrames(pInfo.GetValue(_component).GetType()));
                    _stateProps[pInfo].Add(0, pInfo.GetValue(_component));
                }
                catch (NullReferenceException e)
                {
                    Debug.LogWarning($"{pInfo.Name} property can't be added to RewindList :(.{e.Message}");
                }
                catch
                {
                    continue;
                }
            }
            if (stateProps.Count() > 0) _isNeedToTickStateProps = true;
            if (stateFields.Count() > 0) _isNeedToTickStateFields = true;
        }
        private void InitEventProps(ref IEnumerable<PropertyInfo> eventProps)
        {
            if (_component.GetType().BaseType == typeof(RewindNotifier_MonoBehaviour))
            {
                var rewindBaseClass = (RewindNotifier_MonoBehaviour)_component;
                rewindBaseClass.RewindablePropertyChanged += EventTick;
            }
            else if (_component.GetType().BaseType == typeof(RewindNotifier))
            {
                //var rewindBaseClass = (RewindNotifier)_Component;
                //rewindBaseClass.RewindablePropertyChanged += EventTick;
            }
            foreach (PropertyInfo pInfo in eventProps)
            {
                this._eventProps.Add(pInfo, CreateRewindFrames(pInfo.GetValue(_component).GetType()));
                this._eventProps[pInfo].Add(0, pInfo.GetValue(_component));
            }
        }

        //StateBased Tick
        public void Tick(in int frameID)
        {
            if (_isNeedToTickStateProps) 
            {
                foreach (var prop in _stateProps.Keys)
                {
                    _stateProps[prop].Add(frameID, prop.GetValue(_component));
                }
            }
            if (_isNeedToTickStateFields) 
            {
                 foreach (var field in _stateFields.Keys)
                 {
                    _stateFields[field].Add(frameID, field.GetValue(_component));
                 }
            }
        }
        
        //EventBased Tick
        private void EventTick(object sender, PropertyChangedEventArgs args)
        {
            var x = _eventProps.Keys.Single(prop => prop.Name == args.PropertyName);
            _eventProps[x].Add(_currentFrameID, x.GetValue(_component));
        }
        public void SetFrame(in int frameID)
        {
            foreach (var prop in _stateProps.Keys)
            {
                prop.SetValue(_component, _stateProps[prop].Get(frameID));
            }
            foreach (var field in _stateFields.Keys)
            {
                field.SetValue(_component, _stateFields[field].Get(frameID));
            }
            foreach (var prop in _eventProps.Keys)
            {
                prop.SetValue(_component, _eventProps[prop].Get(frameID));
            }
        }
        public void ClearInfo(in int frameID)
        {
            foreach (var field in _stateFields.Keys)
            {
                _stateFields[field].RemoveAllAfterFrame(frameID, field.GetValue(_component));
            }
            foreach (var prop in _stateProps.Keys)
            {
                _stateProps[prop].RemoveAllAfterFrame(frameID, prop.GetValue(_component));
            }
            foreach (var prop in _eventProps.Keys)
            {
                _eventProps[prop].RemoveAllAfterFrame(frameID, prop.GetValue(_component));
            }
        }

        public int GetSizeOfEventProps()
        {
            int sum = 0;
            foreach(var prop in _eventProps.Keys)
            {
                sum += _eventProps[prop].GetDictSize();
            }
            return sum;
        }

        private static IRewindFrames CreateRewindFrames(Type type)
        {
            Type genericListType = typeof(RewindFrames<>).MakeGenericType(type);
            return (IRewindFrames)Activator.CreateInstance(genericListType, ERewindInterpolationMethod.None, 0);
        }
    }
}
