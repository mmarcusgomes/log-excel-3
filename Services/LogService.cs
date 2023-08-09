using FeatureLogArquivos.Enums;
using FeatureLogArquivos.Interfaces;

namespace FeatureLogArquivos.Services
{
    public class LogService : ILogService
    {
        private readonly ILogRepository _logRepository;
        //private readonly IUserHttpContextService _userHttp;
        private readonly IHttpContextAccessor _httpContextAcessor;

        public LogService(ILogRepository logRepository, /*IUserHttpContextService userHttp, */IHttpContextAccessor httpContextAcessor)
        {
            _logRepository = logRepository;
            //_userHttp = userHttp;
            _httpContextAcessor = httpContextAcessor;
        }

        /// <summary>
        /// Envia o objeto de log para ser trackeado(necessario apenas com AsNoTracking)
        /// Id da tabela de referência1 para o log, e se é uma atualização ou nao
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <param name="idReferencia"></param>
        /// <param name="atualizar"></param>
        public void LogChanges<T>(Guid idReferencia, object obj = null, EnumOperacaoHistorico operacao = EnumOperacaoHistorico.Atualizar)
        {
            _logRepository.LogUpdateAoContext<T>(/*_userHttp.Name,*/ "Jose", idReferencia, obj, operacao);
        }

        public void SalvaLog(string propriedadeChaveEstrangeira, Guid chaveEstrangeira)
        {
            _logRepository.SalvaLog(propriedadeChaveEstrangeira, chaveEstrangeira);
        }
    }
}
