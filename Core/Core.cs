using CardGame.Char;
using CardGame.Cards;
using CardGame.Enemies;
using CardGame.Passives;
using CardGame.Rewards;
using CardGame.Map;
using CardGame.CombatNamespace;
using System;
using System.Windows;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Security.Principal;

var player = new Player { Hp = 500, MaxHp = 500, MaxEnergy = 3 };

//for (int i = 0; i < 2; i++)
//    player.Deck.Add(new Strike());

//for (int i = 0; i < 2; i++)
//    player.Deck.Add(new Defend());

//player.Deck.Add(new Bash());
//player.Deck.Add(new Prepare());
//player.Deck.Add(new Concentrate());
//player.Deck.Add(new InfiniteBlades());

for (int i = 0; i < 20; i++) player.Deck.Add(new Shiv());

player.Passives.Add(new PenNib());

var enemy = new Gremlin();

var ui = new ConsoleCombatUI();
var combat = new Combat(player, enemy, ui);

while (combat.State != CombatState.Victory &&
       combat.State != CombatState.Defeat)
{
    if (combat.State == CombatState.PlayerTurn)
    {
        Console.Clear();
        Console.WriteLine("================================");
        Console.WriteLine($"PLAYER HP: {player.Hp} | Energy: {player.Energy}");
        Console.WriteLine($"PLAYER: {player.GetStatusText()}");

        Console.WriteLine();
        Console.WriteLine($"Draw: {player.DrawPile.Count} | Discard: {player.DiscardPile.Count} | Exhaust: {player.ExhaustPile.Count}");
        Console.WriteLine($"View piles: 'd' - Draw Pile | 's' - Discard Pile | 'x' - Exhaust Pile");

        Console.WriteLine();

        Console.WriteLine($"{enemy.Name} HP: {enemy.Hp}");
        Console.WriteLine($"ENEMY: {enemy.GetStatusText()}");
        Console.WriteLine($"INTENT: {enemy.Intent.Text}");
        Console.WriteLine("================================");

        Console.WriteLine("HAND:");

        for (int i = 0; i < player.Hand.Count; i++)
        {
            var c = player.Hand[i];
            Console.WriteLine($"{i}. {c.Name} [{c.Cost}] - {c.Description}");
        }

        Console.WriteLine();
        Console.WriteLine("Choose card index or 'e' to end turn:");

        var input = Console.ReadLine();

        Console.Clear();

        if (input == "e")
        {
            combat.EndPlayerTurn();
            continue;
        }

        if (input == "d")
        {
            ShowPile("Draw Pile:", player.DrawPile);
            continue;
        }

        if (input == "s")
        {
            ShowPile("Discard Pile:", player.DiscardPile);
            continue;
        }

        if (input == "x")
        {
            ShowPile("Exhaust Pile:", player.ExhaustPile);
            continue;
        }

        if (int.TryParse(input, out int index) &&
            index >= 0 && index < player.Hand.Count)
        {
            combat.PlayCard(player.Hand[index]);
        }
    }
}
RewardGenerator gen = new RewardGenerator();
Reward reward;

Console.WriteLine(combat.State);
if (combat.State == CombatState.Victory)
{
    reward = gen.Generate(CombatType.Basic);
    var card = ui.ChooseCardReward(reward);

    PassiveEffect relic = null;
    if (reward.Relics.Count > 0)
        relic = ui.ChooseRelicReward(reward);

    player.ApplyReward(reward, card, relic);

    var restSite = new RestSite();
    restSite.Enter(player, ui);
}

var shopCards = new List<Card>
{
    new DeadlyPoison(),
    new Prepare(),
    new Concentrate(),
    new InfiniteBlades(),
    new Strike(),
    new Defend()
};

var shopRelics = new List<PassiveEffect>
{
    new Enrage(5),
    new CreateShiv(),
    new CreateShiv()
};

var shop = new Shop(shopCards, shopRelics);
shop.Enter(player, ui);

MapGenerator mapGenerator = new MapGenerator();
GameMap map = mapGenerator.Generate();

map.Print();

static void ShowPile(string title, List<Card> pile)
{
    Console.Clear();
    Console.WriteLine("================================");
    Console.WriteLine(title);

    if (pile.Count == 0)
        Console.WriteLine("Pile is empty");

    for (int i = 0; i < pile.Count; i++)
    {
        var c = pile[i];
        Console.WriteLine($"{i}. {c.Name} [{c.Cost}] - {c.Description}");
    }

    Console.WriteLine("================================");
    Console.WriteLine();
    Console.WriteLine("Press Enter to return");
    Console.ReadLine();
}

