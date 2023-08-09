using Bogus;
using FeatureLogArquivos.Models;

namespace FeatureLogArquivos.GerarDados
{
    public static class GerarDadosCustomer
    {
        public static Customer GerarCustomer()
        {
            var address = new Faker<Address>()
                   .RuleFor(a => a.Line1, f => f.Address.BuildingNumber())
                   .RuleFor(a => a.Line2, f => f.Address.StreetAddress())
                   .RuleFor(a => a.PinCode, f => f.Address.ZipCode())
                 ;
            var customer = new Faker<Customer>()
                    .RuleFor(c => c.Id, f => f.Random.Number(1, 100))
                    .RuleFor(c => c.FirstName, f => f.Person.FirstName)
                    .RuleFor(c => c.LastName, f => f.Person.LastName)
                    .RuleFor(c => c.Email, f => f.Person.Email)
                    .RuleFor(c => c.Bio, f => f.Lorem.Paragraph(1))
                    .RuleFor(c => c.Address, f => address.Generate())
                ;
            return customer.Generate();


        }
        public enum Gender
        {
            Male,
            Female
        }



    }
}
