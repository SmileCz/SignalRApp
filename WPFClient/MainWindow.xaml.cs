using System.Windows;
using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Threading.Tasks;

namespace WPFClient;

public partial class MainWindow : Window
{
    private HubConnection _chatConnection;
    private HubConnection _counterConnection;

    public MainWindow()
    {
        InitializeComponent();

        _chatConnection = new HubConnectionBuilder()
            .WithUrl("https://localhost:7224/chathub")
            .WithAutomaticReconnect()
            .Build();
        
        _counterConnection = new HubConnectionBuilder()
            .WithUrl("https://localhost:7224/counterhub")
            .WithAutomaticReconnect()
            .Build();

        _chatConnection.Reconnecting += (sender) =>
        {
            Dispatcher.Invoke(() =>
            {
                var newMessage = "Attempting to reconeect...";
                messages.Items.Add(newMessage);
            });

            return Task.CompletedTask;
        };
        
        _chatConnection.Reconnected += (sender) =>
        {
            Dispatcher.Invoke(() =>
            {
                var newMessage = "Reconnected to the server";
                messages.Items.Clear();
                messages.Items.Add(newMessage);
            });

            return Task.CompletedTask;
        };
        
        _chatConnection.Closed += (sender) =>
        {
            Dispatcher.Invoke(() =>
            {
                var newMessage = "Connection Closed";
                messages.Items.Add(newMessage);
                OpenConnection.IsEnabled = true;
                sendMessage.IsEnabled = false;
            });

            return Task.CompletedTask;
        };
        
    }

    private async void OpenConnection_OnClick(object sender, RoutedEventArgs e)
    {
        _chatConnection.On<string, string>("ReceiveMessage", (user, message) =>
        {
            Dispatcher.Invoke(() =>
            {
                var newMessage = $"{user}: {message}";
                messages.Items.Add(newMessage);
            });
        });

        try
        {
            await _chatConnection.StartAsync();
            messages.Items.Add("Connection Started");
            OpenConnection.IsEnabled = false;
            sendMessage.IsEnabled = true;
        }
        catch (Exception ex)
        {
            messages.Items.Add(ex.Message);
        }

    }

    private async void SendMessage_OnClick(object sender, RoutedEventArgs e)
    {
        try
        {
            await _chatConnection.InvokeAsync("SendMessage", "WPF Client", messageInput.Text);
        }
        catch (Exception ex)
        {
            messages.Items.Add(ex.Message);
        }
    
}

    private async void OpenCounter_OnClick(object sender, RoutedEventArgs e)
    {
        try
        {
            await _counterConnection.StartAsync();
            OpenCounter.IsEnabled = false;
            IncrementCounter.IsEnabled = true;
        }
        catch (Exception ex)
        {
            messages.Items.Add(ex.Message);
        }
    }

    private async void IncrementCounter_OnClick(object sender, RoutedEventArgs e)
    {
        try
        {
            await _counterConnection.InvokeAsync("AddToTotal", "WPF Client", 1);
        }
        catch (Exception ex)
        {
            messages.Items.Add(ex.Message); 
        }
    }
}