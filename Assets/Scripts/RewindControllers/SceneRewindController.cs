using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using System.Diagnostics;

namespace RewindProject
{
    public sealed class SceneRewindController : ARewindController
    {   
        public static SceneRewindController Instance;

        [SerializeField] public float HowManySecondsToRecord = 10f;
        private float _recordTimer = 0f;
        public int SizeCounter = 0;

        protected override void Awake()
        {
            Instance = this;
            base.Awake();
        }

        private void OnDestroy()
        {
            UnityEngine.Debug.Log($"Size of event-based: {Math.Round(SizeCounter/1000000f, 3)} MB");
            Instance = null;
        }
        private void Update()
        {
            if (Input.GetKeyUp(KeyCode.P)) 
            {
                if (RecordCounter.IsPaused) StartRecording();
                else PauseRecording();
            }
            if (Input.GetKeyDown(KeyCode.Space)) PauseRecording();
            if (Input.GetKeyUp(KeyCode.Space)) StartRecording();
        }

        protected override void FixedUpdate()
        {
            if (Input.GetKey(KeyCode.Space) && RecordCounter.IsPaused)
            {
                SetFrame(--RecordCounter.CurrentFrameID);
            }
            if (RecordCounter.IsPaused) return;

            if (IsPaused) return;
            else if (_skippedTicksCounter < HowManyTicksSkip)
            {
                _skippedTicksCounter++;
                return;
            }
            else
            {
                Tick();
            }
            //Testing
            _recordTimer += Time.deltaTime;
            if (_recordTimer > HowManySecondsToRecord)
            {
                UnityEngine.Debug.Log($"Recorded {_recordTimer} seconds.");
                PauseRecording();
            }
        }
    }
}

