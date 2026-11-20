using UnityEngine;
using UnityEngine.Rendering;

namespace DienGame.Gameplay.Skills.Marksman
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Camera))]
    public sealed class JudgementCameraShake : MonoBehaviour
    {
        private Camera _camera;

        private float _remainingTime;

        private float _totalDuration;

        private float _strength;

        private Vector3 _originalLocalPosition;

        private bool _offsetApplied;

        private void Awake()
        {
            _camera =
                GetComponent<Camera>();
        }

        private void OnEnable()
        {
            RenderPipelineManager.beginCameraRendering +=
                OnBeginCameraRendering;

            RenderPipelineManager.endCameraRendering +=
                OnEndCameraRendering;
        }

        private void OnDisable()
        {
            RenderPipelineManager.beginCameraRendering -=
                OnBeginCameraRendering;

            RenderPipelineManager.endCameraRendering -=
                OnEndCameraRendering;

            RestorePosition();
        }

        private void Update()
        {
            if (_remainingTime <= 0f)
            {
                return;
            }

            _remainingTime -=
                Time.deltaTime;

            if (_remainingTime < 0f)
            {
                _remainingTime = 0f;
            }
        }

        public void Shake(
            float duration,
            float strength)
        {
            duration =
                Mathf.Max(
                    0f,
                    duration
                );

            strength =
                Mathf.Max(
                    0f,
                    strength
                );

            // Nếu đang rung mà có một vụ rung mạnh hơn
            // thì lấy giá trị mạnh hơn.
            _remainingTime =
                Mathf.Max(
                    _remainingTime,
                    duration
                );

            _totalDuration =
                Mathf.Max(
                    0.01f,
                    duration
                );

            _strength =
                Mathf.Max(
                    _strength,
                    strength
                );
        }

        private void OnBeginCameraRendering(
            ScriptableRenderContext context,
            Camera camera)
        {
            if (
                camera != _camera ||
                _remainingTime <= 0f
            )
            {
                return;
            }

            if (_offsetApplied)
            {
                return;
            }

            _originalLocalPosition =
                transform.localPosition;

            float fade =
                Mathf.Clamp01(
                    _remainingTime /
                    _totalDuration
                );

            Vector2 random =
                Random.insideUnitCircle *
                _strength *
                fade;

            transform.localPosition =
                _originalLocalPosition +
                new Vector3(
                    random.x,
                    random.y,
                    0f
                );

            _offsetApplied = true;
        }

        private void OnEndCameraRendering(
            ScriptableRenderContext context,
            Camera camera)
        {
            if (camera != _camera)
            {
                return;
            }

            RestorePosition();

            if (_remainingTime <= 0f)
            {
                _strength = 0f;
            }
        }

        private void RestorePosition()
        {
            if (!_offsetApplied)
            {
                return;
            }

            transform.localPosition =
                _originalLocalPosition;

            _offsetApplied = false;
        }
    }
}