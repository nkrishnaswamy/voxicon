/*
 * Flashback Video Recorder
 * FlashbackRecorder.cs v1.2
 * 
 * Handles capturing video frames from the attached camera. Contains all 
 * input and output parameters. Frames are stored in memory until the
 * SaveCapturedFrames method is called by pressing the associated
 * keyboard key.
 * 
 * A non-blocking thread is spawned during the output process, resulting
 * in minimal performance impact. Frame capture will continue during the 
 * file creation process.
 * 
 * 
 * Copyright 2016 LaunchPoint Games
 * One license per seat. For all terms and conditions, see included 
 * documentation, or visit http://www.launchpointgames.com/unity/flashback.html
 */


using UnityEngine;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

namespace FlashbackVideoRecorder{

	[AddComponentMenu("Miscellaneous/Flashback Recorder")]
	[RequireComponent(typeof(Camera))]
	public class FlashbackRecorder : MonoBehaviour {

		/***
		 * 
		 * Event handlers
		 * 
		 ***/

		//Called when a new video file is saved to disk
		//Subscribe to the OnFlashbackFileCreated event to detect when captured video has been saved to disk
		public delegate void FileCreated(string filepath);
		public event FileCreated OnFlashbackFileCreated;


		/***
		 * 
		 * Variables exposed through the FlashbackEditor class
		 * 
		 ***/

		//Number of frames per second to record
		[SerializeField, Range(1, 30)]
		int m_FrameRate = 15;

		//The type of resolution capture (native, scale, manual)
		public enum ResolutionType { Native, Scale, Manual };

		//By default, capture the native resoltuion
		[SerializeField]
		ResolutionType m_ResolutionType = ResolutionType.Native;

		//Video output height
		[SerializeField]
		int m_Height = 360;

		//Video ouput width
		[SerializeField]
		int m_Width = 640;

		//How much to scale the native resolution for the Scale resolution type
		[SerializeField, Range(0.01f, 4.0f)]
		float m_ResolutionScale = 0.5f;

		//For the Manual resolution type, the width can automatically be set to preserve the screen aspect ratio
		[SerializeField]
		bool m_AutomaticWidth = true;

		//Should the 
		[SerializeField]
		bool m_ContinuousCapture = true;

		[SerializeField]
		bool m_IsCapturing = true;

		//How much history to record (in seconds)
		[SerializeField]
		float m_CaptureTime = 6.0f;

		//The keyboard key that will trigger file write
		[SerializeField]
		KeyCode m_CaptureKey = KeyCode.Space;

		//If true, the recorded video will be saved to a GIF
		[SerializeField]
		bool m_SaveGif = true;

		//If true, the recorded video will be saved to an MP4
		[SerializeField]
		bool m_SaveMp4 = true;

		[SerializeField]
		bool m_SaveOgg = true;

		//Should audio be captured along with video?
		[SerializeField]
		bool m_CaptureAudio = true;

		[SerializeField]
		Watermark[] m_watermarks;

		[SerializeField]
		Watermark m_watermark;

		/***
		 * 
		 * Accessor Methods
		 * 
		 ***/

		public int GetFrameRate() { return m_FrameRate; }
		public int GetNumberOfCapturedFrames() { return m_queue.Count; }
		public float GetMaxCaptureTime() { return m_CaptureTime; }
		public float GetCurrentCaptureTime() { return ((float)GetNumberOfCapturedFrames()) / ((float)GetFrameRate()); }
		public KeyCode GetCaptureKey(){ return m_CaptureKey; }
		public bool CaptureAudio(){ return m_CaptureAudio; }
		public bool ContinuousCapture(){ return m_ContinuousCapture; }
		public bool CanStartToggle() { return m_canStartToggle; }
		public ResolutionType GetResolutionType() { return m_ResolutionType; }
		public int GetHeight() { return m_Height; }
		public int GetWidth() { return m_Width; }

		public float GetResolutionScale() {
			if(m_ResolutionType == ResolutionType.Scale)
				return m_ResolutionScale;
			return (float)m_Height / (float)Screen.height;
		}

		public bool AutomaticWidth() { 
			if(m_ResolutionType == ResolutionType.Manual)
				return m_AutomaticWidth;
			return true;
		}

