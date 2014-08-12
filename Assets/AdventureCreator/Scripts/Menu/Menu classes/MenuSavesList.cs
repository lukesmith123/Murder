/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"MenuSavesList.cs"
 * 
 *	This MenuElement handles the display of any saved games recorded.
 * 
 */

using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;	
#endif

namespace AC
{

	public class MenuSavesList : MenuElement
	{
		
		public bool doOutline = false;
		public TextAnchor anchor;
		public AC_SaveListType saveListType;
		public int maxSaves = 5;

		private bool newSaveSlot = false;

		
		public override void Declare ()
		{
			isVisible = true;
			isClickable = true;
			numSlots = 1;
			maxSaves = 5;

			SetSize (new Vector2 (20f, 5f));
			anchor = TextAnchor.MiddleCenter;
			saveListType = AC_SaveListType.Save;

			newSaveSlot = false;

			base.Declare ();
		}
		
		
		public void CopySavesList (MenuSavesList _element)
		{
			doOutline = _element.doOutline;
			anchor = _element.anchor;
			saveListType = _element.saveListType;
			maxSaves = _element.maxSaves;
			
			base.Copy (_element);
		}
		
		
		#if UNITY_EDITOR
		
		public override void ShowGUI ()
		{
			EditorGUILayout.BeginVertical ("Button");
				numSlots = EditorGUILayout.IntSlider ("Test slots:", numSlots, 1, 10);
				slotSpacing = EditorGUILayout.Slider ("Slot spacing:", slotSpacing, 0f, 20f);
				maxSaves = EditorGUILayout.IntField ("Max saves:", maxSaves);
				anchor = (TextAnchor) EditorGUILayout.EnumPopup ("Text alignment:", anchor);
				doOutline = EditorGUILayout.Toggle ("Outline text?", doOutline);
				saveListType = (AC_SaveListType) EditorGUILayout.EnumPopup ("Click action:", saveListType);
				orientation = (ElementOrientation) EditorGUILayout.EnumPopup ("Slot orientation:", orientation);
				if (orientation == ElementOrientation.Grid)
				{
					gridWidth = EditorGUILayout.IntSlider ("Grid size:", gridWidth, 1, 10);
				}
			EditorGUILayout.EndVertical ();
			
			base.ShowGUI ();
		}
		
		#endif


		public override string GetLabel (int slot)
		{
			return SaveSystem.GetSaveSlotName (slot);
		}
		

		public override void Display (GUIStyle _style, int _slot, float zoom, bool isActive)
		{
			base.Display (_style, _slot, zoom, isActive);
			
			_style.alignment = anchor;
			if (zoom < 1f)
			{
				_style.fontSize = (int) ((float) _style.fontSize * zoom);
			}
			
			string slotLabel = SaveSystem.GetSaveSlotName (_slot);
			if (newSaveSlot && _slot == (numSlots - 1))
			{
				slotLabel = "New save";
			}
			
			if (doOutline)
			{
				AdvGame.DrawTextOutline (ZoomRect (GetSlotRectRelative (_slot), zoom), slotLabel, _style, Color.black, _style.normal.textColor, 2);
			}
			else
			{
				GUI.Label (ZoomRect (GetSlotRectRelative (_slot), zoom), slotLabel, _style);
			}
		}
		
		
		public override void RecalculateSize ()
		{
			newSaveSlot = false;

			if (Application.isPlaying)
			{
				numSlots = SaveSystem.GetNumSlots ();
				
				if (saveListType == AC_SaveListType.Save && numSlots < maxSaves)
				{
					newSaveSlot = true;
					numSlots ++;
				}
			}

			base.RecalculateSize ();
		}
		
		
		protected override void AutoSize ()
		{
			AutoSize (new GUIContent (SaveSystem.GetSaveSlotName (0)));
		}
		
	}

}