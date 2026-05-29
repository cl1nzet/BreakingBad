using Engine.Specs;
using Engine.Utils;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Media;
using System;
using System.Collections.Generic;

namespace Engine.Core {
    public sealed class AudioService : IDisposable {
        private readonly Dictionary<int, SoundEffect> _sounds;
        private readonly Dictionary<int, Queue<SoundEffectInstance>> _soundPools;
        private readonly Dictionary<int, Song> _music;

        private float _masterVolume = 1f;
        private float _sfxVolume = 1f;
        private float _musicVolume = 1f;
        private int _currentMusicId = -1;
        private bool _isDisposed;

        public float MasterVolume
        {
            get => _masterVolume;
            set
            {
                _masterVolume = Math.Clamp(value, 0f, 1f);
                UpdateMusicVolume();
            }
        }

        public float SfxVolume
        {
            get => _sfxVolume;
            set => _sfxVolume = Math.Clamp(value, 0f, 1f);
        }

        public float MusicVolume
        {
            get => _musicVolume;
            set
            {
                _musicVolume = Math.Clamp(value, 0f, 1f);
                UpdateMusicVolume();
            }
        }

        public AudioService(int initialCapacity = 64) {
            _sounds = new Dictionary<int, SoundEffect>(initialCapacity);
            _soundPools = new Dictionary<int, Queue<SoundEffectInstance>>(initialCapacity);
            _music = new Dictionary<int, Song>(initialCapacity);

            MediaPlayer.IsRepeating = true;
            UpdateMusicVolume();
        }

        public void LoadSound(string assetName, int poolSize = 0)
        {
            int hash = AudioHash.Get(assetName);

            if (_sounds.ContainsKey(hash))
                return;

            var effect = AssetManager.GetSound(assetName);
            _sounds.Add(hash, effect);

            if (poolSize > 0)
            {
                var pool = new Queue<SoundEffectInstance>(poolSize);
                for (int i = 0; i < poolSize; i++)
                {
                    pool.Enqueue(effect.CreateInstance());
                }
                _soundPools.Add(hash, pool);
            }
        }

        public void LoadMusic(string assetName)
        {
            int hash = AudioHash.Get(assetName);

            if (!_music.ContainsKey(hash))
            {
                _music.Add(hash, AssetManager.GetMusic(assetName));
            }
        }

        public void PlaySound(int id, float volume = 1f, float pitch = 0f, float pan = 0f)
        {
            float finalVolume = volume * _sfxVolume * _masterVolume;

            if (finalVolume <= 0f)
                return;

            if (_soundPools.TryGetValue(id, out Queue<SoundEffectInstance> pool) && pool.Count > 0)
            {
                var instance = pool.Dequeue();

                if (instance.State == SoundState.Playing)
                    instance.Stop();

                instance.Volume = finalVolume;
                instance.Pitch = pitch;
                instance.Pan = pan;
                instance.Play();

                pool.Enqueue(instance);
                return;
            }

            if (_sounds.TryGetValue(id, out SoundEffect sound))
            {
                sound.Play(finalVolume, pitch, pan);
            }
        }

        public SoundEffectInstance GetSoundInstance(int id)
        {
            if (_sounds.TryGetValue(id, out SoundEffect sound))
            {
                return sound.CreateInstance();
            }

            return null;
        }

        public void PlayMusic(int id, bool loop = true) {
            if (_currentMusicId == id && MediaPlayer.State == MediaState.Playing)
                return;

            if (_music.TryGetValue(id, out Song song))
            {
                MediaPlayer.IsRepeating = loop;
                MediaPlayer.Play(song);
                _currentMusicId = id;
            }
        }

        public void PlayMusic(string assetName, bool loop = true)
        {
            int id = AudioHash.Get(assetName);
            PlayMusic(id, loop);
        }

        public void StopMusic() {
            MediaPlayer.Stop();
            _currentMusicId = -1;
        }
        public void PauseMusic() => MediaPlayer.Pause();
        public void ResumeMusic() => MediaPlayer.Resume();

        private void UpdateMusicVolume()
        {
            float targetVolume = _musicVolume * _masterVolume;

            if (Math.Abs(MediaPlayer.Volume - targetVolume) > 0.001f)
            {
                MediaPlayer.Volume = targetVolume;
            }
        }

        public void Dispose()
        {
            if (_isDisposed)
                return;

            StopMusic();

            foreach (var pool in _soundPools.Values)
            {
                foreach (var instance in pool)
                {
                    if (!instance.IsDisposed)
                        instance.Dispose();
                }
            }

            foreach (var sound in _sounds.Values)
            {
                if (!sound.IsDisposed)
                    sound.Dispose();
            }

            foreach (var song in _music.Values)
            {
                if (!song.IsDisposed)
                    song.Dispose();
            }

            _soundPools.Clear();
            _sounds.Clear();
            _music.Clear();

            _isDisposed = true;
        }
    }
}