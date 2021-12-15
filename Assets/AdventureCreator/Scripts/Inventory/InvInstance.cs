/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2020
 *	
 *	"InvInstance.cs"
 * 
 *	This script stores information about a runtime instance of an inventory item.
 * 
 */

using UnityEngine;
using System.Collections.Generic;

namespace AC
{

	/**
	 * A data class for an Inventory item instance.
	 * When an item is added to the Player's inventory at runtime, a reference is created to it with this class.
	 */
	public class InvInstance
	{

		#region Variables

		private int itemID;
		private int count;
		private int transferCount;
		private InvItem invItem;
		private List<InvVar> invVars;
		private CursorIcon cursorIcon;
		private MatchingInvInteractionData matchingInvInteractionData;
		private int lastInteractionIndex;
		private SelectItemMode selectItemMode = SelectItemMode.Use;
		private bool canBeAnimated;

		#endregion


		#region Constructors

		/**
		 * <summary>A Constructor that copies values of another class instance</summary>
		 * <param name="invInstance">The class instance to copy from</param>
		 */
		public InvInstance (InvInstance invInstance)
		{
			if (IsValid (invInstance))
			{
				itemID = invInstance.itemID;
				invItem = invInstance.invItem;
				invVars = invInstance.invVars;
				cursorIcon = invInstance.cursorIcon;
				matchingInvInteractionData = invInstance.matchingInvInteractionData;
				lastInteractionIndex = invInstance.lastInteractionIndex;
				Count = invInstance.count;
			}
			else
			{
				itemID = -1;
				count = -1;
				invItem = null;
				invVars = null;
				cursorIcon = null;
				matchingInvInteractionData = null;
				lastInteractionIndex = 0;
			}
			canBeAnimated = DetermineCanBeAnimated ();
		}

		
		/**
		 * <summary>A Constructor based on a Container item</summary>
		 * <param name="containerItem">The Container item to create an instance from</param>
		 */
		public InvInstance (ContainerItem containerItem)
		{
			itemID = containerItem.ItemID;
			invItem = (KickStarter.inventoryManager) ? KickStarter.inventoryManager.GetItem (itemID) : null;
			if (invItem != null) invItem.Upgrade ();
			Count = containerItem.Count;
			invVars = new List<InvVar> ();
			cursorIcon = (invItem != null) ? new CursorIcon (invItem.cursorIcon) : null;
			matchingInvInteractionData = null;
			lastInteractionIndex = 0;
			GenerateDefaultProperties ();
			canBeAnimated = DetermineCanBeAnimated ();
		}


		/**
		 * <summary>A Constructor based on the linked InvItem's ID</summary>
		 * <param name="_itemID">The ID number of the associated InvItem</param>
		 * <param name="_count">The amount of that item to reference</param>
		 */
		public InvInstance (int _itemID, int _count = 1)
		{
			itemID = _itemID;
			invItem = (KickStarter.inventoryManager) ? KickStarter.inventoryManager.GetItem (itemID) : null;
			if (invItem != null) invItem.Upgrade ();
			Count = _count;
			invVars = new List<InvVar>();
			cursorIcon = (invItem != null) ? new CursorIcon (invItem.cursorIcon) : null;
			matchingInvInteractionData = null;
			lastInteractionIndex = 0;
			GenerateDefaultProperties ();
			canBeAnimated = DetermineCanBeAnimated ();
		}


		/**
		 * <summary>A Constructor based on the linked InvItem's name</summary>
		 * <param name="_itemName">The name number of the associated InvItem</param>
		 * <param name="_count">The amount of that item to reference</param>
		 */
		public InvInstance (string _itemName, int _count = 1)
		{
			invItem = (KickStarter.inventoryManager) ? KickStarter.inventoryManager.GetItem (_itemName) : null;
			if (invItem != null) invItem.Upgrade ();
			Count = _count;
			itemID = (invItem != null) ? invItem.id : -1;
			invVars = new List<InvVar> ();
			cursorIcon = (invItem != null) ? new CursorIcon (invItem.cursorIcon) : null;
			matchingInvInteractionData = null;
			lastInteractionIndex = 0;
			GenerateDefaultProperties ();
			canBeAnimated = DetermineCanBeAnimated ();
		}


