using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CardGame.Cards;
using CardGame.Enemies;
using CardGame.Passives;
using CardGame.Rewards;

namespace CardGame.Map
{
    public enum NodeType
    {
        BasicEnemy,
        EliteEnemy,
        Boss,
        Shop,
        Treasure,
        RestSite
    }

    public class MapNode
    {
        public int Floor;
        public int Index;
        public bool Active;
        public bool Available;
        public bool Visited;

        public NodeType Type;
        public List<MapNode> Next = new();
        public List<MapNode> Prev = new();

        public bool HasAnyPath => Next.Count > 0 || Prev.Count > 0;
    }

    public class GameMap
    {
        public List<List<MapNode>> Floors = new();
        public MapNode Boss;

        public void Print()
        {
            Console.Clear();

            const int cellWidth = 6;

            PrintBoss(cellWidth);

            for (int floor = Floors.Count - 1; floor >= 0; floor--)
            {
                PrintNodeRow(floor, cellWidth);

                if (floor > 0)
                    PrintConnectionRow(floor, cellWidth);
            }
        }

        private void PrintNodeRow(int floor, int cellWidth)
        {
            for (int i = 0; i < Floors[floor].Count; i++)
            {
                var node = Floors[floor][i];

                if (!node.Active)
                {
                    Console.Write(new string(' ', cellWidth));
                    continue;
                }

                string symbol = $"[{GetNodeSymbol(node)}]";
                Console.Write(symbol.PadRight(cellWidth));
            }

            Console.WriteLine();
        }

        private void PrintConnectionRow(int floor, int cellWidth)
        {
            char[] line = new string(' ', Floors[floor].Count * cellWidth).ToCharArray();

            foreach (var node in Floors[floor])
            {
                if (!node.Active)
                    continue;

                int start = node.Index * cellWidth + 1;

                foreach (var next in node.Prev)
                {
                    int target = next.Index * cellWidth + 1;

                    if (target < start)
                        line[(start + target) / 2] = '/';
                    else if (target > start)
                        line[(start + target) / 2] = '\\';
                    else
                        line[start] = '|';
                }
            }

            Console.WriteLine(new string(line));
        }

        private char GetNodeSymbol(MapNode node)
        {
            if (!node.Active)
                return '@';

            return node.Type switch
            {
                NodeType.BasicEnemy => 'E',
                NodeType.EliteEnemy => 'X',
                NodeType.RestSite => 'R',
                NodeType.Shop => '$',
                NodeType.Treasure => 'T',
                NodeType.Boss => 'B',
                _ => '?'
            };
        }

        private void PrintBoss(int cellWidth)
        {
            int totalWidth = Floors[0].Count * cellWidth;
            int center = totalWidth / 2;

            Console.WriteLine(new string(' ', center) + "[B]");
        }

    }

    public class MapGenerator
    {
        private const int Width = 7;
        private const int Height = 15;
        private const int PathCount = 6;

        private Random random = new Random();

        public GameMap Generate()
        {
            var map = CreateFullGrid();

            GeneratePaths(map);
            RemovePathlessRooms(map);
            AssignRoomTypes(map);
            AddBossRoom(map);
            SetStartNode(map);

            return map;
        }

        private void SetStartNode(GameMap map)
        {
            // Находим первый активный узел на первом этаже
            // Или центральный, если активных нет
            var startNode = map.Floors[0].FirstOrDefault(n => n.Active);

            if (startNode == null)
            {
                // Если нет активных узлов, активируем центральный
                startNode = map.Floors[0][Width / 2];
                startNode.Active = true;
                startNode.Type = NodeType.BasicEnemy;
                System.Diagnostics.Debug.WriteLine($"Fallback: Center node set as start");
            }

            startNode.Available = true;
            System.Diagnostics.Debug.WriteLine($"Start node set at Floor 0, Index {startNode.Index}, Type: {startNode.Type}");
        }

        private GameMap CreateFullGrid()
        {
            var map = new GameMap();

            for (int floor = 0; floor < Height; floor++)
            {
                var row = new List<MapNode>();

                for (int index = 0; index < Width; index++)
                {
                    row.Add(new MapNode { Floor = floor, Index = index });
                }

                map.Floors.Add(row);
            }

            return map;
        }

