using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using Global;
using RCC;
using Satisfaction;

/// <summary>
/// Semantics of each predicate should be explicated within the method itself
/// Could have an issue when it comes to functions for predicates of multiple valencies?
/// *Cannot have objects or subobjects named the same as any of these predicates*
/// </summary>

public class Predicates : MonoBehaviour {
	public List<Triple<String,String,String>> rdfTriples = new List<Triple<String,String,String>>();
	public bool cameraRelativeDirections = true;

	EventManager eventManager;
	AStarSearch aStarSearch;
	ObjectSelector objSelector;
	RelationTracker relationTracker;
	Macros macros;

	void Start () {
		eventManager = gameObject.GetComponent<EventManager> ();
		aStarSearch = GameObject.Find ("BlocksWorld").GetComponent<AStarSearch> ();
		objSelector = GameObject.Find ("BlocksWorld").GetComponent<ObjectSelector> ();
		relationTracker = GameObject.Find ("BehaviorController").GetComponent<RelationTracker> ();
		macros = GameObject.Find ("BehaviorController").GetComponent<Macros> ();
	}

	/// <summary>
	/// Relations
	/// </summary>

	// IN: Object (single element array)
	// OUT: Location
	public Vector3 ON(object[] args)
	{
		Vector3 outValue = Vector3.zero;
		if (args [0] is GameObject) {	// on an object
			GameObject obj = ((GameObject)args [0]);
			Bounds bounds = new Bounds ();

			// check if object is concave
			bool isConcave = false;
			Voxeme voxeme = obj.GetComponent<Voxeme> ();
			if (voxeme != null) {
				isConcave = (voxeme.voxml.Type.Concavity == "Concave");
				isConcave = (isConcave && Vector3.Dot(obj.transform.up,Vector3.up) > 0.5f);
			}
			//Debug.Log (isConcave);

			if (isConcave) {	// on concave object
				// get surface with concavity
				// which side is concavity on? - assume +Y for now
				bounds = Helper.GetObjectWorldSize (obj);

				float concavityMinY = bounds.min.y;
				foreach (Renderer renderer in obj.GetComponentsInChildren<Renderer>()) {
					Debug.Log (renderer.gameObject.name + " " + Helper.GetObjectWorldSize (renderer.gameObject).min.y);
					if (Helper.GetObjectWorldSize (renderer.gameObject).min.y > concavityMinY) {
						concavityMinY = Helper.GetObjectWorldSize (renderer.gameObject).min.y;
					}
				}

				// **check if concavity exposed
				// flip(plate), try to put object on

				outValue = new Vector3 (obj.transform.position.x,
					concavityMinY,//bounds.min.y,
					obj.transform.position.z);
			}
			else {	// on convex or flat object
				/*bounds = Helper.GetObjectWorldSize (obj);

				Debug.Log (Helper.VectorToParsable(bounds.center));
				Debug.Log (Helper.VectorToParsable(bounds.min));
				Debug.Log (Helper.VectorToParsable(bounds.max));*/

				bounds = Helper.GetObjectWorldSize (obj);

				outValue = new Vector3 (obj.transform.position.x,
					bounds.max.y,
					obj.transform.position.z);

				//GameObject mark = GameObject.CreatePrimitive(PrimitiveType.Plane);
				//mark.transform.position = outValue;
				//mark.transform.localScale = new Vector3 (.07f, .07f, .07f);
				//mark.GetComponent<MeshCollider> ().enabled = false;
			}

			/*Voxeme voxComponent = (args [0] as GameObject).GetComponent<Voxeme> ();
			if (voxComponent.isGrasped) {
				outValue = (outValue +
					(voxComponent.graspTracker.position - voxComponent.gameObject.transform.position));
			}*/

			Debug.Log ("on: " + Helper.VectorToParsable (outValue));
		}
		else if (args [0] is Vector3) {	// on a location
			outValue = (Vector3)args[0];
		}

		return outValue;
	}

	// IN: Object (single element array)
	// OUT: Location
	public Vector3 IN(object[] args)
	{
		Vector3 outValue = Vector3.zero;
		if (args [0] is GameObject) {	// on an object
			GameObject obj = ((GameObject)args [0]);
			Bounds bounds = new Bounds ();

			// check if object is concave
			bool isConcave = false;
			Voxeme voxeme = obj.GetComponent<Voxeme> ();
			if (voxeme != null) {
				isConcave = (voxeme.voxml.Type.Concavity == "Concave");
				isConcave = (isConcave && Vector3.Dot(obj.transform.up,Vector3.up) > 0.5f);
			}
			//Debug.Log (isConcave);

			if (isConcave) {	// concavity activated
				// get surface with concavity
				// which side is concavity on? - assume +Y for now
				bounds = Helper.GetObjectWorldSize (obj);

				float concavityMinY = bounds.min.y;
				foreach (Renderer renderer in obj.GetComponentsInChildren<Renderer>()) {
					Debug.Log (renderer.gameObject.name + " " + Helper.GetObjectWorldSize (renderer.gameObject).min.y);
					if (Helper.GetObjectWorldSize (renderer.gameObject).min.y > concavityMinY) {
						concavityMinY = Helper.GetObjectWorldSize (renderer.gameObject).min.y;
					}
				}
					
				outValue = new Vector3 (obj.transform.position.x,
					concavityMinY,
					obj.transform.position.z);
			}
			else {	// concavity deactivated
				outValue = new Vector3(float.NaN,float.NaN,float.NaN);
			}
			Debug.Log ("in: " + Helper.VectorToParsable (outValue));
		}
		else if (args [0] is Vector3) {	// on a location
			outValue = (Vector3)args[0];
		}

		return outValue;
	}

	// IN: Object (single element array)
	// OUT: Location
	public Vector3 OVER(object[] args)
	{
		return ((GameObject)args[0]).transform.position;
	}

	// IN: Object (single element array)
	// OUT: Location
	public Vector3 UNDER(object[] args)
	{
		Vector3 outValue = Vector3.zero;
		if (args [0] is GameObject) {	// under an object
			GameObject obj = ((GameObject)args [0]);

			Bounds bounds = new Bounds ();

			bounds = Helper.GetObjectWorldSize (obj);

			outValue = new Vector3(obj.transform.position.x,bounds.min.y,obj.transform.position.z);

			Debug.Log ("under: " + Helper.VectorToParsable (outValue));
		}
		else if (args [0] is Vector3) {	// behind a location
			outValue = (Vector3)args[0];
		}

		return outValue;
	}

	// IN: Object (single element array)
	// OUT: Location
	public Vector3 TO(object[] args)
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

	// IN: Object (single element array)
	// OUT: Location
	public Vector3 FOR(object[] args)
	{
		Vector3 outValue = Vector3.zero;
		if (args [0] is GameObject) {	// for an object
			GameObject obj = args [0] as GameObject;
			outValue = obj.transform.position;
		}
		else if (args [0] is Vector3) {	// for a location
			outValue = (Vector3)args[0];
		}

		return outValue;
	}

	// IN: Object (single element array)
	// OUT: Location
	public Vector3 BEHIND(object[] args)
	{
		Vector3 outValue = Vector3.zero;
		if (args [0] is GameObject) {	// behind an object
			GameObject obj = ((GameObject)args [0]);

			Bounds bounds = new Bounds ();

			bounds = Helper.GetObjectWorldSize (obj);

			GameObject camera = GameObject.Find ("Main Camera");
			float povDir = cameraRelativeDirections ? camera.transform.eulerAngles.y : 0.0f;
			Vector3 rayStart = new Vector3 (0.0f, 0.0f,
				Mathf.Abs(bounds.size.z));
			rayStart = Quaternion.Euler (0.0f, povDir, 0.0f) * rayStart;
			rayStart += obj.transform.position;
			outValue = Helper.RayIntersectionPoint (rayStart, obj);

			Debug.Log ("behind: " + Helper.VectorToParsable (outValue));
		}
		else if (args [0] is Vector3) {	// behind a location
			outValue = (Vector3)args[0];
		}

		return outValue;
	}

	// IN: Object (single element array)
	// OUT: Location
	public Vector3 IN_FRONT(object[] args)
	{
		Vector3 outValue = Vector3.zero;
		if (args [0] is GameObject) {	// in front of an object
			GameObject obj = ((GameObject)args [0]);

			Bounds bounds = new Bounds ();

			bounds = Helper.GetObjectWorldSize (obj);

			GameObject camera = GameObject.Find ("Main Camera");
			float povDir = cameraRelativeDirections ? camera.transform.eulerAngles.y : 0.0f;
			Vector3 rayStart = new Vector3 (0.0f, 0.0f,
				Mathf.Abs(bounds.size.z));
			rayStart = Quaternion.Euler (0.0f, povDir+180.0f, 0.0f) * rayStart;
			rayStart += obj.transform.position;
			outValue = Helper.RayIntersectionPoint (rayStart, obj);

			Debug.Log ("in_front: " + Helper.VectorToParsable (outValue));
		}
		else if (args [0] is Vector3) {	// in front of a location
			outValue = (Vector3)args[0];
		}

		return outValue;
	}

	// IN: Object (single element array)
	// OUT: Location
	public Vector3 LEFT(object[] args)
	{
		Vector3 outValue = Vector3.zero;
		if (args [0] is GameObject) {	// left of an object
			GameObject obj = ((GameObject)args [0]);

			Bounds bounds = new Bounds ();

			bounds = Helper.GetObjectWorldSize (obj);

			GameObject camera = GameObject.Find ("Main Camera");
			float povDir = cameraRelativeDirections ? camera.transform.eulerAngles.y : 0.0f;
			Vector3 rayStart = new Vector3 (0.0f, 0.0f,
				Mathf.Abs(bounds.size.z));
			rayStart = Quaternion.Euler (0.0f, povDir+270.0f, 0.0f) * rayStart;
			rayStart += obj.transform.position;
			outValue = Helper.RayIntersectionPoint (rayStart, obj);

			Debug.Log ("left: " + Helper.VectorToParsable (outValue));
		}
		else if (args [0] is Vector3) {	// left of a location
			outValue = (Vector3)args[0];
		}

		return outValue;
	}

	// IN: Object (single element array)
	// OUT: Location
	public Vector3 RIGHT(object[] args)
	{
		Vector3 outValue = Vector3.zero;
		if (args [0] is GameObject) {	// right of an object
			GameObject obj = ((GameObject)args [0]);

			Bounds bounds = new Bounds ();

			bounds = Helper.GetObjectWorldSize (obj);

			GameObject camera = GameObject.Find ("Main Camera");
			float povDir = cameraRelativeDirections ? camera.transform.eulerAngles.y : 0.0f;
			Vector3 rayStart = new Vector3 (0.0f, 0.0f,
				Mathf.Abs(bounds.size.z));
			rayStart = Quaternion.Euler (0.0f, povDir+90.0f, 0.0f) * rayStart;
			rayStart += obj.transform.position;
			outValue = Helper.RayIntersectionPoint (rayStart, obj);

			Debug.Log ("left: " + Helper.VectorToParsable (outValue));
		}
		else if (args [0] is Vector3) {	// right of a location
			outValue = (Vector3)args[0];
		}

		return outValue;
	}

	/// <summary>
	/// Functions
	/// </summary>

	// IN: Object (single element array)
	// OUT: Location
	public Vector3 CENTER(object[] args)
	{	// identical to TOP for now
		Vector3 outValue = Vector3.zero;
		if (args [0] is GameObject) {
			GameObject obj = ((GameObject)args[0]);
			Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();
			Bounds bounds = Helper.GetObjectWorldSize(obj);

			Debug.Log("center: " + bounds.max.y);

			//Debug.Log (bounds.ToString());
			//Debug.Log (obj.transform.position.ToString());
			outValue = new Vector3(bounds.center.x,bounds.max.y,bounds.center.z);
		}

		return outValue;
	}

	// IN: Object (single element array)
	// OUT: Location
	public Vector3 TOP(object[] args)
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
	// OUT: String
	public String AS(object[] args)
	{
		return args[0].ToString();
	}

	/// <summary>
	/// Attributes
	/// </summary>

	// IN: String
	// OUT: String
	public String BROWN(object[] args) {
		String objName = "";

		if (args [0] is GameObject) {	// assume all inputs are of same type
			List<GameObject> objs = args.Cast<GameObject>().ToList();
			List<GameObject> attrObjs = objs.FindAll (o => o.GetComponent<AttributeSet> ().attributes.Contains ("brown"));

			if (attrObjs.Count > 0) {
				objName = attrObjs [0].name;
			}
		}

		return objName;
	}