		/**
		 * <summary>A Constructor based on the linked InvItem</summary>
		 * <param name="_itemID">The associated InvItem</param>
		 * <param name="_count">The amount of that item to reference</param>
		 */
		public InvInstance (InvItem _invItem, int _count = 1)
		{
			invItem = _invItem;
			if (invItem != null) invItem.Upgrade ();
			itemID = (invItem != null) ? invItem.id : -1;
			Count = _count;
			invVars = new List<InvVar>();
			cursorIcon = (invItem != null) ? new CursorIcon (invItem.cursorIcon) : null;
			matchingInvInteractionData = null;
			lastInteractionIndex = 0;
			GenerateDefaultProperties ();
			canBeAnimated = DetermineCanBeAnimated ();
		}


		/**
		 * <summary>A Constructor based on the linked InvItem's ID</summary>
		 * <param name="_itemID">The ID number of the associated InvItem</param>
		 * <param name="_count">The amount of that item to reference</param>
		 * <param name="propertyData">Serialized data related to the instance's property data, which will override the default values.</param>
		 */
		public InvInstance (int _itemID, int _count, string propertyData)
		{
			itemID = _itemID;
			invItem = (KickStarter.inventoryManager) ? KickStarter.inventoryManager.GetItem (itemID) : null;
			if (invItem != null) invItem.Upgrade ();
			Count = _count;
			invVars = new List<InvVar>();
			cursorIcon = (invItem != null) ? new CursorIcon (invItem.cursorIcon) : null;
			matchingInvInteractionData = null;
			lastInteractionIndex = 0;
			GenerateDefaultProperties ();
			LoadPropertyData (propertyData);
			canBeAnimated = DetermineCanBeAnimated ();
		}

		#endregion


		#region PublicFunctions

		/** Selects the associated item */
		public void Select (SelectItemMode _selectItemMode = SelectItemMode.Use)
		{
			if (InvInstance.IsValid (this))
			{
				KickStarter.runtimeInventory.SelectItem (this, _selectItemMode);
			}
		}


		/** Dselects the associated item, if selected */
		public void Deselect ()
		{
			if (KickStarter.runtimeInventory.SelectedInstance == this)
			{
				KickStarter.runtimeInventory.SetNull ();
			}
		}


		/**
		 * <summary>Gets the local property of the associated item's</summary>
		 * <param name="propertyID">The ID of the local property to retrieve</param>
		 * <returns>The local property</returns>
		 */
		public InvVar GetProperty (int propertyID)
		{
			if (IsValid (this) && invVars.Count > 0)
			{
				foreach (InvVar var in invVars)
				{
					if (var.id == propertyID)
					{
						return var;
					}
				}
			}
			return null;
		}


		/** Reduces the item instance's own internal amount of associated items, and creates a new one ready to be transferred to another collection */
		public InvInstance CreateTransferInstance ()
		{
			InvCollection invCollection = GetSource ();

			InvInstance newInstance = new InvInstance (this);
			if (transferCount > 0) newInstance.count = transferCount;

			count -= newInstance.count;
			transferCount = 0;

			if (KickStarter.eventManager) KickStarter.eventManager.Call_OnChangeInventory (invCollection, this, InventoryEventType.Remove, newInstance.count);

			return newInstance;
		}


		/** Removes all counts of the item from this instance, while ensuring that the OnInventoryRemove event is triggered */
		public void Clear ()
		{
			Clear (count);
		}


		/** 
		 * <summary>Removes a given amount of the item from this instance, while ensuring that the OnInventoryRemove event is triggered</summary>
		 * <param name="amount">How much of the item to remove</param>
		 */
		public void Clear (int amount)
		{
			if (!IsValid (this)) return;

			transferCount = amount;
			CreateTransferInstance ();
		}


