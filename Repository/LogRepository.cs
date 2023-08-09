using FeatureLogArquivos.Enums;
using FeatureLogArquivos.Extension;
using FeatureLogArquivos.Interfaces;
using FeatureLogArquivos.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel;
using System.Reflection;

namespace FeatureLogArquivos.Repository
{
    public class LogRepository : ILogRepository
    {
        private readonly AplicacaoContext _contexto;
        public IConfiguration Configuration { get; }
        public string MensagemLog { get; set; }

        public LogRepository(AplicacaoContext contexto, IConfiguration configuration)
        {
            _contexto = contexto;
            Configuration = configuration;
        }

        /// <summary>
        /// Envia o objeto de log para ser trackeado(Apenas com o uso de AsNoTracking),nome do usuário para o log
        /// Id de referencia para o log, e se é uma atualização ou nao
        /// </summary>
        /// <param name="nomeUsuario"></param>
        /// <param name="idReferencia"></param>
        /// <param name="obj"></param>
        /// <param name="atualizar"></param>
        public void LogUpdateAoContext<T>(string nomeUsuario, Guid idReferencia, object originalObj = null, EnumOperacaoHistorico operacao = EnumOperacaoHistorico.Atualizar)
        {
            var inst = originalObj.GetType().GetMethod("MemberwiseClone", BindingFlags.Instance | BindingFlags.NonPublic);
            var obj = (T)inst?.Invoke(originalObj, null);

            //Validar uso e viabilidade
            if (obj != null)
            {
                if (operacao == EnumOperacaoHistorico.Atualizar)
                {
                    //Mapea entidades desconectadas para o contexto 
                    _contexto.ChangeTracker.TrackGraph(obj, e =>
                    {
                        if (e.Entry.IsKeySet)
                        {
                            e.Entry.State = EntityState.Modified;
                        }
                    });
                }
            }

            var tipoObj = obj.GetType();
            var nomeClasse = tipoObj.GetCustomAttributes(typeof(DescriptionAttribute), false).SingleOrDefault() as DescriptionAttribute;

            if (operacao == EnumOperacaoHistorico.Adicionar)
            {
                MensagemLog += $"{Brasilia.DataAtual:dd/MM/yyyy HH:mm:ss} - {nomeClasse.Description ?? tipoObj.Name} foi adicionado pelo usuário {nomeUsuario}.~";
            }
            else if (operacao == EnumOperacaoHistorico.Remover)
            {
                MensagemLog += $"{Brasilia.DataAtual:dd/MM/yyyy HH:mm:ss} - {nomeClasse?.Description ?? tipoObj.Name } foi deletado pelo usuário {nomeUsuario}.~";
            }
            else
            {
                //Itera sobre todas as entidades trackeadas
                foreach (var entidade in _contexto.ChangeTracker.Entries())
                {
                    //Recupera o tipo de entidade que esta sendo iterada no momento
                    var tipo = entidade.Entity.GetType();

                    //Recupera o valor da entidade corrente do banco para validação de alterações,
                    // tem que usar esse metodo para entidades desconectadas pois não seria possivel pegar do proprio tracher do EF
                    var valoresBanco = entidade.GetDatabaseValues();
                    var nomeCampo = tipo.GetCustomAttributes(typeof(DescriptionAttribute), false).SingleOrDefault() as DescriptionAttribute;

                    //Itera sobre todas as propriedades
                    foreach (var property in entidade.OriginalValues.Properties)
                    {
                        var nomePropriedade = "";
                        //valor da propriedade do objeto modificado 
                        var valorCorrente = entidade.CurrentValues[property.Name]?.ToString();
                        //valor do objeto vindo do banco
                        var valorAtual = valoresBanco?[property.Name]?.ToString();

                        if (valorAtual != null && (property.ClrType == typeof(decimal) || property.ClrType == typeof(float) || property.ClrType == typeof(double)))
                        {
                            var valorReplaced = valorAtual.Replace(valorCorrente, "");

                            if (valorReplaced != ".00" && valorReplaced != ",00" && valorAtual != valorCorrente)
                            {
                                MensagemLog += $"{Brasilia.DataAtual:dd/MM/yyyy HH:mm:ss} - Campo {nomePropriedade ?? property.Name } com valor {valorAtual} atualizado para {valorCorrente} pelo usuário {nomeUsuario}.~";
                            }
                        }
                        else if (valorAtual != null && !valorAtual.Equals(valorCorrente) && property.Name != "DataCadastro" && property.Name != "DataAtualizacao")
                        {
                            //Verifica se a propriedade é um tipo de enum para formatação especifica
                            if (property.ClrType.IsEnum)
                            {
                                //Recupera o valor da propriedade q estava salva no banco
                                var atributoEnumOld = valoresBanco[property.Name]?.ToString();
                                //Recupera o valor da propriedade ja modificada
                                var atributoEnumNovo = entidade.CurrentValues[property.Name]?.ToString();

                                //Recupera a descrição dos enums para formatar melhor o log
                                valorAtual = EnumExtension.GetDescriptionType(property.ClrType, atributoEnumOld);
                                valorCorrente = EnumExtension.GetDescriptionType(property.ClrType, atributoEnumNovo);
                            }
                            //Para mudança de chaves estrangeiras
                            else if (property.Name.ToLower().EndsWith("id"))
                            {
                                var atributos = GetAttributes(tipo, property.Name);
                                if (!string.IsNullOrEmpty(valorAtual) && Guid.Parse(valorAtual) != Guid.Empty)
                                {
                                    //Recupera o valor da tabela
                                    valorAtual = RecuperaValores(atributos, valorAtual);
                                }
                                else
                                {
                                    valorAtual = "";
                                }
                                if (!string.IsNullOrEmpty(valorCorrente) && Guid.Parse(valorCorrente) != Guid.Empty)
                                {
                                    //Recupera o calor da tabela e da coluna principal determinada no LogAttribute
                                    valorCorrente = RecuperaValores(atributos, valorCorrente);
                                }
                                else
                                {
                                    valorCorrente = "";
                                }
                            }
                            //Retorna o nome da propriedade da class que é igual a propriedade do filtro
                            var description = Util.GetDescriptionPropertyClass<T>(property.Name);
                            //Caso nao existe um parametro com o mesmo nome da propriedade ele mantem o nome da propriedade
                            if (string.IsNullOrEmpty(description))
                            {
                                nomePropriedade = property.Name;
                            }
                            //Caso contrario ele vai escrever a description pois é mais amigavel 
                            else
                            {
                                nomePropriedade = description;
                            }

                            if (!string.IsNullOrEmpty(valorCorrente))
                            {
                                MensagemLog += $"{Brasilia.DataAtual:dd/MM/yyyy HH:mm:ss} - Campo {nomePropriedade} com valor {valorAtual} atualizado para {valorCorrente} pelo usuário {nomeUsuario}.~";
                            }
                        }
                    }
                }
            }

            // Método desanexa todas entidades para evitar erro na modificação de listas
            DetachAll();
        }

