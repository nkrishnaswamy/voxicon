﻿using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

using Global;

public class ModuleVoxemeCreation : ModalWindow {

	public int fontSize = 12;

	GUIStyle buttonStyle = new GUIStyle ("Button");

	float fontSizeModifier;	
	public float FontSizeModifier {
		get { return fontSizeModifier; }
		set { fontSizeModifier = value; }
	}

	string[] listItems;

	List<string> objects = new List<string>();
	public List<string> Objects {
		get { return objects; }
		set {
			objects = value;
			listItems = objects.ToArray ();
		}
	}

	enum PlacementState {
		Add,
		Place,
		Delete
	};

	PlacementState placementState;

	enum ShaderType {
		Default,
		Highlight
	};
		
	UnityEngine.Object[] prefabs;

	int selected = -1;
	GameObject selectedObject;

	string actionButtonText;

	public GameObject sandboxSurface;

	Dictionary<Renderer,Shader> defaultShaders;
	Shader highlightShader;

	ObjectSelector objSelector;
	VoxemeInit voxemeInit;
	Predicates preds;

	GhostFreeRoamCamera cameraControl;

	RaycastHit selectRayhit;
	float surfacePlacementOffset;

	// Use this for initialization
	void Start () {
		actionButtonText = "Add";
		windowTitle = "Add Voxeme Object";
		persistent = true;

		buttonStyle = new GUIStyle ("Button");

		fontSizeModifier = (int)(fontSize / defaultFontSize);
		buttonStyle.fontSize = fontSize;

		objSelector = GameObject.Find ("BlocksWorld").GetComponent<ObjectSelector> ();
		voxemeInit = GameObject.Find ("BlocksWorld").GetComponent<VoxemeInit> ();
		preds = GameObject.Find ("BehaviorController").GetComponent<Predicates> ();
		windowManager = GameObject.Find ("BlocksWorld").GetComponent<ModalWindowManager> ();

		cameraControl = Camera.main.GetComponent<GhostFreeRoamCamera> ();

		prefabs = Resources.LoadAll ("DemoObjects");
		foreach (UnityEngine.Object prefab in prefabs) {
			Objects.Add (prefab.name);
		}

		defaultShaders = new Dictionary<Renderer, Shader>();
		highlightShader = Shader.Find ("Legacy Shaders/Self-Illumin/Parallax Diffuse");

		listItems = Objects.ToArray ();

		windowRect = new Rect (Screen.width - 215, Screen.height - (35 + (int)(20 * fontSizeModifier)) - 205, 200, 200);

		windowManager.NewModalWindow += NewInspector;

		base.Start ();
	}

