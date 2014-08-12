/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"Enums.cs"
 * 
 *	This script containers any enum type used by more than one script.
 * 
 */

namespace AC
{
	public enum GameState { Normal, Cutscene, DialogOptions, Paused };
	public enum ActionListType { PauseGameplay, RunInBackground };
	public enum PlayerSwitching { Allow, DoNotAllow };
	public enum ResultAction { Continue, Stop, Skip, RunCutscene }

	public enum AppearType { Manual, MouseOver, DuringConversation, OnInputKey, OnInteraction, OnHotspot, WhenSpeechPlays, DuringGameplay, OnContainer };
	public enum SpeechMenuType { All, CharactersOnly, NarrationOnly };
	public enum MenuTransition { Fade, Pan, FadeAndPan, Zoom, None };
	public enum PanDirection { Up, Down, Left, Right };
	public enum PanMovement { Linear, Smooth, Overshoot };
	public enum MenuOrientation { Horizontal, Vertical };
	public enum ElementOrientation { Horizontal, Vertical, Grid };
	public enum AC_PositionType { Centred, Aligned, Manual, FollowCursor, AppearAtCursorAndFreeze, OnHotspot, AboveSpeakingCharacter };
	public enum AC_PositionType2 { Aligned, Absolute, RelativeToMenuSize };
	public enum AC_ShiftInventory { ShiftLeft, ShiftRight };
	public enum AC_SizeType { Automatic, Manual, Absolute };
	public enum AC_InputType { AlphaNumeric, NumbericOnly };
	public enum AC_LabelType { Normal, Hotspot, DialogueLine, DialogueSpeaker, DialoguePortrait, GlobalVariable };
	public enum AC_SaveListType { Save, Load };
	public enum AC_ButtonClickType { TurnOffMenu, Crossfade, OffsetInventory, RunActionList, CustomScript, OffsetJournal, SimulateInput };
	public enum SimulateInputType { Button, Axis };
	public enum AC_SliderType { Speech, Music, SFX, CustomScript, Variable };
	public enum AC_CycleType { Language, CustomScript, Variable };
	public enum AC_ToggleType { Subtitles, CustomScript, Variable };
	public enum AC_InventoryBoxType { Default, HostpotBased, CustomScript, DisplaySelected, DisplayLastSelected, Container, PlayerToContainer };
	public enum CraftingElementType { Ingredients, Output };
	public enum ConversationDisplayType { TextOnly, IconOnly };

	public enum AC_TextType { Speech, Hotspot, DialogueOption, InventoryItem, CursorIcon, MenuElement, HotspotPrefix, JournalEntry };
	public enum CursorDisplay { Always, OnlyWhenPaused, Never };
	public enum LookUseCursorAction { DisplayBothSideBySide, DisplayUseIcon };

	public enum InteractionType { Use, Examine, Inventory };
	public enum AC_InteractionMethod { ContextSensitive, ChooseInteractionThenHotspot, ChooseHotspotThenInteraction };
	public enum HotspotDetection { MouseOver, PlayerVicinity };
	public enum HotspotsInVicinity { NearestOnly, CycleMultiple };
	public enum PlayerAction { DoNothing, TurnToFace, WalkTo, WalkToMarker };
	public enum CancelInteractions { CursorLeavesMenus, ClickOffMenu };

	public enum InventoryInteractions { Multiple, Single };
	public enum InventoryActiveEffect { None, Simple, Pulse };

	public enum AnimationEngine { Legacy, Sprites2DToolkit, SpritesUnity, Mecanim, SpritesUnityComplex };
	public enum TalkingAnimation { Standard, CustomFace };
	public enum MovementMethod { PointAndClick, Direct, FirstPerson, Drag, None };
	public enum InputMethod { MouseAndKeyboard, KeyboardOrController, TouchScreen };
	public enum DirectMovementType { RelativeToCamera, TankControls };
	public enum CameraPerspective { TwoD, TwoPointFiveD, ThreeD };
	public enum MovingTurning { WorldSpace, ScreenSpace, TopDown, Unity2D };
	public enum DragAffects { Movement, Rotation };

