using UnityEngine;
using System.Threading.Tasks;

[System.Serializable]
[JobDescription("Executes the selected job when the specified input event occurs.", "Input Triggers")]
public class InputEventTrigger : IJob
{
    public string Name { get; set; } = "InputEventTrigger";
    public bool IsExecuting { get; private set; }

    [Tooltip("The key that triggers the job")]
   [SerializeField]
    public KeyCode triggerKey = KeyCode.Space;

    [Tooltip("When should the job trigger")]
    public InputTriggerType triggerType = InputTriggerType.OnKeyDown;

    [Tooltip("The job to execute when triggered")]
    public IJob jobToExecute;

    [Tooltip("Should this trigger repeat after execution?")]
    public bool repeat = false;

    public enum InputTriggerType
    {
        OnKeyDown,
        OnKeyUp,
        WhileKeyHeld
    }

    public void Initialize()
    {
        Debug.Log($"Initialized {Name} - Waiting for {triggerKey} {triggerType}");
        if (jobToExecute != null)
        {
            jobToExecute.Initialize();
        }
    }

    public async Task ExecuteAsync()
    {
        IsExecuting = true;

        while (IsExecuting)
        {
            bool triggered = false;

            switch (triggerType)
            {
                case InputTriggerType.OnKeyDown:
                    triggered = Input.GetKeyDown(triggerKey);
                    break;
                case InputTriggerType.OnKeyUp:
                    triggered = Input.GetKeyUp(triggerKey);
                    break;
                case InputTriggerType.WhileKeyHeld:
                    triggered = Input.GetKey(triggerKey);
                    break;
            }

            if (triggered && jobToExecute != null)
            {
                Debug.Log($"Input {triggerKey} {triggerType} detected - Executing {jobToExecute.Name}");
                await jobToExecute.ExecuteAsync();

                if (!repeat)
                {
                    IsExecuting = false;
                    break;
                }
            }

            await Task.Delay(10); // Small delay to prevent excessive polling
        }

        Debug.Log($"Input trigger completed for {Name}");
    }
}