	// Update is called once per frame
	void Update () {
		if (sandboxSurface != Helper.GetMostImmediateParentVoxeme (sandboxSurface)) {
			sandboxSurface = Helper.GetMostImmediateParentVoxeme (sandboxSurface);
		}

		if (placementState == PlacementState.Delete) {
			if (Input.GetMouseButtonDown (0)) {
				Ray ray = Camera.main.ScreenPointToRay (Input.mousePosition);
				// Casts the ray and get the first game object hit
				Physics.Raycast (ray, out selectRayhit);
				if (selectRayhit.collider != null) {
					if (selectRayhit.collider.gameObject.transform.root.gameObject != sandboxSurface) {
						selectedObject = selectRayhit.collider.gameObject.transform.root.gameObject;
						DeleteVoxeme (selectedObject);
						actionButtonText = "Add";
						placementState = PlacementState.Add;
						selected = -1;
						cameraControl.allowRotation = true;
					}
				}
			}
		}
		else if (placementState == PlacementState.Place) {
			if (Input.GetMouseButton (0)) {
				Ray ray = Camera.main.ScreenPointToRay (Input.mousePosition);
				// Casts the ray and get the first game object hit
				Physics.Raycast (ray, out selectRayhit);
				if (selectRayhit.collider != null) {
					if (selectRayhit.collider.gameObject.transform.root.gameObject == sandboxSurface) {
						Debug.Log (selectRayhit.point.y);
						if (Mathf.Abs (selectRayhit.point.y - preds.ON (new object[] { sandboxSurface }).y) <= Constants.EPSILON) {
							if ((Mathf.Abs (selectRayhit.point.x - Helper.GetObjectWorldSize (sandboxSurface).min.x) >= Helper.GetObjectWorldSize (selectedObject).extents.x) &&
							    (Mathf.Abs (selectRayhit.point.x - Helper.GetObjectWorldSize (sandboxSurface).max.x) >= Helper.GetObjectWorldSize (selectedObject).extents.x) &&
							    (Mathf.Abs (selectRayhit.point.z - Helper.GetObjectWorldSize (sandboxSurface).min.z) >= Helper.GetObjectWorldSize (selectedObject).extents.z) &&
							    (Mathf.Abs (selectRayhit.point.z - Helper.GetObjectWorldSize (sandboxSurface).max.z) >= Helper.GetObjectWorldSize (selectedObject).extents.z)) {
								selectedObject.transform.position = new Vector3 (selectRayhit.point.x,
									preds.ON (new object[] { sandboxSurface }).y + surfacePlacementOffset, selectRayhit.point.z);
								Voxeme voxComponent = selectedObject.GetComponent<Voxeme> ();
								voxComponent.targetPosition = selectedObject.transform.position;

								foreach (Voxeme child in voxComponent.children) {
									if (child.isActiveAndEnabled) {
										if (child.gameObject != voxComponent.gameObject) {
											child.transform.localPosition = voxComponent.parentToChildPositionOffset [child.gameObject];
											child.targetPosition = child.transform.position;
										}
									}
								}
							}
						}
					}
				}
			}

			if (Input.GetKeyDown (KeyCode.Return)) {
				actionButtonText = "Add";
				placementState = PlacementState.Add;
				selected = -1;
				cameraControl.allowRotation = true;
				selectedObject.GetComponent<Rigging> ().ActivatePhysics (true);
				SetShader (selectedObject, ShaderType.Default);
			}
		}
		else if (placementState == PlacementState.Add) {
			if (Input.GetMouseButton (0)) {
				if (Input.GetKey (KeyCode.LeftShift) || Input.GetKey (KeyCode.RightShift)) {
					Ray ray = Camera.main.ScreenPointToRay (Input.mousePosition);
					// Casts the ray and get the first game object hit
					Physics.Raycast (ray, out selectRayhit);
					if (selectRayhit.collider != null) {
						if (selectRayhit.collider.gameObject.transform.root.gameObject != sandboxSurface) {
							if (selectRayhit.collider.gameObject.transform.root.gameObject.GetComponent<Voxeme> () != null) {
								selectedObject = selectRayhit.collider.gameObject.transform.root.gameObject;
								SetShader (selectedObject, ShaderType.Highlight);
								actionButtonText = "Place";
								placementState = PlacementState.Place;
								cameraControl.allowRotation = false;

								if (selectedObject != null) {
									selectedObject.GetComponent<Rigging> ().ActivatePhysics (false);
								}
							}
						}
					}
				}
			}
		}
	}	

	protected override void OnGUI () {
		if (placementState == PlacementState.Place) {
			if (Input.GetKey (KeyCode.LeftShift) || Input.GetKey (KeyCode.RightShift)) {
				placementState = PlacementState.Delete;
				actionButtonText = "Delete";
			}
		}

		if (placementState == PlacementState.Delete) {
			if (!Input.GetKey (KeyCode.LeftShift) && !Input.GetKey (KeyCode.RightShift)) {
				placementState = PlacementState.Place;
				actionButtonText = "Place";
			}
		}

		if (GUI.Button (new Rect (Screen.width - (15 + (int)(110 * fontSizeModifier / 3)) + 38 * fontSizeModifier - (GUI.skin.label.CalcSize (new GUIContent (actionButtonText)).x + 10),
			    Screen.height - (35 + (int)(20 * fontSizeModifier)), GUI.skin.label.CalcSize (new GUIContent (actionButtonText)).x + 10, 20 * fontSizeModifier),
			    actionButtonText, buttonStyle)) {
			switch (actionButtonText) {
			case "Add":
				render = true;
				break;
		
			case "Place":
				actionButtonText = "Add";
				placementState = PlacementState.Add;
				selected = -1;
				cameraControl.allowRotation = true;
				selectedObject.GetComponent<Rigging> ().ActivatePhysics (true);
				SetShader (selectedObject, ShaderType.Default);
				break;

			case "Delete":
				DeleteVoxeme (selectedObject);
				actionButtonText = "Add";
				placementState = PlacementState.Add;
				selected = -1;
				cameraControl.allowRotation = true;
				break;

			default:
				break;
			}
		}

		base.OnGUI ();
	}

