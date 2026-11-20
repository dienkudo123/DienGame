using DienGame.Client;
using DienGame.Gameplay.Equipment;
using DienGame.Gameplay.Player;
using DienGame.Gameplay.Skills.Assassin;
using UnityEngine;

namespace DienGame.Gameplay.Skills.Warrior
{
    /// <summary>
    /// Warrior-specific 8-direction animated body and equipped sword. Continuous 360-degree
    /// movement is unchanged. If new Resources/Warrior PNGs are missing, the ORIGINAL
    /// Assassin animation asset is an explicit compatibility fallback (not overwritten).
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class WarriorVisual : MonoBehaviour
    {
        [SerializeField] private AssassinSpriteAnimationData fallbackCharacterAtlas;
        private SpriteRenderer _body, _blade;
        private TrailRenderer _slashTrail;
        private Material _ownedTrailMaterial;
        private PlayerMovement _movement;
        private PlayerTargeting _targeting;
        private Animator _legacy;
        private Vector2 _facing = Vector2.down;
        private float _runClock, _attackClock = 1f;
        private int _combo = 1;
        private const float SlashDuration = .30f;

        public void Configure(AssassinSpriteAnimationData originalCharacterAtlas) =>
            fallbackCharacterAtlas = originalCharacterAtlas;

        private void Awake()
        {
            _body = GetComponent<SpriteRenderer>();
            _movement = GetComponentInParent<PlayerMovement>();
            _targeting = GetComponentInParent<PlayerTargeting>();
            _legacy = GetComponent<Animator>();
            if (_legacy != null) _legacy.enabled = false;

            GameObject sword = new GameObject("Warrior_EquippedSwordVisual");
            sword.transform.SetParent(transform, false);
            _blade = sword.AddComponent<SpriteRenderer>();
            _blade.sortingOrder = _body.sortingOrder + 2;
            _blade.transform.localScale = Vector3.one * .23f;

            GameObject tip = new GameObject("SwordEnergyTip");
            tip.transform.SetParent(_blade.transform, false);
            tip.transform.localPosition = new Vector3(0f, -.70f, 0f);
            _slashTrail = tip.AddComponent<TrailRenderer>();
            _slashTrail.time = .14f;
            _slashTrail.minVertexDistance = .045f;
            _slashTrail.widthMultiplier = .11f;
            _slashTrail.widthCurve = AnimationCurve.Linear(0f, 1f, 1f, 0f);
            Shader shader = Shader.Find("Sprites/Default");
            if (shader != null)
            {
                _ownedTrailMaterial = new Material(shader) { name = "WarriorSwordTrail_Instance" };
                _slashTrail.sharedMaterial = _ownedTrailMaterial;
            }
            _slashTrail.sortingOrder = _blade.sortingOrder + 1;
            _slashTrail.emitting = false;
        }

        public void PlaySlash(Vector2 direction, int combo)
        {
            if (direction.sqrMagnitude > .002f) _facing = direction.normalized;
            _combo = combo;
            _attackClock = 0f;
            if (_slashTrail != null) _slashTrail.Clear();
        }

        private void Update()
        {
            if (_body == null) return;
            Vector2 move = _movement != null ? _movement.MoveDirection : Vector2.zero;
            if (_attackClock >= SlashDuration)
            {
                if (_targeting != null && _targeting.CurrentTarget != null)
                {
                    Vector2 aim = (Vector2)_targeting.CurrentTarget.position - (Vector2)transform.position;
                    if (aim.sqrMagnitude > .002f) _facing = aim.normalized;
                }
                else if (move.sqrMagnitude > .002f) _facing = move.normalized;
            }
            bool moving = move.sqrMagnitude > .002f;
            _runClock = moving ? _runClock + Time.deltaTime * 7.2f : 0f;
            if (_attackClock < SlashDuration) _attackClock += Time.deltaTime;
            bool slashing = _attackClock < SlashDuration;
            int frame = slashing ? Mathf.Clamp(Mathf.FloorToInt(_attackClock / SlashDuration * 4f), 0, 3) :
                (moving ? Mathf.FloorToInt(_runClock) % 4 : 0);
            int direction = DirectionIndex(_facing);
            Sprite sprite = slashing ? WarriorArt.Slash(direction, frame) : WarriorArt.Run(direction, frame);
            if (sprite == null && fallbackCharacterAtlas != null)
                sprite = slashing ? fallbackCharacterAtlas.GetSlash(direction, frame) :
                    fallbackCharacterAtlas.GetRun(direction, frame);
            if (sprite != null && _body.sprite != sprite) _body.sprite = sprite;
            _body.color = Color.white; // Do not tint the newly authored warrior armor art.

            EquipmentDefinition item = EquipmentInventory.Selected(PlayableClass.Warrior);
            if (_blade == null || item == null) return;
            Sprite artwork = EquipmentEvolutionArt.IconFor(item);
            if (_blade.sprite != artwork) _blade.sprite = artwork;
            Vector2 bladeDirection = _facing;
            float angle = Mathf.Atan2(bladeDirection.y, bladeDirection.x) * Mathf.Rad2Deg;
            if (slashing)
            {
                float t = Mathf.Clamp01(_attackClock / SlashDuration);
                angle += Mathf.Lerp(-72f, 72f, t) * (_combo % 2 == 0 ? -1f : 1f);
            }
            _blade.transform.localPosition = (Vector3)(bladeDirection * .37f) + new Vector3(0f, -.02f, 0f);
            _blade.transform.localEulerAngles = new Vector3(0f, 0f, angle - 42f);
            _blade.transform.localScale = Vector3.one * (item.Id == "warrior_great" ? .30f : .23f);
            _blade.color = EquipmentResonance.VisualTint(item);
            _blade.sortingOrder = _body.sortingOrder + (bladeDirection.y > .25f ? -1 : 2);
            if (_slashTrail != null)
            {
                _slashTrail.emitting = slashing;
                Color energy = item.Effect == EquipmentEffect.Burn ? new Color(1f, .38f, .13f, .85f) :
                    item.Effect == EquipmentEffect.Slow ? new Color(.22f, .83f, 1f, .85f) :
                    new Color(.88f, .70f, 1f, .83f);
                _slashTrail.startColor = energy;
                _slashTrail.endColor = new Color(energy.r, energy.g, energy.b, 0f);
                _slashTrail.sortingOrder = _blade.sortingOrder + 1;
            }
        }

        private void OnDestroy()
        {
            if (_ownedTrailMaterial != null) Destroy(_ownedTrailMaterial);
        }

        private static int DirectionIndex(Vector2 dir)
        {
            float degrees = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            if (degrees < 0f) degrees += 360f;
            if (degrees >= 337.5f || degrees < 22.5f) return 6;
            if (degrees < 67.5f) return 5;
            if (degrees < 112.5f) return 4;
            if (degrees < 157.5f) return 3;
            if (degrees < 202.5f) return 2;
            if (degrees < 247.5f) return 1;
            if (degrees < 292.5f) return 0;
            return 7;
        }
    }
}