		public int GetNumberOfPendingFiles() { return m_numPendingFiles; }
		public bool SaveMP4(){ return m_SaveMp4; }
		public bool SaveGIF(){ return m_SaveGif; }
		public bool SaveOGG(){ return m_SaveOgg; }


		/***
		 * 
		 * FFMpeg directories and files
		 * 
		 ***/

		public static string FFmpegPackageDir() { return m_FFMpegStorageDir; }
		public static string FFmpegStandaloneDir() { return m_FFMpegStandaloneDir; }
		private static string m_FFMpegStorageDir;
		private static string m_FFMpegStandaloneDir;
		public static readonly string FFmpegWin = "ffmpegWin.exe";
		public static readonly string FFmpegMac = "ffmpegMac";

		//Called from FlashbackRecorder and FlashbackEditor
		//Sets up the Application specific paths to the FFmpeg binaries
		public static void ConfigureFFmpegDirectories(){
			m_FFMpegStorageDir = Application.dataPath + "/FlashbackRecorder/FFmpeg/";
			m_FFMpegStandaloneDir =  Application.streamingAssetsPath + "/FlashbackRecorder/";
		}

		/***
		 * 
		 * Private variables
		 * 
		 ***/

		//The queue contains the last m_CaptureTime seconds worth of video
		Queue<RenderTexture> m_queue;

		//The audio capture queue and file handler
		AudioCaptureWorker m_audio;

		//In certain circumstances, do not allow video capture to be toggled on
		bool m_canStartToggle = true;

		//Only allow file creation after initialization
		bool m_canWriteFrames = false;

		//If audio is being recorded, stop recording when the player is paused in Editor
		bool m_pauseAudioCapture = false;

		//Keeps track of when to take the next screen capture
		float m_timeSinceLastCapture = 0.0f;

		//The number of frames to store. Equals m_FrameRate * m_CaptureTime
		int m_frameCount = 0;

		//Delay between capturing each frame (based on m_FrameRate)
		float m_captureInterval;

		//Keeps track of the "drift" caused by a capture interval that doesn't line up with the delta time between frames
		float m_captureOffset = 0.0f;

		//Where the video files should be stored. Current defaults:
		// Editor: ./Assets/FlashbackRecorder/Output/
		// Standalone: <executable or app root>/FlashbackOutput/
		//Can be dynamically set with "SetOutputDirectory(string)"
		string m_outputDirectory = "";

		//Files created since last update
		List<string> m_createdFiles = new List<string>();

		//Don't modify the file creation list if it is being accessed by another thread
		bool m_FilesCallbackLock = false;

		//The current number of files that are being written to disk
		int m_numPendingFiles = 0;


		/***
		 * 
		 * Unity Methods
		 * 
		 ***/

		//Called once when the class is intiitialized. 
		//Determines the output directory and inititalizes default properties of the recorder.
		void Awake(){
			#if UNITY_EDITOR
			UnityEditor.EditorApplication.playmodeStateChanged += HandleEditorChange;
			#endif

			FlashbackRecorder.ConfigureFFmpegDirectories ();
			m_outputDirectory = GetDefaultOutputDirectory ();
			Init();
		}


		//Called when the class is garbage collected. 
		//Stops the capture coroutine and cleans up allocated resources.
		void OnDestroy(){
			ClearQueue();

			#if UNITY_EDITOR
			UnityEditor.EditorApplication.playmodeStateChanged -= HandleEditorChange;
			#endif

			foreach (Watermark m in m_watermarks) {
				m.CleanupPathToImage ();
			}
			if(Directory.Exists(m_outputDirectory + "/watermarks~/"))
				Directory.Delete (m_outputDirectory + "/watermarks~/", true);
		}


		//Check to see if the capture key was pressed or if files were created since the last update
		void Update () {
			CheckKeyDown ();
			CheckFilesCreated ();
		}

		//Record audio
		void OnAudioFilterRead(float[] data, int channels){
			if(!m_pauseAudioCapture)
				m_audio.CacheAudioData(data, channels);
		}


		void OnRenderImage(RenderTexture source, RenderTexture destination){

			if (GetComponent<Camera> () != null && 
				GetComponent<Camera> ().actualRenderingPath == RenderingPath.Forward) {
				CaptureFrame (source);
			}

			Graphics.Blit (source, destination);

		}


