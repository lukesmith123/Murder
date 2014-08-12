/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"MenuInventoryBox.cs"
 * 
 *	This MenuElement lists all inventory items held by the player.
 * 
 */

using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AC
{

	public class MenuInventoryBox : MenuElement
	{
		
		public bool doOutline;
		public AC_InventoryBoxType inventoryBoxType;
		public bool limitSlots;
		public int maxSlots;
		public bool limitToCategory;
		public int categoryID;
		public List<InvItem> items = null;

		private int offset = 0;
		
		private RuntimeActionList runtimeActionList;
		
		
		public override void Declare ()
		{
			isVisible = true;
			isClickable = true;
			inventoryBoxType = AC_InventoryBoxType.Default;
			numSlots = 0;
			SetSize (new Vector2 (6f, 10f));
			doOutline = false;
			limitSlots = false;
			maxSlots = 10;
			limitToCategory = false;
			categoryID = -1;
			items = new List<InvItem>();
		}
		
		
		public void CopyInventoryBox (MenuInventoryBox _element)
		{
			isClickable = _element.isClickable;
			doOutline = _element.doOutline;
			inventoryBoxType = _element.inventoryBoxType;
			numSlots = _element.numSlots;
			limitSlots = _element.limitSlots;
			maxSlots = _element.maxSlots;
			limitToCategory = _element.limitToCategory;
			categoryID = _element.categoryID;
			PopulateList ();

			base.Copy (_element);
		}
		
		
		#if UNITY_EDITOR
		
		public override void ShowGUI ()
		{
			EditorGUILayout.BeginVertical ("Button");
				doOutline = EditorGUILayout.Toggle ("Outline text?", doOutline);
				inventoryBoxType = (AC_InventoryBoxType) EditorGUILayout.EnumPopup ("Inventory box type:", inventoryBoxType);
				if (inventoryBoxType == AC_InventoryBoxType.Default || inventoryBoxType == AC_InventoryBoxType.CustomScript || inventoryBoxType == AC_InventoryBoxType.PlayerToContainer)
				{
					limitToCategory = EditorGUILayout.Toggle ("Limit to category?", limitToCategory);
					if (limitToCategory)
					{
						if (AdvGame.GetReferences ().inventoryManager)
						{
							List<string> binList = new List<string>();
							List<InvBin> bins = AdvGame.GetReferences ().inventoryManager.bins;
							foreach (InvBin bin in bins)
							{
								binList.Add (bin.label);
							}

							EditorGUILayout.BeginHorizontal ();
								EditorGUILayout.LabelField ("Category:", GUILayout.Width (146f));
								if (binList.Count > 0)
								{
									int binNumber = GetBinSlot (categoryID, bins);
									binNumber = EditorGUILayout.Popup (binNumber, binList.ToArray());
									categoryID = bins[binNumber].id;
								}
								else
								{
									categoryID = -1;
									EditorGUILayout.LabelField ("No categories defined!", EditorStyles.miniLabel, GUILayout.Width (146f));
								}
							EditorGUILayout.EndHorizontal ();
						}
						else
						{
							EditorGUILayout.HelpBox ("No Inventory Manager defined!", MessageType.Warning);
							categoryID = -1;
						}
					}
					else
					{
						categoryID = -1;
					}

					limitSlots = EditorGUILayout.Toggle ("Limit number of slots?", limitSlots);
					if (limitSlots)
					{
						maxSlots = EditorGUILayout.IntSlider ("Maximum number slots:", maxSlots, 1, 30);
					}

					isClickable = true;
				}
				else if (inventoryBoxType == AC_InventoryBoxType.DisplaySelected)
				{
					isClickable = false;
					limitSlots = true;
					maxSlots = 1;
				}
				else if (inventoryBoxType == AC_InventoryBoxType.DisplayLastSelected)
				{
					isClickable = true;
					limitSlots = true;
					maxSlots = 1;
				}
				else if (inventoryBoxType == AC_InventoryBoxType.Container)
				{
					isClickable = true;
					limitSlots = EditorGUILayout.Toggle ("Limit number of slots?", limitSlots);
					if (limitSlots)
					{
						maxSlots = EditorGUILayout.IntSlider ("Maximum number slots:", maxSlots, 1, 30);
					}
				}
				else
				{
					isClickable = true;
					numSlots = EditorGUILayout.IntField ("Test slots:", numSlots);
					limitSlots = false;
					maxSlots = 10;
				}

				if (inventoryBoxType != AC_InventoryBoxType.DisplaySelected && inventoryBoxType != AC_InventoryBoxType.DisplayLastSelected)
				{
					slotSpacing = EditorGUILayout.Slider ("Slot spacing:", slotSpacing, 0f, 20f);
					orientation = (ElementOrientation) EditorGUILayout.EnumPopup ("Slot orientation:", orientation);
					if (orientation == ElementOrientation.Grid)
					{
						gridWidth = EditorGUILayout.IntSlider ("Grid size:", gridWidth, 1, 10);
					}
				}
				
				if (inventoryBoxType == AC_InventoryBoxType.CustomScript)
				{
					ShowClipHelp ();
				}
			EditorGUILayout.EndVertical ();

			PopulateList ();
			base.ShowGUI ();
		}


		private int GetBinSlot (int _id, List<InvBin> bins)
		{
			int i = 0;
			foreach (InvBin bin in bins)
			{
				if (bin.id == _id)
				{
					return i;
				}
				i++;
			}
			
			return 0;
		}
		
		#endif
		
		
		public override void Display (GUIStyle _style, int _slot, float zoom, bool isActive)
		{
			base.Display (_style, _slot, zoom, isActive);

			if (items.Count > 0 && items.Count > (_slot + offset))
			{
				GUI.Label (GetSlotRectRelative (_slot), "", _style);
				DrawTexture (ZoomRect (GetSlotRectRelative (_slot), zoom), _slot);
				_style.normal.background = null;

				if (doOutline)
				{
					AdvGame.DrawTextOutline (ZoomRect (GetSlotRectRelative (_slot), zoom), GetCount (_slot), _style, Color.black, _style.normal.textColor, 2);
				}
				else
				{
					GUI.Label (ZoomRect (GetSlotRectRelative (_slot), zoom), GetCount (_slot), _style);
				}
			}
		}
		
		
		public void HandleDefaultClick (int _buttonPressed, int _slot, AC_InteractionMethod interactionMethod)
		{
			if (GameObject.FindWithTag (Tags.persistentEngine) && GameObject.FindWithTag (Tags.persistentEngine).GetComponent <RuntimeInventory>())
			{
				RuntimeInventory runtimeInventory = GameObject.FindWithTag (Tags.persistentEngine).GetComponent <RuntimeInventory>();

				if (interactionMethod == AC_InteractionMethod.ChooseHotspotThenInteraction)
				{
					runtimeInventory.ShowInteractions (items [_slot + offset]);
				}
				else if (interactionMethod == AC_InteractionMethod.ChooseInteractionThenHotspot)
				{
					if (_buttonPressed == 1)
					{
						int cursorID = GameObject.FindWithTag (Tags.gameEngine).GetComponent <PlayerCursor>().GetSelectedCursorID ();
						int cursor = GameObject.FindWithTag (Tags.gameEngine).GetComponent <PlayerCursor>().GetSelectedCursor ();

						if (cursor == -2)
						{
							if (runtimeInventory.selectedItem != null && items [_slot + offset] == runtimeInventory.selectedItem)
							{
								runtimeInventory.SelectItem (items [_slot + offset]);
							}
							else
							{
								runtimeInventory.Combine (items [_slot + offset]);
							}
						}
						else if (cursor == -1)
						{
							runtimeInventory.SelectItem (items [_slot + offset]);
						}
						else if (cursorID > -1)
						{
							runtimeInventory.RunInteraction (items [_slot + offset], cursorID);
						}
					}
				}
				else
				{
					if (_buttonPressed == 1)
					{
						if (runtimeInventory.selectedItem == null)
						{
							runtimeInventory.Use (items [_slot + offset]);
						}
						else
						{
							runtimeInventory.Combine (items [_slot + offset]);
						}
					}
					else if (_buttonPressed == 2)
					{
						if (runtimeInventory.selectedItem == null)
						{
							runtimeInventory.Look (items [_slot + offset]);
						}
						else
						{
							runtimeInventory.SetNull ();
						}
					}
				}
			}
		}
		
		
		public override void RecalculateSize ()
		{
			PopulateList ();

			if (inventoryBoxType == AC_InventoryBoxType.HostpotBased)
			{
				if (!Application.isPlaying)
				{
					if (numSlots < 0)
					{
						numSlots = 0;
					}
					if (numSlots > items.Count)
					{
						numSlots = items.Count;
					}
				}
				else
				{
					numSlots = items.Count;
				}
			}
			else
			{
				numSlots = items.Count;

				if (limitSlots && numSlots > maxSlots)
				{
					numSlots = maxSlots;
				}
				
				if (offset > 0 && (numSlots + offset) > items.Count)
				{
					offset = items.Count - numSlots;
				}
			}

			base.RecalculateSize ();
		}
		
		
		private void PopulateList ()
		{
			if (Application.isPlaying)
			{
				if (GameObject.FindWithTag (Tags.persistentEngine) && GameObject.FindWithTag (Tags.persistentEngine).GetComponent <RuntimeInventory>())
				{
					RuntimeInventory runtimeInventory = GameObject.FindWithTag (Tags.persistentEngine).GetComponent <RuntimeInventory>();

					if (inventoryBoxType == AC_InventoryBoxType.HostpotBased)
					{
						items = runtimeInventory.MatchInteractions ();
					}
					else if (inventoryBoxType == AC_InventoryBoxType.DisplaySelected)
					{
						items = runtimeInventory.GetSelected ();
					}
					else if (inventoryBoxType == AC_InventoryBoxType.DisplayLastSelected)
					{
						if (runtimeInventory.selectedItem != null)
						{
							items = new List<InvItem>();
							items = runtimeInventory.GetSelected ();
						}
						else if (items.Count == 1 && !runtimeInventory.IsItemCarried (items[0]))
						{
							items.Clear ();
						}
					}
					else if (inventoryBoxType == AC_InventoryBoxType.Container)
					{
						PlayerInput playerInput = GameObject.FindWithTag (Tags.gameEngine).GetComponent <PlayerInput>();
						if (playerInput.activeContainer)
						{
							items.Clear ();
							foreach (ContainerItem containerItem in playerInput.activeContainer.items)
							{
								InvItem referencedItem = new InvItem (AdvGame.GetReferences ().inventoryManager.GetItem (containerItem.linkedID));
								referencedItem.count = containerItem.count;
								items.Add (referencedItem);
							}
						}
					}
					else
					{
						items = new List<InvItem>();
						foreach (InvItem _item in runtimeInventory.localItems)
						{
							items.Add (_item);
						}
					}
				}
			}
			else
			{
				items = new List<InvItem>();
				if (AdvGame.GetReferences ().inventoryManager)
				{
					foreach (InvItem _item in AdvGame.GetReferences ().inventoryManager.items)
					{
						items.Add (_item);
						_item.recipeSlot = -1;
					}
				}
			}

			if (inventoryBoxType == AC_InventoryBoxType.Default || inventoryBoxType == AC_InventoryBoxType.CustomScript || inventoryBoxType == AC_InventoryBoxType.PlayerToContainer)
			{
				if (limitToCategory && categoryID > -1)
				{
					while (AreAnyItemsInWrongCategory ())
					{
						foreach (InvItem _item in items)
						{
							if (_item.binID != categoryID)
							{
								items.Remove (_item);
								break;
							}
						}
					}
				}

				while (AreAnyItemsInRecipe ())
				{
					foreach (InvItem _item in items)
					{
						if (_item.recipeSlot > -1)
						{
							items.Remove (_item);
							break;
						}
					}
				}
			}
		}


		private bool AreAnyItemsInRecipe ()
		{
			foreach (InvItem item in items)
			{
				if (item.recipeSlot >= 0)
				{
					return true;
				}
			}
			return false;
		}


		private bool AreAnyItemsInWrongCategory ()
		{
			foreach (InvItem item in items)
			{
				if (item.binID != categoryID)
				{
					return true;
				}
			}
			return false;
		}
		
		
		public void Shift (AC_ShiftInventory shiftType)
		{
			if (limitSlots)
			{
				if (numSlots >= maxSlots)
				{
					if (shiftType == AC_ShiftInventory.ShiftRight)
					{
						if ((maxSlots + offset) < items.Count)
						{
							offset ++;
						}
					}
					else if (shiftType == AC_ShiftInventory.ShiftLeft && offset > 0)
					{
						offset --;
					}
				}
			}
			else
			{
				Debug.Log ("Cannot offset " + title + " as it does not limit the number of available slots.");
			}
		}


		private void DrawTexture (Rect rect, int i)
		{
			Texture2D tex = null;

			if (Application.isPlaying && inventoryBoxType != AC_InventoryBoxType.DisplaySelected)
			{
				RuntimeInventory runtimeInventory = GameObject.FindWithTag (Tags.persistentEngine).GetComponent <RuntimeInventory>();
				
				if (items[i + offset] == runtimeInventory.selectedItem && items[i + offset].activeTex != null)
				{
					tex = items[i + offset].activeTex;
				}
				else if (items[i + offset].tex != null)
				{
					tex = items[i + offset].tex;
				}
			}
			else if (items[i + offset].tex != null)
			{
				tex = items[i + offset].tex;
			}

			if (tex != null)
			{
				GUI.DrawTexture (rect, tex, ScaleMode.StretchToFill, true, 0f);
			}
		}


		public override string GetLabel (int i)
		{
			if (items [i + offset].altLabel != "")
			{
				return items [i + offset].altLabel;
			}

			return items [i + offset].label;
		}


		public InvItem GetItem (int i)
		{
			return items [i + offset];
		}


		private string GetCount (int i)
		{
			if (items [i + offset].count < 2)
			{
				return "";
			}

			return items [i + offset].count.ToString ();
		}


		public void ResetOffset ()
		{
			offset = 0;
		}
		
		
		protected override void AutoSize ()
		{
			if (items.Count > 0)
			{
				AutoSize (new GUIContent (items[0].tex));
			}
			else
			{
				AutoSize (GUIContent.none);
			}
		}


		public void ClickContainer (int _buttonPressed, int _slot, Container container)
		{
			RuntimeInventory runtimeInventory = GameObject.FindWithTag (Tags.persistentEngine).GetComponent <RuntimeInventory>();

			if (container == null)
			{
				return;
			}

			if (_buttonPressed == 1)
			{
				ContainerItem containerItem = container.items [_slot + offset];
				runtimeInventory.Add (containerItem.linkedID, containerItem.count);
				container.items.Remove (containerItem);
			}
		}


		public void PlaceInContainer (int _buttonPressed, int _slot, Container container)
		{
			RuntimeInventory runtimeInventory = GameObject.FindWithTag (Tags.persistentEngine).GetComponent <RuntimeInventory>();
			
			if (container == null)
			{
				return;
			}
			
			if (_buttonPressed == 1)
			{
				ContainerItem newItem = new ContainerItem (items[_slot + offset].id, container.GetIDArray ());
				newItem.count = items[_slot + offset].count;
				container.items.Add (newItem);
				runtimeInventory.Remove (items[_slot + offset].id, items[_slot + offset].count);
			}
		}

	}

}