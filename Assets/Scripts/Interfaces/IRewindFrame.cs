using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace RewindProject
{
    public interface IRewindFrames
    {
        int LastWrittenTickID{ get; }
        int GetDictSize();
        object Get(in int frameID);
        object GetLastShot();
        void Add(in int frameID, object value);
        void RemoveAllAfterFrame(in int frameID, object frameValue);
    }
}
