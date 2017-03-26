﻿using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

using Global;

public class VoxemeInit : MonoBehaviour {
	Predicates preds;
	ObjectSelector objSelector;

	// Use this for initialization
	void Start () {
		objSelector = GameObject.Find ("BlocksWorld").GetComponent<ObjectSelector> ();
		Macros macros = GameObject.Find ("BehaviorController").GetComponent<Macros> ();

		InitializeVoxemes ();

		objSelector.InitDisabledObjects ();
		macros.PopulateMacros ();
	}

	public void InitializeVoxemes() {

		preds = GameObject.Find ("BehaviorController").GetComponent<Predicates> ();

		/* MAKE GLOBAL OBJECT RUNTIME ALTERATIONS */

		// get all objects
		GameObject[] allObjects = UnityEngine.Object.FindObjectsOfType<GameObject>();
		Voxeme voxeme;

		foreach (GameObject go in allObjects) {
			if (go.activeInHierarchy) {
				// set up all objects to enable consistent manipulation
				// (i.e.) flatten any pos/rot inconsistencies in modeling or prefab setup due to human error
				voxeme = go.GetComponent<Voxeme> ();
				Rigging rigging = go.GetComponent<Rigging> ();
				if ((voxeme != null) && (voxeme.enabled) && (rigging == null)) {	// object has Voxeme component and no Rigging
					GameObject container = new GameObject (go.name, typeof(Rigging), typeof(Voxeme));

					if (go.transform.root != go.transform) { // not a top-level object
						container.transform.parent = go.transform.parent;
					}

					container.transform.position = go.transform.position;
					container.transform.rotation = go.transform.rotation;
					go.transform.parent = container.transform;
					go.name += "*";
					voxeme.enabled = false;
					//container.GetComponent<Entity> ().enabled = false;

					// copy attribute set
					AttributeSet newAttrSet = container.AddComponent<AttributeSet> ();
					AttributeSet attrSet = go.GetComponent<AttributeSet> ();
					if (attrSet != null) {
						foreach (string s in attrSet.attributes) {
							newAttrSet.attributes.Add (s);
						}
					}

					// set up for physics
					// add box colliders and rigid bodies to all subobjects that have MeshFilters
					Renderer[] renderers = go.GetComponentsInChildren<Renderer> ();
					foreach (Renderer renderer in renderers) {
						GameObject subObj = renderer.gameObject;
						if (subObj.GetComponent<MeshFilter> () != null) {
							if (go.tag != "UnPhysic") {
								if (subObj.GetComponent<BoxCollider> () == null) {	// may already have one -- goddamn overachieving scene artists
									BoxCollider collider = subObj.AddComponent<BoxCollider> ();
									//Physics.IgnoreCollision (collider, GameObject.Find ("MainCamera").GetComponent<Collider> ());
								}
							}

							if ((go.tag != "UnPhysic") && (go.tag != "Ground")) {	// Non-physics objects are either scene markers or, like the ground, cognitively immobile
								Debug.Log (subObj.name);
								if (subObj.GetComponent<Rigidbody> () == null) {	// may already have one -- goddamn overachieving scene artists
									Rigidbody rigidbody = subObj.AddComponent<Rigidbody> ();
									// assume mass is a volume of uniform density
									// assumption: all objects have the same density
									float x = Helper.GetObjectWorldSize (subObj).size.x;
									float y = Helper.GetObjectWorldSize (subObj).size.y;
									float z = Helper.GetObjectWorldSize (subObj).size.z;
									rigidbody.mass = x * y * z;

									// bunch of crap assumptions to calculate drag:
									// air density: 1.225 kg/m^3
									// flow velocity = parent voxeme moveSpeed
									// use box collider surface area for reference area
									// use Reynolds number for drag coefficient - assume 1
									// https://en.wikipedia.org/wiki/Drag_coefficient
									rigidbody.drag = 1.225f * voxeme.moveSpeed * ((2 * x * y) + (2 * y * z) + (2 * x * z)) * 1.0f;
									//rigidbody.drag = 0f;
									//rigidbody.angularDrag = 0f;

									//rigidbody.interpolation = RigidbodyInterpolation.Interpolate;

									// get subobject initial rotations
									//Debug.Log(rigidbody.name);
									//Debug.Log(rigidbody.rotation.eulerAngles);
									//container.GetComponent<Voxeme> ().startEventRotations.Add(rigidbody.name,rigidbody.rotation.eulerAngles);

									// log the orientational displacement of each rigidbody relative to the main body
									// relativeDisplacement = rotation to get from main body rotation to rigidbody rotation
									// = rigidbody rotation * (main body rotation)^-1
									Vector3 displacement = rigidbody.transform.localPosition;//-container.transform.position;
									Vector3 rotationalDisplacement = rigidbody.transform.localEulerAngles;//(rigidbody.transform.localRotation * Quaternion.Inverse (container.transform.rotation)).eulerAngles;
									//Debug.Log(rotationalDisplacement);
									//Debug.Log(rigidbody.name);
									container.GetComponent<Voxeme> ().displacement.Add (rigidbody.gameObject, displacement);
									container.GetComponent<Voxeme> ().rotationalDisplacement.Add (rigidbody.gameObject, rotationalDisplacement);
								}
							}
						}
					}
					// add to master voxeme list
					objSelector.allVoxemes.Add (container.GetComponent<Voxeme> ());
					Debug.Log (Helper.VectorToParsable (container.transform.position - Helper.GetObjectWorldSize (container).center));
				}
			}
		}

		// set joint links between all subobjects (Cartesian product)
		foreach (GameObject go in allObjects) {
			if (go.activeInHierarchy) {
				// remove BoxCollider and Rigidbody on non-top level objects
				if ((Helper.GetMostImmediateParentVoxeme (go).gameObject.transform.parent != null) && 
					(go.transform.root.tag != "Agent")){
					BoxCollider boxCollider = go.GetComponent<BoxCollider> ();
					if (boxCollider != null) {
						Destroy (boxCollider);
					}

					Rigidbody rigidbody = go.GetComponent<Rigidbody> ();
					if (rigidbody != null) {
						Destroy (rigidbody);
					}
				}

				Renderer[] renderers = go.GetComponentsInChildren<Renderer> ();
				foreach (Renderer r1 in renderers) {
					GameObject sub1 = r1.gameObject;
					foreach (Renderer r2 in renderers) {
						GameObject sub2 = r2.gameObject;
						if (sub1 != sub2) {	// can't connect body to itself
							// add connections between all bodies EXCEPT:
							//  if the connectedBody is on a GameObject that has a Voxeme component AND IS NOT the top-level voxeme
							Rigidbody connectedBody = sub2.GetComponent<Rigidbody> ();

							if (connectedBody != null) {
								if ((Helper.GetMostImmediateParentVoxeme (sub1).gameObject.transform.parent == null) &&
								   (Helper.GetMostImmediateParentVoxeme (connectedBody.gameObject).gameObject.transform.parent == null)) {
									FixedJoint fixedJoint = sub1.AddComponent<FixedJoint> ();
									fixedJoint.connectedBody = connectedBody;
								}
							}
						}
					}
				}
			}
		}
	}
	
	// Update is called once per frame
	void Update () {
	}
}
