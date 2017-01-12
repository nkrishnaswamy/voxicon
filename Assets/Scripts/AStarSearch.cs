using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using Global;

public class PathNode {
	public Vector3 position;
	public bool examined;
	public float scoreFromStart;
	public float heuristicScore;
	public PathNode cameFrom;

	public PathNode(Vector3 pos) {
		position.x = pos.x;
		position.y = pos.y;
		position.z = pos.z;
		examined = false;
		scoreFromStart = Mathf.Infinity;
		heuristicScore = Mathf.Infinity;
	}
}

public class AStarSearch : MonoBehaviour {
	public bool debugNodes = false;

	public GameObject embeddingSpace;
	Bounds embeddingSpaceBounds;
	List<GameObject> debugVisual = new List<GameObject> ();

	public Vector3 defaultIncrement = Vector3.one;
	public Vector3 increment;
	public List<PathNode> nodes = new List<PathNode>();
	public List<Pair<PathNode,PathNode>> arcs = new List<Pair<PathNode,PathNode>> ();
	public List<Vector3> path;

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

	void QuantizeSpace(GameObject obj, Bounds embeddingSpaceBounds, Vector3 increment, params object[] constraints) {
		// fill the space with boxes!
		float xStart, yStart, zStart;
		float xEnd, yEnd, zEnd;
		Vector3 origin = obj.transform.position;
		Bounds objBounds = Helper.GetObjectWorldSize (obj);
		Vector3 originToCenterOffset = objBounds.center - origin;

		for (xStart = origin.x; xStart > embeddingSpaceBounds.min.x; xStart -= increment.x) {}
		xEnd = embeddingSpaceBounds.max.x;

		for (yStart = origin.y; yStart > embeddingSpaceBounds.min.y; yStart -= increment.y) {}
		yEnd = embeddingSpaceBounds.max.y;

		for (zStart = origin.z; zStart > embeddingSpaceBounds.min.z; zStart -= increment.z) {}
		zEnd = embeddingSpaceBounds.max.z;

		if (constraints.Length > 0) {
			foreach (object constraint in constraints) {
				if (constraint is Bounds) {
					xStart = ((Bounds)constraint).min.x;
					xEnd = ((Bounds)constraint).max.x;

					yStart = ((Bounds)constraint).min.y;
					yEnd = ((Bounds)constraint).max.y;

					zStart = ((Bounds)constraint).min.z;
					zEnd = ((Bounds)constraint).max.z;
				}
				else if (constraint is string) {
					if ((constraint as string).Contains ('X')) {
						xStart = origin.x;
						xEnd = origin.x + increment.x;
					}

					if ((constraint as string).Contains ('Y')) {
						yStart = origin.y;
						yEnd = origin.y + increment.y;
					}

					if ((constraint as string).Contains ('Z')) {
						zStart = origin.z;
						zEnd = origin.z + increment.z;
					}
				}
			}
		}
			
		for (float fx = xStart; fx < xEnd; fx += increment.x) {
			for (float fy = yStart; fy < yEnd; fy += increment.y) {
				for (float fz = zStart; fz < zEnd; fz += increment.z) {

					// create test bounding box
					//Bounds testBounds = new Bounds(new Vector3 (fx+(increment.x/2), fy+(increment.y/2), fz+(increment.z/2)),
					//	new Vector3 (increment.x, increment.y, increment.z));

					Bounds testBounds = new Bounds(new Vector3 (fx,fy,fz)+originToCenterOffset,objBounds.size);
					// get all objects
					GameObject[] allObjects = UnityEngine.Object.FindObjectsOfType<GameObject>();

					bool spaceClear = true;
					foreach (GameObject o in allObjects) {
						if ((o.tag != "UnPhysic") && (o.tag != "Ground")) {
							if (testBounds.Intersects (Helper.GetObjectWorldSize(o))) {
								spaceClear = false;
								break;
							}
						}
					}

					if (spaceClear) {
						// add node
						//Vector3 node = new Vector3 (fx + (increment.x / 2), fy + (increment.y / 2), fz + (increment.z / 2));
						Vector3 node = new Vector3 (fx, fy, fz);
						nodes.Add(new PathNode(node));

						if (debugNodes) {
							GameObject cube = GameObject.CreatePrimitive (PrimitiveType.Cube);
							cube.transform.position = node;
							cube.transform.localScale = new Vector3 (increment.x / 10, increment.y / 10, increment.z / 10);
							cube.tag = "UnPhysic";
							cube.GetComponent<Renderer> ().enabled = true;
							Destroy (cube.GetComponent<Collider> ());

							debugVisual.Add (cube);
						}
					}
				}
			}
		}
	}

