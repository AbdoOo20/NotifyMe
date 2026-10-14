namespace NotifyMe.Core.Entities
{
    public class BaseEntity<TKey>
    {
        public virtual TKey Id { get; set; }

        public DateTime CreateAt { get; set; } = DateTime.UtcNow;
    }
}