using CardGame.Cards;
using CardGame.Rewards;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardGame.Passives
{
    abstract class PassiveEffect
    {
        public string Name;
        public string Description;
        public PassiveType Type;

        public virtual void OnTurnStart(Character player, Combat combat) { }
        public virtual void OnTurnEnd(Character player, Combat combat) { }
        public virtual void OnCardPlayed(Character player, Combat combat, Card card) { }

        public virtual void OnBeforeCardPlayed(Character player, Combat combat, Card card) { }

        public virtual int ModifyDamage(Character source, Character target, Card? card, int damage)
        {
            return damage;
        }

    }
    enum PassiveType
    {
        Power,
        CommonRelic,
        UncommonRelic,
        RareRelic,
        ShopRelic,
        BossRelic
    }

    class CreateShiv : PassiveEffect
    {
        public CreateShiv()
        {
            Name = "Infinite Blades";
            Description = "Add a Shiv into your hand every turn";
            Type = PassiveType.Power;
        }
        public override void OnTurnStart(Character owner, Combat combat)
        {
            if (owner is Player player) player.Hand.Add(new Shiv());
        }
    }

    class Enrage : PassiveEffect
    {
        private int amount;
        public Enrage(int amount)
        {
            Name = "Enrage";
            Description = $"Whenever you play a Skill, gains {amount} Strength";
            Type = PassiveType.Power;

            this.amount = amount;
        }

        public override void OnCardPlayed(Character owner, Combat combat, Card card)
        {
            if (card.Type == CardType.Skill) combat.Enemy.ApplyStrength(amount);
        }
    }

    class PenNib : PassiveEffect
    {
        private int AttacksPlayed;
        bool active;
        public PenNib()
        {
            Name = "Pen Nib";
            Description = "Every 10th Attack you play deals double damage";
            Type = PassiveType.CommonRelic;

        }

        public override void OnBeforeCardPlayed(Character owner, Combat combat, Card card)
        {
            if (card.Type != CardType.Attack) return;

            AttacksPlayed++;

            if (AttacksPlayed == 10)
            {
                active = true;
                AttacksPlayed = 0;
            }
        }

        public override int ModifyDamage(Character source, Character target, Card? card, int damage)
        {
            if (!active) return damage;

            active = false;
            return damage * 2;
        }
    }
}