		/** Gets the Container that this item instance is a part of, if it's in one */
		public Container GetSourceContainer ()
		{
			if (KickStarter.stateHandler)
			{
				foreach (Container container in KickStarter.stateHandler.Containers)
				{
					if (container && container.InvCollection.Contains (this))
					{
						return container;
					}
				}
			}
			return null;
		}


		/** Gets the collection of item instances that this is a part of */
		public InvCollection GetSource ()
		{
			if (!IsValid (this))
			{
				return null;
			}

			if (KickStarter.stateHandler)
			{
				if (KickStarter.runtimeInventory)
				{
					if (KickStarter.runtimeInventory.PlayerInvCollection.Contains (this))
					{
						return KickStarter.runtimeInventory.PlayerInvCollection;
					}
					if (KickStarter.runtimeInventory.CraftingInvCollection.Contains (this))
					{
						return KickStarter.runtimeInventory.CraftingInvCollection;
					}
				}

				Container container = GetSourceContainer ();
				if (container)
				{
					return container.InvCollection;
				}
			}

			return null;
		}


		/** Runs an inventory item's "Examine" interaction */
		public void Examine ()
		{
			if (!IsValid (this)) return;

			if (InvItem.lookActionList)
			{
				KickStarter.eventManager.Call_OnUseInventory (this, KickStarter.cursorManager.lookCursor_ID);
				AdvGame.RunActionListAsset (InvItem.lookActionList);
			}
		}


		/** Runs an inventory item's "Use" interaction */
		public void Use (bool selectIfUnhandled = true)
		{
			if (!IsValid (this)) return;

			if (KickStarter.stateHandler.gameState == GameState.DialogOptions && !KickStarter.settingsManager.allowInventoryInteractionsDuringConversations && !KickStarter.settingsManager.allowGameplayDuringConversations)
			{
				return;
			}

			if (InvItem.useActionList)
			{
				KickStarter.runtimeInventory.SetNull ();
				KickStarter.eventManager.Call_OnUseInventory (this, 0);
				AdvGame.RunActionListAsset (InvItem.useActionList);
			}
			else if (KickStarter.settingsManager.CanSelectItems (true) && selectIfUnhandled)
			{
				KickStarter.runtimeInventory.SelectItem (this, SelectItemMode.Use);
			}
		}


		/**
		 * <summary>Runs an inventory item's interaction, when multiple "use" interactions are defined.</summary>
		 * <param name = "iconID">The ID number of the interaction's icon, defined in CursorManager</param>
		 */
		public void Use (int iconID)
		{
			if (!IsValid (this)) return;

			if (KickStarter.stateHandler.gameState == GameState.DialogOptions &&
				!KickStarter.settingsManager.allowInventoryInteractionsDuringConversations &&
				!KickStarter.settingsManager.allowGameplayDuringConversations)
			{
				return;
			}

			foreach (InvInteraction interaction in InvItem.interactions)
			{
				if (interaction.icon.id == iconID)
				{
					if (interaction.actionList)
					{
						KickStarter.eventManager.Call_OnUseInventory (this, iconID);
						AdvGame.RunActionListAsset (interaction.actionList);
						return;
					}
					break;
				}
			}

			// Unhandled
			if (KickStarter.settingsManager.InventoryInteractions == InventoryInteractions.Multiple && KickStarter.settingsManager.CanSelectItems (false))
			{
				// Auto-select
				if (KickStarter.settingsManager.selectInvWithUnhandled && iconID == KickStarter.settingsManager.selectInvWithIconID)
				{
					KickStarter.runtimeInventory.SelectItem (this, SelectItemMode.Use);
					return;
				}
				if (KickStarter.settingsManager.giveInvWithUnhandled && iconID == KickStarter.settingsManager.giveInvWithIconID)
				{
					KickStarter.runtimeInventory.SelectItem (this, SelectItemMode.Give);
					return;
				}
			}

			KickStarter.eventManager.Call_OnUseInventory (this, iconID);
			AdvGame.RunActionListAsset (KickStarter.cursorManager.GetUnhandledInteraction (iconID));
		}


