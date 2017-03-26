using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Timers;

using Global;
using Satisfaction;

public class EventManagerArgs : EventArgs {

	public string EventString { get; set; }
	public bool MacroEvent { get; set; }

	public EventManagerArgs(string str, bool macroEvent = false)
	{
		this.EventString = str;
		this.MacroEvent = macroEvent;
	}
}

public class EventManager : MonoBehaviour {
	public List<String> events = new List<String>();
	public OrderedDictionary eventsStatus = new OrderedDictionary();
	public ObjectSelector objSelector;
	public InputController inputController;
	public string lastParse = string.Empty;
	//public string lastObjectResolved = string.Empty;
	public Dictionary<String,String> evalOrig = new Dictionary<String, String>();
	public Dictionary<String,String> evalResolved = new Dictionary<String, String>();

	public double eventWaitTime = 2000.0;
	Timer eventWaitTimer;
	bool eventWaitCompleted = false;

	string skolemized, evaluated;
	MethodInfo methodToCall;
	public Predicates preds;
	String nextQueuedEvent = "";
	int argVarIndex = 0;
	Hashtable skolems = new Hashtable();
	string argVarPrefix = @"_ARG";
	Regex r = new Regex(@".*\(.*\)");
	String nextIncompleteEvent;
	bool stayExecution = false;

	public enum EvaluationPass {
		Attributes,
		RelationsAndFunctions
	}

	public bool immediateExecution = true;

	public event EventHandler ObjectsResolved;

	public void OnObjectsResolved(object sender, EventArgs e)
	{
		if (ObjectsResolved != null)
		{
			ObjectsResolved(this, e);
		}
	}

	public event EventHandler SatisfactionCalculated;

	public void OnSatisfactionCalculated(object sender, EventArgs e)
	{
		if (SatisfactionCalculated != null)
		{
			SatisfactionCalculated(this, e);
		}
	}

	public event EventHandler ExecuteEvent;

	public void OnExecuteEvent(object sender, EventArgs e)
	{
		if (ExecuteEvent != null)
		{
			ExecuteEvent(this, e);
		}
	}

	public event EventHandler EventComplete;

	public void OnEventComplete(object sender, EventArgs e)
	{
		if (EventComplete != null)
		{
			EventComplete(this, e);
		}
	}

	public event EventHandler QueueEmpty;

	public void OnQueueEmpty(object sender, EventArgs e)
	{
		if (QueueEmpty != null)
		{
			QueueEmpty(this, e);
		}
	}

	public event EventHandler ForceClear;

	public void OnForceClear(object sender, EventArgs e)
	{
		if (ForceClear != null)
		{
			ForceClear(this, e);
		}
	}

	// Use this for initialization
	void Start () {
		preds = gameObject.GetComponent<Predicates> ();
		objSelector = GameObject.Find ("BlocksWorld").GetComponent<ObjectSelector> ();
		inputController = GameObject.Find ("IOController").GetComponent<InputController> ();

		inputController.ParseComplete += StoreParse;
		//inputController.InputReceived += StartEventWaitTimer;

		//eventWaitTimer = new Timer (eventWaitTime);
		//eventWaitTimer.Enabled = false;
		//eventWaitTimer.Elapsed += ExecuteNextEvent;
	}
	
	// Update is called once per frame
	void Update () {
		if (stayExecution) {
			stayExecution = false;
			return;
		}

		if (events.Count > 0) {
//			if (!SatisfactionTest.ComputeSatisfactionConditions (events [0])) {
//				return;
//			}

			if (SatisfactionTest.IsSatisfied (events [0]) == true) {
				GameObject.Find ("BlocksWorld").GetComponent<AStarSearch> ().path.Clear ();
				Debug.Log ("Satisfied " + events [0]);
				for (int i = 0; i < events.Count - 1; i++) {
					events [i] = events [i + 1];
					Debug.Log (i);
					Debug.Log (events [i]);
				}
				string completedEvent = events [events.Count - 1];
				RemoveEvent (events.Count - 1);
				//Debug.Log (events.Count);

				if (events.Count > 0) {
					ExecuteNextCommand ();
				}
				else {
					OutputHelper.PrintOutput (OutputController.Role.Affector, "OK, I did it.");
					EventManagerArgs eventArgs = new EventManagerArgs (completedEvent);
					OnEventComplete (this, eventArgs);
				}
			}
		}
		else {
		}
	}

