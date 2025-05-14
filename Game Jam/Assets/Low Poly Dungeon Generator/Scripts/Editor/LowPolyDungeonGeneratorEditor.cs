using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using LowPolyDungeonGeneratorBypast12pm;
using UnityEngine.Rendering;


namespace LowPolyDungeonGeneratorBypast12pm
{
    [System.Serializable]
public class RoomPrefabInfo
{
    public GameObject prefab;
    public Vector2Int size; // Width and Height
}

public class LowPolyDungeonGeneratorEditor : EditorWindow
{

    public List<RoomPrefabInfo> roomPrefabs = new List<RoomPrefabInfo>();

    public GameObject stairsPrefab;

    public GameObject[] straightPrefabs;
    public GameObject[] leftPrefabs;
    public GameObject[] rightPrefabs;
    public GameObject[] split1Prefabs;
    public GameObject[] split2Prefabs;

    public int straightChance = 34;
    public int leftChance = 15;
    public int rightChance = 10;
    public int split1Chance = 10;
    public int split2Chance = 10;
    public int roomChance = 20;
    public int stairChance = 1;

    public GameObject deadEndPrefab;

    public int dungeonSize = 200;
    private int remainingModules;
    public float currentHeight = 0f;
    private int localHeight;

    private Dictionary<Vector2Int, bool> occupiedCells = new Dictionary<Vector2Int, bool>();
    private List<PathPrefab> pathPrefabs;

    private Transform dungeonParent;
    private int maxPathLength;
    private static bool isGeneratorOpenedFromToolbar = false;


    private Vector2 scrollPosition;

    [MenuItem("Tools/Low Poly Dungeon Generator")]
    public static void ShowWindow()
    {
            isGeneratorOpenedFromToolbar = true;
            GetWindow<LowPolyDungeonGeneratorEditor>("Low Poly Dungeon Generator");
    }

    private void OnGUI()
    {
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Width(position.width), GUILayout.Height(position.height));

            GUILayout.Label("Dungeon Generator Settings", EditorStyles.boldLabel);

            if (GUILayout.Button("Use URP Prefabs"))
            {
                OnUsingURP();
            }

            if (GUILayout.Button("Use HDRP Prefabs"))
            {
                OnUsingHDRP();
            }

            if (GUILayout.Button("Use SRP Prefabs"))
            {
                OnUsingBuiltInRenderPipeline();
            }

            dungeonSize = EditorGUILayout.IntField("Dungeon Size", dungeonSize);

        currentHeight = EditorGUILayout.FloatField("Starting Height", currentHeight);

        //maxPathLength = EditorGUILayout.IntField("Maximum Path Length", maxPathLength);

        GUILayout.Label("Straight Prefabs", EditorStyles.boldLabel);

        straightChance = EditorGUILayout.IntField("Straight Spawn Chance", straightChance);

        if (GUILayout.Button("Add Prefab"))
        {
            AddPrefabSlot(ref straightPrefabs);
        }
        
        if (straightPrefabs != null)
        {
            for (int i = 0; i < straightPrefabs.Length; i++)
            {
                EditorGUILayout.BeginHorizontal();

                straightPrefabs[i] = (GameObject)EditorGUILayout.ObjectField($"Straight Prefab {i + 1}", straightPrefabs[i], typeof(GameObject), false);

                if (GUILayout.Button("Remove", GUILayout.Width(70)))
                {
                    RemovePrefabSlot(ref straightPrefabs,i);
                }
                EditorGUILayout.EndHorizontal();
            }
        }

        GUILayout.Label("Left Prefabs", EditorStyles.boldLabel);
        if (GUILayout.Button("Add Left Prefab"))
        {
            AddPrefabSlot(ref leftPrefabs);
        }

        leftChance = EditorGUILayout.IntField("Left Spawn Chance", leftChance);

        if (leftPrefabs != null)
        {
            for (int i = 0; i < leftPrefabs.Length; i++)
            {
                EditorGUILayout.BeginHorizontal();

                leftPrefabs[i] = (GameObject)EditorGUILayout.ObjectField($"Left Prefab {i + 1}", leftPrefabs[i], typeof(GameObject), false);

                if (GUILayout.Button("Remove", GUILayout.Width(70)))
                {
                    RemovePrefabSlot(ref leftPrefabs, i);
                }
                EditorGUILayout.EndHorizontal();
            }
        }

        GUILayout.Label("Right Prefabs", EditorStyles.boldLabel);
        if (GUILayout.Button("Add Right Prefab"))
        {
            AddPrefabSlot(ref rightPrefabs);
        }

        rightChance = EditorGUILayout.IntField("Right Spawn Chance", rightChance);

        if (rightPrefabs != null)
        {
            for (int i = 0; i < rightPrefabs.Length; i++)
            {
                EditorGUILayout.BeginHorizontal();

                rightPrefabs[i] = (GameObject)EditorGUILayout.ObjectField($"Right Prefab {i + 1}", rightPrefabs[i], typeof(GameObject), false);

                if (GUILayout.Button("Remove", GUILayout.Width(70)))
                {
                    RemovePrefabSlot(ref rightPrefabs, i);
                }
                EditorGUILayout.EndHorizontal();
            }
        }

