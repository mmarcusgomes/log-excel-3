using FeatureLogArquivos.Enums;

namespace FeatureLogArquivos.Interfaces
{
    public interface ILogRepository
    {
        /// <summary>
        /// Envia o objeto de log para ser trackeado(Apenas com o uso de AsNoTracking),nome do usuário para o log
        /// Id de referencia para o log, e se é uma atualização ou nao
        /// </summary>
        /// <param name="nomeUsuario"></param>
        /// <param name="idReferencia"></param>
        /// <param name="obj"></param>
        /// <param name="atualizar"></param>
        void LogUpdateAoContext<T>(string nomeUsuario, Guid idReferencia, object obj = null, EnumOperacaoHistorico operacao = EnumOperacaoHistorico.Atualizar);
        void SalvaLog(string propriedadeChaveEstrangeira, Guid chaveEstrangeira);
    }
}
