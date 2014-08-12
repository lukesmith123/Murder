/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"MenuElement.cs"
 * 
 *	This is the base class for all menu elements.  It should never
 *	be added itself to a menu, as it is only a container of shared data.
 * 
 */

using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AC
{

	[System.Serializable]
	public class MenuElement : ScriptableObject
	{
		public int ID;
		public bool isEditing = false;
		public string title = "Element";
		public Vector2 slotSize;
		public AC_SizeType sizeType;
		public AC_PositionType2 positionType;
		public float slotSpacing = 0f;
		public int lineID = -1;

		public Font font;
		public float fontScaleFactor = 60f;
		public Color fontColor = Color.white;
		public Color fontHighlightColor = Color.white;
		public Texture2D highlightTexture;
		
		public bool isVisible;
		public bool isClickable;
		public ElementOrientation orientation = ElementOrientation.Vertical;
		public int gridWidth = 3;

		public Texture2D backgroundTexture;

		public AudioClip hoverSound;
		public AudioClip clickSound;
		
		[SerializeField] protected Rect relativeRect;
		[SerializeField] protected Vector2 relativePosition;
		[SerializeField] protected int numSlots;
		

		public virtual void Declare ()
		{
			fontScaleFactor = 2f;
			fontColor = Color.white;
			fontHighlightColor = Color.white;
			highlightTexture = null;
			orientation = ElementOrientation.Vertical;
			positionType = AC_PositionType2.Aligned;
			sizeType = AC_SizeType.Automatic;
			gridWidth = 3;
			lineID = -1;
			hoverSound = null;
			clickSound = null;
		}
		
		
		public virtual void Copy (MenuElement _element)
		{
			ID = _element.ID;
			isEditing = false;
			title = _element.title;
			slotSize = _element.slotSize;
			sizeType = _element.sizeType;
			positionType = _element.positionType;
			relativeRect = _element.relativeRect;
			numSlots = _element.numSlots;
			lineID = _element.lineID;
			slotSpacing = _element.slotSpacing;
		
			font = _element.font;
			fontScaleFactor = _element.fontScaleFactor;
			fontColor = _element.fontColor;
			fontHighlightColor = _element.fontHighlightColor;
			highlightTexture = _element.highlightTexture;
			
			isVisible = _element.isVisible;
			isClickable = _element.isClickable;
			orientation = _element.orientation;
			gridWidth = _element.gridWidth;

			backgroundTexture = _element.backgroundTexture;

			hoverSound = _element.hoverSound;
			clickSound = _element.clickSound;

			relativePosition = _element.relativePosition;
		}


		protected string TranslateLabel (string label)
		{
			if (Options.GetLanguage () > 0 && lineID > -1)
			{
				return (SpeechManager.GetTranslation (lineID, Options.GetLanguage ()));
			}
			else
			{
				return (label);
			}
		}


		public virtual string GetLabel (int slot)
		{
			return "";
		}

		
		#if UNITY_EDITOR
		
		public void ShowGUIStart ()
		{
			EditorGUILayout.BeginVertical ("Button");
				title = EditorGUILayout.TextField ("Element name:", title);
				isVisible = EditorGUILayout.Toggle ("Is visible?", isVisible);
			EditorGUILayout.EndVertical ();
			
			ShowGUI ();
		}
		
		
		public virtual void ShowGUI ()
		{
			EditorGUILayout.BeginVertical ("Button");
				font = (Font) EditorGUILayout.ObjectField ("Font:", font, typeof (Font), false);
				fontScaleFactor = EditorGUILayout.Slider ("Text size:", fontScaleFactor, 1f, 4f);
				fontColor = EditorGUILayout.ColorField ("Text colour:", fontColor);
				if (isClickable)
				{
					fontHighlightColor = EditorGUILayout.ColorField ("Text colour (highlighted):", fontHighlightColor);
				}
			EditorGUILayout.EndVertical ();
			
			EditorGUILayout.BeginVertical ("Button");
				if (isClickable)
				{
					EditorGUILayout.BeginHorizontal ();
						EditorGUILayout.LabelField ("Highlight texture:", GUILayout.Width (145f));
						highlightTexture = (Texture2D) EditorGUILayout.ObjectField (highlightTexture, typeof (Texture2D), false, GUILayout.Width (70f), GUILayout.Height (30f));
					EditorGUILayout.EndHorizontal ();

					hoverSound = (AudioClip) EditorGUILayout.ObjectField ("Hover sound:", hoverSound, typeof (AudioClip), false);
					clickSound = (AudioClip) EditorGUILayout.ObjectField ("Click sound:", clickSound, typeof (AudioClip), false);
				}
				
				EditorGUILayout.BeginHorizontal ();
					EditorGUILayout.LabelField ("Background texture:", GUILayout.Width (145f));
					backgroundTexture = (Texture2D) EditorGUILayout.ObjectField (backgroundTexture, typeof (Texture2D), false, GUILayout.Width (70f), GUILayout.Height (30f));
				EditorGUILayout.EndHorizontal ();
			EditorGUILayout.EndVertical ();
			
			EndGUI ();
		}
		
		
		public void EndGUI ()
		{
			EditorGUILayout.BeginVertical ("Button");
				positionType = (AC_PositionType2) EditorGUILayout.EnumPopup ("Position:", positionType);
				if (positionType == AC_PositionType2.Absolute)
				{
					EditorGUILayout.BeginHorizontal ();
						EditorGUILayout.LabelField ("X:", GUILayout.Width (15f));
						relativeRect.x = EditorGUILayout.FloatField (relativeRect.x);
						EditorGUILayout.LabelField ("Y:", GUILayout.Width (15f));
						relativeRect.y = EditorGUILayout.FloatField (relativeRect.y);
					EditorGUILayout.EndHorizontal ();
				}
				else if (positionType == AC_PositionType2.RelativeToMenuSize)
				{
					EditorGUILayout.BeginHorizontal ();
						EditorGUILayout.LabelField ("X:", GUILayout.Width (15f));
						relativePosition.x = EditorGUILayout.Slider (relativePosition.x, 0f, 100f);
						EditorGUILayout.LabelField ("Y:", GUILayout.Width (15f));
						relativePosition.y = EditorGUILayout.Slider (relativePosition.y, 0f, 100f);
					EditorGUILayout.EndHorizontal ();
				}
			EditorGUILayout.EndVertical ();
			
			EditorGUILayout.BeginVertical ("Button");
				sizeType = (AC_SizeType) EditorGUILayout.EnumPopup ("Size:", sizeType);
				if (sizeType == AC_SizeType.Manual)
				{
					EditorGUILayout.BeginHorizontal ();
					EditorGUILayout.LabelField ("W:", GUILayout.Width (17f));
					slotSize.x = EditorGUILayout.Slider (slotSize.x, 0f, 100f);
					EditorGUILayout.LabelField ("H:", GUILayout.Width (15f));
					slotSize.y = EditorGUILayout.Slider (slotSize.y, 0f, 100f);
					EditorGUILayout.EndHorizontal ();
				}
				else if (sizeType == AC_SizeType.Absolute)
				{
					EditorGUILayout.BeginHorizontal ();
					EditorGUILayout.LabelField ("Width:", GUILayout.Width (50f));
					slotSize.x = EditorGUILayout.FloatField (slotSize.x);
					EditorGUILayout.LabelField ("Height:", GUILayout.Width (50f));
					slotSize.y = EditorGUILayout.FloatField (slotSize.y);
					EditorGUILayout.EndHorizontal ();
				}
			EditorGUILayout.EndVertical ();
		}
		
		
		protected void ShowClipHelp ()
		{
			EditorGUILayout.HelpBox ("MenuSystem.OnElementClick will be run when this element is clicked.", MessageType.Info);
		}
		
		#endif


		public virtual void Display (GUIStyle _style, int _slot, float zoom, bool isActive)
		{
			if (backgroundTexture && _slot == 0)
			{
				GUI.DrawTexture (ZoomRect (relativeRect, zoom), backgroundTexture, ScaleMode.StretchToFill, true, 0f);
			}
		}


		protected virtual Rect ZoomRect (Rect rect, float zoom)
		{
			if (zoom == 1f)
			{
				return rect;
			}

			return (new Rect (rect.x * zoom, rect.y * zoom, rect.width * zoom, rect.height * zoom));
		}
		
		
		public Vector2 GetSize ()
		{
			Vector2 size = new Vector2 (relativeRect.width, relativeRect.height);
			return (size);
		}
		
		
		public Vector2 GetSizeFromCorner ()
		{
			Vector2 size = new Vector2 (relativeRect.width + relativeRect.x, relativeRect.height + relativeRect.y);
			return (size);
		}
		
		
		public void SetSize (Vector2 _size)
		{
			slotSize = new Vector2 (_size.x, _size.y);
		}
		
		
		protected void SetAbsoluteSize (Vector2 _size)
		{
			slotSize = new Vector2 (_size.x * 100f / AdvGame.GetMainGameViewSize ().x, _size.y * 100f / AdvGame.GetMainGameViewSize ().y);
		}


		public int GetNumSlots ()
		{
			return numSlots;
		}
		
		
		public Rect GetSlotRectRelative (int _slot)
		{
			Rect positionRect = relativeRect;
			if (sizeType == AC_SizeType.Absolute)
			{
				positionRect.width = slotSize.x;
				positionRect.height = slotSize.y;
			}
			else
			{
				positionRect.width = slotSize.x / 100f * AdvGame.GetMainGameViewSize ().x;
				positionRect.height = slotSize.y / 100f * AdvGame.GetMainGameViewSize ().y;
			}
			
			if (_slot > numSlots)
			{
				_slot = numSlots;
			}
			
			if (orientation == ElementOrientation.Horizontal)
			{
				positionRect.x += (slotSize.x + slotSpacing) / 100f * _slot * AdvGame.GetMainGameViewSize ().x;
			}
			else if (orientation == ElementOrientation.Vertical)
			{
				positionRect.y += (slotSize.y + slotSpacing) / 100f * _slot * AdvGame.GetMainGameViewSize ().y;
			}
			else if (orientation == ElementOrientation.Grid)
			{
				int xOffset = _slot + 1;
				float numRows = Mathf.CeilToInt ((float) xOffset / gridWidth) - 1;
				while (xOffset > gridWidth)
				{
					xOffset -= gridWidth;
				}
				xOffset -= 1;

				positionRect.x += (slotSize.x + slotSpacing) / 100f * AdvGame.GetMainGameViewSize ().x * (float) xOffset;
				positionRect.y += (slotSpacing + slotSize.y) / 100f * AdvGame.GetMainGameViewSize ().y * numRows;
			}
			
			return (positionRect);
		}
		
		
		public virtual void RecalculateSize ()
		{
			Vector2 screenSize = Vector2.one;
			if (sizeType == AC_SizeType.Automatic)
			{
				AutoSize ();
			}

			if (sizeType != AC_SizeType.Absolute)
			{
				screenSize = new Vector2 (AdvGame.GetMainGameViewSize ().x / 100f, AdvGame.GetMainGameViewSize ().y / 100f);
			}

			if (orientation == ElementOrientation.Horizontal)
			{
				relativeRect.width = slotSize.x * screenSize.x * numSlots;
				relativeRect.height = slotSize.y * screenSize.y;
				if (numSlots > 1)
				{
					relativeRect.width += slotSpacing * screenSize.x * (numSlots - 1);
				}
			}
			else if (orientation == ElementOrientation.Vertical)
			{
				relativeRect.width = slotSize.x * screenSize.x;
				relativeRect.height = slotSize.y * screenSize.y * numSlots;
				if (numSlots > 1)
				{
					relativeRect.height += slotSpacing * screenSize.y * (numSlots - 1);
				}
			}
			else if (orientation == ElementOrientation.Grid)
			{
				if (numSlots < gridWidth)
				{
					relativeRect.width = (slotSize.x + slotSpacing) * screenSize.x * numSlots;
					relativeRect.height = slotSize.y * screenSize.y;
				}
				else
				{
					float numRows = Mathf.CeilToInt ((float) numSlots / gridWidth);

					relativeRect.width = slotSize.x * screenSize.x * gridWidth;
					relativeRect.height = slotSize.y * screenSize.y * numRows;

					if (numSlots > 1)
					{
						relativeRect.width += slotSpacing * screenSize.x * (gridWidth - 1);
						relativeRect.height += slotSpacing * screenSize.y * (numRows - 1);
					}
				}
			}
		}


		public int GetFontSize ()
		{
			if (sizeType == AC_SizeType.Absolute)
			{
				return (int) (fontScaleFactor * 10f);
			}

			return (int) (AdvGame.GetMainGameViewSize ().x * fontScaleFactor / 100);
		}

		
		protected void AutoSize (GUIContent content)
		{
			GUIStyle normalStyle = new GUIStyle();
			normalStyle.font = font;
			normalStyle.fontSize = GetFontSize ();
		
			Vector2 size = GetSize ();
			size = normalStyle.CalcSize (content);
			
			SetAbsoluteSize (size);
		}
		
		
		protected virtual void AutoSize ()
		{
			GUIContent content = new GUIContent (backgroundTexture);
			AutoSize (content);
		}
		
		
		public void SetPosition (Vector2 _position)
		{
			relativeRect.x = _position.x;
			relativeRect.y = _position.y;
		}


		public void SetRelativePosition (Vector2 _size)
		{
			relativeRect.x = relativePosition.x * _size.x;
			relativeRect.y = relativePosition.y * _size.y;
		}
		
		
		public void AutoSetVisibility ()
		{
			if (numSlots == 0)
			{
				isVisible = false;
			}
			else
			{
				isVisible = true;
			}
		}

	}

}