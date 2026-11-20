using System;
using System.Collections.Generic;
using DienGame.Gameplay.Equipment;
using DienGame.Gameplay.Skills.Mage;
using DienGame.Gameplay.World;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DienGame.Client
{
    /// <summary>
    /// Portrait-first, illustrated lobby panels. Original shop/inventory/quest actions
    /// are reused verbatim; only presentation and touch targets have been redesigned.
    /// </summary>
    public sealed class WorldHubPanels : MonoBehaviour
    {
        private string _tab;
        private Action _onResourcesChanged;
        private RectTransform _body;
        private TMP_Text _notice;
        private int _gearPage;
        private float _scrollPosition = 1f;

        public static void Open(Transform parent, string tab, Action resourcesChanged)
        {
            WorldHubPanels previous = parent.GetComponentInChildren<WorldHubPanels>(true);
            if (previous != null) Destroy(previous.gameObject);
            GameObject root = DienGameUITheme.Panel(parent, "WorldHub_" + tab, "Panel");
            DienGameUITheme.Rect((RectTransform)root.transform, 0f, 0f, 1f, 1f);
            Image img = root.GetComponent<Image>();
            img.sprite = null;
            img.color = DienGameUITheme.Veil;
            img.raycastTarget = true;
            var hub = root.AddComponent<WorldHubPanels>();
            hub._tab = tab;
            hub._onResourcesChanged = resourcesChanged;
            hub.Build();
        }

        private static GameObject Panel(Transform parent, string name, string style,
            float x0, float y0, float x1, float y1)
        {
            GameObject p = DienGameUITheme.Panel(parent, name, style);
            DienGameUITheme.Rect((RectTransform)p.transform, x0, y0, x1, y1);
            return p;
        }

        private static TMP_Text Label(Transform p, string name, string s, float size, Color color,
            float x0, float y0, float x1, float y1, TextAlignmentOptions align = TextAlignmentOptions.Center,
            bool bold = false)
        {
            return DienGameUITheme.Text(p, name, s, size, color, x0, y0, x1, y1, align, bold);
        }

        private static Button Action(Transform p, string name, string caption, Action onClick,
            string style, float x0, float y0, float x1, float y1, float size = 31f)
        {
            return DienGameUITheme.Button(p, name, caption, onClick, style, x0, y0, x1, y1, size);
        }

        private void Build()
        {
            // Modal backdrop spans the screen; its interactive content stays clear of notches.
            GameObject safe = new GameObject("SafeAreaContent", typeof(RectTransform));
            safe.transform.SetParent(transform, false);
            DienGameUITheme.Rect((RectTransform)safe.transform, 0f, 0f, 1f, 1f);
            safe.AddComponent<DienGameSafeAreaRoot>();
            Transform view = safe.transform;
            Panel(view, "Frame", "Panel", .037f, .065f, .963f, .967f);
            GameObject header = Panel(view, "Heading", "Header", .050f, .872f, .950f, .958f);
            Label(header.transform, "HeadingLabel", _tab, 45f, DienGameUITheme.Gold,
                .10f, .12f, .76f, .90f, TextAlignmentOptions.Center, true);
            Action(header.transform, "Close", "×", () => Destroy(gameObject),
                "ButtonGhost", .79f, .16f, .965f, .84f, 56f);
            GameObject body = Panel(view, "Content", "Panel", .057f, .160f, .943f, .862f);
            _body = (RectTransform)body.transform;
            _notice = Label(view, "Notification", "", 31f, DienGameUITheme.Gold,
                .085f, .082f, .915f, .147f, TextAlignmentOptions.Center, false);
            DrawContents();
        }

        private void DrawContents()
        {
            if (_tab.Contains("TRANG BỊ")) DrawEquipment();
            else if (_tab.Contains("NHIỆM VỤ")) DrawQuest();
            else if (_tab.Contains("CỬA HÀNG")) DrawShop();
            else if (_tab.Contains("KỸ NĂNG")) DrawSkills();
            else Label(_body, "MapHint", "NGŨ ĐẢO HUYỀN GIỚI", 38f, DienGameUITheme.Gold,
                .07f, .25f, .93f, .75f);
        }

        private void Refresh(string message)
        {
            // Preserve equipment tab and scroll position during an action.
            for (int i = _body.childCount - 1; i >= 0; i--)
                Destroy(_body.GetChild(i).gameObject);
            DrawContents();
            _notice.text = message;
            _onResourcesChanged?.Invoke();
        }

        private void DrawEquipment()
        {
            string resourceTitle = EquipmentQaSandbox.Active
                ? "TEST · 19 TRANG BỊ ĐÃ MỞ · CƯỜNG HÓA MIỄN PHÍ"
                : $"VÀNG  {MetaProgressService.Gold:N0}   •   TINH THỂ  {WorldInventory.Fragments}";
            Label(_body, "Resources", resourceTitle,
                EquipmentQaSandbox.Active ? 24f : 32f, DienGameUITheme.Gold,
                .05f, .926f, .95f, .984f, TextAlignmentOptions.Center, true);
            // Mage has four intentionally short labels to avoid cramming Vietnamese
            // text into narrow portrait tabs. Other classes preserve their v8 tabs.
            bool isMage = LobbySessionService.SelectedClass == PlayableClass.Mage;
            if (isMage)
            {
                Action(_body, "CoreTab", "THÚ", () => { _gearPage = 0; _scrollPosition = 1f; Refresh(""); },
                    _gearPage == 0 ? "ButtonTeal" : "ButtonGhost", .045f, .825f, .262f, .912f, 24f);
                Action(_body, "ArmorTab", "GIÁP", () => { _gearPage = 1; Refresh(""); },
                    _gearPage == 1 ? "ButtonTeal" : "ButtonGhost", .274f, .825f, .489f, .912f, 24f);
                Action(_body, "CalibrationTab", "LÕI", () => { _gearPage = 2; Refresh(""); },
                    _gearPage == 2 ? "ButtonTeal" : "ButtonGhost", .501f, .825f, .729f, .912f, 22f);
                Action(_body, "PetChipTab", "CHIP", () => { _gearPage = 3; Refresh(""); },
                    _gearPage == 3 ? "ButtonTeal" : "ButtonGhost", .741f, .825f, .955f, .912f, 23f);
            }
            else
            {
                Action(_body, "CoreTab", "VŨ KHÍ", () => { _gearPage = 0; _scrollPosition = 1f; Refresh(""); },
                    _gearPage == 0 ? "ButtonTeal" : "ButtonGhost", .045f, .825f, .347f, .912f, 25f);
                Action(_body, "ArmorTab", "GIÁP · PHÙ ẤN", () => { _gearPage = 1; Refresh(""); },
                    _gearPage == 1 ? "ButtonTeal" : "ButtonGhost", .350f, .825f, .650f, .912f, 24f);
                Action(_body, "CalibrationTab", "ĐỒNG BỘ LÕI", () => { _gearPage = 2; Refresh(""); },
                    _gearPage == 2 ? "ButtonTeal" : "ButtonGhost", .653f, .825f, .955f, .912f, 24f);
            }
            if (_gearPage == 1) { DrawArmor(); return; }
            if (_gearPage == 2) { DrawCalibration(); return; }
            if (_gearPage == 3 && isMage) { DrawPetChips(); return; }

            PlayableClass cls = LobbySessionService.SelectedClass;
            EquipmentDefinition selected = EquipmentInventory.Selected(cls);
            if (selected == null)
            {
                Label(_body, "Unavailable", "Hệ trang bị của class này chưa mở trong bản chơi hiện tại.",
                    34f, DienGameUITheme.Muted, .09f, .30f, .91f, .74f);
                return;
            }
            int level = EquipmentInventory.Level(selected);
            GameObject focus = Panel(_body, "EquippedItem", "CardSelected", .047f, .603f, .953f, .816f);
            Panel(focus.transform, "IconFrame", "IconPlate", .025f, .19f, .255f, .83f);
            DienGameUITheme.Icon(focus.transform, "EquippedIcon", EquipmentEvolutionArt.IconFor(selected),
                .047f, .25f, .231f, .78f);
            Label(focus.transform, "EquippedTitle", selected.Name + $"  +{level}", 34f,
                DienGameUITheme.Gold, .28f, .68f, .97f, .95f, TextAlignmentOptions.MidlineLeft, true);
            Label(focus.transform, "EquippedDescription", selected.Description, 28f,
                DienGameUITheme.Ivory, .28f, .20f, .96f, .68f, TextAlignmentOptions.MidlineLeft);
            Label(focus.transform, "EquippedDamage", "ĐÒN THƯỜNG ×" +
                (EquipmentCatalog.DamageAtLevel(selected, level) * EquipmentResonance.DamageFactor(selected)).ToString("0.00")
                + "   •   " + EquipmentResonance.StageName(level), 27f,
                DienGameUITheme.Teal, .28f, .03f, .95f, .22f, TextAlignmentOptions.MidlineLeft, true);

            Label(_body, "ChoiceHeading", "CHỌN TRANG BỊ", 29f,
                DienGameUITheme.Gold, .065f, .568f, .94f, .605f, TextAlignmentOptions.MidlineLeft, true);
            DrawEquipmentScroll(cls, selected);
            DrawUpgrade(selected, level);
        }

        private void DrawEquipmentScroll(PlayableClass cls, EquipmentDefinition selected)
        {
            GameObject viewportGo = Panel(_body, "ChoicesViewport", "Panel", .047f, .265f, .953f, .562f);
            Image viewportImage = viewportGo.GetComponent<Image>();
            viewportImage.raycastTarget = true;
            viewportGo.AddComponent<RectMask2D>();
            ScrollRect scroll = viewportGo.AddComponent<ScrollRect>();
            scroll.horizontal = false;
            scroll.vertical = true;
            scroll.movementType = ScrollRect.MovementType.Clamped;
            scroll.scrollSensitivity = 42f;
            scroll.viewport = (RectTransform)viewportGo.transform;
            GameObject contents = new GameObject("EquipmentRows", typeof(RectTransform));
            contents.transform.SetParent(viewportGo.transform, false);
            RectTransform content = (RectTransform)contents.transform;
            content.anchorMin = new Vector2(0f, 1f);
            content.anchorMax = new Vector2(1f, 1f);
            content.pivot = new Vector2(.5f, 1f);
            content.anchoredPosition = Vector2.zero;
            List<EquipmentDefinition> choices = EquipmentCatalog.ForClass(cls);
            const float rowHeight = 172f;
            content.sizeDelta = new Vector2(-12f, choices.Count * rowHeight);
            scroll.content = content;
            scroll.onValueChanged.AddListener(value => { if (scroll != null) _scrollPosition = value.y; });
            for (int i = 0; i < choices.Count; i++)
            {
                EquipmentDefinition item = choices[i];
                bool isCurrent = selected.Id == item.Id;
                bool unlocked = EquipmentInventory.IsUnlocked(item);
                GameObject row = DienGameUITheme.Panel(content, "Choice_" + item.Id,
                    isCurrent ? "CardSelected" : "Card");
                RectTransform rt = (RectTransform)row.transform;
                rt.anchorMin = new Vector2(0f, 1f);
                rt.anchorMax = new Vector2(1f, 1f);
                rt.pivot = new Vector2(.5f, 1f);
                rt.sizeDelta = new Vector2(0f, rowHeight - 9f);
                rt.anchoredPosition = new Vector2(0f, -i * rowHeight);
                Panel(row.transform, "IconBackdrop", "IconPlate", .018f, .16f, .178f, .84f);
                DienGameUITheme.Icon(row.transform, "EquipmentIcon", EquipmentEvolutionArt.IconFor(item),
                    .033f, .225f, .162f, .775f);
                Label(row.transform, "Name", item.Name, 28f, DienGameUITheme.Ivory,
                    .195f, .52f, .735f, .87f, TextAlignmentOptions.MidlineLeft, true);
                string sub = unlocked ? "Cường hóa +" + EquipmentInventory.Level(item) :
                    $"Mở: {item.UnlockGold:N0} vàng · {item.UnlockFragments} mảnh";
                Label(row.transform, "Sub", sub, 23f, unlocked ? DienGameUITheme.Teal : DienGameUITheme.Muted,
                    .195f, .14f, .742f, .49f, TextAlignmentOptions.MidlineLeft);
                string caption = isCurrent ? "ĐANG DÙNG" : unlocked ? "TRANG BỊ" : "MỞ KHÓA";
                Button button = Action(row.transform, "UseOrUnlock", caption, () =>
                {
                    bool success = unlocked ? EquipmentInventory.TryEquip(item) : EquipmentInventory.TryUnlock(item);
                    Refresh(success ? (unlocked ? "Đã đổi trang bị. Hiệu lực từ trận tiếp theo." :
                        "Đã mở khóa. Chọn TRANG BỊ để dùng.") : "Không đủ tài nguyên.");
                }, isCurrent ? "ButtonGhost" : unlocked ? "ButtonTeal" : "ButtonGold",
                    .748f, .25f, .976f, .76f, 24f);
                button.interactable = !isCurrent;
            }
            scroll.verticalNormalizedPosition = Mathf.Clamp01(_scrollPosition);
        }

        private void DrawUpgrade(EquipmentDefinition selected, int level)
        {
            GameObject card = Panel(_body, "UpgradeCard", "Card", .047f, .027f, .953f, .249f);
            Label(card.transform, "UpgradeTitle", "CƯỜNG HÓA TRANG BỊ", 35f,
                DienGameUITheme.Gold, .05f, .76f, .95f, .98f, TextAlignmentOptions.Center, true);
            string beforeAfter = level < EquipmentCatalog.MaxLevel ?
                $"+{level}   →   +{level + 1}" : $"+{level}   ·   TỐI ĐA";
            Label(card.transform, "Levels", beforeAfter, 37f, DienGameUITheme.Ivory,
                .05f, .51f, .95f, .77f, TextAlignmentOptions.Center, true);
            string power = level < EquipmentCatalog.MaxLevel ?
                $"Đòn thường ×{EquipmentCatalog.DamageAtLevel(selected, level) * EquipmentResonance.DamageFactor(selected):0.00} → ×{EquipmentCatalog.DamageAtLevel(selected, level+1) * EquipmentResonance.DamageFactor(selected):0.00}" :
                "Đã đạt cấp cường hóa tối đa";
            Label(card.transform, "PowerPreview", power, 27f, DienGameUITheme.Teal,
                .05f, .32f, .95f, .54f);
            string cost = level >= EquipmentCatalog.MaxLevel ? "ĐÃ TỐI ĐA" :
                (EquipmentQaSandbox.Active ? "MIỄN PHÍ · TEST" :
                $"{EquipmentInventory.UpgradeGoldCost(selected):N0} vàng  ·  {EquipmentInventory.UpgradeFragmentsCost(selected)} mảnh");
            Label(card.transform, "Cost", cost, 27f, DienGameUITheme.Muted,
                .05f, .05f, .59f, .30f, TextAlignmentOptions.MidlineLeft);
            Button upgrade = Action(card.transform, "Upgrade", "NÂNG CẤP", () =>
                Refresh(EquipmentInventory.TryUpgrade(selected) ?
                    "Cường hóa thành công. Chỉ số áp dụng khi vào trận mới." :
                    "Không đủ tài nguyên hoặc đã đạt cấp tối đa."),
                "ButtonGold", .615f, .05f, .97f, .30f, 28f);
            upgrade.interactable = level < EquipmentCatalog.MaxLevel;
        }

        private void DrawCalibration()
        {
            PlayableClass cls = LobbySessionService.SelectedClass;
            EquipmentDefinition item = EquipmentInventory.Selected(cls);
            if (item == null)
            {
                Label(_body, "Unavailable", "Chưa tìm thấy trang bị hợp lệ cho nhân vật đã chọn.",
                    31f, DienGameUITheme.Muted, .08f, .22f, .92f, .70f);
                return;
            }
            int level = EquipmentInventory.Level(item);
            bool enabled = EquipmentResonance.CanCalibrate(item);
            EquipmentCalibrationMode current = EquipmentResonance.CurrentMode(item);

            GameObject focus = Panel(_body, "ResonanceFocus", "CardSelected", .052f, .590f, .948f, .811f);
            Panel(focus.transform, "IconFrame", "IconPlate", .034f, .23f, .240f, .79f);
            DienGameUITheme.Icon(focus.transform, "CoreIcon", EquipmentEvolutionArt.IconFor(item),
                .063f, .27f, .216f, .73f);
            Label(focus.transform, "CurrentCore", item.Name + $"  +{level}",
                30f, DienGameUITheme.Gold, .275f, .71f, .965f, .94f, TextAlignmentOptions.MidlineLeft, true);
            Label(focus.transform, "OriginClass", EquipmentResonance.FieldTag(item),
                23f, DienGameUITheme.Teal, .275f, .49f, .965f, .73f, TextAlignmentOptions.MidlineLeft, true);
            Label(focus.transform, "Lore", EquipmentResonance.DesignFlavor(item),
                24f, DienGameUITheme.Ivory, .275f, .07f, .961f, .50f, TextAlignmentOptions.MidlineLeft);

            GameObject stage = Panel(_body, "Evolution", "Card", .052f, .455f, .948f, .578f);
            Label(stage.transform, "Stages", "TIẾN HÓA LÕI  •  " + EquipmentResonance.StageName(level),
                27f, DienGameUITheme.Gold, .04f, .56f, .96f, .91f, TextAlignmentOptions.Center, true);
            for (int i = 0; i < 3; i++)
            {
                int needed = i == 0 ? 3 : i == 1 ? 6 : 10;
                string caption = needed == 3 ? "+3  ỔN ÁP" : needed == 6 ? "+6  CỘNG HƯỞNG" : "+10  ĐỒNG BỘ";
                GameObject milestone = Panel(stage.transform, "Stage_" + needed,
                    level >= needed ? "ButtonTeal" : "ButtonGhost", .035f + i * .315f, .10f,
                    .345f + i * .315f, .49f);
                // Preview actual tier artwork. A locked tier is still visible at reduced opacity.
                Image tierPreview = DienGameUITheme.Icon(milestone.transform, "PreviewTier",
                    EquipmentEvolutionArt.IconFor(item, needed), .015f, .07f, .35f, .95f);
                tierPreview.color = level >= needed ? Color.white : new Color(.72f, .78f, .82f, .52f);
                Label(milestone.transform, "Caption", caption, 20f,
                    level >= needed ? DienGameUITheme.Ivory : DienGameUITheme.Muted,
                    .34f, .10f, .98f, .90f, TextAlignmentOptions.Center, true);
            }

            Label(_body, "ModeTitle", enabled ? "TÙY CHỈNH NHỊP CHIẾN ĐẤU" :
                $"ĐỒNG BỘ MỞ KHI CƯỜNG HÓA +{EquipmentResonance.UnlockAtLevel}",
                27f, DienGameUITheme.Gold, .06f, .402f, .94f, .450f, TextAlignmentOptions.Center, true);
            EquipmentCalibrationMode[] options = {
                EquipmentCalibrationMode.Balanced, EquipmentCalibrationMode.Impulse, EquipmentCalibrationMode.Rapid
            };
            for (int i = 0; i < options.Length; i++)
            {
                EquipmentCalibrationMode mode = options[i];
                float top = .402f - i * .106f;
                GameObject row = Panel(_body, "Calibration_" + mode,
                    enabled && current == mode ? "CardSelected" : "Card",
                    .051f, top - .101f, .949f, top - .004f);
                Label(row.transform, "ModeName", EquipmentResonance.ModeName(mode), 27f,
                    enabled && current == mode ? DienGameUITheme.Gold : DienGameUITheme.Ivory,
                    .035f, .49f, .68f, .91f, TextAlignmentOptions.MidlineLeft, true);
                Label(row.transform, "ModeTradeoff", EquipmentResonance.ModeDescription(mode), 22f,
                    DienGameUITheme.Muted, .035f, .05f, .71f, .53f, TextAlignmentOptions.MidlineLeft);
                Button select = Action(row.transform, "EquipCalibration", current == mode ? "ĐANG DÙNG" : "CHỌN",
                    () => Refresh(EquipmentResonance.TrySetMode(item, mode)
                        ? "Đã đồng bộ lõi. Có hiệu lực từ trận tiếp theo."
                        : "Cần mở khóa và nâng trang bị lên +4 trước."),
                    current == mode ? "ButtonGhost" : "ButtonTeal", .74f, .19f, .969f, .81f, 22f);
                select.interactable = enabled && current != mode;
            }
            Label(_body, "Budget", "CÂN BẰNG: 3 cấu hình có DPS đơn mục tiêu xấp xỉ nhau.\n"
                + "Kỹ năng chủ động, khống chế và tỷ lệ gây hiệu ứng không được tăng.",
                22f, DienGameUITheme.Teal, .07f, .018f, .93f, .088f);
        }

        private void DrawPetChips()
        {
            EquipmentDefinition pet = EquipmentInventory.Selected(PlayableClass.Mage);
            if (pet == null)
            {
                Label(_body, "NoPet", "CHƯA CÓ THÚ TRANG BỊ", 31f, DienGameUITheme.Muted,
                    .09f, .36f, .91f, .72f);
                return;
            }

            bool compatible = BiomechPetChips.Supports(pet);
            bool available = BiomechPetChips.Available(pet);
            BiomechPetChipMode equipped = BiomechPetChips.Current(pet);

            GameObject equippedCard = Panel(_body, "PetChipFocus", "CardSelected",
                .053f, .589f, .947f, .814f);
            Panel(equippedCard.transform, "PetIconPlate", "IconPlate", .034f, .18f, .251f, .81f);
            DienGameUITheme.Icon(equippedCard.transform, "PetIcon", EquipmentCatalog.LoadIcon(pet),
                .059f, .25f, .223f, .77f);
            Label(equippedCard.transform, "PetName", pet.Name, 31f,
                DienGameUITheme.Gold, .275f, .66f, .962f, .94f,
                TextAlignmentOptions.MidlineLeft, true);
            Label(equippedCard.transform, "InstalledChip", "CHIP HIỆN TẠI: " + BiomechPetChips.Name(equipped), 26f,
                DienGameUITheme.Teal, .275f, .35f, .962f, .69f,
                TextAlignmentOptions.MidlineLeft, true);
            Label(equippedCard.transform, "ChipNote", !compatible
                ? "Chimera gốc sử dụng bộ kỹ năng nguyên bản, không lắp chip."
                : available ? "Thay chip không mất nguyên liệu. Tác dụng từ trận tiếp theo."
                : "Chip mở từ cấp cường hóa +3 của thú đang trang bị.",
                24f, DienGameUITheme.Ivory, .275f, .06f, .955f, .35f,
                TextAlignmentOptions.MidlineLeft);

            if (!compatible)
            {
                Label(_body, "OriginalChimeraNotice", "GIỮ NGUYÊN CHIMERA GỐC\n"
                    + "M chọn một thú cơ–sinh tại tab THÚ để sử dụng chip kỹ năng.",
                    29f, DienGameUITheme.Muted, .085f, .20f, .915f, .55f);
                return;
            }

            Label(_body, "PetChipHeading", available ? "CẤU HÌNH CHIP CHIẾN ĐẤU" : "CẦN NÂNG THÚ LÊN +3",
                27f, DienGameUITheme.Gold, .07f, .500f, .93f, .573f,
                TextAlignmentOptions.Center, true);

            BiomechPetChipMode[] choices = {
                BiomechPetChipMode.PredatorPulse,
                BiomechPetChipMode.ScatterPulse,
                BiomechPetChipMode.Symbiosis
            };
            for (int i = 0; i < choices.Length; i++)
            {
                BiomechPetChipMode option = choices[i];
                bool selected = available && option == equipped;
                float top = .486f - i * .142f;
                GameObject card = Panel(_body, "Chip_" + option, selected ? "CardSelected" : "Card",
                    .052f, top - .132f, .948f, top);
                Panel(card.transform, "ChipIconPlate", "IconPlate", .019f, .23f, .196f, .80f);
                DienGameUITheme.Icon(card.transform, "SkillIcon", Resources.Load<Sprite>(BiomechPetChips.Icon(option)),
                    .037f, .30f, .178f, .74f);
                Label(card.transform, "ChipName", BiomechPetChips.Name(option), 26f,
                    selected ? DienGameUITheme.Gold : DienGameUITheme.Ivory,
                    .215f, .54f, .765f, .94f, TextAlignmentOptions.MidlineLeft, true);
                Label(card.transform, "ChipDescription", BiomechPetChips.Description(option), 22f,
                    DienGameUITheme.Muted, .215f, .10f, .765f, .58f, TextAlignmentOptions.MidlineLeft);
                Button choose = Action(card.transform, "EquipChip", selected ? "ĐANG LẮP" : "LẮP CHIP",
                    () => Refresh(BiomechPetChips.TryEquip(pet, option)
                        ? "Đã đổi chip thú. Áp dụng từ trận tiếp theo."
                        : "Cần nâng thú đã mở khóa lên +3 trước."),
                    selected ? "ButtonGhost" : "ButtonTeal", .788f, .18f, .968f, .83f, 22f);
                choose.interactable = available && !selected;
            }
            Button unequip = Action(_body, "RemoveChip", equipped == BiomechPetChipMode.Empty
                ? "CHƯA LẮP CHIP" : "THÁO CHIP · PHỤC HỒI ĐÒN THƯỜNG",
                () => Refresh(BiomechPetChips.TryEquip(pet, BiomechPetChipMode.Empty)
                    ? "Đã tháo chip thú. Sát thương thường trở lại mặc định."
                    : "Chưa thể thay chip thú này."),
                "ButtonGhost", .125f, .015f, .875f, .070f, 21f);
            unequip.interactable = available && equipped != BiomechPetChipMode.Empty;
        }

        private void DrawArmor()
        {
            Label(_body, "ArmorIntro", "TRANG BỊ DÙNG CHUNG", 37f, DienGameUITheme.Gold,
                .05f, .71f, .95f, .80f, TextAlignmentOptions.Center, true);
            Sprite icon = Resources.Load<Sprite>("World/UI/GearPickup");
            for (int i = 1; i <= 2; i++)
            {
                int slot = i;
                float bottom = i == 1 ? .40f : .085f;
                int level = WorldInventory.GetLevel(slot);
                GameObject card = Panel(_body, "SharedGear_" + slot, "Card", .05f, bottom, .95f, bottom + .276f);
                Panel(card.transform, "IconFrame", "IconPlate", .05f, .36f, .23f, .91f);
                DienGameUITheme.Icon(card.transform, "GearIcon", icon, .075f, .43f, .205f, .84f);
                Label(card.transform, "Name", WorldInventory.GearNames[slot] + $"  +{level}/5",
                    36f, DienGameUITheme.Ivory, .26f, .64f, .95f, .92f,
                    TextAlignmentOptions.MidlineLeft, true);
                Label(card.transform, "Effect", WorldInventory.GearDescriptions[slot],
                    29f, DienGameUITheme.Muted, .26f, .35f, .94f, .66f, TextAlignmentOptions.MidlineLeft);
                Label(card.transform, "Cost", level < 5 ?
                    (EquipmentQaSandbox.Active ? "MIỄN PHÍ · TEST" :
                    $"{WorldInventory.UpgradeGoldCost(slot):N0} vàng · {WorldInventory.UpgradeFragmentCost(slot)} mảnh") :
                    "Đã nâng cấp tối đa", 28f, DienGameUITheme.Gold, .05f, .06f, .59f, .31f,
                    TextAlignmentOptions.MidlineLeft);
                Button up = Action(card.transform, "Upgrade", "NÂNG CẤP", () =>
                    Refresh(WorldInventory.TryUpgrade(slot) ? "Cường hóa thành công." :
                        "Thiếu tài nguyên hoặc đã tối đa."), "ButtonGold", .61f, .06f, .96f, .31f, 28f);
                up.interactable = level < 5;
            }
        }

        private void DrawQuest()
        {
            int completed = MetaProgressService.HighestCompletedStage;
            int kills = WorldInventory.Kills;
            GameObject journey = Panel(_body, "JourneyCard", "CardSelected", .05f, .66f, .95f, .925f);
            Label(journey.transform, "JourneyTitle", "HÀNH TRÌNH KHÁM PHÁ", 39f, DienGameUITheme.Gold,
                .06f, .70f, .94f, .94f, TextAlignmentOptions.Center, true);
            Label(journey.transform, "JourneySubtitle", $"ĐẢO HOÀN THÀNH   {completed}/5", 34f,
                DienGameUITheme.Ivory, .06f, .39f, .94f, .67f);
            GameObject track = Panel(journey.transform, "ProgressTrack", "Track", .10f, .15f, .90f, .30f);
            Panel(track.transform, "ProgressFill", "FillTeal", .015f, .12f,
                Mathf.Max(.02f, Mathf.Clamp01(completed / 5f) * .985f), .88f);

            GameObject quest = Panel(_body, "KillQuestCard", "Card", .05f, .295f, .95f, .632f);
            Label(quest.transform, "QuestLabel", "THỬ THÁCH THỢ SĂN", 38f, DienGameUITheme.Ivory,
                .07f, .73f, .93f, .93f, TextAlignmentOptions.Center, true);
            Label(quest.transform, "Objective", "Tiêu diệt 20 quái ở bất kỳ đảo nào", 30f,
                DienGameUITheme.Muted, .06f, .49f, .94f, .71f);
            Label(quest.transform, "Progress", $"TIẾN ĐỘ    {Mathf.Min(kills, 20)}/20", 36f,
                DienGameUITheme.Teal, .06f, .25f, .94f, .50f, TextAlignmentOptions.Center, true);
            GameObject questTrack = Panel(quest.transform, "QuestTrack", "Track", .10f, .10f, .90f, .21f);
            Panel(questTrack.transform, "QuestFill", "FillGold", .015f, .10f,
                Mathf.Max(.025f, Mathf.Clamp01(kills / 20f) * .985f), .90f);

            GameObject reward = Panel(_body, "RewardCard", "Panel", .05f, .113f, .95f, .265f);
            Label(reward.transform, "RewardText", "THƯỞNG: 400 VÀNG + 4 TINH THỂ", 28f,
                DienGameUITheme.Gold, .045f, .10f, .955f, .91f, TextAlignmentOptions.Center, true);
            Button claim = Action(_body, "ClaimQuest", WorldInventory.KillQuestClaimed ?
                "ĐÃ NHẬN THƯỞNG" : "NHẬN THƯỞNG", () =>
                Refresh(WorldInventory.TryClaimKillQuest() ? "Đã nhận thưởng nhiệm vụ!" :
                    "Chưa đủ 20 quái hoặc đã nhận."), "ButtonGold", .19f, .025f, .81f, .104f, 36f);
            claim.interactable = kills >= 20 && !WorldInventory.KillQuestClaimed;
        }

        private void DrawShop()
        {
            Label(_body, "ShopBalance", $"CỬA HÀNG CƠ SINH     •     {MetaProgressService.Gold:N0} VÀNG",
                34f, DienGameUITheme.Gold, .055f, .88f, .945f, .976f, TextAlignmentOptions.Center, true);
            ShopItem("TinhThes", "MẢNH TINH THỂ", "Gói 3 mảnh dùng để cường hóa", "200 VÀNG", .53f,
                Resources.Load<Sprite>("World/UI/CrystalPickup"), () =>
                    Refresh(WorldInventory.BuyFragments(3, 200) ? "Đã mua 3 mảnh tinh thể." : "Không đủ vàng."));
            ShopItem("Ruongs", "RƯƠNG TRANG BỊ", "Nhận 3 mảnh + 1 rương", "450 VÀNG", .235f,
                Resources.Load<Sprite>("World/UI/GearPickup"), () =>
                    Refresh(WorldInventory.BuyChest() ? "Đã mua rương trang bị." : "Không đủ vàng."));
            Label(_body, "ShopFooter", "Vật phẩm được lưu trong kho. Cường hóa tại mục TRANG BỊ.",
                29f, DienGameUITheme.Muted, .07f, .035f, .93f, .195f);
        }

        private void ShopItem(string id, string title, string description, string cost, float bottom,
            Sprite sprite, Action buy)
        {
            GameObject row = Panel(_body, id, "Card", .052f, bottom, .948f, bottom + .258f);
            Panel(row.transform, "IconFrame", "IconPlate", .035f, .33f, .22f, .88f);
            DienGameUITheme.Icon(row.transform, "Icon", sprite, .062f, .40f, .193f, .81f);
            Label(row.transform, "Title", title, 32f, DienGameUITheme.Ivory,
                .245f, .64f, .95f, .90f, TextAlignmentOptions.MidlineLeft, true);
            Label(row.transform, "Description", description, 26f, DienGameUITheme.Muted,
                .245f, .34f, .94f, .67f, TextAlignmentOptions.MidlineLeft);
            Label(row.transform, "Cost", cost, 27f, DienGameUITheme.Gold,
                .052f, .06f, .57f, .30f, TextAlignmentOptions.MidlineLeft, true);
            Action(row.transform, "Buy", "MUA", buy, "ButtonTeal", .63f, .07f, .955f, .31f, 31f);
        }

        private void DrawSkills()
        {
            string who = LobbySessionService.SelectedClass == PlayableClass.Mage ? "PHÁP SƯ" :
                LobbySessionService.SelectedClass == PlayableClass.Assassin ? "SÁT THỦ" :
                LobbySessionService.SelectedClass == PlayableClass.Warrior ? "ĐẤU SĨ" : "XẠ THỦ";
            Label(_body, "SkillsHeader", "BỘ KỸ NĂNG  ·  " + who, 37f,
                DienGameUITheme.Gold, .035f, .90f, .965f, .982f, TextAlignmentOptions.Center, true);
            string[] names = LobbySessionService.SelectedClass == PlayableClass.Mage ?
                new[] { "Tinh Châm Phân Liệt", "Trọng Trường Dị Giới", "Trảo Liệt Hợp Kim", "Pháo Sinh Thể",
                    "Song Trùng Oanh Kích", "Cộng Hưởng Săn Mồi", "Màn Chắn Song Thể" } :
                LobbySessionService.SelectedClass == PlayableClass.Assassin ?
                new[] { "Huyết Ảnh Trảm", "Tàn Ảnh", "Phi Nhận", "Ảnh Phân Thân",
                    "Tử Ấn", "Đao Ảnh Hộ Thân", "Hồn Trụ Dẫn Ảnh" } :
                LobbySessionService.SelectedClass == PlayableClass.Warrior ?
                new[] { "Trục Lực Cộng Hưởng", "Giáp Phản Xung", "Dư Chấn Trường X",
                    "Buồng Nhiệt Tăng Áp", "Phá Kết Cấu", "Nhịp Kiếm Đồng Bộ", "Tái Tạo Sinh Cơ" } :
                new[] { "Đạn Xuyên Thấu", "Đạn Sấm Sét", "Drone Chiến Đấu", "Không Kích",
                    "Đạn Nảy", "Cuồng Nộ", "Khiên Chiến Thuật" };
            for (int i = 0; i < names.Length; i++)
            {
                float top = .885f - i * .109f;
                GameObject row = Panel(_body, "Skill_" + i, "Card", .048f, top - .103f, .952f, top);
                Label(row.transform, "Number", (i + 1).ToString("00"), 41f,
                    DienGameUITheme.Gold, .025f, .15f, .14f, .85f, TextAlignmentOptions.Center, true);
                Label(row.transform, "Name", names[i], 33f,
                    DienGameUITheme.Ivory, .18f, .29f, .94f, .85f, TextAlignmentOptions.MidlineLeft, true);
                Label(row.transform, "Level", "CẤP 1–5 · NÂNG TRONG TRẬN", 22f,
                    DienGameUITheme.Muted, .18f, .06f, .94f, .33f, TextAlignmentOptions.MidlineLeft);
            }
            Label(_body, "SkillsNote", "Cấp 5 mở hiệu ứng đột phá cho kỹ năng.", 27f,
                DienGameUITheme.Gold, .05f, .015f, .95f, .104f);
        }
    }
}