	public void RemoveEvent(int index) {
		Debug.Log (string.Format("Removing event {0}",events[index]));
		events.RemoveAt (index);

		if (events.Count == 0) {
			OnQueueEmpty (this, null);
		}
	}

	public void InsertEvent(String commandString, int before) {
		//Debug.Break ();
		Debug.Log ("Inserting: " + commandString);
		events.Insert(before,commandString);
	}

	public void QueueEvent(String commandString) {
		// not using a Queue because I'm horrible
		events.Add(commandString);
	}

	public void StoreParse(object sender, EventArgs e) {
		lastParse = ((InputEventArgs)e).InputString;
	}

	public void WaitComplete(object sender, EventArgs e) {
		((System.Timers.Timer)sender).Enabled = false;
//		RemoveEvent (0);
//		stayExecution = true;
	}

	public void PrintEvents() {
		foreach (String e in events) {
			Debug.Log (e);
		}
	}

	void StartEventWaitTimer(object sender, EventArgs e) {
		eventWaitTimer.Enabled = true;
	}

	void ExecuteNextEvent(object sender, ElapsedEventArgs e) {
		//Debug.Log ("Event wait complete");
		eventWaitCompleted = true;
	}

	public void ExecuteNextCommand() {
		PhysicsHelper.ResolveAllPhysicsDiscepancies (false);
		Debug.Log (events [0]);

		if (!EvaluateCommand (events [0])) {
			return;
		}

		if (events.Count > 0) {
			if (SatisfactionTest.ComputeSatisfactionConditions (events [0])) {
				ExecuteCommand (events [0]);
			}
			else {
				RemoveEvent (0);
			}
		}
	}

	public bool EvaluateCommand(String command) {
		ClearRDFTriples ();
		ClearSkolems ();
		ParseCommand (command);
		FinishSkolemization ();
		skolemized = Skolemize (command);
		Debug.Log ("Skolemized command: " + skolemized);
		//EvaluateSkolemizedCommand(skolemized);

		if (!EvaluateSkolemConstants (EvaluationPass.Attributes)) {
			RemoveEvent (events.Count - 1);
			return false;
		}
		string objectResolved = ApplySkolems (skolemized);
		Debug.Log (objectResolved);

		if (objectResolved != command) {
			OnObjectsResolved (this, new EventManagerArgs (objectResolved));
		}

		if (events.IndexOf (command) < 0) {
			return false;
		}

//		Triple<String,String,String> triple = Helper.MakeRDFTriples(objectResolved);
//		if (triple.Item1 != "" && triple.Item2 != "" && triple.Item3 != "") {
//			preds.rdfTriples.Add(triple);
//			Helper.PrintRDFTriples(preds.rdfTriples);
//		}
//		else {
//			Debug.Log ("Failed to make RDF triple");
//		}

		if (!EvaluateSkolemConstants (EvaluationPass.RelationsAndFunctions)) {
			RemoveEvent (events.Count - 1);
			return false;
		}

		evaluated = ApplySkolems (skolemized);
		Debug.Log ("Evaluated command: " + evaluated);
		Debug.Log (events.IndexOf (command));
		if (!evalOrig.ContainsKey (evaluated)) {
			evalOrig.Add (evaluated, command);
		}

		if (!evalResolved.ContainsKey (evaluated)) {
			evalResolved.Add (evaluated, objectResolved);
		}
		events [events.IndexOf (command)] = evaluated;

		Triple<String,String,String> triple = Helper.MakeRDFTriples(evalResolved[evaluated]);
		Debug.Log(evalOrig[evaluated]);
		Debug.Log(evalResolved[evaluated]);

		if (triple.Item1 != "" && triple.Item2 != "" && triple.Item3 != "") {
			preds.rdfTriples.Add(triple);
			Helper.PrintRDFTriples(preds.rdfTriples);
		}
		else {
			Debug.Log ("Failed to make RDF triple");
		}

		//OnExecuteEvent (this, new EventManagerArgs (evaluated));

		return true;
	}

