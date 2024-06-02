using System.ComponentModel;

namespace RewindProject
{
    public interface IRewindableComponent
    {
        RecordCounter RecordCounter { set; }
        bool IsMainThreadComponent { get; }
        int GetSizeOfEventProps();
        void Tick(in int tickID);
        void SetFrame(in int frameID);
        void ClearInfo(in int frameID);

    }
}

