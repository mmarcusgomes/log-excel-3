using System.ComponentModel.DataAnnotations.Schema;
using static Bogus.DataSets.Name;

namespace FeatureLogArquivos.Models
{
    public class User
    {
        //public User(string id,string field)
        //{
        //    Id = id;
        //    Field = field;
        //}
        public string Id { get; set; }
        public string Field { get; set; }
        public Gender FirstName { get; set; }
        public Gender LastName { get; set; }
        public string Avatar { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string Gender { get; set; }
        public string CartId { get; set; }
    }

    [Table("Customer")]
    public class Customer
    {
        public int Id { get; set; }
        [Column(TypeName = "VARCHAR(50)")]
        public string FirstName { get; set; }
        [Column(TypeName = "VARCHAR(50)")]
        public string LastName { get; set; }
        [Column(TypeName = "VARCHAR(50)")]
        public string Email { get; set; }
        public string Bio { get; set; }
        public Address Address { get; set; }
    }
    public class Address
    {
        public string Line1 { get; set; }
        public string Line2 { get; set; }
        public string PinCode { get; set; }
    }
}