	public void ExecuteCommand(String evaluatedCommand) {
		Debug.Log("Execute command: " + evaluatedCommand);
		Hashtable predArgs = Helper.ParsePredicate (evaluatedCommand);
		String pred = Helper.GetTopPredicate (evaluatedCommand);

		if (predArgs.Count > 0) {
			Queue<String> argsStrings = new Queue<String> (((String)predArgs [pred]).Split (new char[] { ',' }));
			List<object> objs = new List<object> ();
		
			while (argsStrings.Count > 0) {
				object arg = argsStrings.Dequeue ();
			
				if (Helper.v.IsMatch ((String)arg)) {	// if arg is vector form
					objs.Add (Helper.ParsableToVector ((String)arg));
				} else if (arg is String) {	// if arg is String
					if ((arg as String) != string.Empty) {
						Regex q = new Regex ("\".*\"");
						if (q.IsMatch (arg as String)) {
							objs.Add (arg as String);
						} 
						else {
							List<GameObject> matches = new List<GameObject> ();
							foreach (Voxeme voxeme in objSelector.allVoxemes) {
								if (voxeme.voxml.Lex.Pred.Equals (arg)) {
									matches.Add (voxeme.gameObject);
								}
							}

							if (matches.Count <= 1) {
								GameObject go = GameObject.Find (arg as String);
								if (go == null) {
									OutputHelper.PrintOutput (OutputController.Role.Affector, string.Format ("What is that?", (arg as String)));
									return;	// abort
								}
								objs.Add (go);
							} 
							else {
								//Debug.Log (string.Format ("Which {0}?", (arg as String)));
								//OutputHelper.PrintOutput (string.Format("Which {0}?", (arg as String)));
							}
						}
					}
				}
			}

			objs.Add (true);
			methodToCall = preds.GetType ().GetMethod (pred.ToUpper());

			if ((methodToCall != null) &&  (preds.rdfTriples.Count > 0)) {
				Debug.Log ("ExecuteCommand: invoke " + methodToCall.Name);
				object obj = methodToCall.Invoke (preds, new object[]{ objs.ToArray () });
				Debug.Log (evaluatedCommand);
				OnExecuteEvent(this, new EventManagerArgs (evaluatedCommand));
			}
		}
	}

	public void AbortEvent() {
		if (events.Count > 0) {
			//InsertEvent ("satisfy()", 0);
			InsertEvent ("", 0);
			RemoveEvent (1);
			//RemoveEvent (0);
		}
	}

