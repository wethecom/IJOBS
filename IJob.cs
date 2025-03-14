// IScript.cs
using UnityEngine;
using System.Threading.Tasks;

public interface IJob
{
    string Name { get; set; }
    void Initialize();
    Task ExecuteAsync();
    bool IsExecuting { get; }
}