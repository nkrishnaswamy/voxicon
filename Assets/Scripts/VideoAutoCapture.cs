using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Timers;

using Global;
using FlashbackVideoRecorder;
using SQLite4Unity3d;

namespace VideoCapture {
	public enum VideoCaptureMode {
		FullTime,
		Manual,
		PerEvent,
	};

	public enum VideoCaptureFilenameType {
		FlashbackDefault,
		EventString,
		Custom
	};

	public class VideoDBEntry  {
		[PrimaryKey, AutoIncrement]
		public int Id { get; set; }
		public string Filename { get; set; }
		public string InputString { get; set; }
		public string Parse { get; set; }
		public string ObjectResolvedParse { get; set; }
		public string EventPredicate { get; set; }
		public string Objects { get; set; }
		public string ParameterValues { get; set; }
	
		public override string ToString ()
		{
			string head = string.Format ("[VideoDBEntry: Id={0}," +
				" Filename={1}," +
				" InputString={2}," +
				" Parse={3}," +
				" ObjectResolvedParse={4}," +
				" EventPredicate={5},", Id, Filename, InputString, Parse, ObjectResolvedParse, EventPredicate);
			string valueContent = "";
			string tail = "]";
			return head + valueContent + tail;
		}
	}

	public class VideoAutoCapture : MonoBehaviour {
		public KeyCode startCaptureKey;
		public KeyCode stopCaptureKey;

		public List<GameObject> availableObjs;

		FlashbackRecorder recorder;
		InputController inputController;
		EventManager eventManager;
		ObjectSelector objSelector;
		PluginImport commBridge;
		Predicates preds;

		public double eventTimeoutTime = 15000.0f;
		Timer eventTimeoutTimer;

		public double eventCompleteWaitTime = 1000.0f;
		Timer eventCompleteWaitTimer;

		bool capture;

		bool capturing = false;
		bool writingFile = false;
		bool stopCaptureFlag = false;

		VideoCaptureMode captureMode;
		bool resetScene;
		VideoCaptureFilenameType filenameScheme;
		string filenamePrefix;
		string dbFile;

		SQLiteConnection dbConnection;
		VideoDBEntry dbEntry;
		string outFileName = string.Empty;

		public event EventHandler FileWritten;

		public void OnFileWritten(object sender, EventArgs e)
		{
			if (FileWritten != null)
			{
				FileWritten(this, e);
			}
		}

		int eventIndex = 0;
		List<GameObject> eventObjs;

		Dictionary<string,string> paramValues;

		// Use this for initialization
		void Start () {
			InitObjectDisabling ();

			recorder = gameObject.GetComponent<FlashbackRecorder> ();
			inputController = GameObject.Find ("IOController").GetComponent<InputController> ();
			eventManager = GameObject.Find ("BehaviorController").GetComponent<EventManager> ();
			objSelector = GameObject.Find ("BlocksWorld").GetComponent<ObjectSelector> ();
			commBridge = GameObject.Find ("CommunicationsBridge").GetComponent<PluginImport> ();
			preds = GameObject.Find ("BehaviorController").GetComponent<Predicates> ();

			capture = (PlayerPrefs.GetInt ("Capture Video") == 1);
			captureMode = (VideoCaptureMode)PlayerPrefs.GetInt ("Video Capture Mode");
			resetScene = (PlayerPrefs.GetInt ("Reset Between Events") == 1);
			filenameScheme = (VideoCaptureFilenameType)PlayerPrefs.GetInt ("Video Capture Filename Type");
			filenamePrefix = PlayerPrefs.GetString ("Custom Video Filename Prefix");
			dbFile = PlayerPrefs.GetString ("Video Capture DB");

			eventObjs = new List<GameObject> ();

			if (captureMode == VideoCaptureMode.PerEvent) {
				inputController.InputReceived += StartCapture;
				inputController.InputReceived += InputStringReceived;
				inputController.ParseComplete += ParseReceived;

				eventManager.ObjectsResolved += ObjectsResolved;
				eventManager.ObjectsResolved += FilterSpecifiedManner;
				eventManager.ObjectsResolved += ExecuteEvent;
				eventManager.QueueEmpty += EventComplete;

				preds.PrepareLog += PrepareLog;
				preds.ParamsCalculated += ParametersCalculated;

				eventTimeoutTimer = new Timer (eventTimeoutTime);
				eventTimeoutTimer.Enabled = false;
				eventTimeoutTimer.Elapsed += EventTimedOut;
				eventTimeoutTimer.Elapsed += StopCapture;

				eventCompleteWaitTimer = new Timer (eventCompleteWaitTime);
				eventCompleteWaitTimer.Enabled = false;
				eventCompleteWaitTimer.Elapsed += StopCapture;

				FileWritten += CaptureComplete;
			}
		}
		
