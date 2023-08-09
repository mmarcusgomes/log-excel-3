using FeatureLogArquivos.Dto;
using FeatureLogArquivos.Enums;

namespace FeatureLogArquivos.Interfaces
{

    public interface IArquivoService
    {
        /// <summary>
        /// Criar um CSV, colunas igual aos atributos a serem escrito
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dados">Conteudo das linhas do CSV</param>
        /// <param name="colunas">Colunas que serão impressas no CSV(Propriedades vindas do front)</param>
        /// <param name="cabecalhos">Cabeçalhos do CSV</param>
        /// <param name="nomeArquivo">Nome do arquivo, se não for passado sera o DateTime.Now</param>
        /// <param name="delimitador">Delimitador do arquivo, padrão é o ponto e virgula(;)</param>
        /// <returns></returns>
        ArquivoDto GerarCsv<T>(IList<T> dados, IList<string> colunas, IList<string> cabecalhos, string nomeArquivo = null, string delimitador = ";");

        /// <summary>
        /// Gera um arquivo PDF com template padrão de grid <para />
        /// InfoPdf contem Titulo do Pdf,Filtros aplicados, base 64 da logo e da marca D'agua
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dados"></param>
        /// <param name="colunas"></param>
        /// <param name="cabecalhos"></param>
        /// <param name="infosPdf"></param>
        /// <param name="nomeArquivo"></param>
        /// <param name="itensPagina"></param>
        /// <returns></returns>
        ArquivoDto GerarPdf<T>(IList<T> dados, IList<string> colunas, IList<string> cabecalhos, PdfDto infosPdf, string nomeArquivo = null, int itensPagina = 17);
        /// <summary>
        /// Cria um PDF de pagina unica, enviar o html completo da pagina
        /// </summary>
        /// <param name="html"></param>
        /// <param name="infosPdf"></param>
        /// <param name="nomeArquivo"></param>
        /// <returns></returns>
        ArquivoDto GerarPDFPaginaUnica(string html, PdfDto infosPdf, string nomeArquivo = null);

        /// <summary>
        /// Gera um arquivo XLSX. Template passado via paramatro *obrigátorio Description, personalizações extras pelo DTO
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dados"></param>
        /// <param name="colunas"></param>
        /// <param name="cabecalhos"></param>
        /// <param name="templateExcel"></param>
        /// <param name="excelDto"></param>
        /// <param name="nomeArquivo"></param>
        /// <returns></returns>
        ArquivoDto GerarExcel<T>(IList<T> dados, IList<string> colunas, IList<string> cabecalhos, EnumTemplateExcel templateExcel, ExcelDto excelDto, string nomeArquivo = null);


    }
}
