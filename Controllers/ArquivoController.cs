using DocumentFormat.OpenXml.ExtendedProperties;
using FeatureLogArquivos.Dto;
using FeatureLogArquivos.Enums;
using FeatureLogArquivos.GerarDados;
using FeatureLogArquivos.Interfaces;
using FeatureLogArquivos.Models;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace FeatureLogArquivos.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ArquivoController : ControllerBase
    {
        public readonly IArquivoService _arquivoService;


        public ArquivoRequestDto ArquivoRequest = new ArquivoRequestDto { 
         Attributes = new List<string> { "FirstName", "LastName", "Email", "Bio" },
          Headers  = new List<string> { "Primeiro nome", "Ultimo Nome", "Email", "Bio", } 
        
        };

        public ArquivoController(IArquivoService arquivoService)
        {
            _arquivoService = arquivoService;
        }





        // GET api/<ArquivoController>/5
        [HttpGet("{id}")]
        public string Get(int linhas = 50)
        {
            List<Customer> customers = new List<Customer>();
            for (int i = 0; i < linhas; i++)
            {
                customers.Add(GerarDadosCustomer.GerarCustomer());
            }



            return "value";
        }
        /// <summary>
        /// Gera um arquivo com opções de filtro
        /// </summary>
        /// <remarks>
        /// - O arquivo pode ser gerado utilizando dos filtros do Sieve ou GrupoId
        /// - **É obrigatorio o envio dos cabeçalhos e as colunas no qual devem ser escritas no arquivo**
        /// - O Arquivo gerado possue todos os dados do filtro,portanto não é aplicado paginação   
        /// - Passar mesmo filtro do Sieve de consulta para popular o Grid da Tela se houver 
        /// </remarks>
        /// <param name="configArquivo">
        ///     Exemplo do body: Headers= ["Placa", "Nome Motorista",... ]  e Attributes= ["placa", "nomeMotorista",... ] ,Attributes igual as propriedades do objeto recebido na consulta de monitoramento
        /// </param>     
        /// <param name="sieveModel">Filtro passado como consulta para popular o Grid da Tela, passar mesmo filtro da tela</param>
        /// <param name="grupoId"></param>
        /// <param name="tipoArquivo"> 1 - CSV, 2 - PDF</param>
        /// <returns></returns>
        [HttpPost]
        [Route("v1/arquivo/{linhas}/{tipoArquivo}")]
       
        [SwaggerOperation(OperationId = "get_v1__arquivo")]
        public async Task<IActionResult> GetArquivo([FromQuery] int linhas =50, [FromQuery] EnumTipoArquivo tipoArquivo = EnumTipoArquivo.CSV)
        {
            //if (tipoArquivo != EnumTipoArquivo.CSV && tipoArquivo != EnumTipoArquivo.PDF)
            //{
            //    configArquivo.AddNotification("TipoArquivo", "Tipo de arquivo inválido");
            //}

            //configArquivo.Validate();
            //if (configArquivo.Invalid)
            //{
            //    return BadRequest(new { Erros = configArquivo.Notifications });
            //}
            //if (string.IsNullOrEmpty(sieveModel.Sorts))
            //{
            //    sieveModel.Sorts = "-data";
            //}



            //Pega informações do usuario
            //var user = GetUserProppertysHelper.GetUsuario(User);


            List<Customer> customers = new List<Customer>();
            for (int i = 0; i < linhas; i++)
            {
                customers.Add(GerarDadosCustomer.GerarCustomer());
            }

            var arquivo = await ObterArquivo( customers,  "Jose", tipoArquivo);
           
            return File(arquivo.Arquivo, arquivo.MimeType, arquivo.NomeArquivo);

        }

        private async Task<ArquivoDto> ObterArquivo(List<Customer> customers,  string usuarioToken, EnumTipoArquivo tipoArquivo = EnumTipoArquivo.CSV)
        {
            ArquivoRequestDto arquivoRequest = ArquivoRequest;
            var arquivo = new ArquivoDto();
            //Setando valores padrões para trazer todos os dados da api de dados
           
            var dtoDadosArquivo = new List<CustomerArquivoDto>();

            foreach (var customer in customers)
            {
                dtoDadosArquivo.Add(new CustomerArquivoDto(customer));
            }
            if (EnumTipoArquivo.CSV == tipoArquivo)
            {
                arquivo = _arquivoService.GerarCsv<CustomerArquivoDto>(dtoDadosArquivo, arquivoRequest.Attributes, arquivoRequest.Headers);
            }
            else if (EnumTipoArquivo.PDF == tipoArquivo)
            {
                var pdfConfig = new PdfDto()
                {
                    Titulo = "Relatório Customer",
                    NomeUsuario = usuarioToken,
                    //Filtros = sieveModel.Filters
                };
                ////Adicionando Filtro externo ao sieve ao objeto do sieve para simplificar formatação
                //if (grupoId != null && grupoId != Guid.Empty)
                //{
                //    pdfConfig.Filtros += ",Grupo = " + (await _veiculoGrupoRepository.ObterVeiculoGrupo((Guid)grupoId))?.Nome;
                //}

                arquivo = _arquivoService.GerarPdf<CustomerArquivoDto>(dtoDadosArquivo, arquivoRequest.Attributes, arquivoRequest.Headers, pdfConfig);
            }
            return arquivo;
        }
    }
}