        public void SalvaLog(string propriedadeChaveEstrangeira, Guid chaveEstrangeira)
        {
            if (!string.IsNullOrEmpty(MensagemLog))
            {
                // Remove o último delimitador entre as mensagens
                MensagemLog = MensagemLog.Remove(MensagemLog.Length - 1, 1);

                var historicoLog = new Historico()
                {
                    Log = MensagemLog,
                    DataCadastro = Brasilia.DataAtual,
                    DataAtualizacao = Brasilia.DataAtual
                };

                var propriedade = historicoLog.GetType().GetProperty(propriedadeChaveEstrangeira);

                if (propriedade != null)
                {
                    var tipoPropriedade = Nullable.GetUnderlyingType(propriedade.PropertyType) ?? propriedade.PropertyType;

                    var valorConvertido = Convert.ChangeType(chaveEstrangeira, tipoPropriedade);

                    propriedade.SetValue(historicoLog, valorConvertido, null);
                }

                _contexto.Historicos.Add(historicoLog);
                _contexto.SaveChanges();

                MensagemLog = "";
            }
        }

        /// <summary>
        /// Recupera os valores inseridos no LogAttribute (Atributo personalizado)
        /// </summary>
        /// <param name="tipoObjeto"></param>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public AuxiliarLog GetAttributes(Type tipoObjeto, string propertyName)
        {
            //Recupera o nome da propriedade caso exista no filtro 
            var props1 = tipoObjeto.GetProperties().Where(prop => prop.Name.ToLower() == propertyName.Trim().ToLower()).FirstOrDefault();
            object[] attrs = props1.GetCustomAttributes(true);
            var resp = new AuxiliarLog();
            foreach (System.Attribute attr in attrs)
            {
                if (attr is LogAttribute)
                {

                    LogAttribute a = (LogAttribute)attr;
                    resp.Tabela = a.tabela;
                    resp.Coluna = a.coluna;
                    resp.ColunaBackup = a.colunaBackup;
                }
            }
            return resp;
        }

        /// <summary>
        /// Recupera os valores marcado no LogAttribute , caso a coluna principal retorne vazio outra consulta sera executada com a coluna de backup
        /// </summary>
        /// <param name="nomeTabela"></param>
        /// <param name="coluna"></param>
        /// <param name="id"></param>
        /// <param name="colunaBackup"></param>
        /// <returns></returns>
        public string RecuperaValores(AuxiliarLog atributos, string id)
        {
            var colunas = atributos.Coluna;
            if (!string.IsNullOrWhiteSpace(atributos.ColunaBackup))
            {
                colunas += "," + atributos.ColunaBackup;
            }

            //Buscando dados das colunas passadas
            return ExecutaConsulta(atributos.Tabela, colunas, id);
        }

        /// <summary>
        /// Executa a consulta em uma determinada tabela e retorna uma determinada coluna buscando pelo Id
        /// </summary>
        /// <param name="nomeTabela"></param>
        /// <param name="coluna"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public string ExecutaConsulta(string nomeTabela, string coluna, string id)
        {
            var valorFinal = "";
            if (string.IsNullOrWhiteSpace(nomeTabela) || string.IsNullOrWhiteSpace(coluna))
            {
                return valorFinal;
            }
            var query = "SELECT " + coluna + " FROM " + nomeTabela + " WHERE Id='" + id + "'";

            SqlConnection con = new SqlConnection(Configuration["StringConexao:Padrao"]);
            SqlCommand cmd = new SqlCommand(query, con);
            try
            {
                using (var command = cmd)
                {
                    cmd.Connection.Open();
                    command.CommandText = query;

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            if (!string.IsNullOrWhiteSpace(reader[0].ToString()))
                            {
                                valorFinal = reader[0]?.ToString();
                            }
                            else
                            {
                                valorFinal = reader[1]?.ToString();
                            }
                        }
                    }
                    cmd.Connection.Close();
                }
                return valorFinal;
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                con.Close();
            }
        }


        public class AuxiliarLog
        {
            public string Tabela { get; set; }
            public string Coluna { get; set; }
            public string ColunaBackup { get; set; }
            public bool IsEnum { get; set; }
        }

        private void DetachAll()
        {
            foreach (var dbEntityEntry in _contexto.ChangeTracker.Entries().ToList())
            {
                if (dbEntityEntry.Entity != null)
                {
                    dbEntityEntry.State = EntityState.Detached;
                }
            }
        }

    }
}