		// Update is called once per frame
		void Update () {
			if (stopCaptureFlag) {
				SaveCapture ();
				stopCaptureFlag = false;
			}

			if ((writingFile) && (recorder.GetNumberOfPendingFiles () == 0)) {
				Debug.Log ("File written to disk.");
				OnFileWritten (this, null);
				writingFile = false;
			}

			if (captureMode == VideoCaptureMode.Manual) {
				if ((!capturing) && (!writingFile)) {
					if (Input.GetKeyDown (startCaptureKey)) {
						StartCapture (null, null);
					}
				}

				if (!writingFile) {
					if (Input.GetKeyDown (stopCaptureKey)) {
						StopCapture(null, null);
					}
				}
			}
		}

		public void InitObjectDisabling() {
			for (int i = 0; i < availableObjs.Count; i++) {
				availableObjs[i] = Helper.GetMostImmediateParentVoxeme (availableObjs [i]);
			}
		}

		void DisableObjects() {
			foreach (GameObject go in availableObjs) {
				//eventManager.InsertEvent (string.Format("disable({0})",go.name), 0);
				preds.DISABLE (new GameObject[] { go });
			}
		}

		void EnableObjects() {
			foreach (GameObject go in availableObjs) {
				//eventManager.InsertEvent (string.Format("disable({0})",go.name), 0);
				preds.ENABLE (new GameObject[] { go });
			}
		}

		void InputStringReceived(object sender, EventArgs e) {
			if (!capture) {
				return;
			}

			if (filenameScheme == VideoCaptureFilenameType.EventString) {
				outFileName = string.Format ("{0}-{1}", (((InputEventArgs)e).InputString).Replace (" ", "_"), DateTime.Now.ToString ("yyyy-MM-dd-HHmmss"));
			}
			else {
				outFileName = string.Format ("{0}-{1}", filenamePrefix, DateTime.Now.ToString ("yyyy-MM-dd-HHmmss"));
			}

			if (dbFile != string.Empty) {
				OpenDB ();
				dbEntry = new VideoDBEntry ();
				dbEntry.Filename = outFileName;
				dbEntry.InputString = ((InputEventArgs)e).InputString;
			}
		}

		void ParseReceived(object sender, EventArgs e) {
			if (!capture) {
				return;
			}

			string parse = ((InputEventArgs)e).InputString;

			if (dbEntry != null) {
				dbEntry.Parse = parse;

				dbEntry.EventPredicate = Helper.GetTopPredicate (dbEntry.Parse);
			}

			paramValues = PredicateParameters.InitPredicateParametersCollection();
		}

		void StartCapture(object sender, EventArgs e) {
			if (!capture) {
				return;
			}

			if (captureMode == VideoCaptureMode.PerEvent) {
				EnableObjects ();
				if (resetScene) {
					GameObject.Find ("BlocksWorld").GetComponent<ObjectSelector> ().SendMessage ("ResetScene");
				}

				DisableObjects ();
			}

			recorder.StartCapture ();
			Debug.Log ("Starting video capture...");

			capturing = true;
			stopCaptureFlag = false;
		}

		void ExecuteEvent(object sender, EventArgs e) {
			if (!capture) {
				return;
			}

			if (captureMode == VideoCaptureMode.PerEvent) {

				foreach (GameObject obj in eventObjs) {
					preds.ENABLE (new GameObject[]{ obj });
				}

				eventManager.InsertEvent ("wait()", 0);

				eventTimeoutTimer.Enabled = true;
			}
		}
			

		void SaveCapture () {
			if (!capture) {
				return;
			}

			if (captureMode == VideoCaptureMode.PerEvent) {
				eventTimeoutTimer.Enabled = false;
				eventTimeoutTimer.Interval = eventTimeoutTime;

				eventCompleteWaitTimer.Enabled = false;
				eventCompleteWaitTimer.Interval = eventCompleteWaitTime;
			}

			recorder.StopCapture ();

			if (filenameScheme == VideoCaptureFilenameType.FlashbackDefault) {
				recorder.SaveCapturedFrames ();

				if (dbFile != string.Empty) {
					dbEntry.Filename = "Flashback_" + DateTime.Now.ToString ("yyyy-MM-dd-HHmmss");
				}
			}
			else {
				recorder.SaveCapturedFrames (outFileName);
			}

			if (dbFile != string.Empty) {
				WriteToDB ();
			}

			paramValues.Clear();

			capturing = false;
			writingFile = true;

			Debug.Log ("Stopping video capture.");
		}

