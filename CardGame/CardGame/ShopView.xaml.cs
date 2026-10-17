using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using CardGame.Shop;
using CardGame.Char;
using CardGame.Cards;
using CardGame.Passives;

namespace CardGame.WPF
{
    public partial class ShopView : UserControl
    {
        private Player player;
        private ShopState shop;
        private Action onLeave;
        private Action<Player, Action<Card>> onRequestRemoveCard; // callback в MainWindow

        public ShopView()
        {
            InitializeComponent();
        }

        public void Setup(Player player, ShopState shop, Action onLeave,
                          Action<Player, Action<Card>> onRequestRemoveCard)
        {
            this.player = player;
            this.shop = shop;
            this.onLeave = onLeave;
            this.onRequestRemoveCard = onRequestRemoveCard;

            var rem = shop.RemoveItem;
            if (rem != null)
                rem.OnBuyRequested = p =>
                {
                    onRequestRemoveCard(p, card =>
                    {
                        // этот callback вызовется после выбора карты в MainWindow
                        p.Gold -= rem.Price;
                        p.Deck.Remove(card);
                        rem.Sold = true;
                        Refresh();
                    });
                };

            Refresh();
        }

        private void Refresh()
        {
            GoldText.Text = $"💰 {player.Gold}";

            var cardItems = shop.CardItems.ToList();

            // Первые 5 — в верхний ряд
            TopCardsPanel.Items.Clear();
            for (int i = 0; i < 5 && i < cardItems.Count; i++)
                TopCardsPanel.Items.Add(BuildCardItem(cardItems[i]));

            // Следующие 2 — в нижний ряд
            BottomCardsPanel.Items.Clear();
            for (int i = 5; i < cardItems.Count; i++)
                BottomCardsPanel.Items.Add(BuildCardItem(cardItems[i]));

            // Реликвии
            RelicsPanel.Items.Clear();
            foreach (var item in shop.RelicItems)
                RelicsPanel.Items.Add(BuildRelicItem(item));

            // Удаление
            var rem = shop.RemoveItem;
            if (rem != null)
            {
                RemoveCardButton.IsEnabled = rem.CanBuy(player);
                RemoveCardButton.Content = rem.Sold
                    ? "Sold Out"
                    : $"✂ Remove Card\n{rem.Price}g";
                RemoveCardButton.Opacity = rem.CanBuy(player) ? 1.0 : 0.5;
            }
        }

        private UIElement BuildCardItem(CardShopItem item)
        {
            var btn = new Button
            {
                Width = 140,
                Height = 220,
                Margin = new Thickness(10),
                IsEnabled = item.CanBuy(player),
                Opacity = item.CanBuy(player) ? 1.0 : 0.45,
                Background = new SolidColorBrush(Color.FromRgb(30, 30, 50)),
                BorderThickness = new Thickness(3),
                BorderBrush = BorderForCard(item.Card),
                Tag = item
            };

            var panel = new StackPanel { Margin = new Thickness(5) };
            panel.Children.Add(new TextBlock
            {
                Text = item.Card.Name,
                FontSize = 15,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.White,
                TextWrapping = TextWrapping.Wrap,
                TextAlignment = TextAlignment.Center
            });
            panel.Children.Add(new TextBlock
            {
                Text = item.Card.Description,
                FontSize = 12,
                Foreground = new SolidColorBrush(Color.FromRgb(180, 180, 200)),
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 8, 0, 0)
            });
            panel.Children.Add(new TextBlock
            {
                Text = item.Sold ? "SOLD" : $"{item.Price} g",
                FontSize = 16,
                FontWeight = FontWeights.Bold,
                Foreground = item.Sold ? Brushes.Gray : Brushes.Gold,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 15, 0, 0)
            });

            btn.Content = panel;
            btn.Click += (s, e) =>
            {
                item.Buy(player);
                Refresh();
            };
            return btn;
        }

        private UIElement BuildRelicItem(RelicShopItem item)
        {
            var btn = new Button
            {
                Width = 120,
                Height = 120,
                Margin = new Thickness(10),
                IsEnabled = item.CanBuy(player),
                Opacity = item.CanBuy(player) ? 1.0 : 0.45,
                Background = new SolidColorBrush(Color.FromRgb(40, 30, 60)),
                BorderThickness = new Thickness(3),
                BorderBrush = Brushes.Purple,
                Tag = item
            };

            var panel = new StackPanel { Margin = new Thickness(5) };
            panel.Children.Add(new TextBlock
            {
                Text = item.Relic.Name,
                FontSize = 13,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.White,
                TextWrapping = TextWrapping.Wrap,
                TextAlignment = TextAlignment.Center
            });
            panel.Children.Add(new TextBlock
            {
                Text = item.Sold ? "SOLD" : $"{item.Price} g",
                FontSize = 14,
                FontWeight = FontWeights.Bold,
                Foreground = item.Sold ? Brushes.Gray : Brushes.Gold,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 10, 0, 0)
            });

            btn.Content = panel;
            btn.ToolTip = item.Relic.Description;
            btn.Click += (s, e) =>
            {
                item.Buy(player);
                Refresh();
            };
            return btn;
        }

        private void RemoveCard_Click(object sender, RoutedEventArgs e)
        {
            var rem = shop.RemoveItem;
            if (rem == null || !rem.CanBuy(player)) return;
            rem.Buy(player); // вызовет callback, зарегистрированный в Setup
        }

        private void Leave_Click(object sender, RoutedEventArgs e)
        {
            onLeave?.Invoke();
        }

        private Brush BorderForCard(Card c) => c.Type switch
        {
            CardType.Attack => new SolidColorBrush(Color.FromRgb(200, 60, 60)),
            CardType.Skill => new SolidColorBrush(Color.FromRgb(60, 100, 200)),
            CardType.Power => new SolidColorBrush(Color.FromRgb(200, 180, 60)),
            _ => Brushes.Gray
        };
    }
}