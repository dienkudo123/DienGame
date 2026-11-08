using System;
using System.Collections.Generic;
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
                    _buttons[i].gameObject
                        .SetActive(false);

                    continue;
                }

                _buttons[i].gameObject
                    .SetActive(true);

                UpgradeDefinition option =
                    options[i];

                _buttonTexts[i].text =
                    $"{option.Title}\n" +
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
                return;

            if (index < 0)
                return;

            if (index >= _currentOptions.Count)
                return;

            UpgradeDefinition option =
                _currentOptions[index];

            _onSelected?.Invoke(option);
        }

        private void BuildUI()
        {
            _panel =
                CreateUIObject(
                    "LevelUpPanel",
                    transform
                );

            RectTransform panelRect =
                _panel.GetComponent<RectTransform>();

            panelRect.anchorMin =
                new Vector2(0f, 0f);

            panelRect.anchorMax =
                new Vector2(1f, 1f);

            panelRect.offsetMin =
                Vector2.zero;

            panelRect.offsetMax =
                Vector2.zero;

            Image background =
                _panel.AddComponent<Image>();

            background.color =
                new Color(
                    0f,
                    0f,
                    0f,
                    0.75f
                );

            CreateTitle();

            for (int i = 0;
                 i < OptionCount;
                 i++)
            {
                CreateOptionButton(i);
            }
        }

        private void CreateTitle()
        {
            GameObject titleObject =
                CreateUIObject(
                    "Title",
                    _panel.transform
                );

            RectTransform rect =
                titleObject.GetComponent<RectTransform>();

            rect.anchorMin =
                new Vector2(0.5f, 0.5f);

            rect.anchorMax =
                new Vector2(0.5f, 0.5f);

            rect.pivot =
                new Vector2(0.5f, 0.5f);

            rect.sizeDelta =
                new Vector2(800f, 120f);

            rect.anchoredPosition =
                new Vector2(0f, 400f);

            TextMeshProUGUI text =
                titleObject.AddComponent<TextMeshProUGUI>();

            text.text =
                "LEVEL UP";

            text.fontSize = 56f;

            text.alignment =
                TextAlignmentOptions.Center;
        }

        private void CreateOptionButton(
            int index)
        {
            GameObject buttonObject =
                CreateUIObject(
                    $"UpgradeButton_{index + 1}",
                    _panel.transform
                );

            RectTransform rect =
                buttonObject.GetComponent<RectTransform>();

            rect.anchorMin =
                new Vector2(0.5f, 0.5f);

            rect.anchorMax =
                new Vector2(0.5f, 0.5f);

            rect.pivot =
                new Vector2(0.5f, 0.5f);

            rect.sizeDelta =
                new Vector2(800f, 180f);

            float y =
                180f -
                index * 220f;

            rect.anchoredPosition =
                new Vector2(
                    0f,
                    y
                );

            Image image =
                buttonObject.AddComponent<Image>();

            image.color =
                new Color(
                    0.20f,
                    0.20f,
                    0.20f,
                    0.95f
                );

            Button button =
                buttonObject.AddComponent<Button>();

            int capturedIndex = index;

            button.onClick.AddListener(
                () =>
                    SelectOption(
                        capturedIndex
                    )
            );

            _buttons[index] = button;

            CreateButtonText(
                buttonObject.transform,
                index
            );
        }

        private void CreateButtonText(
            Transform parent,
            int index)
        {
            GameObject textObject =
                CreateUIObject(
                    "Text",
                    parent
                );

            RectTransform rect =
                textObject.GetComponent<RectTransform>();

            rect.anchorMin =
                Vector2.zero;

            rect.anchorMax =
                Vector2.one;

            rect.offsetMin =
                new Vector2(25f, 15f);

            rect.offsetMax =
                new Vector2(-25f, -15f);

            TextMeshProUGUI text =
                textObject.AddComponent<TextMeshProUGUI>();

            text.fontSize = 38f;

            text.alignment =
                TextAlignmentOptions.Center;

            text.enableWordWrapping = true;

            _buttonTexts[index] = text;
        }

        private static GameObject CreateUIObject(
            string objectName,
            Transform parent)
        {
            var gameObject =
                new GameObject(
                    objectName,
                    typeof(RectTransform)
                );

            gameObject.transform
                .SetParent(
                    parent,
                    false
                );

            return gameObject;
        }
    }
}