        GUILayout.Label("Double Split Prefabs", EditorStyles.boldLabel);
        if (GUILayout.Button("Add Double Split Prefab"))
        {
            AddPrefabSlot(ref split1Prefabs);
        }
        split1Chance = EditorGUILayout.IntField("Double Split Spawn Chance", split1Chance);
        if (split1Prefabs != null)
        {
            for (int i = 0; i < split1Prefabs.Length; i++)
            {
                EditorGUILayout.BeginHorizontal();
                
                split1Prefabs[i] = (GameObject)EditorGUILayout.ObjectField($"Double Split Prefab {i + 1}", split1Prefabs[i], typeof(GameObject), false);

                if (GUILayout.Button("Remove", GUILayout.Width(70)))
                {
                    RemovePrefabSlot(ref split1Prefabs, i);
                }
                EditorGUILayout.EndHorizontal();
            }
        }

        GUILayout.Label("Triple Split Prefabs", EditorStyles.boldLabel);
        if (GUILayout.Button("Add Triple Split Prefab"))
        {
            AddPrefabSlot(ref split2Prefabs);
        }
        split2Chance = EditorGUILayout.IntField("Triple Split Spawn Chance", split2Chance);
        if (split2Prefabs != null)
        {
            for (int i = 0; i < split2Prefabs.Length; i++)
            {
                EditorGUILayout.BeginHorizontal();

                split2Prefabs[i] = (GameObject)EditorGUILayout.ObjectField($"Triple Split Prefab {i + 1}", split2Prefabs[i], typeof(GameObject), false);

                if (GUILayout.Button("Remove", GUILayout.Width(70)))
                {
                    RemovePrefabSlot(ref split2Prefabs, i);
                }
                EditorGUILayout.EndHorizontal();
            }
        }

        GUILayout.Label("Room Prefabs", EditorStyles.boldLabel);

        if (GUILayout.Button("Add Room Prefab"))
        {
            roomPrefabs.Add(new RoomPrefabInfo()); // Add a new empty prefab info
        }
        roomChance = EditorGUILayout.IntField("Room Spawn Chance", roomChance);

        if (roomPrefabs != null)
        {
            for (int i = 0; i < roomPrefabs.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();

                roomPrefabs[i].prefab = (GameObject)EditorGUILayout.ObjectField($"Room Prefab {i + 1}", roomPrefabs[i].prefab, typeof(GameObject), false);

                EditorGUILayout.BeginHorizontal();
                roomPrefabs[i].size.x = Mathf.Max(1, EditorGUILayout.IntField(roomPrefabs[i].size.x, GUILayout.Width(50)));
                roomPrefabs[i].size.y = Mathf.Max(1, EditorGUILayout.IntField(roomPrefabs[i].size.y, GUILayout.Width(50)));
                EditorGUILayout.EndHorizontal();

                if (GUILayout.Button("Remove", GUILayout.Width(70)))
                {
                    roomPrefabs.RemoveAt(i);
                    continue;
                }
                EditorGUILayout.EndHorizontal();
            }
        }

        stairsPrefab = (GameObject)EditorGUILayout.ObjectField("Stairs", stairsPrefab, typeof(GameObject), false);
        stairChance = EditorGUILayout.IntField("Stairs Spawn Chance", stairChance);
        deadEndPrefab = (GameObject)EditorGUILayout.ObjectField("Dead End", deadEndPrefab, typeof(GameObject), false);

        if (GUILayout.Button("Generate Dungeon"))
        {
            GenerateDungeon();
        }

        if (GUILayout.Button("Clear Dungeon"))
        {
            ClearDungeon();
        }

        EditorGUILayout.EndScrollView();
    }

