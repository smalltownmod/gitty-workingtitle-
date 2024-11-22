using Avalonia.Controls;
using LibGit2Sharp;
using System.Linq;


namespace gitClient.views {
  public partial class BranchView : UserControl {
    public BranchView() {
      InitializeComponent();
    }

    public void RefreshBranches(Repository repo) {
      dgvBranchesLocal.ItemsSource = repo.Branches.Where(b => b.IsCurrentRepositoryHead)
        .Concat(repo.Branches.Where(b => !b.IsCurrentRepositoryHead && !b.IsRemote)).ToList();
      dgvBranchesRemote.ItemsSource = repo.Branches.Where(b => b.IsRemote).ToList();
    }
  }
}