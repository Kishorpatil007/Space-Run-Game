using UnityEngine;
using SpaceJet.Core;
using SpaceJet.Player;

namespace SpaceJet.Player
{
    public class CameraController : MonoBehaviour
    {
        public static CameraController Instance { get; set; }

        [Header("Target & Follow")]
        public Transform target;
        public Vector3 normalOffset = new Vector3(0f, 2.5f, -7.5f);
        public float positionSmoothSpeed = 12f;
        public float rotationSmoothSpeed = 10f;

        [Header("FOV Settings")]
        public float normalFOV = 60f;
        public float boostFOV = 74f;
        public float fovTransitionSpeed = 6f;

        [Header("Camera Shake")]
        private float shakeIntensity = 0f;
        private float shakeDuration = 0f;

        private Camera cam;
        private Vector3 currentVelocity;

        private void Awake()
        {
            Instance = this;
            cam = GetComponent<Camera>();
        }

        private void LateUpdate()
        {
            if (target == null) return;

            GameState state = GameManager.Instance != null ? GameManager.Instance.CurrentState : GameState.Playing;

            if (state == GameState.CinematicArrival)
            {
                HandleCinematicCamera();
                return;
            }

            // Target position calculation
            Vector3 desiredPosition = target.position + normalOffset;

            // Apply camera shake if active
            if (shakeDuration > 0f)
            {
                Vector3 shakeOffset = Random.insideUnitSphere * shakeIntensity;
                desiredPosition += shakeOffset;
                shakeDuration -= Time.deltaTime;
                shakeIntensity = Mathf.Lerp(shakeIntensity, 0f, Time.deltaTime * 5f);
            }

            // Smooth position dampening
            transform.position = Vector3.Lerp(transform.position, desiredPosition, Time.deltaTime * positionSmoothSpeed);

            // Look slightly ahead of player
            Vector3 lookTarget = target.position + Vector3.forward * 15f;
            Quaternion targetRotation = Quaternion.LookRotation(lookTarget - transform.position, Vector3.up);

            // Add subtle roll banking with player
            float playerRoll = target.eulerAngles.z;
            if (playerRoll > 180f) playerRoll -= 360f;
            targetRotation *= Quaternion.Euler(0f, 0f, playerRoll * 0.25f);

            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSmoothSpeed);

            // Dynamic FOV when boosting
            bool boosting = (PlayerBoost.Instance != null && PlayerBoost.Instance.IsBoosting);
            float targetFOV = boosting ? boostFOV : normalFOV;
            if (cam)
            {
                cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, targetFOV, Time.deltaTime * fovTransitionSpeed);
            }
        }

        private void HandleCinematicCamera()
        {
            // Zoom out and elevate to frame the player entering the planet
            Vector3 cinematicOffset = new Vector3(0f, 4.5f, -12f);
            Vector3 desiredPosition = target.position + cinematicOffset;
            transform.position = Vector3.Lerp(transform.position, desiredPosition, Time.deltaTime * 3f);

            Vector3 lookTarget = target.position + Vector3.forward * 25f;
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(lookTarget - transform.position), Time.deltaTime * 3f);

            if (cam)
            {
                cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, 65f, Time.deltaTime * 2f);
            }
        }

        public void TriggerShake(float intensity, float duration)
        {
            shakeIntensity = Mathf.Max(shakeIntensity, intensity);
            shakeDuration = Mathf.Max(shakeDuration, duration);
        }

        public void SetTarget(Transform newTarget)
        {
            target = newTarget;
            if (target != null)
            {
                transform.position = target.position + normalOffset;
            }
        }
    }
}
