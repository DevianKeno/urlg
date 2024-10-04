using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RL.Systems
{
    public class AudioManager : MonoBehaviour
    {
        public AudioSource AudioSource;
        Dictionary<string, AudioClip> _audioClips = new();

        void Awake()
        {
            AudioSource = GetComponent<AudioSource>();
        }

        internal void Initialize()
        {
            var clips = Resources.LoadAll<AudioClip>("Audio");
            foreach (AudioClip clip in clips)
            {
                _audioClips[clip.name] = clip;
            }
        }

        public void PlaySound(string name)
        {
            if (_audioClips.ContainsKey(name))
            {
                AudioSource.clip = _audioClips[name];
                AudioSource.Play();
            }
        }
    }
}