using UnityEngine;
using System.Threading.Tasks;
[System.Serializable]
    [JobDescription("Degugtest.", "Debug")]  // Description and Catagory
public class Debugmebruh  : IJob
{
    public string Name { get; set; } = "DebugMebruh";


    public Vector3 targetScale = Vector3.one;
    public float duration = 1f;
    [SerializeField]
    public GameObject target;


    public bool IsExecuting { get; private set; }

    public void Initialize()
    {
        Debug.Log($"Initialized {Name} script");
    }

    public async Task ExecuteAsync()
    {
        IsExecuting = true;
        Debug.Log($"Starting: {Name}");

        // Simulate some work
        await Task.Delay(1); // Example delay based on speed

        Debug.Log($"Finished {Name}");
        IsExecuting = false;
    }
}
