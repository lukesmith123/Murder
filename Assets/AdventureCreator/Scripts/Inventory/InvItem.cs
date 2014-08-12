/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"InvItem.cs"
 * 
 *	This script is a container class for individual inventory items.
 * 
 */

using UnityEngine;
using System.Collections.Generic;

namespace AC
{

	[System.Serializable]
	public class InvItem
	{

		public int count;
		public Texture2D tex;
		public Texture2D activeTex;
		public bool carryOnStart;
		public bool canCarryMultiple;
		public string label;
		public string altLabel;
		public int id;
		public int lineID = -1;
		public int useIconID = 0;
		public int binID;
		public bool isEditing = false;
		public int recipeSlot = -1;
		
		public InvActionList useActionList;
		public InvActionList lookActionList;
		public List<InvInteraction> interactions;
		public List<InvActionList> combineActionList;
		public InvActionList unhandledActionList;
		public InvActionList unhandledCombineActionList;
		public List<int> combineID;


		public InvItem (int[] idArray)
		{
			count = 0;
			tex = null;
			activeTex = null;
			id = 0;
			binID = -1;
			recipeSlot = -1;

			interactions = new List<InvInteraction>();

			combineActionList = new List<InvActionList>();
			combineID = new List<int>();

			// Update id based on array
			foreach (int _id in idArray)
			{
				if (id == _id)
					id ++;
			}

			label = "Inventory item " + (id + 1).ToString ();
			altLabel = "";
		}
		
		
		public InvItem (InvItem assetItem)
		{
			count = assetItem.count;
			tex = assetItem.tex;
			activeTex = assetItem.activeTex;
			id = assetItem.id;
			label = assetItem.label;
			altLabel = assetItem.altLabel;
			useActionList = assetItem.useActionList;
			lookActionList = assetItem.lookActionList;
			combineActionList = assetItem.combineActionList;
			unhandledActionList = assetItem.unhandledActionList;
			unhandledCombineActionList = assetItem.unhandledCombineActionList;
			combineID = assetItem.combineID;
			interactions = assetItem.interactions;
			binID = assetItem.binID;
			recipeSlot = -1;
		}


		public bool DoesHaveInventoryInteraction (InvItem invItem)
		{
			if (invItem != null)
			{
				foreach (int invID in combineID)
				{
					if (invID == invItem.id)
					{
						return true;
					}
				}
			}
			
			return false;
		}
		
	}

}