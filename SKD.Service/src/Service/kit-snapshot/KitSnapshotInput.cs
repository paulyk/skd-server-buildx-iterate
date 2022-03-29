#nullable enable

namespace SKD.Model;

public class KitSnapshotInput {
    /// <summary>
    /// Leave null to allow system to select current date
    /// </summary>
    public DateTime? RunDate { get; set; }
    public string PlantCode { get; set; } = "";

    /// <summary>
    /// Don't generate snapshot if all kits statuses == NoChange, 
    /// Default: true
    /// </summary>
    public bool RejectIfNoChanges { get; set; } = true;
    /// <summary>
    /// Allow more that one snapshot with the same RunDate.Date but different HHMM.
    /// Default: false
    /// </summary>

    public bool AllowMultipleSnapshotsPerDay { get; set; } = false;
}
