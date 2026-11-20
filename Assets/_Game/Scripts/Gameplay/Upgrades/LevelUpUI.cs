using System;
using System.Collections.Generic;
using DienGame.Client;
using DienGame.Gameplay.Skills.Visuals;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DienGame.Gameplay.Upgrades
{
    /// <summary>
    /// Readable three-choice skill selection for all classes. Replaces the former
    /// 205px high flat cards without touching skill choice/pause/combat logic.
    /// </summary>
    [RequireComponent(typeof(Canvas))]
    public sealed class LevelUpUI : MonoBehaviour
    {
        private const int OptionCount = 3;
        private GameObject _overlay;
        private readonly Button[] _buttons = new Button[OptionCount];
        private readonly TMP_Text[] _titles = new TMP_Text[OptionCount];
        private readonly TMP_Text[] _descriptions = new TMP_Text[OptionCount];
        private readonly Image[] _icons = new Image[OptionCount];
        private IReadOnlyList<UpgradeDefinition> _options;
        private Action<UpgradeDefinition> _onSelect;

        private void Awake()
        {
            CreateUI();
            Hide();
        }

        public void Show(IReadOnlyList<UpgradeDefinition> options,
            Action<UpgradeDefinition> onSelected)
        {
            _options = options;
            _onSelect = onSelected;
            for (int i = 0; i < OptionCount; i++)
            {
                bool has = options != null && i < options.Count;
                _buttons[i].gameObject.SetActive(has);
                if (!has) continue;
                UpgradeDefinition current = options[i];
                _titles[i].text = current.Title;
                _descriptions[i].text = current.Description;
                Sprite sprite = SkillIconCatalogProvider.GetIcon(current.SkillId);
                if (sprite == null && (int)current.SkillId >= 500 && (int)current.SkillId <= 506)
                    sprite = Resources.Load<Sprite>("Equipment/Icons/Warrior_Balanced");
                _icons[i].sprite = sprite;
                _icons[i].enabled = sprite != null;
            }
            _overlay.SetActive(true);
            _overlay.transform.SetAsLastSibling();
        }

        public void Hide()
        {
            if (_overlay != null) _overlay.SetActive(false);
        }

        private void Choose(int index)
        {
            if (_options == null || index < 0 || index >= _options.Count) return;
            _onSelect?.Invoke(_options[index]);
        }

        private void CreateUI()
        {
            _overlay = DienGameUITheme.Panel(transform, "LevelUpOverlay", "Panel");
            DienGameUITheme.Rect((RectTransform)_overlay.transform, 0f, 0f, 1f, 1f);
            Image veil = _overlay.GetComponent<Image>();
            veil.sprite = null;
            veil.color = DienGameUITheme.Veil;
            veil.raycastTarget = true;

            GameObject safe = new GameObject("SafeAreaContent", typeof(RectTransform));
            safe.transform.SetParent(_overlay.transform, false);
            DienGameUITheme.Rect((RectTransform)safe.transform, 0f, 0f, 1f, 1f);
            safe.AddComponent<DienGameSafeAreaRoot>();
            GameObject frame = DienGameUITheme.Panel(safe.transform, "LevelUpFrame", "Panel");
            DienGameUITheme.Rect((RectTransform)frame.transform, .035f, .165f, .965f, .835f);
            GameObject header = DienGameUITheme.Panel(frame.transform, "LevelUpHeader", "Header");
            DienGameUITheme.Rect((RectTransform)header.transform, .03f, .871f, .97f, .965f);
            DienGameUITheme.Text(header.transform, "Title", "LÊN CẤP", 54f,
                DienGameUITheme.Gold, .05f, .14f, .95f, .90f,
                TextAlignmentOptions.Center, true);
            DienGameUITheme.Text(frame.transform, "Hint", "CHỌN MỘT SỨC MẠNH CHO HÀNH TRÌNH",
                30f, DienGameUITheme.Muted, .06f, .795f, .94f, .865f);

            for (int i = 0; i < OptionCount; i++) CreateCard(frame.transform, i);
            DienGameUITheme.Text(frame.transform, "ChoiceFootnote", "Mỗi lựa chọn thay đổi lối chiến đấu của nhân vật.",
                27f, DienGameUITheme.Muted, .05f, .017f, .95f, .077f);
        }

        private void CreateCard(Transform frame, int index)
        {
            float top = .783f - index * .234f;
            GameObject card = DienGameUITheme.Panel(frame, "SkillChoice_" + index, "Card");
            DienGameUITheme.Rect((RectTransform)card.transform, .050f, top - .213f, .950f, top);
            Image image = card.GetComponent<Image>();
            image.raycastTarget = true;
            Button button = card.AddComponent<Button>();
            button.targetGraphic = image;
            ColorBlock colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(.94f, 1.06f, 1.02f);
            colors.pressedColor = new Color(.72f, .91f, .86f);
            colors.selectedColor = Color.white;
            button.colors = colors;
            int captured = index;
            button.onClick.AddListener(() => Choose(captured));
            _buttons[index] = button;

            GameObject plate = DienGameUITheme.Panel(card.transform, "SkillIconPlate", "IconPlate");
            DienGameUITheme.Rect((RectTransform)plate.transform, .027f, .205f, .254f, .805f);
            _icons[index] = DienGameUITheme.Icon(card.transform, "SkillIcon", null,
                .044f, .275f, .237f, .725f);
            _titles[index] = DienGameUITheme.Text(card.transform, "SkillTitle", "",
                36f, DienGameUITheme.Ivory, .278f, .610f, .960f, .900f,
                TextAlignmentOptions.MidlineLeft, true);
            _descriptions[index] = DienGameUITheme.Text(card.transform, "SkillDescription", "",
                28f, DienGameUITheme.Muted, .280f, .115f, .960f, .590f,
                TextAlignmentOptions.MidlineLeft);
            _descriptions[index].fontSizeMin = 20f;
            _titles[index].fontSizeMin = 24f;
        }
    }
}
