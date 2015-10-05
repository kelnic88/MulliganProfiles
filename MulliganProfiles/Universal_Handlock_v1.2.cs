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
        private static bool demonHandLock = false;

        public static void AddOrUpdate<TKey, TValue>(
     this IDictionary<TKey, TValue> map, TKey key, TValue value)
        {
            map[key] = value;
        }
    }

    [Serializable]
    public class bMulliganProfile : MulliganProfile
    {

        #region cardslist
        private string _abusiveSergeant = "CS2_188";
        private string _ancientWatcher = "EX1_045";
        private string _antiqueHealbot = "GVG_069";
        private string _acidicSwampOoze = "EX1_066";
        private string _bigGameHunter = "EX1_005";
        private string _coin = "GAME_005";
        private string _darkbomb = "GVG_015";
        private string _defenderofArgus = "EX1_093";
        private string _doomguard = "EX1_310";
        private string _drBoom = "GVG_110";
        private string _dreadInfernal = "CS2_064";
        private string _emperorThaurissan = "BRM_028";
        private string _harrisonJones = "EX1_558";
        private string _hellfire = "CS2_062";
        private string _ironbeakOwl = "CS2_203";
        private string _kezanMystic = "GVG_074";
        private string _loatheb = "FP1_030";
        private string _lordJaraxxus = "EX1_323";
        private string _malGanis = "GVG_021";
        private string _moltenGiant = "EX1_620";
        private string _mortalCoil = "EX1_302";
        private string _mountainGiant = "EX1_105";
        private string _shadowflame = "EX1_303";
        private string _sylvanasWindrunner = "CS2_117";
        private string _siphonSoul = "EX1_309";
        private string _sludgeBelcher = "FP1_012";
        private string _sunfuryProtector = "EX1_058";
        private string _twilightDrake = "EX1_043";
        private string _voidCaller = "FP1_022";
        private string _zombieChow = "FP1_001";
        #endregion
        private int[] demonPriority = new int[4];
        private Dictionary<string, int> _demonList;
        private Dictionary<string, bool> _whiteList; // CardName, KeepDouble
        private List<Card> _cardsToKeep;


        public bMulliganProfile()
            : base()
        {

            _whiteList = new Dictionary<string, bool>();
            _demonList = new Dictionary<string, int>();
            _cardsToKeep = new List<Card>();
        }
        /*Method designed to keep only 1 high cost demon in your opener assuming you have a voidcaller in your hand*/
        public void findGoodDemon(List<Card> Choices, int MG, int LJ, int DG, int DI)
        {
            bool hasGoodDemon = false;

            //AssignValues to demons
            _demonList.AddOrUpdate(_dreadInfernal, DI);
            _demonList.AddOrUpdate(_malGanis, MG);
            _demonList.AddOrUpdate(_lordJaraxxus, LJ);
            _demonList.AddOrUpdate(_doomguard, DG);


            if (!hasGoodDemon && (Choices.Any(c => c.Name == _demonList.FirstOrDefault(x => x.Value == 1).Key)))
            {
                _whiteList.AddOrUpdate(_demonList.FirstOrDefault(x => x.Value == 1).Key, false);
                hasGoodDemon = true;
            }
            else if (!hasGoodDemon && (Choices.Any(c => c.Name == _demonList.FirstOrDefault(x => x.Value == 2).Key)))
            {
                _whiteList.AddOrUpdate(_demonList.FirstOrDefault(x => x.Value == 2).Key, false);
                hasGoodDemon = true;
            }
            else if (!hasGoodDemon && (Choices.Any(c => c.Name == _demonList.FirstOrDefault(x => x.Value == 3).Key)))
            {
                _whiteList.AddOrUpdate(_demonList.FirstOrDefault(x => x.Value == 3).Key, false);
                hasGoodDemon = true;
            }
            else if (!hasGoodDemon && (Choices.Any(c => c.Name == _demonList.FirstOrDefault(x => x.Value == 4).Key)))
            {
                _whiteList.AddOrUpdate(_demonList.FirstOrDefault(x => x.Value == 4).Key, false);
                hasGoodDemon = true;
            }


        }

        public override List<Card> HandleMulligan(List<Card> Choices, CClass opponentClass, CClass ownClass)
        {
            bool hasCoin = Choices.Count > 3;
            bool ancientOwl = false;           //Ancient watcher + ironbeak Owl
            bool strongHand = false;           //Giants and Drakes in hand.
            bool goodHand = false;
            bool terribleHand = false;
            bool voidcaller = false;           //
            _whiteList.AddOrUpdate(_coin, true);

            //Strong hand is only used against slow matchups 
            if (Choices.Any(c => c.Name == _mountainGiant) && Choices.Any(c => c.Name == _twilightDrake))
                strongHand = true;
            //only 1 good 4 drop 
            if (Choices.Any(c => c.Name == _mountainGiant) || Choices.Any(c => c.Name == _twilightDrake))
                goodHand = true;
            //Check if Voidcaller is in hands
            if (Choices.Any(c => c.Name == _voidCaller))
                voidcaller = true;
            //Check if there are no good drops in the opening hand
            if (!Choices.Any(c => c.Name == _mountainGiant) && !Choices.Any(c => c.Name == _twilightDrake) && !Choices.Any(c => c.Name == _ancientWatcher) && !Choices.Any(c => c.Name == _sunfuryProtector) && !Choices.Any(c => c.Name == _voidCaller))
            {
                _whiteList.AddOrUpdate(_moltenGiant, true);
                terribleHand = true;
            }
            switch (opponentClass)
            {
                case CClass.DRUID:
                    {
                        _whiteList.AddOrUpdate(_mountainGiant, true);
                        _whiteList.AddOrUpdate(_hellfire, false);
                        _whiteList.AddOrUpdate(_ancientWatcher, false);
                        _whiteList.AddOrUpdate(_defenderofArgus, false);
                        _whiteList.AddOrUpdate(_voidCaller, false);
                        if (hasCoin)
                        {
                            //_whiteList.AddOrUpdate(_voidCaller, false);
                            _whiteList.AddOrUpdate(_twilightDrake, true);
                        }
                        else
                            _whiteList.AddOrUpdate(_twilightDrake, false);

                        if (voidcaller) //not optimized yet
                        {
                            findGoodDemon(Choices, 1, 2, 3, 4); // MG   LJ   DG    DI
                        }
                        break;
                    }
                case CClass.HUNTER:
                    {
                        _whiteList.AddOrUpdate(_zombieChow, true);
                        _whiteList.AddOrUpdate(_mortalCoil, false);
                        _whiteList.AddOrUpdate(_moltenGiant, true);
                        _whiteList.AddOrUpdate(_sunfuryProtector, false);
                        _whiteList.AddOrUpdate(_acidicSwampOoze, true);
                        _whiteList.AddOrUpdate(_ancientWatcher, false);
                        _whiteList.AddOrUpdate(_darkbomb, false);

                        break;
                    }
                case CClass.MAGE:
                    {
                        _whiteList.AddOrUpdate(_mortalCoil, false);
                        _whiteList.AddOrUpdate(_ironbeakOwl, false);
                        _whiteList.AddOrUpdate(_twilightDrake, true);
                        _whiteList.AddOrUpdate(_darkbomb, false);
                        _whiteList.AddOrUpdate(_hellfire, false);
                        _whiteList.AddOrUpdate(_sunfuryProtector, false);
                        _whiteList.AddOrUpdate(_voidCaller, false);
                        if (voidcaller) //not optimized yet
                        {
                            findGoodDemon(Choices, 1, 4, 3, 2);// MG   LJ   DG    DI
                        }
                        _whiteList.AddOrUpdate(_ancientWatcher, false);


                        break;
                    }
                case CClass.PALADIN:
                    {
                        _whiteList.AddOrUpdate(_voidCaller, false);
                        _whiteList.AddOrUpdate(_zombieChow, true);
                        _whiteList.AddOrUpdate(_mortalCoil, false);
                        _whiteList.AddOrUpdate(_twilightDrake, true);
                        _whiteList.AddOrUpdate(_hellfire, false);

                        _whiteList.AddOrUpdate(_moltenGiant, true);
                        _whiteList.AddOrUpdate(_mountainGiant, false);
                        _whiteList.AddOrUpdate(_sunfuryProtector, false);
                        if (voidcaller) //not optimized yet
                        {
                            findGoodDemon(Choices, 1, 4, 3, 2);// MG   LJ   DG    DI
                        }

                        _whiteList.AddOrUpdate(_ancientWatcher, false);
                        break;
                    }
                case CClass.PRIEST:
                    {
                        _whiteList.AddOrUpdate(_voidCaller, false);
                        _whiteList.AddOrUpdate(_mountainGiant, true);
                        _whiteList.AddOrUpdate(_twilightDrake, true);
                        _whiteList.AddOrUpdate(_darkbomb, false);
                        _whiteList.AddOrUpdate(_ironbeakOwl, false);
                        if (hasCoin && strongHand)
                            _whiteList.AddOrUpdate(_emperorThaurissan, false);
                        if (voidcaller) //not optimized yet
                        {
                            findGoodDemon(Choices, 1, 5, 3, 2); // MG   LJ   DG    DI
                        }
                        break;
                    }
                case CClass.ROGUE:
                    {
                        _whiteList.AddOrUpdate(_voidCaller, false);
                        _whiteList.AddOrUpdate(_mountainGiant, true);
                        _whiteList.AddOrUpdate(_twilightDrake, true);
                        _whiteList.AddOrUpdate(_darkbomb, false);
                        if (voidcaller) //not optimized yet
                        {
                            findGoodDemon(Choices, 1, 4, 3, 2);
                        }
                        break;
                    }
                case CClass.SHAMAN:
                    {
                        _whiteList.AddOrUpdate(_voidCaller, false);
                        _whiteList.AddOrUpdate(_zombieChow, true);
                        _whiteList.AddOrUpdate(_ancientWatcher, true);
                        _whiteList.AddOrUpdate(_darkbomb, false);
                        _whiteList.AddOrUpdate(_hellfire, false);
                        _whiteList.AddOrUpdate(_sunfuryProtector, false);
                        _whiteList.AddOrUpdate(_moltenGiant, true);

                        if (voidcaller) //not optimized yet
                        {
                            findGoodDemon(Choices, 1, 2, 3, 4); // MG   LJ   DG    DI
                        }
                        break;
                    }
                case CClass.WARLOCK:
                    {

                        _whiteList.AddOrUpdate(_twilightDrake, true);
                        _whiteList.AddOrUpdate(_darkbomb, false);
                        _whiteList.AddOrUpdate(_hellfire, false);
                        _whiteList.AddOrUpdate(_sunfuryProtector, false);
                        _whiteList.AddOrUpdate(_bigGameHunter, false);
                        _whiteList.AddOrUpdate(_moltenGiant, true);
                        if ((Choices.Any(c => c.Name == _mortalCoil) && Choices.Any(c => c.Name == _ironbeakOwl)))
                        {
                            _whiteList.AddOrUpdate(_ironbeakOwl, false);
                            _whiteList.AddOrUpdate(_mortalCoil, false);
                        }

                        _whiteList.AddOrUpdate(_ancientWatcher, false);

                        break;
                    }
                case CClass.WARRIOR:
                    {
                        _whiteList.AddOrUpdate(_voidCaller, false);
                        _whiteList.AddOrUpdate(_ancientWatcher, true);
                        _whiteList.AddOrUpdate(_darkbomb, false);
                        if (goodHand)
                            _whiteList.AddOrUpdate(_acidicSwampOoze, false);
                        _whiteList.AddOrUpdate(_twilightDrake, true);
                        _whiteList.AddOrUpdate(_mountainGiant, true);
                        _whiteList.AddOrUpdate(_ironbeakOwl, false);
                        if (hasCoin && strongHand)
                            _whiteList.AddOrUpdate(_emperorThaurissan, false);
                        if (voidcaller)
                            findGoodDemon(Choices, 1, 4, 3, 2); // MG   LJ   DG    DI

                        break;
                    }
            }


            foreach (Card s in Choices)
            {
                bool keptOneAlready = false;

                if (_cardsToKeep.Any(c => c.Name == s.Name))
                {
                    keptOneAlready = true;
                }
                if (_whiteList.ContainsKey(s.Name))
                {
                    if (!keptOneAlready | _whiteList[s.Name])
                    {
                        _cardsToKeep.Add(s);
                    }
                }
            }

            return _cardsToKeep;
        }


    }
}