		/**
		 * <summary>Combines two inventory items.</summary>
		 * <param name = "combineInstance">The instance of the inventory item to combine</param>
		 * <param name = "allowSelfCombining">If True, then an item can be combined with itself</param>
		 */
		public void Combine (InvInstance combineInstance, bool allowSelfCombining = false)
		{
			if (!IsValid (this)) return;
			if (!IsValid (combineInstance)) return;

			if ((this == combineInstance || InvItem == combineInstance.invItem) && !allowSelfCombining)
			{
				if ((KickStarter.settingsManager.interactionMethod != AC_InteractionMethod.ChooseHotspotThenInteraction || KickStarter.settingsManager.InventoryInteractions == InventoryInteractions.Single) && KickStarter.settingsManager.InventoryDragDrop)
				{
					if (KickStarter.settingsManager.dragDropThreshold <= 0f)
					{
						if (KickStarter.settingsManager.inventoryDropLook)
						{
							combineInstance.Examine ();
						}
					}
					else
					{
						if (KickStarter.playerInput.GetDragState () == DragState.Inventory)
						{
							if (KickStarter.settingsManager.inventoryDropLook)
							{
								combineInstance.Examine ();
							}
						}
						else if (KickStarter.settingsManager.inventoryDropLookNoDrag)
						{
							combineInstance.Examine ();
						}
					}
				}
				else if (KickStarter.settingsManager.interactionMethod == AC_InteractionMethod.ChooseHotspotThenInteraction && KickStarter.settingsManager.inventoryInteractions == InventoryInteractions.Multiple && KickStarter.settingsManager.InventoryDragDrop
					&& KickStarter.settingsManager.dragDropThreshold > 0f && KickStarter.playerInput.GetDragState () != DragState.Inventory && KickStarter.settingsManager.inventoryDropLookNoDrag)
				{
					KickStarter.runtimeInventory.ShowInteractions (this);
					return;
				}
				
				if (this == combineInstance && this == KickStarter.runtimeInventory.SelectedInstance && CanStack ())
				{
					AddStack ();
					return;
				}

				KickStarter.runtimeInventory.SetNull ();
				KickStarter.eventManager.Call_OnCombineInventory (this, combineInstance);
			}
			else
			{
				KickStarter.eventManager.Call_OnCombineInventory (this, combineInstance);

				for (int i = 0; i < combineInstance.InvItem.combineID.Count; i++)
				{
					if (combineInstance.InvItem.combineID[i] == ItemID && combineInstance.InvItem.combineActionList[i])
					{
						if (KickStarter.settingsManager.inventoryDisableDefined)
						{
							KickStarter.runtimeInventory.SetNull ();
						}

						AdvGame.RunActionListAsset (combineInstance.InvItem.combineActionList[i]);
						return;
					}
				}

				if (KickStarter.settingsManager.reverseInventoryCombinations || (KickStarter.settingsManager.SelectInteractionMethod () == SelectInteractions.CyclingCursorAndClickingHotspot && KickStarter.settingsManager.InventoryInteractions == InventoryInteractions.Multiple))
				{
					// Try opposite: search selected item instead
					for (int i = 0; i < InvItem.combineID.Count; i++)
					{
						if (InvItem.combineID[i] == combineInstance.ItemID && InvItem.combineActionList[i])
						{
							if (KickStarter.settingsManager.inventoryDisableDefined)
							{
								KickStarter.runtimeInventory.SetNull ();
							}

							ActionListAsset assetFile = InvItem.combineActionList[i];
							AdvGame.RunActionListAsset (assetFile);
							return;
						}
					}
				}

				// Found no combine match

				if (KickStarter.settingsManager.inventoryDisableUnhandled)
				{
					KickStarter.runtimeInventory.SetNull ();
				}

				if (InvItem.unhandledCombineActionList)
				{
					ActionListAsset unhandledActionList = combineInstance.InvItem.unhandledCombineActionList;
					AdvGame.RunActionListAsset (unhandledActionList);
				}
				else if (KickStarter.inventoryManager.unhandledCombine)
				{
					AdvGame.RunActionListAsset (KickStarter.inventoryManager.unhandledCombine);
				}
			}

			KickStarter.playerCursor.ResetSelectedCursor ();
		}


