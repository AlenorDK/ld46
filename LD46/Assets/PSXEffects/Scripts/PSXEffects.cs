﻿using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

[RequireComponent(typeof(Camera))]
[ExecuteInEditMode]
public class PSXEffects : MonoBehaviour {

	public Vector2Int customRes = new Vector2Int(620, 480);
	public int resolutionFactor = 1;
	public int limitFramerate = -1;
	public bool affineMapping = true;
	public float polygonalDrawDistance = 0f;
	public int vertexInaccuracy = 30;
	public int polygonInaccuracy = 10;
	public int colorDepth = 5;
	public bool scanlines = false;
	public int scanlineIntensity = 5;
	public Texture2D ditherTexture;
	public bool dithering = true;
	public float ditherThreshold = 1;
	public int ditherIntensity = 35;
	public int maxDarkness = 20;
	public int subtractFade = 0;
	public bool skyboxLighting = false;
	public float favorRed = 1.0f;
	public bool worldSpaceSnapping = false;
	public bool postProcessing = true;
	public bool verticalScanlines = true;
	public float shadowIntensity = 0.5f;
	public bool downscale = false;
	public bool snapCamera = true;
	public float camInaccuracy = 0.05f;
	public bool camSnapping = false;

	private Camera cam;
	private Material colorDepthMat;
	private RenderTexture rt;

	void Awake() {
		if (Application.isPlaying) {
			QualitySettings.vSyncCount = 0;
		}

		QualitySettings.antiAliasing = 0;
	}

	void Update() {
		if (!downscale) {
			customRes = new Vector2Int(Screen.width / resolutionFactor, Screen.height / resolutionFactor);
		}

		// Set mesh shader variables
		Shader.SetGlobalFloat("_AffineMapping", affineMapping ? 1.0f : 0.0f);
		Shader.SetGlobalFloat("_DrawDistance", polygonalDrawDistance);
		Shader.SetGlobalInt("_VertexSnappingDetail", vertexInaccuracy / 2);
		Shader.SetGlobalInt("_Offset", polygonInaccuracy);
		Shader.SetGlobalFloat("_DarkMax", (float)maxDarkness / 100);
		Shader.SetGlobalFloat("_SubtractFade", (float)subtractFade / 100);
		Shader.SetGlobalFloat("_SkyboxLighting", skyboxLighting ? 1.0f : 0.0f);
		Shader.SetGlobalFloat("_WorldSpace", worldSpaceSnapping ? 1.0f : 0.0f);
		Shader.SetGlobalFloat("_CamPos", camSnapping ? 1.0f : 0.0f);


		if (postProcessing) {
			// Handles all post processing variables
			if (colorDepthMat == null) {
				colorDepthMat = new Material(Shader.Find("Hidden/PS1ColorDepth"));
			} else {
				colorDepthMat.SetFloat("_ColorDepth", colorDepth);
				colorDepthMat.SetFloat("_Scanlines", scanlines ? 1 : 0);
				colorDepthMat.SetFloat("_ScanlineIntensity", (float)scanlineIntensity / 100);
				colorDepthMat.SetTexture("_DitherTex", ditherTexture);
				colorDepthMat.SetFloat("_Dithering", dithering ? 1 : 0);
				colorDepthMat.SetFloat("_DitherThreshold", ditherThreshold);
				colorDepthMat.SetFloat("_DitherIntensity", (float)ditherIntensity / 100);
				colorDepthMat.SetFloat("_ResX", customRes.x);
				colorDepthMat.SetFloat("_ResY", customRes.y);
				colorDepthMat.SetFloat("_FavorRed", favorRed);
				colorDepthMat.SetFloat("_SLDirection", verticalScanlines ? 1 : 0);
			}
		}

		// Set target framerate
		if (limitFramerate > 0) {
			Application.targetFrameRate = limitFramerate;
		} else {
			Application.targetFrameRate = -1;
		}
	}

	void LateUpdate() {
		if (snapCamera && Application.isPlaying) {
			// Handles the camera position snapping
			if (transform.parent == null || !transform.parent.name.Contains("CameraRealPosition")) {
				GameObject newParent = new GameObject("CameraRealPosition");
				newParent.transform.position = transform.position;
				if (transform.parent)
					newParent.transform.SetParent(transform.parent);
				transform.SetParent(newParent.transform);
			}

			Vector3 snapPos = transform.parent.position;
			snapPos /= camInaccuracy;
			snapPos = new Vector3(Mathf.Round(snapPos.x), Mathf.Round(snapPos.y), Mathf.Round(snapPos.z));
			snapPos *= camInaccuracy;
			transform.position = snapPos;
		} else if(transform.parent != null && transform.parent.name.Contains("CameraRealPosition")) {
			Destroy(transform.parent.gameObject);
		}
	}

	// Draw a transparent red circle around the camera to show its
	// real position
	void OnDrawGizmos() {
		if (snapCamera) {
			Gizmos.color = new Color(1, 0, 0, 0.5f);
			if(transform.parent != null)
				Gizmos.DrawSphere(transform.parent.position, 0.5f);
		}
	}

	void OnRenderImage(RenderTexture src, RenderTexture dst) {
		if (postProcessing) {
			if (customRes.x > 0 && customRes.y > 0) {
				// Renders scene to downscaled render texture using
				// the post processing shader
				if (src != null)
					src.filterMode = FilterMode.Point;
				RenderTexture rt = RenderTexture.GetTemporary(customRes.x, customRes.y);
				rt.filterMode = FilterMode.Point;
				Graphics.Blit(src, rt);
				Graphics.Blit(rt, dst, colorDepthMat);
				RenderTexture.ReleaseTemporary(rt);
			} else {
				Debug.LogError("Downscale resolution width and height must be greater than zero.");
			}
		} else {
			// Renders scene to downscaled render texture
			if (src != null)
				src.filterMode = FilterMode.Point;
			RenderTexture rt = RenderTexture.GetTemporary(customRes.x, customRes.y);
			rt.filterMode = FilterMode.Point;
			Graphics.Blit(src, rt);
			Graphics.Blit(rt, dst);
			RenderTexture.ReleaseTemporary(rt);
		}
	}
}