using Contracts.Domains;
using System.ComponentModel.DataAnnotations;

namespace Customer.API.Entities
{
    public class Customer:EntityBase<int>
    {
        public required string UserName { get; set; }
        public required string Email { get; set; }
        [StringLength(250)]
        public required string FirstName { get; set; }
        [StringLength(250)]

        public required string LastName { get; set; }
    }
}