		/** Gets the amount of instances not being selected/transferred */
		public int GetInventoryDisplayCount ()
		{
			if (transferCount > 0)
			{
				return count - transferCount;
			}
			return count;
		}


		/** Checks if the item instance is only being partially selected/transferred, in that the number of items being affected is not the same as its total capacity */
		public bool IsPartialTransform ()
		{
			return TransferCount != count;
		}


		/** Checks if stacking is possible */
		public bool CanStack ()
		{
			return (ItemStackingMode == ItemStackingMode.Stack && Count != transferCount);
		}


		/** Increases the transfer count by 1, if possible */
		public void AddStack ()
		{
			int newTransferCount = Mathf.Clamp (transferCount + 1, 0, count);
			if (newTransferCount != transferCount)
			{
				transferCount = newTransferCount;
				KickStarter.eventManager.Call_OnChangeInventory (null, this, InventoryEventType.Select);
			}
		}


		/** Removes the transfer count by 1. If the transfer count is then 1, and the item is selected, it will be de-selected automatically. */
		public void RemoveStack ()
		{
			transferCount--;
			if (transferCount <= 0 && KickStarter.runtimeInventory.SelectedInstance == this)
			{
				KickStarter.runtimeInventory.SetNull ();
			}
			else
			{
				KickStarter.eventManager.Call_OnChangeInventory (null, this, InventoryEventType.Select);
			}
		}


		/**
		 * <summary>Gets data related to what interactions are possible</summary>
		 * <param name="rebuild">If True, the data will be rebuilt before being returned</param>
		 * <returns>Data related to what interactions are possible</returns>
		 */
		public MatchingInvInteractionData GetMatchingInvInteractionData (bool rebuild)
		{
			if ((rebuild || matchingInvInteractionData == null) && invItem != null)
			{
				matchingInvInteractionData = new MatchingInvInteractionData (invItem);
			}
			return matchingInvInteractionData;
		}


		/**
		 * <summary>Gets the index number of the next relevant use/combine interaction.</summary>
		 * <param name = "i">The index number to start from</param>
		 */
		public int GetNextInteraction (int i)
		{
			if (invItem == null) return i;

			int numInvInteractions = GetMatchingInvInteractionData (true).NumMatchingInteractions;

			if (i < invItem.interactions.Count)
			{
				i++;

				if (i >= invItem.interactions.Count + numInvInteractions)
				{
					return 0;
				}
				else
				{
					return i;
				}
			}
			else if (i == invItem.interactions.Count - 1 + numInvInteractions)
			{
				return 0;
			}

			return (i + 1);
		}


		/**
		 * <summary>Gets the index number of the previous relevant use/combine interaction.</summary>
		 * <param name = "i">The index number to start from</param>
		 */
		public int GetPreviousInteraction (int i)
		{
			if (invItem == null) return i;

			int numInvInteractions = GetMatchingInvInteractionData (true).NumMatchingInteractions;

			if (i > invItem.interactions.Count && numInvInteractions > 0)
			{
				return (i - 1);
			}
			else if (i == 0)
			{
				return invItem.interactions.Count + numInvInteractions - 1;
			}
			else if (i <= invItem.interactions.Count)
			{
				i--;

				if (i < 0)
				{
					return invItem.interactions.Count + numInvInteractions - 1;
				}
				else
				{
					return i;
				}
			}

			return (i - 1);
		}


