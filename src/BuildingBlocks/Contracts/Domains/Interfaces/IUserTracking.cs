namespace Contracts.Domains.Interfaces
{
    public interface IUserTracking
    {
        public Guid? CreatedBy { get; set; }
        public Guid? UpdatedBy { get; set; }
    }
}