	public enum InteractionIcon { Use, Examine, Talk };
	public enum InventoryHandling { ChangeCursor, ChangeHotspotLabel, ChangeCursorAndHotspotLabel };

	public enum RenderLock { NoChange, Set, Release };
	public enum LockType { Enabled, Disabled, NoChange };
	public enum CharState { Idle, Custom, Move, Decelerate };
	public enum AC_2DFrameFlipping { None, LeftMirrorsRight, RightMirrorsLeft };
	public enum FadeType { fadeIn, fadeOut };
	public enum SortingMapType { SortingLayer, OrderInLayer };

	public enum CameraLocConstrainType { TargetX, TargetZ, TargetAcrossScreen, TargetIntoScreen, SideScrolling };
	public enum CameraRotConstrainType { TargetX, TargetZ, TargetAcrossScreen, TargetIntoScreen, LookAtTarget };

	public enum MoveMethod { Linear, Smooth, Curved, EaseIn, EaseOut };

	public enum AnimLayer {	Base=0, UpperBody=1, LeftArm=2, RightArm=3, Neck=4, Head=5, Face=6, Mouth=7 };
	public enum AnimStandard { Idle, Walk, Run, Talk };
	public enum AnimPlayMode { PlayOnce=0, PlayOnceAndClamp=1, Loop=2 };
	public enum AnimPlayModeBase { PlayOnceAndClamp=1, Loop=2 };
	public enum AnimMethodMecanim { ChangeParameterValue, PlayCustom, BlendShape };
	public enum AnimMethod { PlayCustom, StopCustom, BlendShape };
	public enum AnimMethodCharMecanim { ChangeParameterValue, SetStandard, PlayCustom };
	public enum MecanimCharParameter { MoveSpeedFloat, TalkBool, TurnFloat };
	public enum MecanimParameterType { Float, Int, Bool, Trigger };

	public enum PlayerMoveLock { Free=0, AlwaysWalk=1, AlwaysRun=2, NoChange=3 };
	public enum AC_OnOff { On, Off };
	public enum TransformType { Translate, Rotate, Scale };

	public enum VariableLocation { Global, Local };
	public enum VariableType { Boolean, Integer, String, Float };
	public enum BoolValue { True=1, False=0 };
	public enum SetVarMethod { SetValue, IncreaseByValue, SetAsRandom, Formula };
	public enum SetVarMethodString { EnteredHere=0, SetAsMenuInputLabel=1 };
	public enum SetVarMethodIntBool { EnteredHere=0, SetAsMecanimParameter=1 };
	public enum GetVarMethod { EnteredValue, GlobalVariable, LocalVariable };

	public enum AC_Direction { None, Up, Down, Left, Right };
	public enum CharDirection { Up, Down, Left, Right };
	public enum ArrowPromptType { KeyOnly, ClickOnly, KeyAndClick };

	public enum AC_NavigationMethod { UnityNavigation, meshCollider, PolygonCollider };
	public enum AC_PathType { Loop, PingPong, ForwardOnly, IsRandom };
	public enum PathSpeed { Walk=0, Run=1 };

	public enum SoundType { SFX, Music, Other };

	public enum NewPlayerPosition { RestorePrevious, ReplaceCurrentPlayer, ReplaceNPC, AppearAtMarker, AppearInOtherScene };
	public enum OldPlayer { RemoveFromScene, ReplaceWithNPC };

	public enum SaveTimeDisplay { DateOnly, TimeAndDate, None };
	public enum ConversationAction { ReturnToConveration, Stop, RunOtherConversation };

	public enum AutoManual { Automatic, Manual };
	public enum SceneSetting { DefaultNavMesh, DefaultPlayerStart, SortingMap, OnStartCutscene, OnLoadCutscene };
	public enum AnimatedCameraType { PlayWhenActive, SyncWithTargetMovement };
	public enum VarLink { None, PlaymakerGlobalVariable, OptionsData };

	public enum HotspotIconDisplay { Never, Always, OnlyWhenHighlighting };
	public enum HotspotIcon { Texture, UseIcon };
	public enum OnCreateRecipe { JustMoveToInventory, SelectItem, RunActionList };

	public enum SaveMethod { Binary, XML };

}