	public String BLUE(object[] args) {
		String objName = "";

		if (args [0] is GameObject) {	// assume all inputs are of same type
			List<GameObject> objs = args.Cast<GameObject>().ToList();
			List<GameObject> attrObjs = objs.FindAll (o => o.GetComponent<AttributeSet> ().attributes.Contains ("blue"));

			if (attrObjs.Count > 0) {
				objName = attrObjs [0].name;
			}
		}

		return objName;
	}

	public String BLACK(object[] args) {
		String objName = "";

		if (args [0] is GameObject) {	// assume all inputs are of same type
			List<GameObject> objs = args.Cast<GameObject>().ToList();
			List<GameObject> attrObjs = objs.FindAll (o => o.GetComponent<AttributeSet> ().attributes.Contains ("black"));

			if (attrObjs.Count > 0) {
				objName = attrObjs [0].name;
			}
		}

		return objName;
	}

	public String GREEN(object[] args) {
		String objName = "";

		if (args [0] is GameObject) {	// assume all inputs are of same type
			List<GameObject> objs = args.Cast<GameObject>().ToList();
			List<GameObject> attrObjs = objs.FindAll (o => o.GetComponent<AttributeSet> ().attributes.Contains ("green"));

			if (attrObjs.Count > 0) {
				objName = attrObjs [0].name;
			}
		}

		return objName;
	}

	public String YELLOW(object[] args) {
		String objName = "";

		if (args [0] is GameObject) {	// assume all inputs are of same type
			List<GameObject> objs = args.Cast<GameObject>().ToList();
			List<GameObject> attrObjs = objs.FindAll (o => o.GetComponent<AttributeSet> ().attributes.Contains ("yellow"));

			if (attrObjs.Count > 0) {
				objName = attrObjs [0].name;
			}
		}

		return objName;
	}

	public String RED(object[] args) {
		String objName = "";

		if (args [0] is GameObject) {	// assume all inputs are of same type
			List<GameObject> objs = args.Cast<GameObject>().ToList();
			List<GameObject> attrObjs = objs.FindAll (o => o.GetComponent<AttributeSet> ().attributes.Contains ("red"));

			if (attrObjs.Count > 0) {
				objName = attrObjs [0].name;
			}
		}
	
		return objName;
	}

	// IN: Objects
	// OUT: String
	public String A(object[] args)
	{
		String objName = "";
		System.Random random = new System.Random ();

		if (args [0] is GameObject) {	// assume all inputs are of same type
			int index = random.Next(args.Length);
			objName = (args [index] as GameObject).name;
		}

		return objName;
	}

	// IN: Objects
	// OUT: String
	public List<String> TWO(object[] args)
	{
		List<String> objNames = new List<String>();
		System.Random random = new System.Random ();

		if (args [0] is GameObject) {	// assume all inputs are of same type
			if (args.Length > 2) {
				while (objNames.Count < 2) {
					int index = random.Next (args.Length);
					if (!objNames.Contains ((args [index] as GameObject).name)) {	// make sure all entries are distinct
						objNames.Add ((args [index] as GameObject).name);
					}
				}
			}
		}

		return objNames;
	}

	public String LEFTMOST(object[] args) {
		String objName = "";

		if (args [0] is GameObject) {	// assume all inputs are of same type
			List<GameObject> objs = args.Cast<GameObject>().ToList().OrderBy(o => o.transform.position.x).ToList();

			objName = objs [0].name;
		}

		return objName;
	}

	public String MIDDLE(object[] args) {
		String objName = "";

		if (args [0] is GameObject) {	// assume all inputs are of same type
			List<GameObject> objs = args.Cast<GameObject>().ToList().OrderBy(o => o.transform.position.x).ToList();

			objName = objs [(int)(objs.Count/2)].name;
		}

		return objName;
	}

	public String RIGHTMOST(object[] args) {
		String objName = "";

		if (args [0] is GameObject) {	// assume all inputs are of same type
			List<GameObject> objs = args.Cast<GameObject>().ToList().OrderBy(o => o.transform.position.x).ToList();

			objName = objs [objs.Count-1].name;
		}

		return objName;
	}

	// IN: Objects
	// OUT: String
	public String SELECTED(object[] args)
	{
		String objName = "";

		List<Voxeme> attrObjs = objSelector.allVoxemes.FindAll (v => v.gameObject.GetComponent<AttributeSet> ().attributes.Contains ("selected"));
		if (attrObjs.Count > 0) {
			objName = attrObjs [0].gameObject.name;
		}

		return objName;
	}

	/// <summary>
	/// Programs
	/// </summary>

