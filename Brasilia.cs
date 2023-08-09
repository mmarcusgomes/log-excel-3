namespace FeatureLogArquivos
{
    public static class Brasilia
    {
        public static DateTime DataAtual => TimeZoneInfo
            .ConvertTime(
                DateTime.UtcNow,
                TimeZoneInfo.FindSystemTimeZoneById("E. South America Standard Time")
            );
    }
}