		/** Gets the ID of the active inventory interaction, or -1 if none is found */
		public int GetActiveInvButtonID ()
		{
			int interactionIndex = KickStarter.playerInteraction.InteractionIndex;

			if (KickStarter.settingsManager.SelectInteractionMethod () == SelectInteractions.CyclingCursorAndClickingHotspot)
			{
				int numInteractions = (InvItem.interactions != null) ? InvItem.interactions.Count : 0;
				if (interactionIndex >= numInteractions && GetMatchingInvInteractionData (false).NumMatchingInteractions > 0)
				{
					int combineIndex = GetMatchingInvInteractionData (false).GetInvInteractionIndex (interactionIndex - numInteractions);
					return InvItem.combineID[combineIndex];
				}
			}
			else
			{
				int numInteractions = (InvItem.interactions != null) ? InvItem.interactions.Count : 0;
				if (interactionIndex >= numInteractions && GetMatchingInvInteractionData (false).NumMatchingInteractions > 0)
				{
					int combineIndex = GetMatchingInvInteractionData (false).GetInvInteractionIndex (interactionIndex - numInteractions);
					return InvItem.combineID[combineIndex];
				}
			}

			return -1;
		}


		/** Restores the previous interaction state */
		public void RestoreInteraction ()
		{
			if (IsValid (KickStarter.runtimeInventory.SelectedInstance) && KickStarter.settingsManager.CanSelectItems (false))
			{
				return;
			}

			if (KickStarter.settingsManager.SelectInteractionMethod () != AC.SelectInteractions.CyclingCursorAndClickingHotspot)
			{
				return;
			}

			GetMatchingInvInteractionData (true);

			switch (KickStarter.settingsManager.whenReselectHotspot)
			{
				case WhenReselectHotspot.ResetIcon:
					KickStarter.playerInteraction.InteractionIndex = lastInteractionIndex = 0;
					return;

				case WhenReselectHotspot.RestoreHotspotIcon:
					KickStarter.playerInteraction.InteractionIndex = lastInteractionIndex;
					if (!KickStarter.settingsManager.cycleInventoryCursors && GetActiveInvButtonID () >= 0)
					{
						KickStarter.playerInteraction.InteractionIndex = -1;
						return;
					}
					else
					{
						int invID = GetActiveInvButtonID ();
						if (invID >= 0)
						{
							KickStarter.runtimeInventory.SelectItemByID (invID, SelectItemMode.Use);
						}
						else if (KickStarter.settingsManager.cycleInventoryCursors && KickStarter.settingsManager.InventoryInteractions == InventoryInteractions.Multiple)
						{
							KickStarter.runtimeInventory.SetNull ();
						}
					}
					break;

				default:
					break;
			}
		}


