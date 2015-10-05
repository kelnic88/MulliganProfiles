using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SmartBot.Database;
using SmartBot.Plugins.API;
using SmartBotUI;
using SmartBotUI.Settings;

namespace SmartBotUI.Mulligan
{
    public static class Extension
    {
        public static void AddOrUpdate<TKey, TValue>(
     this IDictionary<TKey, TValue> map, TKey key, TValue value)
        {
            map[key] = value;
        }
    }

    [Serializable]


    public class bMulliganProfile : MulliganProfile
    {
        /*AutoArena Plugin territory*/
        private readonly bool _pluginValueLowCurve = false;
        //private readonly bool _pluginValueMediumCurve = false;
        //private readonly bool _pluginValueHeavyCurve = false;
        //private readonly bool _pluginValueSmartBotCurve = false;
        /****************************/
        private Dictionary<string, bool> _whiteList; // CardName, KeepDouble
        private readonly List<Card> _cardsToKeep;
        private const string Coin = "GAME_005";
        

        public bMulliganProfile()
            : base()
        {
            _whiteList = new Dictionary<string, bool>();
            _cardsToKeep = new List<Card>();
        }

        public override List<Card> HandleMulligan(List<Card> Choices, CClass opponentClass, CClass ownClass)
        {
            var hand = HandleMinions(Choices, _whiteList);
            _whiteList.AddOrUpdate(Coin, true);
            _whiteList.AddOrUpdate(HandleWeapons(Choices), false);          // only 1 weapon is allowed
            _whiteList.AddOrUpdate(HandleSpells(Choices, hand), false); // only 1 spell is allowed

            _whiteList = hand.Item1.ToDictionary(entry => entry.Key, entry => entry.Value);

            foreach (var s in from s in Choices
                              let keptOneAlready = _cardsToKeep.Any(c => c.Name == s.Name)
                              where _whiteList.ContainsKey(s.Name)
                              where !keptOneAlready | _whiteList[s.Name]
                              select s)
                _cardsToKeep.Add(s);
            return _cardsToKeep;
        }
        /*Not yet implemented
         TODO: Handle it*/

