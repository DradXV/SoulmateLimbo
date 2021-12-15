using UnityEngine;
using UnityEditor;

namespace AC
{
	[CustomEditor (typeof (ConversationCamera))]
	public class ConversationCameraEditor : Editor
	{

		private ConversationCamera _target;
		

		public void OnEnable ()
		{
			_target = (ConversationCamera) target;
		}


		public override void OnInspectorGUI ()
		{
			if (_target == null) return;
			_target.ShowGUI ();
		}

	}

}