// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace OtherExampleAnalyzer;
using ToolInterface;

public class OtherExample : ITool
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

    public OtherExample()
    {
        Id = 4;
        Name = "OtherExample";
        Description = "OtherExample Description";
        Version = new Version(1, 0, 0);
        IsDeprecated = true;
        CreatorName = "OtherExample Creator";
        CreatorEmail = "creatorcca@example.com";
        LastUpdated = new DateTime(2023, 11, 10).Date;
        LastUpdated = DateTime.Today.Date;

    }

    public Type[] ImplementedInterfaces => this.GetType().GetInterfaces();
}
