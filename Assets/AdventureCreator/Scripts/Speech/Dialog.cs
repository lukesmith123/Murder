/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"Dialog.cs"
 * 
 *	This script handles the running of dialogue lines, speech or otherwise.
 * 
 */

using UnityEngine;
using System.Collections;
using System.IO;
using AC;

public class Dialog : MonoBehaviour
{

	public bool isMessageAlive { get; set; }
	public bool foundAudio { get; set; }

	private AC.Char speakerChar;
	private string speakerName;
	
	private float alpha;
	private bool isSkippable = false;
	private string displayText = "";
	private string fullText = "";
	private float textWait;
	private float endTime;
	
	private PlayerInput playerInput;
	private SpeechManager speechManager;
	private SettingsManager settingsManager;
	private StateHandler stateHandler;

	
	private void Awake ()
	{
		playerInput = this.GetComponent <PlayerInput>();
		
		if (AdvGame.GetReferences () == null)
		{
			Debug.LogError ("A References file is required - please use the Adventure Creator window to create one.");
		}
		else
		{
			speechManager = AdvGame.GetReferences ().speechManager;
			settingsManager = AdvGame.GetReferences ().settingsManager;

			if (speechManager.textScrollSpeed == 0f)
			{
				Debug.LogError ("Cannot have a Text Scroll Speed of zero - please amend your Speech Manager");
			}
		}
	}


	private void Start ()
	{
		stateHandler = GameObject.FindWithTag  (Tags.persistentEngine).GetComponent <StateHandler>();
	}
	
	
	private void FixedUpdate ()
	{
		if (isSkippable && isMessageAlive)
		{
			if (playerInput && playerInput.buttonPressed > 0 && speechManager && speechManager.allowSpeechSkipping && playerInput.hasUnclickedSinceClick)
			{
				if (endTime - textWait + 0.5f > Time.time)
				{
					return;
				}

				if ((playerInput.CanClick ()) || (speechManager.allowGameplaySpeechSkipping && stateHandler.gameState == GameState.Normal))
				{
					playerInput.ResetClick ();

					if (speechManager.endScrollBeforeSkip && speechManager.scrollSubtitles && displayText != fullText)
					{
						// Stop scrolling
						StopCoroutine ("StartMessage");
						displayText = fullText;
					}
					else
					{
						// Stop message
						StartCoroutine ("EndMessage");
					}
				}
			}
			
			else if (Time.time > endTime)
			{
				// Stop message due to timeout
				StartCoroutine ("EndMessage");
			}
		}
	}
	
	
	public string GetSpeaker ()
	{
		if (speakerChar)
		{
			return speakerChar.name;
		}
		
		return "";
	}


	public AC.Char GetSpeakingCharacter ()
	{
		return speakerChar;
	}


	public Vector2 GetCharScreenCentre ()
	{
		Vector3 worldPosition = speakerChar.transform.position;

		if (speakerChar.collider is CapsuleCollider)
		{
			CapsuleCollider capsuleCollder = (CapsuleCollider) speakerChar.collider;

			float addedHeight = capsuleCollder.height * speakerChar.transform.localScale.y;

			if (speakerChar.spriteChild != null && speakerChar.spriteChild.GetComponent <SpriteRenderer>())
			{
				addedHeight *= speakerChar.spriteChild.localScale.y;
			}

			if (settingsManager && settingsManager.IsTopDown ())
			{
				worldPosition.z += addedHeight;
			}
			else
			{
				worldPosition.y += addedHeight;
			}
		}
		else
		{
			if (speakerChar.spriteChild != null)
			{
				if (speakerChar.spriteChild.GetComponent <SpriteRenderer>())
				{
					worldPosition.y = speakerChar.spriteChild.GetComponent <SpriteRenderer>().bounds.extents.y + speakerChar.spriteChild.GetComponent <SpriteRenderer>().bounds.center.y;
				}
				else if (speakerChar.spriteChild.GetComponent <Renderer>())
				{
					worldPosition.y = speakerChar.spriteChild.GetComponent <Renderer>().bounds.extents.y + speakerChar.spriteChild.GetComponent <Renderer>().bounds.center.y;
				}
			}
		}

		Vector3 screenPosition = Camera.main.WorldToViewportPoint (worldPosition);
		return (new Vector2 (screenPosition.x, 1 - screenPosition.y));
	}
	
	
	public Texture2D GetPortrait ()
	{
		if (speakerChar && speakerChar.portraitGraphic)
		{
			return speakerChar.portraitGraphic;
		}
		
		return null;
	}


	public Color GetColour ()
	{
		if (speakerChar)
		{
			return speakerChar.speechColor;
		}

		return Color.white;
	}
	
	
	public string GetLine ()
	{
		if (isMessageAlive && isSkippable)
		{
			return displayText;
		}
		return "";
	}
	
	
	public string GetFullLine ()
	{
		if (isMessageAlive && isSkippable)
		{
			return fullText;
		}
		return "";
	}


