// ComponentListExample.cs
using UnityEngine;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

public class Jobs : MonoBehaviour
{
    public List<CustomJobs> customComponents = new List<CustomJobs>();
    private bool isExecuting = false;

    public void AddComponent(IJob script)
    {
        customComponents.Add(new CustomJobs { script = script });
    }

    public void RemoveComponent(int index)
    {
        if (index >= 0 && index < customComponents.Count)
        {
            customComponents.RemoveAt(index);
        }
    }

    // Execute all scripts sequentially
    public async Task ExecuteAllSequential()
    {
        if (isExecuting) return;
        isExecuting = true;

        try
        {
            foreach (var component in customComponents)
            {
                if (component.script != null)
                {
                    await component.script.ExecuteAsync();
                    // Wait for each script to complete before moving to the next
                }
            }
        }
        finally
        {
            isExecuting = false;
        }
    }

    // Execute all scripts in parallel
    public async Task ExecuteAllParallel()
    {
        if (isExecuting) return;
        isExecuting = true;

        try
        {
            var executionTasks = customComponents
                .Where(c => c.script != null)
                .Select(c => c.script.ExecuteAsync());

            await Task.WhenAll(executionTasks);
        }
        finally
        {
            isExecuting = false;
        }
    }

    // Unity-friendly wrapper for starting execution
    public void StartExecution(bool parallel = false)
    {
        if (parallel)
        {
            _ = ExecuteAllParallel();
        }
        else
        {
            _ = ExecuteAllSequential();
        }
    }

    // Check if any scripts are still executing
    public bool AreScriptsExecuting()
    {
        return customComponents.Any(c => c.script?.IsExecuting ?? false);
    }
}