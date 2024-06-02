using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

namespace RewindProject
{
    public class RewindFrames<RewindableValue> : IRewindFrames
    {
        private delegate RewindableValue GetMethodDelegate(in int frameID);
        private GetMethodDelegate GetMethod;

        public bool _interpolateOnlySkippedFrames = true;
        private int _howManyFramesSkip;
        private float _lerpCoefficient;
        public int LastWrittenTickID
        {
            get => _lastWrittenTickID;
        }
        private int _lastWrittenTickID = -1;

        Dictionary<int, RewindableValue> frameShots = new Dictionary<int, RewindableValue>();
        SortedDictionary<int, RewindableValue> _unpackedFrames = new SortedDictionary<int, RewindableValue>();    
        public RewindFrames(ERewindInterpolationMethod rewindEventMethod = ERewindInterpolationMethod.None, int howManyFramesSkip = 0)
        {
            switch (rewindEventMethod)
            {
                case ERewindInterpolationMethod.None:
                    GetMethod = GetClosestLeft;
                    break;
                case ERewindInterpolationMethod.LerpSkipped:
                    GetMethod = GetLerp;
                    _interpolateOnlySkippedFrames = true;
                    break;
                case ERewindInterpolationMethod.SlerpSkipped:
                    GetMethod = GetSlerp;
                    _interpolateOnlySkippedFrames = true;
                    break;
                case ERewindInterpolationMethod.LerpAll:
                    GetMethod = GetLerp;
                    _interpolateOnlySkippedFrames = false;
                    break;
                case ERewindInterpolationMethod.SlerpAll:
                    GetMethod = GetSlerp;
                    _interpolateOnlySkippedFrames = false;
                    break;
                default:
                    GetMethod = GetClosestLeft;
                    break;
            }
        }

