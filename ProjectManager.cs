using System.Diagnostics;
using System.Xml.Linq;
using Flow.Launcher.Plugin;

namespace RiderWorkFlow;

public class ProjectManager
{
    private const string IcoPath = "Images/Icon.png";
    private const string IDEName = "rider";
    private readonly string _optionPath;

    public ProjectManager()
    {
        _optionPath = GetOptionPath();
    }
    
    public List<Result> GetResultProjects(string projectName)
    {
        return GetProjects(projectName)
            .Select(e => new Result
            {
                Title = e.Name,
                SubTitle = e.ProjectPath,
                IcoPath = IcoPath,
                Action = (c) => OpenProject(e.ProjectPath)
            }).ToList();
    }
    
    private List<ProjectModel> GetProjects(string projectName)
    {
        var xmlContent = File.ReadAllText(_optionPath);
        var xmlDoc = XDocument.Parse(xmlContent);
        var names = xmlDoc.Descendants("entry")
            .Select(e => new ProjectModel{ ProjectPath = e.Attribute("key")!.Value })
            .Where(e => e.Name.ToLower().Contains(projectName.ToLower())).ToList();
        
        return names;
    }
    
    private static bool OpenProject(string solutionPath)
    {
        var processStartInfo = new ProcessStartInfo
        {
            FileName = solutionPath,
            UseShellExecute = true
        };

        try
        {
            Process.Start(processStartInfo);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private string GetOptionPath()
    {
        var applicationDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\JetBrains\";
        const string optionsFilePath = @"\options\recentSolutions.xml";
        
        var riderVersion = Directory.GetDirectories(applicationDataFolder)
            .Select(Path.GetFileName)
            .FirstOrDefault(e => e != null && e.ToLower().Contains(IDEName));
        
        return applicationDataFolder + riderVersion + optionsFilePath;
    }
}