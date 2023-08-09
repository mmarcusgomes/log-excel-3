using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FeatureLogArquivos.Models
{
    [Table("Historicos")]
    public class Historico : EntityBase
    {
        public string Log { get; set; }
        public Guid? SolicitacaoMonitoramentoId { get; set; }
        //public Cliente Cliente { get; set; }
        public Guid? ClienteId { get; set; }
    }

    public abstract class EntityBase
    {
        [Key]
        public Guid Id { get; set; }
        public DateTime DataCadastro { get; set; }
        public DateTime DataAtualizacao { get; set; }
        public EntityBase()
        {
            Id = Guid.NewGuid();
        }
    }
}
