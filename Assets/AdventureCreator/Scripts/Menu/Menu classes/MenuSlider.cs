/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"MenuSlider.cs"
 * 
 *	This MenuElement creates a slider for eg. volume control.
 * 
 */

using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AC
{

	public class MenuSlider : MenuElement
	{
		
		public string label;
		public bool doOutline;
		public int amount;
		public TextAnchor anchor;
		public Texture2D sliderTexture;
		public AC_SliderType sliderType;
		public int varID;

		
		public override void Declare ()
		{
			label = "Slider";
			doOutline = false;
			isVisible = true;
			isClickable = true;
			numSlots = 1;
			amount = 10;
			anchor = TextAnchor.MiddleLeft;
			sliderType = AC_SliderType.CustomScript;
			varID = 0;
			
			base.Declare ();
		}
		
		
		public void CopySlider (MenuSlider _element)
		{
			label = _element.label;
			doOutline = _element.doOutline;
			amount = _element.amount;
			anchor = _element.anchor;
			sliderTexture = _element.sliderTexture;
			sliderType = _element.sliderType;
			varID = _element.varID;

			base.Copy (_element);
		}
		
		
		#if UNITY_EDITOR
		
		public override void ShowGUI ()
		{
			EditorGUILayout.BeginVertical ("Button");
				label = EditorGUILayout.TextField ("Label text:", label);
				anchor = (TextAnchor) EditorGUILayout.EnumPopup ("Text alignment:", anchor);
				doOutline = EditorGUILayout.Toggle ("Outline text?", doOutline);
				
				EditorGUILayout.BeginHorizontal ();
					EditorGUILayout.LabelField ("Slider texture:", GUILayout.Width (145f));
					sliderTexture = (Texture2D) EditorGUILayout.ObjectField (sliderTexture, typeof (Texture2D), false, GUILayout.Width (70f), GUILayout.Height (30f));
				EditorGUILayout.EndHorizontal ();
			
				sliderType = (AC_SliderType) EditorGUILayout.EnumPopup ("Slider type:", sliderType);
				if (sliderType == AC_SliderType.CustomScript)
				{
					amount = EditorGUILayout.IntSlider ("Default value:", amount, 0, 10);
					ShowClipHelp ();
				}
				else if (sliderType == AC_SliderType.Variable)
				{
					varID = EditorGUILayout.IntField ("Global Variable ID:", varID);
				}
			EditorGUILayout.EndVertical ();
			
			base.ShowGUI ();
		}
		
		#endif
		
		
		public override void Display (GUIStyle _style, int _slot, float zoom, bool isActive)
		{
			base.Display (_style, _slot, zoom, isActive);
			
			_style.alignment = anchor;
			if (zoom < 1f)
			{
				_style.fontSize = (int) ((float) _style.fontSize * zoom);
			}
			
			if (doOutline)
			{
				AdvGame.DrawTextOutline (ZoomRect (relativeRect, zoom), TranslateLabel (label), _style, Color.black, _style.normal.textColor, 2);
			}
			else
			{
				GUI.Label (ZoomRect (relativeRect, zoom), TranslateLabel (label), _style);
			}
			
			if (sliderTexture)
			{
				Rect sliderRect = relativeRect;
				sliderRect.x = relativeRect.x + (relativeRect.width / 2);
				sliderRect.width = slotSize.x / 100 * AdvGame.GetMainGameViewSize ().x * (float) amount / 10 * 0.5f;
				GUI.DrawTexture (ZoomRect (sliderRect, zoom), sliderTexture, ScaleMode.StretchToFill, true, 0f);
			}
		}


		public override string GetLabel (int slot)
		{
			return TranslateLabel (label);
		}
		
		
		public void Change ()
		{
			amount ++; 
			
			if (amount > 10)
			{
				amount = 0;
			}
			
			if (sliderType == AC_SliderType.Speech || sliderType == AC_SliderType.SFX || sliderType == AC_SliderType.Music)
			{
				if (GameObject.FindWithTag (Tags.persistentEngine) && GameObject.FindWithTag (Tags.persistentEngine).GetComponent <Options>())
				{
					Options options = GameObject.FindWithTag (Tags.persistentEngine).GetComponent <Options>();
					
					if (sliderType == AC_SliderType.Speech)
					{
						options.optionsData.speechVolume = amount;
					}
					else if (sliderType == AC_SliderType.Music)
					{
						options.optionsData.musicVolume = amount;
						options.SetVolume (SoundType.Music);
					}
					else if (sliderType == AC_SliderType.SFX)
					{
						options.optionsData.sfxVolume = amount;
						options.SetVolume (SoundType.SFX);
					}		
					
					options.SavePrefs ();
				}
				else
				{
					Debug.LogWarning ("Could not find Options data!");
				}
			}
			else if (sliderType == AC_SliderType.Variable)
			{
				if (varID >= 0)
				{
					GVar var = RuntimeVariables.GetVariable (varID);
					if (var.type == VariableType.Integer)
					{
						var.val = amount;
						var.Upload ();
					}
				}
			}
		}
		
		
		public override void RecalculateSize ()
		{
			if (Application.isPlaying)
			{
				if (sliderType == AC_SliderType.Speech || sliderType == AC_SliderType.SFX || sliderType == AC_SliderType.Music)
				{
					if (GameObject.FindWithTag (Tags.persistentEngine) && GameObject.FindWithTag (Tags.persistentEngine).GetComponent <Options>())
					{	
						Options options = GameObject.FindWithTag (Tags.persistentEngine).GetComponent <Options>();
						if (options.optionsData != null)
						{
							if (sliderType == AC_SliderType.Speech)
							{
								amount = options.optionsData.speechVolume;
							}
							else if (sliderType == AC_SliderType.Music)
							{
								amount = options.optionsData.musicVolume;
							}
							else if (sliderType == AC_SliderType.SFX)
							{
								amount = options.optionsData.sfxVolume;
							}
						}
					}
				}
				else if (sliderType == AC_SliderType.Variable)
				{
					if (varID >= 0)
					{
						if (RuntimeVariables.GetVariable (varID).type != VariableType.Integer)
						{
							Debug.LogWarning ("Cannot link MenuToggle " + title + " to Variable " + varID + " as it is not an Integer.");
						}
						else
						{
							amount = Mathf.Clamp (RuntimeVariables.GetIntegerValue (varID), 0, 10);
						}
					}
				}
			}
			
			base.RecalculateSize ();
		}
		
		
		protected override void AutoSize ()
		{
			AutoSize (new GUIContent (TranslateLabel (label)));
		}

	}

}