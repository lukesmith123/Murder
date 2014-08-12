/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"Button.cs"
 * 
 *	This script is a container class for interactions
 *	that are linked to Hotspots and NPCs.
 * 
 */

using UnityEngine;
using System.Collections;

namespace AC
{

	[System.Serializable]
	public class Button
	{
		
		public Interaction interaction = null;
		public bool isDisabled = false;
		public int invID = 0;
		public int iconID = -1;

		public PlayerAction playerAction = PlayerAction.DoNothing;

		public bool setProximity = false;
		public float proximity = 1f;
		public bool faceAfter = false;
		public bool isBlocking = false;
		
		public Button ()
		{ }
		
	}

}