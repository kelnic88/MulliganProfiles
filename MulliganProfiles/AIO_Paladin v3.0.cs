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
        private bool midrangeSecretPaladin = true;                   //Midrange Secret Paladin based mulligan. 
                                                                     //                Keep rush and anti control on false.

        private bool competitiveMustard = false;            //CM     //Keeps Competitive Spirit with Muster for Battle on coin
        private bool vengefulSecretKeeper = false;         //VSK    //Keeps noble sacrifice with avenge when you have a secretkeeper
        private bool nobleJuggler = true;                  //NKJ    //Keeps noble sacrifice with knife juggler
        private bool redeeming2Drops = false;               //R2D    //Keep  redemption with Shielded Minibot, or Harvest Golem
        private bool keepBloodKnight_onCurve = false;      //KBKoC  //Keeps Blood Knight on divine shield curve
        private bool mysteriousChallenger_coin = false;    //MCC    //Keep  Mysterious Challenger on coin
        private bool mysteriousChallenger_forever = false; //MCF    //Always keeps Mysterious Challenger even without coin
        private bool coghammerLogic = true;                //Cog    //Keeps coghammer on curve, but never against warriors

       
        /* Values for reference:***********************************************************************/
        /***********    CM       VSK       NKJ       R2D       KBKoC       MCF       MCC       Cog    *
        /* v2.1.2:      false    false     true      true      false      false     true       false  *
         * v2.2.1:      true     true      true      true      true       false     true       false  *
         * Dr. 6:       false    false     false     false     false      true      false      false  *
         * Wbulot dr6:  false    false     false     false     false      false     false      false  *
         * ThyFate:     true     false     true      true      false      false     true       true   *
         * ********************************************************************************************
         */

       
        private bool rush = false;                                    //WIP Do not set to true
        private bool antiControlBETA = false;               //AC     //Gets rid of early drops that are liability against Control Warriors/Patrons


        /**********************************************************/
        /*==============End of Custom Behavior Logic==============*/
        /*********Do not change anything below this line***********/
        /**********************************************************/

        private string _abusiveSergeant = "CS2_188";
        private string _aldor = "EX1_382";
        private string _annoyatron = "GVG_085";
        private string _argentSquire = "EX1_008";
        private string _avenge = "FP1_020";
        private string _bloodKnight = "EX1_590";
        private string _coin = "GAME_005";
        private string _consecration = "CS2_093";
        private string _competitiveSpirit = "AT_073";
        private string _harvestGolem = "EX1_556";
        private string _hauntedCreeper = "FP1_002";
        private string _ironbeakOwl = "CS2_203";
        private string _knifeJuggler = "NEW1_019";
        private string _leperGnome = "EX1_029";
        private string _madScientist = "FP1_004";
        private string _murlockKnight = "AT_076"; 
        private string _musterForBattle = "GVG_061";
        private string _mysteriousChallenger = "AT_079";
        private string _nobleSacrifice = "EX1_130";
        private string _pilotedShredder = "GVG_096";
        private string _redemption = "EX1_136";
        private string _repentance = "EX1_379";
        private string _secretkeeper = "EX1_080";
        private string _shieldedMinibot = "GVG_058";
        private string _truesilverChamption = "CS2_097";
        private string _coghammer = "GVG_059";
        private string _zombieChow = "FP1_001"; 


        private Dictionary<string, bool> _whiteList; // CardName, KeepDouble
        private List<Card> _cardsToKeep;

        #endregion Data

        #region Constructor

        public bMulliganProfile()
            : base()
        {
            _whiteList = new Dictionary<string, bool>();
            _cardsToKeep = new List<Card>();
        }

        #endregion Constructor

        #region Methods

        public override List<Card> HandleMulligan(List<Card> Choices, CClass opponentClass, CClass ownClass)
        {
            bool hasCoin = Choices.Count > 3;
            bool has2drop = false;
            bool lazyFlag = false;
            #region Default Mulligan

            _whiteList.AddOrUpdate(_abusiveSergeant, false);
            _whiteList.AddOrUpdate(_argentSquire, true);
            _whiteList.AddOrUpdate(_coin, true); // Would be nice to keep double
            if (!antiControlBETA || (opponentClass != CClass.WARRIOR))
                _whiteList.AddOrUpdate(_hauntedCreeper, true);
            _whiteList.AddOrUpdate(_knifeJuggler, false);
            _whiteList.AddOrUpdate(_leperGnome, true);
            _whiteList.AddOrUpdate(_madScientist, true);
            _whiteList.AddOrUpdate(_secretkeeper, true);
            _whiteList.AddOrUpdate(_shieldedMinibot, true);
            _whiteList.AddOrUpdate(_zombieChow, true);



            #endregion Default Mulligan

            #region Class Specific Mulligan
            if ((Choices.Any(c => c.Name == _harvestGolem) ||
                 Choices.Any(c => c.Name == _shieldedMinibot) ||
                 Choices.Any(c => c.Name == _madScientist) ||
                 Choices.Any(c => c.Name == _knifeJuggler)
               ))
            {
                has2drop = true;
            }
            if (midrangeSecretPaladin && hasCoin && has2drop)
                _whiteList.AddOrUpdate(_pilotedShredder, false);
            else if (midrangeSecretPaladin && (hasCoin || has2drop))
                _whiteList.AddOrUpdate(_musterForBattle, false);

            if (midrangeSecretPaladin)
                _whiteList.AddOrUpdate(_avenge, false);
            if (midrangeSecretPaladin && ((opponentClass == CClass.HUNTER) || (opponentClass == CClass.WARLOCK)))
            {
                _whiteList.AddOrUpdate(_aldor, false);
                _whiteList.AddOrUpdate(_ironbeakOwl, false);
            }

            switch (opponentClass)
            {
                case CClass.DRUID:
                    {
                        if (hasCoin)
                            lazyFlag = true;
                        _whiteList.AddOrUpdate(_musterForBattle, false);
                        if (!midrangeSecretPaladin)
                            _whiteList.AddOrUpdate(_pilotedShredder, false);
                        
                        break;
                    }
                case CClass.HUNTER:
                    {
                        if (coghammerLogic && has2drop)
                            _whiteList.AddOrUpdate(_coghammer, false);
                        _whiteList.AddOrUpdate(_annoyatron, false);
                        if (midrangeSecretPaladin && hasCoin)
                            _whiteList.AddOrUpdate(_consecration, false);
                        else if (!midrangeSecretPaladin)
                            _whiteList.AddOrUpdate(_consecration, false);
                        break;
                    }
                case CClass.MAGE:
                    {
                        if (hasCoin)
                            lazyFlag = true;
                        _whiteList.AddOrUpdate(_musterForBattle, false);
                        if (!midrangeSecretPaladin)
                            _whiteList.AddOrUpdate(_mysteriousChallenger, false);
                        else
                            _whiteList.AddOrUpdate(_nobleSacrifice, false);

                        if (midrangeSecretPaladin && hasCoin)
                            _whiteList.AddOrUpdate(_consecration, false);
                        else if (!midrangeSecretPaladin)
                            _whiteList.AddOrUpdate(_consecration, false);
                        break;
                    }
                case CClass.PALADIN:
                    {
                        if ((hasCoin || has2drop)&&!midrangeSecretPaladin)
                        {
                            _whiteList.AddOrUpdate(_bloodKnight, false);
                            _whiteList.AddOrUpdate(_ironbeakOwl, false);
                        }
                        _whiteList.AddOrUpdate(_annoyatron, false);
                        _whiteList.AddOrUpdate(_consecration, false);
                        
                        if (midrangeSecretPaladin && hasCoin)
                            _whiteList.AddOrUpdate(_consecration, false);
                        else if (!midrangeSecretPaladin)
                            _whiteList.AddOrUpdate(_consecration, false);

                        _whiteList.AddOrUpdate(_musterForBattle, false);
                        if (!midrangeSecretPaladin)
                            _whiteList.AddOrUpdate(_mysteriousChallenger, false);
                        break;
                    }
                case CClass.PRIEST:
                    {
                        if (!midrangeSecretPaladin)
                            _whiteList.AddOrUpdate(_mysteriousChallenger, false);
                        _whiteList.AddOrUpdate(_pilotedShredder, false);
                        
                        break;
                    }
                case CClass.ROGUE:
                    {
                        if (hasCoin)
                            lazyFlag = true;
                        _whiteList.AddOrUpdate(_musterForBattle, false);
                        if (!midrangeSecretPaladin)
                            _whiteList.AddOrUpdate(_mysteriousChallenger, false);
                        _whiteList.AddOrUpdate(_pilotedShredder, false);
                        
                        break;
                    }
                case CClass.SHAMAN:
                    {
                        _whiteList.AddOrUpdate(_musterForBattle, false);
                        if (!midrangeSecretPaladin)
                            _whiteList.AddOrUpdate(_mysteriousChallenger, false);
                        _whiteList.AddOrUpdate(_pilotedShredder, false);
                        break;
                    }
                case CClass.WARLOCK:
                    {
                        _whiteList.AddOrUpdate(_consecration, false);
                        _whiteList.AddOrUpdate(_musterForBattle, false);
                        break;
                    }
                case CClass.WARRIOR:
                    {
                        _whiteList.AddOrUpdate(_annoyatron, false);
                        _whiteList.AddOrUpdate(_musterForBattle, false);
                        if (!midrangeSecretPaladin && hasCoin)
                            _whiteList.AddOrUpdate(_mysteriousChallenger, false);

                        if(antiControlBETA && hasCoin)
                             _whiteList.AddOrUpdate(_pilotedShredder, true);
                        else 
                            _whiteList.AddOrUpdate(_pilotedShredder, false);
                        
                        break;
                    }
            }
            if (mysteriousChallenger_forever)
                _whiteList.AddOrUpdate(_mysteriousChallenger, false);

            if ((Choices.Any(c => c.Name == _coghammer) && (coghammerLogic && has2drop) && (opponentClass != CClass.WARRIOR)))
                _whiteList.AddOrUpdate(_coghammer, false);
            else if((opponentClass == CClass.WARRIOR) || (opponentClass == CClass.PRIEST) ||(opponentClass == CClass.ROGUE) || (opponentClass == CClass.DRUID))
                _whiteList.AddOrUpdate(_truesilverChamption, false);

            if ((competitiveMustard && lazyFlag) && (Choices.Any(c => c.Name == _competitiveSpirit) && Choices.Any(c => c.Name == _musterForBattle)))
            {
                _whiteList.AddOrUpdate(_musterForBattle, false);
                _whiteList.AddOrUpdate(_competitiveSpirit, false);
            }

            //Keep Mysterious Challenger on coin
            if (hasCoin && !midrangeSecretPaladin)
            {
                _whiteList.AddOrUpdate(_annoyatron, false);
                _whiteList.AddOrUpdate(_mysteriousChallenger, false);
            }

            // Redemption and Harvest Golem are kept if you have both.
            if (redeeming2Drops &&
               (Choices.Any(c => c.Name == _harvestGolem) ||
                Choices.Any(c => c.Name == _shieldedMinibot)
                ))
            {
                _whiteList.AddOrUpdate(_harvestGolem, false);
                _whiteList.AddOrUpdate(_redemption, false);
                // has2drop = true;
            }


            // Noble Sacrifice is kept if you have Knife Juggler.
            if (nobleJuggler && (Choices.Any(c => c.Name == _knifeJuggler)))
            {
                _whiteList.AddOrUpdate(_nobleSacrifice, false);
                //has2drop = true;
            }
            // Tech choice with blood knight
            if (keepBloodKnight_onCurve &&
                (Choices.Any(c => c.Name == _argentSquire) &&
                (Choices.Any(c => c.Name == _shieldedMinibot) ||
                Choices.Any(c => c.Name == _annoyatron))))
            {
                _whiteList.AddOrUpdate(_annoyatron, false);
                _whiteList.AddOrUpdate(_bloodKnight, false);
            }
            //Experimental segment that keeps noble sac and avenge with secret keeper. 
            if (vengefulSecretKeeper &&
                (Choices.Any(c => c.Name == _avenge) &&
                (Choices.Any(c => c.Name == _secretkeeper) &&
                 Choices.Any(c => c.Name == _nobleSacrifice))))
            {
                _whiteList.AddOrUpdate(_nobleSacrifice, false);
                _whiteList.AddOrUpdate(_avenge, false);
            }


            #endregion

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

        #endregion Methods
    }
}