	// IN: Objects, Location
	// OUT: none
	public void PUT(object[] args)
	{
		string prep = rdfTriples.Count > 0 ? rdfTriples [0].Item2.Replace ("put", "") : "";
		Debug.Log (prep);

		// look for agent
		GameObject agent = GameObject.FindGameObjectWithTag("Agent");

		// add agent-dependent preconditions
		if (agent != null) {
			// add preconditions
			if (!SatisfactionTest.IsSatisfied (string.Format ("reach({0})", (args [0] as GameObject).name))) {
				eventManager.InsertEvent (string.Format ("reach({0})", (args [0] as GameObject).name), 0);
				eventManager.InsertEvent (string.Format ("grasp({0})", (args [0] as GameObject).name), 1);
				eventManager.InsertEvent (eventManager.evalOrig [string.Format ("put({0},{1})", (args [0] as GameObject).name, Helper.VectorToParsable ((Vector3)args [1]))], 2);
				eventManager.RemoveEvent (3);
				return;
			}
			else {
				if (!SatisfactionTest.IsSatisfied (string.Format ("grasp({0})", (args [0] as GameObject).name))) {
					eventManager.InsertEvent (string.Format ("grasp({0})", (args [0] as GameObject).name), 0);
					eventManager.InsertEvent (eventManager.evalOrig [string.Format ("put({0},{1})", (args [0] as GameObject).name, Helper.VectorToParsable ((Vector3)args [1]))], 1);
					eventManager.RemoveEvent (2);
					return;
				}
			}
		}

		// add agent-independent preconditions
		//if (args [args.Length - 1] is bool) {
		//	if ((bool)args [args.Length - 1] == false) {
				if (prep == "_under") {
					if (args [0] is GameObject) {
						if ((args [0] as GameObject).GetComponent<Voxeme> () != null) {
							GameObject supportingSurface = (args [0] as GameObject).GetComponent<Voxeme> ().supportingSurface;
							if (supportingSurface != null) {
								//Debug.Log (rdfTriples [0].Item3);
								Bounds destBounds = Helper.GetObjectWorldSize (GameObject.Find (rdfTriples [0].Item3));
								destBounds.SetMinMax (destBounds.min + new Vector3 (Constants.EPSILON, 0.0f, Constants.EPSILON),
								destBounds.max - new Vector3 (Constants.EPSILON, Constants.EPSILON, Constants.EPSILON));
								//Debug.Log (Helper.VectorToParsable (bounds.min));
								//Debug.Log (Helper.VectorToParsable ((Vector3)args [1]));
								Bounds themeBounds = Helper.GetObjectWorldSize ((args [0] as GameObject));
								Vector3 min = (Vector3)args [1] - new Vector3 (0.0f, themeBounds.extents.y, 0.0f);
								Vector3 max = (Vector3)args [1] + new Vector3 (0.0f, themeBounds.extents.y, 0.0f);
								if ((min.y <= destBounds.min.y + Constants.EPSILON) && (max.y > destBounds.min.y + Constants.EPSILON)) {
									if (Mathf.Abs (Helper.GetObjectWorldSize (GameObject.Find (rdfTriples [0].Item3)).min.y -	// if no space between dest obj and dest obj's supporting surface
									   	Helper.GetObjectWorldSize (supportingSurface).max.y) < Constants.EPSILON) {
										Vector3 liftPos = GameObject.Find (rdfTriples [0].Item3).transform.position;
										liftPos += new Vector3 (0.0f, Helper.GetObjectWorldSize (args [0] as GameObject).size.y * 4, 0.0f);

										eventManager.InsertEvent (string.Format ("lift({0},{1})", GameObject.Find (rdfTriples [0].Item3).name,
											Helper.VectorToParsable (liftPos)), 0);

										Vector3 adjustedPosition = ((Vector3)args [1]);
										Debug.Log (adjustedPosition.y - (themeBounds.center.y - themeBounds.min.y));
										Debug.Log (Helper.GetObjectWorldSize(supportingSurface).max.y);
										if (adjustedPosition.y - (themeBounds.center.y - themeBounds.min.y) - ((args [0] as GameObject).transform.position.y - themeBounds.center.y) < 
											Helper.GetObjectWorldSize(supportingSurface).max.y) {	// if bottom of theme obj at this position is under the supporting surface's max
											adjustedPosition = new Vector3 (adjustedPosition.x,
											adjustedPosition.y + (themeBounds.center.y - themeBounds.min.y) + ((args [0] as GameObject).transform.position.y - themeBounds.center.y),
											adjustedPosition.z);
										}

										eventManager.InsertEvent (string.Format ("put({0},{1})", (args [0] as GameObject).name, Helper.VectorToParsable (adjustedPosition)), 1);
										eventManager.RemoveEvent (eventManager.events.Count - 1);
										eventManager.InsertEvent (string.Format ("put({0},on({1}))", rdfTriples [0].Item3, (args [0] as GameObject).name), 2);
										return;
									}
								}
							}
						}
					}

					//eventManager.PrintEvents ();
				}
		//	}
		//}


		if (agent != null) {
			// add agent-dependent postconditions
			if (args [args.Length - 1] is bool) {
				if ((bool)args [args.Length - 1] == true) {
					eventManager.InsertEvent (string.Format ("ungrasp({0})", (args [0] as GameObject).name), 1);
				}
			}
		}

		// override physics rigging
		foreach (object arg in args) {
			if (arg is GameObject) {
				(arg as GameObject).GetComponent<Rigging> ().ActivatePhysics(false);
			}
		}

		Vector3 targetPosition = Vector3.zero;
		Vector3 targetRotation = Vector3.zero;

		Helper.PrintRDFTriples (rdfTriples);

		if (prep == "_on") {	// fix for multiple RDF triples
			if (args [0] is GameObject) {
				if (args [1] is Vector3) {
					GameObject theme = args [0] as GameObject;	// get theme obj ("apple" in "put apple on plate")
					GameObject dest = GameObject.Find (rdfTriples [0].Item3);	// get destination obj ("plate" in "put apple on plate")
					//Renderer[] renderers = obj.GetComponentsInChildren<Renderer> ();
					/*Bounds bounds = new Bounds ();
					
					foreach (Renderer renderer in renderers) {
						if (renderer.bounds.min.y - renderer.bounds.center.y < bounds.min.y - bounds.center.y) {
							bounds = renderer.bounds;
						}
					}*/

					Bounds themeBounds = Helper.GetObjectWorldSize (theme);	// bounds of theme obj
					Bounds destBounds = Helper.GetObjectWorldSize (dest);	// bounds of dest obj => alter to get interior enumerated by VoxML structure

					//Debug.Log (Helper.VectorToParsable(bounds.center));
					//Debug.Log (Helper.VectorToParsable(bounds.min));

					float yAdjust = (theme.transform.position.y - themeBounds.center.y);
					Debug.Log ("Y-size = " + (themeBounds.center.y - themeBounds.min.y));
					Debug.Log ("put_on: " + (theme.transform.position.y - themeBounds.min.y).ToString ());

					// compose computed on(a) into put(x,y) formula
					// if the glove don't fit, you must acquit! (recompute)
					Vector3 loc = ((Vector3)args [1]);	// computed coord of "on"
					if (dest.GetComponent<Voxeme> ().voxml.Type.Concavity == "Concave") {	// putting on a concave object
						if (!Helper.FitsIn (themeBounds, destBounds)) {
							loc = new Vector3 (dest.transform.position.x,
								destBounds.max.y,
								dest.transform.position.z);
							Debug.Log (destBounds.max.y);
						}
					}

					if (args [args.Length - 1] is bool) {
						if ((bool)args [args.Length - 1] == false) {
							targetPosition = new Vector3 (loc.x,
								loc.y + (themeBounds.center.y - themeBounds.min.y) + yAdjust,
								loc.z);

							if ((theme.GetComponent<Voxeme> ().voxml.Type.Concavity == "Concave") &&	// this is a concave object
								(Concavity.IsEnabled (theme)) && (Mathf.Abs(Vector3.Dot (theme.transform.up, Vector3.up)+1.0f) <= Constants.EPSILON)) { // TODO: Run this through habitat verification
								// check if concavity is active
								Debug.Log(string.Format("{0} upside down",theme.name));

								targetPosition = new Vector3 (loc.x,
									loc.y + (themeBounds.center.y - themeBounds.min.y) - yAdjust - (destBounds.max.y - destBounds.min.y),
									loc.z);
								//flip(cup1);put(ball,under(cup1))
							}
						} 
						else {
							targetPosition = loc;
						}

						Debug.Log (Helper.VectorToParsable (targetPosition));

						Voxeme voxComponent = theme.GetComponent<Voxeme> ();
						if (voxComponent != null) {
							if (!voxComponent.enabled) {
								voxComponent.gameObject.transform.parent = null;
								voxComponent.enabled = true;
							}

							voxComponent.targetPosition = targetPosition;

							/*if (voxComponent.isGrasped) {
								voxComponent.targetPosition = voxComponent.targetPosition +
								(voxComponent.grasperCoord.position - voxComponent.gameObject.transform.position);
							}*/
						}
					}
				}
			}
		} 
		else if (prep == "_in") {	// fix for multiple RDF triples
			if (args [0] is GameObject) {
				if (args [1] is Vector3) {
					GameObject theme = args [0] as GameObject;	// get theme obj ("apple" in "put apple in plate")
					GameObject dest = GameObject.Find (rdfTriples [0].Item3);	// get destination obj ("plate" in "put apple in plate")

					Bounds themeBounds = Helper.GetObjectWorldSize (theme);	// bounds of theme obj
					Bounds destBounds = Helper.GetObjectWorldSize (dest);	// bounds of dest obj

					//Debug.Log (Helper.VectorToParsable(bounds.center));
					//Debug.Log (Helper.VectorToParsable(bounds.min));

					float yAdjust = (theme.transform.position.y - themeBounds.center.y);
					Debug.Log ("Y-size = " + (themeBounds.center.y - themeBounds.min.y));
					Debug.Log ("put_in: " + (theme.transform.position.y - themeBounds.min.y).ToString ());

					// compose computed in(a) into put(x,y) formula
					Vector3 loc = ((Vector3)args [1]);	// coord of "in"
					if ((dest.GetComponent<Voxeme> ().voxml.Type.Concavity == "Concave") &&	// TODO: Run this through habitat verification
					    (Concavity.IsEnabled (dest)) && (Vector3.Dot (dest.transform.up, Vector3.up) > 0.5f)) {	// check if concavity is active
						if (!Helper.FitsIn (themeBounds, destBounds)) {	// if the glove don't fit, you must acquit! (rotate)
							// rotate to align longest major axis with container concavity axis
							Vector3 majorAxis = Helper.GetObjectMajorAxis (theme);
							Quaternion adjust = Quaternion.LookRotation (majorAxis);
							//Debug.Log (Helper.VectorToParsable (themeBounds.size));
							//Debug.Log (Helper.VectorToParsable (adjust * themeBounds.size));
							// create new test bounds with vector*quat
							Bounds testBounds = new Bounds (themeBounds.center, adjust * themeBounds.size);
							//if (args[args.Length-1] is bool) {
							//	if ((bool)args[args.Length-1] == true) {
							//		theme.GetComponent<Voxeme> ().targetRotation = Quaternion.LookRotation(majorAxis).eulerAngles;
							//	}
							//}
							if (Helper.FitsIn (testBounds, destBounds)) {	// check fit again
								targetRotation = Quaternion.LookRotation (majorAxis).eulerAngles;
							} else {	// if still won't fit, return garbage (NaN) rotation to signal that you can't do that
								targetRotation = new Vector3 (float.NaN, float.NaN, float.NaN);
							}
							loc = new Vector3 (dest.transform.position.x,
								destBounds.max.y,
								dest.transform.position.z);
							Debug.Log (destBounds.max.y);
						}
					} 
					else {
						targetRotation = new Vector3 (float.NaN, float.NaN, float.NaN);
					}

					if (!Helper.VectorIsNaN (targetRotation)) {
						if (args [args.Length - 1] is bool) {
							if ((bool)args [args.Length - 1] == false) {
								targetPosition = new Vector3 (loc.x,
									loc.y + (themeBounds.center.y - themeBounds.min.y) + yAdjust,
									loc.z);
							} 
							else {
								targetPosition = loc;
							}
						}
					} 
					else {
						targetPosition = new Vector3 (float.NaN, float.NaN, float.NaN);
					}

					Debug.Log (Helper.VectorToParsable (targetPosition));

					Voxeme voxComponent = theme.GetComponent<Voxeme> ();
					if (voxComponent != null) {
						if (!voxComponent.enabled) {
							voxComponent.gameObject.transform.parent = null;
							voxComponent.enabled = true;
						}

						voxComponent.targetPosition = targetPosition;
						voxComponent.targetRotation = targetRotation;

						/*if (voxComponent.isGrasped) {
							voxComponent.targetPosition = voxComponent.targetPosition +
							(voxComponent.grasperCoord.position - voxComponent.gameObject.transform.position);
						}*/
					}
				}
			}
		}
		else if (prep == "_under") {	// fix for multiple RDF triples
			if (args [0] is GameObject) {
				if (args [1] is Vector3) {

					// constraints for "under"
					// beneath dest obj (like other position relations)
					// on dest obj's supporting surface
					// no distance between dest and dest's support -> dest must be moved, displaced (precondition)
					// dest is concave -> theme can be placed in

					GameObject theme = args [0] as GameObject;	// get theme obj ("apple" in "put apple under plate")
					GameObject dest = GameObject.Find (rdfTriples [0].Item3);	// get destination obj ("plate" in "put apple on plate")
					GameObject supportingSurface = dest.GetComponent<Voxeme>().supportingSurface;

					Bounds themeBounds = Helper.GetObjectWorldSize (theme);	// bounds of theme obj
					Bounds destBounds = Helper.GetObjectWorldSize (dest);	// bounds of dest obj => alter to get interior enumerated by VoxML structure

					//Debug.Log (Helper.VectorToParsable(bounds.center));
					//Debug.Log (Helper.VectorToParsable(bounds.min));

					float yAdjust = (theme.transform.position.y - themeBounds.center.y);
					Debug.Log ("Y-size = " + (themeBounds.max.y - themeBounds.center.y));
					Debug.Log ("put_under: " + (theme.transform.position.y - themeBounds.min.y).ToString ());

					// compose computed under(a) into put(x,y) formula
					Vector3 loc = ((Vector3)args [1]);	// coord of "under"

					if (args [args.Length - 1] is bool) {
						if ((bool)args [args.Length - 1] == false) {
							targetPosition = new Vector3 (loc.x,
								loc.y - (themeBounds.max.y - themeBounds.center.y) + yAdjust,
								loc.z);
						} 
						else {
							targetPosition = loc;
						}

						Debug.Log (Helper.VectorToParsable (targetPosition));

						Voxeme voxComponent = theme.GetComponent<Voxeme> ();
						if (voxComponent != null) {
							if (!voxComponent.enabled) {
								voxComponent.gameObject.transform.parent = null;
								voxComponent.enabled = true;
							}

							voxComponent.targetPosition = targetPosition;

							/*if (voxComponent.isGrasped) {
								voxComponent.targetPosition = voxComponent.targetPosition +
								(voxComponent.grasperCoord.position - voxComponent.gameObject.transform.position);
							}*/
						}
					}
				}
			}
		}
		else if (prep == "_behind") {	// fix for multiple RDF triples
			if (args [0] is GameObject) {
				if (args [1] is Vector3) {
					GameObject theme = args [0] as GameObject;	// get theme obj ("apple" in "put apple on plate")
					GameObject dest = GameObject.Find (rdfTriples [0].Item3);	// get destination obj ("plate" in "put apple on plate")

					Bounds themeBounds = Helper.GetObjectWorldSize (theme);	// bounds of theme obj
					Bounds destBounds = Helper.GetObjectWorldSize (dest);	// bounds of dest obj => alter to get interior enumerated by VoxML structure

					GameObject mainCamera = GameObject.Find ("Main Camera");
					float povDir = cameraRelativeDirections ? mainCamera.transform.eulerAngles.y : 0.0f;

					float zAdjust = (theme.transform.position.z - themeBounds.center.z);

					Vector3 rayStart = new Vector3 (0.0f, 0.0f,
						                   Mathf.Abs (themeBounds.size.z));
					rayStart = Quaternion.Euler (0.0f, povDir + 180.0f, 0.0f) * rayStart;
					rayStart += theme.transform.position;
					Vector3 contactPoint = Helper.RayIntersectionPoint (rayStart, theme);

					Debug.Log ("Z-adjust = " + zAdjust);
					Debug.Log ("put_behind: " + Helper.VectorToParsable (contactPoint));

					Vector3 loc = ((Vector3)args [1]);	// coord of "behind"

					if (args [args.Length - 1] is bool) {
						if ((bool)args [args.Length - 1] == false) {	// compute satisfaction condition
							Vector3 dir = new Vector3 (loc.x - (contactPoint.x - theme.transform.position.x),
								              loc.y - (contactPoint.y - theme.transform.position.y),
								              loc.z - (contactPoint.z - theme.transform.position.z) + zAdjust) - loc;

							targetPosition = dir + loc;
						}
						else {
							targetPosition = loc;
						}

						Debug.Log (Helper.VectorToParsable (targetPosition));

						Voxeme voxComponent = theme.GetComponent<Voxeme> ();
						if (voxComponent != null) {
							if (!voxComponent.enabled) {
								voxComponent.gameObject.transform.parent = null;
								voxComponent.enabled = true;
							}

							voxComponent.targetPosition = targetPosition;

							/*if (voxComponent.isGrasped) {
								voxComponent.targetPosition = voxComponent.targetPosition +
								(voxComponent.grasperCoord.position - voxComponent.gameObject.transform.position);
							}*/
						}
					}
				}
			}
		} 
		else if (prep == "_in_front") {	// fix for multiple RDF triples
			if (args [0] is GameObject) {
				if (args [1] is Vector3) {
					GameObject theme = args [0] as GameObject;	// get theme obj ("apple" in "put apple on plate")
					GameObject dest = GameObject.Find (rdfTriples [0].Item3);	// get destination obj ("plate" in "put apple on plate")

					Bounds themeBounds = Helper.GetObjectWorldSize (theme);	// bounds of theme obj
					Bounds destBounds = Helper.GetObjectWorldSize (dest);	// bounds of dest obj => alter to get interior enumerated by VoxML structure

					GameObject mainCamera = GameObject.Find ("Main Camera");
					float povDir = cameraRelativeDirections ? mainCamera.transform.eulerAngles.y : 0.0f;

					float zAdjust = (theme.transform.position.z - themeBounds.center.z);

					Vector3 rayStart = new Vector3 (0.0f, 0.0f,
						                   Mathf.Abs (themeBounds.size.z));
					rayStart = Quaternion.Euler (0.0f, povDir, 0.0f) * rayStart;
					rayStart += theme.transform.position;
					Vector3 contactPoint = Helper.RayIntersectionPoint (rayStart, theme);

					Debug.Log ("Z-adjust = " + zAdjust);
					Debug.Log ("put_in_front: " + Helper.VectorToParsable (contactPoint));

					Vector3 loc = ((Vector3)args [1]);	// coord of "in front"

					if (args [args.Length - 1] is bool) {
						if ((bool)args [args.Length - 1] == false) {	// compute satisfaction condition
							Vector3 dir = new Vector3 (loc.x - (contactPoint.x - theme.transform.position.x),
								              loc.y - (contactPoint.y - theme.transform.position.y),
								              loc.z - (contactPoint.z - theme.transform.position.z) + zAdjust) - loc;

							targetPosition = dir + loc;
						}
						else {
							targetPosition = loc;
						}

						Debug.Log (Helper.VectorToParsable (targetPosition));

						Voxeme voxComponent = theme.GetComponent<Voxeme> ();
						if (voxComponent != null) {
							if (!voxComponent.enabled) {
								voxComponent.gameObject.transform.parent = null;
								voxComponent.enabled = true;
							}

							voxComponent.targetPosition = targetPosition;

							/*if (voxComponent.isGrasped) {
								voxComponent.targetPosition = voxComponent.targetPosition +
								(voxComponent.grasperCoord.position - voxComponent.gameObject.transform.position);
							}*/
						}
					}
				}
			}
		} 
		else if (prep == "_left") {	// fix for multiple RDF triples
			if (args [0] is GameObject) {
				if (args [1] is Vector3) {
					GameObject theme = args [0] as GameObject;	// get theme obj ("apple" in "put apple on plate")
					GameObject dest = GameObject.Find (rdfTriples [0].Item3);	// get destination obj ("plate" in "put apple on plate")

					Bounds themeBounds = Helper.GetObjectWorldSize (theme);	// bounds of theme obj
					Bounds destBounds = Helper.GetObjectWorldSize (dest);	// bounds of dest obj => alter to get interior enumerated by VoxML structure

					GameObject mainCamera = GameObject.Find ("Main Camera");
					float povDir = cameraRelativeDirections ? mainCamera.transform.eulerAngles.y : 0.0f;

					float xAdjust = (theme.transform.position.x - themeBounds.center.x);

					Vector3 rayStart = new Vector3 (0.0f, 0.0f,
						                   Mathf.Abs (themeBounds.size.z));
					rayStart = Quaternion.Euler (0.0f, povDir + 90.0f, 0.0f) * rayStart;
					rayStart += theme.transform.position;
					Vector3 contactPoint = Helper.RayIntersectionPoint (rayStart, theme);

					Debug.Log ("X-adjust = " + xAdjust);
					Debug.Log ("put_left: " + Helper.VectorToParsable (contactPoint));

					Vector3 loc = ((Vector3)args [1]);	// coord of "left"

					if (args [args.Length - 1] is bool) {
						if ((bool)args [args.Length - 1] == false) {	// compute satisfaction condition
							Vector3 dir = new Vector3 (loc.x - (contactPoint.x - theme.transform.position.x) + xAdjust,
								              loc.y - (contactPoint.y - theme.transform.position.y),
								              loc.z - (contactPoint.z - theme.transform.position.z)) - loc;

							targetPosition = dir + loc;
						} 
						else {
							targetPosition = loc;
						}

						Debug.Log (Helper.VectorToParsable (targetPosition));

						Voxeme voxComponent = theme.GetComponent<Voxeme> ();
						if (voxComponent != null) {
							if (!voxComponent.enabled) {
								voxComponent.gameObject.transform.parent = null;
								voxComponent.enabled = true;
							}

							voxComponent.targetPosition = targetPosition;

							/*if (voxComponent.isGrasped) {
								voxComponent.targetPosition = voxComponent.targetPosition +
								(voxComponent.grasperCoord.position - voxComponent.gameObject.transform.position);
							}*/
						}
					}
				}
			}
		} 
		else if (prep == "_right") {	// fix for multiple RDF triples
			if (args [0] is GameObject) {
				if (args [1] is Vector3) {
					GameObject theme = args [0] as GameObject;	// get theme obj ("apple" in "put apple on plate")
					GameObject dest = GameObject.Find (rdfTriples [0].Item3);	// get destination obj ("plate" in "put apple on plate")

					Bounds themeBounds = Helper.GetObjectWorldSize (theme);	// bounds of theme obj
					Bounds destBounds = Helper.GetObjectWorldSize (dest);	// bounds of dest obj => alter to get interior enumerated by VoxML structure

					GameObject mainCamera = GameObject.Find ("Main Camera");
					float povDir = cameraRelativeDirections ? mainCamera.transform.eulerAngles.y : 0.0f;

					float xAdjust = (theme.transform.position.x - themeBounds.center.x);

					Vector3 rayStart = new Vector3 (0.0f, 0.0f,
						                   Mathf.Abs (themeBounds.size.z));
					rayStart = Quaternion.Euler (0.0f, povDir + 270.0f, 0.0f) * rayStart;
					rayStart += theme.transform.position;
					Vector3 contactPoint = Helper.RayIntersectionPoint (rayStart, theme);

					Debug.Log ("X-adjust = " + xAdjust);
					Debug.Log ("put_right: " + Helper.VectorToParsable (contactPoint));

					Vector3 loc = ((Vector3)args [1]);	// coord of "left"

					if (args [args.Length - 1] is bool) {
						if ((bool)args [args.Length - 1] == false) {
							Vector3 dir = new Vector3 (loc.x - (contactPoint.x - theme.transform.position.x) + xAdjust,
								              loc.y - (contactPoint.y - theme.transform.position.y),
								              loc.z - (contactPoint.z - theme.transform.position.z)) - loc;

							targetPosition = dir + loc;
						} 
						else {
							targetPosition = loc;
						}
						Debug.Log (Helper.VectorToParsable (targetPosition));

						Voxeme voxComponent = theme.GetComponent<Voxeme> ();
						if (voxComponent != null) {
							if (!voxComponent.enabled) {
								voxComponent.gameObject.transform.parent = null;
								voxComponent.enabled = true;
							}

							voxComponent.targetPosition = targetPosition;

							/*if (voxComponent.isGrasped) {
								voxComponent.targetPosition = voxComponent.targetPosition +
								(voxComponent.grasperCoord.position - voxComponent.gameObject.transform.position);
							}*/
						}
					}
				}
			}
		}
		else {
			if (args [0] is GameObject) {
				if (args [1] is Vector3) {
					GameObject theme = args [0] as GameObject;	// get theme obj ("apple" in "put apple on plate")

					Vector3 loc = ((Vector3)args [1]);	// coord

					targetPosition = loc;

					Debug.Log (Helper.VectorToParsable (targetPosition));

					Voxeme voxComponent = theme.GetComponent<Voxeme> ();
					if (voxComponent != null) {
						if (!voxComponent.enabled) {
							voxComponent.gameObject.transform.parent = null;
							voxComponent.enabled = true;
						}

						voxComponent.targetPosition = targetPosition;

						if (voxComponent.isGrasped) {
							voxComponent.targetPosition = voxComponent.targetPosition +
								(voxComponent.grasperCoord.position - voxComponent.gameObject.transform.position);
						}
					}
				}
			}
		}

		// add to events manager
		if (args[args.Length-1] is bool) {
			if ((bool)args[args.Length-1] == false) {
				//eventManager.eventsStatus.Add ("put("+(args [0] as GameObject).name+","+Helper.VectorToParsable(targetPosition)+")", false);
				eventManager.events[0] = "put("+(args [0] as GameObject).name+","+Helper.VectorToParsable(targetPosition)+")";
			}
		}

		// plan path to destination
		if (!Helper.VectorIsNaN (targetPosition)) { 
			if (aStarSearch.path.Count == 0) {
				aStarSearch.start = (args [0] as GameObject).transform.position;
				aStarSearch.goal = targetPosition;
				aStarSearch.PlanPath (aStarSearch.start, aStarSearch.goal, out aStarSearch.path, (args [0] as GameObject));

				foreach (Vector3 node in aStarSearch.path) {
					(args [0] as GameObject).GetComponent<Voxeme> ().interTargetPositions.Enqueue (node);
				}
			}
		}

		return;
	}

