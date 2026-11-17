using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DungeonKnight
{
    public class SettingsPanel : MonoBehaviour
    {
        [SerializeField] GameObject panel;
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

        void Awake()
        {
            RefCheck.Valid(this, panel, nameof(panel));
            RefCheck.Valid(this, confirmPanel, nameof(confirmPanel));
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
            Select(screenShakeButton);
        }

        public void Close()
        {
            if (IsConfirming)
            {
                CancelReset();
                return;
            }
            if (panel == null || !panel.activeSelf) return;
            panel.SetActive(false);
            Closed?.Invoke();
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
            if (screenShakeLabel != null) screenShakeLabel.text = $"SCREEN SHAKE: {(settings.screenShake ? "ON" : "OFF")}";
            if (autoAimLabel != null) autoAimLabel.text = $"AUTO AIM: {(settings.autoAim ? "ON" : "OFF")}";
        }

        void SetStatus(string message)
        {
            if (statusText != null) statusText.text = message;
        }

        static void Select(Button button)
        {
            if (button != null && EventSystem.current != null) EventSystem.current.SetSelectedGameObject(button.gameObject);
        }
    }
}
