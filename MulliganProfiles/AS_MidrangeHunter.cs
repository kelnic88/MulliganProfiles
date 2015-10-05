using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
         #region Data
        /*==================Custom Behavior Logic==================
         *                                                        *
         *  Warning: custom behavior doesn't apply to all classes *
         *           refer to the forum thread for more details   *
         *                                                        *
         *  If you do not agree with some of the custom           *  
         *  mulligan scenarios you can set their respective       *
         *  boolean value to false                                *
         *                                                        * 
         *      True = this behavior will occur                   *
         *      False = it will avoid that particular             *
         *              mulligan logc                             * 
         *========================================================*/

        private const bool SnakeJuggler = true; //Keeps snake trap if you have a knife juggler in the opener and coin.
                                                //Coin snake trap => Knife juggler
        private const bool AdvancedFreezingTrap = true; 



        /*========================================================*/

        private const string AbusiveSergion = "CS2_188";
        private const string AnimalCompanion = "NEW1_031";
        private const string ArcaneGolem = "CS2_188";
        private const string ArgentHorserider = "AT_087";
        private const string ArgentSquire = "EX1_008";
        private const string BearTrap = "AT_060";
        private const string BigGameHunter = "EX1_005";
        private const string DrBalance = "GVG_110";
        private const string EaglehornBow = "EX1_536";
        private const string ExplosiveTrap = "EX1_610";
        private const string Flare = "EX1_544";
        private const string FreezingTrap = "EX1_611";
        private const string Glavezooka = "GVG_043";
        private const string Haundmaster = "DS1_070";
        private const string HauntedCreeper = "FP1_002";
        private const string HunterMark = "CS2_084";
        private const string IronbeakOwl = "CS2_203";
        private const string KezanMystic = "GVG_074";
        private const string KillCommand = "EX1_539";
        private const string KingsElekk = "AT_058";
        private const string KnifeJuggler = "NEW1_019";
        private const string LanceCarrier = "AT_084"; 
        private const string LepperGnome = "EX1_029";
        private const string Loatheb = "FP1_030";
        private const string MadScientist = "FP1_004";
        private const string PilotedShredder = "GVG_096";
        private const string QuickShot = "BRM_013";
        private const string RamWrangler = "AT_010";
        private const string SavannahHighmane = "EX1_534";
        private const string SnakeTrap = "EX1_554";
        private const string StranglethornTiger = "EX1_028";
        private const string UnleashTheSkill = "EX1_538";
        private const string Webspinner = "FP1_011";
        private const string Wolfrider = "CS2_124";
        private const string WorgenInfiltrator = "EX1_010";
        #endregion

        private readonly Dictionary<string, bool> _whiteList; // CardName, KeepDouble
        private readonly Dictionary<string, int> _weaponList;
        private readonly List<Card> _cardsToKeep;


        public bMulliganProfile()
            : base()
        {
            _whiteList = new Dictionary<string, bool>();
            _weaponList = new Dictionary<string, int>();
            _cardsToKeep = new List<Card>();
        }

        public void ChoseWeapon(List<Card> choices, int gz = 1, int eb = 2) //v2.0
        {
            _weaponList.AddOrUpdate(Glavezooka, gz);
            _weaponList.AddOrUpdate(EaglehornBow, eb);

            /* 
             * Yes, the proper way would be is to itterate through and find a min value,
             * but I figured since we know our decks, hardcoding values isn't half bad :3 
             * Feel free to sort dragonList dictionary and whitelist dragons in proper order to support more dragons later on
             */
            var i = 0;
            while (true)
            {

                if (choices.Any(c => c.Name == _weaponList.FirstOrDefault(x => x.Value == i).Key))
                {
                    _whiteList.AddOrUpdate(_weaponList.FirstOrDefault(x => x.Value == i).Key, false);
                    break;
                }
                i++;
                if (i > _weaponList.Count)
                    break;
            }
        }


        public override List<Card> HandleMulligan(List<Card> choices, CClass opponentClass, CClass ownClass)
        {
            var hasCoin = choices.Count > 3;
            var has2Drop = choices.Any(c => c.Name == HauntedCreeper) || choices.Any(c => c.Name == MadScientist) || choices.Any(c => c.Name == KnifeJuggler);// Kings Elek is not a legitimate 2 drop in my eyes
            var allowHunterMark = (choices.Any(c => c.Name == Webspinner) || (choices.Any(c => c.Name == HauntedCreeper)));
            var hasMadScientist = choices.Any(c => c.Name == MadScientist);
            var perfectCurve = false;
            var badCurve = false;

            if (AdvancedFreezingTrap)
            {
                var hand = AnalyzeMyHandTuple(choices);
                if (hand.Item2 && hand.Item1)
                    _whiteList.AddOrUpdate(FreezingTrap, false);
            }
            /*Default mulligans regardless of matchup/coin*/
            _whiteList.AddOrUpdate(Webspinner, false);
            _whiteList.AddOrUpdate(KnifeJuggler, false);
            _whiteList.AddOrUpdate(HauntedCreeper, false);
            _whiteList.AddOrUpdate(MadScientist, hasCoin); //keeps 2 scientists on coin
            _whiteList.AddOrUpdate(LepperGnome, false);
            _whiteList.AddOrUpdate(AbusiveSergion, false);
            _whiteList.AddOrUpdate(WorgenInfiltrator, false);
            _whiteList.AddOrUpdate(ArgentSquire, false);
            _whiteList.AddOrUpdate(LanceCarrier, false);

            _whiteList.AddOrUpdate(!has2Drop ? KingsElekk : ArgentHorserider, false);

            // ReSharper disable once RedundantLogicalConditionalExpressionOperand
            if (choices.Any(c => c.Name == KnifeJuggler) && SnakeJuggler && hasCoin)
                _whiteList.AddOrUpdate(SnakeTrap, false);

            switch (opponentClass)
            {
                case CClass.DRUID:
                    {
                        if (allowHunterMark)
                            _whiteList.AddOrUpdate(HunterMark, false);
                        if (has2Drop)
                        {
                            _whiteList.AddOrUpdate(EaglehornBow, false);
                            _whiteList.AddOrUpdate(AnimalCompanion, false);
                        }
                        if (has2Drop && hasCoin)
                            _whiteList.AddOrUpdate(PilotedShredder, false);

                        ChoseWeapon(choices); //GZ, EB


                        break;
                    }
                case CClass.HUNTER:
                    {
                        if (has2Drop)
                        {
                            if (hasMadScientist)
                                ChoseWeapon(choices, 2, 1); //Priority to Eaglehorn Bow
                            else
                                ChoseWeapon(choices); //Priority to glavezooka
                        }
                        ChoseWeapon(choices);
                        if (has2Drop || hasCoin) _whiteList.AddOrUpdate(AnimalCompanion, false);
                        _whiteList.AddOrUpdate(UnleashTheSkill, false);
                        break;
                    }
                case CClass.MAGE:
                    {

                        if (has2Drop)
                        {
                            if (hasMadScientist)
                                ChoseWeapon(choices, 2, 1);
                            else
                                ChoseWeapon(choices);

                            _whiteList.AddOrUpdate(QuickShot, false);
                            _whiteList.AddOrUpdate(IronbeakOwl, false);
                            _whiteList.AddOrUpdate(AnimalCompanion, false);
                        }
                        if (hasCoin)
                            _whiteList.AddOrUpdate(AnimalCompanion, false);
                        _whiteList.AddOrUpdate(BearTrap, false);//It is a justifiable secret against tempo mages
                        break;
                    }
                case CClass.PALADIN:
                    {
                        if (has2Drop)
                        {
                            if (hasMadScientist)
                                ChoseWeapon(choices, 2, 1);
                            else
                                ChoseWeapon(choices);
                        }
                        _whiteList.AddOrUpdate(Flare, false);
                        _whiteList.AddOrUpdate(UnleashTheSkill, false);
                        if (hasCoin && has2Drop)
                            _whiteList.AddOrUpdate(AnimalCompanion, false);

                        break;
                    }
                case CClass.PRIEST:
                    {
                        _whiteList.AddOrUpdate(KingsElekk, false);
                        if (hasMadScientist)
                            ChoseWeapon(choices, 2, 1);
                        else
                            ChoseWeapon(choices);
                        if (hasCoin)
                            _whiteList.AddOrUpdate(PilotedShredder, false);
                        if (has2Drop)
                        {
                            _whiteList.AddOrUpdate(AnimalCompanion, false);
                            _whiteList.AddOrUpdate(QuickShot, false);
                        }

                        break;
                    }
                case CClass.ROGUE:
                    {
                        if (has2Drop && hasMadScientist)
                            ChoseWeapon(choices, 2, 1);
                        else
                            ChoseWeapon(choices);

                        if (has2Drop)
                            _whiteList.AddOrUpdate(AnimalCompanion, false);
                        break;
                    }
                case CClass.SHAMAN:
                    {
                        if (allowHunterMark)
                            _whiteList.AddOrUpdate(HunterMark, false);
                        ChoseWeapon(choices);
                        if (has2Drop && choices.Any(c => c.Name != Glavezooka))
                            _whiteList.AddOrUpdate(EaglehornBow, false);
                        break;
                    }
                case CClass.WARLOCK:
                    {
                        if (hasMadScientist)
                            ChoseWeapon(choices, 2, 1);
                        else
                            ChoseWeapon(choices);
                        if (hasCoin || has2Drop)
                            _whiteList.AddOrUpdate(AnimalCompanion, false);
                        if (has2Drop)
                            _whiteList.AddOrUpdate(QuickShot, false);

                        break;
                    }
                case CClass.WARRIOR:
                    {
                        _whiteList.AddOrUpdate(IronbeakOwl, false);
                        _whiteList.AddOrUpdate(Wolfrider, false);
                        ChoseWeapon(choices, 2, 1);
                        if (has2Drop || hasCoin)
                            _whiteList.AddOrUpdate(AnimalCompanion, false);
                        if (has2Drop && hasCoin)
                            _whiteList.AddOrUpdate(AnimalCompanion, true);
                        break;
                    }
            }

            foreach (var s in from s in choices let keptOneAlready = _cardsToKeep.Any(c => c.Name == s.Name) where _whiteList.ContainsKey(s.Name) where !keptOneAlready | _whiteList[s.Name] select s)
                _cardsToKeep.Add(s);

            return _cardsToKeep;
        }
        /*Good hand implies that there is a good 1, 2 and 3 drop
         It's hardcoded for hunters only, but it is the same concept behind Auto_Arena*/
        private static Tuple<bool, bool, bool, bool> AnalyzeMyHandTuple(List<Card> choices)
        {
            var has1Drop = choices.Any(c => c.Cost == 1);
            var has2Drop = choices.Any(c => c.Cost == 2);
            var has3Drop = choices.Any(c => c.Cost == 3);
            var has4Drop = choices.Any(c => c.Cost == 4);

            var handStatus = new Tuple<bool, bool, bool, bool>(has1Drop, has2Drop, has3Drop, has4Drop);
            return handStatus; 
            //throw new NotImplementedException();
        }
    }
    
}
