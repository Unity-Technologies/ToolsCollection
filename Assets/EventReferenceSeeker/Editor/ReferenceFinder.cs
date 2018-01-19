using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Reflection;
using System.Linq;
using System.IO;
using YamlDotNet;
using YamlDotNet.RepresentationModel;

public class ReferenceFinder : EditorWindow
{
    string searchedFunction;
    string searchResult = "";
    string valueToSearch = "";

    System.Type typeSearched;

    string tempSearchResult = "";

    Vector2 scrollViewPosition;

    List<string> filesList = new List<string>();
    int currentParseFile = 0;

    const string gameObjectIdString = "m_GameObject: {fileID: ";
    const string gameObjectNameString = "m_Name: ";
    const string gameObjectParentString = "m_Father: {fileID: ";

    [MenuItem("Extensions/Find Function References")]
    static public void OpenWindow()
    {
        GetWindow<ReferenceFinder>();
    }

    private void Update()
    {
        if (filesList.Count > currentParseFile)
        {
            int purcentage = Mathf.FloorToInt(currentParseFile / (float)filesList.Count * 100.0f);
            searchResult = "Searching " + purcentage + "%";

            //HandleFile(filesList[currentParseFile]);
            NewHandleFile(filesList[currentParseFile]);

            currentParseFile += 1;

            if (currentParseFile == filesList.Count)
            {//reach the end, display the search
                searchResult = tempSearchResult;
            }
        }

        Repaint();
    }

