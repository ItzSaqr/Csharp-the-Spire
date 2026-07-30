using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using CardGame.Map;

namespace CardGame
{
    public partial class MapView : UserControl
    {
        private GameMap map;
        private Game game;
        private MainWindow mainWindow;
        private const int NodeSize = 50;
        private const int HorizontalSpacing = 80;
        private const int VerticalSpacing = 60;

        public MapView(GameMap map, Game game, MainWindow mainWindow)
        {
            InitializeComponent();
            this.map = map;
            this.game = game;
            this.mainWindow = mainWindow;

            DrawMap();
            UpdateInfo();
        }

        private void DrawMap()
        {
            MapCanvas.Children.Clear();

            var floors = map.Floors;

            // Сначала рисуем все связи
            for (int floorIdx = 0; floorIdx < floors.Count; floorIdx++)
            {
                var floor = floors[floorIdx];
                int y = (floors.Count - floorIdx) * VerticalSpacing + 50;

                for (int nodeIdx = 0; nodeIdx < floor.Count; nodeIdx++)
                {
                    var node = floor[nodeIdx];
                    if (!node.Active) continue;

                    int x = (nodeIdx + 1) * HorizontalSpacing + 50;

                    foreach (var next in node.Next)
                    {
                        if (!next.Active) continue;
                        int nextX = (next.Index + 1) * HorizontalSpacing + 50;
                        int nextY = (floors.Count - next.Floor) * VerticalSpacing + 50;
                        DrawConnection(x, y, nextX, nextY);
                    }
                }
            }

            // Затем рисуем все узлы
            for (int floorIdx = 0; floorIdx < floors.Count; floorIdx++)
            {
                var floor = floors[floorIdx];
                int y = (floors.Count - floorIdx) * VerticalSpacing + 50;

                for (int nodeIdx = 0; nodeIdx < floor.Count; nodeIdx++)
                {
                    var node = floor[nodeIdx];
                    if (!node.Active) continue;

                    int x = (nodeIdx + 1) * HorizontalSpacing + 50;
                    DrawNode(node, x, y);
                }
            }

            // Рисуем босса
            if (map.Boss != null)
            {
                int x = (map.Boss.Index + 1) * HorizontalSpacing + 50;
                int y = 50;
                DrawNode(map.Boss, x, y);
            }

            // Логируем для отладки
            System.Diagnostics.Debug.WriteLine($"Map drawn. Total floors: {floors.Count}");
            System.Diagnostics.Debug.WriteLine($"Boss: {map.Boss != null}");

            // Дополнительная отладка - выводим информацию о всех узлах
            for (int floorIdx = 0; floorIdx < floors.Count; floorIdx++)
            {
                var floor = floors[floorIdx];
                foreach (var node in floor)
                {
                    if (node.Active)
                    {
                        System.Diagnostics.Debug.WriteLine($"Floor {node.Floor}, Index {node.Index}, Type: {node.Type}, Available: {node.Available}, Visited: {node.Visited}");
                    }
                }
            }
        }

        private void DrawNode(MapNode node, int x, int y)
        {
            var border = new Border
            {
                Width = NodeSize,
                Height = NodeSize,
                CornerRadius = new CornerRadius(8),
                Background = GetNodeColor(node),
                BorderBrush = Brushes.White,
                BorderThickness = new Thickness(node.Visited ? 3 : 1),
                Cursor = node.Available ? System.Windows.Input.Cursors.Hand : System.Windows.Input.Cursors.Arrow,
                Tag = node,
                IsHitTestVisible = node.Available
            };

            if (node.Available)
                border.MouseLeftButtonDown += (s, e) => OnNodeClick(node);

            var text = new TextBlock
            {
                Text = GetNodeSymbol(node),
                Foreground = Brushes.White,
                FontSize = 20,
                FontWeight = FontWeights.Bold,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };

            border.Child = text;

            border.ToolTip = new ToolTip
            {
                Content = $"{GetNodeTypeName(node.Type)}\n" +
                         $"Floor: {node.Floor}\n" +
                         (node.Visited ? "Visited" : node.Available ? "Available" : "Locked")
            };

            Canvas.SetLeft(border, x - NodeSize / 2);
            Canvas.SetTop(border, y - NodeSize / 2);

            MapCanvas.Children.Add(border);
        }

        private void DrawConnection(int x1, int y1, int x2, int y2)
        {
            var line = new Line
            {
                X1 = x1,
                Y1 = y1,
                X2 = x2,
                Y2 = y2,
                Stroke = Brushes.Gray,
                StrokeThickness = 3
            };

            MapCanvas.Children.Add(line);
        }

        private Brush GetNodeColor(MapNode node)
        {
            if (node.Visited) return Brushes.DarkGray;
            if (node.Available) return Brushes.Goldenrod;

            return node.Type switch
            {
                NodeType.BasicEnemy => new SolidColorBrush(Color.FromRgb(60, 40, 40)),
                NodeType.EliteEnemy => new SolidColorBrush(Color.FromRgb(80, 30, 30)),
                NodeType.Boss => new SolidColorBrush(Color.FromRgb(100, 20, 20)),
                NodeType.RestSite => new SolidColorBrush(Color.FromRgb(30, 50, 30)),
                NodeType.Shop => new SolidColorBrush(Color.FromRgb(50, 50, 20)),
                NodeType.Treasure => new SolidColorBrush(Color.FromRgb(40, 30, 60)),
                _ => Brushes.DarkSlateGray
            };
        }

        private string GetNodeSymbol(MapNode node)
        {
            return node.Type switch
            {
                NodeType.BasicEnemy => "⚔",
                NodeType.EliteEnemy => "💀",
                NodeType.Boss => "👑",
                NodeType.RestSite => "🔥",
                NodeType.Shop => "💰",
                NodeType.Treasure => "🎁",
                _ => "?"
            };
        }

        private string GetNodeTypeName(NodeType type)
        {
            return type switch
            {
                NodeType.BasicEnemy => "Basic Enemy",
                NodeType.EliteEnemy => "Elite Enemy",
                NodeType.Boss => "Boss",
                NodeType.RestSite => "Rest Site",
                NodeType.Shop => "Shop",
                NodeType.Treasure => "Treasure",
                _ => "Unknown"
            };
        }

        private void OnNodeClick(MapNode node)
        {
            if (!node.Available || node.Visited) return;

            game.EnterNode(node);

            // Проверяем тип узла перед переключением на бой
            if (node.Type != NodeType.RestSite &&
                node.Type != NodeType.Shop &&
                node.Type != NodeType.Treasure)
            {
                mainWindow.SwitchToCombat();
            }
            // Для RestSite, Shop, Treasure - переключение происходит внутри Game.EnterNode
        }

        private void UpdateInfo()
        {
            var player = game.Player;
            HpText.Text = $"HP: {player.Hp}/{player.MaxHp}";
            GoldText.Text = $"💰 {player.Gold}";
        }
    }
}