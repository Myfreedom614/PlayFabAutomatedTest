using System;
using System.Collections.Generic;

namespace PlayFabAutomatedTest
{
    public class CmdArguments
    {
        private readonly IDictionary<string, string> _internalValueMap;

        private CmdArguments(IDictionary<string, string> valueMap)
        {
            _internalValueMap = valueMap ?? new Dictionary<string, string>();
        }

        public bool HasOption(string option)
        {
            return _internalValueMap.ContainsKey(option);
        }

        public bool? GetBooleanIfExists(string option)
        {
            if (!HasOption(option))
                return new bool?();

            return bool.Parse(_internalValueMap[option]);
        }

        public int? GetIntIfExists(string option)
        {
            if (!HasOption(option))
                return new int?();
            int result;
            if (int.TryParse(_internalValueMap[option], out result))
                return result;
            return null;
        }

        public string GetStringIfExists(string option)
        {
            if (!HasOption(option))
                return null;

            return _internalValueMap[option];
        }

        public float? GetFloatIfExists(string option)
        {
            if (!HasOption(option))
                return new float?();

            return float.Parse(_internalValueMap[option]);
        }

        public double? GetDoubleIfExists(string option)
        {
            if (!HasOption(option))
                return new double?();

            return double.Parse(_internalValueMap[option]);
        }

        public DateTime? GetDateTime(string option)
        {
            if (!HasOption(option))
                return new DateTime?();

            return DateTime.Parse(_internalValueMap[option]);
        }

        public static CmdArguments From(string[] args, int offset)
        {
            int newLength = args.Length - offset;
            string[] array = new string[args.Length - offset];
            for (int i = 0; i < newLength; i++)
                array[i] = args[offset + i];
            return From(array);
        }

        public static CmdArguments From(string[] args)
        {
            IDictionary<string, string> tokenValueMap = new Dictionary<string, string>();

            for (int i = 0; i < args.Length; i++)
            {
                if (args[i].StartsWith("-"))
                {
                    string token = args[i].TrimStart('-');
                    string value = string.Empty;

                    if (args.Length > i && !args[i + 1].StartsWith("-"))
                        value = args[i + 1];

                    tokenValueMap.Add(token, value);
                }
            }

            return new CmdArguments(tokenValueMap);
        }
    }
}
