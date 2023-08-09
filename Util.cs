using System.ComponentModel;

namespace FeatureLogArquivos
{
    public static class Util
    {
        public static string GetDescriptionPropertyClass<T>(string propriedade)
        {
            //Recupera o nome da propriedade caso exista no filtro
            var props = typeof(T).GetProperties().Where(prop => prop.Name.ToLower() == propriedade.Trim().ToLower()).FirstOrDefault()?.Name;
            if (!string.IsNullOrEmpty(props))
            {
                //Recupera o description da classe
                propriedade = TypeDescriptor.GetProperties(typeof(T))[props].Description;
            }
            return propriedade;
        }

        public static string ConversaoSegundosEmHora(double seconds)
        {
            var hours = Math.Floor(seconds / 3600);
            var minutes = Math.Floor((seconds - hours * 3600) / 60);
            var second = Math.Floor(seconds % 60);

            var hora = hours.ToString();
            var minutos = minutes.ToString();
            var segundos = second.ToString();
            if (hours < 10)
            {
                hora = $"0{hours}";
            }

            if (minutes < 10)
            {
                minutos = $"0{minutes}";
            }

            if (second < 10)
            {
                segundos = $"0{second}";
            }

            var finalHours = $"{hora}:{minutos}:{segundos}";

            return finalHours;
        }

        public static int Hour_Minutes(string fullhour)
        {
            String[] parts = fullhour.Split(":");

            int hour = int.Parse(parts[0]);
            int min = int.Parse(parts[1]);

            return (hour * 60) + min;
        }

        public static T Cast<T>(object obj, T type)
        {
            return (T)obj;
        }

        public static T CastTo<T>(this Object value, T targetType)
        {
            // targetType above is just for compiler magic
            // to infer the type to cast value to
            return (T)value;
        }

        public static bool IsGuid(string value)
        {
            return Guid.TryParse(value, out Guid x);
        }

        public static string Base64LimparString(string base64)
        {
            if (!string.IsNullOrEmpty(base64) && base64.Contains(','))
            {
                return base64.Split(",")[1];
            }
            return base64;
        }

        public static int Base64TamanhoBytes(string base64)
        {
            if (string.IsNullOrEmpty(base64))
            {
                return 0;
            }

            var totalCaracteres = base64.Length;
            var totalCaracteresPreenchimento = base64
                .Substring(totalCaracteres - 2, 2)
                .Count(x => x == '=');
            return (3 * (totalCaracteres / 4)) - totalCaracteresPreenchimento;
        }


        public static string FormatarSomatorioTemposHorasMinitosSegundos(List<string> tempos)
        {

            if (tempos.Count > 0)
            {
                var somatorioTempo = new TimeSpan();

                foreach (var tempo in tempos)
                {
                    var tempoSeparado = tempo.Split(":");
                    if (tempoSeparado.Count() == 3)
                    {
                        somatorioTempo += new TimeSpan(int.Parse(tempoSeparado[0]), int.Parse(tempoSeparado[1]), int.Parse(tempoSeparado[2]));
                    }
                    else if (tempoSeparado.Count() == 2)
                    {
                        somatorioTempo += new TimeSpan(0, int.Parse(tempoSeparado[0]), int.Parse(tempoSeparado[1]));
                    }
                    else if (tempoSeparado.Count() == 1)
                    {
                        somatorioTempo += new TimeSpan(0, 0, int.Parse(tempoSeparado[1]));
                    }
                }
                return HorasMinutosSegundos(somatorioTempo.ToString());
            }
            return string.Empty;
        }

        private static string HorasMinutosSegundos(string tempo)
        {
            var time = TimeSpan.Parse(tempo);


            var horas = time.Hours;
            if (time.Days > 0)
            {
                horas += time.Days * 24;
            }
            return $"{horas:00}:{time.Minutes:00}:{time.Seconds:00}";
        }
    }
}
