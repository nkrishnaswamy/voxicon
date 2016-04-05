using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Global {
	/// <summary>
	/// Triple class
	/// </summary>
	public class Triple<T1, T2, T3> : IEquatable<System.Object>{
		public T1 Item1{
			get;
			set;
		}
		
		public T2 Item2{
			get;
			set;
		}
		
		public T3 Item3{
			get;
			set;
		}
		
		public Triple(T1 Item1, T2 Item2, T3 Item3){
			this.Item1 = Item1;
			this.Item2 = Item2;
			this.Item3 = Item3;
		}
		
		public override bool Equals(object obj) {
			if ( obj == null || (obj as Triple<T1, T2, T3>) == null ) //if the object is null or the cast fails
				return false;
			else {
				Triple<T1,T2,T3> tuple = (Triple<T1, T2, T3>) obj;
				return Item1.Equals(tuple.Item1) && Item2.Equals(tuple.Item2) && Item3.Equals(tuple.Item3);
			}
		}
		
		public override int GetHashCode() {
			return Item1.GetHashCode() ^ Item2.GetHashCode() ^ Item3.GetHashCode();
		}
		
		public static bool operator == (Triple<T1, T2, T3> tuple1, Triple<T1, T2, T3> tuple2) {
			return tuple1.Equals(tuple2);
		}
		
		public static bool operator != (Triple<T1, T2, T3> tuple1, Triple<T1, T2, T3> tuple2) {
			return !tuple1.Equals(tuple2);
		}
	}

	/// <summary>
	/// Helper class
	/// </summary>
	public static class Helper {
		public static Regex v = new Regex (@"<.*>");

		public static Hashtable ParsePredicate(String predicate) {
			Hashtable predArgs = new Hashtable ();
			
			Queue<String> split = new Queue<String>(predicate.Split (new char[] {'('},2,StringSplitOptions.None));
			if (split.Count > 1) {
				String pred = split.ElementAt (0);
				String args = split.ElementAt (1);
				args = args.Remove (args.Length - 1);
				predArgs.Add (pred, args);
			}
			
			return predArgs;
		}

		public static String GetTopPredicate(String formula) {
			Queue<String> split = new Queue<String>(formula.Split (new char[] {'('},2,StringSplitOptions.None));
			return split.ElementAt (0);
		}

		public static void PrintKeysAndValues(Hashtable ht)  {
			foreach (DictionaryEntry entry in ht)
				Debug.Log(entry.Key + " : " + entry.Value);
		}

		public static String VectorToParsable(Vector3 vector) {
			return ("<"+vector.x.ToString ()+"; "+
			       	vector.y.ToString ()+"; "+
			        vector.z.ToString ()+">");
		}
		
		public static Vector3 ParsableToVector(String parsable) {
			List<String> components = new List<String> ((((String)parsable).Replace("<","").Replace(">","")).Split (new char[] {';'}));
			return new Vector3(System.Convert.ToSingle(components[0]),
			                   System.Convert.ToSingle(components[1]),
			                   System.Convert.ToSingle(components[2]));
		}

		public static String QuaternionToParsable(Quaternion quat) {
			return ("<"+quat.x.ToString ()+"; "+
			        quat.y.ToString ()+"; "+
			        quat.z.ToString ()+"; "+
			        quat.w.ToString ()+">");
		}
		
		public static Quaternion ParsableToQuaternion(String parsable) {
			List<String> components = new List<String> ((((String)parsable).Replace("<","").Replace(">","")).Split (new char[] {';'}));
			return new Quaternion(System.Convert.ToSingle(components[0]),
								System.Convert.ToSingle(components[1]),
			                  	System.Convert.ToSingle(components[2]),
			                  	System.Convert.ToSingle(components[3]));
		}

		public static String IntToString(int inInt) {
			return System.Convert.ToString(inInt);
		}

		public static int StringToInt(String inString) {
			return System.Convert.ToInt16(inString);
		}

		public static Triple<String,String,String> MakeRDFTriples(String formula) {		// fix for multiple RDF triples
			Triple<String,String,String> triple = new Triple<String,String,String> ("","","");
			Debug.Log ("MakeRDFTriple: " + formula);
			String[] components = formula.Replace ('(', '/').Replace (')', '/').Replace (',','/').Split ('/');
			if (components.Length <= 3) {
				triple.Item1 = "NULL";
			}

			foreach (String s in components) {
				if (s.Length > 0) {
					//Debug.Log (s);
					GameObject obj = GameObject.Find (s);
					if (obj != null) {
						if (triple.Item1 == "") {
							triple.Item1 = s;
						} else if (triple.Item3 == "") {
							triple.Item3 = s;
						}
					} else {
						triple.Item2 = triple.Item2 + s + "_";
					}
				}
			}
			triple.Item2 = triple.Item2.Remove (triple.Item2.Length - 1, 1);
			//Debug.Log (triple.Item1 + " " + triple.Item2 + " " + triple.Item3);
			return triple;
		}

		public static void PrintRDFTriples(List<Triple<String,String,String>> triples) {
			foreach (Triple<String,String,String> triple in triples) {
				Debug.Log (triple.Item1 + " " + triple.Item2 + " " + triple.Item3);
			}
		}

		public static bool VectorIsNaN(Vector3 vec) {
			return (float.IsNaN (vec.x) || float.IsNaN (vec.y) || float.IsNaN (vec.z));
		}

		public static bool PointInRect(Vector2 point, Rect rect) {
			return (point.x >= rect.xMin && point.x <= rect.xMax && point.y >= rect.yMin && point.y <= rect.yMax);
		}

		public static bool PointOutsideMaskedAreas(Vector2 point, Rect[] rects) {
			bool outside = true;
			
			foreach (Rect r in rects) {
				if (PointInRect(point,r)) {
					outside = false;
				}
			}
			
			return outside;
		}

		public static Bounds GetObjectSize(GameObject obj) {
			Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();

			Bounds combinedBounds = new Bounds (obj.transform.position, Vector3.zero);
			
			foreach (Renderer renderer in renderers) {
				Bounds temp = renderer.gameObject.GetComponent<MeshFilter> ().mesh.bounds;
				Vector3 min = new Vector3 (temp.min.x * renderer.gameObject.transform.localScale.x,
					              temp.min.y * renderer.gameObject.transform.localScale.y,
					              temp.min.z * renderer.gameObject.transform.localScale.z);
				Vector3 max = new Vector3 (temp.max.x * renderer.gameObject.transform.localScale.x,
					temp.max.y * renderer.gameObject.transform.localScale.y,
					temp.max.z * renderer.gameObject.transform.localScale.z);
				temp.SetMinMax (min, max);
				combinedBounds.Encapsulate(temp);
			}
			
			return combinedBounds;
		}

		// get the bounds of the object in the current world
		public static Bounds GetObjectWorldSize(GameObject obj) {
			Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();

			Bounds combinedBounds = new Bounds (obj.transform.position, Vector3.zero);

			foreach (Renderer renderer in renderers) {
				combinedBounds.Encapsulate(renderer.bounds);
			}

			return combinedBounds;
		}

		// get the bounds of the object in the current world, excluding any children listed
		public static Bounds GetObjectWorldSize(GameObject obj, List<GameObject> exclude) {
			Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();

			Bounds combinedBounds = new Bounds (obj.transform.position, Vector3.zero);

			foreach (Renderer renderer in renderers) {
				if (!exclude.Contains (renderer.transform.gameObject)) {
					combinedBounds.Encapsulate (renderer.bounds);
				}
			}

			return combinedBounds;
		}

		// get the collective bounds of the objects in the current world
		public static Bounds GetObjectWorldSize(List<GameObject> objs) {
			Bounds combinedBounds = new Bounds (objs[0].transform.position, Vector3.zero);

			foreach (GameObject obj in objs) {
				Renderer[] renderers = obj.GetComponentsInChildren<Renderer> ();

				foreach (Renderer renderer in renderers) {
					combinedBounds.Encapsulate (renderer.bounds);
				}
			}

			return combinedBounds;
		}

		public static Bounds GetObjectSize(object[] objs) {
			Bounds starterBounds = (objs [0] as GameObject).GetComponentsInChildren<BoxCollider> ()[0].bounds;
			Bounds combinedBounds = starterBounds;
			//Debug.Log (combinedBounds.size);

			foreach (object obj in objs) {
				if (obj is GameObject) {
					//Debug.Log ((obj as GameObject).name);
					Renderer[] renderers = (obj as GameObject).GetComponentsInChildren<Renderer> ();

					foreach (Renderer renderer in renderers) {
						//Bounds bounds = new Bounds(renderer.bounds.center,
						//                           renderer.bounds.size);
						//localBounds.center = renderer.gameObject.transform.position;
						//localBounds.size = new Vector3(localBounds.size.x * renderer.gameObject.transform.localScale.x,
						//                               localBounds.size.y * renderer.gameObject.transform.localScale.y,
						//                               localBounds.size.z * renderer.gameObject.transform.localScale.z);
						combinedBounds.Encapsulate (renderer.bounds);

						//Debug.Log (combinedBounds.center);
						//Debug.Log (combinedBounds.size);
					}
				}
			}
			
			return combinedBounds;
		}

		public static Vector3 GetObjectMajorAxis (GameObject obj) {
			Bounds bounds = GetObjectSize (obj);

			List<float> dims = new List<float>(new float[]{bounds.size.x, bounds.size.y, bounds.size.z});

			int longest = dims.IndexOf(dims.Max());

			Vector3 axis = Vector3.zero;
			if (longest == 0) {			// x
				axis = Vector3.right;
			}
			else if (longest == 1) {	// y
				axis = Vector3.up;
			}
			else if (longest == 2) {	// z
				axis = Vector3.forward;
			}

			return axis;
		}

		// if obj1 fits inside obj2
		public static bool FitsIn(Bounds obj1, Bounds obj2) {
			bool fits = true;

			if ((obj1.size.x >= obj2.size.x) ||	// check for object bounds exceeding along all axes but the axis of major orientaton
				//(obj1.size.y > obj2.size.y) ||
			    (obj1.size.z >= obj2.size.z)) {
				fits = false;
			}

			return fits;
		}

		public static GameObject GetMostImmediateParentVoxeme(GameObject obj) {
			GameObject voxObject = obj;

			while (voxObject.transform.parent != null) {
				voxObject = voxObject.transform.parent.gameObject;
				if (voxObject.GetComponent<Rigging> () != null) {
					if (voxObject.GetComponent<Rigging> ().enabled) {
						break;
					}
				}
			}

			return voxObject;
		}
	}
}