        /// <summary>
        /// ¬озвращает размер словар€ с запис€ми в байтах.
        /// </summary>
        public int GetDictSize()
        {
            int sizeofStruct = 4;
            switch (frameShots.First().Value)
            {
                case Vector3:
                    sizeofStruct = 12;
                    break;
                case Vector4:
                    sizeofStruct = 16;
                    break;
                case Quaternion:
                    sizeofStruct = 16;
                    break;
                case float:
                    sizeofStruct = 4;
                    break;
                case double:
                    sizeofStruct = 8;
                    break;
                case int:
                    sizeofStruct = 4;
                    break;
                case bool:
                    sizeofStruct = 1;
                    break;
                case string:
                    var len = 0;
                    //foreach (var value in frameShots.Values as string[])
                    //{
                    //    len += value.Length;
                    //}
                    return len * 2;
                case char:
                    sizeofStruct = 2;
                    break;
                default:
                    Debug.Log("Unknown sizeofStruct" + frameShots.First().Value.GetType());
                    break;

            }
            return frameShots.Count * (4 + sizeofStruct);
        }
        public object GetLastShot()
        {
            return frameShots.Last().Value;
        }
        public object Get(in int frameID)
        {
            return GetMethod.Invoke(frameID);
        }
        public void Add(in int frameID, object value)
        {
            if (frameShots.ContainsKey(frameID)) frameShots[frameID] = (RewindableValue)value;
            else frameShots.Add(frameID, (RewindableValue)value);
            _lastWrittenTickID = frameID;
        }
        public void RemoveAllAfterFrame(in int frameID, object frameValue)
        {
            var temp = frameID;
            frameShots = frameShots.Where(pair => pair.Key < temp)
                                 .ToDictionary(pair => pair.Key,
                                               pair => pair.Value);
            Add(frameID, frameValue);
            _unpackedFrames.Clear();
        }
        private RewindableValue GetLerp(in int frameID)
        {
            if (frameShots.ContainsKey(frameID)) return frameShots[frameID];
            if (isUnpacked(frameID)) return _unpackedFrames[frameID];

            int lastKnownID = GetClosestRightID(frameID);
            if (lastKnownID == 0)
            {
                lastKnownID = GetClosestLeftID(frameID);
            }
            var lastBeforeID = GetClosestLeftID(lastKnownID - 1);
            if (_interpolateOnlySkippedFrames && lastKnownID - lastBeforeID - 1 > _howManyFramesSkip) return frameShots[lastBeforeID];
            if (_interpolateOnlySkippedFrames && _lerpCoefficient.IsUnityNull()) _lerpCoefficient = 1f / (lastKnownID - lastBeforeID - 1);
            else _lerpCoefficient = 1f / (lastKnownID - lastBeforeID - 1);
            //Debug.Log($"Frames Unpacking ID{frameID} -> {lastBeforeID}...<---->...{lastKnownID}");
            _unpackedFrames.Clear();
            switch (frameShots[lastKnownID])
            {
                case Vector3 value:
                    Vector3 v3_tempLastKnown = value;
                    Vector3 v3_lastBeforeValue = frameShots[lastBeforeID].ConvertTo<Vector3>();
                    for (int i = frameID; i > lastKnownID; i--)
                    {
                        _unpackedFrames.Add(i, value.ConvertTo<RewindableValue>());
                    }
                    ushort v3_iterator = 1;
                    for (int i = lastKnownID - 1; i > lastBeforeID; i--)
                    {
                        var x = _lerpCoefficient * v3_iterator;
                        v3_tempLastKnown = Vector3.Lerp(v3_tempLastKnown, v3_lastBeforeValue, _lerpCoefficient * v3_iterator);
                        _unpackedFrames.Add(i, v3_tempLastKnown.ConvertTo<RewindableValue>());
                        v3_iterator++;
                    }
                    return (RewindableValue)Convert.ChangeType(value, typeof(RewindableValue));

                case Quaternion value:
                    Quaternion quaternion_tempLastKnown = value;
                    Quaternion quaternion_lastBeforeValue = frameShots[lastBeforeID].ConvertTo<Quaternion>();
                    for (int i = frameID; i > lastKnownID; i--)
                    {
                        _unpackedFrames.Add(i, value.ConvertTo<RewindableValue>());
                    }
                    ushort quaternion_iterator = 1;
                    for (int i = lastKnownID; i > lastBeforeID; i--)
                    {
                        quaternion_tempLastKnown = Quaternion.Lerp(quaternion_tempLastKnown, quaternion_lastBeforeValue, _lerpCoefficient * quaternion_iterator);
                        _unpackedFrames.Add(i, quaternion_tempLastKnown.ConvertTo<RewindableValue>());
                        quaternion_iterator++;
                    }
                    return (RewindableValue)Convert.ChangeType(value, typeof(RewindableValue));

                case int value:
                    float int_tempLastKnown = (float)value;
                    float int_lastBeforeValue = frameShots[lastBeforeID].ConvertTo<float>();
                    _lerpCoefficient *= (int_lastBeforeValue - int_tempLastKnown);
                    for (int i = frameID; i > lastKnownID; i--)
                    {
                        _unpackedFrames.Add(i, value.ConvertTo<RewindableValue>());
                    }
                    for (int i = lastKnownID; i > lastBeforeID; i--)
                    {
                        int_tempLastKnown += _lerpCoefficient; 
                        _unpackedFrames.Add(i, Mathf.RoundToInt(int_tempLastKnown).ConvertTo<RewindableValue>());
                    }
                    return (RewindableValue)Convert.ChangeType(value, typeof(RewindableValue));

                case float value:
                    float float_tempLastKnown = (float)value;
                    float float_lastBeforeValue = frameShots[lastBeforeID].ConvertTo<float>();
                    _lerpCoefficient *= (float_lastBeforeValue - float_tempLastKnown);
                    for (int i = frameID; i > lastKnownID; i--)
                    {
                        _unpackedFrames.Add(i, value.ConvertTo<RewindableValue>());
                    }
                    for (int i = lastKnownID; i > lastBeforeID; i--)
                    {
                        float_tempLastKnown += _lerpCoefficient;
                        _unpackedFrames.Add(i, float_tempLastKnown.ConvertTo<RewindableValue>());
                    }
                    return (RewindableValue)Convert.ChangeType(value, typeof(RewindableValue));

                case double value:
                    double double_tempLastKnown = value;
                    double double_lastBeforeValue = frameShots[lastBeforeID].ConvertTo<double>();
                    _lerpCoefficient *= (float)(double_lastBeforeValue - double_tempLastKnown);
                    for (int i = frameID; i > lastKnownID; i--)
                    {
                        _unpackedFrames.Add(i, value.ConvertTo<RewindableValue>());
                    }
                    for (int i = lastKnownID; i > lastBeforeID; i--)
                    {
                        double_tempLastKnown += _lerpCoefficient;
                        _unpackedFrames.Add(i, double_tempLastKnown.ConvertTo<RewindableValue>());
                    }
                    return (RewindableValue)Convert.ChangeType(value, typeof(RewindableValue));

                default:
                    Debug.Log("UNKNOWN TYPE TRYING LERP!");
                    return frameShots[lastKnownID];
            }
        }
        private RewindableValue GetSlerp(in int frameID)
        {
            return GetClosestLeft(frameID);
        }
        private bool isUnpacked(in int frameID)
        {
            if (_unpackedFrames.ContainsKey(frameID))
            {
                return true;
            }
            return false;
        }
        private RewindableValue GetClosestLeft(in int frameID)
        {
            if (frameShots.ContainsKey(frameID)) { return frameShots[frameID]; }
            return frameShots[GetClosestLeftID(frameID)];
        }
        private int GetClosestRightID(in int frameID)
        {
            foreach (var key in frameShots.Keys)
            {
                if (key > frameID) return key;
            }
            return 0;
        }
        private int GetClosestLeftID(in int frameID)
        {
            int tempFrame = 0;
            foreach (var key in frameShots.Keys)
            {
                if (key > frameID)
                {
                    //returning left frame (timeline)
                    return tempFrame;
                }
                tempFrame = key;
            }
            return tempFrame;
        }

    }
}