		void OnPostRender(){
			if (GetComponent<Camera> () != null && 
				GetComponent<Camera> ().actualRenderingPath != RenderingPath.Forward) {
				CaptureFrame (RenderTexture.active);
			}
		}


		/***
		 * 
		 * FlashbackRecorder Private Methods
		 * 
		 ***/

		//Returns the output directory. Can be overwritten here, or at runtime by calling SetOutputDirectory
		string GetDefaultOutputDirectory(){
			#if UNITY_EDITOR
			// When running in the editor, defaults to "FlashbackRecorder/Output" in the Asset folder
			return SanitizeDirectory(Application.dataPath + "/FlashbackRecorder/Output/"); 
			#else
			return GetRootFolder() + "FlashbackOutput/";
			#endif
		}


		//Configures and starts the core Flashback Recorder functionality
		void Init(){
			//Ensure the capture dimensions are valid
			SanitizeVideoDimensions ();

			string watermarkDir = m_outputDirectory + "watermarks~/";
			if (!Directory.Exists (watermarkDir)) {
				Directory.CreateDirectory (watermarkDir);
			}
			foreach (Watermark m in m_watermarks) {
				m.CreatePathToImage (watermarkDir, this);
			}

			//Calculate the maximum number of frames to store and how often each frame should be recorded
			m_frameCount = Mathf.CeilToInt(Mathf.Max(m_FrameRate * m_CaptureTime, 1.0f));
			m_captureInterval = 1.0f / m_FrameRate;

			//Create the queue used to keep track of the last m_CaptureTime seconds
			if (m_queue != null) {
				foreach (RenderTexture rt in m_queue)
					rt.Release ();
				m_queue.Clear ();
			}
			m_queue = new Queue<RenderTexture> (m_frameCount);

			//Create the audio capture device, and start by assuming audio will not be saved
			m_audio = new AudioCaptureWorker (m_CaptureTime);
			m_audio.CaptureAudio (false);

			//Set the default frame capture timer variables
			m_canWriteFrames = true;
			m_captureOffset = 0.0f;
			m_timeSinceLastCapture = 0.0f;

			m_IsCapturing = false;
		}


		//Increments the screen capture timer, and if necessary, kicks off a capture
		bool ShouldCaptureScreen(float deltaTime){

			if (!IsCapturingVideo ())
				return false;

			//Increment the timer, and check if we're due to capture the screen
			m_timeSinceLastCapture += deltaTime;
			if (m_timeSinceLastCapture < (m_captureInterval - m_captureOffset))
				return false;

			//Determine how much we overshot the m_captureInterval value, and compensate for the next screen shot
			m_captureOffset = m_timeSinceLastCapture - (m_captureInterval - m_captureOffset);
			m_timeSinceLastCapture = 0.0f;

			return true;
		}
			

		//Stores the current rendertexture in a queue
		void CaptureFrame(RenderTexture img){
			if (ShouldCaptureScreen (Time.unscaledDeltaTime)) {

				RenderTexture rt = GetRenderTexture ();

				Graphics.Blit (img, rt);
				m_queue.Enqueue (rt);

				//Start recording audio after we start recording video so the files line up correctly
				if (m_queue.Count == 1)
					m_audio.CaptureAudio (m_CaptureAudio);
			}
		}


		//Called before the current video is written to disk
		void BeginWritingProcess(){
			m_canWriteFrames = true;
		}


		//Caled after the current video has been written to disk
		void EndWritingProcess(){
			m_canWriteFrames = true;
			m_captureOffset = 0.0f;
			m_timeSinceLastCapture = 0.0f;
		}


		//If the editor is paused, make sure audio capture is paused, too
		#if UNITY_EDITOR
		public void HandleEditorChange(){
			m_pauseAudioCapture = UnityEditor.EditorApplication.isPaused;
		}
		#endif


		//Either returns the oldest rendertexture from the queue, or allocates a new one
		RenderTexture GetRenderTexture(){

			RenderTexture rt = null;
			if (m_ContinuousCapture && m_queue.Count >= m_frameCount) {
				rt = m_queue.Dequeue ();
			}

			if (rt == null){
				rt = new RenderTexture (m_Width, m_Height, 0, RenderTextureFormat.ARGB32);
				rt.wrapMode = TextureWrapMode.Clamp;
				rt.filterMode = FilterMode.Bilinear;
				rt.anisoLevel = 0;
			}

			return rt;
		}


