using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

using Global;

/// <summary>
/// Semantics of each predicate should be explicated within the method itself
/// Could have an issue when it comes to functions for predicates of multiple valencies?
/// *Cannot have objects or subobjects named the same as any of these predicates*
/// </summary>

public class Predicates : MonoBehaviour {
	public List<Triple<String,String,String>> rdfTriples = new List<Triple<String,String,String>>();
	EventManager eventManager;

	void Start () {
		eventManager = gameObject.GetComponent<EventManager> ();
	}

	// IN: Object (single element array)
	// OUT: Location
	public Vector3 on(object[] args)
	{
		Vector3 outValue = Vector3.zero;
		if (args [0] is GameObject) {	// on an object
			GameObject obj = ((GameObject)args[0]);
			Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();
			Bounds bounds = new Bounds();

			bool isConcave = false;
			SupportingSurface supportingSurface = obj.GetComponent<SupportingSurface>();
			if (supportingSurface != null) {
				if (supportingSurface.surfaceType == SupportingSurface.SupportingSurfaceType.Concave) {
					isConcave = true;
				}
			}

			Debug.Log (isConcave);

			if (isConcave) {
				foreach (Renderer renderer in renderers) {
					if (renderer.bounds.min.y > bounds.min.y) {
						bounds = renderer.bounds;
					}
				}
			}
			else {
				foreach (Renderer renderer in renderers) {
					if (renderer.bounds.max.y > bounds.max.y) {
						bounds = renderer.bounds;
					}
				}
			}
			Debug.Log("on: " + bounds.max.y);

			//Debug.Log (bounds.ToString());
			//Debug.Log (obj.transform.position.ToString());
			if (isConcave) {
				outValue = new Vector3(bounds.center.x,bounds.min.y,bounds.center.z);
			}
			else {
				outValue = new Vector3(bounds.center.x,bounds.max.y,bounds.center.z);
			}
		}
		else if (args [0] is Vector3) {	// on a location
			outValue = (Vector3)args[0];
		}

		return outValue;
	}

	// IN: Object (single element array)
	// OUT: Location
	public Vector3 over(object[] args)
	{
		return ((GameObject)args[0]).transform.position;
	}

	// IN: Object (single element array)
	// OUT: Location
	public Vector3 under(object[] args)
	{
		return ((GameObject)args[0]).transform.position;
	}

	// IN: Object (single element array)
	// OUT: Location
	public Vector3 behind(object[] args)
	{
		Vector3 outValue = Vector3.zero;
		if (args [0] is GameObject) {	// on an object
			GameObject obj = ((GameObject)args[0]);
			Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();
			Bounds bounds = new Bounds();
			
			foreach (Renderer renderer in renderers) {
				if (renderer.bounds.max.z > bounds.max.z) {
					bounds = renderer.bounds;
				}
			}
			Debug.Log("behind: " + bounds.max.z);
			
			//Debug.Log (bounds.ToString());
			//Debug.Log (obj.transform.position.ToString());
			outValue = new Vector3(bounds.center.x,bounds.center.y,bounds.max.z);
		}
		else if (args [0] is Vector3) {	// behind a location
			outValue = (Vector3)args[0];
		}
		
		return outValue;
	}

	// IN: Object (single element array)
	// OUT: Location
	public Vector3 in_front(object[] args)
	{
		Vector3 outValue = Vector3.zero;
		if (args [0] is GameObject) {	// on an object
			GameObject obj = ((GameObject)args[0]);
			Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();
			Bounds bounds = new Bounds();
			
			foreach (Renderer renderer in renderers) {
				if (renderer.bounds.min.z < bounds.min.z) {
					bounds = renderer.bounds;
				}
			}
			Debug.Log("in_front: " + bounds.min.z);
			
			//Debug.Log (bounds.ToString());
			//Debug.Log (obj.transform.position.ToString());
			outValue = new Vector3(bounds.center.x,bounds.center.y,bounds.min.z);
		}
		else if (args [0] is Vector3) {	// in front of a location
			outValue = (Vector3)args[0];
		}
		
		return outValue;
	}

	// IN: Object (single element array)
	// OUT: Location
	public Vector3 left(object[] args)
	{
		return ((GameObject)args[0]).transform.position;
	}

	// IN: Object (single element array)
	// OUT: Location
	public Vector3 right(object[] args)
	{
		return ((GameObject)args[0]).transform.position;
	}

	// IN: Object (single element array)
	// OUT: Location
	public Vector3 center(object[] args)
	{
		return ((GameObject)args[0]).transform.position;
	}