        private void OnEnable()
        {
            if (!isGeneratorOpenedFromToolbar)
            {
                // Skip initialization if not opened from the toolbar (e.g., during Play Mode)
                return;
            }

            isGeneratorOpenedFromToolbar = false;

            if (!TagExists("DungeonPath"))
            {
                Debug.Log("Tag 'DungeonPath' does not exist. Creating it now.");
                AddTag("DungeonPath");
            }
            if (straightPrefabs == null || straightPrefabs.Length == 0)
            {
                AddPrefabSlot(ref straightPrefabs);
            }

            if (leftPrefabs == null || leftPrefabs.Length == 0)
            {
                AddPrefabSlot(ref leftPrefabs);
            }

            if (rightPrefabs == null || rightPrefabs.Length == 0)
            {
                AddPrefabSlot(ref rightPrefabs);
            }

            if (split1Prefabs == null || split1Prefabs.Length == 0)
            {
                AddPrefabSlot(ref split1Prefabs); // Double Split
            }

            if (split2Prefabs == null || split2Prefabs.Length == 0)
            {
                AddPrefabSlot(ref split2Prefabs); // Triple Split
            }

            // Add a new RoomPrefabInfo only if roomPrefabs has no elements
            if (roomPrefabs == null || roomPrefabs.Count == 0)
            {
                roomPrefabs.Add(new RoomPrefabInfo());
            }
            IdentifyRenderPipeline();
        }
        private void IdentifyRenderPipeline()
        {
            if (GraphicsSettings.defaultRenderPipeline != null)
            {
                if (GraphicsSettings.defaultRenderPipeline.GetType().Name == "HDRenderPipelineAsset")
                {
                    OnUsingHDRP();
                    Debug.Log("High Definition Render Pipeline (HDRP) is being used.");
                }
                else if (GraphicsSettings.defaultRenderPipeline.GetType().Name == "UniversalRenderPipelineAsset")
                {
                    OnUsingURP();
                    Debug.Log("Universal Render Pipeline (URP) is being used.");
                }
            }
            else
            {
                OnUsingBuiltInRenderPipeline();
                Debug.Log("Using Built-in (Legacy) Render Pipeline");
            }
        }
        // Logic for Built-in Render Pipeline
        private void OnUsingBuiltInRenderPipeline()
        {
            string srpPath = "Assets/Low Poly Dungeon Generator/Prefabs/SRP/Pathbuilding/";

            // Straight prefab
            straightPrefabs[0] = AssetDatabase.LoadAssetAtPath<GameObject>(srpPath + "Straight1.prefab");
            // Left prefab
            leftPrefabs[0] = AssetDatabase.LoadAssetAtPath<GameObject>(srpPath + "Left1.prefab");
            // Right prefab
            rightPrefabs[0] = AssetDatabase.LoadAssetAtPath<GameObject>(srpPath + "Right1.prefab");
            // Double Split prefab
            split1Prefabs[0] = AssetDatabase.LoadAssetAtPath<GameObject>(srpPath + "DoubleSplit1.prefab");
            // Triple Split prefab
            split2Prefabs[0] = AssetDatabase.LoadAssetAtPath<GameObject>(srpPath + "TripleSplit1.prefab");
            // Room prefab
            if (roomPrefabs.Count > 0)
            {
                roomPrefabs[0].prefab = AssetDatabase.LoadAssetAtPath<GameObject>(srpPath + "Room3x3_MagicLibrary.prefab");
                roomPrefabs[0].size.x = 3;  // Width
                roomPrefabs[0].size.y = 3;  // Length
            }
            // Stairs prefab
            stairsPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(srpPath + "Stairs1.prefab");
            // Dead End prefab
            deadEndPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(srpPath + "DeadEndPrefab.prefab");

            Debug.Log("Custom behavior for Built-in Render Pipeline");
        }

        // Logic for Universal Render Pipeline (URP)
        private void OnUsingURP()
        {
            // Load URP-specific prefabs from the folder path
            string urpPath = "Assets/Low Poly Dungeon Generator/Prefabs/URP/Pathbuilding/";

            // Straight prefab
            straightPrefabs[0] = AssetDatabase.LoadAssetAtPath<GameObject>(urpPath + "Straight1.prefab");
            // Left prefab
            leftPrefabs[0] = AssetDatabase.LoadAssetAtPath<GameObject>(urpPath + "Left1.prefab");
            // Right prefab
            rightPrefabs[0] = AssetDatabase.LoadAssetAtPath<GameObject>(urpPath + "Right1.prefab");
            // Double Split prefab
            split1Prefabs[0] = AssetDatabase.LoadAssetAtPath<GameObject>(urpPath + "DoubleSplit1.prefab");
            // Triple Split prefab
            split2Prefabs[0] = AssetDatabase.LoadAssetAtPath<GameObject>(urpPath + "TripleSplit1.prefab");
            // Room prefab
            if (roomPrefabs.Count > 0)
            {
                roomPrefabs[0].prefab = AssetDatabase.LoadAssetAtPath<GameObject>(urpPath + "Room3x3_MagicLibrary.prefab");
                roomPrefabs[0].size.x = 3;  // Width
                roomPrefabs[0].size.y = 3;  // Length
                
            }
            // Stairs prefab
            stairsPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(urpPath + "Stairs1.prefab");
            // Dead End prefab
            deadEndPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(urpPath + "DeadEndPrefab.prefab");

            Debug.Log("URP prefabs loaded successfully.");
        }

