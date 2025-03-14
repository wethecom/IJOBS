// JobDescriptionAttribute.cs
using System;

[AttributeUsage(AttributeTargets.Class)]
public class JobDescriptionAttribute : Attribute
{
    public string Description { get; private set; }
    public string Category { get; private set; }

    public JobDescriptionAttribute(string description, string category = "General")
    {
        Description = description;
        Category = category;
    }
}