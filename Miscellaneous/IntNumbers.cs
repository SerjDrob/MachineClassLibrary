using Humanizer;
using MachineClassLibrary.Classes;

namespace MachineClassLibrary.Miscellanius
{
    public static class IntNumbers
    {
        public static string ApplyCase(this string str, GrammaticalCase grammaticalCase)
        {
            if (str.EndsWith("ая"))
            {
                switch (grammaticalCase)
                {
                    case GrammaticalCase.Nominative:
                        return str;
                    case GrammaticalCase.Genitive:
                        return str.Substring(0, str.Length - 2) + "ой";
                    case GrammaticalCase.Dative:
                        return str.Substring(0, str.Length - 2) + "ой";

                    case GrammaticalCase.Accusative:
                        return str.Substring(0, str.Length - 2) + "ую";

                    case GrammaticalCase.Instrumental:
                        return str.Substring(0, str.Length - 2) + "ой";

                    case GrammaticalCase.Prepositional:
                        return str.Substring(0, str.Length - 2) + "ой";

                    default:
                        break;
                }
            }
            else if (str.EndsWith("ья"))
            {
                switch (grammaticalCase)
                {
                    case GrammaticalCase.Nominative:
                        return str;
                    case GrammaticalCase.Genitive:
                        return str.Substring(0, str.Length - 2) + "ей";
                    case GrammaticalCase.Dative:
                        return str.Substring(0, str.Length - 2) + "ей";

                    case GrammaticalCase.Accusative:
                        return str.Substring(0, str.Length - 2) + "ью";

                    case GrammaticalCase.Instrumental:
                        return str.Substring(0, str.Length - 2) + "ей";

                    case GrammaticalCase.Prepositional:
                        return str.Substring(0, str.Length - 2) + "ей";

                    default:
                        break;
                }
            }
            return str;
        }

    }
}
