using Flow.Launcher.Plugin;

namespace RiderWorkFlow;

public class Main : IPlugin
{
    private ProjectManager? _projectManager;

    public void Init(PluginInitContext context) 
    {
        _projectManager = new ProjectManager();
    }

    public List<Result> Query(Query query) => _projectManager!.GetResultProjects(query.Search);
}