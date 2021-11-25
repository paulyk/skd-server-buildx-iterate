namespace SKD.Model;

public abstract class EntityBase {
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? RemovedAt { get; set; }

    public EntityBase() {
        CreatedAt = DateTime.UtcNow;
    }
}
