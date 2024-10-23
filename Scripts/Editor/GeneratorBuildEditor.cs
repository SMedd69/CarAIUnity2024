using UnityEngine;
using UnityEditor;
using System.IO;

[CustomEditor(typeof(BuildingGenerator))]
public class BuildingGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        BuildingGenerator script = (BuildingGenerator)target;

        if (GUILayout.Button("Generate Building"))
        {
            script.GenerateBuilding();
        }

        if (GUILayout.Button("Clear Building"))
        {
            script.ClearBuilding();
        }

        if (GUILayout.Button("Save Prefab"))
        {
            SaveBuildingAsPrefab(script);
        }
    }

    // Sauvegarde du bâtiment en tant que prefab
    public void SaveBuildingAsPrefab(BuildingGenerator script)
    {
        if (script.building == null)
        {
            Debug.LogError("Aucun bâtiment à sauvegarder. Génère un bâtiment d'abord.");
            return;
        }

        if (!Directory.Exists(script.prefabSavePath))
        {
            Directory.CreateDirectory(script.prefabSavePath);
        }

        string prefabPath = script.prefabSavePath + script.building.name + Random.Range(0.1f, 10f) + ".prefab";
        UnityEditor.PrefabUtility.SaveAsPrefabAsset(script.building, prefabPath);
        Debug.Log("Prefab sauvegardé à : " + prefabPath);
    }
}
