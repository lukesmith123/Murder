/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"ActionSpeech.cs"
 * 
 *	This action handles the displaying of messages, and talking of characters.
 * 
 */

using UnityEngine;
using System.Collections;
using AC;

#if UNITY_EDITOR
using UnityEditor;
#endif

[System.Serializable]
public class ActionSpeech : Action
{

	public int constantID = 0;
	
	public bool isPlayer;
	public Char speaker;
	public string messageText;
	public int lineID;
	public bool isBackground = false;
	public AnimationClip headClip;
	public AnimationClip mouthClip;

	public bool play2DHeadAnim = false;
	public string headClip2D = "";
	public int headLayer;
	public bool play2DMouthAnim = false;
	public string mouthClip2D = "";
	public int mouthLayer;

	private int splitNumber = 0;
	private bool splitDelay = false;

	private Dialog dialog;
	private StateHandler stateHandler;
	private SpeechManager speechManager;

	
	public ActionSpeech ()
	{
		this.isDisplayed = true;
		title = "Dialogue: Play speech";
		lineID = -1;
	}


	override public float Run ()
	{
		if (speechManager == null)
		{
			speechManager = AdvGame.GetReferences ().speechManager;
		}
		if (speechManager == null)
		{
			Debug.Log ("No Speech Manager present");
			return 0f;
		}

		dialog = GameObject.FindWithTag(Tags.gameEngine).GetComponent <Dialog>();
		stateHandler = GameObject.FindWithTag (Tags.persistentEngine).GetComponent <StateHandler>();

		if (dialog && stateHandler)
		{
			if (!isRunning)
			{
				isRunning = true;
				splitDelay = false;
				splitNumber = 0;
				
				return StartSpeech ();
			}
			else
			{
				if (!dialog.isMessageAlive)
				{
					if (speechManager.separateLines)
					{
						if (!splitDelay)
						{
							// Begin pause if more lines are present
							splitNumber ++;
							string[] textArray = messageText.Split ('\n');

							if (textArray.Length > splitNumber)
							{
								// Still got more to go, so pause for a moment
								splitDelay = true;
								return speechManager.separateLinePause;
							}
							else
							{
								// Finished
								isRunning = false;
								stateHandler.gameState = GameState.Cutscene;
								return 0f;
							}
						}
						else
						{
							// Show next line
							splitDelay = false;
							return StartSpeech ();
						}
					}
					else
					{
						isRunning = false;
						stateHandler.gameState = GameState.Cutscene;
						return 0f;
					}
				}
				else
				{
					return defaultPauseTime;
				}
			}
		}
		
		return 0f;
	}


	override public void Skip ()
	{
		GetAssetFile ();
		GameObject.FindWithTag(Tags.gameEngine).GetComponent <Dialog>().KillDialog ();

		if (speaker)
		{
			speaker.isTalking = false;

			if (speaker.animEngine == null)
			{
				speaker.ResetAnimationEngine ();
			}
			
			if (speaker.animEngine != null)
			{
				speaker.animEngine.ActionSpeechSkip (this);
			}
		}
	}

	
	#if UNITY_EDITOR

	override public void ShowGUI ()
	{
		if (lineID > -1)
		{
			EditorGUILayout.LabelField ("Speech Manager ID:", lineID.ToString ());
		}
		
		isPlayer = EditorGUILayout.Toggle ("Player line?",isPlayer);
		if (isPlayer)
		{
			if (Application.isPlaying)
			{
				if (GameObject.FindWithTag (Tags.player) && GameObject.FindWithTag (Tags.player).GetComponent <Player>())
				{
					speaker = GameObject.FindWithTag (Tags.player).GetComponent <Player>();
				}
			}
			else
			{
				speaker = AdvGame.GetReferences ().settingsManager.GetDefaultPlayer ();
			}
		}
		else
		{
			speaker = (Char) EditorGUILayout.ObjectField ("Speaker:", speaker, typeof(Char), true);

			if (speaker && speaker.GetComponent <ConstantID>())
			{
				constantID = speaker.GetComponent <ConstantID>().constantID;
			}
		}

		EditorGUILayout.LabelField ("Line text:", GUILayout.Width (145f));
		EditorStyles.textField.wordWrap = true;
		messageText = EditorGUILayout.TextArea (messageText);

		if (speaker)
		{
			if (speaker.animEngine == null)
			{
				speaker.ResetAnimationEngine ();
			}
			if (speaker.animEngine)
			{
				speaker.animEngine.ActionSpeechGUI (this);
			}
		}
		else
		{
			EditorGUILayout.HelpBox ("If no Character is set, this line will be considered to be a Narration.", MessageType.Info);
		}
		
		isBackground = EditorGUILayout.Toggle ("Play in background?", isBackground);

		AfterRunningOption ();
	}


	override public string SetLabel ()
	{
		string labelAdd = "";
		
		if (messageText != "")
		{
			string shortMessage = messageText;
			if (shortMessage != null)
			{
				if (shortMessage.Contains ("\n"))
				{
					shortMessage = shortMessage.Replace ("\n", "");
				}
				if (shortMessage.Length > 30)
				{
					shortMessage = shortMessage.Substring (0, 28) + "..";
				}
			}

			labelAdd = " (" + shortMessage + ")";
		}
		
		return labelAdd;
	}

	#endif


	private float StartSpeech ()
	{
		string _text = messageText;
		string _language = "";
		
		if (Options.GetLanguage () > 0)
		{
			// Not in original language, so pull translation in from Speech Manager
			_text = SpeechManager.GetTranslation (lineID, Options.GetLanguage ());
			_language = speechManager.languages [Options.GetLanguage ()];
		}
		
		if (speechManager.separateLines)
		{
			// Split line into an array, and pull the correct one
			string[] textArray = _text.Split ('\n');
			_text = textArray [splitNumber];
		}
		
		_text = AdvGame.ConvertTokens (_text);
		
		if (_text != "")
		{
			dialog.KillDialog ();
			GetAssetFile ();
			
			dialog.StartDialog (speaker, _text, lineID, _language);
				
			if (speaker)
			{
				if (speaker.animEngine == null)
				{
					speaker.ResetAnimationEngine ();
				}
				
				if (speaker.animEngine != null)
				{
					speaker.animEngine.ActionSpeechRun (this);
				}
			}

			if (!isBackground || speechManager.separateLines)
			{
				return defaultPauseTime;
			}
		}
		
		return 0f;
	}


	private void GetAssetFile ()
	{
		if (isPlayer)
		{
			if (GameObject.FindWithTag (Tags.player) && GameObject.FindWithTag (Tags.player).GetComponent <AC.Player>())
			{
				speaker = GameObject.FindWithTag(Tags.player).GetComponent <AC.Player>();
			}
		}
		else if (isAssetFile && constantID != 0)
		{
			// Attempt to find the correct scene object
			ConstantID idObject = Serializer.returnComponent <ConstantID> (constantID);
			if (idObject != null && idObject.GetComponent <Char>())
			{
				speaker = idObject.GetComponent <Char>();
			}
		}
	}

}