		//This texture will store the raw data for each frame
		Texture2D GetTexture2D(){
			Texture2D currTex = new Texture2D (m_Width, m_Height, TextureFormat.RGB24, false);
			currTex.hideFlags = HideFlags.HideAndDontSave;
			currTex.wrapMode = TextureWrapMode.Clamp;
			currTex.filterMode = FilterMode.Trilinear;
			currTex.anisoLevel = 0;

			return currTex;
		}


		//Clears the memory associated with the queue
		void ClearQueue(){
			if (!m_FilesCallbackLock && m_queue != null) {
				foreach (RenderTexture rt in m_queue) {
					rt.Release ();
				}
				m_queue.Clear ();

				if (m_audio != null)
					m_audio.ClearAudioData ();
			}
		}


		//Ensures the video dimensions are valid (greater than 2, and even). Optionally calculates the output
		//width based on the height to keep the same aspect ratio as the rendered frame
		void SanitizeVideoDimensions(){

			//If native, just use the current screen width and height
			if (m_ResolutionType == ResolutionType.Native) {
				m_Height = Screen.height;
				m_Width = Screen.width;
			}

			//If scaling, make sure the scale is valid and then mulitply it by the native resolution
			if (m_ResolutionType == ResolutionType.Scale) {
				m_Height = Mathf.RoundToInt(Screen.height * m_ResolutionScale);
				m_Width = Mathf.RoundToInt(Screen.width * m_ResolutionScale);
			}

			//Ensure the height is an even number
			m_Height = Mathf.Max (m_Height, 2);
			if (m_Height % 2 == 1)
				m_Height--;

			//Does the width need to be calculated?
			if (m_ResolutionType == ResolutionType.Manual && m_AutomaticWidth)
				m_Width = Mathf.CeilToInt(GetComponent<Camera>().aspect * m_Height);

			//Ensure the width is an even number
			m_Width = Mathf.Max (m_Width, 2);
			if (m_Width % 2 == 1)
				m_Width--;
		}


		//Makes sure the output directory is valid (the path will depend on Runtime Platform)
		string SanitizeDirectory(string dir){

			if (dir == null)
				dir = GetDefaultOutputDirectory ();


			if ((Application.platform == RuntimePlatform.OSXPlayer || Application.platform == RuntimePlatform.OSXEditor) &&
				!dir.StartsWith ("/"))
				dir = "/" + dir;

			if (!dir.EndsWith ("/"))
				dir = dir + "/";

			if (!Directory.Exists (dir))
				Directory.CreateDirectory (dir);

			return dir;
		}


		//By default, the saved output is stored in files with the format "Flashback_MM-dd-HH-mm-ss"
		//The extesion will be added based on the format (gif and/or mp4)
		//ex: Flashback_01-21-14-22-02
		string GenerateFilename(){
			return "Flashback_" + DateTime.Now.ToString("yyyy-MM-dd-HHmmss");
		}


		//Makes sure the output filename is valid
		string SanitizeFilename(string filename){

			if (filename == null || filename.Length == 0) {
				return GenerateFilename ();
			}

			int indexOfDot = filename.IndexOf (".");
			if (indexOfDot > -1) {
				filename = filename.Substring (0, indexOfDot);
			}

			if (filename.Length == 0)
				return GenerateFilename ();

			return filename;
		}

		//Called from VideoCaptureThread after the file is created
		//Adds the filename to the stored list, which will be checked during the next update
		internal bool OnFileCreated(string filename){
			if (m_FilesCallbackLock)
				return false;

			m_FilesCallbackLock = true;
			m_numPendingFiles--;
			m_createdFiles.Add (filename);
			m_FilesCallbackLock = false;

			return true;
		}


		//Checks to see if the capture key was pressed. To disable video capture via a key press, set the 
		//m_CaptureKey to KeyCode.None
		void CheckKeyDown(){
			if (Input.GetKeyDown (m_CaptureKey)) {
				if (m_ContinuousCapture) {
					SaveCapturedFrames ();
				}else {
					if (!m_IsCapturing) {
						StartCapture ();
					} else {
						StopCapture ();
						SaveCapturedFrames ();
					}
				}
			}
		}

