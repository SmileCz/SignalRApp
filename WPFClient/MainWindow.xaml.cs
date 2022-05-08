using System.Windows;
using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace WPFClient;

public partial class MainWindow : Window
{
    private HubConnection _connection;

    public MainWindow()
    {
        InitializeComponent();

        _connection = new HubConnectionBuilder()
            .WithUrl("https://localhost:7224/chathub")
            .WithAutomaticReconnect()
            .Build();

        _connection.Reconnecting += (sender) =>
        {
            Dispatcher.Invoke(() =>
            {
                var newMessage = "Attempting to reconeect...";
                messages.Items.Add(newMessage);
            });

            return Task.CompletedTask;
        };
        
        _connection.Reconnected += (sender) =>
        {
            Dispatcher.Invoke(() =>
            {
                var newMessage = "Reconnected to the server";
                messages.Items.Clear();
                messages.Items.Add(newMessage);
            });

            return Task.CompletedTask;
        };
        
        _connection.Closed += (sender) =>
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
        _connection.On<string, string>("ReceiveMessage", (user, message) =>
        {
            Dispatcher.Invoke(() =>
            {
                var newMessage = $"{user}: {message}";
                messages.Items.Add(newMessage);
            });
        });

        try
        {
            await _connection.StartAsync();
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
            await _connection.InvokeAsync("SendMessage", "WPF Client", messageInput.Text);
        }
        catch (Exception ex)
        {
            messages.Items.Add(ex.Message);
        }
    
}
}