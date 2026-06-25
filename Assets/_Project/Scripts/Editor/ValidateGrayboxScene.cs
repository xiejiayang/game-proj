using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Dujiangyan.Core;
using Dujiangyan.Systems;

public class ValidateGrayboxScene
{
    [MenuItem("Dujiangyan/Validate L1 Graybox Scene")]
    public static void Validate()
    {
        var scene = EditorSceneManager.OpenScene("Assets/_Project/Scenes/Level_L1.unity", OpenSceneMode.Single);
        bool ok = true;

        ok &= HasObject<Camera>("Main Camera");
        ok &= HasObject<Light>("Directional Light");
        ok &= HasObject<Collider>("Ground");
        ok &= HasObject<Canvas>("Canvas");
        ok &= HasObject<LevelBootstrap>("LevelBootstrap");
        ok &= HasObject<PuzzleSystem>("Services");
        ok &= HasObject<WaterSimulation>("Services");
        ok &= HasObject<BlockPlacement>("Services");
        ok &= HasObject<InputSystem>("Services");
        ok &= HasObject<SaveSystem>("Services");
        ok &= HasObject<AudioSystem>("Services");
        ok &= HasObject<UIManager>("Services");

        var canvas = GameObject.Find("Canvas");
        if (canvas != null)
        {
            var buttons = canvas.GetComponentsInChildren<Button>(true);
            string[] expectedLabels = { "竹笼", "杩槎", "旋转", "放水", "重置" };
            foreach (var label in expectedLabels)
            {
                bool found = buttons.Any(b =>
                {
                    var text = b.GetComponentInChildren<TextMeshProUGUI>(true);
                    return text != null && text.text == label;
                });
                if (!found)
                {
                    Debug.LogError($"[ValidateGrayboxScene] Button '{label}' not found.");
                    ok = false;
                }
            }
        }
        else
        {
            ok = false;
        }

        if (ok)
        {
            Debug.Log("[ValidateGrayboxScene] L1 graybox scene is valid.");
            EditorApplication.Exit(0);
        }
        else
        {
            EditorApplication.Exit(1);
        }
    }

    private static bool HasObject<T>(string name) where T : Component
    {
        var go = GameObject.Find(name);
        if (go == null)
        {
            Debug.LogError($"[ValidateGrayboxScene] GameObject '{name}' missing.");
            return false;
        }
        if (go.GetComponentInChildren<T>(true) == null)
        {
            Debug.LogError($"[ValidateGrayboxScene] Component {typeof(T).Name} missing on '{name}'.");
            return false;
        }
        return true;
    }
}
