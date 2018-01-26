using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class AssetPackage : ScriptableObject
{
    public string packageName = "Package";
    public string[] dependenciesID = new string[0];
    public string[] outputPath = new string[0];


    //This is costly and create garbage, but it is also easy, so call with parcimony
    public string[] dependencies
    {
        get
        {
            string[] dep = new string[dependenciesID.Length];
            for (int i = 0; i < dep.Length; ++i)
                dep[i] = AssetDatabase.GUIDToAssetPath(dependenciesID[i]);

            return dep;
        }
    }
}
