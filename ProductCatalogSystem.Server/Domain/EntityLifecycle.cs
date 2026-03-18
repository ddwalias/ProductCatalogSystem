namespace ProductCatalogSystem.Server.Domain;

public interface IAuditableEntity
{
    DateTime CreatedAtUtc { get; set; }

    DateTime UpdatedAtUtc { get; set; }
}

public interface ISoftDeletableEntity
{
    DateTime? DeletedAtUtc { get; set; }
}

public abstract class AuditableEntity : IAuditableEntity
{
    public DateTime CreatedAtUtc { get; set; }

    public DateTime UpdatedAtUtc { get; set; }
}

public abstract class SoftDeletableEntity : AuditableEntity, ISoftDeletableEntity
{
    public DateTime? DeletedAtUtc { get; set; }
}