	// IN: Object (single element array)
	// OUT: Location
	public Vector3 top(object[] args)
	{
		Vector3 outValue = Vector3.zero;
		if (args [0] is GameObject) {
			GameObject obj = ((GameObject)args[0]);
			Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();
			Bounds bounds = new Bounds();
			
			foreach (Renderer renderer in renderers) {
				if (renderer.bounds.max.y > bounds.max.y) {
					bounds = renderer.bounds;
				}
			}
			Debug.Log("top: " + bounds.max.y);
			
			//Debug.Log (bounds.ToString());
			//Debug.Log (obj.transform.position.ToString());
			outValue = new Vector3(bounds.center.x,bounds.max.y,bounds.center.z);
		}

		return outValue;
	}

	// IN: Object (single element array)
	// OUT: Location
	public Vector3 to(object[] args)
	{
		Vector3 outValue = Vector3.zero;
		if (args [0] is GameObject) {	// to an object
			GameObject obj = args [0] as GameObject;
			outValue = obj.transform.position;
		}
		else if (args [0] is Vector3) {	// to a location
			outValue = (Vector3)args[0];
		}

		return outValue;
	}

	// IN: Objects, Location
	// OUT: none
	public void put(object[] args)
	{
		Vector3 targetPosition = Vector3.zero;

		Helper.PrintRDFTriples (rdfTriples);

		if (rdfTriples [0].Item2.Contains ("_on")) {	// fix for multiple RDF triples
			if (args [0] is GameObject) {
				if (args [1] is Vector3) {
					GameObject obj = args [0] as GameObject;
					Renderer[] renderers = obj.GetComponentsInChildren<Renderer> ();
					Bounds bounds = new Bounds ();
					
					foreach (Renderer renderer in renderers) {
						if (renderer.bounds.min.y - renderer.bounds.center.y < bounds.min.y - bounds.center.y) {
							bounds = renderer.bounds;
						}
					}

					Debug.Log (Helper.VectorToParsable(bounds.center));
					Debug.Log (Helper.VectorToParsable(bounds.min));
					Debug.Log ("put_on: " + (bounds.center.y - bounds.min.y).ToString ());
					targetPosition = new Vector3 (((Vector3)args [1]).x,
					                              ((Vector3)args [1]).y + (bounds.center.y - bounds.min.y),
					                              ((Vector3)args [1]).z);
					if (args[args.Length-1] is bool) {
						if ((bool)args[args.Length-1] == true) {
							obj.GetComponent<Entity> ().targetPosition = targetPosition;
						}
					}
				}
			}
		}
		else if (rdfTriples [0].Item2.Contains ("_behind")) {	// fix for multiple RDF triples
			if (args [0] is GameObject) {
				if (args [1] is Vector3) {
					GameObject obj = args [0] as GameObject;
					Renderer[] renderers = obj.GetComponentsInChildren<Renderer> ();
					Bounds bounds = new Bounds ();
					
					foreach (Renderer renderer in renderers) {
						if (renderer.bounds.min.z - renderer.bounds.center.z < bounds.min.z - bounds.center.z) {
							bounds = renderer.bounds;
						}
					}
					
					Debug.Log ("put_behind: " + (bounds.center.z - bounds.min.z).ToString ());
					targetPosition = new Vector3 (((Vector3)args [1]).x,
					                              ((Vector3)args [1]).y,
					                              ((Vector3)args [1]).z + (bounds.center.z - bounds.min.z));
					if (args[args.Length-1] is bool) {
						if ((bool)args[args.Length-1] == true) {
							obj.GetComponent<Entity> ().targetPosition = targetPosition;
						}
					}
				}
			}
		}
		else if (rdfTriples [0].Item2.Contains ("_in_front")) {	// fix for multiple RDF triples
			if (args [0] is GameObject) {
				if (args [1] is Vector3) {
					GameObject obj = args [0] as GameObject;
					Renderer[] renderers = obj.GetComponentsInChildren<Renderer> ();
					Bounds bounds = new Bounds ();
					
					foreach (Renderer renderer in renderers) {
						if (renderer.bounds.max.z - renderer.bounds.center.z > bounds.max.z - bounds.center.z) {
							bounds = renderer.bounds;
						}
					}
					
					Debug.Log ("put_in_front: " + (bounds.center.z - bounds.max.z).ToString ());
					targetPosition = new Vector3 (((Vector3)args [1]).x,
					                              ((Vector3)args [1]).y,
					                              ((Vector3)args [1]).z + (bounds.center.z - bounds.max.z));
					if (args[args.Length-1] is bool) {
						if ((bool)args[args.Length-1] == true) {
							obj.GetComponent<Entity> ().targetPosition = targetPosition;
						}
					}
				}
			}
		}

		if (args[args.Length-1] is bool) {
			if ((bool)args[args.Length-1] == false) {
				eventManager.eventsStatus.Add ("put("+(args [0] as GameObject).name+","+Helper.VectorToParsable(targetPosition)+")", false);
			}
		}

		return;
	}

