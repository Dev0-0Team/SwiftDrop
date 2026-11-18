namespace SwiftDrop.Models.Entities;

/// <summary>ERD table: SystemSetting.</summary>
public class SystemSetting
{
    public int Id { get; set; }
    public string SettingName { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string? Description { get; set; }
}
