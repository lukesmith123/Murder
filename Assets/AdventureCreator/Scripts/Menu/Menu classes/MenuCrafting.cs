/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"MenuCrafting.cs"
 * 
 *	This MenuElement stores multiple Inventory Items to be combined.
 * 
 */

using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AC
{
	
	public class MenuCrafting : MenuElement
	{
		
		public bool doOutline;
		public CraftingElementType craftingType = CraftingElementType.Ingredients;
		public bool limitToCategory;
		public int categoryID;
		public List<InvItem> items = null;

		private Recipe activeRecipe;
		

		public override void Declare ()
		{
			isVisible = true;
			isClickable = true;
			numSlots = 4;
			SetSize (new Vector2 (6f, 10f));
			doOutline = false;
			limitToCategory = false;
			categoryID = -1;
			craftingType = CraftingElementType.Ingredients;
			items = new List<InvItem>();
		}
		
		
		public void CopyCrafting (MenuCrafting _element)
		{
			isClickable = _element.isClickable;
			doOutline = _element.doOutline;
			numSlots = _element.numSlots;
			limitToCategory = _element.limitToCategory;
			categoryID = _element.categoryID;
			craftingType = _element.craftingType;
			PopulateList ();
			
			base.Copy (_element);
		}
		
		
		#if UNITY_EDITOR
		
		public override void ShowGUI ()
		{
			EditorGUILayout.BeginVertical ("Button");
			doOutline = EditorGUILayout.Toggle ("Outline text?", doOutline);
			craftingType = (CraftingElementType) EditorGUILayout.EnumPopup ("Crafting element type:", craftingType);

			if (craftingType == CraftingElementType.Ingredients)
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
					
				numSlots = EditorGUILayout.IntSlider ("Number of slots:", numSlots, 1, 12);
				slotSpacing = EditorGUILayout.Slider ("Slot spacing:", slotSpacing, 0f, 20f);
				orientation = (ElementOrientation) EditorGUILayout.EnumPopup ("Slot orientation:", orientation);
				if (orientation == ElementOrientation.Grid)
				{
					gridWidth = EditorGUILayout.IntSlider ("Grid size:", gridWidth, 1, 10);
				}
			}
			else
			{
				categoryID = -1;
				numSlots = 1;
			}

			isClickable = true;
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

			if (craftingType == CraftingElementType.Ingredients)
			{
				// Is slot filled?
				bool isFilled = false;
				foreach (InvItem _item in items)
				{
					if (_item.recipeSlot == _slot)
					{
						isFilled = true;
						break;
					}
				}
				
				GUI.Label (GetSlotRectRelative (_slot), "", _style);

				if (!isFilled)
				{
					return;
				}
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
			else if (craftingType == CraftingElementType.Output)
			{
				GUI.Label (GetSlotRectRelative (_slot), "", _style);
				if (items.Count > 0)
				{
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
		}
		

		public void HandleDefaultClick (int _buttonPressed, int _slot)
		{
			if (GameObject.FindWithTag (Tags.persistentEngine) && GameObject.FindWithTag (Tags.persistentEngine).GetComponent <RuntimeInventory>())
			{
				RuntimeInventory runtimeInventory = GameObject.FindWithTag (Tags.persistentEngine).GetComponent <RuntimeInventory>();

				if (craftingType == CraftingElementType.Ingredients)
				{
					if (_buttonPressed == 1)
					{
						if (runtimeInventory.selectedItem == null)
						{
							if (GetItem (_slot) != null)
							{
								runtimeInventory.selectedItem = runtimeInventory.GetItem (GetItem (_slot).id);
								GetItem (_slot).recipeSlot = -1;
							}
						}
						else
						{
							if (GetItem (_slot) != null)
							{
								GetItem (_slot).recipeSlot = -1;
							}

							runtimeInventory.selectedItem.recipeSlot = _slot;
							runtimeInventory.selectedItem = null;
						}
					}
					else if (_buttonPressed == 2)
					{
						if (runtimeInventory.selectedItem != null)
						{
							runtimeInventory.SetNull ();
						}
					}

					PlayerMenus.ResetInventoryBoxes ();
				}
							}
		}
		

		public void ClickOutput (Menu _menu, int _buttonPressed)
		{
			if (GameObject.FindWithTag (Tags.persistentEngine) && GameObject.FindWithTag (Tags.persistentEngine).GetComponent <RuntimeInventory>())
			{
				RuntimeInventory runtimeInventory = GameObject.FindWithTag (Tags.persistentEngine).GetComponent <RuntimeInventory>();

				if (items.Count > 0)
				{
					if (_buttonPressed == 1)
					{
						if (runtimeInventory.selectedItem == null)
						{
							// Pick up created item
							InvItem resultItem = runtimeInventory.PerformCrafting (activeRecipe);
							if (activeRecipe.onCreateRecipe == OnCreateRecipe.SelectItem)
							{
								runtimeInventory.selectedItem = resultItem;
							}
							else if (activeRecipe.onCreateRecipe == OnCreateRecipe.RunActionList)
							{
								if (activeRecipe.invActionList != null && GameObject.FindWithTag (Tags.gameEngine) && GameObject.FindWithTag (Tags.gameEngine).GetComponent <RuntimeActionList>())
								{
									GameObject.FindWithTag (Tags.gameEngine).GetComponent <RuntimeActionList>().Play (activeRecipe.invActionList, _menu);
								}
							}
						}
					}
					PlayerMenus.ResetInventoryBoxes ();
				}
			}
		}


		public override void RecalculateSize ()
		{
			PopulateList ();
			base.RecalculateSize ();
		}
		
		
		private void PopulateList ()
		{
			if (Application.isPlaying)
			{
				if (GameObject.FindWithTag (Tags.persistentEngine) && GameObject.FindWithTag (Tags.persistentEngine).GetComponent <RuntimeInventory>())
				{
					RuntimeInventory runtimeInventory = GameObject.FindWithTag (Tags.persistentEngine).GetComponent <RuntimeInventory>();

					if (craftingType == CraftingElementType.Ingredients)
					{
						items = new List<InvItem>();
						foreach (InvItem _item in runtimeInventory.localItems)
						{
							if (_item.recipeSlot >= 0)
							{
								items.Add (_item);
							}
						}
					}
					else if (craftingType == CraftingElementType.Output)
					{
						SetOutput (true, runtimeInventory);
						return;
					}
				}
			}
			else
			{
				items = new List<InvItem>();
				return;
			}
			
			if (limitToCategory && categoryID > -1)
			{
				while (AreAnyItemsInWrongCategory ())
				{
					foreach (InvItem _item in items)
					{
						if (_item.binID != categoryID)
						{
							_item.recipeSlot = -1;
							items.Remove (_item);
							break;
						}
					}
				}
			}
		}


		public void SetOutput (bool autoCreate, RuntimeInventory runtimeInventory)
		{
			items = new List<InvItem>();
			activeRecipe = runtimeInventory.CalculateRecipe (autoCreate);
			if (activeRecipe != null)
			{
				foreach (InvItem assetItem in AdvGame.GetReferences ().inventoryManager.items)
				{
					if (assetItem.id == activeRecipe.resultID)
					{
						InvItem newItem = assetItem;
						newItem.count = 1;
						items.Add (newItem);
					}
				}
			}

			if (!autoCreate)
			{
				base.RecalculateSize ();
			}
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

		
		private void DrawTexture (Rect rect, int i)
		{
			Texture2D tex = null;
			
			if (Application.isPlaying)
			{
				tex = GetItem (i).tex;
			}
			else if (items [i].tex != null)
			{
				tex = items [i].tex;
			}
			
			if (tex != null)
			{
				GUI.DrawTexture (rect, tex, ScaleMode.StretchToFill, true, 0f);
			}
		}
		
		
		public override string GetLabel (int i)
		{
			if (GetItem (i).altLabel != "")
			{
				return GetItem (i).altLabel;
			}
			
			return GetItem (i).label;
		}
		
		
		public InvItem GetItem (int i)
		{
			if (craftingType == CraftingElementType.Output)
			{
				if (items.Count > i)
				{
					return items [i];
				}
			}
			else if (craftingType == CraftingElementType.Ingredients)
			{
				foreach (InvItem _item in items)
				{
					if (_item.recipeSlot == i)
					{
						return _item;
					}
				}
			}
			return null;
		}
		
		
		private string GetCount (int i)
		{
			if (GetItem (i).count < 2)
			{
				return "";
			}
			
			return GetItem (i).count.ToString ();
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
		
	}
	
}