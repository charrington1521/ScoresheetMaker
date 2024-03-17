namespace ScoresheetMaker.Models;

public class PdfModel
{
    public string? PdfPath { get; set; }

    // public int? PaneWidth { get; set; }

    // public int? PaneHeight { get; set; }

    public bool HasPath()
    {
        return !string.IsNullOrEmpty(PdfPath);
    }
}