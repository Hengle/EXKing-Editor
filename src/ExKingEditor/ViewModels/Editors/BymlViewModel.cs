﻿using AvaloniaEdit;
using Cead;
using Cead.Handles;
using ExKingEditor.Core;
using ExKingEditor.Models;

namespace ExKingEditor.ViewModels.Editors;

public partial class BymlViewModel : ReactiveEditor
{
    private string _yaml;

    public TextEditor Editor { get; set; } = null!;
    public string Yaml { get; set; }

    public BymlViewModel(string file) : base(file)
    {
        // Load source into readonly field for diffing
        using Byml byml = Byml.FromBinary(RawData());
        _yaml = byml.ToText();

        Yaml = _yaml;
    }

    public override void SaveAs(string path)
    {
        using Byml byml = Byml.FromText(Yaml);
        using DataHandle handle = byml.ToBinary(false, version: 7);

        Span<byte> data = (_compressed = path.EndsWith(".zs")) ? TotkZstd.Compress(path, handle) : handle;

        if (path == _file) {
            _stream.Seek(0, SeekOrigin.Begin);
            _stream.Write(data);
        }
        else {
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            using FileStream fs = File.Create(path);
            fs.Write(data);
        }

        _yaml = Yaml;
        ToastSaveSuccess(path);
    }

    public override void Undo()
    {
        Editor.Undo();
        base.Undo();
    }

    public override void Redo()
    {
        Editor.Redo();
        base.Redo();
    }

    public override void SelectAll()
    {
        Editor.SelectAll();
        base.SelectAll();
    }

    public override void Cut()
    {
        Editor.Cut();
        base.Cut();
    }

    public override void Copy()
    {
        Editor.Copy();
        base.Copy();
    }

    public override void Paste()
    {
        Editor.Paste();
        base.Paste();
    }

    public override void Find()
    {
        Editor.SearchPanel.IsReplaceMode = false;
        Editor.SearchPanel.Open();
        base.Find();
    }

    public override void FindAndReplace()
    {
        Editor.SearchPanel.IsReplaceMode = true;
        Editor.SearchPanel.Open();
        base.FindAndReplace();
    }

    public override bool HasChanged()
    {
        return _yaml != Yaml;
    }
}
