using UnityEngine;

[System.Serializable]
public class CustomJobs
{
    public string name;
    [SerializeReference] public IJob script;
}