        private void GeneratePaths(GameMap map)
        {
            var usedStarts = new HashSet<int>();

            for (int path = 0; path < PathCount; path++)
            {
                int startX;

                if (path < 2)
                {
                    do
                    {
                        startX = random.Next(Width);
                    }
                    while (usedStarts.Contains(startX));

                    usedStarts.Add(startX);
                }
                else
                {
                    startX = random.Next(Width);
                }

                MapNode current = map.Floors[0][startX];

                for (int floor = 0; floor < Height - 1; floor++)
                {
                    var candidates = GetNextCandidates(map, current);

                    candidates = candidates
                        .Where(next => !WouldCrossPath(map, current, next))
                        .ToList();

                    if (candidates.Count == 0)
                        break;

                    MapNode nextNode = candidates[random.Next(candidates.Count)];

                    Connect(current, nextNode);

                    current = nextNode;
                }
            }
        }

        private List<MapNode> GetNextCandidates(GameMap map, MapNode current)
        {
            var result = new List<MapNode>();

            int nextFloor = current.Floor + 1;

            for (int dx = -1; dx <= 1; dx++)
            {
                int nextX = current.Index + dx;

                if (nextX >= 0 && nextX < Width)
                    result.Add(map.Floors[nextFloor][nextX]);
            }

            return result;
        }

        private void Connect(MapNode from, MapNode to)
        {
            if (!from.Next.Contains(to))
                from.Next.Add(to);

            if (!to.Prev.Contains(from))
                to.Prev.Add(from);
        }

        private bool WouldCrossPath(GameMap map, MapNode from, MapNode to)
        {
            int floor = from.Floor;

            foreach (var otherFrom in map.Floors[floor])
            {
                foreach (var otherTo in otherFrom.Next)
                {
                    bool crossingRight =
                        from.Index < to.Index &&
                        otherFrom.Index > otherTo.Index &&
                        from.Index == otherTo.Index &&
                        to.Index == otherFrom.Index;

                    bool crossingLeft =
                        from.Index > to.Index &&
                        otherFrom.Index < otherTo.Index &&
                        from.Index == otherTo.Index &&
                        to.Index == otherFrom.Index;

                    if (crossingRight || crossingLeft)
                        return true;
                }
            }

            return false;
        }

        private void RemovePathlessRooms(GameMap map)
        {
            foreach (var floor in map.Floors)
            {
                foreach (var node in floor)
                {
                    // Для первого этажа - оставляем активными только узлы, у которых есть путь
                    // ИЛИ центральный узел (стартовый)
                    if (node.Floor == 0)
                    {
                        // На первом этаже активны только узлы с путями
                        // Но один узел будет стартовым (у него может не быть путей)
                        node.Active = node.HasAnyPath;
                    }
                    else
                    {
                        node.Active = node.HasAnyPath;
                    }
                }
            }
        }

        private NodeType RollRoomType(int floor)
        {
            int roll = random.Next(100);

            if (roll < 60) return NodeType.BasicEnemy;
            if (roll < 76) return NodeType.EliteEnemy;
            if (roll < 88) return NodeType.RestSite;
            if (roll < 98) return NodeType.Shop;
            return NodeType.Treasure;
        }

        private void AddBossRoom(GameMap map)
        {
            var boss = new MapNode
            {
                Floor = Height,
                Index = Width / 2,
                Type = NodeType.Boss,
                Active = true
            };

            foreach (var node in map.Floors[Height - 1])
            {
                if (!node.Active)
                    continue;

                Connect(node, boss);
            }

            map.Boss = boss;
        }

        private void AssignRoomTypes(GameMap map)
        {
            for (int floor = 0; floor < Height; floor++)
            {
                foreach (var node in map.Floors[floor])
                {
                    if (!node.Active)
                        continue;

                    if (floor == 0)
                    {
                        node.Type = NodeType.BasicEnemy;
                        continue;
                    }

                    if (floor == Height - 1)
                    {
                        node.Type = NodeType.RestSite;
                        continue;
                    }

                    node.Type = RollRoomType(floor);
                }
            }
        }

        public void PrintMapInfo(GameMap map)
        {
            Console.WriteLine("Map Info:");
            for (int floor = 0; floor < map.Floors.Count; floor++)
            {
                var activeNodes = map.Floors[floor].Where(n => n.Active).ToList();
                Console.WriteLine($"Floor {floor}: {activeNodes.Count} active nodes");
                foreach (var node in activeNodes)
                {
                    Console.WriteLine($"  Node {node.Index}: Type={node.Type}, Available={node.Available}, HasPath={node.HasAnyPath}");
                }
            }
        }
    }
}
