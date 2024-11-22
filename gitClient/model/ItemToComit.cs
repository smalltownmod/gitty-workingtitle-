using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gitClient.model {
  public class ItemToComit :ReactiveObject {
    public bool Checked { get; set; }
    public string State { get; set; }
    public string Path { get; set; }
    public ItemToComit(bool check, string state, string path) { 
    Checked = check ;
      State = state ?? string.Empty;
      Path = path  ?? string.Empty;
    }
  }
}
