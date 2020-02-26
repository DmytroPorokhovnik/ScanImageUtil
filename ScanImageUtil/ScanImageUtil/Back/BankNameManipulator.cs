using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScanImageUtil.Back
{
    internal static class BankNameManipulator
    {
        private static readonly Dictionary<string, string> bankNamesStore = new Dictionary<string, string>
        {
            {"MoldIndConBank", "MICB"},
            {"АйбоксБанк", "Айбокс"},
            {"Акордбанк", "Акорд"},
            {"Акцент-Банк", "Акцент"},
            {"А-банк", "Акцент"},
            {"Альтбанк", "Альтбанк"},
            {"Неос", "Альтбанк"},
            {"Альфа-Банк", "Альфа"},
            {"Альянс банк", "Альянс"},
            {"Аркада", "Аркада"},
            {"АсвиоБанк", "Асвио"},
            {"АТАКБ«Львів»", "Львов"},
            {"Банк3/4", "Банк34"},
            {"БМ-Банк", "БМ"},
            {"Восток", "Восток"},
            {"Европромбанк", "Европром"},
            {"ИдеяБанк", "Идея"},
            {"ИнвестицийиСбереж", "ИиС"},
            {"Индустриалбанк", "Индустриал"},
            {"КИБ", "КИБ"},
            {"КлиринговыйДом", "КлирингДом"},
            {"Конкорд", "Конкорд"},
            {"КредиАгриколь", "КредиАгриколь"},
            {"КредитДнепр", "КредитДнепр"},
            {"Кредобанк", "Кредобанк"},
            {"Кристаллбанк", "Кристалл"},
            {"Мегабанк", "Мегабанк"},
            {"МИБ", "МИБ"},
            {"МТБ-Банк", "МТБ"},
            {"ОТП-банк", "ОТП"},
            {"Ощадбанк", "Ощад"},
            {"ПИБ", "ПИБ"},
            {"Пиреус", "Пиреус"},
            {"Приватбанк", "Приват"},
            {"ПУМБ", "ПУМБ"},
            {"Райффайзенбанк Аваль", "Аваль"},
            {"Сбербанк", "Сбербанк"},
            {"Сич", "Сич"},
            {"Скайбанк", "Скай"},
            {"Таскомбанк", "Таском"},
            {"Укрбанктехника", "УБТ"},
            {"Укрбудинвестбанк", "Укрбудинвест"},
            {"Укрпромбанк", "Укрпром"},
            {"УкрСиббанк", "УкрСиб"},
            {"Укрсоцбанк", "Укрсоц"},
            {"Укрэксимбанк", "Укрэксим"},
            {"ФинРостБанк", "ФинРост"},
            {"Форвард", "Форвард"},
        };

        static int ComputeLevenshteinDistance(string source, string target)
        {
            if ((source == null) || (target == null)) return 0;
            if ((source.Length == 0) || (target.Length == 0)) return 0;
            if (source == target) return source.Length;

            int sourceWordCount = source.Length;
            int targetWordCount = target.Length;

            // Step 1
            if (sourceWordCount == 0)
                return targetWordCount;

            if (targetWordCount == 0)
                return sourceWordCount;

            int[,] distance = new int[sourceWordCount + 1, targetWordCount + 1];

            // Step 2
            for (int i = 0; i <= sourceWordCount; distance[i, 0] = i++) ;
            for (int j = 0; j <= targetWordCount; distance[0, j] = j++) ;

            for (int i = 1; i <= sourceWordCount; i++)
            {
                for (int j = 1; j <= targetWordCount; j++)
                {
                    // Step 3
                    int cost = (target[j - 1] == source[i - 1]) ? 0 : 1;

                    // Step 4
                    distance[i, j] = Math.Min(Math.Min(distance[i - 1, j] + 1, distance[i, j - 1] + 1), distance[i - 1, j - 1] + cost);
                }
            }

            return distance[sourceWordCount, targetWordCount];
        }
        static double CalculateSimilarity(string source, string target)
        {
            if ((source == null) || (target == null)) return 0.0;
            if ((source.Length == 0) || (target.Length == 0)) return 0.0;
            source = source.ToLower();
            target = target.ToLower();
            if (source == target) return 1.0;

            int stepsToSame = ComputeLevenshteinDistance(source, target);
            return (1.0 - ((double)stepsToSame / (double)Math.Max(source.Length, target.Length)));
        }

        public static string GetBankName(string bankName)
        {
            var maxSimilarity = 0.0;
            var bestDecision = "";
            bankName = bankName.Trim().ToLower();
            foreach (var currentBank in bankNamesStore.Keys)
            {
                var similarity = CalculateSimilarity(currentBank.ToLower(), bankName);
                if(similarity > 0.6 && similarity > maxSimilarity)
                {
                    maxSimilarity = similarity;
                    bestDecision = currentBank;
                }
            }
            if (string.IsNullOrEmpty(bestDecision))
                return "";
            else
                return bankNamesStore[bestDecision];
        }


    }
}
