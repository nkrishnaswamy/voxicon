using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

using Global;

public class ObjectSelector : MonoBehaviour {
	public List<Voxeme> allVoxemes = new List<Voxeme> ();

	public List<GameObject> selectedObjects = new List<GameObject>();
	public List<GameObject> disabledObjects = new List<GameObject>();
	
	VoxemeInspector inspector;
	
	// Use this for initialization
	void Start () {
		inspector = gameObject.GetComponent ("VoxemeInspector") as VoxemeInspector;
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetMouseButtonDown (0)) {
			if (Helper.PointOutsideMaskedAreas (new Vector2 (Input.mousePosition.x, Screen.height - Input.mousePosition.y), 
			                                           new Rect[]{inspector.InspectorRect})) {
				Ray ray = Camera.main.ScreenPointToRay (Input.mousePosition);
				RaycastHit hit;
				// Casts the ray and get the first game object hit
				Physics.Raycast (ray, out hit);
				if (hit.collider == null) {
					if (!Helper.PointInRect (new Vector2 (Input.mousePosition.x, Screen.height - Input.mousePosition.y), inspector.InspectorRect)) {
						//inspector.InspectorObject = null;
						selectedObjects.Clear ();
					}

					if (!Helper.PointInRect (new Vector2 (Input.mousePosition.x, Screen.height - Input.mousePosition.y), inspector.InspectorRect)) {
						inspector.DrawInspector = false;
					}
				}
				else {
					selectedObjects.Clear ();
					//selectedObjects.Add (hit.transform.root.gameObject);
					//inspector.InspectorObject = hit.transform.root.gameObject;
					//Debug.Log (selectedObjects.Count);
				}
				
				if (!Helper.PointInRect (new Vector2 (Input.mousePosition.x, Screen.height - Input.mousePosition.y), inspector.InspectorRect)) {
					inspector.DrawInspector = false;
				}
			}
		}
		else
		if (Input.GetMouseButtonDown (1)) {
			if (Helper.PointOutsideMaskedAreas (new Vector2 (Input.mousePosition.x, Screen.height - Input.mousePosition.y), 
			                                           new Rect[]{inspector.InspectorRect})) {
				Ray ray = Camera.main.ScreenPointToRay (Input.mousePosition);
				RaycastHit hit;
				// Casts the ray and get the first game object hit
				Physics.Raycast (ray, out hit);
				if (hit.collider == null) {
					if (!Helper.PointInRect (new Vector2 (Input.mousePosition.x, Screen.height - Input.mousePosition.y), inspector.InspectorRect)) {
						inspector.DrawInspector = false;
					}
				}
				else {
					inspector.DrawInspector = true;
					inspector.ScrollPosition = new Vector2 (0, 0);
					inspector.InspectorChoice = -1;
					inspector.InspectorObject = Helper.GetMostImmediateParentVoxeme (hit.transform.gameObject);
					//inspector.InspectorObject = hit.transform.root.gameObject;
					inspector.InspectorPosition = new Vector2 (Input.mousePosition.x, Screen.height - Input.mousePosition.y);
				}
			}
		}
	}
}

