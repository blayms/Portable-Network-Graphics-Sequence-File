namespace Blayms.PNGS.Constructor
{
    public class CommandArgsInfo
    {
        public const string EmptyString = "<>";
        private Dictionary<string, (Type, bool, object)> Parameters { get; } = new();
        private (string Name, (Type Type, bool HasDefaultValue, object DefaultValue))[] raw;
        public int Count => raw.Length;
        private int m_NecessarilyCount = -1;
        public int NecessarilyCount
        {
            get
            {
                if(m_NecessarilyCount == -1)
                {
                    m_NecessarilyCount = raw.Where(x => !x.Item2.HasDefaultValue).Count();
                }
                return m_NecessarilyCount;
            }
        }
        public CommandArgsInfo(params (string Name, (Type Type, bool HasDefaultValue, object DefaultValue) Settings)[] parameters)
        {
            raw = parameters;
            foreach (var param in parameters)
            {
                Parameters.Add(param.Name, param.Settings);
            }
        }
        public Type? GetParameterType(string parameterName)
        {
            if (Parameters.TryGetValue(parameterName, out (Type Type, bool HasDefaultValue, object DefaultValue) settings))
            {
                return settings.Type;
            }
            return null;
        }
        public override string ToString()
        {
            if (raw.Length == 0)
                return EmptyString;

            var sb = new System.Text.StringBuilder();
            sb.Append('<');
            for (int i = 0; i < raw.Length; i++)
            {
                var (name, type) = raw[i];
                sb.Append($"{type.Type.Name}{(type.HasDefaultValue ? "?" : "")}");
                sb.Append(": ");
                sb.Append($"{name}{(type.HasDefaultValue ? $" = {type.DefaultValue}" : "")}");
                if (i < raw.Length - 1)
                    sb.Append(", ");
            }
            sb.Append('>');
            return sb.ToString();
        }
        public string GetNameByIndex(int index)
        {
            return raw[index].Name ?? string.Empty;
        }
        public Type? GetTypeByIndex(int index)
        {
            return raw[index].Item2.Type ?? null;
        }
        public bool GetHasDefaultValueByIndex(int index)
        {
            return raw[index].Item2.HasDefaultValue;
        }
        public object GetDefaultValueByIndex(int index)
        {
            return raw[index].Item2.DefaultValue ?? null!;
        }
    }
}
