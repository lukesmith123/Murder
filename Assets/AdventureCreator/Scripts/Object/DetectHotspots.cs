/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"DetectHotspots.cs"
 * 
 *	This script is used to determine which
 *	active Hotspot is nearest the player.
 * 
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace AC
{

	public class DetectHotspots : MonoBehaviour
	{

		private Hotspot nearestHotspot;
		private int selected = 0;
		private List<Hotspot> hotspots = new List<Hotspot>();
		private StateHandler stateHandler;


		private void Start ()
		{
			stateHandler = GameObject.FindWithTag (Tags.persistentEngine).GetComponent <StateHandler>();
		}


		private void OnLevelWasLoaded ()
		{
			Start ();
			hotspots.Clear ();
			selected = 0;
		}


		public Hotspot GetSelected ()
		{
			if (hotspots.Count > 0)
			{
				if (AdvGame.GetReferences ().settingsManager.hotspotsInVicinity == HotspotsInVicinity.NearestOnly)
				{
					if (hotspots [selected].gameObject.layer == LayerMask.NameToLayer (AdvGame.GetReferences ().settingsManager.hotspotLayer))
					{
						return nearestHotspot;
					}
					else
					{
						nearestHotspot = null;
						hotspots.Remove (nearestHotspot);
					}
				}
				else if (AdvGame.GetReferences ().settingsManager.hotspotsInVicinity == HotspotsInVicinity.CycleMultiple)
				{
					if (selected >= hotspots.Count)
					{
						selected = hotspots.Count - 1;
					}
					else if (selected < 0)
					{
						selected = 0;
					}

					if (hotspots [selected].gameObject.layer == LayerMask.NameToLayer (AdvGame.GetReferences ().settingsManager.hotspotLayer))
					{
						return hotspots [selected];
					}
					else
					{
						if (nearestHotspot == hotspots [selected])
						{
							nearestHotspot = null;
						}

						hotspots.RemoveAt (selected);
					}
				}
			}

			return null;
		}


		private void OnTriggerStay (Collider other)
		{
			if (other.GetComponent <Hotspot>() && other.gameObject.layer == LayerMask.NameToLayer (AdvGame.GetReferences ().settingsManager.hotspotLayer))
			{
				if (nearestHotspot == null || (Vector3.Distance (transform.position, other.transform.position) <= Vector3.Distance (transform.position, nearestHotspot.transform.position)))
				{
					nearestHotspot = other.GetComponent <Hotspot>();
				}

				foreach (Hotspot hotspot in hotspots)
				{
					if (hotspot == other.GetComponent <Hotspot>())
					{
						return;
					}
				}

				hotspots.Add (other.GetComponent <Hotspot>());
			}
         }


		private void OnTriggerExit (Collider other)
		{
			if (other.GetComponent <Hotspot>())
			{
				if (nearestHotspot == other.GetComponent <Hotspot>())
				{
					nearestHotspot = null;
				}

				if (IsHotspotInTrigger (other.GetComponent <Hotspot>()))
				{
					hotspots.Remove (other.GetComponent <Hotspot>());
				}
			}
		}


		private void Update ()
		{
			if (nearestHotspot && nearestHotspot.gameObject.layer == LayerMask.NameToLayer (AdvGame.GetReferences ().settingsManager.deactivatedLayer))
			{
				nearestHotspot = null;
			}

			if (stateHandler == null)
			{
				Start ();
			}
			if (stateHandler != null && stateHandler.gameState == GameState.Normal)
			{
				try
				{
					if (Input.GetButtonDown ("CycleHotspotsLeft"))
					{
						CycleHotspots (false);
					}
					else if (Input.GetButtonDown ("CycleHotspotsRight"))
					{
						CycleHotspots (true);
					}
				}
				catch {}
			}
		}


		private void CycleHotspots (bool goRight)
		{
			if (goRight)
			{
				selected ++;
			}
			else
			{
				selected --;
			}

			if (selected >= hotspots.Count)
			{
				selected = 0;
			}
			else if (selected < 0)
			{
				selected = hotspots.Count - 1;
			}
		}


		public bool IsHotspotInTrigger (Hotspot hotspot)
		{
			if (hotspots.Contains (hotspot))
			{
				return true;
			}

			return false;
		}

	}

}