	public void ClearEvents() {
		events.Clear ();
		OnForceClear (this, null);
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
				}
				break;
			}
		}

		return nextIncompleteEvent;
	}

	public void ClearSkolems() {
		argVarIndex = 0;
		skolems.Clear ();
	}

	public void ClearRDFTriples () {
		preds.rdfTriples.Clear ();
	}

	public void ParseCommand(String command) {
		Hashtable predArgs;
		String predString = null;
		Queue<String> argsStrings = null;

		if (r.IsMatch (command)) {	// if command matches predicate form
			//Debug.Log ("ParseCommand: " + command);
			// make RDF triples only after resolving attributives to atomics (but before evaluating relations and functions)
			/*Triple<String,String,String> triple = Helper.MakeRDFTriples(command);
			if (triple.Item1 != "" && triple.Item2 != "" && triple.Item3 != "") {
				preds.rdfTriples.Add(triple);
				Helper.PrintRDFTriples(preds.rdfTriples);
			}
			else {
				Debug.Log ("Failed to make RDF triple");
			}*/
			predArgs = Helper.ParsePredicate(command);
			foreach (DictionaryEntry entry in predArgs) {
				predString = (String)entry.Key;
				argsStrings = new Queue<String>(((String)entry.Value).Split (new char[] {','}));

				StringBuilder sb = new StringBuilder("[");
				foreach(String arg in argsStrings) {
					sb.Append (arg + ",");
				}
				sb.Remove(sb.Length-1,1);
				sb.Append("]");
				String argsList = sb.ToString();
				//Debug.Log(predString + " : " + argsList);

				for(int i = 0; i < argsStrings.Count; i++) {
					Debug.Log (argsStrings.ElementAt (i));
					if (r.IsMatch (argsStrings.ElementAt (i))) {
						String v = argVarPrefix+argVarIndex.ToString();
						skolems[v] = argsStrings.ElementAt (i);
						Debug.Log (v + " : " + skolems[v]);
						argVarIndex++;

						sb = new StringBuilder(sb.ToString());
						foreach(DictionaryEntry kv in skolems) {
							argsList = argsList.Replace((String)kv.Value, (String)kv.Key);
						}

					}
					ParseCommand (argsStrings.ElementAt (i));
				}
			}
		}
	}

	public void FinishSkolemization() {
		Hashtable temp = new Hashtable ();

		foreach (DictionaryEntry kv in skolems) {
			foreach (DictionaryEntry kkv in skolems) {
				if (kkv.Key != kv.Key) {
					//Debug.Log ("FinishSkolemization: "+kv.Key+ " " +kkv.Key);
					if (!temp.Contains (kkv.Key)) {
						if (((String)kkv.Value).Contains ((String)kv.Value)) {
							//Debug.Log ("FinishSkolemization: " + kv.Value + " found in " + kkv.Value);
							//Debug.Log ("FinishSkolemization: " + kkv.Key + " : " + ((String)kkv.Value).Replace ((String)kv.Value, (String)kv.Key));
							temp [kkv.Key] = ((String)kkv.Value).Replace ((String)kv.Value, (String)kv.Key);
						}
					}
				}
			}
		}

		foreach (DictionaryEntry kv in temp) {
			skolems[kv.Key] = temp[kv.Key];
		}

		Helper.PrintKeysAndValues(skolems);
	}

	public String Skolemize(String inString) {
		String outString = inString;
		String temp = inString;

		int parenCount = temp.Count(f => f == '(') + 
			temp.Count(f => f == ')');
		//Debug.Log ("Skolemize: parenCount = " + parenCount.ToString ());

		do{
			foreach (DictionaryEntry kv in skolems) {
				outString = (String)outString.Replace((String)kv.Value,(String)kv.Key);
				//Debug.Log (outString);
			}
			temp = outString;
			parenCount = temp.Count(f => f == '(') + 
				temp.Count(f => f == ')');
			//Debug.Log ("Skolemize: parenCount = " + parenCount.ToString ());
			//move(mug,from(edge(table)),to(edge(table)))
		}while(parenCount > 2);

		return outString;
	}

	public String ApplySkolems(String inString) {
		String outString = inString;
		String temp = inString;

		int parenCount = temp.Count(f => f == '(') + 
			temp.Count(f => f == ')');
		//Debug.Log ("Skolemize: parenCount = " + parenCount.ToString ());

		foreach (DictionaryEntry kv in skolems) {
			if (kv.Value is Vector3) {
				outString = (String)outString.Replace ((String)kv.Key, Helper.VectorToParsable ((Vector3)kv.Value));
				//Debug.Log (outString);
			}
			else if (kv.Value is String) {
				outString = (String)outString.Replace ((String)kv.Key, (String)kv.Value);
			}
			else if (kv.Value is List<String>) {
				String list = String.Join (",", ((List<String>)kv.Value).ToArray());
				outString = (String)outString.Replace ((String)kv.Key, list);
			}
		}
		temp = outString;
		parenCount = temp.Count(f => f == '(') + 
			temp.Count(f => f == ')');
		//Debug.Log ("Skolemize: parenCount = " + parenCount.ToString ());

		return outString;
	}

	public bool EvaluateSkolemConstants(EvaluationPass pass) {
		Hashtable temp = new Hashtable ();
		Regex regex = new Regex (argVarPrefix+@"[0-9]+");
		Match argsMatch;
		Hashtable predArgs;
		List<object> objs = new List<object>();
		Queue<String> argsStrings;
		bool doSkolemReplacement = false;
		Triple<String,String,String> replaceSkolems = null;

		foreach (DictionaryEntry kv in skolems) {
			Debug.Log (kv.Key + " : " + kv.Value); 
			objs.Clear ();
			if (kv.Value is String) {
				Debug.Log (kv.Value); 
				argsMatch = regex.Match ((String)kv.Value);
				Debug.Log (argsMatch); 
				if (argsMatch.Groups [0].Value.Length == 0) {	// matched an empty string = no match
					Debug.Log (kv.Value);
					predArgs = Helper.ParsePredicate ((String)kv.Value);
					String pred = Helper.GetTopPredicate ((String)kv.Value);
					if (((String)kv.Value).Count (f => f == '(') +	// make sure actually a predicate
						((String)kv.Value).Count (f => f == ')') >= 2) {
						argsStrings = new Queue<String> (((String)predArgs [pred]).Split (new char[] {','}));
						while (argsStrings.Count > 0) {
							object arg = argsStrings.Dequeue ();

							if (Helper.v.IsMatch ((String)arg)) {	// if arg is vector form
								objs.Add (Helper.ParsableToVector ((String)arg));
							}
							else if (arg is String) {	// if arg is String
								if ((arg as String).Count (f => f == '(') +	// not a predicate
								    (arg as String).Count (f => f == ')') == 0) {
									//if (preds.GetType ().GetMethod (pred.ToUpper ()).ReturnType != typeof(String)) {	// if predicate not going to return string (as in "AS")
									List<GameObject> matches = new List<GameObject> ();
									foreach (Voxeme voxeme in objSelector.allVoxemes) {
										if (voxeme.voxml.Lex.Pred.Equals(arg)) {
											matches.Add (voxeme.gameObject);
										}
									}

									if (matches.Count == 0) {
										if (preds.GetType ().GetMethod (pred.ToUpper ()).ReturnType != typeof(String)) {	// if predicate not going to return string (as in "AS")
											GameObject go = GameObject.Find (arg as String);
											if (go == null) {
												OutputHelper.PrintOutput (OutputController.Role.Affector, string.Format ("What is that?", (arg as String)));
												return false;	// abort
											}
											objs.Add (go);
										}
									}
									else if (matches.Count == 1) {
										if (preds.GetType ().GetMethod (pred.ToUpper ()).ReturnType != typeof(String)) {	// if predicate not going to return string (as in "AS")
											GameObject go = matches [0];
											if (go == null) {
												OutputHelper.PrintOutput (OutputController.Role.Affector, string.Format ("What is that?", (arg as String)));
												return false;	// abort
											}
											objs.Add (go);
											doSkolemReplacement = true;
											replaceSkolems = new Triple<String,String,String> (kv.Key as String, arg as String, go.name);
											//skolems[kv] = go.name;
										}
										else {
											objs.Add (matches[0]);
										}
									}
									else {
										//Debug.Log (string.Format ("Which {0}?", (arg as String)));
										//OutputHelper.PrintOutput (OutputController.Role.Affector,string.Format("Which {0}?", (arg as String)));
										//return false;	// abort
										foreach (GameObject match in matches) {
											objs.Add (match);
										}
									}
									//}
								}

								if (objs.Count == 0) {
									Regex q = new Regex ("\".*\"");
									if (q.IsMatch (arg as String)) {
										objs.Add (arg);
									}
									else {
										objs.Add (GameObject.Find (arg as String));
									}
								}
							}
						}

						methodToCall = preds.GetType ().GetMethod (pred.ToUpper());

						if (methodToCall == null) {
							OutputHelper.PrintOutput (OutputController.Role.Affector,"Sorry, what does " + "\"" + pred + "\" mean?");
							return false;
						}

						if (pass == EvaluationPass.Attributes) {
							if ((methodToCall.ReturnType == typeof(String)) ||  (methodToCall.ReturnType == typeof(List<String>))) {
								Debug.Log ("EvaluateSkolemConstants: invoke " + methodToCall.Name);
								object obj = methodToCall.Invoke (preds, new object[]{ objs.ToArray () });
								Debug.Log (obj);

								temp [kv.Key] = obj;
							}
						}
						else if (pass == EvaluationPass.RelationsAndFunctions) {
							if (methodToCall.ReturnType == typeof(Vector3)) {
								Debug.Log ("EvaluateSkolemConstants: invoke " + methodToCall.Name);
								object obj = methodToCall.Invoke (preds, new object[]{ objs.ToArray () });
								Debug.Log (obj);

								temp [kv.Key] = obj;
							}
						}
					}
				}
				else {
					temp [kv.Key] = kv.Value;
				}
			}
		}

		// replace improperly named arguments
		if (doSkolemReplacement) {
			skolems [replaceSkolems.Item1] = ((String)skolems [replaceSkolems.Item1]).Replace (replaceSkolems.Item2, replaceSkolems.Item3);
		}

		//Helper.PrintKeysAndValues(skolems);

//		for (int i = 0; i < temp.Count; i++) {
//			Debug.Log (temp [i]);
//		}

		foreach (DictionaryEntry kv in temp) {
		//for (int i = 0; i < temp.Count; i++) {
			//DictionaryEntry kv = (DictionaryEntry)temp [i];
			//Debug.Log (kv.Value);
			String matchVal = kv.Value as String;
			if (matchVal == null) {
				matchVal = @"DEADBEEF";
			}
			argsMatch = regex.Match (matchVal);
			if (argsMatch.Groups [0].Value.Length > 0) {
				Debug.Log (argsMatch.Groups [0]);
				if (temp.ContainsKey (argsMatch.Groups [0].Value)) {
					object replaceWith = temp [(String)argsMatch.Groups [0].Value];
					Debug.Log (replaceWith.GetType ());
					//String replaced = ((String)skolems [kv.Key]).Replace ((String)argsMatch.Groups [0].Value,
					//	replaceWith.ToString ().Replace (',', ';').Replace ('(', '<').Replace (')', '>'));
					if (regex.Match ((String)replaceWith).Length == 0) {
						String replaced = (String)argsMatch.Groups [0].Value;
						if (replaceWith is String) {
							replaced = ((String)skolems [kv.Key]).Replace ((String)argsMatch.Groups [0].Value, (String)replaceWith);
						} else if (replaceWith is Vector3) {
							replaced = ((String)skolems [kv.Key]).Replace ((String)argsMatch.Groups [0].Value, Helper.VectorToParsable ((Vector3)replaceWith));
						}
						Debug.Log (replaced);
						//if (replace is Vector3) {
						skolems [kv.Key] = replaced;
					}
				}
			}
			else {
				skolems [kv.Key] = temp [kv.Key];
			}
		}

		Helper.PrintKeysAndValues(skolems);

		int newEvaluations = 0;
		foreach (DictionaryEntry kv in skolems) {
			Debug.Log(kv.Key + " : " + kv.Value);
			if (kv.Value is String) {
				argsMatch = r.Match ((String)kv.Value);

				if (argsMatch.Groups [0].Value.Length > 0) {
					string pred = argsMatch.Groups [0].Value.Split ('(') [0];
					Debug.Log (pred);
					methodToCall = preds.GetType ().GetMethod (pred.ToUpper());
					Debug.Log (methodToCall);

					if (methodToCall != null) {
						if (((methodToCall.ReturnType == typeof(String)) || (methodToCall.ReturnType == typeof(List<String>))) &&
							(pass == EvaluationPass.Attributes)) {
							newEvaluations++;
						}
						if ((methodToCall.ReturnType == typeof(Vector3)) && (pass == EvaluationPass.RelationsAndFunctions)) {
							newEvaluations++;
						}
					}
				}
			}
		}

		//Debug.Log (newEvaluations);
		if (newEvaluations > 0) {
			EvaluateSkolemConstants (pass);
		}

		//Helper.PrintKeysAndValues(skolems);

		return true;
	}
}
