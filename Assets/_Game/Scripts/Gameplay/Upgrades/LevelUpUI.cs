using System;
using System.Collections.Generic;
using DienGame.Gameplay.Skills.Visuals;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DienGame.Gameplay.Upgrades
{
    [RequireComponent(typeof(Canvas))]
    public sealed class LevelUpUI : MonoBehaviour
    {
        private const int OptionCount = 3;

        private GameObject _panel;

        private readonly Button[] _buttons =
            new Button[OptionCount];

        private readonly TMP_Text[] _buttonTexts =
            new TMP_Text[OptionCount];

        private readonly Image[] _buttonIcons =
            new Image[OptionCount];

        private readonly RectTransform[] _buttonTextRects =
            new RectTransform[OptionCount];

        private IReadOnlyList<UpgradeDefinition>
            _currentOptions;

        private Action<UpgradeDefinition>
            _onSelected;

        private void Awake()
        {
            BuildUI();
            Hide();
        }

        public void Show(
            IReadOnlyList<UpgradeDefinition> options,
            Action<UpgradeDefinition> onSelected)
        {
            _currentOptions = options;
            _onSelected = onSelected;

            for (int i = 0; i < OptionCount; i++)
            {
                if (i >= options.Count)
                {
                    _buttons[i].gameObject.SetActive(false);
                    continue;
                }

                _buttons[i].gameObject.SetActive(true);

                UpgradeDefinition option = options[i];
                Sprite icon = SkillIconCatalogProvider.GetIcon(option.SkillId);

                ApplyIcon(i, icon);

                _buttonTexts[i].text =
                    $"<b>{option.Title}</b>\n" +
                    $"<size=28>{option.Description}</size>";
            }

            _panel.SetActive(true);
        }

        public void Hide()
        {
            if (_panel != null)
            {
                _panel.SetActive(false);
            }
        }

        private void SelectOption(int index)
        {
            if (_currentOptions == null)
            {
                return;
            }

            if (index < 0 || index >= _currentOptions.Count)
            {
                return;
            }

            UpgradeDefinition option = _currentOptions[index];
            _onSelected?.Invoke(option);
        }

        private void BuildUI()
        {
            _panel = CreateUIObject(
                "LevelUpPanel",
                transform);

            RectTransform panelRect =
                _panel.GetComponent<RectTransform>();

            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.one;
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;

            Image background = _panel.AddComponent<Image>();
            background.color = new Color(0f, 0f, 0f, 0.78f);

            CreateTitle();

            for (int i = 0; i < OptionCount; i++)
            {
                CreateOptionButton(i);
            }
        }

        private void CreateTitle()
        {
            GameObject titleObject = CreateUIObject(
                "Title",
                _panel.transform);

            RectTransform rect =
                titleObject.GetComponent<RectTransform>();

            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(820f, 130f);
            rect.anchoredPosition = new Vector2(0f, 430f);

            TextMeshProUGUI text =
                titleObject.AddComponent<TextMeshProUGUI>();

            text.text = "LÊN CẤP";
            text.fontSize = 58f;
            text.fontStyle = FontStyles.Bold;
            text.alignment = TextAlignmentOptions.Center;
            text.color = new Color(1f, 0.82f, 0.28f, 1f);
        }

        private void CreateOptionButton(int index)
        {
            GameObject buttonObject = CreateUIObject(
                $"UpgradeButton_{index + 1}",
                _panel.transform);

            RectTransform rect =
                buttonObject.GetComponent<RectTransform>();

            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(860f, 205f);

            float y = 180f - index * 235f;
            rect.anchoredPosition = new Vector2(0f, y);

            Image image = buttonObject.AddComponent<Image>();
            image.color = new Color(0.12f, 0.12f, 0.16f, 0.97f);

            Outline outline = buttonObject.AddComponent<Outline>();
            outline.effectColor = new Color(1f, 0.55f, 0.10f, 0.75f);
            outline.effectDistance = new Vector2(2f, -2f);

            Button button = buttonObject.AddComponent<Button>();
            button.targetGraphic = image;

            int capturedIndex = index;
            button.onClick.AddListener(
                () => SelectOption(capturedIndex));

            _buttons[index] = button;

            CreateButtonIcon(buttonObject.transform, index);
            CreateButtonText(buttonObject.transform, index);
        }

        private void CreateButtonIcon(Transform parent, int index)
        {
            GameObject iconObject = CreateUIObject(
                "SkillIcon",
                parent);

            RectTransform rect =
                iconObject.GetComponent<RectTransform>();

            rect.anchorMin = new Vector2(0f, 0.5f);
            rect.anchorMax = new Vector2(0f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(150f, 150f);
            rect.anchoredPosition = new Vector2(105f, 0f);

            Image image = iconObject.AddComponent<Image>();
            image.preserveAspect = true;
            image.raycastTarget = false;
            image.enabled = false;

            _buttonIcons[index] = image;
        }

        private void CreateButtonText(Transform parent, int index)
        {
            GameObject textObject = CreateUIObject(
                "Text",
                parent);

            RectTransform rect =
                textObject.GetComponent<RectTransform>();

            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(205f, 18f);
            rect.offsetMax = new Vector2(-35f, -18f);

            TextMeshProUGUI text =
                textObject.AddComponent<TextMeshProUGUI>();

            text.fontSize = 37f;
            text.alignment = TextAlignmentOptions.MidlineLeft;
            text.enableWordWrapping = true;
            text.color = Color.white;
            text.raycastTarget = false;

            _buttonTexts[index] = text;
            _buttonTextRects[index] = rect;
        }

        private void ApplyIcon(int index, Sprite icon)
        {
            Image image = _buttonIcons[index];
            RectTransform textRect = _buttonTextRects[index];

            bool hasIcon = icon != null;

            image.sprite = icon;
            image.enabled = hasIcon;

            if (hasIcon)
            {
                textRect.offsetMin = new Vector2(205f, 18f);
            }
            else
            {
                textRect.offsetMin = new Vector2(35f, 18f);
            }
        }

        private static GameObject CreateUIObject(
            string objectName,
            Transform parent)
        {
            GameObject gameObject = new(
                objectName,
                typeof(RectTransform));

            gameObject.transform.SetParent(parent, false);
            return gameObject;
        }
    }
}
