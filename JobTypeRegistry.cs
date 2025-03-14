// JobTypeRegistry.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

public class JobTypeRegistry
{
    public class JobTypeInfo
    {
        public Type Type { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
    }

    private static List<JobTypeInfo> _jobTypes;

    public static List<JobTypeInfo> GetAllJobTypes()
    {
        if (_jobTypes == null)
        {
            InitializeJobTypes();
        }
        return _jobTypes;
    }

    public static List<JobTypeInfo> SearchJobs(string searchTerm, string category = null)
    {
        return GetAllJobTypes()
            .Where(job => 
                (string.IsNullOrEmpty(category) || job.Category.Equals(category, StringComparison.OrdinalIgnoreCase)) &&
                (string.IsNullOrEmpty(searchTerm) || 
                 job.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                 job.Description.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)))
            .ToList();
    }

    public static List<string> GetCategories()
    {
        return GetAllJobTypes()
            .Select(job => job.Category)
            .Distinct()
            .OrderBy(c => c)
            .ToList();
    }

    private static void InitializeJobTypes()
    {
        _jobTypes = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(assembly => assembly.GetTypes())
            .Where(type => typeof(IJob).IsAssignableFrom(type) && !type.IsInterface && !type.IsAbstract)
            .Select(type => new JobTypeInfo
            {
                Type = type,
                Name = type.Name,
                Description = GetJobDescription(type),
                Category = GetJobCategory(type)
            })
            .OrderBy(info => info.Category)
            .ThenBy(info => info.Name)
            .ToList();
    }

    private static string GetJobDescription(Type type)
    {
        var attribute = type.GetCustomAttribute<JobDescriptionAttribute>();
        return attribute?.Description ?? "No description available.";
    }

    private static string GetJobCategory(Type type)
    {
        var attribute = type.GetCustomAttribute<JobDescriptionAttribute>();
        return attribute?.Category ?? "General";
    }
}