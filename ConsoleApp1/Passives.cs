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

        public virtual void OnTurnStart(Player player, Combat combat) { }
        public virtual void OnTurnEnd(Player player, Combat combat) { }
        public virtual void OnCardPlayed(Player player, Combat combat, Card card) { }

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
        public override void OnTurnStart(Player player, Combat combat)
        {
            player.Hand.Add(new Shiv());
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

        public override void OnCardPlayed(Player player, Combat combat, Card card)
        {
            if (card.Type == CardType.Skill) combat.Enemy.ApplyStrength(amount);
        }
    }
}
