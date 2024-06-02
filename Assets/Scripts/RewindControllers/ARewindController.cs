using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;

namespace RewindProject
{
    [RequireComponent(typeof(RecordCounter))]
    public abstract class ARewindController : MonoBehaviour, IRewindController
    {
        public RecordCounter RecordCounter => _recordCounter;
        [SerializeField] private RecordCounter _recordCounter;

        public bool SkipTicks = true;
        [Range(0, 49)] public int HowManyTicksSkip = 0;
        protected int _skippedTicksCounter = 0;

        public int CurrentFrameID
        {
            get => _recordCounter.CurrentFrameID;
        }
        public bool IsPaused
        {
            get => _recordCounter.IsPaused;
        }

        [SerializeField] protected List<IRewindObject> _rewindObjects = new List<IRewindObject>();

        protected virtual void Awake()
        {
            Debug.Log(TryGetComponent<RecordCounter>(out _recordCounter));
        }

        protected virtual void Start()
        {
            StartRecording();
        }

        protected virtual void FixedUpdate()
        {
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

        }
        public virtual void SetRewindObjects(List<IRewindObject> rewindObjects) 
        {
            _rewindObjects = rewindObjects;
        }
        public virtual void AddRewindObject(IRewindObject rewindObject)
        {
            _rewindObjects.Add(rewindObject);
        }
        public virtual void RemoveRewindObject(IRewindObject rewindObject)
        {
            _rewindObjects.Remove(rewindObject);
        }
        public virtual void StartRecording()
        {
            RecordCounter.MaxFrameID = _recordCounter.CurrentFrameID;
            Parallel.ForEach(_rewindObjects, obj => obj.ClearInfo(RecordCounter.CurrentFrameID));
            foreach (var obj in _rewindObjects)
            {
                obj.EnablePhysics();
            }
            RecordCounter.Continue();
            _skippedTicksCounter = HowManyTicksSkip;
        }
        public virtual void PauseRecording()
        {
            RecordCounter.Pause();
            UnityEngine.Debug.Log($"RewindManager paused! CurrentFrame={RecordCounter.CurrentFrameID} || MaxFrames={RecordCounter.MaxFrameID}");
            foreach (var obj in _rewindObjects)
            {
                obj.DisablePhysics();
            }
            int sizeofEventProps = 0;
            foreach (var obj in _rewindObjects)
            {
                sizeofEventProps += obj.GetSizeOfEventProps();
            }
            UnityEngine.Debug.Log(sizeofEventProps / 1000000 + " MB");
        }
        protected virtual void Tick()
        {
            Parallel.ForEach(_rewindObjects, obj => { obj.Tick(RecordCounter.CurrentFrameID); });
        }
        public virtual void SetFrame(in int frameID)
        {
            RecordCounter.CurrentFrameID = frameID;;
            Parallel.ForEach(_rewindObjects, obj => { obj.SetFrame(RecordCounter.CurrentFrameID); });
            foreach (var obj in _rewindObjects) obj.SetFrameToMainThreadComponents(RecordCounter.CurrentFrameID);
        }
        public virtual void SetFrame(IRewindObject obj, in int frameID)
        {
            obj.SetFrame(frameID);
            obj.SetFrameToMainThreadComponents(frameID);
        }
        public virtual void SetFrame(IRewindObject[] objArr, in int frameID)
        {
            Parallel.ForEach(objArr, obj => { obj.SetFrame(RecordCounter.CurrentFrameID); });
            foreach (var obj in objArr) obj.SetFrameToMainThreadComponents(RecordCounter.CurrentFrameID);
        }
    }
}

