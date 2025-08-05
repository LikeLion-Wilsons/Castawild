using UnityEngine;
using UnityEditor;
using BKPureNature;

namespace BKPureNature
{
    [CustomEditor(typeof(BK_EnvironmentManager))]
    public class BK_EnvironmentManagerEditor : Editor
    {
        private Editor materialEditor;

        private SerializedProperty overrideSunColorProp;
        private SerializedProperty overrideFogColorProp;
        private SerializedProperty overrideCloudColorProp;
        private SerializedProperty overrideAmbientColorProp;

        private bool lightingFoldout = true;
        private bool windFoldout = true;
        private bool grassFoldout = true;
        private bool cloudsFoldout = true;
        private bool networkWeatherFoldout = true;
        private bool developerFoldout = true; // Foldout for the new dev mode

        private void OnEnable()
        {
            overrideSunColorProp = serializedObject.FindProperty("overrideSunColor");
            overrideFogColorProp = serializedObject.FindProperty("overrideFogColor");
            overrideCloudColorProp = serializedObject.FindProperty("overrideCloudColor");
            overrideAmbientColorProp = serializedObject.FindProperty("overrideAmbientColor");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            GUIStyle foldoutStyle = new GUIStyle(EditorStyles.foldout);
            foldoutStyle.fontStyle = FontStyle.Bold;

            GUIStyle boxStyle = new GUIStyle(GUI.skin.box);
            boxStyle.margin = new RectOffset(2, 2, 2, 2);
            boxStyle.padding = new RectOffset(5, 5, 5, 5);

            BK_EnvironmentManager EMscript = (BK_EnvironmentManager)target;

            // Lighting Foldout
            EditorGUILayout.Space(10);
            EditorGUILayout.BeginVertical(boxStyle);
            lightingFoldout = EditorGUILayout.Foldout(lightingFoldout, "Global Lighting", foldoutStyle);
            if (lightingFoldout)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("directionalLight"), new GUIContent("Directional Light"));
                EditorGUILayout.BeginHorizontal();
                overrideSunColorProp.boolValue = EditorGUILayout.ToggleLeft("Sun", overrideSunColorProp.boolValue, GUILayout.Width(70));
                using (new EditorGUI.DisabledScope(!overrideSunColorProp.boolValue))
                {
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("sunColorGradient"), GUIContent.none);
                }
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
                overrideFogColorProp.boolValue = EditorGUILayout.ToggleLeft("Fog", overrideFogColorProp.boolValue, GUILayout.Width(70));
                using (new EditorGUI.DisabledScope(!overrideFogColorProp.boolValue))
                {
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("fogColorGradient"), GUIContent.none);
                }
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
                overrideCloudColorProp.boolValue = EditorGUILayout.ToggleLeft("Clouds", overrideCloudColorProp.boolValue, GUILayout.Width(70));
                using (new EditorGUI.DisabledScope(!overrideCloudColorProp.boolValue))
                {
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("scatteringColorGradient"), GUIContent.none);
                }
                Rect gradientRect = GUILayoutUtility.GetLastRect();
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
                overrideAmbientColorProp.boolValue = EditorGUILayout.ToggleLeft("Ambient", overrideAmbientColorProp.boolValue, GUILayout.Width(70));
                using (new EditorGUI.DisabledScope(!overrideAmbientColorProp.boolValue))
                {
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("ambientColorGradient"), GUIContent.none);
                }
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(gradientRect.x + 77);
                GUILayout.Label("☼", GUILayout.Width(20));
                GUILayout.FlexibleSpace();
                GUILayout.Label("☽", GUILayout.Width(20));
                GUILayout.Space(5);
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndVertical();

            // Developer Mode Foldout
            EditorGUILayout.Space(10);
            EditorGUILayout.BeginVertical(boxStyle);
            developerFoldout = EditorGUILayout.Foldout(developerFoldout, "Developer Mode", foldoutStyle);
            if (developerFoldout)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("developerMode"));
                EditorGUILayout.HelpBox("If checked, automatic weather change is disabled. The host can then use number keys (1-9) to trigger the corresponding weather preset.", MessageType.Info);
            }
            EditorGUILayout.EndVertical();

            // Networked Weather Foldout
            EditorGUILayout.Space(10);
            EditorGUILayout.BeginVertical(boxStyle);
            networkWeatherFoldout = EditorGUILayout.Foldout(networkWeatherFoldout, "Networked Weather (Server Only)", foldoutStyle);
            if (networkWeatherFoldout)
            {
                EditorGUILayout.HelpBox("These settings control the automatic, networked weather changes. They are only used by the server/host and are ignored if Developer Mode is on.", MessageType.Info);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("weatherPresets"), true);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("minChangeInterval"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("maxChangeInterval"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("transitionDuration"));
            }
            EditorGUILayout.EndVertical();

            // Wind Foldout
            EditorGUILayout.Space(10);
            EditorGUILayout.BeginVertical(boxStyle);
            windFoldout = EditorGUILayout.Foldout(windFoldout, "Wind (Editor Preview)", foldoutStyle);
            if (windFoldout)
            {
                EditorGUILayout.HelpBox("These values are for editor preview only. In-game wind is controlled by the Networked Weather presets.", MessageType.Info);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("baseWindPower"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("baseWindSpeed"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("burstsPower"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("burstsSpeed"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("burstsScale"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("microPower"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("microSpeed"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("microFrequency"));
            }
            EditorGUILayout.EndVertical();

            // Grass Foldout
            EditorGUILayout.Space(10);
            EditorGUILayout.BeginVertical(boxStyle);
            grassFoldout = EditorGUILayout.Foldout(grassFoldout, "Grass", foldoutStyle);
            if (grassFoldout)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("renderDistance"));
            }
            EditorGUILayout.EndVertical();

            // Clouds Foldout
            EditorGUILayout.Space(10);
            EditorGUILayout.BeginVertical(boxStyle);
            cloudsFoldout = EditorGUILayout.Foldout(cloudsFoldout, "Clouds", foldoutStyle);
            if (cloudsFoldout)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("Altitude"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("volumeSamples"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("volumeSize"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("cloudsMaterial"));
                EditorGUILayout.Space(10);
                if (EMscript.cloudsMaterial != null)
                {
                    if (materialEditor == null || materialEditor.target != EMscript.cloudsMaterial)
                    {
                        materialEditor = Editor.CreateEditor(EMscript.cloudsMaterial);
                    }
                    materialEditor.DrawHeader();
                    materialEditor.OnInspectorGUI();
                }
            }
            EditorGUILayout.EndVertical();

            serializedObject.ApplyModifiedProperties();
        }
    }
}