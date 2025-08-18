using System.Diagnostics;
using System.Xml.Linq;
using Flow.Launcher.Plugin;

namespace RiderWorkFlow;

public class ProjectManager
{
    public const string IcoPath = "Images/Icon.png";
    public const string UserHomePlaceholder = "$USER_HOME$";

    private readonly string _riderVersion;
    private readonly string _riderExecutablePath;
    private readonly string _rideRecentSolutionsPath;

    public ProjectManager()
    {
        _riderVersion = LocalRiderInstallations.GetLatestRiderVersion();
        _riderExecutablePath = LocalRiderInstallations.GetRiderExecutablePath(_riderVersion);
        _rideRecentSolutionsPath = LocalRiderInstallations.GetRiderRecentSolutionsFilePath();
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
        var xmlContent = File.ReadAllText(_rideRecentSolutionsPath);
        var xmlDoc = XDocument.Parse(xmlContent);
        var names = xmlDoc.Descendants("entry")
            .Select(e => new ProjectModel { ProjectPath = FixPathForWindows(ReplaceUserHome(e.Attribute("key")!.Value)) })
            .Where(e => e.Name.Contains(projectName, StringComparison.CurrentCultureIgnoreCase)).ToList();

        return names;
    }

    private static string ReplaceUserHome(string path)
    {
        if (string.IsNullOrEmpty(path)) return path;
        return path.Replace(UserHomePlaceholder, Environment.GetFolderPath(Environment.SpecialFolder.UserProfile));
    }

    private static string FixPathForWindows(string path)
    {
        if (string.IsNullOrEmpty(path)) return path;
        return path.Replace("/", "\\");
    }

    private bool OpenProject(string solutionPath)
    {
        var processStartInfo = new ProcessStartInfo
        {
            FileName = _riderExecutablePath,
            Arguments = $"\"{solutionPath}\"",
            UseShellExecute = false,
            CreateNoWindow = false
        };

        try
        {
            Process.Start(processStartInfo);
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }
}