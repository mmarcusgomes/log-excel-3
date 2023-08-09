using FeatureLogArquivos.Enums;

namespace FeatureLogArquivos.Interfaces
{
    public interface ILogService
    {
        /// <summary>
        /// Envia o objeto de log para ser trackeado(necessario apenas com AsNoTracking)
        /// Id da tabela de referência1 para o log, e se é uma atualização ou nao
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="idReferencia"></param>
        /// <param name="obj"></param>
        /// <param name="atualizar"></param>
        void LogChanges<T>(Guid idReferencia, object obj = null, EnumOperacaoHistorico operacao = EnumOperacaoHistorico.Atualizar);
        void SalvaLog(string propriedadeChaveEstrangeira, Guid chaveEstrangeira);
    }
}