    private void OnGUI()
    {
        EditorGUILayout.BeginHorizontal();
        searchedFunction = EditorGUILayout.TextField(searchedFunction);
        if (GUILayout.Button("Search", GUILayout.Width(50)))
        {
            Search();
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginScrollView(scrollViewPosition);
        GUILayout.Label(searchResult);
        EditorGUILayout.EndScrollView();
    }

    private void Search()
    {
        searchResult = "";
        tempSearchResult = "";
        scrollViewPosition = Vector2.zero;

        int pointPosition = searchedFunction.LastIndexOf('.');

        if (pointPosition < 0)
        {
            searchResult = "Malformed search query";
            return;
        }

        string objectPath = searchedFunction.Substring(0, pointPosition);
        string functionName = searchedFunction.Substring(pointPosition + 1);

        var types = FindType(objectPath);

        if (types.Count() > 1)
        {
            searchResult = "Ambiguous object name, could be : \n";
            foreach (var t in types)
                searchResult += "\t" + t.FullName + "\n";
            return;
        }
        else if (types.Count() == 0)
        {
            searchResult = "Could not find object " + objectPath;
            return;
        }

        var type = types.First();

        var func = type.FindMembers(MemberTypes.All, BindingFlags.Instance | BindingFlags.Public, new MemberFilter((info, obj) => { return info.Name == (string)obj; }), functionName);

        if (func.Length == 0)
        {
            searchResult = string.Format("Couldn't find function {0} in object {1}", functionName, objectPath);
            return;
        }

        if (func[0].MemberType == MemberTypes.Property)
        {
            PropertyInfo info = func[0] as PropertyInfo;

            valueToSearch = info.GetSetMethod().Name;
        }
        else
        {
            valueToSearch = func[0].Name;
        }

        typeSearched = type;
        //valueToSearch = valueToSearch;

        //Now that we checked the function name exist and is valid, we list all object that is suceptible of having an unity event data to it, meaning Scene & prefab.
        filesList = new List<string>();
        filesList.AddRange(Directory.GetFiles(Application.dataPath, "*.unity", SearchOption.AllDirectories));
        filesList.AddRange(Directory.GetFiles(Application.dataPath, "*.prefab", SearchOption.AllDirectories));

        currentParseFile = 0;
    }

    public class YamlVisitorEvent : IYamlVisitor
    {
        public ReferenceFinder referenceFinder;
        public bool havePersistentCall = false;
        public HashSet<string> idToCheck = new HashSet<string>();

        public void Visit(YamlStream stream)
        {
            
        }

        public void Visit(YamlDocument document)
        {
            
        }

        public void Visit(YamlScalarNode scalar)
        {
            
        }

        public void Visit(YamlSequenceNode sequence)
        {

        }

        public void Visit(YamlMappingNode mapping)
        {
            foreach(var n in mapping)
            {
                n.Value.Accept(this);
                if(((YamlScalarNode)n.Key).Value == "m_PersistentCalls")
                {
                    var callsSequence = n.Value["m_Calls"] as YamlSequenceNode;

                    foreach(var call  in callsSequence)
                    {
                        if(((YamlScalarNode)call["m_MethodName"]).Value == referenceFinder.valueToSearch)
                        {
                            havePersistentCall = true;
                            idToCheck.Add(((YamlScalarNode)call["m_Target"]["fileID"]).Value);
                        }
                    }
                }
            }
        }
    }

    private void NewHandleFile(string file)
    {
        bool filenameWrote = false;

        var fileContent = File.ReadAllText(file);
        //unity seem to use non valid yaml by added a "stripped" keyword on object that are linked to a prefab. Since the pareser itch on those, we remove them
        fileContent = fileContent.Replace("stripped", "");
        var input = new StringReader(fileContent);

        var yaml = new YamlStream();
        yaml.Load(input);

        YamlVisitorEvent visitor = new YamlVisitorEvent();
        visitor.referenceFinder = this;

        //map gameobject id to a hashset of monobehaviour to check for type against the searched type
        Dictionary<string, HashSet<string>> gameobjectToIdToCheck = new Dictionary<string, HashSet<string>>();

        //we store the anchor <-> node mapping, as there don't seem to be anyway to do that quickly through the YAml graph
        Dictionary<string, YamlMappingNode> parsedNodes = new Dictionary<string, YamlMappingNode>();

        foreach (var doc in yaml.Documents)
        {
            var root = (YamlMappingNode)doc.RootNode;

            parsedNodes[root.Anchor] = root;

            foreach (var node in root.Children)
            {
                var scalarNode = (YamlScalarNode) node.Key;
                if (scalarNode.Value == "MonoBehaviour")
                {//if it's a monobehaviour, it may have a list of event as child
                    YamlMappingNode sequenceNode = node.Value as YamlMappingNode;

                    visitor.havePersistentCall = false;
                    visitor.idToCheck.Clear();
                    sequenceNode.Accept(visitor);

                    if(visitor.havePersistentCall)
                    {//we found persistent call
                        string gameobjectID = ((YamlScalarNode)node.Value["m_GameObject"]["fileID"]).Value;

                        if (!gameobjectToIdToCheck.ContainsKey(gameobjectID))
                            gameobjectToIdToCheck[gameobjectID] = new HashSet<string>();

                        gameobjectToIdToCheck[gameobjectID].UnionWith(visitor.idToCheck);
                    }
                }
            }
        }

        //now we go over all our gameobject to check, and if one of the monobehaviour they ahve been associated with are of the researched type, they are added to the result
        foreach(var pair in gameobjectToIdToCheck)
        {
            bool haveOneValidCall = false;
            if(!parsedNodes.ContainsKey(pair.Key))
            {
                Debug.LogError("Tried to check an object id that don't exist : " + pair.Key);
                continue;
            }

            foreach(var id in pair.Value)
            {
                var targetNode = parsedNodes[id];
                var guid = ((YamlScalarNode)targetNode["MonoBehaviour"]["m_Script"]["guid"]).Value;

                MonoScript script =  AssetDatabase.LoadAssetAtPath<MonoScript>(AssetDatabase.GUIDToAssetPath(guid));

                if(script.GetClass() == typeSearched)
                {
                    haveOneValidCall = true;
                }
            }

            if(haveOneValidCall)
            {
                if (!filenameWrote)
                {
                    filenameWrote = true;
                    tempSearchResult += Path.GetFileName(file) + "\n";
                }

                if (((YamlScalarNode)parsedNodes[pair.Key]["GameObject"]["m_PrefabParentObject"]["fileID"]).Value != "0")
                {//this is a prefab instance, need to find the prefab value linked to it!!
                    tempSearchResult += "\t" + "A Prefab";
                }
                else
                {
                    string fullPath = "";

                    //make an assumption here that the 1st component of every gameobject will always be its transform.
                    string currentGOId = pair.Key;
                    while(currentGOId != "0")
                    {
                        fullPath = parsedNodes[currentGOId]["GameObject"]["m_Name"] + (fullPath == "" ? "" : "/" + fullPath);

                        string transformID = parsedNodes[currentGOId]["GameObject"]["m_Component"][0]["component"]["fileID"].ToString();

                        Debug.Log("trasnofrmID " + transformID);

                        string parentTransformID = parsedNodes[transformID]["Transform"]["m_Father"]["fileID"].ToString();
                        if(parentTransformID != "0")
                        {
                            currentGOId = parsedNodes[parentTransformID]["Transform"]["m_GameObject"]["fileID"].ToString();
                        }

                        Debug.Log("currentGOID " + currentGOId);
                    }

                    tempSearchResult += "\t" + fullPath + "\n";
                }
            }
        }
    }

    private void HandleFile(string file)
    {
        bool nameWritten = false;

        string relativePath = file.Replace(Application.dataPath, "Assets");
        Debug.Log(relativePath);
        Debug.Log(AssetDatabase.AssetPathToGUID(relativePath));

        string content = File.ReadAllText(file);

        //we're doing rough simple parsing here, not robust but way faster than deserializing the YAML & going through object etc...
        int index = content.IndexOf(valueToSearch);
        do
        {//as long as we find the line that point to a reference to that function, we then search backward the line that designate which gameobject the bahviour is one
            int gameobjectIndex = content.LastIndexOf(gameObjectIdString, index);
            if (gameobjectIndex == -1) gameobjectIndex = content.LastIndexOf(gameObjectIdString, index);

            if (gameobjectIndex != -1)
            {//we extract the id of said gameobject
             //correct the id to be at the end of the string, so in front of the number
                gameobjectIndex = gameobjectIndex + gameObjectIdString.Length;
                string id = content.Substring(gameobjectIndex, content.IndexOf('}', gameobjectIndex) - gameobjectIndex);

                string foundName = GetObjectName(content, id, gameobjectIndex);

                if (!nameWritten)
                {
                    tempSearchResult += Path.GetFileName(file) + ":\n";
                    nameWritten = true;
                }

                tempSearchResult += "\t" + foundName + "\n";
            }

            index = content.IndexOf(valueToSearch, index + 10);
        }
        while (index != -1);

        //TODO : handle prefab override in scene, as it is saved in a different place!
    }

    private string GetObjectName(string content, string gameobjectID, int startIndex)
    {
        try
        {
            string fatherName = "";
            //now we find the id in the file to retrieve the name...
            int parentGOIndex = content.LastIndexOf("&" + gameobjectID, startIndex);
            //if it wasn't found, search in the other direction (most of the time, it will be above, but in some case it can be inverted)
            if (parentGOIndex == -1) parentGOIndex = content.IndexOf("&" + gameobjectID, startIndex);

            //we check if that object was "stripped". A stripped gameobject don't have any info, it is just a link to a prefab, so we have to go fetch the prefab data?


            //if that object have a parent, we go fetch the name of it recursivly too
            int fatherIDPlaceIndex = content.IndexOf(gameObjectParentString, parentGOIndex);
            if (fatherIDPlaceIndex != -1)
            {
                fatherIDPlaceIndex = fatherIDPlaceIndex + gameObjectParentString.Length;
                string id = content.Substring(fatherIDPlaceIndex, content.IndexOf('}', fatherIDPlaceIndex) - fatherIDPlaceIndex);
                if (id != "0")
                { //the id is the id of the TRANSFORM, not the gameobject, so need to find the gameobject associated to that transform

                    int parentTransformIndex = content.LastIndexOf("&" + id, parentGOIndex);
                    if (parentTransformIndex == -1) parentTransformIndex = content.IndexOf("&" + id, parentGOIndex);

                    int parentgoidindex = content.IndexOf(gameObjectIdString, parentTransformIndex);
                    parentgoidindex = parentgoidindex + gameObjectIdString.Length;
                    string parentGOID = content.Substring(parentgoidindex, content.IndexOf('}', parentgoidindex) - parentgoidindex); ;

                    fatherName = GetObjectName(content, parentGOID, parentGOIndex) + "/";
                }
            }

            int nameIndex = content.IndexOf(gameObjectNameString, parentGOIndex) + gameObjectNameString.Length;
            return fatherName + content.Substring(nameIndex, content.IndexOf("\n", nameIndex) - nameIndex);
        }
        catch (System.Exception e)
        {
            Debug.Log(e.Message);
        }

        return "ERROR";
    }

    private static IEnumerable<System.Type> FindType(string fullName)
    {
        return
            System.AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .Where(t => t.FullName.Equals(fullName));
    }
}
