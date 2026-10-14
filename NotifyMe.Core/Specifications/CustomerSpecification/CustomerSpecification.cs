using NotifyMe.Core.Entities;

namespace NotifyMe.Core.Specifications.CustomerSpecification
{
    public class CustomerSpecification : BaseSpecification<Customer, string>
    {
        public CustomerSpecification() : base()
        {
        }

        public CustomerSpecification(bool showData) : base()
        {
            Includes.Add(c => c.IdentityUser!);
        }

        public CustomerSpecification(string CustomerId) : base(c => c.Id == CustomerId)
        {
            Includes.Add(c => c.IdentityUser!);
        }
    }
}
