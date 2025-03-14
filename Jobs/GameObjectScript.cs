using UnityEngine;
using System.Threading.Tasks;
[System.Serializable]
[JobDescription("GameObject Handler", "GameObject")]  // Description and Catagory
public class GameObjectScript : IJob
{
    public string Name { get; set; } = "GameObject Handler";
    
    // Unity Object references
    public GameObject targetObject;
    public Transform targetTransform;
    public Rigidbody targetRigidbody;
    public Material material;
    public AudioClip audioClip;
    
    // Unity types
    public Vector3 position;
    public Quaternion rotation;
    public Color color = Color.white;
    public LayerMask layerMask;
    public AnimationCurve curve = new AnimationCurve();
    public Gradient gradient = new Gradient();

    // Basic types
    public float floatValue = 1f;
    public int intValue;
    public bool boolValue;
    public string stringValue = "Default";
    public bool IsExecuting { get; private set; }
    public void Initialize()
    {
        Debug.Log($"Initialized {Name} script");
    }
    public async Task ExecuteAsync()
    {
        IsExecuting = true;
        Debug.Log($"Starting: {Name}");

        // Call a sub-function instead of directly delaying
        await DoJob();

        Debug.Log($"Finished {Name}");
        IsExecuting = false;
    }

    private async Task DoJob()
    {
        // Simulate work or delay in a separate function
        await Task.Delay(1);
    }
}