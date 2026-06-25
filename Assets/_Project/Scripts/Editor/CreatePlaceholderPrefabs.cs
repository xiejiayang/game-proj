using UnityEditor;
using UnityEngine;

public class CreatePlaceholderPrefabs
{
    [MenuItem("Dujiangyan/Create Placeholder Block Prefabs")]
    public static void Create()
    {
        string dir = "Assets/_Project/Prefabs/Blocks";
        EnsureDirectory(dir);

        CreateCubePrefab($"{dir}/Block_Bamboo.prefab", "BambooPlaceholder", new Color(0.4f, 0.6f, 0.3f));
        CreateCubePrefab($"{dir}/Block_Maocha.prefab", "MaochaPlaceholder", new Color(0.55f, 0.4f, 0.25f));
        CreateCubePrefab($"{dir}/Block_Wall.prefab", "WallPlaceholder", new Color(0.5f, 0.5f, 0.5f));

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[CreatePlaceholderPrefabs] Placeholder block prefabs created.");
    }

    private static void CreateCubePrefab(string path, string name, Color color)
    {
        AssetDatabase.DeleteAsset(path);

        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = name;
        go.transform.localScale = Vector3.one * 0.9f;

        var renderer = go.GetComponent<Renderer>();
        Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        mat.color = color;
        renderer.sharedMaterial = mat;

        PrefabUtility.SaveAsPrefabAsset(go, path);
        Object.DestroyImmediate(go);
    }

    private static void EnsureDirectory(string path)
    {
        if (!AssetDatabase.IsValidFolder(path))
        {
            string parent = System.IO.Path.GetDirectoryName(path).Replace('\\', '/');
            string folderName = System.IO.Path.GetFileName(path);
            AssetDatabase.CreateFolder(parent, folderName);
        }
    }
}
