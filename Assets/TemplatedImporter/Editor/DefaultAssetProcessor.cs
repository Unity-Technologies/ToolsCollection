using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEditor;

public class DefaultAssetProcessor : AssetPostprocessor
{
    public static string WildcardToRegex(string pattern)
    {
        return "^" + Regex.Escape(pattern)
                       .Replace(@"\*", ".*")
                       .Replace(@"\?", ".")
                   + "$";
    }

    void OnPreprocessModel()
    {
        ModelImporter importer = assetImporter as ModelImporter;

        string[] importerOptions = AssetDatabase.FindAssets("t:AssetImporterOptions");
        if (importerOptions.Length == 0)
            return; // no options, we don't need to override any data.

        AssetImporterOptions opts = AssetDatabase.LoadAssetAtPath<AssetImporterOptions>(AssetDatabase.GUIDToAssetPath(importerOptions[0]));

        for (int i = 0; i < opts.importOptions.Length; ++i)
        {
            if (opts.importOptions[i].presetEnabled && opts.importOptions[i].preset.CanBeAppliedTo(importer) &&
                Regex.Match(System.IO.Path.GetFileName(assetPath), WildcardToRegex(opts.importOptions[i].nameFilter)).Success)
            {
                opts.importOptions[i].preset.ApplyTo(importer);
            }
        }
    }

    private void OnPreprocessTexture()
    {
        TextureImporter importer = assetImporter as TextureImporter;

        string[] importerOptions = AssetDatabase.FindAssets("t:AssetImporterOptions");
        if (importerOptions.Length == 0)
            return; // no options, we don't need to override any data.

        Debug.Log(assetPath);

        AssetImporterOptions opts = AssetDatabase.LoadAssetAtPath<AssetImporterOptions>(AssetDatabase.GUIDToAssetPath(importerOptions[0]));

        for (int i = 0; i < opts.importOptions.Length; ++i)
        {
            Debug.Log(Regex.Match(System.IO.Path.GetFileName(assetPath), WildcardToRegex(opts.importOptions[i].nameFilter)).Success);

            if (opts.importOptions[i].presetEnabled && opts.importOptions[i].preset.CanBeAppliedTo(importer) &&
                Regex.Match(System.IO.Path.GetFileName(assetPath), WildcardToRegex(opts.importOptions[i].nameFilter)).Success)
            {
                opts.importOptions[i].preset.ApplyTo(importer);
            }
        }
    }
}
