using CardGame.Cards;
using CardGame.Char;
using CardGame.Passives;
using CardGame.Rewards;

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