		/**
		 * <summary>Gets the full prefix to a Hotpsot label when an item is selected, e.g. "Use X on " / "Give X to ".</summary>
		 * <param name = "invItem">The inventory item that is selected</param>
		 * <param name = "languageNumber">The index of the current language, as set in SpeechManager</param>
		 * <param name = "canGive">If True, the the item is assumed to be in "give" mode, as opposed to "use".</param>
		 * <returns>The full prefix to a Hotspot label when the item is selected</returns>
		 */
		public string GetHotspotPrefixLabel (int languageNumber, bool canGive = false)
		{
			if (KickStarter.runtimeInventory == null) return string.Empty;

			string prefix1 = string.Empty;
			string prefix2 = string.Empty;

			string itemName = (invItem != null) ? invItem.GetLabel (languageNumber) : string.Empty;

			if (canGive && selectItemMode == SelectItemMode.Give)
			{
				prefix1 = KickStarter.runtimeLanguages.GetTranslation (KickStarter.cursorManager.hotspotPrefix3.label, KickStarter.cursorManager.hotspotPrefix3.lineID, languageNumber, KickStarter.cursorManager.hotspotPrefix3.GetTranslationType (0));
				prefix2 = KickStarter.runtimeLanguages.GetTranslation (KickStarter.cursorManager.hotspotPrefix4.label, KickStarter.cursorManager.hotspotPrefix4.lineID, languageNumber, KickStarter.cursorManager.hotspotPrefix4.GetTranslationType (0));
			}
			else
			{
				if (invItem != null && invItem.overrideUseSyntax)
				{
					prefix1 = KickStarter.runtimeLanguages.GetTranslation (invItem.hotspotPrefix1.label, invItem.hotspotPrefix1.lineID, languageNumber, invItem.hotspotPrefix1.GetTranslationType (0));
					prefix2 = KickStarter.runtimeLanguages.GetTranslation (invItem.hotspotPrefix2.label, invItem.hotspotPrefix2.lineID, languageNumber, invItem.hotspotPrefix2.GetTranslationType (0));
				}
				else
				{
					prefix1 = KickStarter.runtimeLanguages.GetTranslation (KickStarter.cursorManager.hotspotPrefix1.label, KickStarter.cursorManager.hotspotPrefix1.lineID, languageNumber, KickStarter.cursorManager.hotspotPrefix1.GetTranslationType (0));
					prefix2 = KickStarter.runtimeLanguages.GetTranslation (KickStarter.cursorManager.hotspotPrefix2.label, KickStarter.cursorManager.hotspotPrefix2.lineID, languageNumber, KickStarter.cursorManager.hotspotPrefix2.GetTranslationType (0));
				}
			}

			if (string.IsNullOrEmpty (prefix1) && !string.IsNullOrEmpty (prefix2))
			{
				return prefix2;
			}
			if (!string.IsNullOrEmpty (prefix1) && string.IsNullOrEmpty (prefix2))
			{
				if (invItem != null && invItem.canBeLowerCase)
				{
					itemName = itemName.ToLower ();
				}

				return AdvGame.CombineLanguageString (prefix1, itemName, languageNumber);
			}
			if (prefix1 == " " && !string.IsNullOrEmpty (prefix2))
			{
				return AdvGame.CombineLanguageString (itemName, prefix2, languageNumber);
			}

			if (invItem != null && invItem.canBeLowerCase)
			{
				itemName = itemName.ToLower ();
			}

			if (KickStarter.runtimeLanguages.LanguageReadsRightToLeft (languageNumber))
			{
				return (prefix2 + " " + itemName + " " + prefix1);
			}

			return (prefix1 + " " + itemName + " " + prefix2);
		}

		#endregion


		#region PrivateFunctions

		private string GetPropertySaveData ()
		{
			string dataString = string.Empty;
			if (invVars.Count > 0)
			{
				foreach (InvVar invVar in invVars)
				{
					switch (invVar.type)
					{
						case VariableType.Float:
							dataString += invVar.id.ToString () + "_";
							dataString += invVar.FloatValue.ToString ();
							break;

						case VariableType.String:
							break;

						case VariableType.Vector3:
							dataString += invVar.id.ToString () + "_";
							dataString += invVar.Vector3Value.x + "," + invVar.Vector3Value.y + "," + invVar.Vector3Value.z;
							break;

						default:
							dataString += invVar.id.ToString () + "_";
							dataString += invVar.IntegerValue.ToString ();
							break;
					}

					dataString += "#";
				}
			}

			if (string.IsNullOrEmpty (dataString))
			{
				dataString = "#";
			}
			else
			{
				dataString.Remove (dataString.Length - 1, 1);
			}
			return dataString;
		}


		private void LoadPropertyData (string dataString)
		{
			if (string.IsNullOrEmpty (dataString)) return;

			string[] dataArray = dataString.Split ("#"[0]);
			if (dataArray.Length > 0)
			{
				foreach (string propertyData in dataArray)
				{
					string[] chunkData = propertyData.Split ("_"[0]);
					if (chunkData.Length == 2)
					{
						int _id = -1;
						int.TryParse (chunkData[0], out _id);

						if (_id >= 0)
						{
							InvVar invVar = GetProperty (_id);
							if (invVar != null)
							{
								invVar.LoadData (chunkData[1]);
							}
						}
					}
				}
			}
		}


		private void GenerateDefaultProperties ()
		{
			if (invItem != null)
			{
				invVars = new List<InvVar>();
				foreach (InvVar invVar in invItem.vars)
				{
					invVars.Add (new InvVar (invVar));
				}
			}
		}


