using UnityEngine;
using SpaceJet.Audio;
using SpaceJet.Core;

namespace SpaceJet.Player
{
    [RequireComponent(typeof(Rigidbody))]
    public class PlayerController : MonoBehaviour
    {
        public static PlayerController Instance { get; set; }

        [Header("Flight Speed")]
        public float baseSpeed = 32f;
        public float horizontalSpeed = 22f;
        public float verticalSpeed = 18f;

        [Header("Flight Boundaries")]
        public float minX = -16f;
        public float maxX = 16f;
        public float minY = -7f;
        public float maxY = 8.5f;

        [Header("Tilt & Banking")]
        public float maxBankAngle = 32f;
        public float maxPitchAngle = 18f;
        public float bankSmoothing = 8f;

        [Header("Thrusters & Particles")]
        public ParticleSystem[] normalThrusterParticles;
        public ParticleSystem[] boostThrusterParticles;
        public Light engineLight;

        private Rigidbody rb;
        private Vector3 startPosition;
        private Quaternion targetRotation;
        private float currentRoll = 0f;
        private float currentPitch = 0f;

        public float CurrentForwardSpeed { get; private set; }

        private void Awake()
        {
            Instance = this;
            rb = GetComponent<Rigidbody>();
            rb.useGravity = false;
            rb.isKinematic = true; // Controlled via kinematic movement to ensure silky smooth flight
            startPosition = transform.position;
        }

        private void OnEnable()
        {
            ApplySkin();
            JetSkinManager.OnSkinEquipped += HandleSkinEquipped;
        }

        private void OnDisable()
        {
            JetSkinManager.OnSkinEquipped -= HandleSkinEquipped;
        }

        private void HandleSkinEquipped(int skinId)
        {
            ApplySkin();
        }

        private void Start()
        {
            ApplyUpgrades();
            ApplySkin();
        }

        [HideInInspector]
        public Color currentEngineLightColor = new Color(0.2f, 0.7f, 1f);

        public void ApplySkin()
        {
            var skin = JetSkinManager.GetSkin(SaveManager.SelectedSkinIndex);
            currentEngineLightColor = skin.engineLightColor;
            JetSkinManager.ApplySkinToJet(gameObject, SaveManager.SelectedSkinIndex);
            if (engineLight == null)
            {
                foreach (var l in GetComponentsInChildren<Light>(true))
                {
                    if (l.name == "EngineLight") { engineLight = l; break; }
                }
                if (engineLight == null) engineLight = GetComponentInChildren<Light>();
            }
            if (engineLight != null) engineLight.color = currentEngineLightColor;
        }

        public void ApplyUpgrades()
        {
            int engineLvl = SaveManager.EngineLevel;
            baseSpeed = 30f + (engineLvl - 1) * 3.5f;
        }

        public void ResetToStart()
        {
            transform.position = startPosition;
            transform.rotation = Quaternion.identity;
            currentRoll = 0f;
            currentPitch = 0f;
            ApplyUpgrades();
            ApplySkin();
        }

        private void Update()
        {
            if (GameManager.Instance == null) return;

            GameState state = GameManager.Instance.CurrentState;

            if (state == GameState.Playing)
            {
                HandleFlightMovement();
                UpdateEngineVisualsAndSound();
            }
            else if (state == GameState.CinematicArrival)
            {
                HandleCinematicApproach();
            }
        }

        private void HandleFlightMovement()
        {
            // Input collection (Keyboard + Arrow keys + optional Mouse offset)
            float inputX = Input.GetAxis("Horizontal");
            float inputY = Input.GetAxis("Vertical");

            // Optional mouse drag steering if left click held
            if (Input.GetMouseButton(0))
            {
                float mouseDeltaX = Input.GetAxis("Mouse X");
                float mouseDeltaY = Input.GetAxis("Mouse Y");
                if (Mathf.Abs(mouseDeltaX) > 0.05f) inputX = Mathf.Clamp(inputX + mouseDeltaX * 1.5f, -1f, 1f);
                if (Mathf.Abs(mouseDeltaY) > 0.05f) inputY = Mathf.Clamp(inputY + mouseDeltaY * 1.5f, -1f, 1f);
            }

            // Calculate forward speed (with character perk bonus)
            bool boosting = (PlayerBoost.Instance != null && PlayerBoost.Instance.IsBoosting);
            float speedMult = boosting ? PlayerBoost.Instance.speedMultiplier : 1.0f;
            CurrentForwardSpeed = baseSpeed * speedMult * CharacterManager.GetActiveSpeedMultiplier();

            // Forward delta
            float forwardDelta = CurrentForwardSpeed * Time.deltaTime;
            GameManager.Instance.AddDistance(forwardDelta);

            // Positional update
            Vector3 pos = transform.position;
            pos.x += inputX * horizontalSpeed * Time.deltaTime;
            pos.y += inputY * verticalSpeed * Time.deltaTime;
            pos.z += forwardDelta;

            // Clamp within boundary corridor
            pos.x = Mathf.Clamp(pos.x, minX, maxX);
            pos.y = Mathf.Clamp(pos.y, minY, maxY);
            transform.position = pos;

            // Banking & Pitching angles
            float targetRoll = -inputX * maxBankAngle;
            float targetPitch = -inputY * maxPitchAngle;

            currentRoll = Mathf.Lerp(currentRoll, targetRoll, Time.deltaTime * bankSmoothing);
            currentPitch = Mathf.Lerp(currentPitch, targetPitch, Time.deltaTime * bankSmoothing);

            transform.rotation = Quaternion.Euler(currentPitch, 0f, currentRoll);
        }

        private void HandleCinematicApproach()
        {
            // Smoothly fly toward center line and accelerate into planet
            Vector3 pos = transform.position;
            pos.x = Mathf.Lerp(pos.x, 0f, Time.deltaTime * 3f);
            pos.y = Mathf.Lerp(pos.y, 0f, Time.deltaTime * 3f);
            pos.z += (baseSpeed * 1.6f) * Time.deltaTime;
            transform.position = pos;

            // Slowly rotate level
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.identity, Time.deltaTime * 4f);
        }

        private void UpdateEngineVisualsAndSound()
        {
            bool boosting = (PlayerBoost.Instance != null && PlayerBoost.Instance.IsBoosting);

            // Modulate engine audio pitch
            if (AudioManager.Instance)
            {
                float targetPitch = boosting ? 1.45f : 1.0f;
                AudioManager.Instance.SetEnginePitch(targetPitch);
            }

            // Thruster particle states
            if (boostThrusterParticles != null)
            {
                foreach (var p in boostThrusterParticles)
                {
                    if (p)
                    {
                        var emission = p.emission;
                        emission.enabled = boosting;
                    }
                }
            }

            if (engineLight)
            {
                engineLight.intensity = boosting ? 3.5f : 1.8f;
                engineLight.color = boosting ? Color.Lerp(currentEngineLightColor, Color.white, 0.45f) : currentEngineLightColor;
            }
        }
    }
}
