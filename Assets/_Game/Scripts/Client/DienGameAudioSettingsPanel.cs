using System;
using DienGame.Gameplay.Phase13;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DienGame.Client
{
    /// <summary>
    /// Shared code-built audio settings overlay for Lobby and Pause.
    /// Uses the existing Phase13 BGM/SFX preferences; never creates or reloads audio sources.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class DienGameAudioSettingsPanel : MonoBehaviour
    {
        private Slider _musicSlider;
        private Slider _sfxSlider;
        private TMP_Text _musicPercent;
        private TMP_Text _sfxPercent;
        private TMP_Text _muteCaption;
        private Action _onClose;

        public static GameObject Create(Transform parent, Action onClose)
        {
            GameObject overlay = new GameObject("AudioSettingsOverlay", typeof(RectTransform), typeof(Image));
            overlay.transform.SetParent(parent, false);
            DienGameUITheme.Rect((RectTransform)overlay.transform, 0f, 0f, 1f, 1f);
            Image shade = overlay.GetComponent<Image>();
            shade.color = new Color(.006f, .018f, .035f, .94f);
            shade.raycastTarget = true;

            // Initialize after the root is inside its parent. The owner hides the result
            // until the player opens the settings menu.
            DienGameAudioSettingsPanel controller = overlay.AddComponent<DienGameAudioSettingsPanel>();
            controller._onClose = onClose;
            controller.Build();
            controller.RefreshFromSavedValues();
            return overlay;
        }

        private void OnEnable() => RefreshFromSavedValues();

        private void Build()
        {
            GameObject frame = DienGameUITheme.Panel(transform, "AudioSettingsFrame", "Panel");
            DienGameUITheme.Rect((RectTransform)frame.transform, .07f, .245f, .93f, .755f);
            // The card accepts raycasts so touches never pass through to gameplay.
            frame.GetComponent<Image>().raycastTarget = true;

            GameObject header = DienGameUITheme.Panel(frame.transform, "Header", "Header");
            DienGameUITheme.Rect((RectTransform)header.transform, .045f, .805f, .955f, .95f);
            DienGameUITheme.Text(header.transform, "Title", "CÀI ĐẶT ÂM THANH", 42f,
                DienGameUITheme.Gold, .04f, .1f, .96f, .9f,
                TextAlignmentOptions.Center, true);
            DienGameUITheme.Text(frame.transform, "Explanation", "ÁP DỤNG NGAY · LƯU TỰ ĐỘNG", 24f,
                DienGameUITheme.Muted, .08f, .71f, .92f, .785f);

            DienGameUITheme.Text(frame.transform, "MusicLabel", "NHẠC NỀN", 31f,
                DienGameUITheme.Ivory, .105f, .595f, .70f, .685f,
                TextAlignmentOptions.MidlineLeft, true);
            _musicPercent = DienGameUITheme.Text(frame.transform, "MusicPercent", "", 28f,
                DienGameUITheme.Gold, .71f, .595f, .9f, .685f,
                TextAlignmentOptions.Right, true);
            _musicSlider = MakeSlider(frame.transform, "MusicVolume", .105f, .495f, .895f, .59f,
                DienGameUITheme.Gold);
            _musicSlider.onValueChanged.AddListener(OnMusicChanged);

            DienGameUITheme.Text(frame.transform, "SfxLabel", "HIỆU ỨNG ÂM THANH", 31f,
                DienGameUITheme.Ivory, .105f, .385f, .70f, .475f,
                TextAlignmentOptions.MidlineLeft, true);
            _sfxPercent = DienGameUITheme.Text(frame.transform, "SfxPercent", "", 28f,
                DienGameUITheme.Teal, .71f, .385f, .9f, .475f,
                TextAlignmentOptions.Right, true);
            _sfxSlider = MakeSlider(frame.transform, "SfxVolume", .105f, .285f, .895f, .38f,
                DienGameUITheme.Teal);
            _sfxSlider.onValueChanged.AddListener(OnSfxChanged);

            Button muteButton = DienGameUITheme.Button(frame.transform, "MuteMusic", "TẮT NHẠC NỀN",
                ToggleMusicMute, "ButtonGhost", .105f, .145f, .49f, .25f, 27f);
            _muteCaption = muteButton.GetComponentInChildren<TMP_Text>();
            DienGameUITheme.Button(frame.transform, "CloseAudioSettings", "QUAY LẠI",
                Close, "ButtonGold", .51f, .145f, .895f, .25f, 29f);

            DienGameUITheme.Text(frame.transform, "Footnote",
                "ÂM LƯỢNG GIỮ NGUYÊN KHI CHUYỂN CẢNH", 23f, DienGameUITheme.Muted,
                .07f, .035f, .93f, .115f);
        }

        private static Slider MakeSlider(Transform parent, string name,
            float x0, float y0, float x1, float y1, Color fillColor)
        {
            GameObject bar = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Slider));
            bar.transform.SetParent(parent, false);
            DienGameUITheme.Rect((RectTransform)bar.transform, x0, y0, x1, y1);
            // Invisible but raycastable target for dragging anywhere on the track.
            Image input = bar.GetComponent<Image>();
            input.color = new Color(1f, 1f, 1f, .001f);
            input.raycastTarget = true;

            GameObject track = new GameObject("Track", typeof(RectTransform), typeof(Image));
            track.transform.SetParent(bar.transform, false);
            DienGameUITheme.Rect((RectTransform)track.transform, .015f, .34f, .985f, .66f);
            Image trackImage = track.GetComponent<Image>();
            trackImage.color = new Color(.10f, .18f, .25f, 1f);
            trackImage.raycastTarget = false;

            GameObject fillArea = new GameObject("FillArea", typeof(RectTransform));
            fillArea.transform.SetParent(bar.transform, false);
            DienGameUITheme.Rect((RectTransform)fillArea.transform, .015f, .34f, .985f, .66f);
            GameObject fill = new GameObject("Fill", typeof(RectTransform), typeof(Image));
            fill.transform.SetParent(fillArea.transform, false);
            DienGameUITheme.Rect((RectTransform)fill.transform, 0f, 0f, 1f, 1f);
            Image fillImage = fill.GetComponent<Image>();
            fillImage.color = fillColor;
            fillImage.raycastTarget = false;

            GameObject handleArea = new GameObject("HandleArea", typeof(RectTransform));
            handleArea.transform.SetParent(bar.transform, false);
            DienGameUITheme.Rect((RectTransform)handleArea.transform, .04f, 0f, .96f, 1f);
            GameObject handle = DienGameUITheme.Panel(handleArea.transform, "Handle", "IconPlate");
            RectTransform handleRect = (RectTransform)handle.transform;
            handleRect.anchorMin = handleRect.anchorMax = new Vector2(.5f, .5f);
            handleRect.sizeDelta = new Vector2(39f, 39f);
            handleRect.anchoredPosition = Vector2.zero;
            Image handleImage = handle.GetComponent<Image>();
            handleImage.raycastTarget = true;

            Slider slider = bar.GetComponent<Slider>();
            slider.navigation = new Navigation { mode = Navigation.Mode.None };
            slider.direction = Slider.Direction.LeftToRight;
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.wholeNumbers = false;
            slider.fillRect = (RectTransform)fill.transform;
            slider.handleRect = handleRect;
            slider.targetGraphic = handleImage;
            return slider;
        }

        private void OnMusicChanged(float value)
        {
            Phase13BackgroundMusic.SetVolume(value);
            UpdatePercentTexts();
        }

        private void OnSfxChanged(float value)
        {
            Phase13Audio.SetVolume(value);
            UpdatePercentTexts();
        }

        private void ToggleMusicMute()
        {
            Phase13BackgroundMusic.SetMuted(!Phase13BackgroundMusic.IsMuted);
            RefreshFromSavedValues();
        }

        private void RefreshFromSavedValues()
        {
            if (_musicSlider != null) _musicSlider.SetValueWithoutNotify(Phase13BackgroundMusic.Volume);
            if (_sfxSlider != null) _sfxSlider.SetValueWithoutNotify(Phase13Audio.Volume);
            UpdatePercentTexts();
            if (_muteCaption != null)
                _muteCaption.text = Phase13BackgroundMusic.IsMuted ? "BẬT NHẠC NỀN" : "TẮT NHẠC NỀN";
        }

        private void UpdatePercentTexts()
        {
            if (_musicPercent != null)
                _musicPercent.text = Mathf.RoundToInt(Phase13BackgroundMusic.Volume * 100f) + "%";
            if (_sfxPercent != null)
                _sfxPercent.text = Mathf.RoundToInt(Phase13Audio.Volume * 100f) + "%";
        }

        private void Close()
        {
            if (_onClose != null) _onClose();
            else gameObject.SetActive(false);
        }
    }
}
