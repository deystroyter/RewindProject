using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RewindProject.UI
{
    public class RewindController_View : MonoBehaviour
    {
        [SerializeField] private ARewindController controller;

        [Header("Record/Paused")]
        [SerializeField] private Image icon;
        [SerializeField] private TMP_Text textbox;
        [SerializeField] private Sprite _recordSprite;
        [SerializeField] private Sprite _pauseSprite;

        private const string _RecordText = "Recording...";
        private const string _PauseText = "Pause";

        [Header("FramesSlider")]
        [SerializeField] private TMP_Text minFrame_Textbox;
        [SerializeField] private TMP_Text currentFrame_Textbox;
        [SerializeField] private TMP_Text maxFrames_Textbox;
        [SerializeField] private Slider frameSlider;

        private void Start()
        {
            if (controller == null) { controller = SceneRewindController.Instance; }
            controller.RecordCounter.RecordingStarted += OnRecordingStarted;
            controller.RecordCounter.RecordingPaused += OnRecordingPaused;

            controller.RecordCounter.CurrentFrameIDChanged += UpdateCurrentAndMaxFrameID;
            controller.RecordCounter.MinFrameIDChanged += UpdateMinFrameID;
            frameSlider.onValueChanged.AddListener(UpdateRewindController);
            
        }

        void OnRecordingStarted()
        {
            icon.sprite = _recordSprite;
            textbox.text = _RecordText;
        }
        void OnRecordingPaused()
        {
            icon.sprite = _pauseSprite;
            textbox.text = _PauseText;
        }
        private void UpdateMinFrameID(int minFrame)
        {
            minFrame_Textbox.text = minFrame.ToString();
            frameSlider.minValue = minFrame;
        }
        private void UpdateCurrentAndMaxFrameID(int currentFrame, int maxFrames)
        {
            currentFrame_Textbox.text = currentFrame.ToString();
            maxFrames_Textbox.text = maxFrames.ToString();
            frameSlider.maxValue = maxFrames;
            frameSlider.SetValueWithoutNotify(currentFrame);
        }

        private void UpdateRewindController(float value)
        {
            controller.PauseRecording();
            currentFrame_Textbox.text = value.ToString();
            controller.SetFrame(Mathf.RoundToInt(value));
        }


    }
}