	void PlotArcs(Bounds objBounds) {
		RaycastHit hitInfo;
		for (int i = 0; i < nodes.Count-1; i++) {
			for (int j = i+1; j < nodes.Count; j++) {
				Vector3 dir = (nodes [j].position - nodes [i].position);
				float dist = dir.magnitude;
				bool r = Physics.Raycast (nodes [i].position, dir.normalized, out hitInfo, dist);
				r |= Physics.Raycast (nodes [i].position-objBounds.extents, dir.normalized, out hitInfo, dist);
				r |= Physics.Raycast (nodes [i].position+objBounds.extents, dir.normalized, out hitInfo, dist);
				if (!r) {
					arcs.Add (new Pair<PathNode, PathNode> (nodes [i], nodes [j]));

//					if (debugNodes) {
//						Debug.DrawRay (nodes [i].position, dir);
//					}
				}
//				else {
//					if (false) {}
//					//if (Helper.ContainingObjects (goal).Contains (hitInfo.collider.gameObject)) {
//					//	arcs.Add (new Pair<Vector3, Vector3> (nodes [i], nodes [j]));
//					//}
//				}
			}
		}
	}

	public void PlanPath(Vector3 startPos, Vector3 goalPos, out List<Vector3> path, GameObject obj, params object[] constraints) {
		// clear nodes
		nodes.Clear ();

		// init empty path
		List<PathNode> plannedPath = new List<PathNode>();

		List<PathNode> openSet = new List<PathNode> ();
		List<PathNode> closedSet = new List<PathNode> ();

		PathNode endNode = null;

		path = new List<Vector3>();

		Vector3 size = Helper.GetObjectWorldSize (obj).size;
		increment = size.magnitude > defaultIncrement.magnitude ? size : defaultIncrement;

		//Debug.Log (increment);

		PathNode startNode = new PathNode (startPos);
		startNode.scoreFromStart = 0;
		startNode.heuristicScore = Mathf.Abs(goalPos.x - startNode.position.x) + Mathf.Abs(goalPos.y - startNode.position.y) +
			Mathf.Abs(goalPos.z - startNode.position.z);
		nodes.Add(startNode);
		openSet.Add (startNode);
		QuantizeSpace (obj, embeddingSpaceBounds, increment, constraints);	// set increment to moving object size, clean up after each run

		// find closest node to goal
		float dist = Mathf.Infinity;
		foreach (PathNode node in nodes) {
			if ((node.position - goalPos).magnitude < dist) {
				dist = (node.position - goalPos).magnitude;
				endNode = node;
			}
		}

		//PathNode endNode = new PathNode(goalPos);
		//nodes.Add (endNode);

		PlotArcs (Helper.GetObjectWorldSize (obj));
		//return;

		//path.Add(startPos);

		PathNode nextNode = new PathNode(embeddingSpaceBounds.max+Vector3.one);

		// find closest node to start
//		float dist = (startNode - startPos).magnitude;
//		foreach (Vector3 node in nodes) {
//			if ((node - startPos).magnitude < dist) {
//				dist = (node - startPos).magnitude;
//				startNode = node;
//			}
//		}
//		path.Add(startNode);
//		//Debug.Log (startNode);

		plannedPath.Add (new PathNode(startPos));

		// starting with startNode, for each neighborhood node of last node, assess A* heuristic
		// using best node found until endNode reached
		while (openSet.Count > 0) {
			PathNode curNode = null;
			float testHeuristicScore = Mathf.Infinity;
			foreach (PathNode node in openSet) {
				if (node.heuristicScore < testHeuristicScore) {
					testHeuristicScore = node.heuristicScore;
					curNode = node;
				}
			}

			//nextNode = embeddingSpaceBounds.max + Vector3.one;
			//dist = (nextNode - endNode).magnitude;
			//float dist = ((embeddingSpaceBounds.max + Vector3.one)-endNode.position).magnitude;

			if ((curNode.position - endNode.position).magnitude < Constants.EPSILON) {
				PathNode goalNode = new PathNode (goalPos);
				goalNode.cameFrom = curNode;
				path = ReconstructPath (startNode, goalNode);
				break;
			}

			openSet.Remove (curNode);
			closedSet.Add (curNode);

			List<Pair<PathNode,PathNode>> arcList = arcs.Where(n => ((n.Item1.position-curNode.position).magnitude < Constants.EPSILON) || 
				(n.Item2.position-curNode.position).magnitude < Constants.EPSILON).ToList();
			foreach (Pair<PathNode,PathNode> arc in arcList) {
				float testScore;
				if ((arc.Item1.position - curNode.position).magnitude < Constants.EPSILON) {
					if (!closedSet.Contains(arc.Item2)) {
						testScore = curNode.scoreFromStart + (arc.Item2.position - curNode.position).magnitude;
						if (testScore < arc.Item2.scoreFromStart) {
							nextNode = arc.Item2;
							arc.Item2.cameFrom = curNode;
							arc.Item2.scoreFromStart = testScore;
							arc.Item2.heuristicScore = arc.Item2.scoreFromStart + 
								(Mathf.Abs(goalPos.x - arc.Item2.position.x) + Mathf.Abs(goalPos.y - arc.Item2.position.y) +
									Mathf.Abs(goalPos.z - arc.Item2.position.z));

							if (!openSet.Contains (arc.Item2)) {
								openSet.Add (arc.Item2);
							}
						}
//						if ((arc.Item2.position - endNode.position).magnitude < dist) {	// heuristic here: linear distance
//							dist = (arc.Item2.position - endNode.position).magnitude;
//							nextNode = arc.Item2;
//						}
					}
				}
				else if ((arc.Item2.position - curNode.position).magnitude < Constants.EPSILON) {
					if (!closedSet.Contains(arc.Item1)) {
						testScore = curNode.scoreFromStart + (arc.Item1.position - curNode.position).magnitude;
						if (testScore < arc.Item1.scoreFromStart) {
							nextNode = arc.Item1;
							arc.Item1.cameFrom = curNode;
							arc.Item1.scoreFromStart = testScore;
							arc.Item1.heuristicScore = arc.Item1.scoreFromStart + 
								(Mathf.Abs(goalPos.x - arc.Item1.position.x) + Mathf.Abs(goalPos.y - arc.Item1.position.y) +
									Mathf.Abs(goalPos.z - arc.Item1.position.z));

							if (!openSet.Contains (arc.Item1)) {
								openSet.Add (arc.Item1);
							}
						}
//						if ((arc.Item1.position - endNode.position).magnitude < dist) {	// heuristic here: linear distance
//							dist = (arc.Item1.position - endNode.position).magnitude;
//							nextNode = arc.Item1;
//						}
					}
				}
			}
//			foreach (Vector3 node in nodes) {
//				//List<Pair<Vector3,Vector3>> arcList = arcs.Where(n => (n.Item1 == curNode)).ToList();
//				foreach (Pair<Vector3,Vector3> nodePair in arcList) {
//					if (nodePair.Item1 == path [path.Count - 1]) {
//						if ((nodePair.Item2 - endNode).magnitude < dist) {	// heuristic here: linear distance
//							dist = (nodePair.Item2 - endNode).magnitude;
//							nextNode = nodePair.Item2;
//						}
//					}
//					else if (nodePair.Item2 == path [path.Count - 1]) {
//						if ((nodePair.Item1 - endNode).magnitude < dist) {	// heuristic here: linear distance
//							dist = (nodePair.Item1 - endNode).magnitude;
//							nextNode = nodePair.Item1;
//						}
//					}
////				if (((node - path [path.Count - 1]).x <= increment.x) &&
////					((node - path [path.Count - 1]).y <= increment.y) &&
////					((node - path [path.Count - 1]).z <= increment.z)){ // look for nodes $increment units from the current
//				//if ((node - path [path.Count - 1]).magnitude <= increment.magnitude) { // look for nodes $increment units from the current
////					if ((Vector3.Dot ((node - path [path.Count - 1]).normalized, Vector3.right) == -1.0f) ||
////						(Vector3.Dot ((node - path [path.Count - 1]).normalized, Vector3.right) == 0.0f) ||
////						(Vector3.Dot ((node - path [path.Count - 1]).normalized, Vector3.right) == 1.0f)){	// orthogonal movements only for now
//						//if (!Physics.Raycast (new Ray (path [path.Count - 1], (node - path [path.Count - 1]).normalized), (node - path [path.Count - 1]).magnitude)) {
////							if ((node - endNode).magnitude < dist) {	// heuristic here: linear distance
////								dist = (node - endNode).magnitude;
////								nextNode = node;
////							}
//						//}
//					//}
//				}
//			}
			//plannedPath.Add (nextNode);
			//Debug.Log (nextNode);
			//AddDebugCube (nextNode);
		} 

		//path.Add(endNode);
		//Debug.Log (endNode);

		//plannedPath.Add(endNode);
	}

	List<Vector3> ReconstructPath(PathNode firstNode, PathNode lastNode) {
		path = new List<Vector3> ();
		PathNode node = lastNode;

		//path.Add (lastNode.position);

		while (node != firstNode) {
			path.Insert (0, node.position);
			node = node.cameFrom;
		}

		return path;
	}
}