		string GetOutputDirectory(string outputDir, string filename){
			if (!Directory.Exists (outputDir))
				Directory.CreateDirectory (outputDir);

			string baseFilePath = outputDir + filename;
			int i = 0;
			while (Directory.Exists (baseFilePath)) {
				baseFilePath = outputDir + filename + "_" + i;
				i++;
			}

			return baseFilePath;
		}

		//Writes the files to disk based on the formats specified
		//Takes the filename and the queue of frames to turn into an MP4 and/or GIF
		IEnumerator WriteFilesToDisk(string filename, Queue<RenderTexture> queue){

			//Ensures the filename is valid (no extension, is not null, etc)
			filename = SanitizeFilename (filename);
			string outputPath = GetOutputDirectory (m_outputDirectory, filename);

			//If audio is being captured, write it out to a file
			string audioPath = null;
			if(m_CaptureAudio)
				audioPath = m_audio.WriteAudioToFile(outputPath);

			//Keep track of how many files are going to be written to disk
			if (m_SaveGif)
				m_numPendingFiles++;

			if (m_SaveMp4)
				m_numPendingFiles++;

			if (m_SaveOgg) {
				m_numPendingFiles++;
			}


			//Get the frames in order
			RenderTexture[] array = queue.ToArray ();

			//This texture will store the raw data for each frame
			Texture2D currTex = GetTexture2D();

			byte[][] framesToWrite = new byte[array.Length][];

			//For each frame, get the raw texture data and store it 
			for(int i = 0; i < array.Length; i++){
				RenderTexture rt = array [i];
				framesToWrite [i] = GetBytesFromTexture(rt, currTex);

				//After processing a frame, wait before processing the next one to prevent performance hiccups
				yield return null;
			}

			//Spawn a new thread to write the captured video data to disk
			VideoCaptureWorker captureThread = new VideoCaptureWorker () {
				m_frames = framesToWrite,
				m_filedir = outputPath,
				m_inputWidth = this.m_Width,
				m_inputHeight = this.m_Height,
				m_outputWidth = this.m_Width,
				m_outputHeight = this.m_Height,
				m_framerate = this.m_FrameRate,
				m_outputToGif = this.m_SaveGif,
				m_outputToMp4 = this.m_SaveMp4,
				m_outputToOog = this.m_SaveOgg,
				m_audioFile = audioPath,
				m_watermarks = this.m_watermarks,
				m_recorder = this
			};
			captureThread.WriteToDisk ();

			EndWritingProcess ();
		}

		byte[] GetBytesFromTexture(RenderTexture source, Texture2D target){
			RenderTexture old = RenderTexture.active;
			RenderTexture.active = source;

			target.ReadPixels (new Rect (0, 0, source.width, source.height), 0, 0);
			byte[] buffer = target.GetRawTextureData ();
			byte[] frame = new byte[buffer.Length];
			Array.Copy (buffer, frame, buffer.Length);

			RenderTexture.active = old;

			return frame;
		}


		//If video files were written to disk since the last update
		void CheckFilesCreated(){
			if (!m_FilesCallbackLock && m_createdFiles.Count > 0) {
				m_FilesCallbackLock = true;
				foreach (string filename in m_createdFiles) {
					if (OnFlashbackFileCreated != null)
						OnFlashbackFileCreated (filename);
				}

				m_createdFiles.Clear ();
				m_FilesCallbackLock = false;
				m_canStartToggle = true;
			}
		}


		/***
		 * 
		 * FlashbackRecorder Public Interface
		 * 
		 ***/

		//Allows you to define which keyboard key will start the file creation process.
		//Pass in KeyCode.None if you want to explicitly call the SaveCapturedFrames 
		//method directly
		public void SetCaptureKey(KeyCode key){
			m_CaptureKey = key;
		}


		//If true, then the last m_CaptureTime seconds of video will be saved. 
		//If false, then the recorder will be toggled on/off
		public void SetContinuousCapture(bool continuous){
			ClearQueue ();

			m_ContinuousCapture = continuous;

			Init ();
		}


		//Sets the capture resolution to the native screen size
		//Changing the capture resoltuion always requires that all stored video history be discarded
		public void SetResolutionToNative(){
			ClearQueue ();

			m_ResolutionType = ResolutionType.Native;

			Init ();
		}


