using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
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

        //If we already have a meta file for that asset, mean it's a reimport and not a first import, se we don't want to apply the preset
        if (File.Exists(AssetDatabase.GetTextMetaFilePathFromAssetPath(importer.assetPath)))
            return;

        Debug.Log("new import");

        string[] importerOptions = AssetDatabase.FindAssets("t:AssetImporterOptions");
        if (importerOptions.Length == 0)
            return; // no options, we don't need to override any data.

        AssetImporterOptions opts = AssetDatabase.LoadAssetAtPath<AssetImporterOptions>(AssetDatabase.GUIDToAssetPath(importerOptions[0]));

        for (int i = 0; i < opts.importOptions.Length; ++i)
        {
            Debug.Log(Regex.Match(System.IO.Path.GetFileName(assetPath), WildcardToRegex(opts.importOptions[i].nameFilter)).Success);

            if (opts.importOptions[i].presetEnabled && opts.importOptions[i].preset.CanBeAppliedTo(importer) &&
                Regex.Match(System.IO.Path.GetFileName(assetPath), WildcardToRegex(opts.importOptions[i].nameFilter)).Success)
            {
                // Preset made from texture importer save the width/height of the texture used to create the preset.
                // as our default preset are created from a 4x4 texture, applying the preset to any texture make that texture 4x4
                // so we save the original width/height of the texture & reapply it to the texture importer once the preset applied.
                // (this does not change any setting about maximum texture size)
                SerializedObject obj = new SerializedObject(importer);

                SerializedProperty widthProp = obj.FindProperty("m_Output.sourceTextureInformation.width");
                SerializedProperty heightProp = obj.FindProperty("m_Output.sourceTextureInformation.height");

                int prevW = widthProp.intValue;
                int prevH = heightProp.intValue;

                opts.importOptions[i].preset.ApplyTo(importer);

                obj.Update();
                widthProp.intValue = prevW;
                heightProp.intValue = prevH;

                obj.ApplyModifiedProperties();
            }
        }
    }
}
