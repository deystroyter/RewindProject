namespace RewindProject
{
    public interface IRewindController
    {
        RecordCounter RecordCounter { get; }
        bool IsPaused { get; }
        public void AddRewindObject(IRewindObject rewindObject);
        public void RemoveRewindObject(IRewindObject rewindObject);
        public void StartRecording();
        public void PauseRecording();
        public void SetFrame(in int frameID);
        public void SetFrame(IRewindObject obj, in int frameID);
        public void SetFrame(IRewindObject[] objArr, in int frameID);
    }
}
