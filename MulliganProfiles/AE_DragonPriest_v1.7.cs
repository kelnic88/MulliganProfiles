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

        private const string Coin = "GAME_005";


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

       
        public override List<Card> HandleMulligan(List<Card> Choices, CClass opponentClass, CClass ownClass)
        {
            var hasCoin = Choices.Count > 3;
            _whiteList.AddOrUpdate(Coin, true);

            /*hasDragonMinions includes early drops that have dragon synergy such as:
             */
           

            #region Class Specific Mulligan

            switch (opponentClass)
            {

                
                case CClass.DRUID:
                    {
                        
                        break;
                    }
                case CClass.HUNTER:
                    {
                       
                        break;
                    }
                case CClass.MAGE: //I didn't think much through pain logic with mages
                    //Major assumptions is that you are facing mechs. Although logic for freeze mages isn't that far off
                    {
                       
                        break;
                    }
                case CClass.PALADIN:
                    {
                       
                        break;
                    }

                //************Everything below this line, I haven't put much thought to it***********************
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
                       
                        break;
                    }
                case CClass.WARLOCK:
                    {   
                        break;
                    }
                case CClass.WARRIOR:
                    {
                       
                        break;
                    }
            }

            #endregion

            foreach (var s in from s in Choices let keptOneAlready = _cardsToKeep.Any(c => c.Name == s.Name) where _whiteList.ContainsKey(s.Name) where !keptOneAlready | _whiteList[s.Name] select s)
                _cardsToKeep.Add(s);


            return _cardsToKeep;
        }

      
    }
}