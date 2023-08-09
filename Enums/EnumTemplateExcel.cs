using System.ComponentModel;

namespace FeatureLogArquivos.Enums
{
    //Adicionar na decription o nome do template, pela description que será carregado o template
    public enum EnumTemplateExcel
    {
        [Description("ExtratoPMA")]
        ExtratoPMA = 1,
        [Description("ExtratoCelig")]
        ExtratoCelig = 2,
        [Description("Vazio")]
        Vazio = 3
    }
}
