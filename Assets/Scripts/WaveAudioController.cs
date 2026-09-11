using System.Collections.Generic;
using UnityEngine;

namespace ConwayGame
{
    [RequireComponent(typeof(AudioSource))]
    public sealed class WaveAudioController : MonoBehaviour
    {
        private const string PositiveResourcesPath = "Audio/Positive";
        private const string NegativeResourcesPath = "Audio/Negative";

        [Header("Wave SFX")]
        [SerializeField] private AudioClip[] positiveClips;
        [SerializeField] private AudioClip[] negativeClips;
        [SerializeField, Min(1)] private int soundsPerWave = 3;
        [SerializeField, Range(0f, 1f)] private float volume = 1f;

        private readonly List<AudioClip> currentWaveClips = new List<AudioClip>();
        private AudioSource audioSource;
        private int nextClipIndex;

        private void Awake()
        {
            audioSource = GetComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.loop = false;
            audioSource.spatialBlend = 0f;

            LoadResourceClipsIfNeeded();
        }

        public void BeginWave(State waveState)
        {
            AudioClip[] sourceClips = waveState == State.Fill ? positiveClips : negativeClips;

            StopWave();

            List<AudioClip> candidates = BuildCandidateList(sourceClips);
            if (candidates.Count == 0)
            {
                Debug.LogWarning($"No audio clips are available for the {waveState} wave.", this);
                return;
            }

            Shuffle(candidates);

            int clipCount = Mathf.Min(soundsPerWave, candidates.Count);
            for (int i = 0; i < clipCount; i++)
            {
                AudioClip clip = candidates[i];
                clip.LoadAudioData();
                currentWaveClips.Add(clip);
            }
        }

        public void PlayNextWaveStep()
        {
            if (nextClipIndex >= currentWaveClips.Count)
            {
                return;
            }

            AudioClip clip = currentWaveClips[nextClipIndex];
            audioSource.PlayOneShot(clip, volume);
            nextClipIndex++;
        }

        public void StopWave()
        {
            currentWaveClips.Clear();
            nextClipIndex = 0;

            if (audioSource != null)
            {
                audioSource.Stop();
            }
        }

        private void LoadResourceClipsIfNeeded()
        {
            if (positiveClips == null || positiveClips.Length == 0)
            {
                positiveClips = Resources.LoadAll<AudioClip>(PositiveResourcesPath);
            }

            if (negativeClips == null || negativeClips.Length == 0)
            {
                negativeClips = Resources.LoadAll<AudioClip>(NegativeResourcesPath);
            }
        }

        private static List<AudioClip> BuildCandidateList(AudioClip[] sourceClips)
        {
            List<AudioClip> candidates = new List<AudioClip>();
            if (sourceClips == null)
            {
                return candidates;
            }

            foreach (AudioClip clip in sourceClips)
            {
                if (clip != null && !candidates.Contains(clip))
                {
                    candidates.Add(clip);
                }
            }

            return candidates;
        }

        private static void Shuffle(List<AudioClip> clips)
        {
            for (int i = clips.Count - 1; i > 0; i--)
            {
                int swapIndex = Random.Range(0, i + 1);
                (clips[i], clips[swapIndex]) = (clips[swapIndex], clips[i]);
            }
        }

        private void OnDisable()
        {
            StopWave();
        }
    }
}
