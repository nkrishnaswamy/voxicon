using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using Global;
using RCC;

/// <summary>
/// Semantics of each predicate should be explicated within the method itself
/// Could have an issue when it comes to functions for predicates of multiple valencies?
/// *Cannot have objects or subobjects named the same as any of these predicates*
/// </summary>

public class Predicates : MonoBehaviour {
	public List<Triple<String,String,String>> rdfTriples = new List<Triple<String,String,String>>();
	EventManager eventManager;
	AStarSearch aStarSearch;
	ObjectSelector objSelector;

	void Start () {
		eventManager = gameObject.GetComponent<EventManager> ();
		aStarSearch = GameObject.Find ("BlocksWorld").GetComponent<AStarSearch> ();
		objSelector = GameObject.Find ("BlocksWorld").GetComponent<ObjectSelector> ();
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
		return ((GameObject)args[0]).transform.position;
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
	public Vector3 BEHIND(object[] args)
	{
		Vector3 outValue = Vector3.zero;
		if (args [0] is GameObject) {	// behind an object
			GameObject obj = ((GameObject)args [0]);

			Bounds bounds = new Bounds ();

			bounds = Helper.GetObjectWorldSize (obj);

			GameObject camera = GameObject.Find ("Main Camera");
			float povDir = camera.transform.eulerAngles.y;
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
			float povDir = camera.transform.eulerAngles.y;
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
			float povDir = camera.transform.eulerAngles.y;
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
			float povDir = camera.transform.eulerAngles.y;
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
	{
		return ((GameObject)args[0]).transform.position;
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

		List<Voxeme> attrObjs = objSelector.allVoxemes.FindAll (v => v.gameObject.GetComponent<AttributeSet> ().attributes.Contains ("brown"));
		if (attrObjs.Count > 0) {
			objName = attrObjs [0].gameObject.name;
		}

		return objName;
	}

	public String BLUE(object[] args) {
		String objName = "";

		List<Voxeme> attrObjs = objSelector.allVoxemes.FindAll (v => v.gameObject.GetComponent<AttributeSet> ().attributes.Contains ("blue"));
		if (attrObjs.Count > 0) {
			objName = attrObjs [0].gameObject.name;
		}

		return objName;
	}

	public String BLACK(object[] args) {
		String objName = "";

		List<Voxeme> attrObjs = objSelector.allVoxemes.FindAll (v => v.gameObject.GetComponent<AttributeSet> ().attributes.Contains ("black"));
		if (attrObjs.Count > 0) {
			objName = attrObjs [0].gameObject.name;
		}

		return objName;
	}

	public String GREEN(object[] args) {
		String objName = "";

		List<Voxeme> attrObjs = objSelector.allVoxemes.FindAll (v => v.gameObject.GetComponent<AttributeSet> ().attributes.Contains ("green"));
		if (attrObjs.Count > 0) {
			objName = attrObjs [0].gameObject.name;
		}

		return objName;
	}

	public String YELLOW(object[] args) {
		String objName = "";

		List<Voxeme> attrObjs = objSelector.allVoxemes.FindAll (v => v.gameObject.GetComponent<AttributeSet> ().attributes.Contains ("yellow"));
		if (attrObjs.Count > 0) {
			objName = attrObjs [0].gameObject.name;
		}

		return objName;
	}

	public String RED(object[] args) {
		String objName = "";

		List<Voxeme> attrObjs = objSelector.allVoxemes.FindAll (v => v.gameObject.GetComponent<AttributeSet> ().attributes.Contains ("red"));
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
		EventManager em = GameObject.Find ("BehaviorController").GetComponent<EventManager> ();

		// override physics rigging
		foreach (object arg in args) {
			if (arg is GameObject) {
				(arg as GameObject).GetComponent<Rigging> ().ActivatePhysics(false);
			}
		}

		Vector3 targetPosition = Vector3.zero;
		Vector3 targetRotation = Vector3.zero;

		Helper.PrintRDFTriples (rdfTriples);

		string prep = rdfTriples [0].Item2.Replace ("put", "");

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
					Debug.Log ("Y-size = " + (themeBounds.center.y-themeBounds.min.y));
					Debug.Log ("put_on: " + (theme.transform.position.y - themeBounds.min.y).ToString ());

					// compose computed on(a) into put(x,y) formula
					// if the glove don't fit, you must acquit! (recompute)
					Vector3 loc = ((Vector3)args [1]);	// coord of "on"
					if (dest.GetComponent<Voxeme> ().voxml.Type.Concavity == "Concave") {
						if (!Helper.FitsIn(themeBounds,destBounds)) {
							loc = new Vector3 (dest.transform.position.x,
								destBounds.max.y,
								dest.transform.position.z);
							Debug.Log (destBounds.max.y);
						}
					}

					if (args[args.Length-1] is bool) {
						if ((bool)args [args.Length - 1] == false) {
							targetPosition = new Vector3 (loc.x,
								loc.y + (themeBounds.center.y - themeBounds.min.y) + yAdjust,
								loc.z);
						}
						else {
							targetPosition = loc;
						}
						Debug.Log (Helper.VectorToParsable(targetPosition));
						theme.GetComponent<Voxeme> ().targetPosition = targetPosition;
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
					Debug.Log ("Y-size = " + (themeBounds.center.y-themeBounds.min.y));
					Debug.Log ("put_in: " + (theme.transform.position.y - themeBounds.min.y).ToString ());

					// compose computed in(a) into put(x,y) formula
					Vector3 loc = ((Vector3)args [1]);	// coord of "in"
					if ((dest.GetComponent<Voxeme> ().voxml.Type.Concavity == "Concave") &&
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

					theme.GetComponent<Voxeme> ().targetPosition = targetPosition;
					theme.GetComponent<Voxeme> ().targetRotation = targetRotation;
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
					float povDir = mainCamera.transform.eulerAngles.y;

					float zAdjust = (theme.transform.position.z - themeBounds.center.z);

					Vector3 rayStart = new Vector3 (0.0f, 0.0f,
						Mathf.Abs(themeBounds.size.z));
					rayStart = Quaternion.Euler (0.0f, povDir+180.0f, 0.0f) * rayStart;
					rayStart += theme.transform.position;
					Vector3 contactPoint = Helper.RayIntersectionPoint (rayStart, theme);

					Debug.Log ("Z-adjust = " + zAdjust);
					Debug.Log ("put_behind: " + Helper.VectorToParsable (contactPoint));

					Vector3 loc = ((Vector3)args [1]);	// coord of "behind"

					if (args[args.Length-1] is bool) {
						if ((bool)args [args.Length - 1] == false) {	// compute satisfaction condition
							Vector3 dir = new Vector3 (loc.x - (contactPoint.x-theme.transform.position.x),
								loc.y - (contactPoint.y-theme.transform.position.y),
								loc.z - (contactPoint.z-theme.transform.position.z) + zAdjust) - loc;

							targetPosition = dir+loc;
						}
						else {
							targetPosition = loc;
						}

						Debug.Log (Helper.VectorToParsable(targetPosition));
						theme.GetComponent<Voxeme> ().targetPosition = targetPosition;
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
					float povDir = mainCamera.transform.eulerAngles.y;

					float zAdjust = (theme.transform.position.z - themeBounds.center.z);

					Vector3 rayStart = new Vector3 (0.0f, 0.0f,
						Mathf.Abs(themeBounds.size.z));
					rayStart = Quaternion.Euler (0.0f, povDir, 0.0f) * rayStart;
					rayStart += theme.transform.position;
					Vector3 contactPoint = Helper.RayIntersectionPoint (rayStart, theme);

					Debug.Log ("Z-adjust = " + zAdjust);
					Debug.Log ("put_in_front: " + Helper.VectorToParsable (contactPoint));

					Vector3 loc = ((Vector3)args [1]);	// coord of "in front"

					if (args[args.Length-1] is bool) {
						if ((bool)args [args.Length - 1] == false) {	// compute satisfaction condition
							Vector3 dir = new Vector3 (loc.x - (contactPoint.x-theme.transform.position.x),
								loc.y - (contactPoint.y-theme.transform.position.y),
								loc.z - (contactPoint.z-theme.transform.position.z) + zAdjust) - loc;

							targetPosition = dir+loc;
						}
						else {
							targetPosition = loc;
						}

						Debug.Log (Helper.VectorToParsable(targetPosition));
						theme.GetComponent<Voxeme> ().targetPosition = targetPosition;
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
					float povDir = mainCamera.transform.eulerAngles.y;

					float xAdjust = (theme.transform.position.x - themeBounds.center.x);

					Vector3 rayStart = new Vector3 (0.0f, 0.0f,
						Mathf.Abs(themeBounds.size.z));
					rayStart = Quaternion.Euler (0.0f, povDir+90.0f, 0.0f) * rayStart;
					rayStart += theme.transform.position;
					Vector3 contactPoint = Helper.RayIntersectionPoint (rayStart, theme);

					Debug.Log ("X-adjust = " + xAdjust);
					Debug.Log ("put_left: " + Helper.VectorToParsable (contactPoint));

					Vector3 loc = ((Vector3)args [1]);	// coord of "left"

					if (args[args.Length-1] is bool) {
						if ((bool)args [args.Length - 1] == false) {	// compute satisfaction condition
							Vector3 dir = new Vector3 (loc.x - (contactPoint.x-theme.transform.position.x) + xAdjust,
								loc.y - (contactPoint.y-theme.transform.position.y),
								loc.z - (contactPoint.z-theme.transform.position.z)) - loc;

							targetPosition = dir+loc;
						}
						else {
							targetPosition = loc;
						}

						Debug.Log (Helper.VectorToParsable(targetPosition));
						theme.GetComponent<Voxeme> ().targetPosition = targetPosition;
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
					float povDir = mainCamera.transform.eulerAngles.y;

					float xAdjust = (theme.transform.position.x - themeBounds.center.x);

					Vector3 rayStart = new Vector3 (0.0f, 0.0f,
						Mathf.Abs(themeBounds.size.z));
					rayStart = Quaternion.Euler (0.0f, povDir+270.0f, 0.0f) * rayStart;
					rayStart += theme.transform.position;
					Vector3 contactPoint = Helper.RayIntersectionPoint (rayStart, theme);

					Debug.Log ("X-adjust = " + xAdjust);
					Debug.Log ("put_right: " + Helper.VectorToParsable (contactPoint));

					Vector3 loc = ((Vector3)args [1]);	// coord of "left"

					if (args[args.Length-1] is bool) {
						if ((bool)args [args.Length - 1] == false) {
							Vector3 dir = new Vector3 (loc.x - (contactPoint.x-theme.transform.position.x) + xAdjust,
								loc.y - (contactPoint.y-theme.transform.position.y),
								loc.z - (contactPoint.z-theme.transform.position.z)) - loc;

							targetPosition = dir+loc;
						}
						else {
							targetPosition = loc;
						}
						Debug.Log (Helper.VectorToParsable(targetPosition));
						theme.GetComponent<Voxeme> ().targetPosition = targetPosition;
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
			if (aStarSearch.plannedPath.Count == 0) {
				aStarSearch.start = (args [0] as GameObject).transform.position;
				aStarSearch.goal = targetPosition;
				aStarSearch.PlanPath (aStarSearch.start, aStarSearch.goal, out aStarSearch.plannedPath, (args [0] as GameObject));

				foreach (Vector3 node in aStarSearch.plannedPath) {
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
					obj.GetComponent<Voxeme> ().targetPosition = targetPosition;
				}
			}
		}
		return;
	}

	// IN: Objects
	// OUT: none
	public void LIFT(object[] args)
	{
		// override physics rigging
		foreach (object arg in args) {
			if (arg is GameObject) {
				(arg as GameObject).GetComponent<Rigging> ().ActivatePhysics(false);
			}
		}

		Vector3 targetPosition = Vector3.zero;

		if (args [0] is GameObject) {
			GameObject obj = (args [0] as GameObject);
			targetPosition = new Vector3 (obj.transform.position.x,obj.transform.position.y+UnityEngine.Random.value, obj.transform.position.z);
			//Debug.Log (targetPosition);
			//targetPosition = new Vector3 (obj.transform.position.x+1.0f, obj.transform.position.y, obj.transform.position.z);
			obj.GetComponent<Voxeme> ().targetPosition = targetPosition;
		}

		// add to events manager
		if (args[args.Length-1] is bool) {
			if ((bool)args[args.Length-1] == false) {
				eventManager.events[0] = "lift("+(args [0] as GameObject).name+","+Helper.VectorToParsable(targetPosition)+")";
			}
		}

		return;
	}

	// IN: Objects
	// OUT: none
	public void SLIDE(object[] args)
	{
		// override physics rigging
		/*foreach (object arg in args) {
			if (arg is GameObject) {
				(arg as GameObject).GetComponent<Rigging> ().ActivatePhysics(false);
			}
		}*/

		Vector3 targetPosition = Vector3.zero;

		if (args [0] is GameObject) {
			GameObject obj = (args [0] as GameObject);
			targetPosition = new Vector3 (obj.transform.position.x+UnityEngine.Random.insideUnitSphere.x,
				obj.transform.position.y, obj.transform.position.z+UnityEngine.Random.insideUnitSphere.z);
			//Debug.Log (targetPosition);
			//targetPosition = new Vector3 (obj.transform.position.x+1.0f, obj.transform.position.y, obj.transform.position.z);
			obj.GetComponent<Voxeme> ().targetPosition = targetPosition;
		}

		// add to events manager
		if (args[args.Length-1] is bool) {
			if ((bool)args[args.Length-1] == false) {
				eventManager.events[0] = "slide("+(args [0] as GameObject).name+","+Helper.VectorToParsable(targetPosition)+")";
			}
		}

		return;
	}

	// IN: Objects, Location
	// OUT: none
	public void ROLL(object[] args)
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
					obj.GetComponent<Voxeme> ().targetPosition = targetPosition;
				}
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
			obj.GetComponent<Voxeme> ().targetRotation = targetRotation;
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
	{	// it no work
		foreach (object obj in args) {
			if (obj is GameObject) {
				foreach (Renderer renderer in (obj as GameObject).GetComponentsInChildren<Renderer>()) {
					renderer.enabled = true;
				}
			}
		}
	}

	// IN: Objects
	// OUT: none
	public void DISABLE(object[] args)
	{
		foreach (object obj in args) {
			if (obj is GameObject) {
				foreach (Renderer renderer in (obj as GameObject).GetComponentsInChildren<Renderer>()) {
					renderer.enabled = false;
				}
			}
		}
	}

	/* AGENT-DEPENDENT BEHAVIORS */

	// IN: Objects
	// OUT: none
	public void REACH(object[] args)
	{
		GameObject agent = GameObject.FindGameObjectWithTag ("Agent");
		if (agent != null) {
			Animator anim = agent.GetComponent<Animator> ();
			GameObject grasper = anim.GetBoneTransform (HumanBodyBones.RightHand).transform.gameObject;
			//anim["Grasp_3"].wrapMode = WrapMode.Once;
			if (args [args.Length - 1] is bool) {
				if ((bool)args [args.Length - 1] == true) {
					foreach (object arg in args) {
						if (arg is GameObject) {
							// find bounds corner closest to grasper
							Bounds bounds = Helper.GetObjectWorldSize((arg as GameObject));
							Vector3 target = bounds.ClosestPoint(grasper.transform.position);
							target = new Vector3 (bounds.max.x, bounds.center.y, bounds.center.z);
							GameObject.Find ("ReachObject").transform.position = target;
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
			Animator anim = agent.GetComponentInChildren<Animator> ();
			GameObject grasper = anim.GetBoneTransform (HumanBodyBones.RightHand).gameObject;
			//GameObject grasper = GameObject.Find("hand.R");
			//anim["Grasp_3"].wrapMode = WrapMode.Once;
			if (args [args.Length - 1] is bool) {
				if ((bool)args [args.Length - 1] == true) {
					foreach (object arg in args) {
						if (arg is GameObject) {
							//BoxCollider collider = grasper.GetComponent<BoxCollider> ();
							//Debug.Log (collider.transform.position);
							GameObject grasperCoord = GameObject.Find ("GrasperCoord");
							//grasperCoord.transform.position = grasper.transform.position;
							//Debug.Log(grasperCoord.transform.position);
							//Debug.Log (GameObject.Find ("ReachObject").transform.position);
							//Debug.Log((arg as GameObject).transform.position);
							//Debug.Log((grasper.transform.position - (arg as GameObject).transform.position).magnitude);
							//Debug.Log (Helper.GetObjectWorldSize ((arg as GameObject)).max);
							//Debug.Log (Helper.GetObjectWorldSize ((arg as GameObject)).center);
							//Debug.Log ((Helper.GetObjectWorldSize ((arg as GameObject)).max - Helper.GetObjectWorldSize ((arg as GameObject)).center).magnitude);
							if ((grasperCoord.transform.position - (arg as GameObject).transform.position).magnitude <
							    (Helper.GetObjectWorldSize ((arg as GameObject)).max - Helper.GetObjectWorldSize ((arg as GameObject)).center).magnitude) {
								//if (RCC8.EC (Helper.GetObjectWorldSize((arg as GameObject)), Helper.GetObjectWorldSize(grasper)) ||	// do actual touching test
								//	RCC8.PO (Helper.GetObjectWorldSize((arg as GameObject)), Helper.GetObjectWorldSize(grasper))) {
								anim.Play ("Grasp_2");
								anim.SetInteger ("grasp", 2);
								(arg as GameObject).GetComponent<Rigging> ().ActivatePhysics (false);
								RiggingHelper.RigTo ((arg as GameObject), grasperCoord);
								(arg as GameObject).GetComponent<Voxeme> ().enabled = true;
								(arg as GameObject).GetComponent<Voxeme> ().isGrasped = true;
							}
							else {
								OutputHelper.PrintOutput("I can't grasp the " + (arg as GameObject).name + ".  I'm not touching it."); 
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
			Animator anim = agent.GetComponent<Animator> ();
			if (args [args.Length - 1] is bool) {
				if ((bool)args [args.Length - 1] == true) {
					anim.CrossFade ("New State",0.2f);
					anim.SetInteger ("grasp", 0);
					foreach (object arg in args) {
						if (arg is GameObject) {
							GameObject grasperCoord = GameObject.Find ("GrasperCoord");
							GameObject.Find ("ReachObject").transform.position = grasperCoord.transform.position;
							//if ((arg as GameObject).transform.IsChildOf (anim.GetBoneTransform (HumanBodyBones.RightHand).transform)) {
							if ((arg as GameObject).transform.IsChildOf (grasperCoord.transform)) {
								(arg as GameObject).GetComponent<Rigging> ().ActivatePhysics (true);
								//RiggingHelper.UnRig ((arg as GameObject), anim.GetBoneTransform (HumanBodyBones.RightHand).transform.gameObject);
								RiggingHelper.UnRig ((arg as GameObject), grasperCoord);
								(arg as GameObject).GetComponent<Voxeme> ().isGrasped = false;
							}
							else {
								OutputHelper.PrintOutput("I can't drop the " + (arg as GameObject).name + ".  I'm not holding it."); 
							}
						}
					}
				}
			}
		}
	}
}