		//Sets the capture resolution to the native resolution scaled by the specified value
		//Changing the capture resoltuion always requires that all stored video history be discarded
		public void SetResolutionByScale(float scale){
			ClearQueue ();

			m_ResolutionScale = scale;
			m_ResolutionType = ResolutionType.Scale;

			Init ();
		}


		//Sets the capture resolution explicitly. 
		//If the width is set to "-1", it will be calculated automatically based on the camera's aspect ratio
		//Changing the capture resoltuion always requires that all stored video history be discarded
		public void SetResolutionByValue(int height, int width){
			ClearQueue ();

			m_AutomaticWidth = (width == -1);
			m_Height = height;
			m_Width = width;

			m_ResolutionType = ResolutionType.Manual;

			Init ();
		}


		//A helper method that returns the root path of the game binary
		public string GetRootFolder(){

			string rootFolder = "";

			string[] splitPath = Application.dataPath.Split ('/');
			int offset = 1;
			if (Application.platform == RuntimePlatform.OSXPlayer)
				offset = 2;

			for(int i = 0; i < splitPath.Length - offset; i++){
				rootFolder = rootFolder + splitPath[i] + "/";
			}

			return SanitizeDirectory (rootFolder);
		}


		//The default output directory can be overwritten programatically
		public void SetOutputDirectory(string dir){
			m_outputDirectory = SanitizeDirectory(dir);

			foreach (Watermark m in m_watermarks) {
				m.CreatePathToImage (m_outputDirectory, this);
			}
		}


		//Public interface to delete saved frame data
		public void ClearStoredFrames(){
			ClearQueue ();
		}


		//Set whether audio will be captured, requires stored video history to be discarded
		public void CaptureAudio(bool saveAudio){
			if (m_CaptureAudio != saveAudio) {
				ClearQueue ();
				m_CaptureAudio = saveAudio;
				Init ();
			}
		}


		//Set whether the captured video will be written to a GIF file
		public void SaveGIF(bool saveGif){
			if (Application.isPlaying)
				this.m_SaveGif = saveGif;
		}


		//Set whether the captured video will be written to an MP4 file
		public void SaveMP4(bool saveMP4){
			if (Application.isPlaying)
				this.m_SaveMp4 = saveMP4;
		}


		//Set whether the captured video will be written to an OGG file
		public void SaveOGG(bool saveOGG){
			if (Application.isPlaying)
				this.m_SaveOgg = saveOGG;
		}


		//If any recording parameters change during runtime, recording is stopped, all history is cleared, and 
		//the settings are re-initialiazed
		public void UpdateCaptureSettings(float captureTime, int framerate){
			ClearQueue ();

			m_FrameRate = framerate;
			m_CaptureTime = captureTime;

			Init ();
		}
			

		//If ContinuousCapture() is false, toggles video capture on 
		//Returns true if video capture started
		public bool StartCapture(){
			if (IsCapturingVideo() || !m_canStartToggle)
				return false;

			Init ();
			m_IsCapturing = true;

			return true;
		}


		//If ContinuousCapture() is false, toggles video capture off
		//Returns true if video capture stopped
		public bool StopCapture(){
			if (!IsCapturingVideo())
				return false;

			m_IsCapturing = false;
			m_canStartToggle = false;

			return true;
		}


		//Return true if frames are being saved
		public bool IsCapturingVideo(){
			if (m_ContinuousCapture || m_IsCapturing)
				return true;
			return false;
		}


		//Returns true if the "SaveCapturedFrames" function can be executed, false otherwise
		public bool CanSaveCapturedFrames(){
			if (m_queue.Count > 1 && (m_SaveGif || m_SaveMp4 || m_SaveOgg) && m_canWriteFrames)
				return true;

			return false;
		}


		//Saves the rendered frames with an automatically generated filename
		//Returns true if the save started successfully
		public bool SaveCapturedFrames(){
			return SaveCapturedFrames (GenerateFilename ());
		}


		//Saves the rendered frames with the given filename
		//Returns true if the save started successfully
		//Filename should not contain an extension (ex: use "myVideo" not "myVideo.mp4")
		//Filename can contain directories (ex: "level3/myVideo" becomes <m_outputDirectory>/level3/myVideo)
		public bool SaveCapturedFrames(string filename){
			if (CanSaveCapturedFrames()) {
				BeginWritingProcess ();
				StartCoroutine (WriteFilesToDisk (filename, m_queue));
				return true;
			}

			return false;
		}
	}
}