        // Logic for High Definition Render Pipeline (HDRP)
        private void OnUsingHDRP()
        {
            // Load URP-specific prefabs from the folder path
            string hdrpPath = "Assets/Low Poly Dungeon Generator/Prefabs/HDRP/Pathbuilding/";

            // Straight prefab
            straightPrefabs[0] = AssetDatabase.LoadAssetAtPath<GameObject>(hdrpPath + "Straight1.prefab");
            // Left prefab
            leftPrefabs[0] = AssetDatabase.LoadAssetAtPath<GameObject>(hdrpPath + "Left1.prefab");
            // Right prefab
            rightPrefabs[0] = AssetDatabase.LoadAssetAtPath<GameObject>(hdrpPath + "Right1.prefab");
            // Double Split prefab
            split1Prefabs[0] = AssetDatabase.LoadAssetAtPath<GameObject>(hdrpPath + "DoubleSplit1.prefab");
            // Triple Split prefab
            split2Prefabs[0] = AssetDatabase.LoadAssetAtPath<GameObject>(hdrpPath + "TripleSplit1.prefab");
            // Room prefab
            if (roomPrefabs.Count > 0)
            {
                roomPrefabs[0].prefab = AssetDatabase.LoadAssetAtPath<GameObject>(hdrpPath + "Room3x3_MagicLibrary.prefab");
                roomPrefabs[0].size.x = 3;  // Width
                roomPrefabs[0].size.y = 3;  // Length

            }
            // Stairs prefab
            stairsPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(hdrpPath + "Stairs1.prefab");
            // Dead End prefab
            deadEndPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(hdrpPath + "DeadEndPrefab.prefab");
            // Add your specific logic for High Definition Render Pipeline here
            Debug.Log("Custom behavior for High Definition Render Pipeline (HDRP)");
        }

        private bool TagExists(string tagName)
        {
            return UnityEditorInternal.InternalEditorUtility.tags.Contains(tagName);
        }

        private void AddTag(string tagName)
        {
            // Open the Tag Manager
            SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);

            // Find the tags property
            SerializedProperty tagsProp = tagManager.FindProperty("tags");

            // Check if the tag already exists (just in case)
            for (int i = 0; i < tagsProp.arraySize; i++)
            {
                SerializedProperty tag = tagsProp.GetArrayElementAtIndex(i);
                if (tag.stringValue == tagName)
                {
                    return; // Tag already exists
                }
            }

            // Add the new tag
            tagsProp.InsertArrayElementAtIndex(tagsProp.arraySize);
            SerializedProperty newTag = tagsProp.GetArrayElementAtIndex(tagsProp.arraySize - 1);
            newTag.stringValue = tagName;

            // Apply changes to save the new tag
            tagManager.ApplyModifiedProperties();

