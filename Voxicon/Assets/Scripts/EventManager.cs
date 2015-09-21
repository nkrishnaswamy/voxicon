using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Reflection;
using System.Text.RegularExpressions;

using Global;
using Satisfaction;

public class EventManager : MonoBehaviour {

	public List<String> events = new List<String>();
	public OrderedDictionary eventsStatus = new OrderedDictionary();
	MethodInfo methodToCall;
	Predicates preds;
	String nextQueuedEvent = "";

	// Use this for initialization
	void Start () {
		preds = gameObject.GetComponent<Predicates> ();
	}
	
	// Update is called once per frame
	void Update () {
		if (eventsStatus.Keys.Count > 0) {
			Debug.Log (eventsStatus.Keys.Count);
			String nextIncompleteEvent = GetNextIncompleteEvent ();
			Debug.Log ("Next incomplete event: " + nextIncompleteEvent);
			Debug.Log ("Next queued event: " + nextQueuedEvent);

			if (nextIncompleteEvent != "") {
				if (SatisfactionTest.IsSatisfied (nextIncompleteEvent) == true) {
					Debug.Log ("Satisfied " + nextIncompleteEvent);
					eventsStatus[nextIncompleteEvent] = true;
					if (nextQueuedEvent != "") {
						SatisfactionTest.ComputeSatisfactionConditions(nextQueuedEvent);
						Debug.Log ("eventsStatus.Keys.Count:" + eventsStatus.Keys.Count.ToString());
						ExecuteCommand (nextQueuedEvent);
					}
					else {
						nextIncompleteEvent = "";
					}
				}
			}
		}
	}

	public void ExecuteNextCommand() {
		ExecuteCommand (events[0]);
	}

	public void ExecuteCommand(String evaluatedCommand) {
		Hashtable predArgs = Helper.ParsePredicate (evaluatedCommand);
		String pred = Helper.GetTopPredicate (evaluatedCommand);
		Queue<String> argsStrings = new Queue<String> (((String)predArgs [pred]).Split (new char[] {','}));
		List<object> objs = new List<object>();
		
		while (argsStrings.Count > 0) {
			object arg = argsStrings.Dequeue ();
			
			if (Helper.v.IsMatch((String)arg)) {	// if arg is vector form
				objs.Add (Helper.ConvertParsableToVector((String)arg));
			}
			else if (arg is String) {	// if arg is String
				objs.Add (GameObject.Find (arg as String));
			}
		}

		objs.Add (true);
		methodToCall = preds.GetType ().GetMethod (pred);
		Debug.Log ("ExecuteCommand: invoke " + methodToCall.Name);
		object obj = methodToCall.Invoke (preds, new object[]{objs.ToArray ()});
	}

	public void ClearEvents() {
		events.Clear ();
	}

	String GetNextIncompleteEvent() {
		String[] keys = new String[eventsStatus.Keys.Count];
		bool[] values = new bool[eventsStatus.Keys.Count];

		eventsStatus.Keys.CopyTo (keys,0);
		eventsStatus.Values.CopyTo (values,0);

		String nextIncompleteEvent = "";
		for (int i = 0; i < keys.Length; i++) {
			if ((bool)eventsStatus[keys[i]] == false) {
				nextIncompleteEvent = (String)keys[i];
				if (i < events.Count-1) {
					SatisfactionTest.ComputeSatisfactionConditions(events[i+1]);
					eventsStatus.Keys.CopyTo (keys,0);
					eventsStatus.Values.CopyTo (values,0);
					nextQueuedEvent = (String)keys[i+1];
				}
				else {
					nextQueuedEvent = "";
					//put(spoon,on(mug));put(apple,on(plate))
					//put(block1,on(block2));put(block3,on(block1))
				}
				break;
			}
		}

		return nextIncompleteEvent;

		/*SatisfactionTest.ComputeSatisfactionConditions(events[eventsStatus.Keys.Count-1]);

		eventsStatus.Keys.CopyTo (keys,0);
		eventsStatus.Values.CopyTo (values,0);

		String nextIncompleteEvent = keys [keys.Length - 1];

		if (events.Count > eventsStatus.Keys.Count) {
			nextQueuedEvent = SatisfactionTest.ComputeSatisfactionConditions(events[eventsStatus.Keys.Count]);

			eventsStatus.Keys.CopyTo (keys,0);
			eventsStatus.Values.CopyTo (values,0);
			
			nextQueuedEvent = keys [keys.Length - 1];
		}
		else {
			nextQueuedEvent = "";
		}

		return nextIncompleteEvent;*/
	}
}