        /// <summary>
        /// 
        /// </summary>
        /// <param name="choices"></param>
        /// <param name="hand">
        /// Sends a tuple with the following information:
        /// Whitelisted minions, hasOneManaDrop, hasTwoManaDrop, hasThreeManaDrop and hasFourManaDrop
        /// </param>
        /// <returns></returns>
        private static string HandleSpells(IEnumerable<Card> choices, Tuple<Dictionary<string, bool>, bool, bool, bool, bool> hand)
        {
            var coin = choices.Count() > 4;
            var allowedSpells = new List<string>()
            {
                /*
                 * Mage: Frostbolt, Flamecannon, Unstable Portal, Arcane Missiles, Mirror Image
                 * Shaman: Rockbiter Weapon, Feral Spirit
                 * Priest: Holy Smite, Velens Chosen, Thoughtsteal
                 * 
                 * Paladin: Noble Sac, Avenge, Seal of the Champion, Mustard for Butter
                 * Warrior: Bash, Slam, Shield Block
                 * Warlock: Mortal Coin, Darkbomb, ImpLosion
                 * 
                 * Hunter: Tracking, Animal Companion, Secrets, Quick Shot, unleash
                 * Rogue: Deadly Poison, Burgle, Beneath the Grounds, Backstab
                 * Druid: Innervate, Wild Growth, Living Roots, Power of the Wild, Wrath
                 */
                "EX1_277", "GVG_001","CS2_024","CS2_027","GVG_003",     
                "CS2_045","EX1_248",                                        
                "CS1_130","GVG_010","EX1_339",
                "EX1_130","FP1_020","AT_074","GVG_061",
                "AT_064","EX1_391","EX1_606",
                "EX1_302","GVG_015","GVG_045",
                "DS1_184","NEW1_031","BRM_013","EX1_538",
                "CS2_074","AT_033","AT_035","CS2_072",
                "EX1_169","CS2_013","AT_037","EX1_160","EX1_154"
            };
            foreach (var c in choices)
            {
                var spells = CardTemplate.LoadFromId(c.Name);

                if (spells.Type == SmartBot.Plugins.API.Card.CType.SPELL && allowedSpells.Contains(c.Name))
                {
                    if (spells.Cost == 0 && allowedSpells.Contains(c.Name))
                        return c.Name;
                    if (!spells.IsSecret && spells.Cost == 1 && !hand.Item2)
                        return c.Name;
                    if (!spells.IsSecret && spells.Cost == 2 && !hand.Item3 || coin)
                        return c.Name;
                    if (!spells.IsSecret && spells.Cost == 3 && !hand.Item4)
                        return c.Name;
                    if (!spells.IsSecret && spells.Cost == 4 && !hand.Item5)
                        return c.Name;
                 
                }
                if (spells.Cost == 1 && spells.IsSecret && !hand.Item2)
                    return c.Name;
                if (spells.Cost == 2 && spells.IsSecret && !hand.Item3)
                    return c.Name;
                if (spells.Cost == 3 && spells.IsSecret && !hand.Item4 && coin)
                    return c.Name;
            }

            return "";
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="choices"></param>
        /// <returns></returns>
        private static string HandleWeapons(IEnumerable<Card> choices)
        {
            return (from c in choices let minion = CardTemplate.LoadFromId(c.Name) where minion.Type == SmartBot.Plugins.API.Card.CType.WEAPON && minion.Cost <= 4 select c).Aggregate("EX1_409t", (current, c) => WhichIsStronger(current, c.Name));
        }

        /// <summary>
        /// Method is designed to look through your hand and whitelist best possible minions to fit your curve well
        /// </summary>
        /// <param name="choices">List of 3 to 4 card choices that are analyzed</param>
        /// <param name="whiteList">Dictionary list which contains cards that are kept</param>
        /// <returns></returns>
        private static Tuple<Dictionary<string, bool>, bool, bool, bool, bool> HandleMinions(IReadOnlyCollection<Card> choices, Dictionary<string, bool> whiteList)
        {
            var has1Drop = false;
            var has2Drop = false;
            var has3Drop = false;
            var has4Drop = false;
            var num2Drops = 0;
            var num3Drops = 0;
            var num4Drops = 0;
            var hasCoin = choices.Count > 3;
            var badMinions = new List<string>() { "CS2_173", "CS2_203", "FP1_017", "EX1_045", "NEW1_037", "EX1_055", "EX1_058", "NEW1_021",
                "GVG_025", "GVG_039", "EX1_306", "EX1_084", "EX1_582", "GVG_084", "CS2_118", "CS2_122", "CS2_124", "EX1_089", "EX1_050", "GVG_089", "EX1_005", "EX1_595", "EX1_396", "EX1_048", "AT_091",
                "EX1_584", "EX1_093","GVG_094", "GVG_109","GVG_107", "DS1_175", "EX1_362", "GVG_122" };
            //checkOneDrops();

            foreach (var c in from c in choices where !badMinions.Contains(c.Name) where c.Cost == 1 let minion = CardTemplate.LoadFromId(c.Name) where !badMinions.Contains(c.Name) && minion.Type == SmartBot.Plugins.API.Card.CType.MINION && minion.Atk >= 1 select c)
            {
                has1Drop = true;
                whiteList.AddOrUpdate(GetBestOne(choices, 1), false);
            }
            //checkTwoDrop()
            foreach (var c in choices)
            {
                if (badMinions.Contains(c.Name)||c.Cost != 2) continue;
                var minion = CardTemplate.LoadFromId(c.Name);
                if (minion.Type == SmartBot.Plugins.API.Card.CType.MINION)
                {
                    whiteList.AddOrUpdate(GetBestOne(choices, 2), false);
                    num2Drops++;
                    has2Drop = true;
                }
                if (minion.Type == SmartBot.Plugins.API.Card.CType.MINION && hasCoin)
                {
                    whiteList.AddOrUpdate(c.Name, true);
                    num2Drops++;
                }
                if (num2Drops >= 2) break;
            }
            //CheckThreeDrops();
            foreach (var c in choices)
            {
                if (badMinions.Contains(c.Name)) continue;
                if (c.Cost != 3) continue;
                var minion = CardTemplate.LoadFromId(c.Name);
                if (minion.Type == SmartBot.Plugins.API.Card.CType.MINION)
                {
                    whiteList.AddOrUpdate(GetBestOne(choices, 3), hasCoin);
                    num3Drops++;
                    has3Drop = true;
                }
                if (has2Drop || num3Drops > 1 || minion.Health <= 2 || !hasCoin)
                    continue;
                whiteList.AddOrUpdate(c.Name, false);
                num3Drops++;
                has3Drop = true;
            }
            //checkFourDrops();
            foreach (var minion in choices.Where(c => !badMinions.Contains(c.Name) && has3Drop).Where(c => c.Cost == 4 && has3Drop || hasCoin).Select(c => CardTemplate.LoadFromId(c.Name)).Where(minion => minion.Type == SmartBot.Plugins.API.Card.CType.MINION))
            {
                whiteList.AddOrUpdate(GetBestOne(choices, 4), false);
                has4Drop = true;
            }
            
            return new Tuple<Dictionary<string, bool>, bool, bool, bool, bool>(whiteList, has1Drop, has2Drop, has3Drop, has4Drop);
        }

        /// <summary>
        /// its redudndant method. I will remove it after we get out of beta
        /// </summary>
        /// <param name="choices"></param>
        /// <param name="cost"></param>
        /// <returns></returns>
        private static string GetBestOne(IEnumerable<Card> choices, int cost)
        {
            return choices.Where(c => c.Cost == cost).Aggregate("CS2_118", (current, c) => WhichIsStronger(current, c.Name));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="curBest"></param>
        /// <param name="comparison"></param>
        /// <returns></returns>
        private static string WhichIsStronger(string curBest, string comparison)
        {
            var curBestCheck = CardTemplate.LoadFromId(curBest);
            var comparisonCheck = CardTemplate.LoadFromId(comparison);
            if (curBestCheck.Type == SmartBot.Plugins.API.Card.CType.MINION)
                return curBestCheck.Health > comparisonCheck.Health ? curBest : comparison; // handles minions
            return curBestCheck.Atk > comparisonCheck.Atk ? curBest : comparison; //handles weapons
        }
    }
}