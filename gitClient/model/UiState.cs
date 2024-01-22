using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibGit2Sharp;
using System.Reactive.Linq;
using ReactiveUI;
using DynamicData;

namespace gitClient.model {
  public class UiState : ReactiveObject {
    private string _LastSelectedDirectory;
    public string LastSelectedDirectory { 
      get => _LastSelectedDirectory; 
      
      set {
        _LastSelectedDirectory = value;
      } }

    public string Content { get; set; }

    public Repository Repository { get; set; }

   


    public void Safe() {
      System.IO.File.WriteAllText("settings.json", System.Text.Json.JsonSerializer.Serialize(this));

    }
    public static UiState Load() {
      return File.Exists("settings.json") ? System.Text.Json.JsonSerializer.Deserialize<UiState>(System.IO.File.ReadAllText("settings.json")) ?? new UiState() : new UiState();
    } 
    }
}
