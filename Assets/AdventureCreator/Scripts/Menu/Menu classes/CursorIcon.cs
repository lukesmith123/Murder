/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"CursorIcon.cs"
 * 
 *	This script is a data class for cursor icons.
 * 
 */

using UnityEngine;

namespace AC
{

	[System.Serializable]
	public class CursorIcon
	{

		public string label;
		public Texture2D texture;
		public int lineID = -1;
		public int id;				// Internal ID to allow order-independence
		
		
		public CursorIcon ()
		{
			texture = null;
			id = 0;
			lineID = -1;

			label = "Icon " + (id + 1).ToString ();
		}
		
		
		public CursorIcon (int[] idArray)
		{
			texture = null;
			id = 0;
			lineID = -1;
			
			// Update id based on array
			foreach (int _id in idArray)
			{
				if (id == _id)
				{
					id ++;
				}
			}
			
			label = "Icon " + (id + 1).ToString ();
		}


		public void Copy (CursorIcon _cursorIcon)
		{
			label = _cursorIcon.label;
			texture = _cursorIcon.texture;
			lineID = _cursorIcon.lineID;
			id = _cursorIcon.id;
		}

	}

	[System.Serializable]
	public class HotspotPrefix
	{

		public string label;
		public int lineID;

		public HotspotPrefix (string text)
		{
			label = text;
			lineID = -1;
		}

	}

}