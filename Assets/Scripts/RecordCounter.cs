using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RewindProject
{
    public class RecordCounter : MonoBehaviour
    {
        public event Action RecordingStarted;
        public event Action RecordingPaused;

        public event Action<int, int> CurrentFrameIDChanged;
        public event Action<int> MinFrameIDChanged;

        public bool IsPaused { get => _isPaused; }
        public int CurrentFrameID
        {
            get => _currentFrameID;
            set
            {
                _currentFrameID = value >= 0 ? value : 0;
                _maxFrameID = _currentFrameID > _maxFrameID ? _currentFrameID : _maxFrameID;
                CurrentFrameIDChanged?.Invoke(_currentFrameID, _maxFrameID);
            }
        }
        private int _currentFrameID = 0;
        public int MaxFrameID
        {
            get => _maxFrameID;
            set => _maxFrameID = value > CurrentFrameID ? value : CurrentFrameID;
        }
        private int _maxFrameID = 0;
        public int MinFrameID
        {
            get => _minFrameID;
            set
            {
                _minFrameID = value > CurrentFrameID ? CurrentFrameID : value;
                MinFrameIDChanged?.Invoke(_minFrameID);
            }
        }
        private int _minFrameID = 0;

        private bool _isPaused = false;

        public void Continue() { _isPaused = false; RecordingStarted?.Invoke(); }
        public void Pause() { _isPaused = true; RecordingPaused?.Invoke(); }
        public void Clear() { _currentFrameID = 0; _maxFrameID = 0; _minFrameID = 0; }

        private void Awake()
        {
            _isPaused = true;
            _minFrameID = 0;
            _currentFrameID = 0;
            _maxFrameID = 0;
        }
        void FixedUpdate()
        {
            if (!_isPaused)
            {
                _maxFrameID++;
                CurrentFrameID++;
            }
        }
    }
}
