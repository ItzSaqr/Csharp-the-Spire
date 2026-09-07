using CardGame.Cards;
using CardGame.Passives;
using CardGame.Rewards;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardGame.Char
{
    public class Character
    {
        public int Hp;
        public int MaxHp;
        public int Block;
        public int Vulnerable;
        public int Weak;
        public int Strength;
        public int Dexterity;
        public int Poison;
        public List<PassiveEffect> Passives = new();

        public void TakeDamage(int amount)
        {
            int blocked = Math.Min(Block, amount);
            Block -= blocked;
            Hp -= amount - blocked;
        }

        public void Heal(int amount)
        {
            Hp += amount;
            if (Hp > MaxHp) Hp = MaxHp;
        }

        public void TakeDirectDamage(int amount)
        {
            Hp -= amount;
        }

        public void GainBlock(int amount)
        {
            Block += (amount + Dexterity);
        }

        public int ModifyOutgoingDamage(int damage)
        {
            damage += Strength;
            if (Weak > 0) damage = (int)(damage * 0.75);
            return damage;
        }

        public int ModifyIncomingDamage(int damage)
        {
            if (Vulnerable > 0) damage = (int)(damage * 1.5);
            return damage;
        }

        public int MultiplyDamage(int damage, int mult)
        {
            return (int)(damage * mult);
        }

        public void ApplyWeak(int amount)
        {
            Weak += amount;
        }

        public void ApplyVulnerable(int amount)
        {
            Vulnerable += amount;
        }

        public void ApplyPoison(int amount)
        {
            Poison += amount;
        }

        public void ApplyStrength(int amount)
        {
            Strength += amount;
        }

        public void ApplyDexterity(int amount)
        {
            Dexterity += amount;
        }

        public void OnTurnEnd()
        {
            if (Weak > 0) Weak--;
            if (Vulnerable > 0) Vulnerable--;
            if (Poison > 0)
            {
                TakeDirectDamage(Poison);
                Poison--;
            }
        }
        public string GetStatusText()
        {
            var parts = new List<string>();

            if (Block > 0) parts.Add($"Block {Block}");
            if (Weak > 0) parts.Add($"Weak {Weak}");
            if (Vulnerable > 0) parts.Add($"Vulnerable {Vulnerable}");
            if (Strength != 0) parts.Add($"Strength {Strength}");
            if (Dexterity != 0) parts.Add($"Dexterity {Dexterity}");
            if (Poison != 0) parts.Add($"Poison {Poison}");

            for (int i = 0; i < Passives.Count; i++) parts.Add($"{Passives[i].Name} | {Passives[i].Description}");

            return parts.Count == 0 ? "No statuses" : string.Join(", ", parts);
        }
    }

    public class Player : Character
    {
        public int Energy;
        public int MaxEnergy;
        public int Gold;
        public int HandSize = 10;

        public List<Card> Deck = new();
        public List<Card> DrawPile = new();
        public List<Card> Hand = new();
        public List<Card> DiscardPile = new();
        public List<Card> ExhaustPile = new();

        public void Reshuffle()
        {
            DrawPile.AddRange(DiscardPile);
            DiscardPile.Clear();
            DrawPile = DrawPile.OrderBy(x => Random.Shared.Next()).ToList();
        }

        public void DrawCards(int amount)
        {
            for (int i = 0; i < amount; i++)
            {
                if (DrawPile.Count == 0)
                    Reshuffle();

                if (DrawPile.Count == 0)
                    return;

                if (Hand.Count >= HandSize)
                    return;

                Card card = DrawPile[0];
                DrawPile.RemoveAt(0);
                Hand.Add(card);
            }
        }

        public void DiscardRandom(int amount)
        {
            Random random = new Random();
            for (int i = 0; i < amount && Hand.Count > 0; i++)
            {
                int randInt = random.Next(Hand.Count);

                Card card = Hand[randInt];

                Hand.RemoveAt(randInt);
                DiscardPile.Add(card);
            }
        }

        public void DiscardSelected(List<Card> toDiscard)
        {
            foreach (Card card in toDiscard)
            {
                if (Hand.Remove(card))
                    DiscardPile.Add(card);
            }
        }

        public void DiscardHand()
        {
            DiscardPile.AddRange(Hand);
            Hand.Clear();
        }

        public void OnCombatStart()
        {
            Weak = 0;
            Vulnerable = 0;
            Strength = 0;
            Dexterity = 0;
            Poison = 0;

            DrawPile.Clear();
            Hand.Clear();
            DiscardPile.Clear();
            ExhaustPile.Clear();
            Passives.RemoveAll(p => p.Type == PassiveType.Power);

            DrawPile.AddRange(Deck);
            Reshuffle();
        }

        public void ApplyReward(Reward reward, Card card, PassiveEffect relic)
        {
            Gold += reward.Gold;
            if (card != null) Deck.Add(card);
            if (relic != null) Passives.Add(relic);
        }

        public void MoveInnateCardsOnTop()
        {
            var innate = DrawPile
                .Where(card => card.Innate)
                .ToList();

            DrawPile.RemoveAll(card => card.Innate);
            DrawPile.InsertRange(0, innate);
        }
    }
}
