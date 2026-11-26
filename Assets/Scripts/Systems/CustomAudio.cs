using UnityEngine;

namespace NooMiBetter
{
    public sealed class CustomAudio : MonoBehaviour
    {
        [SerializeField] private AudioSource musicSource;
        [SerializeField] private AudioSource effectSource;
        public void SetMusicVolume(float value) { if (musicSource) musicSource.volume = Mathf.Clamp01(value); }
        public void SetEffectsVolume(float value) { if (effectSource) effectSource.volume = Mathf.Clamp01(value); }
        public void PlayCatch() { if (effectSource && effectSource.clip) effectSource.PlayOneShot(effectSource.clip); }
        public void PlayMusic() { if (musicSource && musicSource.clip) musicSource.Play(); }
        public void StopMusic() { if (musicSource) musicSource.Stop(); }
    }
}
