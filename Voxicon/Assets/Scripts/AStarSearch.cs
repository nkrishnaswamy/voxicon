using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using Global;

public class AStarSearch : MonoBehaviour {
	public GameObject embeddingSpace;
	Bounds embeddingSpaceBounds;
	List<GameObject> debugVisual = new List<GameObject> ();

	Vector3 defaultIncrement = new Vector3 (1.0f, 1.0f, 1.0f);
	public Vector3 increment;
	public List<Vector3> nodes = new List<Vector3>();
	public List<Vector3> plannedPath;

	public Vector3 start = new Vector3();
	public Vector3 goal = new Vector3();
		

	// Use this for initialization
	void Start () {
		Renderer r = embeddingSpace.GetComponent<Renderer> ();
		embeddingSpaceBounds = r.bounds;
		//Debug.Log (embeddingSpaceBounds.min);
		//Debug.Log (embeddingSpaceBounds.max);
	}
	
	// Update is called once per frame
	void Update () {
		/*if ((goal - start).magnitude > 0.0f) {
			Debug.Log ("Start: " + start);
			Debug.Log ("Goal: " + goal);

			//PlanPath (start, goal, out plannedPath);

			// clear debug visualization
			foreach (GameObject o in debugVisual) {
				GameObject.Destroy(o);
			}
				
			foreach (Vector3 coord in plannedPath) {
				if (nodes.Contains(coord)) {
					AddDebugCube(coord);
				}
			}

			goal = start;	// temp hack to stop reprints
		}*/
	}

	void AddDebugCube(Vector3 coord) {
		GameObject cube = GameObject.CreatePrimitive (PrimitiveType.Cube);
		cube.transform.position = coord;
		cube.transform.localScale = new Vector3 (increment.x / 10, increment.y / 10, increment.z / 10);
		cube.tag = "UnPhysic";

		debugVisual.Add (cube);
	}

	void QuantizeSpace(Bounds embeddingSpaceBounds, Vector3 increment) {
		// fill the space with cubes!
		for (float fx = embeddingSpaceBounds.min.x; fx < embeddingSpaceBounds.max.x; fx += increment.x) {
			for (float fy = embeddingSpaceBounds.min.y; fy < embeddingSpaceBounds.max.y; fy += increment.y) {
				for (float fz = embeddingSpaceBounds.min.z; fz < embeddingSpaceBounds.max.z; fz += increment.z) {

					// create test bounding box
					Bounds testBounds = new Bounds(new Vector3 (fx+(increment.x/2), fy+(increment.y/2), fz+(increment.z/2)),
						new Vector3 (increment.x, increment.y, increment.z));
					// get all objects
					GameObject[] allObjects = UnityEngine.Object.FindObjectsOfType<GameObject>();

					bool spaceClear = true;
					foreach (GameObject obj in allObjects) {
						if ((obj.tag != "UnPhysic") && (obj.tag != "Ground")) {
							if (testBounds.Intersects (Helper.GetObjectWorldSize(obj))) {
								spaceClear = false;
								break;
							}
						}
					}

					if (spaceClear) {
						// add node
						Vector3 node = new Vector3 (fx + (increment.x / 2), fy + (increment.y / 2), fz + (increment.z / 2));
						nodes.Add(node);

						//GameObject cube = GameObject.CreatePrimitive (PrimitiveType.Cube);
						//cube.transform.position = new Vector3 (fx + (increment / 2), fy + (increment / 2), fz + (increment / 2));
						//cube.transform.localScale = new Vector3 (increment / 10, increment / 10, increment / 10);
						//cube.tag = "UnPhysic";
						//cube.GetComponent<Renderer>().enabled = false;

						//debugVisual.Add (node, cube);
					}
				}
			}
		}
	}

	public void PlanPath(Vector3 startPos, Vector3 goalPos, out List<Vector3> path, params GameObject[] objs) {
		// clear nodes
		nodes.Clear ();

		if (objs.Length > 0) {
			Vector3 size = Helper.GetObjectWorldSize (objs [0]).size;
			increment = size.magnitude > defaultIncrement.magnitude ? size : defaultIncrement;
		}
		//Debug.Log (increment);

		// get moving object bound
		QuantizeSpace (embeddingSpaceBounds, increment);	// set incremenet to moving object size, clean up after each run

		path = new List<Vector3>();
		//path.Add(startPos);

		Vector3 startNode = embeddingSpaceBounds.max+Vector3.one,
			nextNode = embeddingSpaceBounds.max+Vector3.one,
			endNode = embeddingSpaceBounds.max+Vector3.one;

		// find closest node to start
		float dist = (startNode - startPos).magnitude;
		foreach (Vector3 node in nodes) {
			if ((node - startPos).magnitude < dist) {
				dist = (node - startPos).magnitude;
				startNode = node;
			}
		}
		path.Add(startNode);
		Debug.Log (startNode);

		// find closest node to goal
		dist = (endNode - goalPos).magnitude;
		foreach (Vector3 node in nodes) {
			if ((node - goalPos).magnitude < dist) {
				dist = (node - goalPos).magnitude;
				endNode = node;
			}
		}

		// starting with startNode, for each neighborhood node of last node, assess A* heuristic
		// using best node found until endNode reached
		while (path [path.Count - 1] != endNode) {
			//nextNode = embeddingSpaceBounds.max + Vector3.one;
			//dist = (nextNode - endNode).magnitude;
			dist = ((embeddingSpaceBounds.max + Vector3.one)-endNode).magnitude;
			foreach (Vector3 node in nodes) {
				if (((node - path [path.Count - 1]).x <= increment.x) &&
					((node - path [path.Count - 1]).y <= increment.y) &&
					((node - path [path.Count - 1]).z <= increment.z)){ // look for nodes $increment units from the current
				//if ((node - path [path.Count - 1]).magnitude <= increment.magnitude) { // look for nodes $increment units from the current
					if ((Vector3.Dot ((node - path [path.Count - 1]).normalized, Vector3.right) == -1.0f) ||
						(Vector3.Dot ((node - path [path.Count - 1]).normalized, Vector3.right) == 0.0f) ||
						(Vector3.Dot ((node - path [path.Count - 1]).normalized, Vector3.right) == 1.0f)){	// orthogonal movements only for now
						//if (!Physics.Raycast (new Ray (path [path.Count - 1], (node - path [path.Count - 1]).normalized), (node - path [path.Count - 1]).magnitude)) {
							if ((node - endNode).magnitude < dist) {	// heuristic here: linear distance
								dist = (node - endNode).magnitude;
								nextNode = node;
							}
						//}
					}
				}
			}
			path.Add (nextNode);
			Debug.Log (nextNode);
			//AddDebugCube (nextNode);
		} 

		//path.Add(endNode);
		//Debug.Log (endNode);

		path.Add(goalPos);
	}
}
