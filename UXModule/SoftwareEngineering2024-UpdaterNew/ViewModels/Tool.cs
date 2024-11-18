/******************************************************************************
* Filename    = Tool.cs
*
* Author      = Garima Ranjan
*
* Product     = Updater
* 
* Project     = Lab Monitoring Software
*
* Description = ViewModel for tools
*****************************************************************************/

using System.ComponentModel;

namespace ViewModels;
public class Tool : INotifyPropertyChanged
{

    private string? _id;
    private string? _name;
    private string? _version;
    private string? _description;
    private string? _deprecated;
    private string? _createdBy;
    private string? _creatorEmail;
    private string? _lastUpdated;
    private string? _lastModified;

    public string ID
    {
        get => _id ?? "N/A";
        set {
            if (_id != value)
            {
                _id = value;
                OnPropertyChanged(nameof(ID));
            }
        }
    }

    public string Name
    {
        get => _name ?? "N/A";
        set {
            if (_name != value)
            {
                _name = value;
                OnPropertyChanged(nameof(Name));
            }
        }
    }

    public string Version
    {
        get => _version ?? "N/A";
        set {
            if (_version != value)
            {
                _version = value;
                OnPropertyChanged(nameof(Version));
            }
        }
    }

    public string Description
    {
        get => _description ?? "N/A";
        set {
            if (_description != value)
            {
                _description = value;
                OnPropertyChanged(nameof(Description));
            }
        }
    }

    public string Deprecated
    {
        get => _deprecated ?? "N/A";
        set {
            if (_deprecated != value)
            {
                _deprecated = value;
                OnPropertyChanged(nameof(Deprecated));
            }
        }
    }

    public string CreatedBy
    {
        get => _createdBy ?? "N/A";
        set {
            if (_createdBy != value)
            {
                _createdBy = value;
                OnPropertyChanged(nameof(CreatedBy));
            }
        }
    }

    public string CreatorEmail
    {
        get => _creatorEmail ?? "N/A";
        set {
            if (_creatorEmail != value)
            {
                _creatorEmail = value;
                OnPropertyChanged(nameof(CreatorEmail));
            }
        }
    }
    public string LastModified
    {
        get => _lastModified ?? "N/A";
        set {
            if (_lastModified != value)
            {
                _lastModified = value;
                OnPropertyChanged(nameof(LastModified));
            }
        }
    }
    public string LastUpdated
    {
        get => _lastUpdated ?? "N/A";
        set {
            if (_lastUpdated != value)
            {
                _lastUpdated = value;
                OnPropertyChanged(nameof(LastUpdated));
            }
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged(string? propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
