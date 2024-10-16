using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RL.Systems
{
    public class AudioManager : MonoBehaviour
    {
        public AudioSource AudioSource;
        Dictionary<string, AudioClip> _audioClipsDict = new();

        void Awake()
        {
            AudioSource = GetComponent<AudioSource>();
        }

        internal void Initialize()
        {
            var clips = Resources.LoadAll<AudioClip>("Audio");
            foreach (AudioClip clip in clips)
            {
                _audioClipsDict[clip.name] = clip;
            }
        }

        public void PlaySound(string name)
        {
            if (_audioClipsDict.ContainsKey(name))
            {
                Play(name);
                // AudioSource.clip = _audioClipsDict[name];
                // AudioSource.Play();
            }
        }

        public void Play(string name)
        {
            if (_audioClipsDict.TryGetValue(name, out var clip))
            {
                PlayClipAsNewSource(clip);
            }
        }

        public void PlayInWorld(string name, Vector3 position)
        {
            if (_audioClipsDict.TryGetValue(name, out var clip))
            {
                PlayClipAsNewSource3D(clip, position);
            }
        }
        
        public void PlayClipAsNewSource(AudioClip clip)
        {
            var source = InstantiateAudioSource();
            source.clip = clip;
            source.spatialBlend = 0f;
            source.Play();
            Destroy(source.gameObject, source.clip.length);
        }

        public void PlayClipAsNewSource3D(AudioClip clip, Vector3 position)
        {
            var source = InstantiateAudioSource();
            source.transform.position = position;
            source.clip = clip;
            source.spatialBlend = 1f;
            source.Play();
            Destroy(source.gameObject, source.clip.length);
        }

        AudioSource InstantiateAudioSource()
        {
            var go = new GameObject("Audio Source Instance");
            return go.AddComponent<AudioSource>();
        }
    }
}