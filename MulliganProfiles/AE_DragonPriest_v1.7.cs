using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SmartBotUI;
using SmartBotUI.Settings;
/*
 * Dragon Priest from TomVicious 
 * Main Contributors: Arthur && Zephery
 * 
 * v1.4 Arthur's Edition
 *      Massive Cleanup
 *      Dragon Logic
 */
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

        private string _azureDrake = "EX1_284";
        private string _blackwingCorruptor = "BRM_034";
        private string _blackwingTechnician = "BRM_033";
        private string _cabalShadowPriest = "EX1_091";
        private string _chillmaw = "AT_123";
        private string _chromaggus = "BRM_031";
        private string _coin = "GAME_005";
        private string _darkCultist = "FP1_023";
        private string _dragonkinSorcerer = "BRM_020";
        private string _holyNova = "CS1_112";
        private string _nefarian = "BRM_030";
        private string _northshireCleric = "CS2_235";
        private string _powerWordShield = "CS2_004";
        private string _shadowWordDeath = "EX1_622";
        private string _shadowWordPain = "CS2_234";
        private string _shrinkmeister = "GVG_011";
        private string _twilightGuardian = "AT_017"; //This value was soooooooooooo off that I nearly broke my screen lol 
        private string _twilightWhelp = "BRM_004";
        private string _velensChosen = "GVG_010";
        private string _wildPyromancer = "NEW1_020"; 
        private string _wyrmrestAgent = "AT_116";
        private string _ysera = "EX1_572";


        private Dictionary<string, bool> _whiteList; // CardName, KeepDouble
        private Dictionary<string, int> _dragonList; // This will be used to hunt for high cost dragons to activate synergies with the deck
        private List<Card> _cardsToKeep;

        #endregion Data

        #region Constructor

        public bMulliganProfile()
            : base()
        {
            _whiteList = new Dictionary<string, bool>();
            _dragonList = new Dictionary<string, int>();
            _cardsToKeep = new List<Card>();
        }

        #endregion Constructor

        #region Methods
        /* This method will search for high cost dragons in the mulligan and allow their keep if you have early
         * level dragon Synergy
         * DSS = DragonSkin Sorcerer
         * TG = Twighlight Guardian
         * AZD = Azure Drake
         * CHIL =  Chillmaw
         * YSR = Ysera
         * Nef = Nefarian 
         * 
         * How to effectively call this method?
         * Dragons have different value points when it comes to matchups. Against Control I wouldn't mind having Azure Drake over Twighlight Guardian
         * since I am in no immediate threat, however against lovely paladins/hunters/mech mages, I would much rather keep twitghlight guardian. 
         * To achieve that behavior one must call findDragonSynergy() and give appropriate priority to dragons ranging [1,6] 
         * Example: 
         * findDragonSynergy(Choices, 3, 1, 5, 2, 6, 7) will asign values to dragons in the following order 
         * ************************
         * DragonSkin Sorcerer || 3
         * Twighlight Guardian || 1
         * Azure Drake         || 5
         * Chillmaw            || 2
         * Ysera               || 6
         * Nefarian            || 7
         * ************************
         * If you have early minions like welp and wyrmest agent, you want an activator for them, this method will
         * try to keep only 1 high cost dragon to activate your early drop minions.
         * In this case if you hand cosist of Twighlight Whelp, Wyrmest agent, DragonSkin Sorcerer and Chillmaw.
         * This method will keep DragonSkin Sorcerer for as an activator. 
         * 
         * In short, it keeps dragon with the lowest key to activate our minions. 
         */
        public void findDragonSynergy(List<Card> Choices, int DDC, int TG, int AZD, int CHIL, int YSR, int NEF, int CHR) //v2.0
        {
            bool kiblerDragonActivator = false;
            _dragonList.AddOrUpdate(_dragonkinSorcerer, DDC);
            _dragonList.AddOrUpdate(_twilightGuardian, TG);
            _dragonList.AddOrUpdate(_azureDrake, AZD);
            _dragonList.AddOrUpdate(_chillmaw, CHIL);
            _dragonList.AddOrUpdate(_ysera, YSR);
            _dragonList.AddOrUpdate(_nefarian, NEF);
            _dragonList.AddOrUpdate(_chromaggus, CHR);

            /* 
             * Yes, the proper way would be is to itterate through and find a min value,
             * but I figured since we know our decks, hardcoding values isn't half bad :3 
             * Feel free to sort dragonList dictionary and whitelist dragons in proper order to support more dragons later on
             */
            int i = 0;
            while (true)
            {
                
                if (Choices.Any(c => c.Name == _dragonList.FirstOrDefault(x => x.Value == i).Key))
                {
                    _whiteList.AddOrUpdate(_dragonList.FirstOrDefault(x => x.Value == i).Key, false);
                    break;
                }
                i++;
                if (i > 7) //moved it from while condition
                    break; //Doesn't bark anymore
            }

            

        }
        /****************************************************************************************************************************/
        /****************************************************END OF DRAGON HUNT/*****************************************************/
        /****************************************************************************************************************************/


        public override List<Card> HandleMulligan(List<Card> Choices, CClass opponentClass, CClass ownClass)
        {
            bool hasCoin = Choices.Count > 3;
            _whiteList.AddOrUpdate(_coin, true);

            /*hasDragonMinions includes early drops that have dragon synergy such as:
             */
            bool hasDragonMinions = false; // Minions that require dragon to shine
            bool hasEarlyCurve = false; // 1 and 2 drop
            bool activator = false; //Activator Dragon

            #region Default Mulligan

            /*********************************************/
            /*Arthur note:                               */
            /* I believe these are the core              */
            /* cards regardless of the matchup           */
            /*********************************************/
            _whiteList.AddOrUpdate(_twilightWhelp, true);
            _whiteList.AddOrUpdate(_wyrmrestAgent, false);

            if ((opponentClass != CClass.WARRIOR) && hasCoin) //can be argued that it's fine since you both will just draw cards until one of you fatigues
                _whiteList.AddOrUpdate(_northshireCleric, false);

            if (hasCoin)
            {
                _whiteList.AddOrUpdate(_northshireCleric, false);
                _whiteList.AddOrUpdate(_powerWordShield, false);
                _whiteList.AddOrUpdate(_darkCultist, false);
            }
            /*********************************************/
            /*************EndCore*************************/
            /*********************************************/
            

            //Check early dragon synergy minions
            if (Choices.Any(c => c.Name == _twilightWhelp) || Choices.Any(c => c.Name == _wyrmrestAgent))
                hasDragonMinions = true;

            if (Choices.Any(c => c.Name == _twilightWhelp) && Choices.Any(c => c.Name == _wyrmrestAgent))
                hasEarlyCurve = true;

            if (hasEarlyCurve && hasDragonMinions && (Choices.Any(c => c.Name == _dragonkinSorcerer) || Choices.Any(c => c.Name == _twilightGuardian) || Choices.Any(c => c.Name == _azureDrake) ||
                Choices.Any(c => c.Name == _chillmaw) || Choices.Any(c => c.Name == _ysera) || Choices.Any(c => c.Name == _nefarian) || Choices.Any(c => c.Name == _chromaggus)))
                activator = true; //This is used in case we want to whitelist blackwing technician, or corruptor

            // check to see if you have 2 whelps in your opener
            if (Choices.Count(g => g.Name == _twilightWhelp) == 2) //2 of the cards are whelps 
                activator = true; //The idea behind this case is keeping 1 whelp in hand for activator purpse. Not sure if bot actually does that.

            if (activator)
                _whiteList.AddOrUpdate(_wyrmrestAgent, true);
            
            #endregion Default Mulligan

            #region Class Specific Mulligan

            switch (opponentClass)
            {

                /* findDragonSynergy(Choices, 3, 2, 1, 4, 5, 6);
                 * Numbers in order are
                 * DSS = DragonSkin Sorcerer
                 * TG = Twighlight Guardian
                 * AZD = Azure Drake
                 * CHIL =  Chillmaw
                 * YSR = Ysera
                 * Nef = Nefarian 
                 */
                case CClass.DRUID:
                    {
                        if (hasDragonMinions)// could be optimized since keeping whelp and ysera might not be bringt if you go first. But then again, it's rng and that case is super rare
                            findDragonSynergy(Choices, 3, 1, 2, 4, 5, 6, 7);

                        if (activator && hasCoin)//stron turn 3 play or turn 2 with coin assuming you can activate the anti shredder
                            _whiteList.AddOrUpdate(_blackwingTechnician, false);

                        if (hasCoin && hasEarlyCurve)//Druids have a hard time dealing with fat minion and a good board
                            _whiteList.AddOrUpdate(_velensChosen, false);

                        break;
                    }
                case CClass.HUNTER:
                    {
                        //Not sure if I should keep pain if I only have a whelp
                        _whiteList.AddOrUpdate(_northshireCleric, false);
                        _whiteList.AddOrUpdate(_shrinkmeister, false);
                        _whiteList.AddOrUpdate(_wildPyromancer, false);
                        if (hasEarlyCurve)
                        {
                            _whiteList.AddOrUpdate(_darkCultist, false);
                            _whiteList.AddOrUpdate(_blackwingTechnician, false);
                        }
                        if (hasDragonMinions || activator)
                        {
                            _whiteList.AddOrUpdate(_shadowWordPain, false);
                            findDragonSynergy(Choices, 2, 1, 3, 4, 5, 6, 7);//Guardian has the priority
                        }
                        else if (hasCoin)
                            _whiteList.AddOrUpdate(_shadowWordPain, false);

                        break;
                    }
                case CClass.MAGE: //I didn't think much through pain logic with mages
                    //Major assumptions is that you are facing mechs. Although logic for freeze mages isn't that far off
                    {
                        if (hasEarlyCurve) // 1 and 2 dragon synergy drop
                        {
                            _whiteList.AddOrUpdate(_darkCultist, false);
                            _whiteList.AddOrUpdate(_blackwingTechnician, false);
                        }
                        if (hasDragonMinions && activator)
                            _whiteList.AddOrUpdate(_shadowWordPain, false);
                        else if (hasCoin)
                            _whiteList.AddOrUpdate(_shadowWordPain, false);

                        if (hasDragonMinions) //Assumption is that it's a mech mage, that is why Guardian has a higher priority
                            findDragonSynergy(Choices, 2, 1, 3, 4, 5, 6, 7); // TG > DSS > Azure

                        break;
                    }
                case CClass.PALADIN:
                    {
                        _whiteList.AddOrUpdate(_wildPyromancer, false);
                        _whiteList.AddOrUpdate(_shadowWordPain, false);
                        _whiteList.AddOrUpdate(_northshireCleric, false);
                        _whiteList.AddOrUpdate(_shrinkmeister, false);
                        if (hasDragonMinions && hasCoin)
                        {
                            findDragonSynergy(Choices, 2, 1, 3, 4, 5, 6, 7); // TG > DSS > Azure
                        }
                        break;
                    }

                //************Everything below this line, I haven't put much thought to it***********************
                case CClass.PRIEST:
                    {   //You could argue that pain is good, but I'd rather hunt for early minions, since dragon priest is a tempo control

                        //_whiteList.AddOrUpdate(_shadowWordPain, false);
                        if (hasDragonMinions)
                            findDragonSynergy(Choices, 2, 1, 3, 4, 5, 6, 7);

                        if (activator && hasCoin)
                            _whiteList.AddOrUpdate(_blackwingTechnician, false);
                        if (hasCoin && hasEarlyCurve)
                            _whiteList.AddOrUpdate(_velensChosen, false);
                        break;
                    }
                case CClass.ROGUE:
                    { //I have no idea, I saw 34 rogues in my stats in over 1000 games this month. Class is dead, pick whatever lol 
                        if (hasDragonMinions)
                            findDragonSynergy(Choices, 2, 1, 3, 4, 5, 6, 7);

                        if (activator && hasCoin)
                            _whiteList.AddOrUpdate(_blackwingTechnician, false);

                        if (hasCoin && hasEarlyCurve)
                            _whiteList.AddOrUpdate(_velensChosen, false);
                        break;
                    }
                case CClass.SHAMAN:
                    {
                        //86% winrate over 50 games. Class is as dead as rogue. Pick whatever
                        if (hasDragonMinions)
                            findDragonSynergy(Choices, 2, 1, 3, 4, 5, 6, 7);

                        if (activator && hasCoin)
                            _whiteList.AddOrUpdate(_blackwingTechnician, false);

                        if (hasCoin && hasEarlyCurve)
                            _whiteList.AddOrUpdate(_velensChosen, false);
                        break;
                    }
                case CClass.WARLOCK:
                    {   //Major assumption here is that it's a demon handlock because post 5 I've seen 0 zoo decks. 
                        //So I decided that early tempo is better than removal like pain. 

                        if (hasDragonMinions)
                            findDragonSynergy(Choices, 2, 1, 3, 4, 5, 6, 7);

                        if (activator && hasCoin)
                            _whiteList.AddOrUpdate(_blackwingTechnician, false);

                        if (hasCoin && hasEarlyCurve)
                            _whiteList.AddOrUpdate(_velensChosen, false);
                        break;
                    }
                case CClass.WARRIOR:
                    {
                        _whiteList.AddOrUpdate(_shadowWordPain, false); //Screw acolytes
                        if (hasDragonMinions)
                            findDragonSynergy(Choices, 2, 1, 3, 4, 5, 6, 7);

                        if (activator && hasCoin)
                            _whiteList.AddOrUpdate(_blackwingTechnician, false);
                        if (hasCoin && hasEarlyCurve)
                            _whiteList.AddOrUpdate(_velensChosen, false);
                        break;
                    }
            }

            #endregion

            foreach (var s in from s in Choices let keptOneAlready = _cardsToKeep.Any(c => c.Name == s.Name) where _whiteList.ContainsKey(s.Name) where !keptOneAlready | _whiteList[s.Name] select s)
                _cardsToKeep.Add(s);


            return _cardsToKeep;
        }

        #endregion Methods
    }
}