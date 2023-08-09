namespace FeatureLogArquivos
{
    [System.AttributeUsage(System.AttributeTargets.All,

                         AllowMultiple = true)]  // Multiuse attribute.  
    public class LogAttribute : System.Attribute
    {
        public readonly string tabela;
        public readonly string coluna;
        public readonly string colunaBackup;



        public LogAttribute(string tabela, string coluna, string colunaBackup = "")
        {
            this.tabela = tabela;
            this.coluna = coluna;
            this.colunaBackup = colunaBackup;

        }
    }

}
