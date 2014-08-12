/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"CursorManager.cs"
 * 
 *	This script handles the "Cursor" tab of the main wizard.
 *	It is used to define cursor icons and the method in which
 *	interactions are triggered by the player.
 * 
 */

using UnityEngine;
using System.IO;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AC
{

	[System.Serializable]
	public class CursorManager : ScriptableObject
	{

		public CursorDisplay cursorDisplay = CursorDisplay.Always;
		public bool allowMainCursor = false;

		public bool addHotspotPrefix = false;
		public bool allowInteractionCursor = false;
		public bool allowInteractionCursorForInventory = false;
		public bool cycleCursors = false;
		public bool leftClickExamine = false;
		
		public float normalCursorSize = 0.015f;
		public float mouseOverCursorSize = 0.015f;
		public float iconCursorSize = 0.04f;
		public float inventoryCursorSize = 0.06f;

		public Texture2D waitTexture = null;
		public Texture2D pointerTexture = null;
		public Texture2D walkTexture = null;
		public Texture2D mouseOverTexture = null;
		public InventoryHandling inventoryHandling = InventoryHandling.ChangeCursor;
		public HotspotPrefix hotspotPrefix1 = new HotspotPrefix ("Use");
		public HotspotPrefix hotspotPrefix2 = new HotspotPrefix ("on");

		public List<CursorIcon> cursorIcons = new List<CursorIcon>();
		public LookUseCursorAction lookUseCursorAction = LookUseCursorAction.DisplayBothSideBySide;
		public int lookCursor_ID = 0;
		public int lookCursor_int = 0;
		
		
		#if UNITY_EDITOR
		
		private static GUIContent
			insertContent = new GUIContent("+", "Insert variable"),
			deleteContent = new GUIContent("-", "Delete variable");

		private static GUILayoutOption
			buttonWidth = GUILayout.MaxWidth (20f),
			labelWidth = GUILayout.MaxWidth (50f);
		
		
		public void ShowGUI ()
		{
			SettingsManager settingsManager = AdvGame.GetReferences().settingsManager;
			
			EditorGUILayout.BeginVertical ("Button");
				EditorGUILayout.LabelField ("Main cursor", EditorStyles.boldLabel);
				cursorDisplay = (CursorDisplay) EditorGUILayout.EnumPopup ("Display cursor:", cursorDisplay);
				allowMainCursor = EditorGUILayout.Toggle ("Replace mouse cursor?", allowMainCursor);
				if (allowMainCursor || (settingsManager && settingsManager.inputMethod == InputMethod.KeyboardOrController))
				{
					pointerTexture = (Texture2D) EditorGUILayout.ObjectField ("Main cursor texture:", pointerTexture, typeof (Texture2D), false);
					walkTexture = (Texture2D) EditorGUILayout.ObjectField ("Walk cursor (optional):", walkTexture, typeof (Texture2D), false);
					normalCursorSize = EditorGUILayout.FloatField ("Main cursor size:", normalCursorSize);
				}
			EditorGUILayout.EndVertical ();
			
			EditorGUILayout.Space ();

			EditorGUILayout.BeginVertical ("Button");
				EditorGUILayout.LabelField ("Hotspot settings", EditorStyles.boldLabel);
				addHotspotPrefix = EditorGUILayout.Toggle ("Prefix cursor labels?", addHotspotPrefix);
				if (settingsManager && settingsManager.interactionMethod == AC_InteractionMethod.ChooseHotspotThenInteraction)
				{
					mouseOverTexture = (Texture2D) EditorGUILayout.ObjectField ("Mouseover cursor texture:", mouseOverTexture, typeof (Texture2D), false);
					mouseOverCursorSize = EditorGUILayout.FloatField ("Main cursor size:", mouseOverCursorSize);
				}
			EditorGUILayout.EndVertical ();

			EditorGUILayout.Space ();
			
			EditorGUILayout.BeginVertical ("Button");
				EditorGUILayout.LabelField ("Inventory cursor", EditorStyles.boldLabel);
				if (settingsManager && settingsManager.interactionMethod != AC_InteractionMethod.ChooseHotspotThenInteraction)
				{
					inventoryHandling = (InventoryHandling) EditorGUILayout.EnumPopup ("When inventory selected:", inventoryHandling);
					if (inventoryHandling == InventoryHandling.ChangeCursor || inventoryHandling == InventoryHandling.ChangeCursorAndHotspotLabel)
					{
						inventoryCursorSize = EditorGUILayout.FloatField ("Inventory cursor size:", inventoryCursorSize);
					}
				}
				EditorGUILayout.BeginHorizontal ();
					EditorGUILayout.LabelField ("Syntax:", GUILayout.Width (60f));
					hotspotPrefix1.label = EditorGUILayout.TextField (hotspotPrefix1.label, GUILayout.MaxWidth (80f));
					EditorGUILayout.LabelField ("(item)", GUILayout.MaxWidth (40f));

					hotspotPrefix2.label = EditorGUILayout.TextField (hotspotPrefix2.label, GUILayout.MaxWidth (80f));
					EditorGUILayout.LabelField ("(hotspot)", GUILayout.MaxWidth (55f));
				EditorGUILayout.EndHorizontal ();
			EditorGUILayout.EndVertical ();
			
			EditorGUILayout.Space ();
			
			EditorGUILayout.BeginVertical ("Button");
				EditorGUILayout.LabelField ("Interaction icons", EditorStyles.boldLabel);
				
				if (settingsManager == null || settingsManager.interactionMethod != AC_InteractionMethod.ChooseHotspotThenInteraction)
				{
					allowInteractionCursor = EditorGUILayout.BeginToggleGroup ("Change cursor when over Hotspots?", allowInteractionCursor);
						iconCursorSize = EditorGUILayout.FloatField ("Interaction icon size:", iconCursorSize);
						if (settingsManager && settingsManager.interactionMethod == AC_InteractionMethod.ChooseInteractionThenHotspot)
						{
							cycleCursors = EditorGUILayout.Toggle ("Cycle with right click?", cycleCursors);
						}
						if (settingsManager == null || settingsManager.interactionMethod == AC_InteractionMethod.ContextSensitive)
						{
							allowInteractionCursorForInventory = EditorGUILayout.Toggle ("Change for inventory too?", allowInteractionCursorForInventory);
						}
					EditorGUILayout.EndToggleGroup ();
				}
				
				IconsGUI ();
			
				EditorGUILayout.Space ();
			
				if (settingsManager == null || settingsManager.interactionMethod == AC_InteractionMethod.ContextSensitive)
				{
					LookIconGUI ();
				}
			EditorGUILayout.EndVertical ();

			EditorGUILayout.BeginVertical ("Button");
				EditorGUILayout.LabelField ("Wait cursor", EditorStyles.boldLabel);
				waitTexture = (Texture2D) EditorGUILayout.ObjectField ("Wait cursor texture:", waitTexture, typeof (Texture2D), false);
			EditorGUILayout.EndVertical ();
			
			if (GUI.changed)
			{
				EditorUtility.SetDirty (this);
			}
		}
		
		
		private void IconsGUI ()
		{
			// List icons
			foreach (CursorIcon _cursorIcon in cursorIcons)
			{
				GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(1));
				EditorGUILayout.BeginHorizontal ();
					EditorGUILayout.LabelField ("Label:", labelWidth);
					_cursorIcon.label = EditorGUILayout.TextField (_cursorIcon.label, GUILayout.Width (80));
					EditorGUILayout.LabelField ("Texture:", labelWidth);
					_cursorIcon.texture = (Texture2D) EditorGUILayout.ObjectField (_cursorIcon.texture, typeof (Texture2D), false, GUILayout.Width (70), GUILayout.Height (70));
					
					if (GUILayout.Button (insertContent, EditorStyles.miniButtonLeft, buttonWidth))
					{
						Undo.RecordObject (this, "Add icon");
						int position = cursorIcons.IndexOf (_cursorIcon) + 1;
						cursorIcons.Insert (position, new CursorIcon (GetIDArray ()));
						break;
					}
					if (GUILayout.Button (deleteContent, EditorStyles.miniButtonRight, buttonWidth))
					{
						Undo.RecordObject (this, "Delete icon: " + _cursorIcon.label);
						cursorIcons.Remove (_cursorIcon);
						break;
					}
			
				EditorGUILayout.EndHorizontal ();
			}

			if (GUILayout.Button("Create new icon"))
			{
				Undo.RecordObject (this, "Add icon");
				cursorIcons.Add (new CursorIcon (GetIDArray ()));
			}
		}
		
		
		private void LookIconGUI ()
		{
			if (cursorIcons.Count > 0)
			{
				lookCursor_int = GetIntFromID (lookCursor_ID);
				lookCursor_int = EditorGUILayout.Popup ("Examine icon:", lookCursor_int, GetLabelsArray (lookCursor_int));
				lookCursor_ID = cursorIcons[lookCursor_int].id;

				EditorGUILayout.LabelField ("When Use and Examine interactions are both available:");
				lookUseCursorAction = (LookUseCursorAction) EditorGUILayout.EnumPopup (" ", lookUseCursorAction);

				EditorGUILayout.BeginHorizontal ();
				EditorGUILayout.LabelField ("Left-click to examine when no use interaction exists?", GUILayout.Width (300f));
				leftClickExamine = EditorGUILayout.Toggle (leftClickExamine);
				EditorGUILayout.EndHorizontal ();
			}
		}
		
		#endif
		
		
		public string[] GetLabelsArray (int requestedInt)
		{
			// Create a string List of the field's names (for the PopUp box)
			List<string> iconLabels = new List<string>();
			
			foreach (CursorIcon cursorIcon in cursorIcons)
			{
				iconLabels.Add (cursorIcon.label);
			}
		
			return (iconLabels.ToArray());
		}
		
		
		public string GetLabelFromID (int _ID)
		{
			foreach (CursorIcon cursorIcon in cursorIcons)
			{
				if (cursorIcon.id == _ID)
				{
					if (Options.GetLanguage () > 0)
					{
						return (SpeechManager.GetTranslation (cursorIcon.lineID, Options.GetLanguage ()) + " ");
					}
					else
					{
						return (cursorIcon.label + " ");
					}
				}
			}
			
			return ("");
		}
		
		
		public Texture2D GetTextureFromID (int _ID)
		{
			foreach (CursorIcon cursorIcon in cursorIcons)
			{
				if (cursorIcon.id == _ID)
				{
					return (cursorIcon.texture);
				}
			}
			
			return (null);
		}
		
		
		public int GetIntFromID (int _ID)
		{
			int i = 0;
			int requestedInt = -1;
			
			foreach (CursorIcon cursorIcon in cursorIcons)
			{
				if (cursorIcon.id == _ID)
				{
					requestedInt = i;
				}
				
				i++;
			}
			
			if (requestedInt == -1)
			{
				// Wasn't found (icon was deleted?), so revert to zero
				requestedInt = 0;
			}
		
			return (requestedInt);
		}
		
		
		private int[] GetIDArray ()
		{
			// Returns a list of id's in the list
			
			List<int> idArray = new List<int>();
			
			foreach (CursorIcon cursorIcon in cursorIcons)
			{
				idArray.Add (cursorIcon.id);
			}
			
			idArray.Sort ();
			return idArray.ToArray ();
		}
		
	}

}