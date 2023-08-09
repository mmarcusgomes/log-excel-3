using FeatureLogArquivos.Models;
using System.ComponentModel;

namespace FeatureLogArquivos.Dto
{
    public class CustomerArquivoDto
    {
        public string Id { get; set; }
        [Description("Primeiro Nome")]
        public string FirstName { get; set; }
        [Description("Ultimo Nome")]
        public string LastName { get; set; }
        public string Email { get; set; }
        [Description("Biografia")]
        public string Bio { get; set; }

        //converte tudo pra string
        public CustomerArquivoDto(Customer customer)
        {
            Id = customer.Id.ToString();
            FirstName = customer.FirstName;
            LastName = customer.LastName;
            Bio = customer.Bio;
            Email = customer.Email;
        }
    }
}
