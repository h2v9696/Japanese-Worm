using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Berry.Utils;

[RequireComponent (typeof (AudioListener))]
[RequireComponent (typeof (AudioSource))]
public class AudioManager : SingletonMonoBehaviour<AudioManager> {


	AudioSource music, sfx;

	public bool isMute {
        get {
            return PlayerPrefs.GetFloat("IsMute") == 1 ? true: false;
        }
        set {
            PlayerPrefs.SetFloat("IsMute", value ? 1 : 0);
        }
    }

    public List<MusicTrack> tracks = new List<MusicTrack>();
    public List<Sound> sounds = new List<Sound>();
    Sound GetSoundByName(string name) {
        return sounds.Find(x => x.name == name);
    }

	//static List<string> mixBuffer = new List<string>();
	//static float mixBufferClearDelay = 0.05f;

//    public bool quiet_mode = false;
    public string currentTrack;

    public override void Awake() {
        AudioSource[] sources = GetComponents<AudioSource>();
        music = sources[0];
        sfx = sources[1];
    }

	// Coroutine responsible for limiting the frequency of playing sounds
    //IEnumerator MixBufferRoutine() {
    //    float time = 0;

    //    while (true) {
    //        time += Time.unscaledDeltaTime;
    //        yield return 0;
    //        if (time >= mixBufferClearDelay) {
    //            mixBuffer.Clear();
    //            time = 0;
    //        }
    //    }
    //}

	// Launching a music track
    public void PlayMusic(string trackName) {
        if (trackName != "")
            currentTrack = trackName;
		AudioClip to = null;
        foreach (MusicTrack track in tracks)
            if (track.name == trackName)
                to = track.track;
        StartCoroutine(Instance.CrossFade(to));
	}

	// A smooth transition from one to another music
	IEnumerator CrossFade(AudioClip to) {
		float delay = 0.3f;
		if (music.clip != null) {
			while (delay > 0) {		
                delay -= Time.unscaledDeltaTime;
				yield return 0;
			}
		}
		music.clip = to;
        if (to == null || isMute) {
            music.Stop();
            yield break;
        }
        delay = 0;
		if (!music.isPlaying) music.Play();
		while (delay < 0.3f) {
    
			delay += Time.unscaledDeltaTime;
			yield return 0;
		}
	}

	// A single sound effect
	public void Shot(string clip) {
        if (isMute) return;
        Sound sound = Instance.GetSoundByName(clip);

        if (sound != null) {
            if (sound.clips.Count == 0) return;
            Instance.sfx.PlayOneShot(sound.clips.GetRandom());
		}
	}

    // Turn on/off music
    public void MuteButton(bool value) {
        isMute = value;

        PlayMusic(isMute ? "" : currentTrack);
    }


    [System.Serializable]
    public class MusicTrack {
        public string name;
        public AudioClip track;
    }

    [System.Serializable]
    public class Sound {
        public string name;
        public List<AudioClip> clips = new List<AudioClip>();
    }
}
