/*
Component Title: Audio Manager
Data written: June 12, 2024
Date revised: October 27, 2024

Programmer/s:
    Gian Paolo Buenconsejo

Purpose:
    This component manages the entire game's audio.
    Contains methods necessary for handling in-game sound effects and music
        e.g., Play(), PlayInWorld(), PlayMusic(), StopMusic()

Data Structures:
    Dictionary: used to store the loaded audio clips for the game
        Key is the file name of the audio clip; Value is the Audio Clip data.
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RL.Systems
{
    public class AudioManager : MonoBehaviour
    {
        [Range(0, 1)] public float SoundVolume = 1;
        [Range(0, 1)] public float MusicVolume = 1;

        public AudioSource AudioSource;
        Dictionary<string, AudioClip> _audioClipsDict = new();
        Dictionary<string, AudioClip> _musicClipsDict = new();
        
        Dictionary<string, AudioSource> _playingMusicSources = new();


        void Awake()
        {
            AudioSource = GetComponent<AudioSource>();
        }

        internal void Initialize()
        {
            foreach (AudioClip clip in Resources.LoadAll<AudioClip>("Audio"))
            {
                _audioClipsDict[clip.name] = clip;
            }

            foreach (AudioClip musicClip in Resources.LoadAll<AudioClip>("Music"))
            {
                _musicClipsDict[musicClip.name] = musicClip;
            }
        }

        public void Play(string name)
        {
            if (_audioClipsDict.TryGetValue(name, out var clip))
            {
                PlayClipAsNewSource(clip);
            }
        }

        public void PlayMusic(string id, bool loop = true)
        {
            if (_musicClipsDict.TryGetValue(id, out var clip))
            {
                if (!_playingMusicSources.TryGetValue(id, out _))
                {
                    var source = PlayClipAsNewSource(clip, !loop);
                    source.transform.SetParent(transform);
                    source.loop = loop;
                    source.volume = MusicVolume;
                    _playingMusicSources[id] = source;
                }
            }
        }

        public void StopMusic(string id)
        {
            if (_playingMusicSources.TryGetValue(id, out var source))
            {
                _playingMusicSources.Remove(id);
                Destroy(source.gameObject);
            }
        }

        public void PlayInWorld(string name, Vector3 position)
        {
            if (_audioClipsDict.TryGetValue(name, out var clip))
            {
                PlayClipAsNewSource3D(clip, position);
            }
        }
        
        public AudioSource PlayClipAsNewSource(AudioClip clip, bool destroyOnDone = true)
        {
            var source = InstantiateAudioSource();
            source.clip = clip;
            source.spatialBlend = 0f;
            source.volume = SoundVolume;
            source.Play();
            if (destroyOnDone) Destroy(source.gameObject, source.clip.length);
            return source;
        }

        public AudioSource PlayClipAsNewSource3D(AudioClip clip, Vector3 position, bool destroyOnDone = true)
        {
            var source = InstantiateAudioSource();
            source.transform.position = position;
            source.clip = clip;
            source.spatialBlend = 1f;
            source.Play();
            if (destroyOnDone) Destroy(source.gameObject, source.clip.length);
            return source;
        }

        AudioSource InstantiateAudioSource()
        {
            var go = new GameObject("Audio Source Instance");
            return go.AddComponent<AudioSource>();
        }
    }
}