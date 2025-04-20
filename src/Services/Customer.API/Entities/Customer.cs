using Contracts.Domains;
using System.ComponentModel.DataAnnotations;

namespace Customer.API.Entities
{
    public class Customer : EntityBase<int>
    {
        public string UserName { get; set; }
        public string Email { get; set; }

        [StringLength(250)]
        public string FirstName { get; set; }

        [StringLength(250)]
        public string LastName { get; set; }
    }
}