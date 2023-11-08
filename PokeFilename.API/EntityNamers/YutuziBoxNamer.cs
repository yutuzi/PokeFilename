using PKHeX.Core;
using System;

namespace PokeFilename.API
{
    public sealed class BoxNamer : IFileNamer<PKM>
    {
        public string GetName(PKM obj)
        {
            if (obj is GBPKM gb)
                return GetGBPKM(gb);
            return GetRegular(obj);
        }

        private static string GetRegular(PKM pk)
        {
            string form = pk.Form > 0 ? $"-{pk.Form:00}" : string.Empty;
            string shinytype = GetShinyTypeString(pk);
            string gender = GetGender(pk);
            string ability = GetAbility(pk);
            string nature = GetNature(pk);
            string formName = GetFormName(pk).Length > 0 ? $"-{GetFormName(pk)}" : string.Empty;
            string rareForm = GetRareForm(pk);
            string teraType = GetTeraType(pk);
            string size = GetScale(pk);

            string IVList = $"{pk.IV_HP}.{pk.IV_ATK}.{pk.IV_DEF}.{pk.IV_SPA}.{pk.IV_SPD}.{pk.IV_SPE}";

            var balllist = GameInfo.Strings.balllist;
            string ballFormatted = pk.Ball < balllist.Length ? balllist[pk.Ball].Split(' ')[0] : "???";

            string speciesName = SpeciesName.GetSpeciesNameGeneration(pk.Species, (int)LanguageID.English, pk.Format);
            if (pk is IGigantamax { CanGigantamax: true })
                speciesName += "-Gmax";

            if (pk.IsEgg)
                return $"{pk.Species:000}{form}{shinytype} - {speciesName}{formName}{rareForm} ({ballFormatted}) {gender}{ability} - {nature} - {IVList} - {pk.OT_Name} - {pk.EncryptionConstant} - {teraType} - {size}";

            if (pk.IsNicknamed)
                return $"{pk.Species:000}{form}{shinytype} - {pk.Nickname} [{speciesName}{formName}{rareForm} ({ballFormatted})] {gender}{ability} - {nature} - {IVList} - {pk.OT_Name} - {pk.EncryptionConstant} - {teraType} - {size}";
            else
                return $"{pk.Species:000}{form}{shinytype} - {speciesName}{formName}{rareForm} [{speciesName}{formName} ({ballFormatted})] {gender}{ability} - {nature} - {IVList} - {pk.OT_Name} - {pk.EncryptionConstant} - {teraType} - {size}";
        }

        private static string GetShinyTypeString(PKM pk)
        {
            if (!pk.IsShiny)
                return string.Empty;
            if (pk.Format >= 8 && (pk.ShinyXor == 0 || pk.FatefulEncounter || pk.Version == (int)GameVersion.GO))
                return " ■";
            return " ★";
        }

        private static string GetTeraType (PKM pk)
        {
            if (pk is PK9)
            {
                var pk9 = (PK9)pk;
                var tera = (int)pk9.TeraTypeOriginal;
                var strings = Util.GetTypesList("en");
                return strings[tera];
            }
            else
            {
                return "";
            }
            
        }

        private static string GetScale (PKM pk)
        {
            if (pk is PK9)
            {
                var pk9 = (PK9)pk;
                var scale = pk9.Scale;
                var strings = Enum.GetNames(typeof(PokeSizeDetailed));
                return strings[(int)PokeSizeDetailedUtil.GetSizeRating(scale)];
            }
            else
            {
                return "";
            }
        }

        private static string GetNature(INature pk)
        {
            var nature = pk.Nature;
            var strings = Util.GetNaturesList("en");
            if ((uint)nature >= strings.Length)
                nature = 0;
            return strings[nature];
        }

        private static string GetAbility(PKM pk)
        {
            var ability = pk.Ability;
            var index = pk.PersonalInfo.GetIndexOfAbility(ability);
            var count = pk.PersonalInfo.AbilityCount;
            if (index < count - 1)
            {
                return String.Empty;
            }
            if (pk.Gender == 2)
                return $"- [HA]";
            else
                return $"[HA]";
        }

        private static string GetFormName(PKM pk)
        {
            var Strings = GameInfo.GetStrings(GameLanguage.DefaultLanguage);
            string FormString = ShowdownParsing.GetStringFromForm(pk.Form, Strings, pk.Species, pk.Context);
            string FormName = ShowdownParsing.GetShowdownFormName(pk.Species, FormString);
            return FormName;
        }

        private static string GetRareForm(PKM pk)
        {
            if (((Species)pk.Species is Species.Dunsparce || (Species)pk.Species is Species.Tandemaus) && pk.EncryptionConstant % 100 == 0)
            {
                if ((Species)pk.Species is Species.Dunsparce)
                    return "-Three Segment";
                else if ((Species)pk.Species is Species.Tandemaus)
                    return "-Family of Three";
            }

            return String.Empty;
        }

        private static string GetGender (PKM pk)
        {
            if (pk.Gender == 0)
            {
                return "- [M]";
            }
            else if (pk.Gender == 1)
            {
                return "- [F]";
            }
            else
            {
                return String.Empty;
            }
        }

        private static string GetGBPKM(GBPKM gb)
        {
            string form = gb.Form > 0 ? $"-{gb.Form:00}" : string.Empty;
            string star = gb.IsShiny ? " ★" : string.Empty;

            string IVList = $"{gb.IV_HP}.{gb.IV_ATK}.{gb.IV_DEF}.{gb.IV_SPA}.{gb.IV_SPD}.{gb.IV_SPE}";
            string speciesName = SpeciesName.GetSpeciesNameGeneration(gb.Species, (int)LanguageID.English, gb.Format);
            string OTInfo = string.IsNullOrEmpty(gb.OT_Name) ? "" : $" - {gb.OT_Name} - {gb.TID16:00000}";

            var raw = gb switch
            {
                PK1 pk1 => new PokeList1(pk1).Write(),
                PK2 pk2 => new PokeList2(pk2).Write(),
                _ => gb.Data
            };
            var checksum = Checksums.CRC16_CCITT(raw);
            return $"{gb.Species:000}{form}{star} - {speciesName} - {IVList}{OTInfo} - {checksum:X4}";
        }
    }
}