	public override void DoModalWindow(int windowID){
		if (placementState != PlacementState.Add) {
			return;
		}

		base.DoModalWindow (windowID);

		//makes GUI window scrollable
		scrollPosition = GUILayout.BeginScrollView (scrollPosition);
		selected = GUILayout.SelectionGrid(selected, listItems, 1, buttonStyle, GUILayout.ExpandWidth(true));
		GUILayout.EndScrollView ();

		if (selected != -1) {
			render = false;

			GameObject go = (GameObject)GameObject.Instantiate (prefabs[selected]);
			go.transform.position = Vector3.zero;
			go.SetActive (true);
			go.name = go.name.Replace ("(Clone)", "");

			int exisitingObjCount = objSelector.allVoxemes.FindAll (v => v.gameObject.name.StartsWith(go.name)).Count;
			Debug.Log (exisitingObjCount);
			if (exisitingObjCount > 0) {
				go.name = go.name + (exisitingObjCount + 1).ToString ();
			}

			// store shaders
			foreach (Renderer renderer in go.GetComponentsInChildren<Renderer> ()) {
				defaultShaders [renderer] = renderer.material.shader;
			}
		
			voxemeInit.InitializeVoxemes ();

//			Debug.Log (go);
//			foreach (Voxeme vox in objSelector.allVoxemes) {
//				Debug.Log (vox.gameObject);
//			}
			selectedObject = objSelector.allVoxemes.Find (v => v.gameObject.transform.FindChild(go.name) != null).gameObject;
			surfacePlacementOffset = (Helper.GetObjectWorldSize (selectedObject.gameObject).center.y - Helper.GetObjectWorldSize (selectedObject.gameObject).min.y) +
			(selectedObject.gameObject.transform.position.y - Helper.GetObjectWorldSize (selectedObject.gameObject).center.y);
			selectedObject.transform.position = new Vector3 (sandboxSurface.transform.position.x,
					preds.ON(new object[] { sandboxSurface }).y + surfacePlacementOffset,
					sandboxSurface.transform.position.z);
			SetShader (selectedObject, ShaderType.Highlight);
			actionButtonText = "Place";
			placementState = PlacementState.Place;
			cameraControl.allowRotation = false;
			selectedObject.GetComponent<Rigging> ().ActivatePhysics (false);
		}
	}

	void SetShader(GameObject obj, ShaderType shaderType) {
		switch (shaderType) {
		case ShaderType.Default:
			foreach (Renderer renderer in obj.GetComponentsInChildren<Renderer> ()) {
				renderer.material.shader = defaultShaders[renderer];
			}
			break;

		case ShaderType.Highlight:
			foreach (Renderer renderer in obj.GetComponentsInChildren<Renderer> ()) {
				renderer.material.shader = highlightShader;
			}
			break;
		}
	}

	void DeleteVoxeme(GameObject obj) {
		objSelector.allVoxemes.Remove(objSelector.allVoxemes.Find (v => v.gameObject == obj));
		Destroy (obj);
	}

	void NewInspector(object sender, EventArgs e) {
		if (placementState != PlacementState.Add) {
			Debug.Log (((ModalWindowEventArgs)e).WindowID);
			Debug.Log (sender);
			((ModalWindowManager)sender).windowManager [((ModalWindowEventArgs)e).WindowID].DestroyWindow();
		}
	}
}