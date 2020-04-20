using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

[CustomEditor(typeof(PSXEffects))]
public class PSXEffectsEditor : Editor {

	SerializedProperty downscale;
	SerializedProperty resolutionVert;
	SerializedProperty resolutionFactor;
	SerializedProperty limitFramerate;
	SerializedProperty affineMapping;
	SerializedProperty polygonalDrawDistance;
	SerializedProperty vertexInaccuracy;
	SerializedProperty polygonInaccuracy;
	SerializedProperty colorDepth;
	SerializedProperty scanlines;
	SerializedProperty scanlineIntensity;
	SerializedProperty dithering;
	SerializedProperty ditherType;
	SerializedProperty ditherThreshold;
	SerializedProperty ditherIntensity;
	SerializedProperty maxDarkness;
	SerializedProperty subtractFade;
	SerializedProperty skyboxLighting;
	SerializedProperty favorRed;
	SerializedProperty postProcessing;
	SerializedProperty verticalScanlines;
	SerializedProperty snapCamera;
	SerializedProperty camInaccuracy;
	SerializedProperty worldSpaceSnapping;
	SerializedProperty camSnapping;

	void OnEnable() {
		resolutionFactor = serializedObject.FindProperty("resolutionFactor");
		limitFramerate = serializedObject.FindProperty("limitFramerate");
		affineMapping = serializedObject.FindProperty("affineMapping");
		polygonalDrawDistance = serializedObject.FindProperty("polygonalDrawDistance");
		vertexInaccuracy = serializedObject.FindProperty("vertexInaccuracy");
		polygonInaccuracy = serializedObject.FindProperty("polygonInaccuracy");
		colorDepth = serializedObject.FindProperty("colorDepth");
		scanlines = serializedObject.FindProperty("scanlines");
		scanlineIntensity = serializedObject.FindProperty("scanlineIntensity");
		dithering = serializedObject.FindProperty("dithering");
		ditherType = serializedObject.FindProperty("ditherTexture");
		ditherThreshold = serializedObject.FindProperty("ditherThreshold");
		ditherIntensity = serializedObject.FindProperty("ditherIntensity");
		maxDarkness = serializedObject.FindProperty("maxDarkness");
		subtractFade = serializedObject.FindProperty("subtractFade");
		skyboxLighting = serializedObject.FindProperty("skyboxLighting");
		favorRed = serializedObject.FindProperty("favorRed");
		postProcessing = serializedObject.FindProperty("postProcessing");
		verticalScanlines = serializedObject.FindProperty("verticalScanlines");
		downscale = serializedObject.FindProperty("downscale");
		resolutionVert = serializedObject.FindProperty("customRes");
		snapCamera = serializedObject.FindProperty("snapCamera");
		camInaccuracy = serializedObject.FindProperty("camInaccuracy");
		worldSpaceSnapping = serializedObject.FindProperty("worldSpaceSnapping");
		camSnapping = serializedObject.FindProperty("camSnapping");
	}

	public override void OnInspectorGUI() {
		serializedObject.Update();

		EditorGUILayout.LabelField("Video Output", EditorStyles.boldLabel);
		downscale.boolValue = EditorGUILayout.Toggle("Custom Resolution", downscale.boolValue);
		if (downscale.boolValue) {
			resolutionVert.vector2IntValue = EditorGUILayout.Vector2IntField("Resolution", resolutionVert.vector2IntValue);
		} else {
			resolutionFactor.intValue = EditorGUILayout.IntField("Resolution Factor", resolutionFactor.intValue);
		}
		limitFramerate.intValue = EditorGUILayout.IntField("Limit Framerate", limitFramerate.intValue);
		snapCamera.boolValue = EditorGUILayout.Toggle("Enable Camera Position Inaccuracy", snapCamera.boolValue);
		if (snapCamera.boolValue) {
			camInaccuracy.floatValue = EditorGUILayout.FloatField("Camera Inaccuracy", camInaccuracy.floatValue);
		}
		EditorGUILayout.Separator();

		EditorGUILayout.LabelField("Mesh Settings", EditorStyles.boldLabel);
		affineMapping.boolValue = EditorGUILayout.Toggle("Affine Texture Mapping", affineMapping.boolValue);
		polygonalDrawDistance.floatValue = EditorGUILayout.FloatField("Polygonal Draw Distance", polygonalDrawDistance.floatValue);
		polygonInaccuracy.intValue = EditorGUILayout.IntField("Polygon Inaccuracy", polygonInaccuracy.intValue);
		vertexInaccuracy.intValue = EditorGUILayout.IntField("Vertex Inaccuracy", vertexInaccuracy.intValue);
		worldSpaceSnapping.boolValue = EditorGUILayout.Toggle("Use World Space Snapping", worldSpaceSnapping.boolValue);
		if (worldSpaceSnapping.boolValue) {
			camSnapping.boolValue = EditorGUILayout.Toggle("Camera-Based Snapping", camSnapping.boolValue);
		}
		maxDarkness.intValue = EditorGUILayout.IntSlider("Saturated Diffuse", maxDarkness.intValue, 0, 100);
		skyboxLighting.boolValue = EditorGUILayout.Toggle("Use Skybox Lighting", skyboxLighting.boolValue);
		EditorGUILayout.Separator();

		EditorGUILayout.LabelField("Post Processing", EditorStyles.boldLabel);
		postProcessing.boolValue = EditorGUILayout.Toggle("Enable Post Processing", postProcessing.boolValue);
		if (postProcessing.boolValue) {
			colorDepth.intValue = EditorGUILayout.IntSlider("Color Depth", colorDepth.intValue, 1, 24);
			subtractFade.intValue = EditorGUILayout.IntSlider("Subtraction Fade", subtractFade.intValue, 0, 100);
			favorRed.floatValue = EditorGUILayout.FloatField("Darken Darks/Favor Red", favorRed.floatValue);
			scanlines.boolValue = EditorGUILayout.Toggle("Scanlines", scanlines.boolValue);
			if (scanlines.boolValue) {
				verticalScanlines.boolValue = EditorGUILayout.Toggle("Vertical", verticalScanlines.boolValue);
				scanlineIntensity.intValue = EditorGUILayout.IntSlider("Scanline Intensity", scanlineIntensity.intValue, 0, 100);
			}
			dithering.boolValue = EditorGUILayout.Toggle("Enable Dithering", dithering.boolValue);
			if (dithering.boolValue) {
				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.PrefixLabel("Dither Texture");
				ditherType.objectReferenceValue = (Texture2D)EditorGUILayout.ObjectField(ditherType.objectReferenceValue, typeof(Texture2D), false);
				EditorGUILayout.EndHorizontal();
				ditherThreshold.floatValue = EditorGUILayout.FloatField("Dither Threshold", ditherThreshold.floatValue);
				ditherIntensity.intValue = EditorGUILayout.IntSlider("Dither Intensity", ditherIntensity.intValue, 0, 100);
			}
		}
		EditorGUILayout.Separator();

		if (GUILayout.Button("Rate on the Asset Store!")) {
			Application.OpenURL("https://assetstore.unity.com/packages/vfx/shaders/psxeffects-132368");
		}

		serializedObject.ApplyModifiedProperties();
	}
}
