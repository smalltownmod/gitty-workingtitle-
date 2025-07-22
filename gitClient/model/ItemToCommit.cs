

namespace gitClient.model {
  public class ItemToCommit  {
    public bool Checked { get; set; }
    public string State { get; set; }
    public string Path { get; set; }
    public ItemToCommit(bool check, string state, string path) { 
    Checked = check ;
      State = state ?? string.Empty;
      Path = path  ?? string.Empty;
    }
  }
}
