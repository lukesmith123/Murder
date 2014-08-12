/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"SettingsManager.cs"
 * 
 *	This script handles the "Settings" tab of the main wizard.
 *	It is used to define the player, and control methods of the game.
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
	public class SettingsManager : ScriptableObject
	{
		
		#if UNITY_EDITOR
		private static GUIContent
			deleteContent = new GUIContent("-", "Delete item");
		
		private static GUILayoutOption
			buttonWidth = GUILayout.MaxWidth (20f);
		#endif
		
		// Save settings
		public string saveFileName = "";
		public SaveTimeDisplay saveTimeDisplay = SaveTimeDisplay.DateOnly;
		
		// Character settings
		public PlayerSwitching playerSwitching = PlayerSwitching.DoNotAllow;
		public Player player;
		public List<PlayerPrefab> players = new List<PlayerPrefab>();
		public bool shareInventory = false;
		
		// Interface settings
		public MovementMethod movementMethod = MovementMethod.PointAndClick;
		public InputMethod inputMethod = InputMethod.MouseAndKeyboard;
		public AC_InteractionMethod interactionMethod = AC_InteractionMethod.ContextSensitive;
		public HotspotDetection hotspotDetection = HotspotDetection.MouseOver;
		public CancelInteractions cancelInteractions = CancelInteractions.CursorLeavesMenus;
		public HotspotsInVicinity hotspotsInVicinity = HotspotsInVicinity.NearestOnly;
		public bool hideLockedCursor = false;
		
		// Inventory settings
		public bool inventoryDragDrop = false;
		public bool inventoryDropLook = false;
		public InventoryInteractions inventoryInteractions = InventoryInteractions.Single;
		public InventoryActiveEffect inventoryActiveEffect = InventoryActiveEffect.Simple;
		public bool inventoryDisableLeft = true;
		public float inventoryPulseSpeed = 1f;
		public bool activeWhenUnhandled = true;
		
		// Movement settings
		public Transform clickPrefab;
		public DirectMovementType directMovementType = DirectMovementType.RelativeToCamera;
		public float destinationAccuracy = 0.9f;
		public float walkableClickRange = 0.5f;
		public DragAffects dragAffects = DragAffects.Movement;
		public float verticalReductionFactor = 0.7f;
		public bool doubleClickMovement = false;
		
		// Drag settings
		public float freeAimTouchSpeed = 0.1f;
		public float dragWalkThreshold = 5f;
		public float dragRunThreshold = 20f;
		public bool drawDragLine = false;
		public float dragLineWidth = 3f;
		public Color dragLineColor = Color.white;
		
		// Touch Screen settings
		public bool doubleTapHotspots = true;
		
		// Camera settings
		public bool forceAspectRatio = false;
		public float wantedAspectRatio = 1.5f;
		public bool landscapeModeOnly = true;
		public CameraPerspective cameraPerspective = CameraPerspective.ThreeD;
		private int cameraPerspective_int;
		#if UNITY_EDITOR
		private string[] cameraPerspective_list = { "2D", "2.5D", "3D" };
		#endif
		public MovingTurning movingTurning = MovingTurning.Unity2D;
		
		// Hotspot settings
		public HotspotIconDisplay hotspotIconDisplay = HotspotIconDisplay.Never;
		public HotspotIcon hotspotIcon;
		public Texture2D hotspotIconTexture = null;
		public float hotspotIconSize = 0.04f;
		
		// Raycast settings
		public float navMeshRaycastLength = 100f;
		public float hotspotRaycastLength = 100f;
		
		// Layer names
		public string hotspotLayer = "Default";
		public string navMeshLayer = "NavMesh";
		public string backgroundImageLayer = "BackgroundImage";
		public string deactivatedLayer = "Ignore Raycast";
		
		// Options data
		#if UNITY_EDITOR
		private OptionsData optionsData = new OptionsData ();
		private string ppKey = "Options";
		private string optionsBinary = "";
		
		
		public void ShowGUI ()
		{
			EditorGUILayout.LabelField ("Save game settings", EditorStyles.boldLabel);
			
			if (saveFileName == "")
			{
				saveFileName = SaveSystem.SetProjectName ();
			}
			saveFileName = EditorGUILayout.TextField ("Save filename:", saveFileName);
			#if !UNITY_WEBPLAYER
			saveTimeDisplay = (SaveTimeDisplay) EditorGUILayout.EnumPopup ("Time display:", saveTimeDisplay);
			#endif
			
			EditorGUILayout.Space ();
			EditorGUILayout.LabelField ("Character settings:", EditorStyles.boldLabel);
			
			CreatePlayersGUI ();
			
			EditorGUILayout.Space ();
			EditorGUILayout.LabelField ("Interface settings", EditorStyles.boldLabel);
			
			movementMethod = (MovementMethod) EditorGUILayout.EnumPopup ("Movement method:", movementMethod);
			inputMethod = (InputMethod) EditorGUILayout.EnumPopup ("Input method:", inputMethod);
			interactionMethod = (AC_InteractionMethod) EditorGUILayout.EnumPopup ("Interaction method:", interactionMethod);
			hotspotDetection = (HotspotDetection) EditorGUILayout.EnumPopup ("Hotspot detection method:", hotspotDetection);
			
			if (hotspotDetection == HotspotDetection.PlayerVicinity && (movementMethod == MovementMethod.Direct || movementMethod == MovementMethod.FirstPerson))
			{
				hotspotsInVicinity = (HotspotsInVicinity) EditorGUILayout.EnumPopup ("Hotspots in vicinity:", hotspotsInVicinity);
			}
			
			cancelInteractions = (CancelInteractions) EditorGUILayout.EnumPopup ("Cancel interactions with:", cancelInteractions);
			
			if (movementMethod != MovementMethod.PointAndClick)
			{
				hideLockedCursor = EditorGUILayout.Toggle ("Hide cursor when locked?", hideLockedCursor);
			}
			
			EditorGUILayout.Space ();
			EditorGUILayout.LabelField ("Inventory settings", EditorStyles.boldLabel);
			
			if (interactionMethod != AC_InteractionMethod.ContextSensitive)
			{
				inventoryInteractions = (InventoryInteractions) EditorGUILayout.EnumPopup ("Inventory interactions:", inventoryInteractions);
			}
			if (interactionMethod != AC_InteractionMethod.ChooseHotspotThenInteraction || inventoryInteractions == InventoryInteractions.Single)
			{
				inventoryDragDrop = EditorGUILayout.Toggle ("Drag and drop items?", inventoryDragDrop);
				if (!inventoryDragDrop)
				{
					inventoryDisableLeft = EditorGUILayout.Toggle ("Disable with left-click?", inventoryDisableLeft);
				}
				else
				{
					inventoryDropLook = EditorGUILayout.Toggle ("Drop on self to Examine?", inventoryDropLook);
				}
				inventoryActiveEffect = (InventoryActiveEffect) EditorGUILayout.EnumPopup ("Active cursor FX:", inventoryActiveEffect);
				if (inventoryActiveEffect == InventoryActiveEffect.Pulse)
				{
					inventoryPulseSpeed = EditorGUILayout.Slider ("Active FX pulse speed:", inventoryPulseSpeed, 0.5f, 2f);
				}
				activeWhenUnhandled = EditorGUILayout.Toggle ("Active FX when unhandled?", activeWhenUnhandled);
			}
			
			EditorGUILayout.Space ();
			EditorGUILayout.LabelField ("Required inputs:", EditorStyles.boldLabel);
			EditorGUILayout.HelpBox ("The following input axes are available for the chosen interface settings:" + GetInputList (), MessageType.Info);
			
			EditorGUILayout.Space ();
			EditorGUILayout.LabelField ("Movement settings", EditorStyles.boldLabel);
			
			if ((inputMethod == InputMethod.TouchScreen && movementMethod != MovementMethod.PointAndClick) || movementMethod == MovementMethod.Drag)
			{
				dragWalkThreshold = EditorGUILayout.FloatField ("Walk threshold:", dragWalkThreshold);
				dragRunThreshold = EditorGUILayout.FloatField ("Run threshold:", dragRunThreshold);
				
				if (inputMethod == InputMethod.TouchScreen && movementMethod == MovementMethod.FirstPerson)
				{
					freeAimTouchSpeed = EditorGUILayout.FloatField ("Freelook speed:", freeAimTouchSpeed);
				}
				
				drawDragLine = EditorGUILayout.Toggle ("Draw drag line?", drawDragLine);
				if (drawDragLine)
				{
					dragLineWidth = EditorGUILayout.FloatField ("Drag line width:", dragLineWidth);
					dragLineColor = EditorGUILayout.ColorField ("Drag line colour:", dragLineColor);
				}
			}
			else if (movementMethod == MovementMethod.Direct)
			{
				directMovementType = (DirectMovementType) EditorGUILayout.EnumPopup ("Direct-movement type:", directMovementType);
			}
			else if (movementMethod == MovementMethod.PointAndClick)
			{
				clickPrefab = (Transform) EditorGUILayout.ObjectField ("Click marker:", clickPrefab, typeof (Transform), false);
				walkableClickRange = EditorGUILayout.Slider ("NavMesh search %:", walkableClickRange, 0f, 1f);
				doubleClickMovement = EditorGUILayout.Toggle ("Double-click to move?", doubleClickMovement);
			}
			if (movementMethod == MovementMethod.FirstPerson && inputMethod == InputMethod.TouchScreen)
			{
				dragAffects = (DragAffects) EditorGUILayout.EnumPopup ("Touch-drag affects:", dragAffects);
			}
			
			destinationAccuracy = EditorGUILayout.Slider ("Destination accuracy:", destinationAccuracy, 0f, 1f);
			
			if (inputMethod == InputMethod.TouchScreen)
			{
				EditorGUILayout.Space ();
				EditorGUILayout.LabelField ("Touch Screen settings", EditorStyles.boldLabel);
				
				doubleTapHotspots = EditorGUILayout.Toggle ("Double-tap Hotspots?", doubleTapHotspots);
			}
			
			EditorGUILayout.Space ();
			EditorGUILayout.LabelField ("Camera settings", EditorStyles.boldLabel);
			
			cameraPerspective_int = (int) cameraPerspective;
			cameraPerspective_int = EditorGUILayout.Popup ("Camera perspective:", cameraPerspective_int, cameraPerspective_list);
			cameraPerspective = (CameraPerspective) cameraPerspective_int;
			if (movementMethod == MovementMethod.FirstPerson)
			{
				cameraPerspective = CameraPerspective.ThreeD;
			}
			if (cameraPerspective == CameraPerspective.TwoD)
			{
				movingTurning = (MovingTurning) EditorGUILayout.EnumPopup ("Moving and turning:", movingTurning);
				if (movingTurning == MovingTurning.TopDown || movingTurning == MovingTurning.Unity2D)
				{
					verticalReductionFactor = EditorGUILayout.Slider ("Vertical movement factor:", verticalReductionFactor, 0.1f, 1f);
				}
			}
			
			forceAspectRatio = EditorGUILayout.Toggle ("Force aspect ratio?", forceAspectRatio);
			if (forceAspectRatio)
			{
				wantedAspectRatio = EditorGUILayout.FloatField ("Aspect ratio:", wantedAspectRatio);
				#if UNITY_IPHONE
				landscapeModeOnly = EditorGUILayout.Toggle ("Landscape-mode only?", landscapeModeOnly);
				#endif
			}
			
			EditorGUILayout.Space ();
			EditorGUILayout.LabelField ("Hotpot settings", EditorStyles.boldLabel);
			
			hotspotIconDisplay = (HotspotIconDisplay) EditorGUILayout.EnumPopup ("Display Hotspot icon:", hotspotIconDisplay);
			if (hotspotIconDisplay != HotspotIconDisplay.Never)
			{
				hotspotIcon = (HotspotIcon) EditorGUILayout.EnumPopup ("Hotspot icon type:", hotspotIcon);
				if (hotspotIcon == HotspotIcon.Texture)
				{
					hotspotIconTexture = (Texture2D) EditorGUILayout.ObjectField ("Hotspot icon texture:", hotspotIconTexture, typeof (Texture2D), false);
				}
				hotspotIconSize = EditorGUILayout.FloatField ("Hotspot icon size:", hotspotIconSize);
			}
			
			EditorGUILayout.Space ();
			EditorGUILayout.LabelField ("Raycast settings", EditorStyles.boldLabel);
			
			navMeshRaycastLength = EditorGUILayout.FloatField ("NavMesh ray length:", navMeshRaycastLength);
			hotspotRaycastLength = EditorGUILayout.FloatField ("Hotspot ray length:", hotspotRaycastLength);
			
			EditorGUILayout.Space ();
			EditorGUILayout.LabelField ("Layer names", EditorStyles.boldLabel);
			
			hotspotLayer = EditorGUILayout.TextField ("Hotspot:", hotspotLayer);
			navMeshLayer = EditorGUILayout.TextField ("Nav mesh:", navMeshLayer);
			if (cameraPerspective == CameraPerspective.TwoPointFiveD)
			{
				backgroundImageLayer = EditorGUILayout.TextField ("Background image:", backgroundImageLayer);
			}
			deactivatedLayer = EditorGUILayout.TextField ("Deactivated:", deactivatedLayer);
			
			EditorGUILayout.Space ();
			EditorGUILayout.LabelField ("Options data", EditorStyles.boldLabel);
			
			if (!PlayerPrefs.HasKey (ppKey))
			{
				optionsData = new OptionsData ();
				optionsBinary = Serializer.SerializeObjectBinary (optionsData);
				PlayerPrefs.SetString (ppKey, optionsBinary);
			}
			
			optionsBinary = PlayerPrefs.GetString (ppKey);
			optionsData = Serializer.DeserializeObjectBinary <OptionsData> (optionsBinary);
			
			optionsData.speechVolume = EditorGUILayout.IntSlider ("Speech volume:", optionsData.speechVolume, 0, 10);
			optionsData.musicVolume = EditorGUILayout.IntSlider ("Music volume:", optionsData.musicVolume, 0, 10);
			optionsData.sfxVolume = EditorGUILayout.IntSlider ("SFX volume:", optionsData.sfxVolume, 0, 10);
			optionsData.showSubtitles = EditorGUILayout.Toggle ("Show subtitles?", optionsData.showSubtitles);
			optionsData.language = EditorGUILayout.IntField ("Language:", optionsData.language);
			
			optionsBinary = Serializer.SerializeObjectBinary (optionsData);
			PlayerPrefs.SetString (ppKey, optionsBinary);
			
			if (GUILayout.Button ("Reset options data"))
			{
				PlayerPrefs.DeleteKey ("Options");
				optionsData = new OptionsData ();
				Debug.Log ("PlayerPrefs cleared");
			}
			
			if (GUI.changed)
			{
				EditorUtility.SetDirty (this);
			}
		}
		
		#endif
		
		
		private string GetInputList ()
		{
			string result = "";
			
			if (inputMethod == InputMethod.KeyboardOrController)
			{
				result += "\n";
				result += "- InteractionA";
				result += "\n";
				result += "- InteractionB";
				result += "\n";
				result += "- CursorHorizontal";
				result += "\n";
				result += "- CursorVertical";
			}
			
			if (movementMethod != MovementMethod.PointAndClick)
			{
				result += "\n";
				result += "- ToggleCursor";
			}
			
			if (movementMethod == MovementMethod.Direct || movementMethod == MovementMethod.FirstPerson)
			{
				if (inputMethod != InputMethod.TouchScreen)
				{
					result += "\n";
					result += "- Horizontal";
					result += "\n";
					result += "- Vertical";
					result += "\n";
					result += "- Run";
				}
				
				if (movementMethod == MovementMethod.FirstPerson)
				{
					if (inputMethod == InputMethod.MouseAndKeyboard)
					{
						result += "\n";
						result += "- MouseScrollWheel";
						result += "\n";
						result += "- CursorHorizontal";
						result += "\n";
						result += "- CursorVertical";
					}
				}
				
				if (hotspotDetection == HotspotDetection.PlayerVicinity && hotspotsInVicinity == HotspotsInVicinity.CycleMultiple)
				{
					result += "\n";
					result += "- CycleHotspotsLeft";
					result += "\n";
					result += "- CycleHotspotsRight";
				}
			}
			
			result += "\n";
			result += "- FlashHotspots";
			result += "\n";
			result += "- Menu";
			result += "\n";
			result += "- EndCutscene";
			
			return result;
		}
		
		
		public bool ActInScreenSpace ()
		{
			if ((movingTurning == MovingTurning.ScreenSpace || movingTurning == MovingTurning.Unity2D) && cameraPerspective == CameraPerspective.TwoD)
			{
				return true;
			}
			return false;
		}
		
		
		public bool IsUnity2D ()
		{
			if (movingTurning == MovingTurning.Unity2D && cameraPerspective == CameraPerspective.TwoD)
			{
				return true;
			}
			return false;
		}
		
		
		public bool IsTopDown ()
		{
			if (movingTurning == MovingTurning.TopDown && cameraPerspective == CameraPerspective.TwoD)
			{
				return true;
			}
			return false;
		}
		
		
		public bool IsFirstPersonDragRotation ()
		{
			if (movementMethod == MovementMethod.FirstPerson && inputMethod == InputMethod.TouchScreen && dragAffects == DragAffects.Rotation)
			{
				return true;
			}
			return false;
		}
		
		
		#if UNITY_EDITOR
		
		private void CreatePlayersGUI ()
		{
			playerSwitching = (PlayerSwitching) EditorGUILayout.EnumPopup ("Player switching:", playerSwitching);
			if (playerSwitching == PlayerSwitching.DoNotAllow)
			{
				player = (Player) EditorGUILayout.ObjectField ("Player:", player, typeof (Player), false);
			}
			else
			{
				shareInventory = EditorGUILayout.Toggle ("Share same Inventory?", shareInventory);
				
				foreach (PlayerPrefab _player in players)
				{
					EditorGUILayout.BeginHorizontal ();
					
					_player.playerOb = (Player) EditorGUILayout.ObjectField ("Player " + _player.ID + ":", _player.playerOb, typeof (Player), false);
					
					if (_player.isDefault)
					{
						GUILayout.Label ("DEFAULT", EditorStyles.boldLabel, GUILayout.Width (80f));
					}
					else
					{
						if (GUILayout.Button ("Make default", GUILayout.Width (80f)))
						{
							SetDefaultPlayer (_player);
						}
					}
					
					if (GUILayout.Button (deleteContent, EditorStyles.miniButtonRight, buttonWidth))
					{
						Undo.RecordObject (this, "Delete player reference");
						players.Remove (_player);
						break;
					}
					
					EditorGUILayout.EndHorizontal ();
				}
				
				if (GUILayout.Button("Add new player"))
				{
					Undo.RecordObject (this, "Add player");
					
					PlayerPrefab newPlayer = new PlayerPrefab (GetPlayerIDArray ());
					players.Add (newPlayer);
				}
			}
		}
		
		#endif
		
		
		private int[] GetPlayerIDArray ()
		{
			// Returns a list of id's in the list
			
			List<int> idArray = new List<int>();
			
			foreach (PlayerPrefab player in players)
			{
				idArray.Add (player.ID);
			}
			
			idArray.Sort ();
			return idArray.ToArray ();
		}
		
		
		public int GetDefaultPlayerID ()
		{
			if (playerSwitching == PlayerSwitching.DoNotAllow)
			{
				return 0;
			}
			
			foreach (PlayerPrefab _player in players)
			{
				if (_player.isDefault)
				{
					return _player.ID;
				}
			}
			
			return 0;
		}
		
		
		public Player GetDefaultPlayer ()
		{
			if (playerSwitching == PlayerSwitching.DoNotAllow)
			{
				return player;
			}
			
			foreach (PlayerPrefab _player in players)
			{
				if (_player.isDefault)
				{
					if (_player.playerOb != null)
					{
						return _player.playerOb;
					}
					
					Debug.LogWarning ("Default Player has no prefab!");
					return null;
				}
			}
			
			Debug.LogWarning ("Cannot find default player!");
			return null;
		}
		
		
		private void SetDefaultPlayer (PlayerPrefab defaultPlayer)
		{
			foreach (PlayerPrefab _player in players)
			{
				if (_player == defaultPlayer)
				{
					_player.isDefault = true;
				}
				else
				{
					_player.isDefault = false;
				}
			}
		}
		
		
		private bool DoPlayerAnimEnginesMatch ()
		{
			AnimationEngine animationEngine = AnimationEngine.Legacy;
			bool foundFirst = false;
			
			foreach (PlayerPrefab _player in players)
			{
				if (_player.playerOb != null)
				{
					if (!foundFirst)
					{
						foundFirst = true;
						animationEngine = _player.playerOb.animationEngine;
					}
					else
					{
						if (_player.playerOb.animationEngine != animationEngine)
						{
							return false;
						}
					}
				}
			}
			
			return true;
		}
		
	}
	
}