/******************************************************************************
* Filename    = ITool.cs
*
* Author      = Garima Ranjan
*
* Product     = Updater
* 
* Project     = Lab Monitoring Software
*
* Description = Interface defined for tool
*****************************************************************************/

namespace ToolInterface;

/// <summary>
/// Represents the contract that any tool must implement
/// It provides metadata about the tool such as version, description, and creator info.
/// </summary>
public interface ITool
{
    /// <summary>
    /// Gets or sets the unique identifier for the tool.
    /// </summary>
    int Id { get; set; }

    /// <summary>
    /// Gets or sets the name of the tool.
    /// </summary>
    string Name { get; set; }

    /// <summary>
    /// Gets or sets the description of the tool.
    /// </summary>
    string Description { get; set; }

    /// <summary>
    /// Gets or sets the version of the tool.
    /// </summary>
    Version? Version { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the last update of the tool was released.
    /// </summary>
    DateTime? LastUpdated { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the tool was last modified.
    /// </summary>
    DateTime? LastModified { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the tool is deprecated.
    /// </summary>
    bool IsDeprecated { get; set; }

    /// <summary>
    /// Gets or sets the name of the creator of the tool.
    /// </summary>
    string CreatorName { get; set; }

    /// <summary>
    /// Gets or sets the email address of the creator of the tool.
    /// </summary>
    string CreatorEmail { get; set; }

    /// <summary>
    /// Gets the array of interfaces that the tool implements.
    /// </summary>
    Type[] ImplementedInterfaces { get; }
}
