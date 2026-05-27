using Microsoft.Xna.Framework.Audio;

namespace Engine.Specs {
    public interface IAudioService {
        float MasterVolume { get; set; }
        float SfxVolume { get; set; }
        float MusicVolume { get; set; }

        void LoadSound(string assetName, int poolSize = 0);
        void LoadMusic(string assetName);

        void PlaySound(int id, float volume = 1f, float pitch = 0f, float pan = 0f);
        SoundEffectInstance GetSoundInstance(int id);

        void PlayMusic(int id, bool loop = true);
        void StopMusic();
        void PauseMusic();
        void ResumeMusic();
    }
}