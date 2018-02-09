using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.AnimatedValues;
using UnityEditor.Presets;

[CreateAssetMenu(fileName = "AssetImporterOptions", menuName = "Asset Importer Option")]
public class AssetImporterOptions : ScriptableObject
{
    [System.Serializable]
    public class ImportOption
    {
        public string nameFilter;
        public bool presetEnabled;
        public Preset preset;
    }

    public ImportOption[] importOptions;
}


[CustomEditor(typeof(AssetImporterOptions))]
public class AssetImporterOptionsEditor : Editor
{
    protected AssetImporterOptions _opts;

    protected bool[] m_InspectorsFade;

    protected TextureImporter defaultTextureImport;
    protected ModelImporter defaultMeshImporter;

    private void OnEnable()
    {
        _opts = target as AssetImporterOptions;

        if (_opts.importOptions != null)
        {
            m_InspectorsFade = new bool[_opts.importOptions.Length];
            for (int i = 0; i < _opts.importOptions.Length; ++i)
            {
                m_InspectorsFade[i] = false;
            }
        }

        var meshasset = AssetDatabase.GUIDToAssetPath(AssetDatabase.FindAssets("__importermeshdummy__")[0]);
        var textureasset = AssetDatabase.GUIDToAssetPath(AssetDatabase.FindAssets("__importertexturedummy__")[0]);

        defaultMeshImporter = AssetImporter.GetAtPath(meshasset) as ModelImporter;
        defaultTextureImport = AssetImporter.GetAtPath(textureasset) as TextureImporter;
    }

    private void OnDisable()
    {

    }

    protected void AddNewPreset(Preset p)
    {
        AssetImporterOptions.ImportOption option = new AssetImporterOptions.ImportOption();
        option.nameFilter = "";
        option.presetEnabled = true;
        option.preset = p;

        if (_opts.importOptions == null)
        {
            _opts.importOptions = new AssetImporterOptions.ImportOption[0];
            m_InspectorsFade = new bool[0];
        }

        ArrayUtility.Add(ref _opts.importOptions, option);
        ArrayUtility.Add(ref m_InspectorsFade, false);

        AssetDatabase.AddObjectToAsset(p, _opts);
        AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(p));
        AssetDatabase.Refresh();
    }

    public void AddNewTypeofPreset(object obj)
    {
        Object unityObj = obj as Object;
        Preset newPreset = new Preset(unityObj);

        
        //if (unityObj == defaultTextureImport)
        //{
        //    int count = 0;
        //    while (count < newPreset.PropertyModifications.Length)
        //    {
        //        defaultTextureImport.
        //        if (newPreset.PropertyModifications[count].propertyPath.Contains("sourceTextureInformation"))
        //        {
        //            ArrayUtility.RemoveAt(ref newPreset.PropertyModifications, count);
        //        }
        //    }
        //}

        AddNewPreset(newPreset);
        EditorUtility.SetDirty(_opts);
    }

    public override void OnInspectorGUI()
    {
        //TODO : move the generic menu out of that to build it only once
        if (EditorGUILayout.DropdownButton(new GUIContent("New Preset"), FocusType.Passive))
        {
            GenericMenu menu = new GenericMenu();

            menu.AddItem(new GUIContent("Texture"), false, AddNewTypeofPreset, defaultTextureImport);
            menu.AddItem(new GUIContent("Mesh"), false, AddNewTypeofPreset, defaultMeshImporter);

            menu.DropDown(GUILayoutUtility.GetLastRect());
        }

        if (_opts.importOptions != null)
        {
            Editor ed = null;
            for (int i = 0; i < _opts.importOptions.Length; ++i)
            {
                m_InspectorsFade[i] = EditorGUILayout.Foldout(m_InspectorsFade[i], "Preset : " + _opts.importOptions[i].preset.GetTargetTypeName() + " on " + _opts.importOptions[i].nameFilter);

                EditorGUI.BeginChangeCheck();

                if (m_InspectorsFade[i])
                {
                    _opts.importOptions[i].nameFilter =
                        EditorGUILayout.TextField("Filter", _opts.importOptions[i].nameFilter);
                    _opts.importOptions[i].presetEnabled = EditorGUILayout.Toggle("Preset is enabled", _opts.importOptions[i].presetEnabled);

                    EditorGUILayout.BeginVertical("box");

                    CreateCachedEditor(_opts.importOptions[i].preset,
                        System.Type.GetType("UnityEditor.Presets.PresetEditor, UnityEditor"), ref ed);
                    //DrawPropertiesExcluding(serializedObjects[i], new string[]{});
                    ed.OnInspectorGUI();

                    EditorGUILayout.EndVertical();
                }

                if (EditorGUI.EndChangeCheck())
                    EditorUtility.SetDirty(_opts);

                EditorGUILayout.EndFadeGroup();
            }

            if (ed != null)
            {
                DestroyImmediate(ed);
            }
        }
    }
}