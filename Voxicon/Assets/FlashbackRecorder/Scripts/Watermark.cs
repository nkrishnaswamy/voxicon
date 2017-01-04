using UnityEngine;
using System.IO;
using System.Collections;

namespace FlashbackVideoRecorder{

	[System.Serializable]
	public class Watermark {

		[SerializeField]
		private Texture2D m_Image;

		[SerializeField]
		public enum HorizontalAlignment{LEFT, CENTER, RIGHT};

		[SerializeField]
		public enum VerticalAlignment{TOP, CENTER, BOTTOM};

		[SerializeField]
		public enum ScaleType{Image, VideoHeight, VideoWidth};

		[SerializeField]
		private HorizontalAlignment m_horizontalAlignment;

		[SerializeField]
		private int m_horizontalOffset;

		[SerializeField]
		private VerticalAlignment m_verticalAlignment;

		[SerializeField]
		private int m_verticalOffset;

		[SerializeField]
		private ScaleType m_ScaleBy;

		[SerializeField]
		private float m_ScaleAmount = 1.0f;



		private string m_pathToImage;

		public void SetImage(Texture2D img, string basePath, FlashbackRecorder fbr){
			m_Image = img;
			CreatePathToImage (basePath, fbr);
		}

		public string CreatePathToImage(string basePath, FlashbackRecorder fbr){
			CleanupPathToImage ();

			if (m_Image == null)
				return null;

			m_pathToImage = basePath + "watermark.png";
			int i = 0;
			while (File.Exists (m_pathToImage)) {
				m_pathToImage = basePath + "watermark_" + i + ".png";
				i++;
			}

			m_ScaleAmount = Mathf.Clamp (m_ScaleAmount, 0.01f, 100.0f);

			Texture2D watermarkImg = ScaleWatermark (fbr.GetWidth(), fbr.GetHeight());
			if (watermarkImg != null) {
				byte[] pngData = watermarkImg.EncodeToPNG ();
				FileStream pngStream = new FileStream (m_pathToImage, FileMode.Create, FileAccess.Write);
				pngStream.Write (pngData, 0, pngData.Length);
				pngStream.Close ();

				return m_pathToImage;
			}

			return null;
		}

		public void CleanupPathToImage(){
			if (m_pathToImage == null || !File.Exists(m_pathToImage))
				return;

			File.Delete (m_pathToImage);
			m_pathToImage = null;
		}

		public string GetImagePath(){
			return m_pathToImage;
		}

		public string GetHorizontalString(){

			string horizontalString = "x=0";

			if (m_horizontalAlignment == HorizontalAlignment.CENTER) {
				if (m_horizontalOffset > 0) {
					horizontalString = string.Format ("x=((main_w-overlay_w)/2)+{0}", m_horizontalOffset);
				} else {
					horizontalString = string.Format ("x=(main_w-overlay_w)/2-{0}", -m_horizontalOffset);
				}
			}

			if (m_horizontalAlignment == HorizontalAlignment.LEFT) {
				horizontalString = string.Format("{0}", m_horizontalOffset);
			}

			if (m_horizontalAlignment == HorizontalAlignment.RIGHT) {
				if (m_horizontalOffset > 0) {
					horizontalString = string.Format ("x=(main_w-overlay_w+{0})", m_horizontalOffset);
				} else {
					horizontalString = string.Format ("x=(main_w-overlay_w-{0})", -m_horizontalOffset);
				}
			}

			return horizontalString;
		}

		public string GetVerticalString(){
			if (m_verticalAlignment == VerticalAlignment.CENTER) {
				return "y=(main_h-overlay_h)/2";
			}

			if (m_verticalAlignment == VerticalAlignment.TOP) {
				return string.Format ("{0}", m_verticalOffset);
			}

			if (m_verticalAlignment == VerticalAlignment.BOTTOM) {
				return string.Format ("y=(main_h-overlay_h-{0})", m_verticalOffset);
			}

			return "y=0";
		}

		private Texture2D ScaleWatermark(int videoWidth, int videoHeight){

			if (m_Image == null || m_Image.width == 0 || m_Image.height == 0)
				return null;

			int newHeight = m_Image.height;
			int newWidth = m_Image.width;
			float ratio = newHeight / newWidth;

			if (m_ScaleBy == ScaleType.Image) {
				newHeight = (int)(newHeight * m_ScaleAmount);
				newWidth = (int)(newWidth * m_ScaleAmount);
			} else if (m_ScaleBy == ScaleType.VideoHeight) {
				newHeight = (int)(videoHeight * m_ScaleAmount);
				newWidth = (int)(newHeight / ratio);
			} else if (m_ScaleBy == ScaleType.VideoWidth) {
				newWidth = (int)(videoWidth * m_ScaleAmount);
				newHeight = (int)(newWidth * ratio);
			}

			Texture2D watermarkImg = ScaleTexture (m_Image, newWidth, newHeight);

			return watermarkImg;
		}

		private Texture2D ScaleTexture(Texture2D source,int targetWidth,int targetHeight) {
			Texture2D result=new Texture2D(targetWidth,targetHeight,source.format,false);
			for (int i = 0; i < result.height; ++i) {
				for (int j = 0; j < result.width; ++j) {
					Color newColor = source.GetPixelBilinear((float)j / (float)result.width, (float)i / (float)result.height);
					result.SetPixel(j, i, newColor);
				}
			}
			result.Apply();
			return result;
		}
	}
}