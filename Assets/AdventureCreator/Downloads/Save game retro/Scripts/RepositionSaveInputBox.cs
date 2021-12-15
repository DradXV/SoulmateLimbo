using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace AC
{

	public class RepositionSaveInputBox : MonoBehaviour
	{

		public string menuName = "RetroSaves";
		public string savesListName = "SaveList";
		

		private void Awake ()
		{
			EventManager.OnMenuElementClick += OnMenuElementClick;
		}


		private void OnDestroy ()
		{
			EventManager.OnMenuElementClick -= OnMenuElementClick;
		}


		private void OnMenuElementClick (Menu _menu, MenuElement _element, int _slot, int buttonPressed)
		{
			if (_menu.title == menuName && _element.title == savesListName)
			{
				MenuSavesList savesList = _element as MenuSavesList;
				Vector3 position = savesList.uiSlots[_slot].uiButton.GetComponent <RectTransform>().position;

				GetComponent <RectTransform>().position = position;
			}
		}

	}

}