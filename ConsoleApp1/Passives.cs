using CardGame.Cards;
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
        Relic,
        Power,
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
}
