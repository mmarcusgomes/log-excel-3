using ClosedXML.Excel;
using CsvHelper;
using DinkToPdf;
using DinkToPdf.Contracts;
using FeatureLogArquivos.Dto;
using FeatureLogArquivos.Enums;
using FeatureLogArquivos.Extension;
using FeatureLogArquivos.Interfaces;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Data;
using System.Globalization;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace FeatureLogArquivos.Services
{
    public class ArquivoService : IArquivoService
    {
        private IConverter _converter;
        private readonly PdfOptions _pdfOptions;

        public ArquivoService(IConverter converter, IOptions<PdfOptions> pdfOptions)
        {
            _converter = converter;
            _pdfOptions = pdfOptions.Value;
        }

        /// <summary>
        /// Criar um CSV
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dados">Conteudo das linhas do CSV</param>
        /// <param name="colunas">Colunas que serão impressas no CSV(Propriedades vindas do front)</param>
        /// <param name="cabecalhos">Cabeçalhos do CSV</param>
        /// <param name="nomeArquivo">Nome do arquivo, se não for passado sera o Brasilia.DataAtual</param>
        /// <param name="delimitador">Delimitador do arquivo, padrão é o ponto e virgula(;)</param>
        /// <returns></returns>
        public ArquivoDto GerarCsv<T>(IList<T> dados, IList<string> colunas, IList<string> cabecalhos, string nomeArquivo = null, string delimitador = ";")
        {
            if (dados.Count == 0)
            {
                return null;
            }

            ArquivoDto arquivo = new ArquivoDto();
            //Seta como null as colunas que não serão impressas no CSV
            var dadosNulificados = NulificarColunasNaoSelecionadas(dados, colunas);

            ////Remove os campos que são nulls
            var table = RemovePropriedadesNulas(dadosNulificados);

            //Guarda em memoria o arquivo
            using var memory = new MemoryStream();
            using (var writer = new StreamWriter(memory, Encoding.UTF8)) // Criar o arquivo com a codificação para acentuação
            using (CsvWriter csv = new CsvWriter(writer, new CultureInfo("pt-BR")))
            {
                csv.Configuration.Delimiter = delimitador;
                //Escreve as colunas(Cabeçalho)
                for (int i = 0; i < colunas.Count; i++)
                {
                    table.Columns[colunas[i]].SetOrdinal(i);
                    csv.WriteField(cabecalhos[i]);
                }
                csv.NextRecord();
                // Escreve cada linha de cada coluna passando em cada campo
                foreach (DataRow row in table.Rows)
                {
                    for (var i = 0; i < table.Columns.Count; i++)
                    {
                        csv.WriteField(row[i]);
                    }
                    csv.NextRecord();
                }

                writer.Flush();
                memory.Seek(0, SeekOrigin.Begin);
                arquivo.Arquivo = memory.ToArray();
            }

            if (string.IsNullOrEmpty(nomeArquivo))
            {
                arquivo.NomeArquivo = Brasilia.DataAtual.ToShortDateString();
            }
            else
            {
                arquivo.NomeArquivo = nomeArquivo;
            }
            arquivo.NomeArquivo += ".csv";
            arquivo.MimeType = MimeType.CSV;
            return arquivo;
        }

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
        public ArquivoDto GerarPdf<T>(IList<T> dados, IList<string> colunas, IList<string> cabecalhos, PdfDto infosPdf, string nomeArquivo = null, int itensPagina = 17)
        {
            if (dados.Count == 0)
            {
                return null;
            }
            infosPdf.Filtros = FormatarFiltro<T>(infosPdf.Filtros);
            var dataArquivo = Brasilia.DataAtual.ToString("dd/MM/yyyy HH:mm");
            var cabecalhosGrid = string.Empty;
            var listaLinhas = new StringBuilder();
            var paginas = new List<string>();
            ArquivoDto arquivo = new ArquivoDto();
            //Seta como null as colunas que não serão impressas no CSV
            var dadosNulificados = NulificarColunasNaoSelecionadas(dados, colunas);

            //Remove os campos que são nulls
            var table = RemovePropriedadesNulas(dadosNulificados);
            //if (colunas.Count < 7)
            //{
            //    itensPagina = 20;
            //}
            //Template padrão do PDF
            string templateDefault = File.ReadAllText(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @"\Templates\PDF\TemplateGridPadrao.html");
            //Template do Header
            string header = File.ReadAllText(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @"\Templates\PDF\HeaderGrid.html");
            //Armazenando o template para uso posterior em outras paginas onde o header da pagina deve replicar
            var templateCapa = templateDefault;

            //Configura itens por pagina de acordo com a quantidade de colunas
            itensPagina = ConfiguraItensPorPagina(colunas, itensPagina);

            // Escreve as colunas(Cabeçalho)
            for (int i = 0; i < colunas.Count; i++)
            {
                table.Columns[colunas[i]].SetOrdinal(i);
                cabecalhosGrid += $"<th>{cabecalhos[i]}</th>";
            }
            var contador = 1;
            var paginaMenor = false; //Sinaliza se a pagina é menos dos que as outras
            foreach (DataRow row in table.Rows)
            {
                if (contador < itensPagina)
                {
                    //Cria a row do grid
                    listaLinhas.Append(CriaRow(table, row));
                    paginaMenor = false;
                }
                //Caso cheque no limite da pagina cria a proxima pagina com mesmo header
                else if (contador >= itensPagina)
                {
                    var estilo = "style=\"height: 768px;\"";
                    listaLinhas.Append(CriaRow(table, row));
                    templateCapa = ReplaceVariaveisHtml(templateCapa, dataArquivo, header, listaLinhas.ToString(),
                                                      cabecalhosGrid, infosPdf?.LogoMarca,
                                                     infosPdf?.Filtros,
                                                      infosPdf?.Titulo, estilo);
                    paginas.Add(templateCapa);
                    contador = 0;
                    listaLinhas.Clear();
                    //Cria uma nova pagina, necessario para replicar o cabeçalho da pagina
                    templateCapa = templateDefault;
                    paginaMenor = true;
                }
                contador++;
            }
            //Caso os itens sejam menores o valor completo de uma pagina adiciona esses extras .Exemplo: 10 itens por pagina mas so tem 6
            if (contador > 0 && paginaMenor == false && contador <= itensPagina)
            {
                templateCapa = ReplaceVariaveisHtml(templateDefault, dataArquivo, header, listaLinhas.ToString(), cabecalhosGrid, infosPdf?.LogoMarca,
                                                         infosPdf?.Filtros,
                                                        infosPdf?.Titulo);
                paginas.Add(templateCapa);
            }

            var doc = new HtmlToPdfDocument()
            {
                GlobalSettings = {
                      PaperSize = PaperKind.A4,
                      Orientation = Orientation.Landscape,
                      Margins = new MarginSettings { Top = 10, Bottom = 10, Left = 10, Right = 10 },
                   },
            };
            //Cria as paginas
            foreach (var item in paginas)
            {
                var page = new ObjectSettings()
                {
                    PagesCount = true,
                    WebSettings = { DefaultEncoding = "utf-8" },
                    HtmlContent = item,
                    HeaderSettings = { FontSize = 9, Line = true, Spacing = 2.812 },
                    FooterSettings ={Center=$"Relatório extraído pelo sistema  {(string.IsNullOrWhiteSpace(infosPdf.NomeUsuario) ? "" : "pelo usuário " + infosPdf.NomeUsuario)}",
                        Right = "Página [page] de [toPage]", Line = true, Spacing = 5.812 }
                };
                doc.Objects.Add(page);
            }
            arquivo.Arquivo = _converter.Convert(doc);
            if (string.IsNullOrEmpty(arquivo.NomeArquivo))
            {
                arquivo.NomeArquivo = Brasilia.DataAtual.ToShortDateString() + ".pdf";
            }
            else
            {
                arquivo.NomeArquivo += ".pdf";
            }
            arquivo.MimeType = MimeType.PDF;
            return arquivo;
        }

        /// <summary>
        /// Cria um PDF de pagina unica, enviar o html completo da pagina
        /// </summary>
        /// <param name="html"></param>
        /// <param name="infosPdf"></param>
        /// <param name="nomeArquivo"></param>
        /// <returns></returns>
        public ArquivoDto GerarPDFPaginaUnica(string html, PdfDto infosPdf, string nomeArquivo = null)
        {
            ArquivoDto arquivo = new ArquivoDto();
            if (!string.IsNullOrWhiteSpace(infosPdf.Titulo))
            {
                html = html.Replace("{{titulo}}", infosPdf.Titulo);
            }
            var doc = new HtmlToPdfDocument()
            {
                GlobalSettings = {
                      PaperSize = PaperKind.A4,
                      Orientation = Orientation.Portrait,
                      Margins = new MarginSettings { Top = 10, Bottom = 10, Left = 10, Right = 10 },
                   },
            };
            var page = new ObjectSettings()
            {
                PagesCount = true,
                WebSettings = { DefaultEncoding = "utf-8" },
                HtmlContent = html,
                HeaderSettings = { FontSize = 7, Line = false, Spacing = 2.812 },
                FooterSettings ={Center=$"Relatório extraído pelo sistema CELIG pelo usuário {infosPdf.NomeUsuario} em {Brasilia.DataAtual:dd/MM/yyyy HH:mm}",
                         Line = false, Spacing = 2.812, FontSize = 9 }
            };
            doc.Objects.Add(page);
            arquivo.Arquivo = _converter.Convert(doc);
            if (string.IsNullOrEmpty(arquivo.NomeArquivo))
            {
                arquivo.NomeArquivo = Brasilia.DataAtual.ToShortDateString() + ".pdf";
            }
            else
            {
                arquivo.NomeArquivo += ".pdf";
            }
            arquivo.MimeType = MimeType.PDF;
            return arquivo;
        }

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
        public ArquivoDto GerarExcel<T>(IList<T> dados, IList<string> colunas, IList<string> cabecalhos, EnumTemplateExcel templateExcel, ExcelDto excelDto, string nomeArquivo = null)
        {
            if (dados.Count == 0)
            {
                return null;
            }
            var template = templateExcel.GetEnumDescription();
            if (!template.EndsWith(".xlsx"))
            {
                template += ".xlsx";
            }
            if (string.IsNullOrWhiteSpace(excelDto.Celula))
            {
                excelDto.Celula = "A1";
            }
            var dadosNulificados = NulificarColunasNaoSelecionadas(dados, colunas);
            //Remove os campos que são nulls
            var table = RemovePropriedadesNulas(dadosNulificados);

            ArquivoDto arquivo = new ArquivoDto();
            //Usado para buscar os templatse e salvar temporariamente os arquivos enquanto gerados
            string caminhoGeral = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @"\Templates\Excel\";
            string destino = caminhoGeral + DateTime.Now.Ticks + ".xlsx";

            string templateModelo = caminhoGeral + template;

            //FileStream file = new FileStream(path, FileMode.OpenOrCreate);
            //Copia o arquivo gerado temporariamente para a memoria
            File.Copy(templateModelo, destino);

            using (var workbook = new XLWorkbook(destino))
            {
                //Pega a planilha existente no arquivo excel
                IXLWorksheet worksheet = workbook.Worksheets.FirstOrDefault();

                // Escreve as colunas(Cabeçalho)
                for (int i = 0; i < colunas.Count; i++)
                {
                    table.Columns[colunas[i]].SetOrdinal(i);
                    table.Columns[colunas[i]].ColumnName = cabecalhos[i];
                }
                //Table com formatação inserida por padrao
                if (cabecalhos == null || cabecalhos.Count == 0)
                {
                    //Se não for passado os cabeçalhos, vai manter o do template
                    worksheet.Cell(excelDto.Celula).InsertData(table.Rows);
                }
                else
                {
                    //Se for passado cabelçalhos eles serão inseridos
                    worksheet.Cell(excelDto.Celula).InsertTable(table);
                    var tabelaExcel = worksheet.Tables.FirstOrDefault();
                    //A table automaticamente recebe o nome de Table1, entao remova os filtros automaticos da table existente
                    //var tabelaExcel = worksheet.Table("Table1");
                    //Tema cinza zebrado, cabeçalho é preto por isso é sobreescrito em azul
                    tabelaExcel.Theme = XLTableTheme.TableStyleMedium1;
                    //Coloca bordas em todas as celulas usadas
                    tabelaExcel.CellsUsed().Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                    //Recupera o range do cabeçalho
                    var cabecalhoTable = tabelaExcel.FirstRowUsed().RangeAddress;
                    worksheet.Range(cabecalhoTable).Style.Fill.BackgroundColor = XLColor.FromHtml("#baf6ff"); // azul padrão de excel para table
                    worksheet.Range(cabecalhoTable).Style.Border.OutsideBorder = XLBorderStyleValues.Medium;
                    tabelaExcel.ShowAutoFilter = false;
                }
                if (excelDto.DadosExtras != null && excelDto.DadosExtras.Count > 0)
                {
                    //Para a primeira pagina do excel adiciona os dados avulsos em suas celulas especificadas no dictionary
                    var ws = workbook.Worksheet(1);
                    foreach (var celula in excelDto.DadosExtras)
                    {
                        //Escreve o conteudo extra na celula e com o valor passado
                        ws.Cell(celula.Key).Value = celula.Value;
                    }
                }

                if (string.IsNullOrWhiteSpace(nomeArquivo))
                {
                    arquivo.NomeArquivo = Brasilia.DataAtual.ToShortDateString();
                }
                else
                {
                    arquivo.NomeArquivo = nomeArquivo;
                }
                arquivo.NomeArquivo += ".xlsx";
                arquivo.MimeType = MimeType.XLSX;
                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    arquivo.Arquivo = stream.ToArray();
                    workbook.Dispose();
                }
            }
            //file.Close();
            //file.Dispose();
            ExcluirArquivo(destino);
            return arquivo;
        }

        #region Metodos privados para auxilio do serviço de arquivos

        public static void ExcluirArquivo(string caminhoAbsoluto)
        {
            if (File.Exists(caminhoAbsoluto))
            {
                File.Delete(caminhoAbsoluto);
            }
        }

        private int ConfiguraItensPorPagina(IList<string> colunas, int itensPagina)
        {
            var totalColunas = colunas.Count();

            if (totalColunas > 10)
            {
                return 15;
            }

            return itensPagina;
        }

        /// <summary>
        /// Substitui as variaveis pelas informações passadas
        /// </summary>
        /// <param name="template"></param>
        /// <param name="header"></param>
        /// <param name="body"></param>
        /// <param name="cabecalhos"></param>
        /// <returns></returns>
        private string ReplaceVariaveisHtml(string template, string dataArquivo, string header = null,
            string body = null, string cabecalhos = null, string logoMarca = null, string filtro = "", string titulo = null, string estilo = "")
        {
            //Header da pagina
            if (!string.IsNullOrEmpty(header))
            {
                template = template.Replace("{{header}}", header);
            }
            //Cabeçalhos do grid
            if (!string.IsNullOrEmpty(cabecalhos))
            {
                template = template.Replace("{{cabecalhos}}", cabecalhos);
            }
            //Itens do grid
            if (!string.IsNullOrEmpty(body))
            {
                template = template.Replace("{{body}}", body);
            }
            if (!string.IsNullOrEmpty(logoMarca))
            {
                template = template.Replace("{{logomarca}}", logoMarca);
            }
            else
            {
                template = template.Replace("{{logomarca}}", _pdfOptions.LogoMarca);
            }
            template = template.Replace("{{dataArquivo}}", dataArquivo);
            if (!string.IsNullOrEmpty(titulo))
            {
                template = template.Replace("{{titulo}}", titulo);
            }
            template = template.Replace("{{marcadagua}}", _pdfOptions.MarcaDagua);
            template = template.Replace("{{filtro}}", filtro);
            template = template.Replace("{{styletable}}", estilo);
            return template;
        }

        /// <summary>
        /// Cria uma row da TableHTML
        /// </summary>
        /// <param name="table"></param>
        /// <param name="row"></param>
        /// <returns></returns>
        private string CriaRow(DataTable table, DataRow row)
        {
            var body = string.Empty;
            body += "<tr>";

            for (var i = 0; i < table.Columns.Count; i++)
            {
                body += $"<td>{row[i]}</td>";
            }
            body += "</tr>";
            return body;
        }

        private IList<T> NulificarColunasNaoSelecionadas<T>(IList<T> dados, IList<string> colunas)
        {
            //Pega as propriedades do objeto para mapeamento
            var propriedades = typeof(T).GetProperties().Select(x => x.Name).ToList();

            //Colunas e propriedade são iguais, abaixo anula determinadas propriedades
            foreach (var item in dados)
            {
                foreach (var propriedade in propriedades)
                {
                    PropertyInfo property = item.GetType().GetProperty(propriedade);
                    if (colunas.Any(coluna => coluna.ToLower() == property.Name.ToLower()))
                    {
                        if (property.GetValue(item) == null)
                        {
                            Type tipo = property.PropertyType;
                            //Verificando se a propriedade é do tipo int ou double para pelo menos ser mostrado a coluna e o valor default
                            if (tipo == typeof(int) || tipo == typeof(double) || tipo == typeof(decimal))
                            {
                                property.SetValue(item, 0);
                            }
                            //Verificando se a propriedade é do tipo string ou char e colocando um valor padrão para ser mostrado a coluna
                            else if (tipo == typeof(string) || tipo == typeof(char))
                            {
                                property.SetValue(item, "");
                            }
                            else if (tipo == typeof(bool))
                            {
                                property.SetValue(item, "");
                            }
                            else if (tipo == typeof(DateTime))
                            {
                                property.SetValue(item, "");
                            }
                        }
                    }
                    //Se a propriedade não esta mapeada no grid, setar null para remove-la
                    else
                    {
                        //Marca a propriedade que não deve ser impressa como null para futura remoção
                        property.SetValue(item, null);
                    }
                }
            }
            return dados;
        }

        /// <summary>
        /// Remove as propriedades nulas e devolve uma datatable
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dadosNulificados"></param>
        /// <returns></returns>
        private DataTable RemovePropriedadesNulas<T>(IList<T> dadosNulificados)
        {
            //Separado em outro metodo caso necessite de alguma outra regra ou lib para remoção dos nulls
            return JsonConvert.DeserializeObject<DataTable>(JsonConvert.SerializeObject(dadosNulificados, Newtonsoft.Json.Formatting.None,
                  new JsonSerializerSettings
                  {
                      NullValueHandling = NullValueHandling.Ignore
                  }));
        }

        /// <summary>
        /// Formata os filtros do PDF
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="filtros"></param>
        /// <returns></returns>
        private string FormatarFiltro<T>(string filtros)
        {
            //Caracter usado para logica de formatação, ao troca aqui tambem atualizar no regex de MatchCollection
            string split = "#";
            var filtroFinal = "";
            if (!string.IsNullOrEmpty(filtros))
            {
                var listaSemRepeticao = new List<string>();
                filtros = filtros.Replace(",", split);
                //Regex para selecionar os operadores do sieve
                Regex regex = new Regex(@"(>=|<=|==|>|<|@=|_=|!@=|!_=|@=*|_=*|==*|!@=*|!_=*|=)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
                //Substitui os operadores e cria uma lista de filtros passados
                var operadoresRemovidos = split + regex.Replace(filtros, "=").Replace("*","");

                MatchCollection matchList = Regex.Matches(operadoresRemovidos, @"(?<=\#)(.*?)(?=\=)");
                //Lista as chaves dos filtros
                var listaChaves = matchList.Cast<Match>().Select(match => match.Value).ToList();
                var filtrosRepetidos = listaChaves.GroupBy(x => x)
               .Where(g => g.Count() > 1)
               .Select(g => g.Key)
               .ToList();

                //Adicionando os filtros que não repetem
                listaSemRepeticao.AddRange(operadoresRemovidos.Split(split).Where(filtro => !filtrosRepetidos.Any(x => filtro.Contains(x))).ToList());
                foreach (var filtroRepetido in filtrosRepetidos)
                {
                    var listaFiltros = regex.Replace(filtros, "=").Split(split).ToList();

                    //Filtros repetidos
                    var listaRepetida = listaFiltros.Where(x => x.Contains(filtroRepetido)).ToList();
                    var stringRepetida = (string.Join(split, listaRepetida)) + split;

                    //Recupera os valores do filtros repetidos
                    var regexRepetidos = new Regex(@"(?<=\=)(.*?)(?=\#)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
                    var valoresRepetidos = regexRepetidos.Matches(stringRepetida).ToList();
                    //Concatena os valores repetidos
                    var valores = String.Join(" - ", valoresRepetidos);
                    var filtroUnico = filtroRepetido + "=" + valores;
                    listaSemRepeticao.Add(filtroUnico);
                }

                foreach (var filter in listaSemRepeticao)
                {
                    if (!string.IsNullOrWhiteSpace(filter))
                    {
                        var filtro = split + filter;
                        //Regex para selecionar a chave
                        regex = new Regex(@"(?<=\#)(.*?)(?=\=)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
                        var parametro = regex.Match(filtro).Value;

                        Regex regexValues = new Regex(@"(?<=\=)(.*?)(?=\#)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
                        var filterValues = filter + split;
                        var valor = regexValues.Match(filterValues).Value;

                        Type tipo = typeof(T);
                        //Procura por determinado metodo na classe de arquivo
                        var objectCurrent = tipo.GetMethod("FormataFiltroArquivo");
                        object[] parametros = new object[] { parametro, valor };
                        var valorFormatado = new object();
                        if (objectCurrent != null)
                        {
                            //Cria uma instacia do objeto generico para uso dos metodos internos
                            var novoObjetoGenerico = (T)Activator.CreateInstance(typeof(T));
                            //Invoca determinado metodo , passando o novo objeto e os parametro do metodo, nos quais devem ser padrões nas DTOs de arquivo
                            valorFormatado = objectCurrent != null ? objectCurrent.Invoke(novoObjetoGenerico, parametros) : valor;
                        }
                        else
                        {
                            valorFormatado = valor;
                        }
                        //Retorna o nome da propriedade da class que é igual a propriedade do filtro
                        var description = Util.GetDescriptionPropertyClass<T>(parametro);
                        //Caso nao existe um parametro com o mesmo nome da propriedade ele mantem o formato vindo do filters do sieve
                        if (string.IsNullOrEmpty(description))
                        {
                            filtroFinal += ", " + parametro + ": " + valorFormatado;
                        }
                        //Caso contrario ele vai escrever a description pois é mais amigavel
                        else
                        {
                            filtroFinal += ", " + description + ": " + valorFormatado;
                        }
                    }
                }
            }

            filtroFinal = TrataCabecalhoFiltroPdf(filtroFinal);
            return filtroFinal;
        }

        private static string TrataCabecalhoFiltroPdf(string filtroFinal)
        {
            if (filtroFinal.StartsWith(","))
            {
                filtroFinal = filtroFinal.Remove(0, 1);
            }

            if (filtroFinal.Length > 100)
            {
                filtroFinal = filtroFinal.Replace(",", ",<br/>");
            }

            return filtroFinal.Replace("~~", ",");
        }

        #endregion Metodos privados para auxilio do serviço de arquivos
    }
}