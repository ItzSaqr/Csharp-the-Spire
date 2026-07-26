using CardGame.Cards;
using CardGame.Passives;
using CardGame.CombatNamespace;
using CardGame.Enemies;
using System.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardGame.Rewards
{
    public class Reward
    {
        public int Gold;
        public List<Card> CardChoices = new();
        public List<PassiveEffect> Relics = new();
    }

    public class RewardGenerator
    {
        private Random rand = new Random();

        private List<Func<Card>> cardPool;
        private List<Func<PassiveEffect>> relicPool;

        public RewardGenerator()
        {
            cardPool = Assembly.GetExecutingAssembly()
                .GetTypes()
                .Where(t => typeof(Card).IsAssignableFrom(t) &&
                            !t.IsAbstract &&
                            t.Namespace == "CardGame.Cards")
                .Select(t => (Card)Activator.CreateInstance(t)!)
                .Where(c => c.Rewardable)
                .Select(c =>
                {
                    var type = c.GetType();
                    return (Func<Card>)(() => (Card)Activator.CreateInstance(type)!);
                })
                .ToList();


            relicPool = Assembly.GetExecutingAssembly()
                .GetTypes()
                .Where(t => typeof(PassiveEffect).IsAssignableFrom(t) &&
                            !t.IsAbstract &&
                            t.Namespace == "CardGame.Passives")
                .Select(t => (Func<PassiveEffect>)(() => (PassiveEffect)Activator.CreateInstance(t)!))
                .ToList();
        }

        public Reward Generate(CombatType type)
        {
            var reward = new Reward();

            reward.Gold = type switch
            {
                CombatType.Basic => rand.Next(15, 30),
                CombatType.Elite => rand.Next(45, 60),
                CombatType.Boss => rand.Next(90, 110),
                _ => 0
            };

            reward.CardChoices = GenerateCardChoises(3);

            if (type != CombatType.Basic) reward.Relics = GenerateRandomRelics(1);

            return reward;
        }

        private List<Card> GenerateCardChoises(int amount)
        {
            return cardPool.OrderBy(x => rand.Next()).Take(amount).Select(create => create()).ToList();
        }

        private List<PassiveEffect> GenerateRandomRelics(int amount)
        {
            return relicPool.OrderBy(x => rand.Next()).Take(amount).Select(create => create()).ToList();
        }
    }
}
