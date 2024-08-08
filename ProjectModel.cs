namespace RiderWorkFlow;

public class ProjectModel
{
    public string Name => Path.GetFileNameWithoutExtension(ProjectPath);

    public string ProjectPath { get; set; } = string.Empty;
}