            Debug.Log($"Tag '{tagName}' successfully created.");
        }

        private void AddPrefabSlot(ref GameObject[] prefabArray)
    {
      if (prefabArray == null) // Check if the array is null
         {
          prefabArray = new GameObject[0]; // Initialize it as an empty array
         }
        ArrayUtility.Add(ref prefabArray, null);
    }
    private void RemovePrefabSlot(ref GameObject[] prefabArray, int index)
    {
        ArrayUtility.RemoveAt(ref prefabArray, index);
    }

    private void GenerateDungeon()
    {
        if (straightPrefabs == null || leftPrefabs == null || rightPrefabs == null || split1Prefabs == null || split2Prefabs == null)
        {
            Debug.LogError("Please assign all pathbuilding prefabs.");
            return;
        }

        InitializeRoomSizes();
        InitializePathPrefabs();
        occupiedCells.Clear();

        remainingModules = dungeonSize; // Start with the total dungeon size

        Vector2Int startPosition = Vector2Int.zero;
        int startRotation = 0;

        //int originalMaxPathLength = maxPathLength;

        GeneratePath(startPosition, startRotation, 10, localHeight);

        //Debug.Log($"Dungeon generation complete. Total modules placed: {occupiedCells.Count} / {dungeonSize}");

        Vector2Int dimensions = CalculateDungeonDimensions();
        //Debug.Log($"Generated dungeon with dimensions: {dimensions.x}x{dimensions.y}");
    }

    private void GeneratePath(Vector2Int startPosition, int startRotation, int maxPathLength, int localHeight)
    {
        Vector2Int currentPosition = startPosition;
        int currentRotation = startRotation;
        int yOffset = 0;


        //int originalMaxPathLength = maxPathLength;

        for (int i = 0; i < maxPathLength; i++)
        {
            /*if (remainingModules <= 0) // Stop if no modules are left to place
            {
                PlacePrefab(deadEndPrefab, ref currentPosition, ref currentRotation, ref localHeight);
                return;
            }*/
            PathPrefab nextPrefab = SelectPrefabWeighted();
            Vector2Int nextPosition = currentPosition + GetDirectionFromRotation(currentRotation);

            if (CanPlacePrefab(nextPrefab.prefab, nextPosition, currentRotation))
            {
                if (yOffset > 0)
                {
                    nextPosition.y += yOffset;
                    yOffset = 0; // Reset offset after applying
                }

                else if (split1Prefabs.Contains(nextPrefab.prefab))
                {
                    GameObject randomSplit1Prefab = split1Prefabs[Random.Range(0, split1Prefabs.Length)];
                    nextPrefab.prefab = randomSplit1Prefab;

                    PlacePrefab(nextPrefab.prefab, ref currentPosition, ref currentRotation, ref localHeight);
                    
                    GenerateSplit1(currentPosition, currentRotation, 10, localHeight); // Continue path generation for Split1
                    break; // Exit the loop after split is placed
                }
                else if (split2Prefabs.Contains(nextPrefab.prefab))
                {
                    GameObject randomSplit2Prefab = split2Prefabs[Random.Range(0, split2Prefabs.Length)];
                    nextPrefab.prefab = randomSplit2Prefab;
                    PlacePrefab(nextPrefab.prefab, ref currentPosition, ref currentRotation, ref localHeight);
                    
                    GenerateSplit2(currentPosition, currentRotation, 10, localHeight); // Continue path generation for Split2
                    break; // Exit the loop after split is placed
                }
                else if (roomPrefabs.Any(room => room.prefab == nextPrefab.prefab))
                {
                    //RefreshRoomPrefabs();
                    var matchingRoom = roomPrefabs.First(room => room.prefab == nextPrefab.prefab);

                    int skipDistance = matchingRoom.size.y - 1;

                    PlacePrefab(nextPrefab.prefab, ref currentPosition, ref currentRotation, ref localHeight);
                    

                    currentPosition += GetDirectionFromRotation(currentRotation) * skipDistance;
                }

                else if (nextPrefab.prefab == stairsPrefab)
                {
                    int skipDistance = GetRoomSkipDistance(nextPrefab.prefab);
                    PlacePrefab(nextPrefab.prefab, ref currentPosition, ref currentRotation, ref localHeight);
                    

                    currentPosition += GetDirectionFromRotation(currentRotation) * skipDistance;
                }
                else
                {
                    //RefreshStraightPrefabs();
                    PlacePrefab(nextPrefab.prefab, ref currentPosition, ref currentRotation, ref localHeight);
                    
                }
            }
            else
            {
                    if (remainingModules <= 0)
                    {
                        //Debug.Log($"Path generation stopped at {currentPosition} (Last module placed: {nextPrefab.prefab.name})");
                        PlacePrefab(deadEndPrefab, ref currentPosition, ref currentRotation, ref localHeight);
                        break; // End the path generation
                    }

                    else if (maxPathLength <= 0)
                    {
                        //Debug.Log($"Path generation stopped at {currentPosition} (Last module placed: {nextPrefab.prefab.name})");
                        PlacePrefab(deadEndPrefab, ref currentPosition, ref currentRotation, ref localHeight);
                        break; // End the path generation
                    }
                    else
                    {
                        continue;
                    }
                    //Debug.LogWarning($"Failed to place prefab {nextPrefab.prefab.name} at {nextPosition}. Skipping.");
                    //continue; // Skip to the next iteration (try a different prefab)
            }

            if (remainingModules <= 0)
            {
                //Debug.Log($"Path generation stopped at {currentPosition} (Last module placed: {nextPrefab.prefab.name})");
                PlacePrefab(deadEndPrefab, ref currentPosition, ref currentRotation, ref localHeight);
                break; // End the path generation
            }

            if (maxPathLength <= 0)
            {
                //Debug.Log($"Path generation stopped at {currentPosition} (Last module placed: {nextPrefab.prefab.name})");
                PlacePrefab(deadEndPrefab, ref currentPosition, ref currentRotation, ref localHeight);
                break; // End the path generation
            }
        }
        maxPathLength = 10; // Reset maxPathLength to 5
    }
    private void RefreshRoomPrefabs()
    {
        for (int i = 0; i < pathPrefabs.Count; i++)
        {
            if (pathPrefabs[i].weight == roomChance) // Assuming room prefabs have a specific weight
            {
                pathPrefabs[i] = new PathPrefab
                {
                    prefab = GetRandomRoomPrefab(),
                    weight = roomChance // Maintain the same weight
                };
            }
        }
    }

    private void RefreshStraightPrefabs()
    {
        for (int i = 0; i < pathPrefabs.Count; i++)
        {
            if (pathPrefabs[i].weight == straightChance) // Assuming room prefabs have a specific weight
            {
                pathPrefabs[i] = new PathPrefab
                {
                    prefab = GetRandomStraightPrefab(),
                    weight = straightChance // Maintain the same weight
                };
            }
        }
    }


    private int GetRoomSkipDistance(GameObject roomPrefab)
    {
        if (roomPrefab == stairsPrefab) return 1;
        return 0; // Default, should not occur
    }


    private void GenerateSplit1(Vector2Int splitPosition, int splitRotation, int remainingModules, int localHeight)
    {
        //Debug.Log($"[Split1] Generating Split1 at position {splitPosition} with rotation {splitRotation}");

        //Debug.Log(currentHeight);

        //if (remainingModules <= 0) return;

        

        int branchLength = 10; // Ensure at least 1 module per branch

        int leftRotation = (splitRotation - 90 + 360) % 360;
        Vector2Int leftStart = splitPosition + GetDirectionFromRotation(leftRotation);
        //Debug.Log($"[Split1] Left branch starts at {leftStart} with rotation {leftRotation}");

        foreach (var prefab in straightPrefabs)
        {
            if (CanPlacePrefab(prefab, leftStart, leftRotation))
            {
            //Debug.Log($"[Split1] Placing left branch module at {leftStart}");
            PlacePrefab(prefab, ref splitPosition, ref leftRotation, ref localHeight, leftStart); // Use fixed position

                Vector2Int nextLeftPosition = splitPosition + GetDirectionFromRotation(leftRotation);


                GeneratePath(nextLeftPosition, leftRotation, branchLength, localHeight); // Start path from the calculated position
                break;
            }
            else
            {
                //Debug.Log($"[Split1] Left branch failed to generate at {leftStart}. Skipping.");
            }
        }

        int rightRotation = (splitRotation + 90) % 360;
        Vector2Int rightStart = splitPosition + GetDirectionFromRotation(rightRotation);
        //Debug.Log($"[Split1] Right branch starts at {rightStart} with rotation {rightRotation}");
        
        foreach (var prefab in straightPrefabs)
        {
            if (CanPlacePrefab(prefab, rightStart, rightRotation))
        {
            //Debug.Log($"[Split1] Placing right branch module at {rightStart}");
            PlacePrefab(prefab, ref splitPosition, ref rightRotation, ref localHeight, rightStart); // Use fixed position

                Vector2Int nextRightPosition = splitPosition + GetDirectionFromRotation(rightRotation);

                GeneratePath(nextRightPosition, rightRotation, branchLength, localHeight); // Start path from the calculated position
                break;
        }
        else
        {
            //Debug.Log($"[Split1] Right branch failed to generate at {rightStart}. Skipping.");
        }
        }
    }

    private void GenerateSplit2(Vector2Int splitPosition, int splitRotation, int remainingModules, int localHeight)
    {
        //Debug.Log($"[Split2] Generating Split2 at position {splitPosition} with rotation {splitRotation}");

        

        //if (remainingModules <= 0) return;

        int branchLength = 10; // Ensure at least 1 module per branch

        int leftRotation = (splitRotation - 90 + 360) % 360;
        Vector2Int leftStart = splitPosition + GetDirectionFromRotation(leftRotation);
        //Debug.Log($"[Split2] Left branch starts at {leftStart} with rotation {leftRotation}");

        foreach (var prefab in straightPrefabs)
        {
            if (CanPlacePrefab(prefab, leftStart, leftRotation))
        {
            //Debug.Log($"[Split2] Placing left branch module at {leftStart}");
            PlacePrefab(prefab, ref splitPosition, ref leftRotation, ref localHeight,  leftStart); // Fixed position

                Vector2Int nextLeftPosition = splitPosition + GetDirectionFromRotation(leftRotation);

                GeneratePath(nextLeftPosition, leftRotation, branchLength, localHeight);
                break;
            }
        else
        {
            //Debug.Log($"[Split2] Left branch failed to generate at {leftStart}. Skipping.");
        }
        }
        Vector2Int straightStart = splitPosition + GetDirectionFromRotation(splitRotation);
        //Debug.Log($"[Split2] Straight branch starts at {straightStart} with rotation {splitRotation}");

        foreach (var prefab in straightPrefabs)
        {
            if (CanPlacePrefab(prefab, straightStart, splitRotation))
        {
            //Debug.Log($"[Split2] Placing straight branch module at {straightStart}");
            PlacePrefab(prefab, ref splitPosition, ref splitRotation, ref localHeight, straightStart); // Fixed position

                Vector2Int nextStraightPosition = splitPosition + GetDirectionFromRotation(splitRotation);

                GeneratePath(nextStraightPosition, splitRotation, branchLength, localHeight);
                break;
            }
        else
        {
            //Debug.Log($"[Split2] Straight branch failed to generate at {straightStart}. Skipping.");
        }
        }
        // Generate Right Branch
        int rightRotation = (splitRotation + 90) % 360;
        Vector2Int rightStart = splitPosition + GetDirectionFromRotation(rightRotation);
        //Debug.Log($"[Split2] Right branch starts at {rightStart} with rotation {rightRotation}");

        foreach (var prefab in straightPrefabs)
        {
            if (CanPlacePrefab(prefab, rightStart, rightRotation))
        {
            //Debug.Log($"[Split2] Placing right branch module at {rightStart}");
            PlacePrefab(prefab, ref splitPosition, ref rightRotation, ref localHeight, rightStart); // Fixed position

                Vector2Int nextRightPosition = splitPosition + GetDirectionFromRotation(rightRotation);

                GeneratePath(nextRightPosition, rightRotation, branchLength, localHeight);
                break;
            }
        else
        {
            //Debug.Log($"[Split2] Right branch failed to generate at {rightStart}. Skipping.");
        }
        }
    }
    
    private void ClearDungeon()
    {
        foreach (var obj in GameObject.FindGameObjectsWithTag("DungeonPath"))
        {
            DestroyImmediate(obj);
        }
        occupiedCells.Clear();
        Debug.Log("Dungeon cleared.");
    }

    private void InitializePathPrefabs()
    {
        pathPrefabs = new List<PathPrefab>
    {
        new PathPrefab { prefab = GetRandomStraightPrefab(), weight = straightChance },
        new PathPrefab { prefab = GetRandomLeftPrefab(), weight = leftChance },
        new PathPrefab { prefab = GetRandomRightPrefab(), weight = rightChance },
        new PathPrefab { prefab = GetRandomSplit1Prefab(), weight = split1Chance },
        new PathPrefab { prefab = GetRandomSplit2Prefab(), weight = split2Chance },
        new PathPrefab { prefab = GetRandomRoomPrefab(), weight = roomChance },
        new PathPrefab { prefab = stairsPrefab, weight = stairChance },
    };
    }

    private GameObject GetRandomStraightPrefab()
    {
        if (straightPrefabs == null || straightPrefabs.Length == 0)
        {
            Debug.LogError("No straight prefabs assigned!");
            return null;
        }
        return straightPrefabs[Random.Range(0, straightPrefabs.Length)];
    }
    private GameObject GetRandomLeftPrefab()
    {
        if (leftPrefabs == null || leftPrefabs.Length == 0)
        {
            Debug.LogError("No left prefabs assigned!");
            return null;
        }
        return leftPrefabs[Random.Range(0, leftPrefabs.Length)];
    }
    private GameObject GetRandomRightPrefab()
    {
        if (rightPrefabs == null || rightPrefabs.Length == 0)
        {
            Debug.LogError("No right prefabs assigned!");
            return null;
        }
        return rightPrefabs[Random.Range(0, rightPrefabs.Length)];
    }
    private GameObject GetRandomSplit1Prefab()
    {
        if (split1Prefabs == null || split1Prefabs.Length == 0)
        {
            Debug.LogError("No Double Split prefabs assigned!");
            return null;
        }
        return split1Prefabs[Random.Range(0, split1Prefabs.Length)];
    }
    private GameObject GetRandomSplit2Prefab()
    {
        if (split2Prefabs == null || split2Prefabs.Length == 0)
        {
            Debug.LogError("No Triple Split prefabs assigned!");
            return null;
        }
        return split2Prefabs[Random.Range(0, split2Prefabs.Length)];
    }

    private GameObject GetRandomRoomPrefab()
    {
        if (roomPrefabs == null || roomPrefabs.Count == 0)
        {
            Debug.LogError("No room prefabs assigned!");
            return null;
        }
        return roomPrefabs[Random.Range(0, roomPrefabs.Count)].prefab;
    }

    private PathPrefab SelectPrefabWeighted()
    {
        int totalWeight = 0;
        foreach (var pathPrefab in pathPrefabs)
        {
            totalWeight += pathPrefab.weight;
        }

        int randomValue = Random.Range(0, totalWeight);
        foreach (var pathPrefab in pathPrefabs)
        {
            if (randomValue < pathPrefab.weight)
            {
                return pathPrefab;
            }
            randomValue -= pathPrefab.weight;
        }
        return null;
    }

    private bool CanPlacePrefab(GameObject prefab, Vector2Int position, int currentRotation)
    {
        if (roomSizes.ContainsKey(prefab))
        {
            Vector2Int roomSize = roomSizes[prefab];
            List<Vector2Int> occupiedRoomCells = GetOccupiedCells(position, roomSize, currentRotation);

            //Debug.Log($"[CanPlacePrefab] Checking room {prefab.name} at position {position} with size {roomSize} and rotation {currentRotation}");

            foreach (var cell in occupiedRoomCells)
            {
                if (occupiedCells.ContainsKey(cell))
                {
                    //Debug.LogWarning($"[CanPlacePrefab] Cell {cell} is occupied.");
                    return false; // If any cell is occupied, return false
                }
                else
                {
                    //Debug.Log($"[CanPlacePrefab] Cell {cell} is free.");
                }
            }
        }

        if (occupiedCells.ContainsKey(position))
        {
            //Debug.LogWarning($"[CanPlacePrefab] Position {position} is occupied by another module.");
            return false; // If the position itself is occupied, return false
        }
        else
        {
            //Debug.Log($"[CanPlacePrefab] Position {position} is free.");
        }
        return true;
    }

    private void PlacePrefab(GameObject prefab, ref Vector2Int position, ref int currentRotation, ref int localHeight, Vector2Int? fixedPosition = null)
    {
        //Debug.Log($"remaining modules  { remainingModules}");
        remainingModules--; // Decrement for the placed module
        maxPathLength--;

        if (dungeonParent == null)
        {
            GameObject parentObject = GameObject.Find("Generated Dungeon");
            if (parentObject == null)
            {
                parentObject = new GameObject("Generated Dungeon");
            }
            dungeonParent = parentObject.transform;
        }

        Vector2Int placementPosition = fixedPosition ?? (position + GetDirectionFromRotation(currentRotation));

        int prefabRotation = currentRotation;
        if (leftPrefabs.Contains(prefab))
        {
            prefab = leftPrefabs[Random.Range(0, leftPrefabs.Length)];
            currentRotation = (currentRotation - 90 + 360) % 360;
        }
        else if (rightPrefabs.Contains(prefab))
        {
            prefab = rightPrefabs[Random.Range(0, rightPrefabs.Length)];
            currentRotation = (currentRotation + 90) % 360;
        }
        else if (straightPrefabs.Contains(prefab))
        {
            prefab = straightPrefabs[Random.Range(0, straightPrefabs.Length)];
            
        }
        /*else if (roomPrefabs.Contains(prefab))
        {
            prefab = roomPrefabs[Random.Range(0, roomPrefabs.Length)];
            currentRotation = (currentRotation + 90) % 360;
        }*/

        Vector3 worldPosition = new Vector3(placementPosition.x * 2, localHeight, placementPosition.y * 2);
        if (prefab == stairsPrefab)
        {
            localHeight += 2; // Increase the height for stairs

            // Mark the occupied cells for the stairsPrefab (1x2 size)
            List<Vector2Int> stairsOccupiedCells = GetOccupiedCells(placementPosition, new Vector2Int(1, 2), prefabRotation);
            foreach (var cell in stairsOccupiedCells)
            {
                occupiedCells[cell] = true;
            }
        }
        Quaternion rotation = Quaternion.Euler(0, prefabRotation, 0);
        GameObject instance = Instantiate(prefab, worldPosition, rotation);
        instance.transform.SetParent(dungeonParent);
        instance.tag = "DungeonPath";

        occupiedCells[placementPosition] = true;

        if (roomSizes.ContainsKey(prefab))
        {
            Vector2Int roomSize = roomSizes[prefab];
            List<Vector2Int> occupiedRoomCells = GetOccupiedCells(placementPosition, roomSize, prefabRotation);

            foreach (var cell in occupiedRoomCells)
            {
                occupiedCells[cell] = true; // Mark the cell as occupied
            }

            //Debug.Log($"Placed room {prefab.name} at {placementPosition} with rotation {prefabRotation}. Occupied cells: {string.Join(", ", occupiedRoomCells)}");

            RefreshRoomPrefabs();
        }
        else
        {
            //Debug.Log($"Placed {prefab.name} at {placementPosition} with rotation {prefabRotation}");
        }

        if (!fixedPosition.HasValue)
        {
            position = placementPosition; // Update the original position reference only when no fixed position is provided
        }

        //Debug.Log($"Placed {prefab.name} at {placementPosition} with rotation {prefabRotation}. New direction: {GetDirectionFromRotation(currentRotation)}");
    }


    private List<Vector2Int> GetOccupiedCells(Vector2Int center, Vector2Int roomSize, int rotation)
    {
        List<Vector2Int> occupiedCells = new List<Vector2Int>();
        int width = roomSize.x;
        int height = roomSize.y;

        for (int y = 0; y < height; y++) // Iterate through rows
        {
            for (int x = -width / 2; x <= width / 2; x++) // Iterate through columns
            {
                // Flip both X and Y axes
                Vector2Int offset = new Vector2Int(-x, height - y - 1);
                offset = RotateOffset(offset, rotation); // Apply rotation
                occupiedCells.Add(center + offset); // Add to final list
            }
        }

        return occupiedCells;
    }

    private Vector2Int GetDirectionFromRotation(int rotation)
    {
        switch (rotation % 360)
        {
            case 0: return Vector2Int.up;
            case 90: return Vector2Int.right;
            case 180: return Vector2Int.down;
            case 270: return Vector2Int.left;
            default: return Vector2Int.zero;
        }
    }

    private class PathPrefab
    {
        public GameObject prefab;
        public int weight;
    }

    // Dictionary to store room sizes
    private Dictionary<GameObject, Vector2Int> roomSizes;

    private void InitializeRoomSizes()
    {
        roomSizes = new Dictionary<GameObject, Vector2Int>();

        foreach (var roomInfo in roomPrefabs)
        {
            if (roomInfo.prefab != null)
            {
                roomSizes[roomInfo.prefab] = roomInfo.size;
            }
        }
    }

    private Vector2Int RotateOffset(Vector2Int offset, int rotation)
    {
        switch (rotation)
        {
            case 90:
                return new Vector2Int(-offset.y, -offset.x); // Clockwise 90°
            case 180:
                return new Vector2Int(-offset.x, -offset.y); // Upside down
            case 270:
                return new Vector2Int(-offset.y, -offset.x); // Counterclockwise 90°
            default: // 0 degrees
                return offset;
        }
    }

    private Vector2Int CalculateDungeonDimensions()
    {
        if (occupiedCells.Count == 0)
        {
            Debug.LogWarning("No modules have been placed. Dungeon dimensions cannot be calculated.");
            return Vector2Int.zero;
        }

        // Extract all X and Y coordinates
        int minX = int.MaxValue, maxX = int.MinValue;
        int minY = int.MaxValue, maxY = int.MinValue;

        foreach (var cell in occupiedCells.Keys)
        {
            if (cell.x < minX) minX = cell.x;
            if (cell.x > maxX) maxX = cell.x;
            if (cell.y < minY) minY = cell.y;
            if (cell.y > maxY) maxY = cell.y;
        }

        // Calculate width and length
        int width = maxX - minX + 1;
        int length = maxY - minY + 1;

        //Debug.Log($"Dungeon dimensions - Width: {width}, Length: {length}");
        return new Vector2Int(width, length);
    }

}
}