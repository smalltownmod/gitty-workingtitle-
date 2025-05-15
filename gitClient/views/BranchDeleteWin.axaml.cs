using Avalonia.Controls;
using System;
using System.Linq;
using MsBox.Avalonia.Enums;
using static MsBox.Avalonia.MessageBoxManager;

namespace gitClient.views {
  public partial class BranchDeleteWin : Window {
    public BranchDeleteWin() {
      InitializeComponent();
      try {
        ComBranches.ItemsSource = MainWindow.Repo!.Branches.Where(b => !b.IsCurrentRepositoryHead).ToList();
        ComBranches.SelectedIndex = 0;
      }
      catch {
        //
      }
    }

    private void BtnCancel(object sender, Avalonia.Interactivity.RoutedEventArgs e) {
      Close();
    }

    private void BtnDelete(object sender, Avalonia.Interactivity.RoutedEventArgs e) {
      try {
        var branchname = ComBranches.SelectedItem?.ToString() ?? string.Empty;
        MainWindow.Repo!.Branches.Remove(branchname);
      }
      catch (Exception ex) {
        GetMessageBoxStandard("error", ex.Message, ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error,
          WindowStartupLocation.CenterOwner).ShowWindowDialogAsync(this);
      }

      Close();
    }
  }
}