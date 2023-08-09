namespace FeatureLogArquivos.Dto
{
    public class ExcelDto
    {
        public ExcelDto()
        {
            DadosExtras = new Dictionary<string, string>();
        }
        //Celula na qual tera inicio a inserção dos dados 
        public string Celula { get; set; }

        //Outras células que precisam ser escritas, enviar célula e valor para ser escrito 
        public Dictionary<string, string> DadosExtras { get; set; }

        //Nome da planilha onde o template se encontra ou WorkSheet
        //public string NomePlanilha { get; set; }
    }
}
