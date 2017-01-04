/*
 * Flashback Video Recorder
 * FlashbackDemo.cs v1.2
 * 
 * Shows how to use Flashback Video Recorder. Demonstrates how to capture video 
 * with the built-in Capture Key, or through code with a function call. Also 
 * allows the user to dynamically adjust some capture and output settings.
 * 
 * 
 * Copyright 2016 LaunchPoint Games
 * One license per seat. For all terms and conditions, see included 
 * documentation, or visit http://www.launchpointgames.com/unity/flashback.html
 */


using UnityEngine;
using System.Collections;
using FlashbackVideoRecorder;

public class FlashbackDemo : MonoBehaviour {

	//Private variables
	private FlashbackRecorder m_recorder;
	private string m_lastFileCreated = "None";

	//UI elements that can change at runtime
	public UnityEngine.UI.Text m_instructions;
	public UnityEngine.UI.Text m_status;

	//Toggle buttons
	public UnityEngine.UI.Toggle m_saveGif;
	public UnityEngine.UI.Toggle m_saveMp4;



	// Use this for initialization
	void Start () {
		//Grab the FlashbackRecorder associated with this camera
		m_recorder = GetComponent<FlashbackRecorder> ();

		//Subscribe to the file creation delegate
		m_recorder.OnFlashbackFileCreated += OnFileCreated;

		if (m_saveGif != null)
			m_saveGif.isOn = m_recorder.SaveGIF ();

		if (m_saveMp4 != null)
			m_saveMp4.isOn = m_recorder.SaveMP4 ();
	}


	//Unsubscribe from the file creation delegate when this object is destoryed
	void OnDestroy(){
		m_recorder.OnFlashbackFileCreated -= OnFileCreated;
	}


	// Update is called once per frame
	void UpdateGUI () {

		//Instruction label default: Press space to record the last 6 seconds.
		if (m_instructions != null) {
			if (m_recorder.IsCapturingVideo ()) {
				m_instructions.text = string.Format ("Press {0} to save the last {1} seconds.", 
					m_recorder.GetCaptureKey ().ToString (), m_recorder.GetCurrentCaptureTime ().ToString ("n1"));
			} else {
				if(m_recorder.CanStartToggle())
					m_instructions.text = string.Format ("Press {0} to start video capture.", 
						m_recorder.GetCaptureKey ().ToString ());
				else 
					m_instructions.text = string.Format ("Processing... wait to start capture.", 
						m_recorder.GetCaptureKey ().ToString ());
			}
		}

		//Indicates whether files are being written to disk. If not, shows the full path of the last file written
		string status = "";
		int numPendingFiles = m_recorder.GetNumberOfPendingFiles ();
		if (numPendingFiles > 0) {
			status = string.Format ("Writing files to disk...", m_recorder.GetNumberOfPendingFiles ());
		} else {
			status = string.Format ("Last file written to disk: {0}", m_lastFileCreated);
		}
		m_status.text = status;
	}


	//The escape key will quit the demo
	void Update(){
		if (Input.GetKeyDown (KeyCode.Escape)) {
			Application.Quit ();
		}

		UpdateGUI ();
	}


	//Called from the "Capture Button" GUI element
	public void StartCaptureButtonPressed(){
		Debug.Log ("Button clicked");
		//Save the video to disk
		if (m_recorder.ContinuousCapture ()) {
			m_recorder.SaveCapturedFrames ();
		} else {
			//Toggle on/off  video recording
			if (!m_recorder.IsCapturingVideo ()) {
				m_recorder.StartCapture ();
			} else {
				m_recorder.StopCapture ();
				m_recorder.SaveCapturedFrames ();
			}
		}
	}


	//Delegate that gets called when a video file is written to disk
	public void OnFileCreated(string file){
		m_lastFileCreated = file;
		Debug.Log ("File was created: " + file);
	}
}
