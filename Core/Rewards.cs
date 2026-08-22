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

        public PassiveEffect GenerateRandomRelic()
        {
            // веса реликов
            var rarityWeights = new Dictionary<PassiveType, int>
            {
                { PassiveType.CommonRelic, 60 },
                { PassiveType.UncommonRelic, 30 },
                { PassiveType.RareRelic, 10 }
            };

            var relicsByRarity = new Dictionary<PassiveType, List<Func<PassiveEffect>>>();

            foreach (var createFunc in relicPool)
            {
                var relic = createFunc();
                if (!relicsByRarity.ContainsKey(relic.Type))
                    relicsByRarity[relic.Type] = new List<Func<PassiveEffect>>();

                relicsByRarity[relic.Type].Add(createFunc);
            }

            // выбор редкости на основе весов
            int totalWeight = rarityWeights.Values.Sum();
            int roll = rand.Next(totalWeight);
            int cumulative = 0;

            PassiveType selectedRarity = PassiveType.CommonRelic;
            foreach (var kvp in rarityWeights)
            {
                cumulative += kvp.Value;
                if (roll < cumulative)
                {
                    selectedRarity = kvp.Key;
                    break;
                }
            }

            // скип если нет реликов выбранной редкости
            if (!relicsByRarity.ContainsKey(selectedRarity) || relicsByRarity[selectedRarity].Count == 0)
            {
                foreach (var rarity in new[] { PassiveType.CommonRelic, PassiveType.UncommonRelic,
                                       PassiveType.RareRelic, PassiveType.BossRelic,
                                       PassiveType.ShopRelic, PassiveType.Power })
                {
                    if (relicsByRarity.ContainsKey(rarity) && relicsByRarity[rarity].Count > 0)
                    {
                        selectedRarity = rarity;
                        break;
                    }
                }
            }

            if (!relicsByRarity.ContainsKey(selectedRarity) || relicsByRarity[selectedRarity].Count == 0)
                return null;

            var pool = relicsByRarity[selectedRarity];
            return pool[rand.Next(pool.Count)]();
        }

        public List<PassiveEffect> GenerateRandomRelicsFromPool(int amount)
        {
            var result = new List<PassiveEffect>();
            for (int i = 0; i < amount; i++)
            {
                var relic = GenerateRandomRelic();
                if (relic != null)
                    result.Add(relic);
            }
            return result;
        }
    }
}