public interface IUserInterface
{
    List<Card> ChooseCards(Player player, List<Card> source, int amount);
    void ShowMessage(string message);

    Card ChooseCardReward(Reward reward);

    PassiveEffect ChooseRelicReward(Reward reward);
}

public class ConsoleCombatUI : IUserInterface
{
    public List<Card> ChooseCards(Player player, List<Card> source, int amount)
    {
        var selected = new List<Card>();
        if (amount >= source.Count) {
            selected.AddRange(source);
            return selected; 
        }
        while (selected.Count() < amount)
        {
            Console.Clear();
            Console.WriteLine($"Choose {amount} cards. {selected.Count}/{amount} chosen.");

            for (int i = 0; i < source.Count(); i++)
            {
                var c = source[i];
                var mark = selected.Contains(c) ? " [selected]" : "";
                Console.WriteLine($"{i}. {c.Name} [{c.Cost}] - {c.Description}{mark}");
            }

            var input = Console.ReadLine();

            if (!int.TryParse(input, out int index))
                continue;

            if (index < 0 || index >= source.Count)
                continue;

            var card = source[index];

            if (!selected.Contains(card))
            {
                selected.Add(card);
            }
            else if (selected.Contains(card))
            {
                selected.Remove(card);
            }
        }
        return selected;
    }

    public Card ChooseCardReward(Reward reward)
    {
        var source = reward.CardChoices;

        Console.Clear();
        Console.WriteLine($"Choose card between {source.Count} cards.");

        for (int i = 0; i < source.Count(); i++)
        {
            var c = source[i];
            Console.WriteLine($"{i}. {c.Name} [{c.Cost}] - {c.Description}");
        }
        Console.WriteLine("s - skip");

        var input = "";
        int index = 0;

        while (true)
        {
            input = Console.ReadLine();

            if (input == "s")
                return null;

            if (int.TryParse(input, out index) &&
                index >= 0 && index < source.Count)
            {
                return source[index];
            }
        }
    }

    public PassiveEffect ChooseRelicReward(Reward reward)
    {
        var source = reward.Relics;

        Console.Clear();
        Console.WriteLine($"Choose card between {source.Count} cards.");

        for (int i = 0; i < source.Count(); i++)
        {
            var c = source[i];
            Console.WriteLine($"{i}. {c.Name} - {c.Description}");
        }
        Console.WriteLine("s - skip");

        var input = "";
        int index = 0;


        while (true)
        {
            input = Console.ReadLine();

            if (input == "s")
                return null;

            if (int.TryParse(input, out index) &&
                index >= 0 && index < source.Count)
            {
                return source[index];
            }
        }
    }

    public void ShowMessage(string message)
    {
        Console.WriteLine(message);
    }
}

abstract class RestOption
{
    public string Name;
    public string Description;

    public virtual bool CanUse(Player player)
    {
        return true;
    }

    public abstract void Use(Player player, IUserInterface ui);
}

class HealOption : RestOption
{
    public HealOption()
    {
        Name = "Heal";
        Description = "Heal 30% of max HP";
    }

    public override bool CanUse(Player player)
    {
        return player.Hp < player.MaxHp;
    }

    public override void Use(Player player, IUserInterface ui)
    {
        int heal = (int)(player.MaxHp * 0.3);
        player.Heal(heal);
    }
}

class UpgradeCardOption : RestOption
{
    public UpgradeCardOption()
    {
        Name = "Upgrade card";
        Description = "Upgrade a card in your deck";
    }

    public override bool CanUse(Player player)
    {
        return player.Deck.Any(card => !card.Upgraded);
    }

    public override void Use(Player player, IUserInterface ui)
    {
        var cards = player.Deck
            .Where(card => !card.Upgraded)
            .ToList();

        if (cards.Count == 1)
        {
            cards[0].Upgrade();
            return;
        }

        var selected = ui.ChooseCards(player, cards, 1);

        if (selected.Count > 0) selected[0].Upgrade();
    }
}

class RestSite
{
    private readonly List<RestOption> options = new List<RestOption>()
    {
        new HealOption(),
        new UpgradeCardOption()
    };

