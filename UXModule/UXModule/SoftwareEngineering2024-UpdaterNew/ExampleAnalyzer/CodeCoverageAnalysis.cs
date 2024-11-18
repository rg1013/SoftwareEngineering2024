using ToolInterface;

namespace ExampleAnalyzer;

public class CodeCoverageAnalysis : ITool
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public Version? Version { get; set; }
    public bool IsDeprecated { get; set; }
    public string CreatorName { get; set; }
    public string CreatorEmail { get; set; }
    public DateTime? LastUpdated { get; set; }
    public DateTime? LastModified { get; set; }

    public CodeCoverageAnalysis()
    {
        Id = 3;
        Name = "CodeCoverageAnalysis";
        Description = "CodeCoverageAnalysis Description";
        Version = new Version(2, 0, 0);
        IsDeprecated = false;
        CreatorName = "CodeCoverageAnalysis Creator";
        CreatorEmail = "creatorcca@example.com";
        LastUpdated = new DateTime(2023, 11, 12).Date;
        LastUpdated = DateTime.Today.Date;

    }

    public Type[] ImplementedInterfaces => this.GetType().GetInterfaces();
}
