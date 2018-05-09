using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;

public class FindMissingScripts: EditorWindow
{
    [MenuItem("Content Extensions/Find Missing Component")]
    static public void FindMissing()
    {
        GetWindow<FindMissingScripts>();
    }

    protected List<GameObject> _objectWithMissingScripts;
    protected Vector2 _scrollPosition;

    private void OnEnable()
    {
        _objectWithMissingScripts = new List<GameObject>();
        _scrollPosition = Vector2.zero;
    }

    private void OnGUI()
    {
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Find in Assets"))
            FindInAssets();
        if (GUILayout.Button("Find in Current Scene"))
            FindInScenes();
        EditorGUILayout.EndHorizontal();

        _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

        for (int i = 0; i < _objectWithMissingScripts.Count; ++i)
        {
            if (GUILayout.Button(_objectWithMissingScripts[i].name))
            {
                EditorGUIUtility.PingObject(_objectWithMissingScripts[i]);
            }
        }

        EditorGUILayout.EndScrollView();
    }

    void FindInAssets()
    {
        var assetGUIDs = AssetDatabase.FindAssets("t:GameObject");
        _objectWithMissingScripts.Clear();

        Debug.Log("Testing " + assetGUIDs.Length + " GameObject in Assets");

        foreach (string assetGuiD in assetGUIDs)
        {
            GameObject obj = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(assetGuiD));
            
            RecursiveDepthSearch(obj);
        }
    }

    void RecursiveDepthSearch(GameObject root)
    {
        Component[] components = root.GetComponents<Component>();
        foreach (Component c in components)
        {
            if (c == null)
            {
                if (!_objectWithMissingScripts.Contains(root))
                    _objectWithMissingScripts.Add(root);
            }
        }

        foreach (Transform t in root.transform)
        {
            RecursiveDepthSearch(t.gameObject);
        }
    }

    void FindInScenes()
    {
        _objectWithMissingScripts.Clear();

        for(int i= 0; i < SceneManager.sceneCount; ++i)
        {
            var rootGOs = SceneManager.GetSceneAt(i).GetRootGameObjects();

            Debug.Log("Testing " + rootGOs.Length + " Gameobjects in scene " + i);

            foreach (GameObject obj in rootGOs)
            {
                RecursiveDepthSearch(obj);
            }
        }
    }
}
