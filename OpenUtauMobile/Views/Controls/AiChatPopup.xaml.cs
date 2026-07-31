using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using OpenUtauMobile.Utils;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace OpenUtauMobile.Views.Controls;

public partial class AiChatPopup : ContentPage, IDisposable {
    private readonly AiChatViewModel _viewModel;
    private bool _disposed;

    /// <summary>
    /// AI 返回指令时的回调，由 EditViewModel 注入
    /// </summary>
    public Func<AiCommandRoot, Task>? OnExecuteCommand { get; set; }

    public AiChatPopup() {
        InitializeComponent();
        _viewModel = new AiChatViewModel();
        BindingContext = _viewModel;
    }

    private async void ButtonSend_Clicked(object sender, EventArgs e) {
        var input = _viewModel.InputText?.Trim();
        if (string.IsNullOrWhiteSpace(input)) return;

        _viewModel.InputText = string.Empty;
        AddMessage(input, isUser: true);

        LoadingIndicator.IsRunning = true;
        LoadingIndicator.IsVisible = true;

        try {
            var reply = await DeepSeekService.Instance.SendChatAsync(input);
            
            // 解析指令
            var commands = DeepSeekService.Instance.ParseCommands(reply);
            
            // 显示 AI 的回复消息
            var displayText = commands?.Message;
            if (string.IsNullOrWhiteSpace(displayText)) {
                displayText = $"已执行 {commands?.Actions?.Count ?? 0} 个操作";
            }
            AddMessage(displayText, isUser: false);

            // 如果有指令需要执行（只要 actions 中存在任一非 message 指令就全部执行）
            if (commands?.Actions != null && commands.Actions.Count > 0
                && commands.Actions.Any(a => a.Type != "message")
                && OnExecuteCommand != null) {
                await OnExecuteCommand(commands);
                AddMessage("✅ 编曲操作已自动执行完毕！", isUser: false);
            }
        } catch (Exception ex) {
            AddMessage($"❌ 错误：{ex.Message}", isUser: false);
        } finally {
            LoadingIndicator.IsRunning = false;
            LoadingIndicator.IsVisible = false;
        }
    }

    private void AddMessage(string content, bool isUser) {
        _viewModel.Messages.Add(new ChatMessage {
            Content = content,
            IsUser = isUser
        });
        // 滚动到底部
        if (_viewModel.Messages.Count > 0) {
            MessagesCollection.ScrollTo(_viewModel.Messages.Count - 1);
        }
    }

    private void ButtonClose_Clicked(object sender, EventArgs e) {
        Navigation.PopModalAsync();
    }

    public void Dispose() {
        if (!_disposed) {
            _disposed = true;
        }
    }

    protected override void OnDisappearing() {
        base.OnDisappearing();
        Dispose();
    }
}

public class AiChatViewModel : ReactiveObject {
    [Reactive] public string InputText { get; set; } = string.Empty;
    public ObservableCollection<ChatMessage> Messages { get; set; } = new();
}

public class ChatMessage {
    public string Content { get; set; } = string.Empty;
    public bool IsUser { get; set; }

    public Color BubbleColor => IsUser 
        ? Color.FromArgb("#0078D4") 
        : Color.FromArgb("#3A3A3A");

    public Color TextColor => IsUser 
        ? Colors.White 
        : Color.FromArgb("#E0E0E0");

    public Thickness BubbleMargin => IsUser 
        ? new Thickness(60, 2, 5, 2) 
        : new Thickness(5, 2, 60, 2);
}
