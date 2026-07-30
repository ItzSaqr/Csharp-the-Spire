using CardGame.Cards;
using CardGame.Char;
using CardGame.CombatNamespace;
using CardGame.Rewards;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardGame.Passives
{
    public abstract class PassiveEffect
    {
        public string Name;
        public string Description;
        public PassiveType Type;

        public virtual void OnTurnStart(Character player, Combat combat) { }
        public virtual void OnTurnEnd(Character player, Combat combat) { }
        public virtual void OnCardPlayed(Character player, Combat combat, Card card) { }

        public virtual void OnBeforeCardPlayed(Character player, Combat combat, Card card) { }

        public virtual void OnCombatStart(Character player, Combat combat) { }

        public virtual int ModifyDamage(Character source, Character target, Card? card, int damage)
        {
            return damage;
        }

        public virtual string GetDescription()
        {
            return null;
        }

        public virtual void RestOptionUse() { }
    }
    public enum PassiveType
    {
        Power,
        CommonRelic,
        UncommonRelic,
        RareRelic,
        ShopRelic,
        BossRelic
    }

    public class CreateShiv : PassiveEffect
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

    public class Enrage : PassiveEffect
    {
        public int amount;
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

    public class PenNib : PassiveEffect
    {
        private int AttacksPlayed;
        private bool active;
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

        public override string GetDescription()
        {
            return $"Attacks played: {AttacksPlayed}/10";
        }
    }

    public class Girya : PassiveEffect
    {
        public int strength;

        public Girya()
        {
            Name = "Girya";
            Description = "You can now gain Strength at Rest Sites. (3 times max)";
            Type = PassiveType.RareRelic;
            strength = 0;
        }

        public override void OnCombatStart(Character owner, Combat combat)
        {
            owner.ApplyStrength(strength);
        }

        public override void RestOptionUse()
        {
            if (strength < 3) strength += 1;
        }

        public override string GetDescription()
        {
            return $"Strength gained: {strength}/3";
        }
    }
}