		private bool DetermineCanBeAnimated ()
		{
			if (InvItem != null)
			{
				if (cursorIcon != null && cursorIcon.texture && cursorIcon.isAnimated)
				{
					return true;
				}
				if (InvItem.activeTex)
				{
					return true;
				}
			}
			return false;
		}

		#endregion


		#region StaticFunctions

		/**
		 * <summary>Checks if the instance is both non-null, and references an item</summary>
		 * <param name="invInstance">The instance to check</param>
		 * <returns>True if the instance is non-null and references an item</returns>
		 */
		public static bool IsValid (InvInstance invInstance)
		{
			if (invInstance != null)
			{
				return (invInstance.count > 0 && invInstance.itemID >= 0 && invInstance.invItem != null);
			}
			return false;
		}


		/**
		 * <summary>Generates data to save about an item instance in a single string.</summary>
		 * <param name = "invInstance">The inventory item instance to save</param>
		 * <returns>Save data for the instance as a single string</returns>
		 */
		public static string GetSaveData (InvInstance invInstance)
		{
			string dataString = string.Empty;

			if (IsValid (invInstance))
			{
				dataString += invInstance.itemID.ToString ();
				dataString += SaveSystem.colon;
				dataString += invInstance.count.ToString ();

				dataString += SaveSystem.colon;
				dataString += invInstance.GetPropertySaveData ();
				dataString += SaveSystem.pipe;
			}
			else if (KickStarter.settingsManager.canReorderItems)
			{
				dataString += "-1";
				dataString += SaveSystem.colon;
				dataString += "0";
				dataString += SaveSystem.colon;
				dataString += "_";
				dataString += SaveSystem.pipe;
			}

			return dataString;
		}

		#endregion


		#region GetSet

		/** The amount of the associated item this refers to */
		public int Count
		{
			get
			{
				return count;
			}
			set
			{
				count = (invItem != null && invItem.canCarryMultiple) ? Mathf.Clamp (value, 0, invItem.maxCount) : Mathf.Max (value, 0);
			}
		}


		/** The amount of items to transfer. To move only some of the items associated with this instance, set this value and then add the result of CreateTransferInstance to an InvCollection */
		public int TransferCount
		{
			get
			{
				return (transferCount > 0) ? transferCount : count;
			}
			set
			{
				transferCount = Mathf.Clamp (value, 0, count);
			}
		}


		/** The associated inventory item */
		public InvItem InvItem
		{
			get
			{
				return invItem;
			}
		}


		/** The associated inventory item's ID number */
		public int ItemID
		{
			get
			{
				return itemID;
			}
		}


		/** The associated inventory item's Cursor Icon */
		public CursorIcon CursorIcon
		{
			get
			{
				return cursorIcon;
			}
		}


		/** How many additional counts of the item can occupy this instance */
		public int Capacity
		{
			get
			{
				if (invItem != null)
				{
					return invItem.maxCount - Count;
				}
				return 0;
			}
		}


		/** An identifier number of the last Use/Inventory interaction associated with the item */
		public int LastInteractionIndex
		{
			get
			{
				return lastInteractionIndex;
			}
			set
			{
				lastInteractionIndex = value;
			}
		}


		/** The item selection mode */
		public SelectItemMode SelectItemMode
		{
			get
			{
				return selectItemMode;
			}
			set
			{
				if (!KickStarter.settingsManager.CanGiveItems ())
				{
					value = SelectItemMode.Use;
				}

				selectItemMode = value;
			}
		}


		/** The item stacking mode */
		public ItemStackingMode ItemStackingMode
		{
			get
			{
				if (count > 1 && invItem.canCarryMultiple && invItem.maxCount > 1)
				{
					return invItem.itemStackingMode;
				}

				return ItemStackingMode.All;;
			}
		}


		/** Checks if the item's assigned textures are enough for animated effects to be possible. */
		public bool CanBeAnimated
		{
			get
			{
				return canBeAnimated;
			}
		}

		#endregion

	}

}