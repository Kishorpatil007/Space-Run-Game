using System.Collections.Generic;
using UnityEngine;

namespace SpaceJet.Audio
{
    /// <summary>
    /// Complete procedural audio generator and audio manager.
    /// Synthesizes all sci-fi sound effects and ambient electronic space music at runtime.
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; set; }

        [Header("Audio Sources")]
        private AudioSource musicSource;
        private AudioSource engineSource;
        private AudioSource sfxSource;
        private AudioSource coinSource;
        private AudioSource alarmSource;

        private int coinStreak = 0;
        private float lastCoinCollectTime = 0f;

        [Header("Volume Controls")]
        [Range(0f, 1f)] public float masterVolume = 1f;
        [Range(0f, 1f)] public float musicVolume = 0.7f;
        [Range(0f, 1f)] public float sfxVolume = 0.85f;

        private Dictionary<string, AudioClip> proceduralClips = new Dictionary<string, AudioClip>();

        private const int SAMPLE_RATE = 44100;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeAudio();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public void InitializeAudio()
        {
            SetupAudioSources();
            GenerateProceduralClips();
        }

        private void SetupAudioSources()
        {
            musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.loop = true;
            musicSource.playOnAwake = false;

            engineSource = gameObject.AddComponent<AudioSource>();
            engineSource.loop = true;
            engineSource.playOnAwake = false;

            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.loop = false;
            sfxSource.playOnAwake = false;

            coinSource = gameObject.AddComponent<AudioSource>();
            coinSource.loop = false;
            coinSource.playOnAwake = false;

            alarmSource = gameObject.AddComponent<AudioSource>();
            alarmSource.loop = true;
            alarmSource.playOnAwake = false;

            UpdateVolumes();
        }

        public void UpdateVolumes()
        {
            if (musicSource) musicSource.volume = masterVolume * musicVolume;
            if (engineSource) engineSource.volume = masterVolume * sfxVolume * 0.4f;
            if (sfxSource) sfxSource.volume = masterVolume * sfxVolume;
            if (coinSource) coinSource.volume = masterVolume * sfxVolume;
            if (alarmSource) alarmSource.volume = masterVolume * sfxVolume * 0.6f;
        }

        private void GenerateProceduralClips()
        {
            proceduralClips["coin"] = CreateCoinClip();
            proceduralClips["energy"] = CreateEnergyClip();
            proceduralClips["shield"] = CreateShieldClip();
            proceduralClips["collision"] = CreateCollisionClip();
            proceduralClips["boost"] = CreateBoostClip();
            proceduralClips["complete"] = CreateMissionCompleteClip();
            proceduralClips["failed"] = CreateMissionFailedClip();
            proceduralClips["alarm"] = CreateAlarmClip();
            proceduralClips["critical"] = CreateCriticalAlarmClip();
            proceduralClips["click"] = CreateClickClip();
            proceduralClips["engine"] = CreateEngineLoopClip();
            proceduralClips["station"] = CreateStationClip();

            // Background music loops for different mission moods
            proceduralClips["bgm_orbit"] = CreateSpaceMusicClip(120f, 1.0f);
            proceduralClips["bgm_intense"] = CreateSpaceMusicClip(132f, 1.25f);
        }

        #region Sound Effect Synthesis

        private AudioClip CreateCoinClip()
        {
            int length = (int)(SAMPLE_RATE * 0.28f);
            float[] data = new float[length];
            float freq1 = 987.77f; // B5
            float freq2 = 1318.51f; // E6

            for (int i = 0; i < length; i++)
            {
                float t = (float)i / SAMPLE_RATE;
                float currentFreq = (t < 0.12f) ? freq1 : freq2;
                float env = Mathf.Exp(-t * 12f);
                data[i] = Mathf.Sin(2 * Mathf.PI * currentFreq * t) * env * 0.6f;
            }

            AudioClip clip = AudioClip.Create("CoinSFX", length, 1, SAMPLE_RATE, false);
            clip.SetData(data, 0);
            return clip;
        }

        private AudioClip CreateEnergyClip()
        {
            int length = (int)(SAMPLE_RATE * 0.45f);
            float[] data = new float[length];

            for (int i = 0; i < length; i++)
            {
                float t = (float)i / SAMPLE_RATE;
                // Rising resonant frequency
                float f = Mathf.Lerp(440f, 1046.5f, t / 0.45f);
                float env = Mathf.Sin(Mathf.PI * (t / 0.45f)) * Mathf.Exp(-t * 2.5f);
                float wave = Mathf.Sin(2 * Mathf.PI * f * t) * 0.6f + Mathf.Sin(2 * Mathf.PI * (f * 1.5f) * t) * 0.3f;
                data[i] = wave * env;
            }

            AudioClip clip = AudioClip.Create("EnergySFX", length, 1, SAMPLE_RATE, false);
            clip.SetData(data, 0);
            return clip;
        }

        private AudioClip CreateShieldClip()
        {
            int length = (int)(SAMPLE_RATE * 0.5f);
            float[] data = new float[length];

            for (int i = 0; i < length; i++)
            {
                float t = (float)i / SAMPLE_RATE;
                float f = Mathf.Lerp(300f, 880f, Mathf.Sqrt(t / 0.5f));
                float mod = Mathf.Sin(2 * Mathf.PI * 25f * t) * 0.2f;
                float env = Mathf.Exp(-t * 4f);
                data[i] = Mathf.Sin(2 * Mathf.PI * (f + mod * 100f) * t) * env * 0.7f;
            }

            AudioClip clip = AudioClip.Create("ShieldSFX", length, 1, SAMPLE_RATE, false);
            clip.SetData(data, 0);
            return clip;
        }

        private AudioClip CreateCollisionClip()
        {
            int length = (int)(SAMPLE_RATE * 0.65f);
            float[] data = new float[length];
            var rand = new System.Random(42);

            for (int i = 0; i < length; i++)
            {
                float t = (float)i / SAMPLE_RATE;
                float env = Mathf.Exp(-t * 6f);
                float noise = (float)(rand.NextDouble() * 2.0 - 1.0);
                float sub = Mathf.Sin(2 * Mathf.PI * (80f - t * 40f) * t) * 0.7f;
                data[i] = Mathf.Clamp((noise * 0.5f + sub) * env, -1f, 1f);
            }

            AudioClip clip = AudioClip.Create("CollisionSFX", length, 1, SAMPLE_RATE, false);
            clip.SetData(data, 0);
            return clip;
        }

        private AudioClip CreateBoostClip()
        {
            int length = (int)(SAMPLE_RATE * 0.7f);
            float[] data = new float[length];
            var rand = new System.Random(101);

            for (int i = 0; i < length; i++)
            {
                float t = (float)i / SAMPLE_RATE;
                float env = Mathf.Sin(Mathf.PI * (t / 0.7f));
                float whiteNoise = (float)(rand.NextDouble() * 2.0 - 1.0) * 0.4f;
                float whoosh = Mathf.Sin(2 * Mathf.PI * Mathf.Lerp(150f, 600f, t / 0.7f) * t) * 0.6f;
                data[i] = (whoosh + whiteNoise) * env;
            }

            AudioClip clip = AudioClip.Create("BoostSFX", length, 1, SAMPLE_RATE, false);
            clip.SetData(data, 0);
            return clip;
        }

        private AudioClip CreateAlarmClip()
        {
            int length = (int)(SAMPLE_RATE * 0.35f);
            float[] data = new float[length];

            for (int i = 0; i < length; i++)
            {
                float t = (float)i / SAMPLE_RATE;
                float f = (t < 0.18f) ? 880f : 0f;
                float env = Mathf.Exp(-((t % 0.18f) * 8f));
                data[i] = Mathf.Sin(2 * Mathf.PI * f * t) * env * 0.4f;
            }

            AudioClip clip = AudioClip.Create("LowAlarmSFX", length, 1, SAMPLE_RATE, false);
            clip.SetData(data, 0);
            return clip;
        }

        private AudioClip CreateCriticalAlarmClip()
        {
            int length = (int)(SAMPLE_RATE * 0.4f);
            float[] data = new float[length];

            for (int i = 0; i < length; i++)
            {
                float t = (float)i / SAMPLE_RATE;
                float f = (t < 0.2f) ? 1200f : 800f;
                data[i] = Mathf.Sign(Mathf.Sin(2 * Mathf.PI * f * t)) * 0.35f;
            }

            AudioClip clip = AudioClip.Create("CritAlarmSFX", length, 1, SAMPLE_RATE, false);
            clip.SetData(data, 0);
            return clip;
        }

        private AudioClip CreateClickClip()
        {
            int length = (int)(SAMPLE_RATE * 0.05f);
            float[] data = new float[length];

            for (int i = 0; i < length; i++)
            {
                float t = (float)i / SAMPLE_RATE;
                data[i] = Mathf.Sin(2 * Mathf.PI * 1800f * t) * Mathf.Exp(-t * 80f) * 0.5f;
            }

            AudioClip clip = AudioClip.Create("ClickSFX", length, 1, SAMPLE_RATE, false);
            clip.SetData(data, 0);
            return clip;
        }

        private AudioClip CreateEngineLoopClip()
        {
            int length = SAMPLE_RATE; // 1 second loop
            float[] data = new float[length];
            var rand = new System.Random(77);

            for (int i = 0; i < length; i++)
            {
                float t = (float)i / SAMPLE_RATE;
                float sub = Mathf.Sin(2 * Mathf.PI * 55f * t) * 0.4f;
                float mid = Mathf.Sin(2 * Mathf.PI * 110f * t) * 0.25f;
                float hiss = (float)(rand.NextDouble() * 2.0 - 1.0) * 0.15f;
                data[i] = sub + mid + hiss;
            }

            AudioClip clip = AudioClip.Create("EngineLoop", length, 1, SAMPLE_RATE, false);
            clip.SetData(data, 0);
            return clip;
        }

        private AudioClip CreateStationClip()
        {
            int length = (int)(SAMPLE_RATE * 0.8f);
            float[] data = new float[length];

            for (int i = 0; i < length; i++)
            {
                float t = (float)i / SAMPLE_RATE;
                float f = Mathf.Lerp(220f, 1320f, t / 0.8f);
                float env = Mathf.Sin(Mathf.PI * (t / 0.8f));
                data[i] = Mathf.Sin(2 * Mathf.PI * f * t) * env * 0.65f;
            }

            AudioClip clip = AudioClip.Create("StationSFX", length, 1, SAMPLE_RATE, false);
            clip.SetData(data, 0);
            return clip;
        }

        private AudioClip CreateMissionCompleteClip()
        {
            int length = (int)(SAMPLE_RATE * 2.2f);
            float[] data = new float[length];
            float[] chordNotes = { 523.25f, 659.25f, 783.99f, 1046.50f }; // C5, E5, G5, C6

            for (int i = 0; i < length; i++)
            {
                float t = (float)i / SAMPLE_RATE;
                float sample = 0f;
                int currentNoteIndex = Mathf.Min((int)(t / 0.35f), chordNotes.Length - 1);
                float f = chordNotes[currentNoteIndex];
                float env = Mathf.Exp(-(t % 0.35f) * 3f);

                if (t >= 1.05f) // Sustained final chord
                {
                    float tailEnv = Mathf.Clamp01(1f - (t - 1.05f) / 1.15f);
                    foreach (var note in chordNotes)
                    {
                        sample += Mathf.Sin(2 * Mathf.PI * note * t) * 0.18f * tailEnv;
                    }
                }
                else
                {
                    sample = Mathf.Sin(2 * Mathf.PI * f * t) * env * 0.5f;
                }

                data[i] = Mathf.Clamp(sample, -1f, 1f);
            }

            AudioClip clip = AudioClip.Create("VictorySFX", length, 1, SAMPLE_RATE, false);
            clip.SetData(data, 0);
            return clip;
        }

        private AudioClip CreateMissionFailedClip()
        {
            int length = (int)(SAMPLE_RATE * 1.8f);
            float[] data = new float[length];

            for (int i = 0; i < length; i++)
            {
                float t = (float)i / SAMPLE_RATE;
                float f = Mathf.Lerp(220f, 40f, t / 1.8f);
                float env = Mathf.Clamp01(1f - (t / 1.8f));
                data[i] = (Mathf.Sin(2 * Mathf.PI * f * t) * 0.6f + Mathf.Sin(2 * Mathf.PI * (f * 0.5f) * t) * 0.3f) * env;
            }

            AudioClip clip = AudioClip.Create("FailedSFX", length, 1, SAMPLE_RATE, false);
            clip.SetData(data, 0);
            return clip;
        }

        private AudioClip CreateSpaceMusicClip(float tempoBpm, float speedScale)
        {
            // Synthesizes an ambient electronic looping space synth track (~6.0s seamless loop)
            float secondsPerBeat = 60f / (tempoBpm * speedScale);
            int totalBeats = 16;
            float totalDuration = secondsPerBeat * totalBeats;
            int length = (int)(SAMPLE_RATE * totalDuration);
            float[] data = new float[length];

            // Space chord progression: Am -> F -> C -> G
            float[][] chords = new float[][]
            {
                new float[] { 220.00f, 261.63f, 329.63f }, // Am (A3, C4, E4)
                new float[] { 174.61f, 220.00f, 261.63f }, // F  (F3, A3, C4)
                new float[] { 130.81f, 164.81f, 196.00f }, // C  (C3, E3, G3)
                new float[] { 196.00f, 246.94f, 293.66f }  // G  (G3, B3, D4)
            };

            for (int i = 0; i < length; i++)
            {
                float t = (float)i / SAMPLE_RATE;
                int currentChordIndex = (int)((t / totalDuration) * 4) % 4;
                float[] chord = chords[currentChordIndex];

                // Arpeggiated sequence (16th notes)
                float beatFraction = (t / secondsPerBeat) % 1f;
                int arpIndex = ((int)(t / (secondsPerBeat * 0.25f))) % chord.Length;
                float arpFreq = chord[arpIndex] * 2f;
                float arpEnv = Mathf.Exp(-((t % (secondsPerBeat * 0.25f)) * 7f));
                float arp = Mathf.Sin(2 * Mathf.PI * arpFreq * t) * arpEnv * 0.25f;

                // Warm bass pad
                float bassFreq = chord[0] * 0.5f;
                float bass = (Mathf.Sin(2 * Mathf.PI * bassFreq * t) + 0.3f * Mathf.Sin(2 * Mathf.PI * bassFreq * 2f * t)) * 0.25f;

                // Subtle kick pulse on the downbeat
                float kickEnv = Mathf.Exp(-beatFraction * 14f);
                float kick = Mathf.Sin(2 * Mathf.PI * Mathf.Lerp(120f, 40f, beatFraction) * t) * kickEnv * 0.3f;

                data[i] = Mathf.Clamp((arp + bass + kick) * 0.7f, -1f, 1f);
            }

            AudioClip clip = AudioClip.Create("SpaceMusicTrack", length, 1, SAMPLE_RATE, false);
            clip.SetData(data, 0);
            return clip;
        }

        #endregion

        #region Playback API

        public void PlaySound(string soundName, float volumeMultiplier = 1f)
        {
            if (soundName == "coin")
            {
                PlayCoinSound();
                return;
            }

            if (proceduralClips.TryGetValue(soundName, out AudioClip clip))
            {
                sfxSource.PlayOneShot(clip, sfxVolume * masterVolume * volumeMultiplier);
            }
        }

        public void PlayCoinSound()
        {
            float now = Time.time;
            if (now - lastCoinCollectTime < 0.65f)
            {
                coinStreak = Mathf.Clamp(coinStreak + 1, 0, 9);
            }
            else
            {
                coinStreak = 0;
            }
            lastCoinCollectTime = now;

            // Subway Surfers style chromatic pitch escalation (each consecutive coin rises 1 semitone)
            float pitch = Mathf.Pow(1.059463f, coinStreak);
            if (proceduralClips.TryGetValue("coin", out AudioClip clip) && coinSource != null)
            {
                coinSource.pitch = pitch;
                coinSource.PlayOneShot(clip, sfxVolume * masterVolume * 0.95f);
            }
        }

        public void StartEngineSound()
        {
            if (proceduralClips.TryGetValue("engine", out AudioClip clip))
            {
                if (engineSource.clip != clip)
                {
                    engineSource.clip = clip;
                }
                if (!engineSource.isPlaying)
                {
                    engineSource.Play();
                }
            }
        }

        public void StopEngineSound()
        {
            if (engineSource && engineSource.isPlaying)
            {
                engineSource.Stop();
            }
        }

        public void SetEnginePitch(float pitch)
        {
            if (engineSource)
            {
                engineSource.pitch = Mathf.Clamp(pitch, 0.7f, 1.8f);
            }
        }

        public void PlayAlarm(bool isCritical)
        {
            string key = isCritical ? "critical" : "alarm";
            if (proceduralClips.TryGetValue(key, out AudioClip clip))
            {
                if (alarmSource.clip != clip || !alarmSource.isPlaying)
                {
                    alarmSource.clip = clip;
                    alarmSource.Play();
                }
            }
        }

        public void StopAlarm()
        {
            if (alarmSource && alarmSource.isPlaying)
            {
                alarmSource.Stop();
            }
        }

        public void PlayMusic(string trackName = "bgm_orbit")
        {
            if (proceduralClips.TryGetValue(trackName, out AudioClip clip))
            {
                musicSource.clip = clip;
                musicSource.Play();
            }
        }

        public void StopMusic()
        {
            if (musicSource)
            {
                musicSource.Stop();
            }
        }

        #endregion
    }
}
