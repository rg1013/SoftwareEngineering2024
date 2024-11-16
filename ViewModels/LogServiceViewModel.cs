/*************************************************************************************
* Filename    = LogServiceViewModel.cs
*
* Author      = N.Pawan Kumar
*
* Product     = Updater
* 
* Project     = Lab Monitoring Software
*
* Description = View Model for displaying available analyzers information on the UI
**************************************************************************************/

using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Threading;
using Updater;

namespace ViewModels;

///<summary>
/// The LogServiceViewModel class handles the logic for managing log details,
/// notifications, and controls the visibility of the log details section
/// in the UI. It implements INotifyPropertyChanged for data binding.
///</summary>
public class LogServiceViewModel : INotifyPropertyChanged
{
    private string _logDetails = "";  // Holds the current log details to be displayed
    private string _notificationMessage = "";  // Stores the current notification message
    private bool _notificationVisible = false;  // Controls whether the notification is visible
    private string _toolsDirectoryMessage;  // Message for displaying the tools directory
    private bool _isLogExpanded = false;  // Tracks whether the log section is expanded or collapsed
    private DispatcherTimer _timer;  // Timer to auto-hide notifications after a set interval

    ///<summary>
    /// Constructor for LogServiceViewModel.
    /// Initializes the timer and sets up the tools directory message.
    ///</summary>
    public LogServiceViewModel()
    {
        // Initialize the timer with a 15-second interval for auto-hiding notifications
        _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(15) };
        _timer.Tick += (sender, e) => { HideNotification(); };  // Event handler to hide notification after the interval

        // Setting the message that indicates where new tools can be added
        _toolsDirectoryMessage = $"New Tools can be added in {AppConstants.ToolsDirectory}";
    }

    ///<summary>
    /// Gets or sets the log details that are displayed in the UI.
    /// This is bound to the LogDetails section in the UI.
    ///</summary>
    public string LogDetails
    {
        get => _logDetails;
        set
        {
            // Update the log details and notify the UI of the change
            _logDetails = value;
            OnPropertyChanged(nameof(LogDetails));
        }
    }

    ///<summary>
    /// Gets or sets the notification message that is displayed in the notification popup.
    ///</summary>
    public string NotificationMessage
    {
        get => _notificationMessage;
        set
        {
            // Update the notification message and notify the UI of the change
            _notificationMessage = value;
            OnPropertyChanged(nameof(NotificationMessage));
        }
    }

    ///<summary>
    /// Gets or sets the visibility of the notification popup.
    /// Controls whether the notification popup is visible or not.
    ///</summary>
    public bool NotificationVisible
    {
        get => _notificationVisible;
        set {
            // Update the notification visibility and notify the UI of the change
            _notificationVisible = value;
            OnPropertyChanged(nameof(NotificationVisible));
        }
    }

    ///<summary>
    /// Gets the message that indicates where new tools can be added.
    /// This message is displayed in the UI to inform the user.
    ///</summary>
    public string ToolsDirectoryMessage => _toolsDirectoryMessage;

    ///<summary>
    /// Gets or sets whether the log section is expanded or collapsed.
    /// This property is bound to the toggle button and controls the visibility
    /// of the log details.
    ///</summary>
    public bool IsLogExpanded
    {
        get => _isLogExpanded;
        set
        {
            // Update the expanded/collapsed state of the log section and notify the UI of the change
            _isLogExpanded = value;
            OnPropertyChanged(nameof(IsLogExpanded));
            // Trigger a change in the visibility of the log details
            OnPropertyChanged(nameof(LogDetailsVisibility));
        }
    }

    ///<summary>
    /// Returns the visibility of the log details section.
    /// If the log section is expanded, it returns Visibility.Visible;
    /// otherwise, it returns Visibility.Collapsed.
    ///</summary>
    public Visibility LogDetailsVisibility => IsLogExpanded ? Visibility.Visible : Visibility.Collapsed;

    ///<summary>
    /// Appends a message to the log details.
    /// This method is used to update the log with new messages, prefixed with a timestamp.
    ///</summary>
    ///<param name="message">The message to append to the log.</param>
    public void UpdateLogDetails(string message)
    {
        // Get the current timestamp in HH:mm:ss dd-MM-yyyy format
        string timestamp = DateTime.Now.ToString("HH:mm:ss dd-MM-yyyy");
        // Append the new message with the timestamp to the log details
        LogDetails = $"[{timestamp}] {message}\n" + LogDetails;
    }

    ///<summary>
    /// Displays a notification with a specified message.
    /// The notification will remain visible for 15 seconds before auto-hiding.
    ///</summary>
    ///<param name="message">The message to display in the notification.</param>
    public void ShowNotification(string message)
    {
        // Set the notification message and make the notification visible
        NotificationMessage = message;
        NotificationVisible = true;

        // Start the timer to auto-hide the notification after 15 seconds
        _timer.Start();
    }

    ///<summary>
    /// Hides the notification popup and stops the auto-hide timer.
    /// This method is called when the timer completes (after 15 seconds).
    ///</summary>
    private void HideNotification()
    {
        // Hide the notification and stop the timer
        NotificationVisible = false;
        _timer.Stop();
    }

    ///<summary>
    /// Occurs when a property value changes.
    /// This is the standard INotifyPropertyChanged event that allows
    /// the ViewModel to notify the UI of changes to properties.
    ///</summary>
    public event PropertyChangedEventHandler? PropertyChanged;

    ///<summary>
    /// Notifies listeners about property changes.
    /// This method is used to raise the PropertyChanged event for any property
    /// that has been modified.
    ///</summary>
    ///<param name="propertyName">The name of the property that changed.</param>
    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        // Raise the PropertyChanged event to notify the UI of property changes
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName ?? string.Empty));
    }
}
