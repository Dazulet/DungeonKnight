using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DungeonKnight
{
    public class SettingsPanel : MonoBehaviour
    {
        [SerializeField] GameObject panel;
        [SerializeField] Slider musicSlider;
        [SerializeField] Text musicLabel;
        [SerializeField] Slider sfxSlider;
        [SerializeField] Text sfxLabel;
        [SerializeField, Min(0f)] float sfxPreviewInterval = 0.15f;
        [SerializeField] Button screenShakeButton;
        [SerializeField] Text screenShakeLabel;
        [SerializeField] Button autoAimButton;
        [SerializeField] Text autoAimLabel;
        [SerializeField] Button resetButton;
        [SerializeField] Button backButton;
        [SerializeField] Text statusText;
        [SerializeField] GameObject confirmPanel;
        [SerializeField] Button confirmButton;
        [SerializeField] Button cancelButton;

        public bool IsOpen => panel != null && panel.activeSelf;
        public bool IsConfirming => confirmPanel != null && confirmPanel.activeSelf;
        public event System.Action Closed;

        bool volumeDirty;
        float nextPreview;

        void Awake()
        {
            RefCheck.Valid(this, panel, nameof(panel));
            RefCheck.Valid(this, confirmPanel, nameof(confirmPanel));
            if (musicSlider != null) musicSlider.onValueChanged.AddListener(OnMusicChanged);
            if (sfxSlider != null) sfxSlider.onValueChanged.AddListener(OnSfxChanged);
            if (screenShakeButton != null) screenShakeButton.onClick.AddListener(ToggleScreenShake);
            if (autoAimButton != null) autoAimButton.onClick.AddListener(ToggleAutoAim);
            if (resetButton != null) resetButton.onClick.AddListener(AskReset);
            if (backButton != null) backButton.onClick.AddListener(Close);
            if (confirmButton != null) confirmButton.onClick.AddListener(ConfirmReset);
            if (cancelButton != null) cancelButton.onClick.AddListener(CancelReset);
            if (confirmPanel != null) confirmPanel.SetActive(false);
            if (panel != null) panel.SetActive(false);
        }

        public void Open()
        {
            if (panel == null) return;
            SetStatus("");
            Refresh();
            panel.SetActive(true);
            if (musicSlider != null) Select(musicSlider);
            else Select(screenShakeButton);
        }

        public void Close()
        {
            if (IsConfirming)
            {
                CancelReset();
                return;
            }
            if (panel == null || !panel.activeSelf) return;
            CommitVolume();
            panel.SetActive(false);
            Closed?.Invoke();
        }

        void OnDisable() => CommitVolume();

        void OnMusicChanged(float value)
        {
            SaveManager.SetMusicVolume(value, false);
            volumeDirty = true;
            RefreshVolumeLabels();
        }

        void OnSfxChanged(float value)
        {
            SaveManager.SetSfxVolume(value, false);
            volumeDirty = true;
            RefreshVolumeLabels();
            if (Time.unscaledTime < nextPreview) return;
            nextPreview = Time.unscaledTime + sfxPreviewInterval;
            GameAudio.Play(SoundId.CoinPickup);
        }

        void CommitVolume()
        {
            if (!volumeDirty) return;
            volumeDirty = false;
            SaveManager.Save();
        }

        void ToggleScreenShake()
        {
            SaveManager.SetScreenShake(!SaveManager.Settings.screenShake);
            Refresh();
        }

        void ToggleAutoAim()
        {
            SaveManager.SetAutoAim(!SaveManager.Settings.autoAim);
            Refresh();
        }

        void AskReset()
        {
            if (confirmPanel == null) return;
            confirmPanel.SetActive(true);
            Select(cancelButton);
        }

        void ConfirmReset()
        {
            SaveManager.ResetProgress();
            if (confirmPanel != null) confirmPanel.SetActive(false);
            SetStatus("PROGRESS RESET");
            Select(backButton);
        }

        void CancelReset()
        {
            if (confirmPanel != null) confirmPanel.SetActive(false);
            Select(resetButton);
        }

        void Refresh()
        {
            GameSettings settings = SaveManager.Settings;
            if (musicSlider != null) musicSlider.SetValueWithoutNotify(settings.musicVolume);
            if (sfxSlider != null) sfxSlider.SetValueWithoutNotify(settings.sfxVolume);
            RefreshVolumeLabels();
            if (screenShakeLabel != null) screenShakeLabel.text = $"SCREEN SHAKE: {(settings.screenShake ? "ON" : "OFF")}";
            if (autoAimLabel != null) autoAimLabel.text = $"AUTO AIM: {(settings.autoAim ? "ON" : "OFF")}";
        }

        void RefreshVolumeLabels()
        {
            GameSettings settings = SaveManager.Settings;
            if (musicLabel != null) musicLabel.text = $"{Mathf.RoundToInt(settings.musicVolume * 100f)}%";
            if (sfxLabel != null) sfxLabel.text = $"{Mathf.RoundToInt(settings.sfxVolume * 100f)}%";
        }

        void SetStatus(string message)
        {
            if (statusText != null) statusText.text = message;
        }

        static void Select(Selectable selectable)
        {
            if (selectable != null && EventSystem.current != null) EventSystem.current.SetSelectedGameObject(selectable.gameObject);
        }
    }
}
