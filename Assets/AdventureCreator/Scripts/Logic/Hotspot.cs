/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"Hotspot.cs"
 * 
 *	This script handles all the possible
 *	interactions on both hotspots and NPCs.
 * 
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;


namespace AC
{

	public class Hotspot : MonoBehaviour
	{

		public bool showInEditor = true;
		
		public string hotspotName;
		public Highlight highlight;
		public bool playUseAnim;
		public Marker walkToMarker;
		public int lineID = -1;
		
		public bool provideUseInteraction;
		public Button useButton = new Button();
		
		public List<Button> useButtons = new List<Button>();
		public bool oneClick = false;
		
		public bool provideLookInteraction;
		public Button lookButton = new Button();
		
		public bool provideInvInteraction;
		public List<Button> invButtons = new List<Button>();

		private SettingsManager settingsManager;
		private CursorManager cursorManager;
		private StateHandler stateHandler;


		private void Awake ()
		{
			if (AdvGame.GetReferences () && AdvGame.GetReferences ().settingsManager && AdvGame.GetReferences ().cursorManager)
			{
				settingsManager = AdvGame.GetReferences ().settingsManager;
				cursorManager = AdvGame.GetReferences ().cursorManager;
			}
		}


		private void Start ()
		{
			if (GameObject.FindWithTag (Tags.persistentEngine) && GameObject.FindWithTag (Tags.persistentEngine).GetComponent <StateHandler>())
			{
				stateHandler = GameObject.FindWithTag (Tags.persistentEngine).GetComponent <StateHandler>();
			}
		}


		private void OnGUI ()
		{
			if (stateHandler && settingsManager && stateHandler.gameState == GameState.Normal && IsOn ())
			{
				if (settingsManager.hotspotIconDisplay != HotspotIconDisplay.Never && Camera.main.WorldToScreenPoint (transform.position).z > 0f)
				{
					float alpha = 1f;
					Texture2D icon = null;

					if (settingsManager.hotspotIcon == HotspotIcon.Texture)
					{
						icon = settingsManager.hotspotIconTexture;
					}
					else
					{
						icon = GetMainIcon ();
					}

					if (settingsManager.hotspotIconDisplay == HotspotIconDisplay.OnlyWhenHighlighting)
					{
						if (highlight)
						{
							alpha = highlight.GetHighlightAlpha ();
						}
						else
						{
							Debug.Log ("A Highlight script is expected on " + this.name);
						}
					}

					if (alpha > 0f && icon != null)
					{
						Color c = GUI.color;
						Color tempColor = c;
						c.a = alpha;
						GUI.color = c;
						GUI.DrawTexture (AdvGame.GUIBox (GetIconPosition (), settingsManager.hotspotIconSize), icon, ScaleMode.ScaleToFit, true, 0f);
						GUI.color = tempColor;
					}
				}
			}
		}

		
		private void TurnOn ()
		{
			gameObject.layer = LayerMask.NameToLayer (AdvGame.GetReferences ().settingsManager.hotspotLayer);
		}
		
		
		private void TurnOff ()
		{
			gameObject.layer = LayerMask.NameToLayer (AdvGame.GetReferences ().settingsManager.deactivatedLayer);
		}
		
		
		public bool IsOn ()
		{
			if (gameObject.layer == LayerMask.NameToLayer (AdvGame.GetReferences ().settingsManager.deactivatedLayer))
			{
				return false;
			}

			return true;
		}
		
		
		public void Select ()
		{
			if (highlight)
			{
				highlight.HighlightOn ();
			}
		}
		
		
		public void Deselect ()
		{
			if (highlight)
			{
				highlight.HighlightOff ();
			}
		}


		public bool IsSingleInteraction ()
		{
			if (oneClick && provideUseInteraction && useButtons != null && useButtons.Count == 1 && !useButtons[0].isDisabled && (invButtons == null || invButtons.Count == 0))
			{
				return true;
			}

			return false;
		}
		
		
		public void DeselectInstant ()
		{
			if (highlight)
			{
				highlight.HighlightOffInstant ();
			}
		}
		

		private void OnDrawGizmos ()
		{
			if (showInEditor)
			{
				DrawGizmos ();
			}
		}
		
		
		private void OnDrawGizmosSelected ()
		{
			DrawGizmos ();
		}
		
		
		private void DrawGizmos ()
		{
			if (this.GetComponent <AC.Char>() == null)
			{
				if (GetComponent <PolygonCollider2D>())
				{
					AdvGame.DrawPolygonCollider (transform, GetComponent <PolygonCollider2D>(), new Color (1f, 1f, 0f, 0.6f));
				}
				else
				{
					AdvGame.DrawCubeCollider (transform, new Color (1f, 1f, 0f, 0.6f));
				}
			}
		}


		private Vector2 GetIconPosition ()
		{
			if (this.collider is BoxCollider)
			{
				BoxCollider boxCollider = (BoxCollider) this.collider;
				return new Vector2 (Camera.main.WorldToScreenPoint (boxCollider.center + this.transform.position).x, Camera.main.WorldToScreenPoint (boxCollider.center + this.transform.position).y);
			}
			if (this.collider is CapsuleCollider)
			{
				CapsuleCollider capsuleCollider = (CapsuleCollider) this.collider;
				return new Vector2 (Camera.main.WorldToScreenPoint (capsuleCollider.center + this.transform.position).x, Camera.main.WorldToScreenPoint (capsuleCollider.center + this.transform.position).y);
			}
			else
			{
				return new Vector2 (Camera.main.WorldToScreenPoint (this.transform.position).x, Camera.main.WorldToScreenPoint (this.transform.position).y);
			}
		}


		private Texture2D GetMainIcon ()
		{
			if (cursorManager == null)
			{
				return null;
			}

			if (provideUseInteraction && useButton != null && !useButton.isDisabled)
			{
				return cursorManager.GetTextureFromID (useButton.iconID);
			}

			if (provideLookInteraction && lookButton != null && !lookButton.isDisabled)
			{
				return cursorManager.GetTextureFromID (lookButton.iconID);
			}

			if (provideUseInteraction && useButtons != null && useButtons.Count > 0 && !useButtons[0].isDisabled)
			{
				return cursorManager.GetTextureFromID (useButtons[0].iconID);
			}

			return null;
		}


		public bool HasContextUse ()
		{
			if (provideUseInteraction && useButton != null && !useButton.isDisabled)
			{
				return true;
			}

			if (oneClick && provideUseInteraction && useButtons != null && useButtons.Count == 1 && !useButtons[0].isDisabled)
			{
				return true;
			}

			return false;
		}


		public bool HasContextLook ()
		{
			if (provideLookInteraction && lookButton != null && !lookButton.isDisabled)
			{
				return true;
			}

			return false;
		}


		private void OnDestroy ()
		{
			settingsManager = null;
			cursorManager = null;
		}

	}

}