	// IN: Objects, Location
	// OUT: none
	public void move(object[] args)
	{
		Vector3 targetPosition;

		Helper.PrintRDFTriples (rdfTriples);

		if (rdfTriples [0].Item2.Contains ("_to_top")) {	// fix for multiple RDF triples
			if (args [0] is GameObject) {
				if (args [1] is Vector3) {
					GameObject obj = args [0] as GameObject;
					Renderer[] renderers = obj.GetComponentsInChildren<Renderer> ();
					Bounds bounds = new Bounds ();
					
					foreach (Renderer renderer in renderers) {
						if (renderer.bounds.min.y - renderer.bounds.center.y < bounds.min.y - bounds.center.y) {
							bounds = renderer.bounds;
						}
					}

					Debug.Log ("move_to_top: " + (bounds.center.y - bounds.min.y).ToString ());
					targetPosition = new Vector3 (((Vector3)args [1]).x,
					                              ((Vector3)args [1]).y + (bounds.center.y - bounds.min.y),
					                              ((Vector3)args [1]).z);
					obj.GetComponent<Entity> ().targetPosition = targetPosition;
				}
			}
		}
		return;
	}

	// IN: Objects, Location
	// OUT: none
	public void roll(object[] args)
	{
		Vector3 targetPosition;

		Helper.PrintRDFTriples (rdfTriples);
		
		if (rdfTriples [0].Item2.Contains ("_to_top")) {	// fix for multiple RDF triples
			if (args [0] is GameObject) {
				if (args [1] is Vector3) {
					GameObject obj = args [0] as GameObject;
					Renderer[] renderers = obj.GetComponentsInChildren<Renderer> ();
					Bounds bounds = new Bounds ();
					
					foreach (Renderer renderer in renderers) {
						if (renderer.bounds.min.y - renderer.bounds.center.y < bounds.min.y - bounds.center.y) {
							bounds = renderer.bounds;
						}
					}
					
					Debug.Log ("move_to_top: " + (bounds.center.y - bounds.min.y).ToString ());
					targetPosition = new Vector3 (((Vector3)args [1]).x,
					                              ((Vector3)args [1]).y + (bounds.center.y - bounds.min.y),
					                              ((Vector3)args [1]).z);
					obj.GetComponent<Entity> ().targetPosition = targetPosition;
				}
			}
		}
		return;
	}

	// IN: Objects
	// OUT: none
	public void flip(object[] args)
	{
		Vector3 targetRotation;

		Helper.PrintRDFTriples (rdfTriples);

		if (args [0] is GameObject) {
			GameObject obj = args [0] as GameObject;
			Vector3 rotation = obj.transform.eulerAngles;

			float targetX = (rotation.x >= 360.0f) ? rotation.x - 360.0f : rotation.x;
			float targetY = (rotation.y+180.0f >= 360.0f) ? rotation.y+180.0f - 360.0f : rotation.y+180.0f;
			float targetZ = (rotation.z+180.0f >= 360.0f) ? rotation.z+180.0f - 360.0f : rotation.z+180.0f;
			//targetX = (rotation.x < 0.0f) ? rotation.x + 360.0f : rotation.x;
			//targetY = (rotation.y+180.0f < 0.0f) ? rotation.y+180.0f + 360.0f : rotation.y+180.0f;
			//targetZ = (rotation.z+180.0f < 0.0f) ? rotation.z+180.0f + 360.0f : rotation.z+180.0f;
			targetRotation = new Vector3 (targetX,targetY,targetZ);
			obj.GetComponent<Entity> ().targetRotation = targetRotation;
		}
	}

	// IN: Objects
	// OUT: none
	public void bind(object[] args)
	{
		//Vector3 targetRotation;
		
		//Helper.PrintRDFTriples (rdfTriples);
		GameObject container = null;

		if (args [args.Length - 1] is bool) {
			if ((bool)args [args.Length - 1] == true) {
				container = new GameObject ("bind");
				//container.transform.parent = GameObject.Find("Room").transform;

				if (args.Length-1 == 0) {
					container.transform.position = Vector3.zero;
				}

				float x = 0.0f;
				float y = 0.0f;
				float z = 0.0f;

				foreach (object arg in args)
				{
					if (arg is GameObject) {
						x += (arg as GameObject).transform.position.x;
						y += (arg as GameObject).transform.position.y;
						z += (arg as GameObject).transform.position.z;
					}
				}
				//Debug.Log (x.ToString() + " " + y.ToString() + " " + z.ToString());
				container.transform.position = new Vector3(x / (float)(args.Length-1), y / (float)(args.Length-1), z / (float)(args.Length-1));
				//Debug.Log (container.transform.position.x.ToString() + " " + container.transform.position.y.ToString() + " " +
				//           container.transform.position.z.ToString());

				foreach (object arg in args) {
					if (arg is GameObject) {
						(arg as GameObject).GetComponent<Entity>().enabled = false;
						container.name = container.name + " " + (arg as GameObject).name;
						(arg as GameObject).transform.parent = container.transform;
					}
				}
			}
		}

		if (container != null) {
			container.AddComponent<Entity> ();
		}
	}

	// put on side
}
