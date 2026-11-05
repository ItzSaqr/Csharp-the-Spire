using CardGame.Cards;
using CardGame.Char;
using CardGame.Map;
using CardGame.Passives;
using System.Text.Json;

namespace CardGame.Saves
{
    public class CardSave
    {
        public string Type { get; set; }
        public bool Upgraded { get; set; }
    }

    public class NodeSave
    {
        public int Floor { get; set; }
        public int Index { get; set; }
        public bool Active { get; set; }
        public bool Available { get; set; }
        public bool Visited { get; set; }
        public NodeType Type { get; set; }
        public List<int[]> Next { get; set; } = new();
        public List<int[]> Prev { get; set; } = new();
    }

    public class MapSave
    {
        public List<List<NodeSave>> Floors { get; set; } = new();
        public NodeSave Boss { get; set; }
    }

    public class PlayerSave
    {
        public int Hp { get; set; }
        public int MaxHp { get; set; }
        public int MaxEnergy { get; set; }
        public int Gold { get; set; }
        public List<CardSave> Deck { get; set; } = new();
        public List<string> Passives { get; set; } = new();
    }

    public class SaveData
    {
        public PlayerSave Player { get; set; }
        public MapSave Map { get; set; }
    }

    public static class SaveManager
    {
        public static string SavePath =>
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                         "CardGame", "savegame.json");

        public static void Save(Player player, GameMap map)
        {
            var data = new SaveData
            {
                Player = new PlayerSave
                {
                    Hp = player.Hp,
                    MaxHp = player.MaxHp,
                    MaxEnergy = player.MaxEnergy,
                    Gold = player.Gold,
                    Deck = player.Deck.Select(c => new CardSave
                    {
                        Type = c.GetType().FullName,
                        Upgraded = c.Upgraded
                    }).ToList(),
                    Passives = player.Passives.Select(p => p.GetType().FullName).ToList()
                },
                Map = new MapSave
                {
                    Floors = map.Floors.Select(floor => floor.Select(SaveNode).ToList()).ToList(),
                    Boss = SaveNode(map.Boss)
                }
            };

            Directory.CreateDirectory(Path.GetDirectoryName(SavePath)!);
            File.WriteAllText(SavePath, JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true }));
        }

        private static NodeSave SaveNode(MapNode node)
        {
            return new NodeSave
            {
                Floor = node.Floor,
                Index = node.Index,
                Active = node.Active,
                Available = node.Available,
                Visited = node.Visited,
                Type = node.Type,
                Next = node.Next.Select(n => new[] { n.Floor, n.Index }).ToList(),
                Prev = node.Prev.Select(n => new[] { n.Floor, n.Index }).ToList()
            };
        }

        // null, если сейва нет или он повреждён
        public static SaveData? Load()
        {
            if (!File.Exists(SavePath))
                return null;

            try
            {
                return JsonSerializer.Deserialize<SaveData>(File.ReadAllText(SavePath));
            }
            catch (JsonException)
            {
                return null;
            }
        }

        public static void DeleteSave()
        {
            if (File.Exists(SavePath))
                File.Delete(SavePath);
        }

        public static Player RestorePlayer(PlayerSave save)
        {
            var player = new Player
            {
                Hp = save.Hp,
                MaxHp = save.MaxHp,
                MaxEnergy = save.MaxEnergy,
                Gold = save.Gold
            };

            foreach (var cardSave in save.Deck)
            {
                var card = RestoreCard(cardSave);
                if (card != null)
                    player.Deck.Add(card);
            }

            foreach (var typeName in save.Passives)
            {
                var relic = typeof(PassiveEffect).Assembly.GetType(typeName!) is { } t
                    ? (PassiveEffect?)Activator.CreateInstance(t)
                    : null;
                if (relic != null)
                    player.Passives.Add(relic);
            }

            return player;
        }

        public static Card? RestoreCard(CardSave save)
        {
            var t = typeof(Card).Assembly.GetType(save.Type!);
            if (t == null)
                return null;

            var card = (Card?)Activator.CreateInstance(t);
            if (card != null && save.Upgraded && !card.Upgraded)
                card.Upgrade();

            return card;
        }

        public static GameMap RestoreMap(MapSave save)
        {
            var map = new GameMap();
            var nodes = new Dictionary<string, MapNode>();

            MapNode RestoreNode(NodeSave ns)
            {
                var node = new MapNode
                {
                    Floor = ns.Floor,
                    Index = ns.Index,
                    Active = ns.Active,
                    Available = ns.Available,
                    Visited = ns.Visited,
                    Type = ns.Type
                };
                nodes[$"{ns.Floor}:{ns.Index}"] = node;
                return node;
            }

            foreach (var floor in save.Floors)
                map.Floors.Add(floor.Select(RestoreNode).ToList());

            var boss = RestoreNode(save.Boss);
            map.Boss = boss;

            void Link(NodeSave ns)
            {
                var node = nodes[$"{ns.Floor}:{ns.Index}"];
                node.Next = ns.Next.Select(k => nodes[$"{k[0]}:{k[1]}"]).ToList();
                node.Prev = ns.Prev.Select(k => nodes[$"{k[0]}:{k[1]}"]).ToList();
            }

            foreach (var floor in save.Floors)
                foreach (var ns in floor)
                    Link(ns);
            Link(save.Boss);

            return map;
        }
    }
}
