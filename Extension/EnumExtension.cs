using System.ComponentModel;

namespace FeatureLogArquivos.Extension
{
    public static class EnumExtension
    {
        public static T[] GetEnumValues<T>() where T : struct
        {
            if (!typeof(T).IsEnum)
            {
                return null;
            }
            return (T[])Enum.GetValues(typeof(T));
        }

        public static string GetEnumDescription(this Enum value)
        {
            var fi = value.GetType().GetField(value.ToString());

            var attributes = (DescriptionAttribute[])fi.GetCustomAttributes(typeof(DescriptionAttribute), false);

            return attributes.Length > 0
                ? attributes[0].Description
                : value.ToString();
        }

        public static T GetEnum<T>(string descricao) where T : struct
        {
            try
            {
                if (string.IsNullOrEmpty(descricao))
                    return GetEnumValues<T>().FirstOrDefault();

                var item = (T)Enum.Parse(typeof(T), descricao, true);

                return item;
            }
            catch
            {
                return GetEnumValues<T>().FirstOrDefault();
            }
        }

        public static string GetDescriptionType(Type tipo, string name)
        {
            var result = "";
            var teste2 = tipo.GetFields().ToList();
            foreach (var item in teste2)
            {
                var field = (DescriptionAttribute[])item.GetCustomAttributes(typeof(DescriptionAttribute), false);
                if (item.Name.ToLower() == name.ToLower())
                {
                    if (field.Count() > 0)
                    {
                        result = field[0].Description;
                    }
                    else
                    {
                        result = name;
                    }
                }
            }
            return result;
        }
    }
}
