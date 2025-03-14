// MovementScript.cs
using UnityEngine;
using System.Threading.Tasks;

[System.Serializable]
[JobDescription("Movement", "Movement")]  // Description and Catagory
public class MovementScript : IJob
{
    public string Name { get; set; } = "Movement";
    public float speed = 5f;
    public Vector3 direction;
    public LayerMask layerMask = new LayerMask();
    public bool IsExecuting { get; private set; }

    public void Initialize()
    {
        Debug.Log($"Initialized {Name} script");
    }

    public async Task ExecuteAsync()
    {
        IsExecuting = true;
        Debug.Log($"Starting movement at speed: {speed}");

        // Simulate some work
        await Task.Delay((int)(1000 * speed)); // Example delay based on speed

        Debug.Log($"Finished movement at speed: {speed}");
        IsExecuting = false;
    }
}

// Similar updates for RotationScript and ScalingScript...

[System.Serializable]
[JobDescription("Rotation", "Rotation")]  // Description and Catagory
public class RotationScript : IJob
{
    public string Name { get; set; } = "Rotation";
    public float rotationSpeed = 30f;
    public Vector3 axis = Vector3.up;
    public bool IsExecuting { get; private set; }
    public void Initialize() 
    {
        Debug.Log($"Initialized {Name} script");
    }

    public async Task ExecuteAsync()
    {
        IsExecuting = true;
        // Call a sub-function instead of directly delaying
        await DoJob();
        Debug.Log($"Executing rotation at speed: {rotationSpeed}");
        IsExecuting = true;
    }
    private async Task DoJob()
    {
        // Simulate work or delay in a separate function
        await Task.Delay(1);
    }
}

[System.Serializable]
[JobDescription("Scaling", "Scaling")]  // Description and Catagory
public class ScalingScript : IJob
{
    public string Name { get; set; } = "Scaling";
    public Vector3 targetScale = Vector3.one;
    public float duration = 1f;
    public bool IsExecuting { get; private set; }
    public void Initialize() 
    {
        Debug.Log($"Initialized {Name} script");
    }

    public async Task ExecuteAsync()
    {
        IsExecuting = true;
        Debug.Log($"Executing scaling to: {targetScale}");
        // Call a sub-function instead of directly delaying
        await DoJob();
        IsExecuting = false;
    }
    private async Task DoJob()
    {
        // Simulate work or delay in a separate function
        await Task.Delay(1);
    }
}