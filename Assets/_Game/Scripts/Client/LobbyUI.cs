using System.Collections.Generic;
using DienGame.Gameplay.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DienGame.Client
{
    /// <summary>
    /// Art-backed lobby/world-map UI.
    /// Sprite references are serialized directly into Lobby.unity by LobbySceneBuilder.
    /// Runtime does not use Resources.Load and does not require manual Inspector wiring.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class LobbyUI : MonoBehaviour
    {
        private static readonly string[] StageNames =
        {
            "",
            "ĐẢO 1 - LÀNG PHÙ SA",
            "ĐẢO 2 - RỪNG THIÊN MỘC",
            "ĐẢO 3 - CỔ TỰ CÁT VÀNG",
            "ĐẢO 4 - HỎA THÀNH LONG MẠCH",
            "ĐẢO 5 - CUNG BĂNG VÂN THIÊN",
        };

        private static readonly string[] StageDescriptions =
        {
            "",
            "Làng ven sông mang sắc Việt, nơi bắt đầu hành trình khống chế dị biến.",
            "Đền cổ và đại thụ linh mộc. Tránh những gò cây chắn đường của thú rừng.",
            "Cổ tự đá vàng giữa cồn cát, nơi những tàn tích che khuất lối đi.",
            "Dung nham len vào thành đá cổ. Vượt qua bãi nham thạch và quái biến dị.",
            "Điện băng trên mây tuyết, nơi tinh thể giá lạnh cản bước chiến binh.",
        };

        private static readonly Vector2[] IslandPositions =
        {
            Vector2.zero,
            new(-280f, -355f),
            new(-290f, -52f),
            new(20f, 245f),
            new(300f, 100f),
            new(270f, -285f),
        };

        private static readonly Vector2[] IslandSizes =
        {
            Vector2.zero,
            new(390f, 250f),
            new(355f, 230f),
            new(360f, 245f),
            new(355f, 235f),
            new(390f, 245f),
        };

        private static readonly Color Navy = new(0.025f, 0.075f, 0.13f, 0.94f);
        private static readonly Color NavySoft = new(0.03f, 0.10f, 0.18f, 0.88f);
        private static readonly Color Gold = new(1f, 0.68f, 0.12f, 1f);
        private static readonly Color GoldSoft = new(1f, 0.84f, 0.38f, 1f);
        private static readonly Color Muted = new(0.76f, 0.82f, 0.90f, 1f);
        // Playtest: 5 islands are selectable immediately, regardless of old save progress.
        private const bool UnlockAllIslandsForTest = true;
        private static readonly Color LockedTint = new(0.38f, 0.42f, 0.48f, 0.78f);
        private static readonly string[] ShortIslandNames =
        {
            "", "PHÙ SA", "THIÊN MỘC", "CÁT VÀNG", "HỎA THÀNH", "VÂN THIÊN"
        };

        [Header("World Art")]
        [SerializeField] private Sprite backgroundSprite;
        [SerializeField] private Sprite[] islandSprites = new Sprite[5];

        [Header("UI Art")]
        [SerializeField] private Sprite panelStageInfoSprite;
        [SerializeField] private Sprite buttonPlaySprite;
        [SerializeField] private Sprite buttonLockedSprite;
        [SerializeField] private Sprite iconMapSprite;
        [SerializeField] private Sprite iconEquipmentSprite;
        [SerializeField] private Sprite iconSkillsSprite;
        [SerializeField] private Sprite iconMissionSprite;
        [SerializeField] private Sprite iconShopSprite;
        [SerializeField] private Sprite iconCoinSprite;
        [SerializeField] private Sprite iconGemSprite;
        [SerializeField] private Sprite starFullSprite;
        [SerializeField] private Sprite starEmptySprite;

        private readonly Dictionary<int, Image> _islandImages = new();
        private readonly Dictionary<int, GameObject> _stageSelectionGlows = new();

        private RectTransform _root;
        private TMP_Text _goldText;
        private TMP_Text _gemText;
        private Image _stagePreview;
        private TMP_Text _stageTitle;
        private TMP_Text _stageDescription;
        private TMP_Text _stageStatus;
        private TMP_Text _stageReward;
        private TMP_Text _toast;
        private RectTransform _starRow;
        private Button _playButton;
        private Image _playButtonImage;
        private TMP_Text _playButtonLabel;
        private int _selectedStage = 1;

        // Character selection is lobby-only. Selection is applied when the user confirms.
        private TMP_Text _profileClassName;
        private GameObject _characterSelectionOverlay;
        private GameObject _audioSettingsPanel;
        private Button _marksmanClassButton;
        private Button _assassinClassButton;
        private Button _mageClassButton;
        private Button _warriorClassButton;
        private TMP_Text _marksmanClassStatus;
        private TMP_Text _assassinClassStatus;
        private TMP_Text _mageClassStatus;
        private TMP_Text _warriorClassStatus;
        private PlayableClass _pendingClass;

        public void ConfigureArt(
            Sprite background,
            Sprite[] islands,
            Sprite stageInfoPanel,
            Sprite playButton,
            Sprite lockedButton,
            Sprite mapIcon,
            Sprite equipmentIcon,
            Sprite skillsIcon,
            Sprite missionIcon,
            Sprite shopIcon,
            Sprite coinIcon,
            Sprite gemIcon,
            Sprite fullStar,
            Sprite emptyStar)
        {
            backgroundSprite = background;
            islandSprites = islands != null ? islands : new Sprite[5];
            panelStageInfoSprite = stageInfoPanel;
            buttonPlaySprite = playButton;
            buttonLockedSprite = lockedButton;
            iconMapSprite = mapIcon;
            iconEquipmentSprite = equipmentIcon;
            iconSkillsSprite = skillsIcon;
            iconMissionSprite = missionIcon;
            iconShopSprite = shopIcon;
            iconCoinSprite = coinIcon;
            iconGemSprite = gemIcon;
            starFullSprite = fullStar;
            starEmptySprite = emptyStar;
        }

        private void Awake()
        {
            // The illustrated island sprites in the pre-existing Lobby scene were bound to
            // older small/dark thumbnails. Prefer the new Resources sprites at runtime.
            // A scene rebuild is unnecessary, and references still work on mobile builds.
            LoadLatestWorldArt();
            Build();
            SelectStage(1);
            RefreshMeta();
        }

        private void LoadLatestWorldArt()
        {
            string[] names =
            {
                "Island_01_DongBang", "Island_02_RungTham", "Island_03_TanTich",
                "Island_04_NuiLua", "Island_05_CuDiem"
            };
            if (islandSprites == null || islandSprites.Length < 5)
                islandSprites = new Sprite[5];

            for (int i = 0; i < 5; i++)
            {
                Sprite updated = Resources.Load<Sprite>("Client/" + names[i]);
                if (updated != null) islandSprites[i] = updated;
            }
            Sprite bg = Resources.Load<Sprite>("Client/BG_WorldMap");
            if (bg != null) backgroundSprite = bg;
        }

        private void Build()
        {
            ClearChildren(transform);

            GameObject rootObject = CreateUIObject("ClientRoot", transform);
            _root = rootObject.GetComponent<RectTransform>();
            Stretch(_root);

            CreateBackground(_root);
            CreateTopBar(_root);
            CreateWorldMap(_root);
            CreateStageInfo(_root);
            CreateBottomNavigation(_root);
            CreateToast(_root);
            CreateCharacterSelectionOverlay(_root);
            _audioSettingsPanel = DienGameAudioSettingsPanel.Create(_root, CloseLobbyAudioSettings);
            _audioSettingsPanel.SetActive(false);
            RefreshCharacterProfile();
        }

        private void CreateBackground(Transform parent)
        {
            GameObject obj = CreateUIObject("WorldBackground", parent);
            RectTransform rect = obj.GetComponent<RectTransform>();
            Stretch(rect);

            Image image = obj.AddComponent<Image>();
            image.sprite = backgroundSprite;
            image.color = Color.white;
            image.raycastTarget = false;

            if (image.sprite == null)
            {
                image.color = new Color(0.02f, 0.14f, 0.25f, 1f);
                return;
            }

            AspectRatioFitter fitter = obj.AddComponent<AspectRatioFitter>();
            fitter.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
            fitter.aspectRatio = image.sprite.rect.width / image.sprite.rect.height;
        }

        private void CreateTopBar(Transform parent)
        {
            GameObject top = CreatePanel(parent, "TopBar", new Color(0.015f, 0.035f, 0.075f, 0.93f));
            SetAnchoredStretch(top.GetComponent<RectTransform>(), new Vector2(0f, 0.905f), Vector2.one);

            DienGameUITheme.Skin(top.GetComponent<Image>(), "TopBar");
            AddBottomLine(top, Gold, 2f);

            GameObject profile = CreatePanel(top.transform, "Profile", new Color(0.02f, 0.08f, 0.14f, 0.95f));
            // Responsive in the real top-bar width: avoid four absolute positions overflowing narrow devices.
            SetAnchoredStretch(profile.GetComponent<RectTransform>(),
                new Vector2(.012f, .18f), new Vector2(.303f, .84f));
            DienGameUITheme.Skin(profile.GetComponent<Image>(), "Pill");

            // Tapping the existing profile opens class selection; no new navbar tab.
            Button profileButton = profile.AddComponent<Button>();
            profileButton.targetGraphic = profile.GetComponent<Image>();
            profileButton.onClick.AddListener(OpenCharacterSelection);

            TMP_Text className = CreateText(profile.transform, string.Empty, 32f, TextAlignmentOptions.Left);
            _profileClassName = className;
            SetAnchoredStretch(className.rectTransform, new Vector2(.07f, .50f), new Vector2(.93f, .89f));
            className.fontStyle = FontStyles.Bold;

            TMP_Text level = CreateText(profile.transform, "Lv.12", 18f, TextAlignmentOptions.Left);
            SetAnchoredStretch(level.rectTransform, new Vector2(.07f, .14f), new Vector2(.27f, .48f));
            level.color = GoldSoft;

            GameObject xpTrack = CreatePanel(profile.transform, "ExpTrack", new Color(.12f, .18f, .25f, .9f));
            SetAnchoredStretch((RectTransform)xpTrack.transform,
                new Vector2(.30f, .245f), new Vector2(.93f, .35f));
            GameObject xpFill = CreatePanel(xpTrack.transform, "ExpFill", GoldSoft);
            RectTransform xpFillRect = (RectTransform)xpFill.transform;
            xpFillRect.anchorMin = Vector2.zero;
            xpFillRect.anchorMax = new Vector2(.56f, 1f);
            xpFillRect.offsetMin = xpFillRect.offsetMax = Vector2.zero;

            CreateResourcePill(top.transform, "Energy", null, "120/120", .319f, .535f);
            GameObject goldPill = CreateResourcePill(top.transform, "Gold", iconCoinSprite, string.Empty, .545f, .758f);
            _goldText = goldPill.GetComponentInChildren<TMP_Text>();
            _gemText = CreateResourcePill(top.transform, "Gem", iconGemSprite, "0", .769f, .986f).GetComponentInChildren<TMP_Text>();
        }

        private GameObject CreateResourcePill(Transform parent, string name, Sprite iconSprite, string value, float xMin, float xMax)
        {
            GameObject pill = CreatePanel(parent, name, new Color(0.03f, 0.07f, 0.12f, 0.94f));
            SetAnchoredStretch(pill.GetComponent<RectTransform>(),
                new Vector2(xMin, .295f), new Vector2(xMax, .705f));
            DienGameUITheme.Skin(pill.GetComponent<Image>(), "Pill");

            if (iconSprite != null)
            {
                Image icon = CreateImage(pill.transform, "Icon", iconSprite, true);
                SetAnchoredStretch(icon.rectTransform, new Vector2(.055f, .18f), new Vector2(.29f, .82f));
            }
            else
            {
                TMP_Text energy = CreateText(pill.transform, "NL", 25f, TextAlignmentOptions.Center);
                SetAnchoredStretch(energy.rectTransform, new Vector2(.055f, .18f), new Vector2(.29f, .82f));
                energy.color = Gold;
            }

            TMP_Text label = CreateText(pill.transform, value, 28f, TextAlignmentOptions.Center);
            SetAnchoredStretch(label.rectTransform, new Vector2(.30f, .08f), new Vector2(.96f, .92f));
            label.fontStyle = FontStyles.Bold;
            return pill;
        }

        private void CreateWorldMap(Transform parent)
        {
            GameObject map = CreateUIObject("WorldMap", parent);
            RectTransform mapRect = map.GetComponent<RectTransform>();
            SetAnchoredStretch(mapRect, new Vector2(0f, 0.31f), new Vector2(1f, 0.905f));

            GameObject mapHeading = CreatePanel(map.transform, "WorldTitlePlate", new Color(0.018f, 0.055f, 0.10f, 0.88f));
            RectTransform mapHeadingRect = mapHeading.GetComponent<RectTransform>();
            SetRect(mapHeadingRect, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0f, -23f), new Vector2(410f, 68f), new Vector2(0.5f, 1f));
            mapHeading.GetComponent<Image>().raycastTarget = false;
            DienGameUITheme.Skin(mapHeading.GetComponent<Image>(), "Header");
            TMP_Text heading = CreateText(mapHeading.transform, "NGŨ ĐẢO HUYỀN GIỚI", 34f, TextAlignmentOptions.Center);
            heading.fontStyle = FontStyles.Bold;
            heading.color = GoldSoft;
            Stretch(heading.rectTransform);

            CreateRoute(map.transform, IslandPositions[1], IslandPositions[2]);
            CreateRoute(map.transform, IslandPositions[2], IslandPositions[3]);
            CreateRoute(map.transform, IslandPositions[3], IslandPositions[4]);
            CreateRoute(map.transform, IslandPositions[4], IslandPositions[5]);

            for (int stage = 1; stage <= MetaProgressService.MaxStageCount; stage++)
            {
                CreateIslandNode(map.transform, stage);
            }

            Button checkIn = CreateTextButton(map.transform, "DailyCheckIn", "ĐIỂM DANH", OnCheckInPressed, NavySoft);
            SetRect(checkIn.GetComponent<RectTransform>(), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(24f, -28f), new Vector2(235f, 68f), new Vector2(0f, 1f));

            // A separate settings entry in the client; does not steal bottom navigation space.
            Button audioSettings = CreateTextButton(map.transform, "AudioSettings", "CÀI ĐẶT", OpenLobbyAudioSettings, NavySoft);
            SetRect(audioSettings.GetComponent<RectTransform>(), new Vector2(1f, 1f), new Vector2(1f, 1f),
                new Vector2(-24f, -28f), new Vector2(202f, 68f), new Vector2(1f, 1f));
        }

        private void CreateIslandNode(Transform parent, int stage)
        {
            GameObject glow = CreateUIObject($"Stage_{stage}_Glow", parent);
            RectTransform glowRect = glow.GetComponent<RectTransform>();
            SetRect(glowRect, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), IslandPositions[stage], IslandSizes[stage] * 1.12f, new Vector2(0.5f, 0.5f));
            Image glowImage = glow.AddComponent<Image>();
            glowImage.sprite = GetIslandSprite(stage);
            glowImage.preserveAspect = true;
            glowImage.color = new Color(1f, 0.83f, 0.40f, 0.32f);
            glowImage.raycastTarget = false;
            Shadow shadow = glow.AddComponent<Shadow>();
            shadow.effectColor = new Color(1f, 0.72f, 0.12f, 0.75f);
            shadow.effectDistance = Vector2.zero;
            _stageSelectionGlows[stage] = glow;

            GameObject islandObj = CreateUIObject($"Stage_{stage}", parent);
            RectTransform rect = islandObj.GetComponent<RectTransform>();
            SetRect(rect, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), IslandPositions[stage], IslandSizes[stage], new Vector2(0.5f, 0.5f));

            Image image = islandObj.AddComponent<Image>();
            image.sprite = GetIslandSprite(stage);
            image.preserveAspect = true;
            image.color = IsStagePlayable(stage) ? Color.white : LockedTint;
            _islandImages[stage] = image;

            Button button = islandObj.AddComponent<Button>();
            button.targetGraphic = image;
            int capturedStage = stage;
            button.onClick.AddListener(() => SelectStage(capturedStage));

            GameObject labelPanel = CreatePanel(parent, $"Stage_{stage}_Label", new Color(0.032f, 0.075f, 0.14f, 0.96f));
            SetRect(labelPanel.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                IslandPositions[stage] + new Vector2(0f, -IslandSizes[stage].y * .43f),
                new Vector2(300f, 62f), new Vector2(.5f, .5f));
            DienGameUITheme.Skin(labelPanel.GetComponent<Image>(), "Pill");

            TMP_Text name = CreateText(labelPanel.transform,
                $"ĐẢO {stage} · {ShortIslandNames[stage]}", 27f, TextAlignmentOptions.Center);
            Stretch(name.rectTransform);
            name.fontStyle = FontStyles.Bold;
            name.color = GoldSoft;
            name.enableWordWrapping = false;
            name.fontSizeMin = 21f;
            name.overflowMode = TextOverflowModes.Ellipsis;

            // Test unlock means no KHÓA overlay and no dark tint on islands.
            if (!IsStagePlayable(stage) && stage > 1)
            {
                TMP_Text locked = CreateText(parent, "KHÓA", 22f, TextAlignmentOptions.Center);
                SetRect(locked.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                    IslandPositions[stage], new Vector2(120f, 46f), new Vector2(0.5f, 0.5f));
                locked.fontStyle = FontStyles.Bold;
            }
        }

        private void CreateStageInfo(Transform parent)
        {
            // Anchored subregions prevent reward text and button from covering one another.
            GameObject panel = DienGameUITheme.Panel(parent, "StageInfo", "Panel");
            SetAnchoredStretch(panel.GetComponent<RectTransform>(),
                new Vector2(.028f, .108f), new Vector2(.972f, .318f));
            Image image = panel.GetComponent<Image>();
            image.raycastTarget = false;

            GameObject imagePlate = DienGameUITheme.Panel(panel.transform, "StageArtFrame", "IconPlate");
            DienGameUITheme.Rect((RectTransform)imagePlate.transform, .028f, .175f, .326f, .91f);
            _stagePreview = DienGameUITheme.Icon(panel.transform, "IslandPreview", GetIslandSprite(1),
                .048f, .23f, .304f, .86f);

            _stageTitle = DienGameUITheme.Text(panel.transform, "StageName", "", 32f,
                DienGameUITheme.Gold, .35f, .745f, .961f, .935f,
                TextAlignmentOptions.MidlineLeft, true);
            _stageDescription = DienGameUITheme.Text(panel.transform, "StageDescription", "", 27f,
                DienGameUITheme.Ivory, .355f, .465f, .954f, .730f,
                TextAlignmentOptions.TopLeft);
            _stageStatus = DienGameUITheme.Text(panel.transform, "StageStatus", "", 25f,
                DienGameUITheme.Teal, .355f, .308f, .954f, .460f,
                TextAlignmentOptions.MidlineLeft, true);
            _stageReward = DienGameUITheme.Text(panel.transform, "StageRewards", "", 23f,
                DienGameUITheme.Muted, .355f, .068f, .690f, .305f,
                TextAlignmentOptions.MidlineLeft);

            _starRow = new GameObject("Stars", typeof(RectTransform)).GetComponent<RectTransform>();
            _starRow.SetParent(panel.transform, false);
            DienGameUITheme.Rect(_starRow, .067f, .035f, .290f, .19f);
            _playButton = DienGameUITheme.Button(panel.transform, "PlayStage", "VÀO ẢI",
                OnPlayPressed, "ButtonGold", .710f, .07f, .975f, .298f, 33f);
            _playButtonImage = _playButton.GetComponent<Image>();
            _playButtonLabel = _playButton.GetComponentInChildren<TMP_Text>();
        }

        private void CreateBottomNavigation(Transform parent)
        {
            GameObject nav = CreatePanel(parent, "BottomNavigation", new Color(0.015f, 0.035f, 0.075f, 0.98f));
            SetAnchoredStretch(nav.GetComponent<RectTransform>(), Vector2.zero, new Vector2(1f, 0.105f));
            DienGameUITheme.Skin(nav.GetComponent<Image>(), "TopBar");
            AddTopLine(nav, Gold, 2f);

            CreateNavItem(nav.transform, "BẢN ĐỒ", iconMapSprite, 0, true);
            CreateNavItem(nav.transform, "TRANG BỊ", iconEquipmentSprite, 1, false);
            CreateNavItem(nav.transform, "KỸ NĂNG", iconSkillsSprite, 2, false);
            CreateNavItem(nav.transform, "NHIỆM VỤ", iconMissionSprite, 3, false);
            CreateNavItem(nav.transform, "CỬA HÀNG", iconShopSprite, 4, false);
        }

        private void CreateNavItem(Transform parent, string label, Sprite iconSprite, int index, bool selected)
        {
            GameObject buttonObj = CreateUIObject($"Nav_{index}", parent);
            RectTransform rect = buttonObj.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(index / 5f, 0f);
            rect.anchorMax = new Vector2((index + 1f) / 5f, 1f);
            rect.offsetMin = new Vector2(3f, 6f);
            rect.offsetMax = new Vector2(-3f, -6f);

            Image bg = buttonObj.AddComponent<Image>();
            bg.color = selected ? Color.white : new Color(1f,1f,1f,.75f);
            DienGameUITheme.Skin(bg, selected ? "CardSelected" : "Panel");

            Button button = buttonObj.AddComponent<Button>();
            button.targetGraphic = bg;
            button.onClick.AddListener(() => OnNavigationPressed(label, selected));

            Image icon = CreateImage(buttonObj.transform, "Icon", iconSprite, true);
            SetAnchoredStretch(icon.rectTransform, new Vector2(.20f, .35f), new Vector2(.80f, .92f));
            icon.color = selected ? Color.white : new Color(0.78f, 0.82f, 0.9f, 0.9f);

            TMP_Text text = CreateText(buttonObj.transform, label, 25f, TextAlignmentOptions.Center);
            SetAnchoredStretch(text.rectTransform, new Vector2(.035f, .045f), new Vector2(.965f, .325f));
            text.fontStyle = FontStyles.Bold;
            text.enableWordWrapping = false;
            text.fontSizeMin = 18f;
            text.fontSizeMax = 25f;
            text.overflowMode = TextOverflowModes.Truncate;
            text.color = selected ? GoldSoft : Color.white;
        }

        private void CreateCharacterSelectionOverlay(Transform parent)
        {
            _characterSelectionOverlay = CreatePanel(parent, "CharacterSelectionOverlay", DienGameUITheme.Veil);
            Stretch((RectTransform)_characterSelectionOverlay.transform);
            Image dim = _characterSelectionOverlay.GetComponent<Image>();
            dim.raycastTarget = true;
            // Decorative veil remains full screen while the interactive area avoids the notch.
            GameObject safe = new GameObject("SafeAreaContent", typeof(RectTransform));
            safe.transform.SetParent(_characterSelectionOverlay.transform, false);
            DienGameUITheme.Rect((RectTransform)safe.transform, 0f, 0f, 1f, 1f);
            safe.AddComponent<DienGameSafeAreaRoot>();
            GameObject frame = DienGameUITheme.Panel(safe.transform,
                "CharacterSelectionFrame", "Panel");
            DienGameUITheme.Rect((RectTransform)frame.transform, .035f, .055f, .965f, .965f);
            GameObject heading = DienGameUITheme.Panel(frame.transform, "Heading", "Header");
            DienGameUITheme.Rect((RectTransform)heading.transform, .025f, .865f, .975f, .962f);
            DienGameUITheme.Text(heading.transform, "Title", "CHỌN NHÂN VẬT", 51f,
                DienGameUITheme.Gold, .06f, .18f, .94f, .88f, TextAlignmentOptions.Center, true);
            DienGameUITheme.Text(frame.transform, "Hint", "MỖI NHÂN VẬT CÓ LỐI CHIẾN ĐẤU RIÊNG",
                29f, DienGameUITheme.Muted, .09f, .800f, .91f, .864f);

            CreateCharacterCard(PlayableClass.Marksman, "01", "XẠ THỦ", "SÚNG HIỆN ĐẠI",
                "Tầm xa · Đạn nguyên tố · Drone", Vector2.zero,
                out _marksmanClassButton, out _marksmanClassStatus);
            CreateCharacterCard(PlayableClass.Assassin, "02", "SÁT THỦ", "SONG ĐAO / LIỀM",
                "Cận chiến · Di chuyển linh hoạt", Vector2.zero,
                out _assassinClassButton, out _assassinClassStatus);
            CreateCharacterCard(PlayableClass.Mage, "03", "PHÁP SƯ", "TRƯỢNG / THÚ CƠ SINH",
                "Triệu hồi · Điều khiển song song", Vector2.zero,
                out _mageClassButton, out _mageClassStatus);
            CreateCharacterCard(PlayableClass.Warrior, "04", "ĐẤU SĨ", "KIẾM HỢP KIM / KIẾM X",
                "Cận chiến · Combo · Giáp phản xung", Vector2.zero,
                out _warriorClassButton, out _warriorClassStatus);

            DienGameUITheme.Button(frame.transform, "ConfirmClass", "XÁC NHẬN NHÂN VẬT",
                ConfirmCharacterSelection, "ButtonGold", .13f, .113f, .87f, .183f, 39f);
            DienGameUITheme.Button(frame.transform, "CancelClass", "QUAY LẠI",
                CancelCharacterSelection, "ButtonGhost", .235f, .038f, .765f, .094f, 32f);
            _characterSelectionOverlay.SetActive(false);
        }

        private void CreateCharacterCard(
            PlayableClass playerClass,
            string number,
            string className,
            string weapon,
            string description,
            Vector2 unusedPosition,
            out Button cardButton,
            out TMP_Text status)
        {
            // Positions are normalized rather than absolute pixels. The old card used
            // top-left text pivots with overlapping 40px lines on narrow screens.
            Transform frame = _characterSelectionOverlay.transform.Find("SafeAreaContent/CharacterSelectionFrame");
            // Four independent cards inside the safe-area panel, with room for buttons.
            float bottom = playerClass == PlayableClass.Marksman ? .641f :
                playerClass == PlayableClass.Assassin ? .492f :
                playerClass == PlayableClass.Mage ? .343f : .194f;
            GameObject card = DienGameUITheme.Panel(frame, "Card_" + className, "Card");
            DienGameUITheme.Rect((RectTransform)card.transform, .05f, bottom, .95f, bottom + .137f);
            Image background = card.GetComponent<Image>();
            background.raycastTarget = true;
            cardButton = card.AddComponent<Button>();
            cardButton.targetGraphic = background;
            cardButton.onClick.AddListener(() => ChoosePendingCharacter(playerClass));
            Sprite icon = playerClass == PlayableClass.Marksman ?
                Resources.Load<Sprite>("Equipment/Icons/Marksman_Basic") :
                playerClass == PlayableClass.Assassin ?
                Resources.Load<Sprite>("Equipment/Icons/Assassin_TwinDaggers") :
                playerClass == PlayableClass.Warrior ?
                Resources.Load<Sprite>("Equipment/Icons/Warrior_Balanced") :
                Resources.Load<Sprite>("Equipment/Icons/Mage_Chimera");
            GameObject iconFrame = DienGameUITheme.Panel(card.transform, "ClassIconPlate", "IconPlate");
            DienGameUITheme.Rect((RectTransform)iconFrame.transform, .035f, .19f, .205f, .85f);
            DienGameUITheme.Icon(card.transform, "ClassIcon", icon, .047f, .26f, .193f, .79f);
            DienGameUITheme.Text(card.transform, "ClassName", className, 40f,
                DienGameUITheme.Ivory, .245f, .63f, .705f, .92f,
                TextAlignmentOptions.MidlineLeft, true);
            DienGameUITheme.Text(card.transform, "ClassWeapon", weapon, 28f,
                DienGameUITheme.Gold, .245f, .37f, .945f, .615f,
                TextAlignmentOptions.MidlineLeft);
            DienGameUITheme.Text(card.transform, "ClassDescription", description, 26f,
                DienGameUITheme.Muted, .245f, .095f, .95f, .375f,
                TextAlignmentOptions.MidlineLeft);
            status = DienGameUITheme.Text(card.transform, "SelectedState", string.Empty,
                25f, DienGameUITheme.Teal, .71f, .72f, .964f, .925f,
                TextAlignmentOptions.Right, true);
        }

        private void OpenCharacterSelection()
        {
            if (_characterSelectionOverlay == null) return;
            _pendingClass = LobbySessionService.SelectedClass;
            RefreshCharacterCards();
            _characterSelectionOverlay.SetActive(true);
            _characterSelectionOverlay.transform.SetAsLastSibling();
        }

        private void ChoosePendingCharacter(PlayableClass playerClass)
        {
            _pendingClass = playerClass;
            RefreshCharacterCards();
        }

        private void RefreshCharacterCards()
        {
            RefreshCharacterCard(_marksmanClassButton, _marksmanClassStatus,
                _pendingClass == PlayableClass.Marksman);
            RefreshCharacterCard(_assassinClassButton, _assassinClassStatus,
                _pendingClass == PlayableClass.Assassin);
            RefreshCharacterCard(_mageClassButton, _mageClassStatus,
                _pendingClass == PlayableClass.Mage);
            RefreshCharacterCard(_warriorClassButton, _warriorClassStatus,
                _pendingClass == PlayableClass.Warrior);
        }

        private static void RefreshCharacterCard(Button button, TMP_Text status, bool selected)
        {
            if (button != null)
                DienGameUITheme.Skin(button.GetComponent<Image>(), selected ? "CardSelected" : "Card");
            if (status != null) status.text = selected ? "ĐANG CHỌN" : string.Empty;
        }

        private void ConfirmCharacterSelection()
        {
            LobbySessionService.SelectClass(_pendingClass);
            RefreshCharacterProfile();
            CancelCharacterSelection();
        }

        private void CancelCharacterSelection()
        {
            if (_characterSelectionOverlay != null)
                _characterSelectionOverlay.SetActive(false);
        }

        private void RefreshCharacterProfile()
        {
            if (_profileClassName != null)
                _profileClassName.text = LobbySessionService.SelectedClass == PlayableClass.Assassin
                    ? "SÁT THỦ"
                    : LobbySessionService.SelectedClass == PlayableClass.Mage ? "PHÁP SƯ" :
                    LobbySessionService.SelectedClass == PlayableClass.Warrior ? "ĐẤU SĨ" : "XẠ THỦ";
        }

        private void CreateToast(Transform parent)
        {
            _toast = CreateText(parent, string.Empty, 20f, TextAlignmentOptions.Center);
            SetRect(_toast.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 220f), new Vector2(760f, 42f), new Vector2(0.5f, 0f));
            _toast.color = GoldSoft;
        }

        private void SelectStage(int stage)
        {
            _selectedStage = Mathf.Clamp(stage, 1, MetaProgressService.MaxStageCount);

            foreach (KeyValuePair<int, GameObject> pair in _stageSelectionGlows)
            {
                pair.Value.SetActive(pair.Key == _selectedStage);
            }

            _stageTitle.text = _selectedStage == 3 ? "ĐẢO 3 · CỔ TỰ" :
                _selectedStage == 4 ? "ĐẢO 4 · HỎA THÀNH" :
                _selectedStage == 5 ? "ĐẢO 5 · CUNG BĂNG" : StageNames[_selectedStage];
            _stageDescription.text = StageDescriptions[_selectedStage];

            bool implemented = MetaProgressService.IsStageImplemented(_selectedStage);
            bool unlocked = IsStagePlayable(_selectedStage);
            bool completed = _selectedStage <= MetaProgressService.HighestCompletedStage;

            if (_stagePreview != null) _stagePreview.sprite = GetIslandSprite(_selectedStage);
            _stageStatus.text = completed ? "ĐÃ HOÀN THÀNH" :
                implemented && unlocked ? "SẴN SÀNG CHIẾN ĐẤU" : "CHƯA MỞ";

            var stageData = LobbyStageCatalog.Get(_selectedStage);
            _stageReward.text = $"{stageData.RewardGold} vàng · {stageData.RewardGem} đá\n{stageData.RewardChest} rương";

            BuildStageStars(completed ? 3 : implemented && unlocked ? 1 : 0);

            if (_playButton != null)
            {
                _playButton.interactable = implemented && unlocked;
                DienGameUITheme.Skin(_playButtonImage,
                    implemented && unlocked ? "ButtonGold" : "ButtonGhost");
                _playButtonLabel.text = implemented && unlocked ? "VÀO ẢI" : "CHƯA MỞ";
                _playButtonLabel.color = implemented && unlocked
                    ? new Color(.19f, .13f, .07f, 1f)
                    : DienGameUITheme.Ivory;
            }
        }

        private void BuildStageStars(int count)
        {
            ClearChildren(_starRow);
            for (int i = 0; i < 3; i++)
            {
                Image star = CreateImage(_starRow, $"Star_{i}", i < count ? starFullSprite : starEmptySprite, true);
                SetRect(star.rectTransform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(i * 58f, 0f), new Vector2(50f, 50f), new Vector2(0f, 0.5f));
            }
        }

        private void RefreshMeta()
        {
            if (_goldText != null) _goldText.text = MetaProgressService.Gold.ToString("N0");
            if (_gemText != null) _gemText.text = MetaProgressService.Gems.ToString("N0");
        }

        private void OnPlayPressed()
        {
            if (!LobbySessionService.StartStage(_selectedStage))
                _toast.text = "Không tìm thấy scene của class đã chọn trong Build Settings. Chạy DienGame > World > Install All.";
        }

        private void OnCheckInPressed()
        {
            int reward = MetaProgressService.ClaimDailyCheckIn();
            _toast.text = reward > 0
                ? $"Điểm danh thành công: +{reward} Vàng"
                : "Hôm nay bạn đã điểm danh rồi.";
            RefreshMeta();
        }

        private void OpenLobbyAudioSettings()
        {
            if (_audioSettingsPanel == null) return;
            _audioSettingsPanel.SetActive(true);
            _audioSettingsPanel.transform.SetAsLastSibling();
        }

        private void CloseLobbyAudioSettings()
        {
            if (_audioSettingsPanel != null) _audioSettingsPanel.SetActive(false);
        }

        private void OnNavigationPressed(string label, bool selected)
        {
            if (selected) { _toast.text=string.Empty; return; }
            WorldHubPanels.Open(_root,label,RefreshMeta);
        }

        private static bool IsStagePlayable(int stage)
        {
            return stage >= 1 && stage <= MetaProgressService.MaxStageCount &&
                   (UnlockAllIslandsForTest || MetaProgressService.IsStageUnlocked(stage));
        }

        private string GetStageStars(int stage)
        {
            if (stage <= MetaProgressService.HighestCompletedStage)
            {
                return "★★★";
            }

            return IsStagePlayable(stage) ? "★☆☆" : "☆☆☆";
        }

        private void CreateRoute(Transform parent, Vector2 from, Vector2 to)
        {
            Vector2 delta = to - from;
            float length = delta.magnitude;
            int dotCount = Mathf.Max(3, Mathf.RoundToInt(length / 42f));

            for (int i = 1; i < dotCount; i++)
            {
                float t = i / (float)dotCount;
                Vector2 p = Vector2.Lerp(from, to, t);
                GameObject dot = CreatePanel(parent, "RouteDot", new Color(1f, 0.82f, 0.36f, 0.78f));
                SetRect(dot.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), p, new Vector2(11f, 11f), new Vector2(0.5f, 0.5f));
                dot.GetComponent<Image>().raycastTarget = false;
            }
        }

        private Sprite GetIslandSprite(int stage)
        {
            int index = stage - 1;
            return islandSprites != null && index >= 0 && index < islandSprites.Length
                ? islandSprites[index]
                : null;
        }

        private Image CreateImage(Transform parent, string name, Sprite sprite, bool preserveAspect)
        {
            GameObject obj = CreateUIObject(name, parent);
            Image image = obj.AddComponent<Image>();
            image.sprite = sprite;
            image.preserveAspect = preserveAspect;
            image.raycastTarget = false;
            return image;
        }

        private Button CreateImageButton(Transform parent, string name, Sprite sprite, UnityEngine.Events.UnityAction action)
        {
            GameObject obj = CreateUIObject(name, parent);
            Image image = obj.AddComponent<Image>();
            image.sprite = sprite;
            image.preserveAspect = true;

            Button button = obj.AddComponent<Button>();
            button.targetGraphic = image;
            button.onClick.AddListener(action);
            return button;
        }

        private static Button CreateTextButton(Transform parent, string name, string text, UnityEngine.Events.UnityAction action, Color color)
        {
            GameObject obj = CreatePanel(parent, name, color);
            Button button = obj.AddComponent<Button>();
            button.targetGraphic = obj.GetComponent<Image>();
            button.onClick.AddListener(action);
            DienGameUITheme.Skin(obj.GetComponent<Image>(), "ButtonGhost");

            TMP_Text label = CreateText(obj.transform, text, 26f, TextAlignmentOptions.Center);
            Stretch(label.rectTransform);
            label.fontStyle = FontStyles.Bold;
            return button;
        }

        private static void CreateProgressBar(Transform parent, Vector2 position, Vector2 size, float value)
        {
            GameObject bg = CreatePanel(parent, "ProgressBG", new Color(0.1f, 0.12f, 0.16f, 1f));
            SetRect(bg.GetComponent<RectTransform>(), new Vector2(0f, 0f), new Vector2(0f, 0f), position, size, new Vector2(0f, 0.5f));

            GameObject fill = CreatePanel(bg.transform, "Fill", Gold);
            RectTransform fillRect = fill.GetComponent<RectTransform>();
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = new Vector2(Mathf.Clamp01(value), 1f);
            fillRect.offsetMin = Vector2.zero;
            fillRect.offsetMax = Vector2.zero;
        }

        private static GameObject CreatePanel(Transform parent, string name, Color color)
        {
            GameObject obj = CreateUIObject(name, parent);
            Image image = obj.AddComponent<Image>();
            image.color = color;
            return obj;
        }

        private static TMP_Text CreateText(Transform parent, string text, float size, TextAlignmentOptions alignment)
        {
            GameObject obj = CreateUIObject("Text", parent);
            TextMeshProUGUI tmp = obj.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = size;
            tmp.alignment = alignment;
            tmp.color = Color.white;
            DienGameUITheme.StyleText(tmp, size, Color.white, false, false, alignment);
            return tmp;
        }

        private static GameObject CreateUIObject(string name, Transform parent)
        {
            GameObject obj = new(name, typeof(RectTransform));
            obj.transform.SetParent(parent, false);
            return obj;
        }

        private static void AddOutline(GameObject obj, Color color, float distance)
        {
            Outline outline = obj.AddComponent<Outline>();
            outline.effectColor = color;
            outline.effectDistance = new Vector2(distance, -distance);
            outline.useGraphicAlpha = true;
        }

        private static void AddBottomLine(GameObject parent, Color color, float height)
        {
            GameObject line = CreatePanel(parent.transform, "BottomLine", color);
            RectTransform rect = line.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 0f);
            rect.anchorMax = new Vector2(1f, 0f);
            rect.pivot = new Vector2(0.5f, 0f);
            rect.sizeDelta = new Vector2(0f, height);
            rect.anchoredPosition = Vector2.zero;
        }

        private static void AddTopLine(GameObject parent, Color color, float height)
        {
            GameObject line = CreatePanel(parent.transform, "TopLine", color);
            RectTransform rect = line.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.sizeDelta = new Vector2(0f, height);
            rect.anchoredPosition = Vector2.zero;
        }

        private static void ClearChildren(Transform parent)
        {
            for (int i = parent.childCount - 1; i >= 0; i--)
            {
                Destroy(parent.GetChild(i).gameObject);
            }
        }

        private static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        private static void SetAnchoredStretch(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax)
        {
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        private static void SetRect(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax, Vector2 position, Vector2 size, Vector2 pivot)
        {
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = pivot;
            rect.sizeDelta = size;
            rect.anchoredPosition = position;
        }
    }
}
