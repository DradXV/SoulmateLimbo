using UnityEngine;

using System.Collections;

using AC;



public class DontDestroyMe : MonoBehaviour

{


	private void Start()

	{
		DontDestroyOnLoad(gameObject);
	}


}