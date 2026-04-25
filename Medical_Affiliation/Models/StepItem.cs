public class StepItem
{
    public string Key { get; set; }
    public string Label { get; set; }
    public string Group { get; set; }
    public string Ctrl { get; set; }
    public string Act { get; set; }
    public List<string> Levels { get; set; } = new();

}