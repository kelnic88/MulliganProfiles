using System;
using System.Collections.Generic;
using System.Linq;
using SmartBot.Database;

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
        /// <summary>
        ///     Boolean Logic Place
        /// </summary>
        /// ////////////////////////////////// //
        private const bool MMFelRever = true; // [Mech Mage] Keeps felReaver on a curve


        /// ////////////////////////////////// //
        private const string Coin = "GAME_005";

        private readonly List<string> _removalSpells;
        private readonly Dictionary<string, bool> _whiteList; // CardName, KeepDouble
        private readonly List<Card> _cardsToKeep;


        public bMulliganProfile()
        {
            _removalSpells = new List<string> {"EX1_246", "CS2_022", "CS2_076", "AT_048"};
            _whiteList = new Dictionary<string, bool>();
            _cardsToKeep = new List<Card>();
        }


        public override List<Card> HandleMulligan(List<Card> Choices, CClass opponentClass, CClass ownClass)
        {
            var hasCoin = Choices.Count > 3;
            _whiteList.AddOrUpdate(Coin, true);

            #region Class Specific Mulligan

            switch (ownClass)
            {
                case CClass.DRUID:
                {
                    break;
                }
                case CClass.HUNTER:
                {
                    break;
                }
                case CClass.MAGE:
                {
                    MulliganMage(Choices, _whiteList, opponentClass, hasCoin);
                    break;
                }
                case CClass.PALADIN:
                {
                    break;
                }

                case CClass.PRIEST:
                {
                    break;
                }
                case CClass.ROGUE:
                {
                    break;
                }
                case CClass.SHAMAN:
                {
                    MulliganShaman(Choices, _whiteList, opponentClass, hasCoin, _removalSpells);
                    break;
                }
                case CClass.WARLOCK:
                {
                    break;
                }
                case CClass.WARRIOR:
                {
                    MulliganWarrior(Choices, _whiteList, opponentClass, hasCoin);
                    break;
                }
            }

            #endregion

            foreach (var s in from s in Choices
                let keptOneAlready = _cardsToKeep.Any(c => c.Name == s.Name)
                where _whiteList.ContainsKey(s.Name)
                where !keptOneAlready | _whiteList[s.Name]
                select s)
                _cardsToKeep.Add(s);


            return _cardsToKeep;
        }

        /// <summary>
        /// </summary>
        /// <param name="choices"></param>
        /// <param name="whiteList"></param>
        /// <param name="opponentClass"></param>
        /// <param name="hasCoin"></param>
        /// <param name="removalSpells"></param>
        private static void MulliganShaman(List<Card> choices, Dictionary<string, bool> whiteList, CClass opponentClass,
            bool hasCoin, List<string> removalSpells)
        {
            //var gotWeapon = false;
            var control = false;
            var oneDrop = false;
            var twoDrop = false;
            var threeDrop = false;

            foreach (var c in choices)
            {
                var temp = CardTemplate.LoadFromId(c.Name);
                if (temp.Race == SmartBot.Plugins.API.Card.CRace.TOTEM && temp.Cost == 2 && temp.Atk != 0)
                {
                    twoDrop = true;
                    whiteList.AddOrUpdate(c.Name, hasCoin);
                }

                switch (opponentClass)
                {
                    case CClass.SHAMAN:
                        control = true;
                        break;
                    case CClass.PRIEST:
                        control = true;
                        break;
                    case CClass.MAGE:
                        //control = true;
                        break;
                    case CClass.PALADIN:
                        break;
                    case CClass.WARRIOR:
                        control = true;
                        break;
                    case CClass.WARLOCK:
                    {
                        whiteList.AddOrUpdate("EX1_245", false); //earth shock because I assume it's a handlock
                        control = true;
                    }
                        break;
                    case CClass.HUNTER:
                        break;
                    case CClass.ROGUE:
                        control = true;
                        break;
                    case CClass.DRUID: // I assume that druids on the ladder are aggro
                        //control = true;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException("opponentClass", opponentClass, null);
                }
                if (temp.Type == SmartBot.Plugins.API.Card.CType.MINION &&
                    temp.Quality != SmartBot.Plugins.API.Card.CQuality.Epic)
                {
                    if (temp.Cost == 1)
                    {
                        oneDrop = true;
                        whiteList.AddOrUpdate(c.Name, !control);
                    }
                    if (temp.Cost == 2 && temp.Atk > 0)
                    {
                        twoDrop = true;
                        whiteList.AddOrUpdate(c.Name, false);
                    }
                    if (!control && temp.Cost == 3 && temp.Atk > 1)
                    {
                        threeDrop = true;
                        whiteList.AddOrUpdate(c.Name, hasCoin);
                    }
                }
                var curve = twoDrop || threeDrop && hasCoin;
                if (curve && temp.Cost == 4 && temp.Atk > 3)
                {
                    whiteList.AddOrUpdate(c.Name, false);
                }
            }
            foreach (var c in choices)
            {
                var temp = CardTemplate.LoadFromId(c.Name);
                if (temp.Type != SmartBot.Plugins.API.Card.CType.SPELL) continue;
                if (control && temp.Cost == 1) whiteList.AddOrUpdate(c.Name, false);
                if (!control && temp.Cost == 3 && !removalSpells.Contains(c.Name)) whiteList.AddOrUpdate(c.Name, false);
            }
        }

        /// <summary>
        ///     Mech Mages
        /// </summary>
        /// <param name="choices"></param>
        /// <param name="whiteList"></param>
        /// <param name="opponentClass"></param>
        /// <param name="hasCoin"></param>
        private static void MulliganMage(List<Card> choices, Dictionary<string, bool> whiteList, CClass opponentClass,
            bool hasCoin)
        {
            var control = false;
            var oneDrop = false;
            var twoDrop = false;
            var threeDrop = false;
            var curve = false;
            CardTemplate oneManaMinion = null;
            foreach (var c in choices)
            {
                var temp = CardTemplate.LoadFromId(c.Name);
                if (temp.Race == SmartBot.Plugins.API.Card.CRace.MECH)
                {
                    if (temp.Cost == 1)
                        oneManaMinion = CardTemplate.LoadFromId(c.Name);
                    if (temp.Cost == 2)
                    {
                        twoDrop = true;
                        whiteList.AddOrUpdate(c.Name, true);
                    }
                    if (temp.Cost == 3)
                    {
                        threeDrop = true;
                        whiteList.AddOrUpdate(c.Name, true);
                    }
                }
                if (temp.Race != SmartBot.Plugins.API.Card.CRace.MECH &&
                    temp.Quality != SmartBot.Plugins.API.Card.CQuality.Epic &&
                    temp.Type != SmartBot.Plugins.API.Card.CType.SPELL) //minions handler
                {
                    if (temp.Cost == 1 && temp.Race != SmartBot.Plugins.API.Card.CRace.MECH)
                    {
                        oneDrop = true;
                        whiteList.AddOrUpdate(c.Name, false);
                    }
                    if (temp.Cost == 2)
                    {
                        twoDrop = true;
                        whiteList.AddOrUpdate(c.Name, true);
                    }
                    if (temp.Cost == 3)
                    {
                        threeDrop = true;
                        whiteList.AddOrUpdate(c.Name, true);
                    }
                }

                switch (opponentClass)
                {
                    case CClass.SHAMAN:
                        control = true;
                        break;
                    case CClass.PRIEST:
                        control = true;
                        break;
                    case CClass.MAGE:
                        break;
                    case CClass.PALADIN:
                    {
                        if (temp.Cost == 3 && temp.Quality == SmartBot.Plugins.API.Card.CQuality.Epic)
                            whiteList.AddOrUpdate(c.Name, false);
                        break;
                    }
                    case CClass.WARRIOR:
                        control = true;
                        break;
                    case CClass.WARLOCK:
                        control = true;
                        break;
                    case CClass.HUNTER:
                        break;
                    case CClass.ROGUE:
                        control = true;
                        break;
                    case CClass.DRUID:
                        control = true;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException("opponentClass", opponentClass, null);
                }

                if (temp.Type == SmartBot.Plugins.API.Card.CType.SPELL && temp.Cost == 2)
                    whiteList.AddOrUpdate(c.Name, false);

                if (!control)
                {
                    if (temp.Type == SmartBot.Plugins.API.Card.CType.SPELL && temp.Cost == 1)
                        whiteList.AddOrUpdate(c.Name, false);
                    continue;
                }
                if (temp.Cost == 4 && temp.Race == SmartBot.Plugins.API.Card.CRace.MECH && temp.Atk > 3)
                    whiteList.AddOrUpdate(c.Name, false);
                curve = twoDrop && threeDrop;
            }

            if (oneManaMinion != null && (!oneDrop && oneManaMinion.Cost == 1))
                whiteList.AddOrUpdate("GVG_082", false);
            if (!threeDrop && !hasCoin) return;
            if (MMFelRever && threeDrop && hasCoin && choices.Any(c => c.Name == "GVG_016"))
                whiteList.AddOrUpdate("GVG_016", false);
            if (choices.Any(c => c.Name == "GVG_004") && threeDrop || curve)
                whiteList.AddOrUpdate("GVG_004", false);
        }

        /// <summary>
        ///     Supported deck arthetypes:
        ///     Mech Warrior
        /// </summary>
        /// <param name="choices"></param>
        /// <param name="whiteList"></param>
        /// <param name="opponentClass"></param>
        /// <param name="coin"></param>
        private static void MulliganWarrior(IEnumerable<Card> choices, IDictionary<string, bool> whiteList,
            CClass opponentClass, bool coin)
        {
            var gotWeapon = false;
            var control = false;
            var oneDrop = false;
            var twoDrop = false;
            var threeDrop = false;
            var weapons = new List<string>();

            foreach (var c in choices)
            {
                var temp = CardTemplate.LoadFromId(c.Name);
                if (!gotWeapon && temp.Type == SmartBot.Plugins.API.Card.CType.WEAPON && temp.Cost <= 4)
                {
                    twoDrop = true;
                    whiteList.AddOrUpdate(c.Name, false);
                    gotWeapon = true;
                }
                if (temp.Race == SmartBot.Plugins.API.Card.CRace.MECH)
                {
                    if (temp.Cost == 1)
                    {
                        oneDrop = true;
                        whiteList.AddOrUpdate(c.Name, false);
                    }
                    if (temp.Cost == 2)
                    {
                        twoDrop = true;
                        whiteList.AddOrUpdate(c.Name, false);
                    }
                    if (temp.Cost == 3)
                    {
                        threeDrop = true;
                        whiteList.AddOrUpdate(c.Name, false);
                    }
                }
                if (temp.Type != SmartBot.Plugins.API.Card.CType.WEAPON && temp.Cost <= 3 &&
                    temp.Type != SmartBot.Plugins.API.Card.CType.SPELL &&
                    temp.Race != SmartBot.Plugins.API.Card.CRace.MECH)
                {
                    whiteList.AddOrUpdate(c.Name, false);
                }
                switch (opponentClass)
                {
                    case CClass.SHAMAN:
                        control = true;
                        break;
                    case CClass.PRIEST:
                        control = true;
                        break;
                    case CClass.MAGE:
                        //control = true;
                        break;
                    case CClass.PALADIN:
                        break;
                    case CClass.WARRIOR:
                        control = true;
                        break;
                    case CClass.WARLOCK:
                        control = true;
                        break;
                    case CClass.HUNTER:
                        break;
                    case CClass.ROGUE:
                        control = true;
                        break;
                    case CClass.DRUID:
                        control = true;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException("opponentClass", opponentClass, null);
                }
                if (!control) continue;
                //var curve = twoDrop && threeDrop;
                if (temp.Type != SmartBot.Plugins.API.Card.CType.WEAPON && twoDrop && temp.Cost == 4 &&
                    temp.Race == SmartBot.Plugins.API.Card.CRace.MECH && temp.Atk > 3)
                    whiteList.AddOrUpdate(c.Name, false);
                //if (coin && temp.Quality == SmartBot.Plugins.API.Card.CQuality.Epic)
                //    _whiteList.AddOrUpdate(c.Name,false);
            }
        }
    }
}