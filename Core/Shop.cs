using CardGame.Cards;
using CardGame.Char;
using CardGame.CombatNamespace;
using CardGame.Passives;
using CardGame.Rewards;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardGame.Shop
{
    public abstract class ShopItem
    {
        public string Name;
        public string Description;
        public int Price;
        public bool Sold;

        public virtual bool CanBuy(Player player)
        {
            return !Sold && player.Gold >= Price;
        }

        public abstract void Buy(Player player);
    }

    public class CardShopItem : ShopItem
    {
        public Card Card;
        public CardShopItem(Card card, int price)
        {
            Card = card;
            Name = card.Name;
            Description = card.Description;
            Price = price;
            Sold = false;
        }
        public override void Buy(Player player)
        {
            if (CanBuy(player))
            {
                player.Gold -= Price;
                player.Deck.Add(Card);
                Sold = true;
            }
        }
    }

    public class RelicShopItem : ShopItem {
        public PassiveEffect Relic;
        public RelicShopItem(PassiveEffect relic, int price)
        {
            Relic = relic;
            Name = relic.Name;
            Description = relic.Description;
            Price = price;
            Sold = false;
        }
        public override void Buy(Player player)
        {
            if (CanBuy(player))
            {
                player.Gold -= Price;
                player.Passives.Add(Relic);
                Sold = true;
            }
        }
    }

    public class RemoveCardShopItem : ShopItem
    {
        public Action<Player> OnBuyRequested;
        public RemoveCardShopItem(int price)
        {
            Name = "Card Removal";
            Description = "Remove a card from your deck";
            Price = price;
        }

        public override void Buy(Player player)
        {
            if (!CanBuy(player)) return;
            OnBuyRequested?.Invoke(player);
        }
    }

    public class ShopState
    {
        public List<CardShopItem> CardItems = new List<CardShopItem>();
        public List<RelicShopItem> RelicItems = new List<RelicShopItem>();
        public RemoveCardShopItem RemoveItem = new RemoveCardShopItem(75);
        private RewardGenerator Generator = new RewardGenerator();

        public ShopState(List<Card> cards, List<PassiveEffect> relics)
        {
            foreach (var c in cards) CardItems.Add(new CardShopItem(c, 50));
            foreach (var r in relics) RelicItems.Add(new RelicShopItem(r, 150));
        }
    }
}
