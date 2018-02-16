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

    protected GenericMenu importerMenu = new GenericMenu();


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

        importerMenu.AddItem(new GUIContent("Texture"), false, AddNewTypeofPreset, defaultTextureImport);
        importerMenu.AddItem(new GUIContent("Mesh"), false, AddNewTypeofPreset, defaultMeshImporter);
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
    
        AddNewPreset(newPreset);
        EditorUtility.SetDirty(_opts);
    }

    public void RemovePreset(int index)
    {
        var opt = _opts.importOptions[index];

        ArrayUtility.RemoveAt(ref _opts.importOptions, index);
        ArrayUtility.RemoveAt(ref m_InspectorsFade, index);

        string assetpath = AssetDatabase.GetAssetPath(opt.preset);
        DestroyImmediate(opt.preset, true);
        AssetDatabase.ImportAsset(assetpath);
        AssetDatabase.Refresh();
    }

    public override void OnInspectorGUI()
    {
        if (EditorGUILayout.DropdownButton(new GUIContent("New Preset"), FocusType.Passive, GUILayout.Width(100)))
        {
            importerMenu.ShowAsContext();
        }

        if (_opts.importOptions != null)
        {
            Editor ed = null;
            bool deletionHappened = false;
            for (int i = 0; i < _opts.importOptions.Length; ++i)
            {
                deletionHappened = false;

                EditorGUILayout.BeginHorizontal();
                m_InspectorsFade[i] = EditorGUILayout.Foldout(m_InspectorsFade[i], "Preset : " + _opts.importOptions[i].preset.GetTargetTypeName() + " on " + _opts.importOptions[i].nameFilter);

                if (GUILayout.Button("Delete", GUILayout.Width(100)))
                {
                    if (EditorUtility.DisplayDialog("Confirm", "Are you sure you want to delete that preset rule?",
                        "Delete", "Cancel"))
                    {
                        RemovePreset(i);
                        i--;
                        deletionHappened = true;
                    }
                }

                EditorGUILayout.EndHorizontal();

                EditorGUI.BeginChangeCheck();

                if (!deletionHappened && m_InspectorsFade[i])
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