	private IEnumerator EndMessage ()
	{
		StopCoroutine ("StartMessage");
		isSkippable = false;
		
		if (speakerChar)
		{
			speakerChar.isTalking = false;
			
			// Turn off animations on the character's "mouth" layer
			if (speakerChar.GetComponent <Animation>())
			{
				foreach (AnimationState state in speakerChar.GetComponent <Animation>().animation)
				{
					if (state.layer == (int) AnimLayer.Mouth)
					{
						state.normalizedTime = 1f;
						state.weight = 0f;
					}
				}
			}
			
			if (speakerChar.GetComponent <AudioSource>())
			{
				speakerChar.GetComponent<AudioSource>().Stop();
			}
		}
		
		// Wait a short moment for fade-out
		yield return new WaitForSeconds (0.3f);
		isMessageAlive = false;
	}
	
	
	private IEnumerator StartMessage (string message)
	{
		isMessageAlive = true;
		isSkippable = true;
	
		displayText = "";
		fullText = message;
		
		endTime = textWait + Time.time;

		if (speechManager.scrollSubtitles)
		{
			// Start scroll the message
			for (int i = 0; i < message.Length; i++)
			{
				displayText += message[i];
				yield return new WaitForSeconds (1 / speechManager.textScrollSpeed);
			}
		}
		else
		{
			displayText = message;
			yield return new WaitForSeconds (message.Length / speechManager.textScrollSpeed);
		}
		
		if (endTime == Time.time)
		{
			endTime += 2f;
		}
	}

	
	public void StartDialog (AC.Char _speakerChar, string message, int lineNumber, string language)
	{
		isMessageAlive = false;
		
		if (_speakerChar)
		{
			speakerChar = _speakerChar;
			speakerChar.isTalking = true;
			
			speakerName = _speakerChar.name;
			if (_speakerChar.GetComponent <Player>())
			{
				speakerName = "Player";
			}
		
			if (_speakerChar.GetComponent <Hotspot>())
			{
				if (_speakerChar.GetComponent <Hotspot>().hotspotName != "")
				{
					speakerName = _speakerChar.GetComponent <Hotspot>().hotspotName;
				}
			}
		}
		else
		{
			if (speakerChar)
			{
				speakerChar.isTalking = false;
			}
			speakerChar = null;
			
			speakerName = "Narrator";
		}
		
		// Play sound and time textWait to it
		if (lineNumber > -1 && speakerName != "" && speechManager.searchAudioFiles)
		{
			string filename = "Speech/";
			if (language != "" && speechManager.translateAudio)
			{
				// Not in original language
				filename += language + "/";
			}
			filename += speakerName + lineNumber;
			
			foundAudio = false;
			AudioClip clipObj = Resources.Load(filename) as AudioClip;
			if (clipObj)
			{
				if (_speakerChar.GetComponent <AudioSource>())
				{
					if (GameObject.FindWithTag (Tags.persistentEngine) && GameObject.FindWithTag (Tags.persistentEngine).GetComponent <Options>())
					{
						Options options = GameObject.FindWithTag (Tags.persistentEngine).GetComponent <Options>();
						_speakerChar.GetComponent <AudioSource>().volume = options.optionsData.speechVolume / 10f;
					}
					_speakerChar.GetComponent <AudioSource>().clip = clipObj;
					_speakerChar.GetComponent <AudioSource>().loop = false;
					_speakerChar.GetComponent <AudioSource>().Play();
					
					foundAudio = true;
				}
				else
				{
					Debug.LogWarning (_speakerChar.name + " has no audio source component!");
				}
				
				textWait = clipObj.length;
			}
			else
			{
				textWait = speechManager.screenTimeFactor * (float) message.Length;
				if (textWait < 0.5f)
				{
					textWait = 0.5f;
				}
				
				Debug.Log ("Cannot find audio file: " + filename);
			}
		}
		else
		{
			textWait = speechManager.screenTimeFactor * (float) message.Length;
			if (textWait < 0.5f)
			{
				textWait = 0.5f;
			}
		}
		
		StopCoroutine ("StartMessage");
		StartCoroutine ("StartMessage", message);
	}
	

	public void KillDialog ()
	{
		isSkippable = false;
		isMessageAlive = false;
		
		StopCoroutine ("StartMessage");
		StopCoroutine ("EndMessage");

		if (speakerChar && speakerChar.GetComponent <AudioSource>())
		{
			speakerChar.GetComponent <AudioSource>().Stop();
		}
	}

	
	private void OnDestroy ()
	{
		playerInput = null;
		speakerChar = null;
		speechManager = null;
		settingsManager = null;
	}

}