    public void Enter(Player player, IUserInterface ui)
    {
        while (true)
        {
            Console.Clear();
            Console.WriteLine("Rest Site");
            Console.WriteLine("================================");
            Console.WriteLine($"Hp: {player.Hp}/{player.MaxHp}");
            Console.WriteLine();
            Console.WriteLine("Options available:");
            Console.WriteLine("================================");

            for(int i = 0; i < options.Count; i++)
            {
                var option = options[i];
                string locked = option.CanUse(player) ? "" : " [unavailable]";
                Console.WriteLine($"{i}. {option.Name} - {option.Description}{locked}");
            }
            Console.WriteLine("s - skip");
            Console.WriteLine("================================");

            var input = Console.ReadLine();

            if (input == "s") break;

            if (!int.TryParse(input, out int index))
                continue;
            if (index < 0 || index >= options.Count)
                continue;
            
            var chosen = options[index];

            if (!chosen.CanUse(player))
                continue;

            chosen.Use(player, ui);
            break;
        }
    }
}

abstract class ShopItem
{
    public string Name;
    public string Description;
    public int Price;
    public bool Sold;

    public virtual bool CanBuy(Player player)
    {
        return !Sold && player.Gold >= Price;
    }

    public abstract void Buy(Player player, IUserInterface ui);
}

class CardShopItem : ShopItem
{
    private Card card;

    public CardShopItem(Card card, int price)
    {
        this.card = card;
        Name = card.Name;
        Description = card.Description;
        Price = price;
    }

    public override void Buy(Player player, IUserInterface ui)
    {
        if (CanBuy(player))
        {
            player.Gold -= Price;
            player.Deck.Add(card);
            Sold = true;
        }
    }
}

class RelicShopItem : ShopItem
{
    private PassiveEffect relic;

    public RelicShopItem(PassiveEffect relic, int price)
    {
        this.relic = relic;
        Name = relic.Name;
        Description = relic.Description;
        Price = price;
    }

    public override void Buy(Player player, IUserInterface ui)
    {
        if (CanBuy(player))
        {
            player.Gold -= Price;
            player.Passives.Add(relic);
            Sold = true;
        }
    }
}

class RemoveCardShopItem : ShopItem
{
    public RemoveCardShopItem(int price)
    {
        Name = "Card remove";
        Description = "Remove a card from your deck";
        Price = price;
    }

    public override bool CanBuy(Player player)
    {
        return !Sold && player.Gold >= Price && player.Deck.Count > 0;
    }

    public override void Buy(Player player, IUserInterface ui)
    {
        if (CanBuy(player))
        {
            var card = ui.ChooseCards(player, player.Deck, 1);

            if (card.Count == 0) return;

            player.Gold -= Price;
            player.Deck.Remove(card[0]);
            Sold = true;
        }
    }
}

class Shop
{
    private readonly List<ShopItem> items = new();

    public Shop(List<Card> cards, List<PassiveEffect> relics)
    {
        foreach (var card in cards) items.Add(new CardShopItem(card, 50));

        foreach (var relic in relics) items.Add(new RelicShopItem(relic, 150));

        items.Add(new RemoveCardShopItem(75));
    }

    public void Enter(Player player, IUserInterface ui)
    {
        while (true)
        {
            Console.Clear();
            Console.WriteLine("Shop");
            Console.WriteLine("================================");
            Console.WriteLine($"Hp: {player.Hp}/{player.MaxHp} | Gold: {player.Gold}");
            Console.WriteLine();
            Console.WriteLine("Options available:");
            Console.WriteLine("================================");

            for (int i = 0; i < items.Count; i++)
            {
                var item = items[i];
                string state;

                if (item.Sold)
                    state = " [sold]";
                else if (!item.CanBuy(player))
                    state = " [too expensive]";
                else
                    state = "";

                Console.WriteLine($"{i}.[{item.Price}] gold: {item.Name} - {item.Description}{state}");
            }
            Console.WriteLine("s - leave");
            Console.WriteLine("================================");

            var input = Console.ReadLine();

            if (input == "s") break;

            if (!int.TryParse(input, out int index))
                continue;
            if (index < 0 || index >= items.Count)
                continue;

            var chosen = items[index];

            if (!chosen.CanBuy(player))
                continue;

            chosen.Buy(player, ui);
            break;
        }
    }
}