	// IN: Objects, Location
	// OUT: none
	public void MOVE(object[] args)
	{
		// override physics rigging
		foreach (object arg in args) {
			if (arg is GameObject) {
				(arg as GameObject).GetComponent<Rigging> ().ActivatePhysics(false);
			}
		}

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

					Voxeme voxComponent = obj.GetComponent<Voxeme> ();
					if (voxComponent != null) {
						if (!voxComponent.enabled) {
							voxComponent.gameObject.transform.parent = null;
							voxComponent.enabled = true;
						}

						voxComponent.targetPosition = targetPosition;
					}
				}
			}
		}
		return;
	}

	// IN: Objects
	// OUT: none
	public void LIFT(object[] args)
	{
		// look for agent
		GameObject agent = GameObject.FindGameObjectWithTag("Agent");
		if (agent != null) {
			// add preconditions
			if (!SatisfactionTest.IsSatisfied (string.Format ("reach({0})", (args [0] as GameObject).name))) {
				eventManager.InsertEvent (string.Format ("reach({0})", (args [0] as GameObject).name), 0);
				eventManager.InsertEvent (string.Format ("grasp({0})", (args [0] as GameObject).name), 1);
				if (args.Length > 2) {
					eventManager.InsertEvent (eventManager.evalOrig [string.Format ("lift({0},{1})", (args [0] as GameObject).name, Helper.VectorToParsable ((Vector3)args [1]))], 1);
				}
				else {
					eventManager.InsertEvent (eventManager.evalOrig [string.Format ("lift({0})", (args [0] as GameObject).name)], 1);
				}
				eventManager.RemoveEvent (3);
				return;
			}
			else {
				if (!SatisfactionTest.IsSatisfied (string.Format ("grasp({0})", (args [0] as GameObject).name))) {
					eventManager.InsertEvent (string.Format ("grasp({0})", (args [0] as GameObject).name), 0);
					if (args.Length > 2) {
						eventManager.InsertEvent (eventManager.evalOrig [string.Format ("lift({0},{1})", (args [0] as GameObject).name, Helper.VectorToParsable ((Vector3)args [1]))], 1);
					}
					else {
						eventManager.InsertEvent (eventManager.evalOrig [string.Format ("lift({0})", (args [0] as GameObject).name)], 1);
					}
					eventManager.RemoveEvent (2);
					return;
				}
			}

			// add postconditions
			if (args [args.Length - 1] is bool) {
				if ((bool)args [args.Length - 1] == true) {
					eventManager.InsertEvent (string.Format ("ungrasp({0})", (args [0] as GameObject).name), 1);
				}
			}
		}

		// override physics rigging
		foreach (object arg in args) {
			if (arg is GameObject) {
				(arg as GameObject).GetComponent<Rigging> ().ActivatePhysics(false);
			}
		}

		// unrig contained-but-not-supported objects
		foreach (DictionaryEntry pair in relationTracker.relations)
		{
			// support,contain cup1 ball
			if ((pair.Value as string).Contains ("contain") && (!(pair.Value as string).Contains ("support"))) {
				List<GameObject> objs = (pair.Key as List<GameObject>);
				if (objs [0] == (args [0] as GameObject)) {
					for (int i = 1; i < (pair.Key as List<GameObject>).Count; i++) {
						RiggingHelper.UnRig (objs [1], objs [0]);
						objs [1].GetComponent<Voxeme> ().targetPosition = objs [1].transform.position;
						objs [1].GetComponent<Voxeme> ().targetRotation = objs [1].transform.eulerAngles;
					}
				}
			}
		}

		Vector3 targetPosition = Vector3.zero;

		if (args [0] is GameObject) {
			GameObject obj = (args [0] as GameObject);
			Bounds bounds = Helper.GetObjectWorldSize (obj);
			Voxeme voxComponent = obj.GetComponent<Voxeme> ();
			if (voxComponent != null) {
				if (!voxComponent.enabled) {
					voxComponent.gameObject.transform.parent = null;
					voxComponent.enabled = true;
				}

				if (args [1] is Vector3) {
					targetPosition = (Vector3)args [1];
				}
				else {
					targetPosition = new Vector3 (obj.transform.position.x,
						obj.transform.position.y+bounds.size.y+UnityEngine.Random.value,
						obj.transform.position.z);
				}

				voxComponent.targetPosition = targetPosition;
			}
		}


		// add to events manager
		if (args[args.Length-1] is bool) {
			if ((bool)args[args.Length-1] == false) {
				eventManager.events[0] = "lift("+(args [0] as GameObject).name+","+Helper.VectorToParsable(targetPosition)+")";
				Debug.Log (eventManager.events [0]);
			}
		}

		return;
	}

	// IN: Objects
	// OUT: none
	public void SLIDE(object[] args)
	{
		// look for agent
		GameObject agent = GameObject.FindGameObjectWithTag("Agent");
		if (agent != null) {
			// add preconditions
			if (!SatisfactionTest.IsSatisfied (string.Format ("reach({0})", (args [0] as GameObject).name))) {
				eventManager.InsertEvent (string.Format ("reach({0})", (args [0] as GameObject).name), 0);
				eventManager.InsertEvent (string.Format ("grasp({0})", (args [0] as GameObject).name), 1);
				if (args.Length > 2) {
					eventManager.InsertEvent (eventManager.evalOrig [string.Format ("slide({0},{1})", (args [0] as GameObject).name, Helper.VectorToParsable ((Vector3)args [1]))], 1);
				}
				else {
					eventManager.InsertEvent (eventManager.evalOrig [string.Format ("slide({0})", (args [0] as GameObject).name)], 1);
				}
				eventManager.RemoveEvent (3);
				return;
			}
			else {
				if (!SatisfactionTest.IsSatisfied (string.Format ("grasp({0})", (args [0] as GameObject).name))) {
					eventManager.InsertEvent (string.Format ("grasp({0})", (args [0] as GameObject).name), 0);
					if (args.Length > 2) {
						eventManager.InsertEvent (eventManager.evalOrig [string.Format ("slide({0},{1})", (args [0] as GameObject).name, Helper.VectorToParsable ((Vector3)args [1]))], 1);
					}
					else {
						eventManager.InsertEvent (eventManager.evalOrig [string.Format ("slide({0})", (args [0] as GameObject).name)], 1);
					}
					eventManager.RemoveEvent (2);
					return;
				}
			}

			// add postconditions
			if (args [args.Length - 1] is bool) {
				if ((bool)args [args.Length - 1] == true) {
					eventManager.InsertEvent (string.Format ("ungrasp({0})", (args [0] as GameObject).name), 1);
				}
			}
		}

		// override physics rigging
		/*foreach (object arg in args) {
			if (arg is GameObject) {
				(arg as GameObject).GetComponent<Rigging> ().ActivatePhysics(false);
			}
		}*/

		Vector3 targetPosition = Vector3.zero;

		Helper.PrintRDFTriples (rdfTriples);

		string prep = rdfTriples.Count > 0 ? rdfTriples [0].Item2.Replace ("slide", "") : "";

		if (args [0] is GameObject) {
			GameObject obj = (args [0] as GameObject);
			Voxeme voxComponent = obj.GetComponent<Voxeme> ();
			if (voxComponent != null) {
				if (!voxComponent.enabled) {
					voxComponent.gameObject.transform.parent = null;
					voxComponent.enabled = true;
				}

				if (args [1] is Vector3) {
					targetPosition  = (Vector3)args [1];
				}
				else {
					targetPosition = new Vector3 (obj.transform.position.x + UnityEngine.Random.insideUnitSphere.x,
						obj.transform.position.y, obj.transform.position.z + UnityEngine.Random.insideUnitSphere.z);
				}

				voxComponent.targetPosition = targetPosition;
			}
		}

		// add to events manager
		if (args[args.Length-1] is bool) {
			if ((bool)args[args.Length-1] == false) {
				eventManager.events[0] = "slide("+(args [0] as GameObject).name+","+Helper.VectorToParsable(targetPosition)+")";
				Debug.Log (eventManager.events [0]);
			}
		}

		// plan path to destination
		if (!Helper.VectorIsNaN (targetPosition)) { 
			if (aStarSearch.path.Count == 0) {
				Bounds surfaceBounds = Helper.GetObjectWorldSize ((args [0] as GameObject).GetComponent<Voxeme> ().supportingSurface);
				Bounds objBounds = Helper.GetObjectWorldSize (args [0] as GameObject);
				Bounds embeddingSpaceBounds = new Bounds ();
				embeddingSpaceBounds.SetMinMax (new Vector3 (surfaceBounds.min.x+(objBounds.size.x/2), surfaceBounds.max.y, surfaceBounds.min.z+(objBounds.size.z/2)),
					new Vector3 (surfaceBounds.max.x, objBounds.max.y, surfaceBounds.max.z));

				aStarSearch.start = (args [0] as GameObject).transform.position;
				aStarSearch.goal = targetPosition;
				aStarSearch.PlanPath (aStarSearch.start, aStarSearch.goal, out aStarSearch.path, (args [0] as GameObject), embeddingSpaceBounds, "Y");

				foreach (Vector3 node in aStarSearch.path) {
					(args [0] as GameObject).GetComponent<Voxeme> ().interTargetPositions.Enqueue (node);
				}
			}
		}

		return;
	}

	// IN: Objects, Location
	// OUT: none
	public void ROLL(object[] args)
	{
		// look for agent
		GameObject agent = GameObject.FindGameObjectWithTag("Agent");

		// add agent-dependent preconditions
		if (agent != null) {
			if (!SatisfactionTest.IsSatisfied (string.Format ("reach({0})", (args [0] as GameObject).name))) {
				eventManager.InsertEvent (string.Format ("reach({0})", (args [0] as GameObject).name), 0);
				eventManager.InsertEvent (string.Format ("grasp({0})", (args [0] as GameObject).name), 1);
				if (args.Length > 2) {
					eventManager.InsertEvent (eventManager.evalOrig [string.Format ("roll({0},{1})", (args [0] as GameObject).name, Helper.VectorToParsable ((Vector3)args [1]))], 1);
				} else {
					eventManager.InsertEvent (eventManager.evalOrig [string.Format ("roll({0})", (args [0] as GameObject).name)], 1);
				}
				eventManager.RemoveEvent (3);
				return;
			}
			else if (!SatisfactionTest.IsSatisfied (string.Format ("grasp({0})", (args [0] as GameObject).name))) {
				eventManager.InsertEvent (string.Format ("grasp({0})", (args [0] as GameObject).name), 0);
				if (args.Length > 2) {
					eventManager.InsertEvent (eventManager.evalOrig [string.Format ("roll({0},{1})", (args [0] as GameObject).name, Helper.VectorToParsable ((Vector3)args [1]))], 1);
				} else {
					eventManager.InsertEvent (eventManager.evalOrig [string.Format ("roll({0})", (args [0] as GameObject).name)], 1);
				}
				eventManager.RemoveEvent (2);
				return;
			}
		}

		// check and see if rigidbody orientations and main body orientations are getting out of sync
		// due to physics effects

		// find the smallest displacement angle between an axis on the main body and an axis on this rigidbody
		float displacementAngle = 360.0f;
		Quaternion rigidbodyRotation = Quaternion.identity;
		Rigidbody[] rigidbodies = (args [0] as GameObject).GetComponentsInChildren<Rigidbody> ();
		foreach (Rigidbody rigidbody in rigidbodies) {
			foreach (Vector3 mainBodyAxis in Constants.Axes.Values) {
				foreach (Vector3 rigidbodyAxis in Constants.Axes.Values) {
					if (Vector3.Angle ((args [0] as GameObject).transform.rotation * mainBodyAxis, rigidbody.rotation * rigidbodyAxis) < displacementAngle) {
						displacementAngle = Vector3.Angle ((args [0] as GameObject).transform.rotation * mainBodyAxis, rigidbody.rotation * rigidbodyAxis);
						rigidbodyRotation = rigidbody.rotation;
					}
				}
			}
		}

		// if rigidbody is out of sync
		if (displacementAngle > Mathf.Rad2Deg * Constants.EPSILON) {
			Vector3 relativeDisplacement = (rigidbodyRotation * Quaternion.Inverse ((args [0] as GameObject).transform.rotation)).eulerAngles;
			Debug.Log (string.Format ("Displacement: {0}", relativeDisplacement));

			Quaternion resolve = Quaternion.identity;
			Quaternion resolveInv = Quaternion.identity;
			Voxeme voxComponent = (args [0] as GameObject).GetComponent<Voxeme> ();
			if (voxComponent != null) {
				foreach (Rigidbody rigidbody in rigidbodies) {
					if ((voxComponent.displacement.ContainsKey (rigidbody.name)) && (voxComponent.rotationalDisplacement.ContainsKey (rigidbody.name))) {
						// initial = initial rotational displacement
						Quaternion initial = Quaternion.Euler (voxComponent.rotationalDisplacement [rigidbody.name]);
						Debug.Log (initial.eulerAngles);
						// current = current rotational displacement due to physics
						Quaternion current = rigidbody.transform.localRotation;// * Quaternion.Inverse ((args [0] as GameObject).transform.rotation));
						Debug.Log (current.eulerAngles);
						// resolve = rotation to get from initial rotational displacement to current rotational displacement
						resolve = current * Quaternion.Inverse (initial);
						Debug.Log (resolve.eulerAngles);
						//Debug.Log ((initial * resolve).eulerAngles);
						Debug.Log ((resolve * initial).eulerAngles);
						// resolveInv = rotation to get from final (current rigidbody) rotation back to initial (aligned with main obj) rotation
						resolveInv = initial * Quaternion.Inverse (current);
						//Debug.Log (resolveInv.eulerAngles);
						//rigidbody.transform.rotation = obj.transform.rotation * initial;
						rigidbody.transform.localRotation = initial;// * (args [0] as GameObject).transform.rotation;
						Debug.Log (rigidbody.transform.rotation.eulerAngles);

						//rigidbody.transform.localPosition = voxComponent.displacement [rigidbody.name];
						//rigidbody.transform.position = (args [0] as GameObject).transform.position + voxComponent.displacement [rigidbody.name];
					}
				}

				//Debug.Break ();

				//Debug.Log (resolve.eulerAngles);
				Debug.Log (Helper.VectorToParsable (rigidbodies [0].transform.position));
				//Debug.Log (Helper.VectorToParsable (rigidbodies [0].transform.localPosition));
				Debug.Log (Helper.VectorToParsable ((args [0] as GameObject).transform.position));
				(args [0] as GameObject).transform.position = rigidbodies [0].transform.position;// - voxComponent.displacement [rigidbodies[0].name];
				voxComponent.targetPosition = (args [0] as GameObject).transform.position;
				Debug.Log (Helper.VectorToParsable ((args [0] as GameObject).transform.position));

				Debug.Log (Helper.VectorToParsable (rigidbodies [0].transform.position));
				//Debug.Log (Helper.VectorToParsable (voxComponent.displacement [rigidbodies[0].name]));

				foreach (Rigidbody rigidbody in rigidbodies) {
					if ((voxComponent.displacement.ContainsKey (rigidbody.name)) && (voxComponent.rotationalDisplacement.ContainsKey (rigidbody.name))) {
						Debug.Log (rigidbody.name);
						rigidbody.transform.localPosition = voxComponent.displacement [rigidbody.name];
					}
				}
			
				Debug.Log (Helper.VectorToParsable ((args [0] as GameObject).transform.position));
				Debug.Log (Helper.VectorToParsable (rigidbodies [0].transform.localPosition));

				Debug.Log ((args [0] as GameObject).transform.rotation.eulerAngles);
				foreach (Rigidbody rigidbody in rigidbodies) {
					Debug.Log (Helper.VectorToParsable (rigidbody.transform.localPosition));
				}

				(args [0] as GameObject).transform.rotation = resolve * (args [0] as GameObject).transform.rotation;
				voxComponent.targetRotation = (args [0] as GameObject).transform.rotation.eulerAngles;
				Debug.Log ((args [0] as GameObject).transform.rotation.eulerAngles);

				//Debug.Break ();
			}
		}

		// calc object properties
		float diameter = Helper.GetObjectWorldSize ((args [0] as GameObject)).size.y;	// bounds sphere diameter = world size.y
		float circumference = Mathf.PI * diameter;	// circumference = pi*diameter
		float revs = 0;

		// get the path
		Vector3 offset = Vector3.zero;

		while (offset.magnitude <= 0.5f * circumference) {
			offset = new Vector3 (UnityEngine.Random.insideUnitSphere.x, 0.0f, UnityEngine.Random.insideUnitSphere.z);	// random by default
			revs = offset.magnitude / circumference;	// # revolutions = path length/circumference
		}

		if (args [1] is Vector3) {
			Debug.Log ((Vector3)args [1]);
			Debug.Log ((args [0] as GameObject).transform.position);
			offset = ((Vector3)args [1]) - (args [0] as GameObject).transform.position;
			offset = new Vector3 (offset.x, 0.0f, offset.z);
		}
		Debug.Log (string.Format("Offset: {0}",offset));
		Debug.Log (offset.magnitude);
//		System.Random rand = new System.Random();
//		if (rand.Next(0, 2) == 0)
//			offset = new Vector3 (0.0f,0.0f,1.0f);
//		else
//			offset = new Vector3 (-1.0f,0.0f,0.0f);
		//offset = new Vector3 (0.5f,0.0f,0.5f);

		// compute axis of rotation
		Vector3 planeNormal = Constants.yAxis;	// TODO: compute normal of surface
		Vector3 worldRotAxis = Vector3.Cross (offset.normalized, planeNormal);

		// rotate object such that an axis of rotation is perpendicular to the intended path and coplanar with the surface (TODO assuming surface normal of Y-axis for now)
		// determine axis of symmetry from VoxML
		Vector3 objRotAxis = Vector3.zero;
		float angleToRot = 360.0f;
		Debug.Log (worldRotAxis);

//		Debug.Log (worldRotAxis);
//
//		if ((args [0] as GameObject).GetComponent<Voxeme> () != null) {
//			if (!(args [0] as GameObject).GetComponent<Voxeme> ().enabled) {
//				(args [0] as GameObject).GetComponent<Voxeme> ().gameObject.transform.parent = null;
//				(args [0] as GameObject).GetComponent<Voxeme> ().enabled = true;
//			}
//
//			foreach (string s in (args [0] as GameObject).GetComponent<Voxeme> ().opVox.Type.RotatSym) {
//				if (Vector3.Angle ((args [0] as GameObject).transform.rotation * Constants.Axes [s], worldRotAxis) < angleToRot) {
//					angleToRot = Vector3.Angle ((args [0] as GameObject).transform.rotation * Constants.Axes [s], worldRotAxis);
//					objRotAxis = Constants.Axes [s];
//				}
//				Debug.Log ((args [0] as GameObject).transform.rotation * objRotAxis);
//				//Debug.Log (angleToRot);
//			}
//		}

		// add agent-independent preconditions
		if (args [0] is GameObject) {
			GameObject obj = (args [0] as GameObject);
			Voxeme voxComponent = obj.GetComponent<Voxeme> ();
			if (voxComponent != null) {
				if (!voxComponent.enabled) {
					voxComponent.gameObject.transform.parent = null;
					voxComponent.enabled = true;
				}

				foreach (string s in voxComponent.opVox.Type.RotatSym) {
					if (Constants.Axes.ContainsKey (s)) {
						if (Vector3.Angle (obj.transform.rotation * Constants.Axes [s], worldRotAxis) < angleToRot) {
							angleToRot = Vector3.Angle (obj.transform.rotation * Constants.Axes [s], worldRotAxis);
							objRotAxis = Constants.Axes [s];
							//objRotAxis = obj.transform.rotation * Constants.Axes [s];
						}
					}
					else {
						voxComponent.targetPosition = new Vector3 (float.NaN, float.NaN, float.NaN);
						return;
					}
				}

				//Debug.Break ();
				Debug.Log (obj.transform.rotation * objRotAxis);
				Debug.Log (angleToRot);

				//Debug.Log (Quaternion.FromToRotation (objRotAxis, worldRotAxis).eulerAngles);

				if (!SatisfactionTest.IsSatisfied (string.Format ("turn({0},{1},{2})", (args [0] as GameObject).name,
					    Helper.VectorToParsable (objRotAxis), Helper.VectorToParsable (worldRotAxis)))) {
					Debug.Log (string.Format ("turn({0},{1},{2})", (args [0] as GameObject).name,
						Helper.VectorToParsable (obj.transform.rotation * objRotAxis), Helper.VectorToParsable (worldRotAxis)));
					eventManager.InsertEvent (string.Format ("turn({0},{1},{2})", (args [0] as GameObject).name,
						Helper.VectorToParsable (objRotAxis), Helper.VectorToParsable (worldRotAxis)), 0);
					Debug.Log (string.Format ("roll({0},{1})", (args [0] as GameObject).name, Helper.VectorToParsable ((args [0] as GameObject).transform.position + offset)));
					eventManager.InsertEvent (string.Format ("roll({0},{1})", (args [0] as GameObject).name, Helper.VectorToParsable ((args [0] as GameObject).transform.position + offset)), 1);
					eventManager.RemoveEvent (eventManager.events.Count - 1);

					// update subobject rigidbody rotations
					// TODO: UpdateSubObjectRigidbodyRotations()
//					Rigidbody[] rigidbodies = obj.gameObject.GetComponentsInChildren<Rigidbody> ();
//					foreach (Rigidbody rigidbody in rigidbodies) {
//						if (voxComponent.staticStateRotations.ContainsKey (rigidbody.name)) {
//							Debug.Log(rigidbody.name);
//							Debug.Log(rigidbody.rotation.eulerAngles);
//							voxComponent.staticStateRotations [rigidbody.name] = rigidbody.rotation.eulerAngles;
//						}
//					}
					return;
				}
				else {
					Debug.Log ("Turn already satisfied");
				}
			}
		}

		if (agent != null) {
			// add agent-dependent postconditions
			if (args [args.Length - 1] is bool) {
				if ((bool)args [args.Length - 1] == true) {
					eventManager.InsertEvent (string.Format ("ungrasp({0})", (args [0] as GameObject).name), 1);
				}
			}
		}

		// override physics rigging
		// TODO: for programs with implied surfaces in the VoxML encoding, don't deactivate physics?
//		foreach (object arg in args) {
//			if (arg is GameObject) {
//				(arg as GameObject).GetComponent<Rigging> ().ActivatePhysics(false);
//			}
//		}

		Vector3 targetPosition = Vector3.zero;
		Quaternion targetRotation = Quaternion.identity;

		Helper.PrintRDFTriples (rdfTriples);

		string prep = rdfTriples.Count > 0 ? rdfTriples [0].Item2.Replace ("roll", "") : "";

		if (args [0] is GameObject) {
			GameObject obj = (args [0] as GameObject);
			Voxeme voxComponent = obj.GetComponent<Voxeme> ();
			if (voxComponent != null) {
				if (!voxComponent.enabled) {
					voxComponent.gameObject.transform.parent = null;
					voxComponent.enabled = true;
				}

				targetRotation = obj.transform.rotation;

				if (args [1] is Vector3) {
					targetPosition  = (Vector3)args [1];
				}
				else {
					targetPosition = obj.transform.position + offset;
				}

				// calculate how many revolutions object will make
				float degrees = -180.0f * revs;
				//Debug.Log (degrees);

				//Vector3 transverseAxis = Quaternion.AngleAxis (90.0f, Vector3.up) * offset.normalized;	// the axis parallel to the surface

				//Debug.Log (worldRotAxis);

				while (degrees < -180.0f) {
					targetRotation = Quaternion.AngleAxis (-180.0f, worldRotAxis) * targetRotation;
					//Debug.Log (targetRotation.eulerAngles);
					voxComponent.interTargetRotations.Enqueue (targetRotation.eulerAngles);
					degrees += 180.0f;
				}

				targetRotation = Quaternion.AngleAxis (degrees, worldRotAxis) * targetRotation;
				//Debug.Log (targetRotation.eulerAngles);
				voxComponent.interTargetRotations.Enqueue (targetRotation.eulerAngles);
				//	}
				//}

				// calculate turnSpeed (angular velocity)
				// estimate where obj will be next time step
				Vector3 normalizedOffset = offset.normalized;
				Vector3 lookAheadPos = new Vector3 (obj.transform.position.x - normalizedOffset.x * Time.deltaTime * voxComponent.moveSpeed,
					obj.transform.position.y - normalizedOffset.y * Time.deltaTime * voxComponent.moveSpeed,
					obj.transform.position.z - normalizedOffset.z * Time.deltaTime * voxComponent.moveSpeed);
				// appox distance to be traveled over next timestep
				float distPerTimestep = (lookAheadPos - obj.transform.position).magnitude;
				//Debug.Log(distPerTimestep);
				// estimate approx timesteps to traverse path
				float time = (offset.magnitude*Time.deltaTime/distPerTimestep);
				//Debug.Log(time);
				// velocity = dist/time
				float vel = offset.magnitude/time;
				//Debug.Log(vel);
				// w = v/d
				float angularVelocity = vel / Helper.GetObjectWorldSize (obj).size.y;
				//Debug.Log(angularVelocity);

				voxComponent.targetPosition = targetPosition;
				voxComponent.targetRotation = targetRotation.eulerAngles;
				voxComponent.turnSpeed = angularVelocity;
			}
		}

		// add to events manager
		if (args[args.Length-1] is bool) {
			if ((bool)args[args.Length-1] == false) {
				eventManager.events[0] = "roll("+(args [0] as GameObject).name+","+Helper.VectorToParsable(targetPosition)+")";
				//Debug.Log (eventManager.events [0]);
			}
		}

		return;
	}

	// IN: Objects
	// OUT: none
	public void FLIP(object[] args)
	{
		// override physics rigging
		foreach (object arg in args) {
			if (arg is GameObject) {
				(arg as GameObject).GetComponent<Rigging> ().ActivatePhysics(false);
			}
		}

		Vector3 targetRotation = Vector3.zero;

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

			Voxeme voxComponent = obj.GetComponent<Voxeme> ();
			if (voxComponent != null) {
				if (!voxComponent.enabled) {
					voxComponent.gameObject.transform.parent = null;
					voxComponent.enabled = true;
				}

				voxComponent.targetRotation = targetRotation;
			}
		}

		// add to events manager
		if (args[args.Length-1] is bool) {
			if ((bool)args[args.Length-1] == false) {
				//eventManager.eventsStatus.Add ("flip("+(args [0] as GameObject).name+","+Helper.VectorToParsable(targetRotation)+")", false);
				eventManager.events[0] = "flip("+(args [0] as GameObject).name+","+Helper.VectorToParsable(targetRotation)+")";
			}
		}

		return;
	}

	// IN: Objects
	// OUT: none
	public void TURN(object[] args)
	{
		// look for agent
		GameObject agent = GameObject.FindGameObjectWithTag("Agent");
		if (agent != null) {
			// add preconditions
			if (!SatisfactionTest.IsSatisfied (string.Format ("reach({0})", (args [0] as GameObject).name))) {
				eventManager.InsertEvent (string.Format ("reach({0})", (args [0] as GameObject).name), 0);
				eventManager.InsertEvent (string.Format ("grasp({0})", (args [0] as GameObject).name), 1);
				if (args.Length > 2) {
					eventManager.InsertEvent (eventManager.evalOrig [string.Format ("turn({0},{1})", (args [0] as GameObject).name, Helper.VectorToParsable ((Vector3)args [1]))], 1);
				}
				else {
					eventManager.InsertEvent (eventManager.evalOrig [string.Format ("turn({0})", (args [0] as GameObject).name)], 1);
				}
				eventManager.RemoveEvent (3);
				return;
			}
			else {
				if (!SatisfactionTest.IsSatisfied (string.Format ("grasp({0})", (args [0] as GameObject).name))) {
					eventManager.InsertEvent (string.Format ("grasp({0})", (args [0] as GameObject).name), 0);
					if (args.Length > 2) {
						eventManager.InsertEvent (eventManager.evalOrig [string.Format ("turn({0},{1})", (args [0] as GameObject).name, Helper.VectorToParsable ((Vector3)args [1]))], 1);
					}
					else {
						eventManager.InsertEvent (eventManager.evalOrig [string.Format ("turn({0})", (args [0] as GameObject).name)], 1);
					}
					eventManager.RemoveEvent (2);
					return;
				}
			}

			// add postconditions
			if (args [args.Length - 1] is bool) {
				if ((bool)args [args.Length - 1] == true) {
					eventManager.InsertEvent (string.Format ("ungrasp({0})", (args [0] as GameObject).name), 1);
				}
			}
		}

//		if (args [args.Length - 1] is bool) {
//			if ((bool)args [args.Length - 1] == true) {
//				// resolve subobject rigidbody rotations
//				// TODO: ResolveSubObjectRigidbodyRotations()
//				Debug.Log ((args [0] as GameObject).name);
//				Debug.Log ((args [0] as GameObject).transform.rotation.eulerAngles);
//				Rigidbody[] rigidbodies = (args [0] as GameObject).GetComponentsInChildren<Rigidbody> ();
//				Quaternion resolveInv = Quaternion.identity;
//				foreach (Rigidbody rigidbody in rigidbodies) {
//					if ((args [0] as GameObject).GetComponent<Voxeme> ().staticStateRotations.ContainsKey (rigidbody.name)) {
//						Debug.Log (rigidbody.name);
//						Quaternion initial = Quaternion.Euler ((args [0] as GameObject).GetComponent<Voxeme> ().staticStateRotations [rigidbody.name]);
//						Debug.Log (initial.eulerAngles);
//						Quaternion final = rigidbody.rotation;
//						Debug.Log (final.eulerAngles);
//						// resolve = rotation to get from final resting orientation back to initial orientation before physics effects
//						Quaternion resolve = final * Quaternion.Inverse (initial);
//						Debug.Log (resolve.eulerAngles);
//						resolveInv = initial * Quaternion.Inverse (final);
//						rigidbody.MoveRotation (resolve * rigidbody.rotation);
//					}
//				}
//
//				(args [0] as GameObject).transform.rotation = resolveInv * (args [0] as GameObject).transform.rotation;
//			}
//		}

		// override physics rigging
		foreach (object arg in args) {
			if (arg is GameObject) {
				(arg as GameObject).GetComponent<Rigging> ().ActivatePhysics(false);
			}
		}

		Vector3 targetRotation = Vector3.zero;

		Helper.PrintRDFTriples (rdfTriples);

		string prep = rdfTriples.Count > 0 ? rdfTriples [0].Item2.Replace ("turn", "") : "";

		if (args [0] is GameObject) {
			GameObject obj = (args [0] as GameObject);
			Voxeme voxComponent = obj.GetComponent<Voxeme> ();
			if (voxComponent != null) {
				if (!voxComponent.enabled) {
					voxComponent.gameObject.transform.parent = null;
					voxComponent.enabled = true;
				}

				if (args [1] is Vector3 &&  args [2] is Vector3){
					Debug.Log ((Vector3)args [1]);
					Debug.Log (obj.transform.rotation * (Vector3)args [1]);
					Debug.Log ((Vector3)args [2]);
					targetRotation = Quaternion.FromToRotation((Vector3)args [1],(Vector3)args [2]).eulerAngles;
					//targetRotation = Quaternion.LookRotation(obj.transform.rotation * (Vector3)args [1],(Vector3)args [2]).eulerAngles;
				}
				else {
					targetRotation = (obj.transform.rotation * UnityEngine.Random.rotation).eulerAngles;
				}

				voxComponent.targetRotation = targetRotation;
				Debug.Log (Helper.VectorToParsable (voxComponent.targetRotation));
			}
		}

		// add to events manager
		if (args[args.Length-1] is bool) {
			if ((bool)args[args.Length-1] == false) {
				if (args [1] is Vector3 && args [2] is Vector3) {
					eventManager.events [0] = "turn(" + (args [0] as GameObject).name + "," + Helper.VectorToParsable ((Vector3)args [1]) + "," + Helper.VectorToParsable ((Vector3)args [2]) + ")";
				}
				else {
					eventManager.events [0] = "turn(" + (args [0] as GameObject).name + "," +
						Helper.VectorToParsable ((args [0] as GameObject).transform.rotation * Constants.yAxis) + "," +
						Helper.VectorToParsable ((args [0] as GameObject).transform.rotation * Quaternion.Euler(targetRotation) * Constants.yAxis) + ")";
				}
				Debug.Log (eventManager.events [0]);
			}
		}

		return;
	}

	// IN: Objects
	// OUT: none
	public void SPIN(object[] args)
	{
	}

	// IN: Objects
	// OUT: none
	public void STACK(object[] args)
	{
	}

	// IN: Objects
	// OUT: none
	public void LEAN(object[] args)
	{
	}

	// IN: Objects
	// OUT: none
	public void SWITCH(object[] args)
	{
		if ((args [0] is GameObject) && (args [1] is GameObject)) {
			Debug.Log ((args [0] is GameObject));
			Debug.Log ((args [1] is GameObject));
			Vector3[] startPos = new Vector3[] { (args [0] as GameObject).transform.position,(args [1] as GameObject).transform.position };

			if (startPos [0].x < startPos [1].x) {	// if args[0] is left of args[1]
				eventManager.InsertEvent (string.Format ("slide({0},{1})", (args [0] as GameObject).name,
					Helper.VectorToParsable (startPos [1] + (Vector3.right*.8f))), 0);
				eventManager.InsertEvent (string.Format ("slide({0},{1})", (args [1] as GameObject).name,
					Helper.VectorToParsable (startPos [0])), 1);
				eventManager.InsertEvent (string.Format ("slide({0},{1})", (args [0] as GameObject).name,
					Helper.VectorToParsable (startPos [1])), 2);
			}
			else if (startPos [0].x > startPos [1].x) {	// if args[0] is right of args[1]
				eventManager.InsertEvent (string.Format ("slide({0},{1})", (args [0] as GameObject).name,
					Helper.VectorToParsable (startPos [1] + (Vector3.left*.8f))), 0);
				eventManager.InsertEvent (string.Format ("slide({0},{1})", (args [1] as GameObject).name,
					Helper.VectorToParsable (startPos [0])), 1);
				eventManager.InsertEvent (string.Format ("slide({0},{1})", (args [0] as GameObject).name,
					Helper.VectorToParsable (startPos [1])), 2);
			}
			eventManager.RemoveEvent (3);
		}
	}

	// IN: Objects
	// OUT: none
	public void BIND(object[] args)
	{
		//Vector3 targetRotation;
		
		//Helper.PrintRDFTriples (rdfTriples);
		bool r = false;
		foreach (object arg in args) {
			if (arg == null) {
				r = true;
				break;
			}
		}

		if (r) {
			return;
		}

		GameObject container = null;
		Vector3 boundsCenter = Vector3.zero,boundsSize = Vector3.zero;

		if (args [args.Length - 1] is bool) {
			if ((bool)args [args.Length - 1] == true) {
				if (args [args.Length - 2] is String) {
					container = new GameObject ((args [args.Length - 2] as String).Replace("\"",""));
				}
				else {
					container = new GameObject ("bind");
				}

				if (args.Length-1 == 0) {
					container.transform.position = Vector3.zero;
				}

				// get bounds of composite to be created
				List<GameObject> objs = new List<GameObject> ();
				foreach (object arg in args) {
					if (arg is GameObject) {
						objs.Add (arg as GameObject);
					}
				}
				Bounds bounds = Helper.GetObjectWorldSize (objs);
				boundsCenter = container.transform.position = bounds.center;
				boundsSize = bounds.size;

				// nuke any relations between objects to be bound
				RelationTracker relationTracker = (RelationTracker)GameObject.Find("BehaviorController").GetComponent("RelationTracker");
				List<object> toRemove = new List<object>();

				foreach (DictionaryEntry pair in relationTracker.relations) {
					if (objs.Contains ((pair.Key as List<GameObject>) [0]) && objs.Contains ((pair.Key as List<GameObject>) [1])) {
						toRemove.Add (pair.Key);
					}
				}

				foreach (object key in toRemove) {
					relationTracker.RemoveRelation (key as List<GameObject>);
				}

				// bind objects
				foreach (object arg in args) {
					if (arg is GameObject) {
						(arg as GameObject).GetComponent<Voxeme>().enabled = false;
						(arg as GameObject).GetComponent<Rigging>().ActivatePhysics(false);

						Collider[] colliders = (arg as GameObject).GetComponentsInChildren<Collider> ();
						foreach (Collider collider in colliders) {
							collider.isTrigger = false;
						}

						(arg as GameObject).transform.parent = container.transform;
						if (!(args [args.Length - 2] is String)) {
							container.name = container.name + " " + (arg as GameObject).name;
						}
					}
				}
			}
		}

		if (container != null) {
			container.AddComponent<Voxeme> ();
			container.AddComponent<Rigging> ();
			BoxCollider collider = container.AddComponent<BoxCollider> ();
			//collider.center = boundsCenter;
			collider.size = boundsSize;
			collider.isTrigger = true;
		}
	}

	// IN: Objects
	// OUT: none
	public void CLOSE(object[] args)
	{
		if (args [0] is GameObject) {
			GameObject theme = (args [0] as GameObject);

			Voxeme voxComponent = theme.GetComponent<Voxeme> ();

			if (voxComponent != null) {
				if (voxComponent.voxml.Type.Concavity != "Concave") {
					voxComponent.targetPosition = new Vector3 (float.NaN, float.NaN, float.NaN);
					return;
				}
				else {
					if (!SatisfactionTest.IsSatisfied (string.Format ("put(lid,on({0}))", (args [0] as GameObject).name))) {
						eventManager.InsertEvent (string.Format ("put(lid,on({0}))", (args [0] as GameObject).name), 0);
						eventManager.RemoveEvent (1);
						return;
					}
				}
			}
		}
	}

	// IN: Objects
	// OUT: none
	public void OPEN(object[] args)
	{
	}

	// IN: Objects
	// OUT: none
	public void UNBIND(object[] args)
	{
		if (args [0] is GameObject) {
			GameObject obj = (args [0] as GameObject);

			foreach (Transform transform in obj.GetComponentsInChildren<Transform>()) {
				transform.parent = null;
				if (transform.gameObject.GetComponent<Rigging> () != null) {
					transform.gameObject.GetComponent<Rigging> ().ActivatePhysics (true);
					transform.gameObject.GetComponent<Voxeme> ().enabled = true;
				}
			}

			GameObject.Destroy (obj);
		}
	}

	// IN: Objects
	// OUT: none
	public void ENABLE(object[] args)
	{	
		foreach (object obj in args) {
			if (obj is GameObject) {
				objSelector.disabledObjects.Remove((obj as GameObject));
				(obj as GameObject).SetActive (true);
//				foreach (Renderer renderer in (obj as GameObject).GetComponentsInChildren<Renderer>()) {
//					renderer.enabled = true;
//				}
			}
		}

		macros.ClearMacros ();
		macros.PopulateMacros ();
	}

	// IN: Objects
	// OUT: none
	public void DISABLE(object[] args)
	{
		foreach (object obj in args) {
			if (obj is GameObject) {
				objSelector.disabledObjects.Add((obj as GameObject));
				(obj as GameObject).SetActive (false);
//				foreach (Renderer renderer in (obj as GameObject).GetComponentsInChildren<Renderer>()) {
//					renderer.enabled = false;
//				}
			}
		}

		macros.ClearMacros ();
		macros.PopulateMacros ();
	}

	/* AGENT-DEPENDENT BEHAVIORS */

	// IN: Objects
	// OUT: none
	public void POINT(object[] args)
	{
		GameObject agent = GameObject.FindGameObjectWithTag ("Agent");
		if (agent != null) {
			Animator anim = agent.GetComponentInChildren<Animator> ();
			GameObject leftGrasper = anim.GetBoneTransform (HumanBodyBones.LeftHand).transform.gameObject;
			GameObject rightGrasper = anim.GetBoneTransform (HumanBodyBones.RightHand).transform.gameObject;
			GameObject grasper;

			if (args [args.Length - 1] is bool) {
				if ((bool)args [args.Length - 1] == true) {
					foreach (object arg in args) {
						if (arg is GameObject) {
							// find bounds corner closest to grasper
							Bounds bounds = Helper.GetObjectWorldSize((arg as GameObject));

							// which hand is closer?
							float leftToGoalDist = (leftGrasper.transform.position-bounds.ClosestPoint(leftGrasper.transform.position)).magnitude;
							float rightToGoalDist = (rightGrasper.transform.position-bounds.ClosestPoint(rightGrasper.transform.position)).magnitude;

							if (leftToGoalDist < rightToGoalDist) {
								grasper = leftGrasper;
								agent.GetComponent<GraspScript>().grasper = (int)Gestures.HandPose.LeftPoint;
							}
							else {
								grasper = rightGrasper;
								agent.GetComponent<GraspScript>().grasper = (int)Gestures.HandPose.RightPoint;
							}
								
							IKControl ikControl = agent.GetComponent<IKControl> ();
							if (ikControl != null) {
								Vector3 target;
								if (grasper == leftGrasper) {
									target = new Vector3 (bounds.min.x, bounds.min.y, bounds.center.z);
									ikControl.leftHandObj.transform.position = target;
								}
								else {
									target = new Vector3 (bounds.max.x, bounds.min.y, bounds.center.z);
									ikControl.rightHandObj.transform.position = target;
								}
							}
						}
					}
				}
			}
		}
	}

	// IN: Objects
	// OUT: none
	public void REACH(object[] args)
	{
		GameObject agent = GameObject.FindGameObjectWithTag ("Agent");
		if (agent != null) {
			Animator anim = agent.GetComponentInChildren<Animator> ();
			GameObject leftGrasper = anim.GetBoneTransform (HumanBodyBones.LeftHand).transform.gameObject;
			GameObject rightGrasper = anim.GetBoneTransform (HumanBodyBones.RightHand).transform.gameObject;
			GameObject grasper;
			GraspScript graspController = agent.GetComponent<GraspScript> ();
			Transform leftGrasperCoord = graspController.leftGrasperCoord;
			Transform rightGrasperCoord = graspController.rightGrasperCoord;
			Vector3 offset = graspController.graspTrackerOffset;

			if (args [args.Length - 1] is bool) {
				if ((bool)args [args.Length - 1] == true) {
					foreach (object arg in args) {
						if (arg is GameObject) {
							// find bounds corner closest to grasper
							Bounds bounds = Helper.GetObjectWorldSize((arg as GameObject));

							// which hand is closer?
							float leftToGoalDist = (leftGrasper.transform.position-bounds.ClosestPoint(leftGrasper.transform.position)).magnitude;
							float rightToGoalDist = (rightGrasper.transform.position-bounds.ClosestPoint(rightGrasper.transform.position)).magnitude;

							if (leftToGoalDist < rightToGoalDist) {
								grasper = leftGrasper;
							}
							else {
								grasper = rightGrasper;
							}
								
							IKControl ikControl = agent.GetComponent<IKControl> ();
							if (ikControl != null) {
								Vector3 target;
								if (grasper == leftGrasper) {
									agent.GetComponent<GraspScript>().grasper = (int)Gestures.HandPose.LeftClaw;
									if ((grasper.GetComponent<BoxCollider> ().bounds.size.x > bounds.size.x) &&
									    (grasper.GetComponent<BoxCollider> ().bounds.size.z > bounds.size.z)) {
										target = new Vector3 (bounds.center.x, bounds.center.y, bounds.center.z);
									}
									else {
										target = new Vector3 (bounds.min.x, bounds.center.y, bounds.center.z);
									}
									ikControl.leftHandObj.transform.position = target+offset;
								}
								else {
									agent.GetComponent<GraspScript>().grasper = (int)Gestures.HandPose.RightClaw;
									if ((grasper.GetComponent<BoxCollider> ().bounds.size.x > bounds.size.x) &&
										(grasper.GetComponent<BoxCollider> ().bounds.size.z > bounds.size.z)) {
										target = new Vector3 (bounds.center.x, bounds.center.y, bounds.center.z);
									}
									else {
										target = new Vector3 (bounds.max.x, bounds.center.y, bounds.center.z);
									}
									ikControl.rightHandObj.transform.position = target+offset;
								}
							}
						}
					}
				}
			}
		}
	}

	// IN: Objects
	// OUT: none
	public void GRASP(object[] args)
	{
		GameObject agent = GameObject.FindGameObjectWithTag ("Agent");

		if (agent != null) {
			Bounds bounds = Helper.GetObjectWorldSize((args[0] as GameObject));
			Animator anim = agent.GetComponentInChildren<Animator> ();
			GameObject leftGrasper = anim.GetBoneTransform (HumanBodyBones.LeftHand).transform.gameObject;
			GameObject rightGrasper = anim.GetBoneTransform (HumanBodyBones.RightHand).transform.gameObject;
			GameObject grasper;
			Transform leftGraspTracker = agent.GetComponent<IKControl> ().leftHandObj;
			Transform rightGraspTracker = agent.GetComponent<IKControl> ().rightHandObj;
			Vector3 offset = agent.GetComponent<GraspScript> ().graspTrackerOffset;

			// make sure we're reaching toward the object first
			if (!bounds.Contains(leftGraspTracker.position-offset) && 
				!bounds.Contains(rightGraspTracker.position-offset)) {
				eventManager.InsertEvent (string.Format ("reach({0})", (args[0] as GameObject).name), 0);
				//eventManager.RemoveEvent (eventManager.events.Count - 1);
				return;
			}

			if (args [args.Length - 1] is bool) {
				if ((bool)args [args.Length - 1] == true) {
					foreach (object arg in args) {
						if (arg is GameObject) {
							//Debug.Log (rightGrasper.GetComponent<BoxCollider> ().bounds);
							//Debug.Log (bounds);
							if (leftGrasper.GetComponent<BoxCollider>().bounds.Intersects(bounds)) {
								(arg as GameObject).GetComponent<Rigging> ().ActivatePhysics (false);
								RiggingHelper.RigTo ((arg as GameObject), leftGrasper);
								Voxeme voxeme = (arg as GameObject).GetComponent<Voxeme> ();
								voxeme.enabled = true;
								voxeme.isGrasped = true;
								voxeme.graspTracker = agent.GetComponent<IKControl>().leftHandObj;
								voxeme.grasperCoord = agent.GetComponent<GraspScript>().leftGrasperCoord;
							}
							else if (rightGrasper.GetComponent<BoxCollider>().bounds.Intersects(bounds)) {
								(arg as GameObject).GetComponent<Rigging> ().ActivatePhysics (false);
								RiggingHelper.RigTo ((arg as GameObject), rightGrasper);
								Voxeme voxeme = (arg as GameObject).GetComponent<Voxeme> ();
								voxeme.enabled = true;
								voxeme.isGrasped = true;
								voxeme.graspTracker = agent.GetComponent<IKControl>().rightHandObj;
								voxeme.grasperCoord = agent.GetComponent<GraspScript>().rightGrasperCoord;
							}
							else {
								OutputHelper.PrintOutput(OutputController.Role.Affector,"I can't grasp the " + (arg as GameObject).name + ".  I'm not touching it."); 
							}
						}
					}
				}
			}
		}
	}

	// IN: Objects
	// OUT: none
	public void UNGRASP(object[] args)
	{
		GameObject agent = GameObject.FindGameObjectWithTag ("Agent");
		if (agent != null) {
			Animator anim = agent.GetComponentInChildren<Animator> ();
			GameObject leftGrasper = anim.GetBoneTransform (HumanBodyBones.LeftHand).transform.gameObject;
			GameObject rightGrasper = anim.GetBoneTransform (HumanBodyBones.RightHand).transform.gameObject;
			GameObject grasper = null;
			Transform leftGrasperCoord = agent.GetComponent<GraspScript>().leftGrasperCoord;
			Transform rightGrasperCoord = agent.GetComponent<GraspScript>().rightGrasperCoord;
			GraspScript graspController = agent.GetComponent<GraspScript> ();

			if (args [args.Length - 1] is bool) {
				if ((bool)args [args.Length - 1] == true) {
					foreach (object arg in args) {
						if (arg is GameObject) {
							Voxeme voxComponent = (arg as GameObject).GetComponent<Voxeme> ();
							if (voxComponent != null) {
								if (voxComponent.isGrasped) {
									//voxComponent.transform.position = voxComponent.transform.position + 
									//	(voxComponent.grasperCoord.position - voxComponent.gameObject.transform.position);

									if (voxComponent.grasperCoord == leftGrasperCoord) {
										grasper = leftGrasper;
									}
									else if (voxComponent.grasperCoord == rightGrasperCoord) {
										grasper = rightGrasper;
									}
									RiggingHelper.UnRig ((arg as GameObject), grasper);
									graspController.grasper = (int)Gestures.HandPose.Neutral;
									//agent.GetComponent<GraspScript>().isGrasping = false;
									agent.GetComponent<IKControl> ().leftHandObj.position = graspController.leftDefaultPosition;
									agent.GetComponent<IKControl> ().rightHandObj.position = graspController.rightDefaultPosition;

									voxComponent.isGrasped = false;
									voxComponent.graspTracker = null;
									voxComponent.grasperCoord = null;
								}
							}
						}
					}
				}
			}
		}
	}

	// IN: Objects
	// OUT: none
	public void DROP(object[] args)
	{
		GameObject agent = GameObject.FindGameObjectWithTag ("Agent");
		if (agent != null) {
			Animator anim = agent.GetComponentInChildren<Animator> ();
			GameObject leftGrasper = anim.GetBoneTransform (HumanBodyBones.LeftHand).transform.gameObject;
			GameObject rightGrasper = anim.GetBoneTransform (HumanBodyBones.RightHand).transform.gameObject;
			GameObject grasper = null;
			Transform leftGrasperCoord = agent.GetComponent<GraspScript>().leftGrasperCoord;
			Transform rightGrasperCoord = agent.GetComponent<GraspScript>().rightGrasperCoord;
			GraspScript graspController = agent.GetComponent<GraspScript> ();

			if (args [args.Length - 1] is bool) {
				if ((bool)args [args.Length - 1] == true) {
					foreach (object arg in args) {
						if (arg is GameObject) {
							Voxeme voxComponent = (arg as GameObject).GetComponent<Voxeme> ();
							if (voxComponent != null) {
								if (voxComponent.isGrasped) {
									//voxComponent.transform.position = voxComponent.transform.position + 
									//	(voxComponent.grasperCoord.position - voxComponent.gameObject.transform.position);

									if (voxComponent.grasperCoord == leftGrasperCoord) {
										grasper = leftGrasper;
									}
									else if (voxComponent.grasperCoord == rightGrasperCoord) {
										grasper = rightGrasper;
									}
									RiggingHelper.UnRig ((arg as GameObject), grasper);
									(arg as GameObject).GetComponent<Rigging> ().ActivatePhysics (true);
									graspController.grasper = (int)Gestures.HandPose.Neutral;
									//agent.GetComponent<GraspScript>().isGrasping = false;
									//agent.GetComponent<IKControl> ().leftHandObj.position = graspController.leftDefaultPosition;
									//agent.GetComponent<IKControl> ().rightHandObj.position = graspController.rightDefaultPosition;

									voxComponent.isGrasped = false;
									voxComponent.graspTracker = null;
									voxComponent.grasperCoord = null;
								}
							}
						}
					}
				}
			}
		}
	}

	// IN: Objects
	// OUT: none
	public void HOLD(object[] args)
	{
		// look for agent
		GameObject agent = GameObject.FindGameObjectWithTag("Agent");
		if (agent != null) {
			// add preconditions
			if (!SatisfactionTest.IsSatisfied (string.Format ("reach({0})", (args [0] as GameObject).name))) {
				eventManager.InsertEvent (string.Format ("reach({0})", (args [0] as GameObject).name), 0);
				eventManager.InsertEvent (string.Format ("grasp({0})", (args [0] as GameObject).name), 1);
				eventManager.InsertEvent (eventManager.evalOrig [string.Format ("hold({0})", (args [0] as GameObject).name)], 1);
				eventManager.RemoveEvent (3);
				return;
			}
			else {
				if (!SatisfactionTest.IsSatisfied (string.Format ("grasp({0})", (args [0] as GameObject).name))) {
					eventManager.InsertEvent (string.Format ("grasp({0})", (args [0] as GameObject).name), 0);
					eventManager.InsertEvent (eventManager.evalOrig [string.Format ("hold({0})", (args [0] as GameObject).name)], 1);
					eventManager.RemoveEvent (2);
					return;
				}
			}
		}
	}

	// IN: Objects
	// OUT: none
	public void TOUCH(object[] args)
	{
		GameObject agent = GameObject.FindGameObjectWithTag ("Agent");
		if (agent != null) {
			Animator anim = agent.GetComponentInChildren<Animator> ();
			GameObject leftGrasper = anim.GetBoneTransform (HumanBodyBones.LeftHand).transform.gameObject;
			GameObject rightGrasper = anim.GetBoneTransform (HumanBodyBones.RightHand).transform.gameObject;
			GameObject grasper;
			GraspScript graspController = agent.GetComponent<GraspScript> ();
			Transform leftGrasperCoord = graspController.leftGrasperCoord;
			Transform rightGrasperCoord = graspController.rightGrasperCoord;
			Transform leftFingerCoord = graspController.leftFingerCoord;
			Transform rightFingerCoord = graspController.rightFingerCoord;

			if (args [args.Length - 1] is bool) {
				if ((bool)args [args.Length - 1] == true) {
					foreach (object arg in args) {
						if (arg is GameObject) {
							// find bounds corner closest to grasper
							Bounds bounds = Helper.GetObjectWorldSize((arg as GameObject));

							// which hand is closer?
							Vector3 leftClosestPoint = bounds.ClosestPoint(leftGrasper.transform.position);
							Vector3 rightClosestPoint = bounds.ClosestPoint(rightGrasper.transform.position);
							float leftToGoalDist = (leftGrasper.transform.position-leftClosestPoint).magnitude;
							float rightToGoalDist = (rightGrasper.transform.position-rightClosestPoint).magnitude;

							if (leftToGoalDist < rightToGoalDist) {
								grasper = leftGrasper;
							}
							else {
								grasper = rightGrasper;
							}

							IKControl ikControl = agent.GetComponent<IKControl> ();
							if (ikControl != null) {
								Vector3 target;
								if (grasper == leftGrasper) {
									target = leftClosestPoint;
									Vector3 dir = leftFingerCoord.position - leftGrasper.transform.position;
									dir = leftGrasper.transform.rotation * dir;
									ikControl.leftHandObj.transform.position = dir + target;//-leftFingerCoord.localPosition;//-(leftGrasper.transform.rotation*leftFingerCoord.localPosition);
								}
								else {
									target = rightClosestPoint;
									ikControl.rightHandObj.transform.position = target;//-rightFingerCoord.position;
								}
							}
						}
					}
				}
			}
		}
	}

	// IN: Objects
	// OUT: bool
	public bool IF(object[] args)
	{
		bool r = false;
		//TestRelation();

		return r;
	}

	// IN: Objects
	// OUT: bool
	public bool ADD(object[] args)
	{
		bool r = false;

		return r;
	}
}
