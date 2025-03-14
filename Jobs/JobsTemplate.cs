using UnityEngine;
using System.Threading.Tasks;

[System.Serializable]
[JobDescription("JobsTemplate", "JobsTemplate")]  // Description and Catagory
public class JobsTemplate: IJob
{
    public string Name { get; set; } = "Movement";
    public bool IsExecuting { get; private set; }

    public void Initialize()
    {
        Debug.Log($"Initialized {Name} script");
    }

    public async Task ExecuteAsync()
    {
        IsExecuting = true;
        Debug.Log($"Starting ");

        // Simulate some work
        await Task.Delay(1); // Example delay based on speed

        Debug.Log($"Finished ");
        IsExecuting = false;
    }
}