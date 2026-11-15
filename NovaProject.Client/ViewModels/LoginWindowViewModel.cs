using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NovaProject.Client.Services;
using NovaProject.Core.Infrastructure.Data.Shared;

namespace NovaProject.Client.ViewModels;

public partial class LoginWindowViewModel : ViewModelBase
{
    private readonly ChatService _chatService;

    [ObservableProperty] private string _userNameInput;
    [ObservableProperty] private string _passwordInput;
    [ObservableProperty] private bool _showPasswordInput;
    
    //Registration
    [ObservableProperty] private string _registerUserNameInput;
    [ObservableProperty] private string _registerPasswordInput;
    [ObservableProperty] private string _registerRepeatPasswordInput;
    [ObservableProperty] private bool _showRegisterPasswordInput;
    
    [ObservableProperty] private bool _isRegistrationToggled;

    [ObservableProperty] private bool _isWaitingForDaemonResponse;
    [ObservableProperty] private string _loadingMessage;
    [ObservableProperty] private string _responseMessage;

    [ObservableProperty] private bool _isLoggedIn;
    
    public LoginWindowViewModel(ChatService chatService)
    {
        _chatService = chatService;
        _chatService.AuthenticationSucceeded += OnAuthenticationSuccess;
        _chatService.AuthenticationFailed += OnAuthenticationFailed;
        _chatService.RegisterSucceeded += OnRegistrationSuccess;
        _chatService.RegisterFailed += OnRegistrationFailed;
    }

    [RelayCommand]
    private void TryLogIn()
    {
        if (string.IsNullOrEmpty(UserNameInput) || string.IsNullOrEmpty(UserNameInput))
        {
            return;
        }

        IsWaitingForDaemonResponse = true;
        LoadingMessage = "Attempting Log In...";
        
        _chatService.Login(UserNameInput, PasswordInput);
    }

    [RelayCommand]
    private void TryRegister()
    {
        if (string.IsNullOrWhiteSpace(RegisterUserNameInput) || 
            string.IsNullOrWhiteSpace(RegisterPasswordInput) ||
            string.IsNullOrWhiteSpace(RegisterRepeatPasswordInput))
            return;
        if (RegisterPasswordInput != RegisterRepeatPasswordInput) return;

        IsWaitingForDaemonResponse = true;
        LoadingMessage = "Attempting Register...";
        _chatService.Register(RegisterUserNameInput, RegisterPasswordInput);
    }

    [RelayCommand]
    private void ToggleRegister()
    {
        IsRegistrationToggled = !IsRegistrationToggled;
    }

    private void OnAuthenticationSuccess(object? sender, LocalUid loggedInUser)
    {
        IsWaitingForDaemonResponse = true;
        LoadingMessage = "Unlocking Vault...";
        IsLoggedIn = true;
    }

    private void OnAuthenticationFailed(object? sender, string reason)
    {
        IsWaitingForDaemonResponse = false;
        ResponseMessage = reason;
    }

    private void OnRegistrationSuccess(object? sender, EventArgs e)
    {
        IsWaitingForDaemonResponse = true;
        ResponseMessage = "Registration Successful";
        IsRegistrationToggled = false;
    }

    private void OnRegistrationFailed(object? sender, string reason)
    {
        IsWaitingForDaemonResponse = false;
        ResponseMessage = reason;
    }
}