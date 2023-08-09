namespace FeatureLogArquivos.Dto
{
    public class PdfDto
    {
        // base 64 Marca D'agua para colocar na table 
        //public string MarcaDAgua { get; set; } // appsettings 
        //base 64 da logo marca da empresa
        public string LogoMarca { get; set; }
        //Titulo do documento
        public string Titulo { get; set; }
        //Filtros ja convertidos para escrita no pdf
        public string Filtros { get; set; }
        //Nome do usuario logado
        public string NomeUsuario { get; set; }
    }
}
