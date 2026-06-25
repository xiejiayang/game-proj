using UnityEditor;
using UnityEngine;
using Dujiangyan.Data;
using Dujiangyan.Systems;

public class ValidateCoreServices
{
    [MenuItem("Dujiangyan/Validate Core Services")]
    public static void Validate()
    {
        var runner = new GameObject("CoreServiceTestRunner");
        runner.AddComponent<InputSystem>();
        runner.AddComponent<AudioSystem>();
        runner.AddComponent<SaveSystem>();
        runner.AddComponent<UIManager>();
        runner.AddComponent<PuzzleSystem>();
        runner.AddComponent<WaterSimulation>();

        // Wait one editor update so all Awake/OnEnable complete
        EditorApplication.delayCall += () =>
        {
            bool ok = ExecuteTest();
            EditorApplication.Exit(ok ? 0 : 1);
        };
    }

    private static bool ExecuteTest()
    {
        bool ok = true;

        var puzzle = PuzzleSystem.Instance;
        if (puzzle == null)
        {
            Debug.LogError("[ValidateCoreServices] PuzzleSystem instance is null.");
            return false;
        }

        var runtime = puzzle.InitLevel("L1");
        if (runtime == null)
        {
            Debug.LogError("[ValidateCoreServices] InitLevel returned null.");
            return false;
        }

        if (runtime.state != PuzzleState.Editing)
        {
            Debug.LogError($"[ValidateCoreServices] Expected Editing, got {runtime.state}.");
            ok = false;
        }

        if (runtime.inventory.bamboo != 8)
        {
            Debug.LogError($"[ValidateCoreServices] Expected bamboo=8, got {runtime.inventory.bamboo}.");
            ok = false;
        }

        // Place a bamboo in front of the wall to split/deflect water
        var place = puzzle.TryPlaceBlock("bamboo", new Vector3(-1f, 0f, 2f), 0);
        if (!place.success)
        {
            Debug.LogError($"[ValidateCoreServices] Place block failed: {place.errorMessage}");
            ok = false;
        }

        if (runtime.inventory.bamboo != 7)
        {
            Debug.LogError($"[ValidateCoreServices] Inventory not decremented: bamboo={runtime.inventory.bamboo}.");
            ok = false;
        }

        var simResult = puzzle.StartSimulation();
        if (!simResult.success)
        {
            Debug.LogError($"[ValidateCoreServices] StartSimulation failed: {simResult.errorMessage}");
            ok = false;
        }

        if (runtime.state != PuzzleState.Simulating)
        {
            Debug.LogError($"[ValidateCoreServices] Expected Simulating, got {runtime.state}.");
            ok = false;
        }

        // Advance simulation manually to reach target duration
        const float fixedStep = 0.02f;
        int steps = Mathf.CeilToInt(puzzle.CurrentConfig.targetDuration / fixedStep) + 100;
        for (int i = 0; i < steps && runtime.state == PuzzleState.Simulating; i++)
        {
            WaterSimulation.Instance.Tick(fixedStep);
            runtime.simulationTime += fixedStep;

            var check = LevelResult.Evaluate(puzzle.CurrentConfig, runtime);
            if (check.isSuccess || check.failReason != FailReason.None)
            {
                runtime.state = PuzzleState.Settling;
                runtime.result = check;
                WaterSimulation.Instance.Clear();
                break;
            }
        }

        if (runtime.state != PuzzleState.Settling)
        {
            Debug.LogError($"[ValidateCoreServices] Expected Settling, got {runtime.state}.");
            ok = false;
        }

        if (!runtime.result.isSuccess)
        {
            Debug.LogError($"[ValidateCoreServices] Expected success, got failReason={runtime.result.failReason}.");
            ok = false;
        }

        if (!runtime.result.isFrugal)
        {
            Debug.LogError("[ValidateCoreServices] Expected frugal result.");
            ok = false;
        }

        // Test reset
        puzzle.Reset();
        if (runtime.state != PuzzleState.Editing)
        {
            Debug.LogError("[ValidateCoreServices] Reset did not return to Editing.");
            ok = false;
        }
        if (runtime.inventory.bamboo != 8)
        {
            Debug.LogError("[ValidateCoreServices] Reset did not restore inventory.");
            ok = false;
        }

        if (ok)
        {
            Debug.Log("[ValidateCoreServices] Core services validation passed.");
        }
        return ok;
    }
}
