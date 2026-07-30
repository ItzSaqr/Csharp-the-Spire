using CardGame.Cards;
using CardGame.Char;
using CardGame.Enemies;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardGame.CombatNamespace
{
    public enum CombatState
    {
        PlayerTurn,
        EnemyTurn,
        Victory,
        Defeat
    }

    public enum CombatType
    {
        Basic,
        Elite,
        Boss
    }

    public class Combat
    {
        private readonly IUserInterface ui;

        public Player Player { get; }
        public Enemy Enemy { get; }

        public CombatState State { get; private set; }

        public Combat(Player player, Enemy enemy, IUserInterface ui)
        {
            Player = player;
            Enemy = enemy;
            this.ui = ui;

            State = CombatState.PlayerTurn;

            player.OnCombatStart();

            foreach (var passive in player.Passives) passive.OnCombatStart(player, this);
            foreach (var passive in Enemy.Passives) passive.OnCombatStart(Enemy, this);

            player.MoveInnateCardsOnTop();
            StartPlayerTurn();
        }

        public void PlayCard(Card card)
        {
            if (State != CombatState.PlayerTurn) return;
            if (!Player.Hand.Contains(card)) return;
            if (Player.Energy < card.Cost) return;

            Player.Energy -= card.Cost;

            Player.Hand.Remove(card);

            foreach (var passive in Player.Passives) passive.OnBeforeCardPlayed(Player, this, card);
            foreach (var passive in Enemy.Passives) passive.OnBeforeCardPlayed(Player, this, card);

            card.Play(Player, Enemy, this);

            foreach (var passive in Player.Passives) passive.OnCardPlayed(Player, this, card);
            foreach (var passive in Enemy.Passives) passive.OnCardPlayed(Player, this, card);


            if (card.Type == CardType.Power) { }
            else if (card.Exhaust) Player.ExhaustPile.Add(card);
            else Player.DiscardPile.Add(card);

            CheckEndCombat();
        }

        public void DiscardFromHand(int amount)
        {
            var cards = ui.ChooseCards(Player, Player.Hand, amount);
            Player.DiscardSelected(cards);
        }

        public void StartPlayerTurn()
        {
            State = CombatState.PlayerTurn;

            Player.Block = 0;
            Player.Energy = Player.MaxEnergy;

            foreach (var passive in Player.Passives) passive.OnTurnStart(Player, this);

            Player.DrawCards(5);
        }

        public void EndPlayerTurn()
        {
            if (State != CombatState.PlayerTurn) return;

            Player.DiscardHand();
            foreach (var passive in Player.Passives) passive.OnTurnEnd(Player, this);
            Player.OnTurnEnd();

            CheckEndCombat();
            if (State != CombatState.PlayerTurn) return;

            StartEnemyTurn();
        }

        public void StartEnemyTurn()
        {
            State = CombatState.EnemyTurn;

            CheckEndCombat();

            Enemy.ExecuteIntent(Player, this);

            CheckEndCombat();

            if (State == CombatState.EnemyTurn)
            {
                Enemy.OnTurnEnd();
                Enemy.ChooseIntent();
                StartPlayerTurn();
            }
        }

        private void CheckEndCombat()
        {
            if (Enemy.Hp <= 0) State = CombatState.Victory;
            else if (Player.Hp <= 0) State = CombatState.Defeat;
        }

        public void DealDamage(Character source, Character target, int amount, Card? card)
        {
            amount = source.ModifyOutgoingDamage(amount);
            amount = target.ModifyIncomingDamage(amount);

            foreach (var passive in source.Passives)
                amount = passive.ModifyDamage(source, target, card, amount);

            target.TakeDamage(amount);
        }
    }
}
