using System.ComponentModel.DataAnnotations;
using System.Diagnostics.Contracts;

namespace FeatureLogArquivos.Dto
{
    public class ArquivoRequestDto //: Notifiable, IValidatable
    {
        [Required]
        public IList<string> Headers { get; set; }
        [Required]
        public IList<string> Attributes { get; set; }

        //public void Validate()
        //{
        //    AddNotifications(new Contract()
        //        .IsTrue(Headers.Any(), "Headers", "É obrigatório pelo menos um header.")
        //        .IsTrue(Attributes.Any(), "Attributes", "É obrigatório pelo menos um attributes.")
        //    );
        //}
    }
}
