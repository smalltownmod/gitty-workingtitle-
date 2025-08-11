using System;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using MsBox.Avalonia.Enums;
using static MsBox.Avalonia.MessageBoxManager;


namespace gitClient.views {
  public partial class OpenRepWin : Window {
    
    public OpenRepWin() {
      InitializeComponent();
    }
    public void TitleOverride(string title) {
      Title = title;
      btnOpen.Content = title; 
    }

    public void BtnOpenRepo(object sender, Avalonia.Interactivity.RoutedEventArgs e) {
      try {
        MainWindow.RepPath = RepPath.Text;
         Close();
      }
      catch (Exception ex) {
        GetMessageBoxStandard("Error!", ex.Message, ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error,
          WindowStartupLocation.CenterOwner).ShowWindowDialogAsync(this);
      }
    }


    private async void Browse_OnClick(object? sender, RoutedEventArgs e) {
      try {
        var res = await GetTopLevel(this)!.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions {
          Title = "Browse Folder",
        });
        RepPath.Text = res.First().TryGetLocalPath() ?? string.Empty;
      }
      catch (Exception ex) {
        //damit es nicht abraucht
      }
    }

    private void BtnCancel(object? sender, RoutedEventArgs e) {
      Close();
    }
  }
}