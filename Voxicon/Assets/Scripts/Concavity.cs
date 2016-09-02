using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using Global;
using RCC;

public static class Concavity {

	public static bool IsEnabled(GameObject obj) {
		bool enabled = true;

		Ray ray = new Ray (obj.transform.position, Vector3.up);		// => get concavity vector from VoxML structure
		RaycastHit hitInfo;
		bool hit = Physics.Raycast (ray, out hitInfo);
		if (hit) {
			GameObject hitObj = hitInfo.collider.gameObject;	// if there's an object in the direction of the concavity's opening
			//Debug.Log ("Ray collide: " + hitObj);
			while (hitObj.GetComponent<Rigging> () == null) {	// get first parent to have rigging component (= voxeme root)
				if (hitObj.transform.parent != null) {
					hitObj = hitObj.transform.parent.gameObject;
				}
				else {
					hitObj = null;
					break;
				}
			}

			if (hitObj != null) {
				//Debug.Log ("Ray collide: " + hitObj);
				Bounds objBounds;
				Bounds hitObjBounds = Helper.GetObjectWorldSize (hitObj);
				if (hitObj.transform.IsChildOf (obj.transform)) {
					Debug.Log (hitObj.name + " is child of " + obj.name);
					Transform[] children = hitObj.GetComponentsInChildren<Transform> ();
					List<GameObject> toExclude = new List<GameObject> ();
					foreach (Transform transform in children) {
						toExclude.Add (transform.gameObject);
					}
					objBounds = Helper.GetObjectWorldSize (obj, toExclude);
				}
				else {
					objBounds = Helper.GetObjectWorldSize (obj);
				}

				if (RCC8.EC (hitObjBounds, objBounds)) {
					enabled = false;
				}
			}
		}

		return enabled;
	}
}
