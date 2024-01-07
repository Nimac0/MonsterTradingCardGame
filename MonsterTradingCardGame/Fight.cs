using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using MonsterTradingCardGame.Db;
using MonsterTradingCardGame.Schemas;

namespace MonsterTradingCardGame
{
    public class Fight
    {
        public DbQuery dbQuery = new DbQuery();
        public enum fightResult
        {
            WIN = 0,
            LOSE,
            DRAW
        }

        private User _player;
        private User _enemy;
        private int _round_counter = 0;
        private static Random rng = new Random();
        private List<string> _battleLog = new List<string>();

        public Fight(User player, User enemy)
        {
            this._player = player;
            this._enemy = enemy;
        }

        public List<string> StartFight()
        {

            do
            {
                if (!this._enemy.PlayingDeck.Any() || !this._player.PlayingDeck.Any())
                {
                    User winner = !this._enemy.PlayingDeck.Any() ? _player : _enemy;
                    User loser = !this._player.PlayingDeck.Any() ? _player : _enemy;
                    _battleLog.Add(winner.Name + " WINS!");
                    if (!HandleEndOfBattle(winner.Id, loser.Id, pickCard(winner.CardStack))) return null;
                    
                    return _battleLog;
                }

                this._round_counter++;

                _battleLog.Add("---[ROUND " + _round_counter + "]---");

                Card currPlayerCard = this.pickCard(this._player.PlayingDeck);
                Card currEnemyCard = this.pickCard(this._enemy.PlayingDeck);
                Card? WinnerCard = this.Attack(currPlayerCard, currEnemyCard);

                if (WinnerCard == null) continue;

                if(WinnerCard == currPlayerCard)
                {
                    _battleLog.Add(currPlayerCard.Name + " beats " + currEnemyCard.Name + ".");
                    this._player.CardStack.Add(this._enemy.PlayingDeck.First()); //take losers card to cardstack
                    this._enemy.PlayingDeck.RemoveAt(0);
                    continue;
                }
                _battleLog.Add(currEnemyCard.Name + " beats " + currPlayerCard.Name + ".");
                this._enemy.CardStack.Add(this._player.PlayingDeck.First());
                this._player.PlayingDeck.RemoveAt(0);
                continue;

            } while (this._round_counter < 100);

            _battleLog.Add("DRAW");
            return _battleLog;
        }

        public Card pickCard(List<Card> deck)
        {
            //https://stackoverflow.com/questions/273313/randomize-a-listt
            int n = deck.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                Card value = deck[k];
                deck[k] = deck[n];
                deck[n] = value;
            }
            return deck.First();
        }

        public Card? Attack(Card playerCard, Card enemyCard)
        {
            float dmgMultiplier = 1;

            if (playerCard.Type == CardType.SPELL || enemyCard.Type == CardType.SPELL)
            {
                dmgMultiplier = getMultiplier(playerCard.CardElement, enemyCard.CardElement);
            }
            if (getImmunity(playerCard, enemyCard))
            {
                _battleLog.Add(playerCard.Name + " is immune.");
                return playerCard;
            }
            if (getImmunity(enemyCard, playerCard))
            {
                _battleLog.Add(enemyCard.Name + " is immune.");
                return enemyCard;
            } 

            return (playerCard.Damage * dmgMultiplier) > (enemyCard.Damage / dmgMultiplier) ?
                    playerCard : (playerCard.Damage * dmgMultiplier) < (enemyCard.Damage / dmgMultiplier) ?
                        enemyCard : null;
        }

        public float getMultiplier(Element playerElement, Element enemyElement)
        {
            if (playerElement == enemyElement) return 1;
            switch(playerElement)
            {
                case Element.WATER when enemyElement == Element.FIRE:
                case Element.FIRE when enemyElement == Element.NORMAL:
                case Element.NORMAL when enemyElement == Element.WATER:
                    return 2;
            }
            return 0.5f;
        }
        
        public bool getImmunity(Card playerCard, Card enemyCard)
        {
            switch(playerCard.Type)
            {
                case CardType.DRAGON when enemyCard.Type == CardType.GOBLIN:
                case CardType.WIZZARD when enemyCard.Type == CardType.ORK:
                case CardType.SPELL when playerCard.CardElement == Element.WATER && enemyCard.Type == CardType.KNIGHT:
                case CardType.KRAKEN when enemyCard.Type == CardType.SPELL:
                case CardType.ELF when playerCard.CardElement == Element.FIRE && enemyCard.Type == CardType.DRAGON:
                    return true;
            }
            return false;
        }

        public bool HandleEndOfBattle(int? winnerId, int? loserId, Card wonCard)
        {
            DbQuery updateWinnerStats = this.dbQuery.NewCommand(@"UPDATE users SET elo = elo + 5, wins = wins + 1 WHERE id = winnerId;");
            updateWinnerStats.AddParameterWithValue("winnerid", DbType.Int32, winnerId);
            if(updateWinnerStats.ExecuteNonQuery() == 0) return false;
            
            DbQuery updateLoserStats = this.dbQuery.NewCommand(@"UPDATE users SET elo = elo - 3, losses = losses + 1, coins = coins + 1 WHERE id = loserId;");
            updateLoserStats.AddParameterWithValue("loserid", DbType.Int32, loserId);
            if(updateLoserStats.ExecuteNonQuery() == 0) return false;
            
            DbQuery addWonCard = this.dbQuery.NewCommand(@"UPDATE cards SET userid = @userid, indeck = FALSE WHERE id = @cardid;");
            addWonCard.AddParameterWithValue("userid", DbType.Int32, winnerId);
            addWonCard.AddParameterWithValue("cardid", DbType.Int32, wonCard.Id);
            if(addWonCard.ExecuteNonQuery() == 0) return false;
            return true;
        }
    }
}