		void StopCapture(object sender, EventArgs e) {
			if (!capture) {
				return;
			}

			if (capturing) {
				stopCaptureFlag = true;
			}
		}

		void EventTimedOut(object sender, EventArgs e) {
			eventManager.AbortEvent ();
		}

		void EventComplete(object sender, EventArgs e) {
			if (!capture) {
				return;
			}

			if (capturing) {
				eventCompleteWaitTimer.Enabled = true;
			}
		}

		void ObjectsResolved(object sender, EventArgs e) {
			dbEntry.ObjectResolvedParse = ((EventManagerArgs)e).EventString;

//			Regex r = new Regex (@"\([^()]*\)");
//			MatchCollection matches = r.Matches (dbEntry.ObjectResolvedParse);
//			List<String> argsStrings = matches.Cast<Match> ().Select (m => m.Value.Replace("(","").Replace(")","")).ToList ();

			string[] constituents = dbEntry.ObjectResolvedParse.Split (new char[]{ '(', ',', ')' });
			List<String> argsStrings = new List<string> ();

			foreach (string constituent in constituents) {
				if ((GameObject.Find (constituent) != null) || (objSelector.disabledObjects.Find(o => o.name == constituent) != null)) {
					string objName = GameObject.Find (constituent) != null ? GameObject.Find (constituent).name : 
						objSelector.disabledObjects.Find (o => o.name == constituent).name;
					
					if (!argsStrings.Contains (constituent)) {
						argsStrings.Add (constituent);
					}
				}
			}

			dbEntry.Objects = string.Join(", ",argsStrings.ToArray());

//			preds.rdfTriples.Clear ();
//			preds.rdfTriples.Add(Helper.MakeRDFTriples (dbEntry.ObjectResolvedParse));
		}

		void FilterSpecifiedManner(object sender, EventArgs e) {
			string specifiedEvent = PredicateParameters.FilterSpecifiedManner (((EventManagerArgs)e).EventString);

			string[] constituents = specifiedEvent.Split (new char[]{ '(', ',', ')' });

			eventObjs.Clear ();
			foreach (string constituent in constituents) {
				if ((GameObject.Find (constituent) != null) || (objSelector.disabledObjects.Find(o => o.name == constituent) != null)) {
					GameObject obj = GameObject.Find (constituent) != null ? GameObject.Find (constituent) :
						objSelector.disabledObjects.Find (o => o.name == constituent);

					if (!eventObjs.Contains (obj)) {
						eventObjs.Add(obj);
					}
				}
			}

			if (specifiedEvent != ((EventManagerArgs)e).EventString) {
				paramValues["MotionManner"] = specifiedEvent;

				eventManager.InsertEvent (specifiedEvent, 1);

				if (captureMode == VideoCaptureMode.PerEvent) {
					eventManager.InsertEvent ("wait()", 1);
				}

				eventManager.AbortEvent ();
			}
		}

		void PrepareLog(object sender, EventArgs e) {
			paramValues[((ParamsEventArgs)e).KeyValue.Key] = ((ParamsEventArgs)e).KeyValue.Value;
		}

		void ParametersCalculated(object sender, EventArgs e) {
			Dictionary<string,string> values = paramValues.Where(kvp => kvp.Value != string.Empty).
				ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
//			foreach (string key in values.Keys) {
//				Debug.Log (key + " " + values [key]);
//			}
//			Debug.Break ();
			//dbEntry.ParameterValues = Helper.SerializeObjectToBinary (values);
			dbEntry.ParameterValues = Helper.SerializeObjectToJSON (new PredicateParametersJSON(values));
		}

		void CaptureComplete(object sender, EventArgs e) {
			if (eventIndex != -1) {
				string[] args = new string[]{ eventIndex.ToString (), PlayerPrefs.GetString ("Listener Port") };
				string result = Marshal.PtrToStringAuto (PluginImport.PythonCall (Application.dataPath + "/Externals/python/", "auto_event_script", "send_next_event_to_port", args, args.Length));
				Debug.Log (result);
				eventIndex = System.Convert.ToInt32 (result);
			}
		}

		void OpenDB() {
			dbConnection = new SQLiteConnection (string.Format ("{0}.db", dbFile),
				SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create);

			dbConnection.CreateTable<VideoDBEntry> ();
		}

		void WriteToDB() {
			dbConnection.InsertAll (new[]{ dbEntry });
		}
	}
}