using System.Collections;
using UnityEngine;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AC
{

	public class ConversationCamera : MonoBehaviour
	{

		#region Variables

		[SerializeField] private float minShotLength = 3f;
		[SerializeField] private bool extendCutscenesToFitMinShotLength = true;
		[SerializeField] private FloatRange cutDelay = new FloatRange (0f, 2f);
		[SerializeField] private float animatedShotFrequency = 0.6f;
		[SerializeField] private float maxAnimationDuration = 5f;
		[SerializeField] private bool autoTurnHeads = true;
		[SerializeField] private bool moveWithCharacters = false;

		private CharacterData characterDataA;
		private CharacterData characterDataB;

		private Coroutine focusOnCharacterCo;
		private bool isRunning;
		private bool isSleeping;
		private _Camera ownCamera;
		private enum ShotType { Centred=0, CloseUp=1, OverTheShoulder=2 };

		private float lastShotTime;
		private CameraShot activeCameraShot;
		private bool characterAOnRight;

		#endregion


		#region UnityStandards

		private void OnEnable ()
		{
			ownCamera = GetComponent <_Camera>();

			EventManager.OnStartSpeech_Alt += OnStartSpeech;
			EventManager.OnSwitchCamera += OnSwitchCamera;
		}


		private void OnDisable ()
		{
			EventManager.OnStartSpeech_Alt -= OnStartSpeech;
			EventManager.OnSwitchCamera -= OnSwitchCamera;
		}


		private void OnValidate ()
		{
			cutDelay.OnValidate ();
		}


		private void LateUpdate ()
		{
			if (activeCameraShot != null)
			{
				if (moveWithCharacters)
				{
					bool isReverseShot = activeCameraShot.IsReverseShot;
					CameraShot newShot = CreateShot (activeCameraShot.ShotType, (isReverseShot) ? characterDataB : characterDataA, (isReverseShot) ? characterDataA : characterDataB, isReverseShot);
					if (newShot != null)
					{
						activeCameraShot.Apply (ownCamera, newShot);
					}
				}

				bool endAnim = (Time.time - lastShotTime) > maxAnimationDuration;
				if (!endAnim)
				{
					activeCameraShot.Update (transform);
				}
			}
		}

		#endregion


		#region PublicFunctions

		public void Begin (Char _characterA, Char _characterB)
		{
			if (_characterA == null || _characterB == null) return;

			isRunning = true;
			activeCameraShot = null;

			characterDataA = new CharacterData (_characterA);
			characterDataB = new CharacterData (_characterB);

			Vector3 characterAWorldPosition = characterDataA.character.transform.position;
			Vector2 characterAScreenPosition = Camera.main.WorldToScreenPoint (characterAWorldPosition);

			Vector3 characterBWorldPosition = characterDataB.character.transform.position;
			Vector2 characterBScreenPosition = Camera.main.WorldToScreenPoint (characterBWorldPosition);

			characterAOnRight = (characterAScreenPosition.x > characterBScreenPosition.x);

			if (autoTurnHeads)
			{
				characterDataA.LookAt (characterDataB);
				characterDataB.LookAt (characterDataA);
			}
		}

		
		public void End (bool isSkipping = false)
		{
			if (!isRunning) return;

			isRunning = false;
			
			if (focusOnCharacterCo != null) StopCoroutine (focusOnCharacterCo);

			if (autoTurnHeads)
			{
				characterDataA.StopLooking (isSkipping);
				characterDataB.StopLooking (isSkipping);
			}

			if (KickStarter.mainCamera.attachedCamera == ownCamera)
			{
				if (isSkipping)
				{
					KickStarter.mainCamera.SetGameCamera (KickStarter.mainCamera.GetLastGameplayCamera ());
				}
				else if (extendCutscenesToFitMinShotLength)
				{
					AddEndPause ();
				}
				else
				{
					KickStarter.mainCamera.SetGameCamera (KickStarter.mainCamera.GetLastGameplayCamera ());
				}
			}

			characterDataA = null;
			characterDataB = null;

			activeCameraShot = null;
		}

		#endregion


		#region CustomEvents

		private void OnStartSpeech (Speech speech)
		{
			if (isRunning && !isSleeping)
			{
				if (IsReferenced (speech.GetSpeakingCharacter ()))
				{
					if (focusOnCharacterCo != null) StopCoroutine (focusOnCharacterCo);
					focusOnCharacterCo = StartCoroutine (FocusOnCharacterCo (speech.GetSpeakingCharacter ()));
				}
			}
		}


		private void OnSwitchCamera (_Camera oldCamera, _Camera newCamera, float transitionTime)
		{
			if (isRunning)
			{
				isSleeping = (newCamera != ownCamera);
				if (!isSleeping)
				{
					lastShotTime = Time.time;
				}
			}
			else
			{
				isSleeping = false;
			}
		}

		#endregion


		#region PrivateFunctions

		private IEnumerator FocusOnCharacterCo (Char character)
		{
			if (character != null)
			{
				float delayBeforeCut = 0f;
				
				if (activeCameraShot != null)
				{
					delayBeforeCut = cutDelay.GetRandomValue ();
					delayBeforeCut = Mathf.Max (delayBeforeCut, minShotLength + lastShotTime - Time.time);
				}
				
				yield return new WaitForSeconds (delayBeforeCut);
				
				bool isReverseShot = (character == characterDataB.character);
				
				CameraShot newShot = null;
				
				if (activeCameraShot != null)
				{
					if (activeCameraShot.MainCharacter.character == character)
					{
						bool changeCharacter = Random.Range (0, 10) >= 5;
						if (changeCharacter)
						{
							isReverseShot = !isReverseShot;
							newShot = CreateShot ((isReverseShot) ? characterDataB : characterDataA, (isReverseShot) ? characterDataA : characterDataB, isReverseShot);
						}
						else
						{
							if (activeCameraShot.ShotType == ShotType.Centred)
							{
								newShot = CreateShot (ShotType.CloseUp, (isReverseShot) ? characterDataB : characterDataA, (isReverseShot) ? characterDataA : characterDataB, isReverseShot);
							}
							else
							{
								newShot = CreateShot (ShotType.Centred, (isReverseShot) ? characterDataB : characterDataA, (isReverseShot) ? characterDataA : characterDataB, isReverseShot);
							}
						}
					}
					else
					{
						int i = 0;
						while (i < 10 && (newShot == null || newShot.ShotType == activeCameraShot.ShotType))
						{
							newShot = CreateShot ((isReverseShot) ? characterDataB : characterDataA, (isReverseShot) ? characterDataA : characterDataB, isReverseShot);
							i++;
						}
					}
				}
				else
				{
					int i=0;
					while (i < 10 && (newShot == null || newShot.ShotType == ShotType.CloseUp))
					{
						newShot = CreateShot ((isReverseShot) ? characterDataB : characterDataA, (isReverseShot) ? characterDataA : characterDataB, isReverseShot);
						i++;
					}
				}

				if (newShot != null)
				{
					activeCameraShot = newShot;
					activeCameraShot.Apply (ownCamera);
				}

				lastShotTime = Time.time;
			}
		}


		private ShotType GetRandomShotType ()
		{
			int rand = Random.Range (0, 3);

			switch (rand)
			{
				case 0:
					return ShotType.Centred;

				case 1:
					return ShotType.CloseUp;

				default:
					return ShotType.OverTheShoulder;
			}
		}


		private CameraShot CreateShot (CharacterData mainChar, CharacterData otherChar, bool isReverseShot)
		{
			return CreateShot (GetRandomShotType (), mainChar, otherChar, isReverseShot);
		}


		private CameraShot CreateShot (ShotType shotType, CharacterData mainChar, CharacterData otherChar, bool isReverseShot)
		{
			Vector3 forwardDirection = (mainChar.character.transform.position - otherChar.character.transform.position).normalized;

			float spinAngle = 0f;
			Vector3 centre = Vector3.zero;
			Vector3 lookAt = Vector3.zero;
			float distance = 0f;
			float fov = 0f;

			switch (shotType)
			{
				case ShotType.Centred:
					{
						Vector3 mainCharCentre = mainChar.character.transform.position + (Vector3.up * mainChar.height * 0.7f);
						Vector3 otherCharCentre = otherChar.character.transform.position + (Vector3.up * otherChar.height * 0.7f);

						spinAngle = (isReverseShot) ? 270f : 90f;
						if (characterAOnRight)
						{
							spinAngle += 180f;
						}

						centre = (mainCharCentre + otherCharCentre) / 2f;
						distance = Vector3.Distance (mainCharCentre, otherCharCentre) * 1.2f;
						if (distance < 2f)
						{
							centre += Vector3.up * mainChar.height * 0.1f;
						}
						lookAt = centre;
						fov = Mathf.Clamp (40f + (distance * 0.67f), 40f, 45f);
					}
					break;

				case ShotType.CloseUp:
					{
						Vector3 mainCharCentre = mainChar.character.transform.position + (Vector3.up * mainChar.height * 0.9f);

						spinAngle = (isReverseShot != characterAOnRight) ? 215f : 145f;

						centre = mainCharCentre - (forwardDirection * mainChar.radius);
						lookAt = centre;
						distance = mainChar.radius * 1.3f;
						fov = 38f;
					}
					break;

				case ShotType.OverTheShoulder:
					{
						Vector3 mainCharCentre = mainChar.character.transform.position + (Vector3.up * mainChar.height * 0.9f);
						Vector3 otherCharCentre = otherChar.character.transform.position + (Vector3.up * otherChar.height * 0.9f);
						Vector3 mainCharCentreOffset = mainCharCentre - (Vector3.up * mainChar.height * 0.1f);
						float characterDistance = Vector3.Distance (mainCharCentre, otherCharCentre);

						spinAngle = 180f + (Mathf.Rad2Deg * Mathf.Atan2 (otherChar.radius * 1.0f, characterDistance / 2f));
						if (isReverseShot == characterAOnRight) spinAngle *= -1f;

						centre = (mainCharCentre + otherCharCentre) / 2f;
						lookAt = (otherCharCentre + mainCharCentreOffset) / 2f;
						distance = (otherChar.radius * 1.7f) + (characterDistance / 2f);
						fov = 35f - (distance * 5f);
					}
					break;

				default:
					break;
			}

			Quaternion rotation = Quaternion.AngleAxis (spinAngle, Vector3.down);
			Vector3 position = centre + rotation * forwardDirection * distance;

			CameraShot newShot = new CameraShot (shotType, mainChar, position, lookAt, fov, animatedShotFrequency, isReverseShot);

			if (newShot.IsValid (mainChar, otherChar))
			{
				return newShot;
			}
			return null;
		}


		private void AddEndPause ()
		{
			float timeLeft = minShotLength + lastShotTime - Time.time;
			if (timeLeft > 0f)
			{
				ActionList actionList = GetComponent<ActionList> ();
				if (actionList)
				{
					if (actionList.AreActionsRunning ())
					{
						actionList.Kill ();
					}
				}
				else
				{
					actionList = gameObject.AddComponent<ActionList> ();
				}

				actionList.actions = new List<Action>
				{
					ActionPause.CreateNew (timeLeft),
					ActionCamera.CreateNew (KickStarter.mainCamera.GetLastGameplayCamera ())
				};

				actionList.Interact ();
			}
		}


		private bool IsReferenced (Char character)
		{
			if (character)
			{
				if (characterDataA != null && characterDataA.character == character)
				{
					return true;
				}
				if (characterDataB != null && characterDataB.character == character)
				{
					return true;
				}
			}
			return false;
		}

		#endregion


		#region StaticFunctions

		private static ConversationCamera instance;

		public static ConversationCamera Instance
		{
			get
			{ 
				if (instance == null)
				{
					instance = FindObjectOfType <ConversationCamera>();
				}
				return instance;
			}
		}

		#endregion


		#region PrivateClasses

		[System.Serializable]
		private class CharacterData
		{

			public Char character { get; private set; }
			public float height { get; private set; }
			public float radius { get; private set; }


			public CharacterData (Char _character)
			{
				character = _character;

				CapsuleCollider capsuleCollder = character.GetComponent<CapsuleCollider> ();
				if (capsuleCollder) 
				{
					height = capsuleCollder.height;
					radius = capsuleCollder.radius;
				}
				else
				{
					CharacterController characterController = character.GetComponent<CharacterController> ();
					height = characterController.height;
					radius = characterController.radius;
				}
			}


			public void LookAt (CharacterData otherCharacter)
			{
				character.SetHeadTurnTarget (otherCharacter.character.transform, Vector3.one * otherCharacter.height * 0.94f, false);
			}


			public void StopLooking (bool isSkipping)
			{
				character.ClearHeadTurnTarget (isSkipping, HeadFacing.Manual);
			}

		}

		[System.Serializable]
		private class FloatRange
		{

			[SerializeField] private float minValue;
			[SerializeField] private float maxValue;

			private float minLimit;
			private float maxLimit;


			public FloatRange (float min, float max)
			{
				minValue = min;
				maxValue = max;

				minLimit = min;
				maxLimit = max;
			}


			public float GetRandomValue ()
			{
				return Random.Range (minValue, maxValue);
			}


			public void OnValidate ()
			{
				minValue = Mathf.Max (minValue, minLimit);
				maxValue = Mathf.Min (maxValue, maxLimit);
			}


			#if UNITY_EDITOR

			public void ShowGUI (string label)
			{
				EditorGUILayout.BeginHorizontal ();
				EditorGUILayout.LabelField (label, GUILayout.Width (100f));
				
				string minValueLabel = minValue.ToString ();
				if (minValueLabel.Length > 4) minValueLabel = minValueLabel.Substring (0, 4);

				string maxValueLabel = maxValue.ToString ();
				if (maxValueLabel.Length > 4) maxValueLabel = maxValueLabel.Substring (0, 4);

				EditorGUILayout.LabelField ("Min: " + minValueLabel + "s", GUILayout.Width (80f));
				EditorGUILayout.LabelField ("Max: " + maxValueLabel + "s", GUILayout.Width (80f));

				EditorGUILayout.MinMaxSlider (ref minValue, ref maxValue, minLimit, maxLimit);
				EditorGUILayout.EndHorizontal ();
			}

			#endif

		}


		private class CameraShot
		{

			private ShotType shotType;
			private CharacterData mainCharacter;
			private Vector3 cameraPosition;
			private Vector3 lookAtPosition;
			private float cameraFOV;
			private RaycastHit raycastHit;
			
			private bool isAnimated;
			private AnimationStyle animationStyle;
			private enum AnimationStyle { Pivot, PanHorizontal, PanVertical, PanIn };
			private float animationSpeed;
			private float shotTime;
			private bool isReverseShot;


			public CameraShot (ShotType _shotType, CharacterData _mainCharacter, Vector3 _cameraPosition, Vector3 _lookAtPosition, float _cameraFOV, float animatedShotFrequency, bool _isReverseShot)
			{
				shotType = _shotType;
				mainCharacter = _mainCharacter;
				cameraPosition = _cameraPosition;
				lookAtPosition = _lookAtPosition;
				cameraFOV = _cameraFOV;
				isAnimated = Random.Range (0f, 1f) > animatedShotFrequency;
				isReverseShot = _isReverseShot;
				
				if (isAnimated)
				{
					switch (shotType)
					{
						case ShotType.Centred:
							animationStyle = AnimationStyle.Pivot;
							animationSpeed = 0.1f;
							break;

						case ShotType.CloseUp:
							animationStyle = (Random.Range (0, 10) >= 5) ? AnimationStyle.PanVertical : AnimationStyle.PanIn;
							animationSpeed = (animationStyle == AnimationStyle.PanVertical) ? 0.01f : 0.04f;
							break;

						case ShotType.OverTheShoulder:
							animationStyle = AnimationStyle.PanHorizontal;
							animationSpeed = 0.04f;
							break;
					}

					if (Random.Range (0, 10) >= 5) animationSpeed *= -1f;
				}
			}


			public void Apply (_Camera camera)
			{
				camera.transform.position = cameraPosition;
				camera.transform.LookAt (lookAtPosition);
				camera.Camera.fieldOfView = cameraFOV;

				KickStarter.mainCamera.SetGameCamera (camera);
			}


			public void Apply (_Camera camera, CameraShot cameraShot)
			{
				cameraPosition = cameraShot.cameraPosition;
				lookAtPosition = cameraShot.lookAtPosition;

				camera.transform.position = cameraPosition;
				camera.transform.LookAt (lookAtPosition);
				camera.Camera.fieldOfView = cameraFOV;
			}


			public void Update (Transform transform)
			{
				if (!isAnimated)
				{
					return;
				}
				
				shotTime += Time.deltaTime;

				switch (animationStyle)
				{
					case AnimationStyle.Pivot:
						transform.position = cameraPosition + (transform.right * animationSpeed * shotTime);
						transform.LookAt (lookAtPosition);
						break;

					case AnimationStyle.PanHorizontal:
						transform.position = cameraPosition + (transform.right * animationSpeed * shotTime);
						break;

					case AnimationStyle.PanVertical:
						transform.position = cameraPosition + (transform.up * animationSpeed * shotTime);
						transform.LookAt (lookAtPosition);
						break;

					case AnimationStyle.PanIn:
						transform.position = cameraPosition + (transform.forward * animationSpeed * shotTime);
						break;
				}

			}


			public bool IsValid (CharacterData characterA, CharacterData characterB)
			{
				Vector3 headPositionA = characterA.character.transform.position + (characterA.height * 0.9f * Vector3.up);
				if (Physics.Raycast (cameraPosition, headPositionA - cameraPosition, out raycastHit, Vector3.Distance (cameraPosition, headPositionA)))
				{
					if (raycastHit.collider.gameObject != characterA.character.gameObject)
					{
						return false;
					}
				}

				if (ShowsBothCharacters ())
				{
					Vector3 headPositionB = characterB.character.transform.position + (characterB.height * 0.9f * Vector3.up);
					if (Physics.Raycast (cameraPosition, headPositionB - cameraPosition, out raycastHit, Vector3.Distance (cameraPosition, headPositionB)))
					{
						if (raycastHit.collider.gameObject != characterB.character.gameObject)
						{
							return false;
						}
					}
				}

				return true;
			}


			private bool ShowsBothCharacters ()
			{
				switch (shotType)
				{ 
					case ShotType.CloseUp:
						return false;

					default:
						return true;
				}
			}


			public CharacterData MainCharacter
			{
				get
				{
					return mainCharacter;
				}
			}


			public ShotType ShotType
			{
				get
				{
					return shotType;
				}
			}


			public bool IsReverseShot
			{
				get
				{
					return isReverseShot;
				}
			}

		}

		#endregion


		#if UNITY_EDITOR

		public void ShowGUI ()
		{
			minShotLength = EditorGUILayout.Slider ("Min. shot duration (s):", minShotLength, 0f, 10f);
			if (minShotLength > 0f) extendCutscenesToFitMinShotLength = EditorGUILayout.ToggleLeft ("Extend cutscenes to fit min shot duration?", extendCutscenesToFitMinShotLength);
			
			EditorGUILayout.Space ();

			cutDelay.ShowGUI ("Cut delay:");

			EditorGUILayout.Space ();

			moveWithCharacters = EditorGUILayout.Toggle ("Move with characters?", moveWithCharacters);
			animatedShotFrequency = EditorGUILayout.Slider ("Animated frequency:", animatedShotFrequency, 0f, 1f);
			if (animatedShotFrequency > 0f)
			{
				maxAnimationDuration = EditorGUILayout.Slider ("Max. animation duration:", maxAnimationDuration, 0f, 10f);
			}
			autoTurnHeads = EditorGUILayout.Toggle ("Auto-turn character heads?", autoTurnHeads);
		}

		#endif

	}

}