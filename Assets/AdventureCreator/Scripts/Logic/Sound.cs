/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"Sound.cs"
 * 
 *	This script allows for easy playback of audio sources from within the ActionList system.
 * 
 */

using UnityEngine;
using System.Collections;

namespace AC
{

	public class Sound : MonoBehaviour
	{
		public SoundType soundType;
		public bool playWhilePaused = false;
		public float relativeVolume = 1f;
		public bool surviveSceneChange = false;

		private float maxVolume = 1f;
		private float fadeStartTime;
		private float fadeEndTime;
		private FadeType fadeType;
		private bool isFading = false;

		private Options options;


		private void Awake ()
		{
			if (soundType == SoundType.Music && surviveSceneChange)
			{
				DontDestroyOnLoad (this);
			}
		}
		
		
		private void Start ()
		{
			if (GetComponent <AudioSource>())
			{
				GetComponent <AudioSource>().ignoreListenerPause = playWhilePaused;
			}
			else
			{
				Debug.LogWarning ("Sound object " + this.name + " has no AudioSource component.");
			}

			SetMaxVolume ();
		}
		
		
		private void Update ()
		{
			if (isFading && audio.isPlaying)
			{
				float progress = (Time.time - fadeStartTime) / (fadeEndTime - fadeStartTime);
				
				if (fadeType == FadeType.fadeIn)
				{
					if (progress > 1f)
					{
						audio.volume = maxVolume;
						isFading = false;
					}
					else
					{
						audio.volume = progress * maxVolume;
					}
				}
				else if (fadeType == FadeType.fadeOut)
				{
					if (progress > 1f)
					{
						audio.volume = 0f;
						audio.Stop ();
						isFading = false;
					}
					else
					{
						audio.volume = (1 - progress) * maxVolume;
					}
				}
			}
		}
		
		
		public void Interact ()
		{
			isFading = false;
			SetMaxVolume ();
			Play (audio.loop);
		}
		
		
		public void FadeIn (float fadeTime, bool loop)
		{
			audio.loop = loop;
			
			fadeStartTime = Time.time;
			fadeEndTime = Time.time + fadeTime;
			fadeType = FadeType.fadeIn;
			
			SetMaxVolume ();
			isFading = true;
			audio.volume = 0f;
			audio.Play ();
		}
		
		
		public void FadeOut (float fadeTime)
		{
			if (audio.isPlaying)
			{
				fadeStartTime = Time.time;
				fadeEndTime = Time.time + fadeTime;
				fadeType = FadeType.fadeOut;
				
				SetMaxVolume ();
				isFading = true;
			}
		}
		
		
		public void Play (bool loop)
		{
			audio.loop = loop;
			isFading = false;
			SetMaxVolume ();
			audio.Play ();
		}
		
		
		public void SetMaxVolume ()
		{
			maxVolume = relativeVolume;
			
			if (GameObject.FindWithTag (Tags.persistentEngine) && GameObject.FindWithTag (Tags.persistentEngine).GetComponent <Options>())
			{
				options = GameObject.FindWithTag (Tags.persistentEngine).GetComponent <Options>();
			}

			if (options && options.optionsData != null && soundType != SoundType.Other)
			{
				if (soundType == SoundType.Music)
				{
					maxVolume *= (float) options.optionsData.musicVolume / 10;
				}
				else if (soundType == SoundType.SFX)
				{
					maxVolume *= (float) options.optionsData.sfxVolume / 10;
				}
			}
			
			if (!isFading)
			{
				audio.volume = maxVolume;
			}
		}
		
		
		public void Stop ()
		{
			audio.Stop ();
		}


		public void EndOldMusic (Sound newSound)
		{
			if (soundType == SoundType.Music && audio.isPlaying && this != newSound)
			{
				if (!isFading || fadeType == FadeType.fadeIn)
				{
					FadeOut (0.1f);
				}
			}
		}


		public bool IsPlayingSameMusic (AudioClip clip)
		{
			if (soundType == SoundType.Music && audio != null && clip != null && audio.clip != null && audio.clip == clip && audio.isPlaying)
			{
				return true;
			}

			return false;
		}


		public bool canDestroy
		{
			get
			{
				if (soundType == SoundType.Music && surviveSceneChange && !audio.isPlaying)
				{
					return true;
				}

				return false;
			}
		}

	}

}