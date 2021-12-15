using System.Collections.Generic;

namespace AC
{

	public class MatchingInvInteractionData
	{

		private List<InvInstance> invInstances;
		private List<int> invInteractionIndices;
		private List<SelectItemMode> selectItemModes;


		public MatchingInvInteractionData (Hotspot hotspot)
		{
			invInstances = new List<InvInstance> ();
			invInteractionIndices = new List<int>();
			selectItemModes = new List<SelectItemMode>();

			for (int i=0; i<hotspot.invButtons.Count; i++)
			{
				Button button = hotspot.invButtons[i];
				if (button.isDisabled) continue;

				foreach (InvInstance invInstance in KickStarter.runtimeInventory.PlayerInvCollection.InvInstances)
				{
					if (InvInstance.IsValid (invInstance) && invInstance.ItemID == button.invID && !button.isDisabled)
					{
						invInteractionIndices.Add (i);
						selectItemModes.Add (button.selectItemMode);
						invInstances.Add (invInstance);
						break;
					}
				}
			}
		}


		public MatchingInvInteractionData (InvItem invItem)
		{
			invInstances = new List<InvInstance> ();
			invInteractionIndices = new List<int> ();
			selectItemModes = new List<SelectItemMode> ();

			for (int i=0; i<invItem.combineID.Count; i++)
			{
				foreach (InvInstance localInvInstance in KickStarter.runtimeInventory.PlayerInvCollection.InvInstances)
				{
					if (InvInstance.IsValid (localInvInstance) && localInvInstance.ItemID == invItem.combineID[i])
					{
						invInteractionIndices.Add (i);
						selectItemModes.Add (SelectItemMode.Use);
						invInstances.Add (localInvInstance);
						break;
					}
				}
			}
		}

		public void SetSelectItemMode (int index)
		{
			if (index >= 0 && index < selectItemModes.Count)
			{
				invInstances[index].SelectItemMode = selectItemModes[index];
			}
		}


		public int GetInvInteractionIndex (int index)
		{
			if (index >= 0 && index < invInteractionIndices.Count)
			{
				return invInteractionIndices[index];
			}
			return -1;
		}


		public List<InvInstance> InvInstances
		{
			get
			{
				return invInstances;
			}
		}


		public int NumMatchingInteractions
		{
			get
			{
				return invInstances.Count;
			}
		}

	}

}