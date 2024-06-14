using System;
using System.Collections.Generic;
using System.Media;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace Pacman {
    enum GameState: byte {
        MainMenu,
        PauseMenu,
        HitPause,
        KillScreen,
        WinSceen,
        Fun,
    }
    enum TileType: byte {
        Wall,
        Empty,
        Portal,
    }
    enum ItemType: byte {
        Dot,
        Coin,
        Heart,
    }
    enum EntityType: byte {
        Pacman,
        Blinky,
        Pinky,
        Inky,
        Clyde,
    }
    enum EntityDirection: byte {
        Up,
        Down,
        Left,
        Right,
    }
    class Entity {
        public EntityType Type { get; init; }
        private string[] uris;
        public string Uri => uris[(int)Direction];
        public (int x, int y) Coords;
        public EntityDirection  Direction;
        public EntityDirection? NextDirection;
        public Entity(EntityType type, string[] uris) {
            Type      = type;
            this.uris = uris;
        }
    }
    public partial class MainWindow : Window {
        private DispatcherTimer timer;
        private readonly static SoundPlayer musicPlayer  = new("assets/music.wav"),
                                            munchPlayer  = new("assets/munch.wav"),
                                         // nomnomPlayer = new("assets/nomnom.wav"),
                                            ouchPlayer   = new("assets/ouch.wav");
        private readonly Random random = new();
        private GameState gameState;
        private const int mapWidth   = 32,
                          mapHeight  = 32,
                          gridWidth  = mapWidth,
                          gridHeight = mapHeight + 1;
        private (TileType tile, ItemType? item)[,] map;
        private List<(int x, int y)> portalsCoords;
        private bool teleported;
        private int lives, coins, dots;
        private readonly static string dotUri        = "assets/dot.png",
                                       coinUri       = "assets/coin.png",
                                       moneyUri      = "assets/money.png",
                                       heartFullUri  = "assets/heart-full.png",
                                       heartHalfUri  = "assets/heart-half.png",
                                       mainMenuUri   = "assets/main-menu.png",
                                       pauseMenuUri  = "assets/pause-menu.png",
                                       killScreenUri = "assets/kill-screen.png",
                                       winScreenUri  = "assets/win-screen.png";
        private readonly static string[] wallTileUris   = { "assets/wall-tile-01.png",  "assets/wall-tile-02.png",  "assets/wall-tile-03.png", "assets/wall-tile-04.png" },
                                         emptyTileUris  = { "assets/empty-tile-01.png", "assets/empty-tile-02.png", "assets/empty-tile-03.png" },
                                         portalTileUris = { "assets/portal-tile-01.png" },
                                         pacmanUris     = { "assets/pacman-up.png",   "assets/pacman-down.png",  "assets/pacman-left.png", "assets/pacman-right.png" },
                                         blinkyUris     = { "assets/blinky-left.png", "assets/blinky-right.png", "assets/blinky-left.png", "assets/blinky-right.png" },
                                         pinkyUris      = { "assets/pinky-left.png",  "assets/pinky-right.png",  "assets/pinky-left.png",  "assets/pinky-right.png"  },
                                         inkyUris       = { "assets/inky-left.png",   "assets/inky-right.png",   "assets/inky-left.png",   "assets/inky-right.png"   },
                                         clydeUris      = { "assets/clyde-left.png",  "assets/clyde-right.png",  "assets/clyde-left.png",  "assets/clyde-right.png"  };
        private Entity pacman = new(EntityType.Pacman, pacmanUris),
                       blinky = new(EntityType.Blinky, blinkyUris),
                       pinky  = new(EntityType.Pinky,  pinkyUris),
                       inky   = new(EntityType.Inky,   inkyUris),
                       clyde  = new(EntityType.Clyde,  clydeUris);
        public MainWindow() {
            InitializeComponent();
            for (int column = 0; column < gridWidth;  column += 1) grid.ColumnDefinitions.Add(new ColumnDefinition());
            for (int row    = 0; row    < gridHeight; row    += 1) grid.RowDefinitions.Add(new RowDefinition());
            timer = new();
            timer.Interval = TimeSpan.FromMilliseconds(1000 / 5);
            timer.Tick += GameLoop;
            // CompositionTarget.Rendering += RenderLoop
            gameState = GameState.MainMenu;
            InitGame();
        }
        // private void RenderLoop(object sender, EventArgs e) {}
        private void InitGame() {
            map           = new (TileType, ItemType?)[mapWidth, mapHeight];
            portalsCoords = new();
            teleported    = false;
            lives         = 6;
            coins         = 0;
            GenerateMap();
            ResetEntityCoords();
            RenderTilesOntoGrid();
            RenderItemsOntoGrid();
            RenderEntitiesOntoGrid();
            RenderStatsOntoGrid();
            timer.Start();
        }
        private void ResetEntityCoords() { 
            pacman.Coords = (mapWidth / 2, mapHeight / 2);
            blinky.Coords = (3,            3);
            pinky.Coords  = (mapWidth - 4, 3);
            inky.Coords   = (3,            mapHeight - 4);
            clyde.Coords  = (mapWidth - 4, mapHeight - 4);
        }
        private void EntityStep(Entity entity) {
            if (entity.NextDirection != null && entity.NextDirection switch {
                EntityDirection.Up    => (map[entity.Coords.x,     entity.Coords.y - 1].tile != TileType.Wall),
                EntityDirection.Down  => (map[entity.Coords.x,     entity.Coords.y + 1].tile != TileType.Wall),
                EntityDirection.Left  => (map[entity.Coords.x - 1, entity.Coords.y].tile     != TileType.Wall),
                EntityDirection.Right => (map[entity.Coords.x + 1, entity.Coords.y].tile     != TileType.Wall),
            }) entity.Direction = (EntityDirection)entity.NextDirection;
            switch (entity.Direction) {
                case EntityDirection.Up:    if (map[entity.Coords.x, entity.Coords.y - 1].tile != TileType.Wall) entity.Coords.y -= 1; break;
                case EntityDirection.Down:  if (map[entity.Coords.x, entity.Coords.y + 1].tile != TileType.Wall) entity.Coords.y += 1; break;
                case EntityDirection.Left:  if (map[entity.Coords.x - 1, entity.Coords.y].tile != TileType.Wall) entity.Coords.x -= 1; break;
                case EntityDirection.Right: if (map[entity.Coords.x + 1, entity.Coords.y].tile != TileType.Wall) entity.Coords.x += 1; break;
            }
        }
        private void EntityRandomStep(Entity entity) {
            List<EntityDirection> possibleDirections = new();
            if (map[entity.Coords.x, entity.Coords.y - 1].tile == TileType.Empty) possibleDirections.Add(EntityDirection.Up);
            if (map[entity.Coords.x, entity.Coords.y + 1].tile == TileType.Empty) possibleDirections.Add(EntityDirection.Down);
            if (map[entity.Coords.x - 1, entity.Coords.y].tile == TileType.Empty) possibleDirections.Add(EntityDirection.Left);
            if (map[entity.Coords.x + 1, entity.Coords.y].tile == TileType.Empty) possibleDirections.Add(EntityDirection.Right);
            entity.Direction = SelectRandom(possibleDirections);
            EntityStep(entity);
        }
        private void EnemyStep(Entity enemy) {
            if ((enemy.Direction switch {
                EntityDirection.Up    => (map[enemy.Coords.x - 1, enemy.Coords.y].tile != TileType.Empty && map[enemy.Coords.x + 1, enemy.Coords.y].tile != TileType.Empty && map[enemy.Coords.x, enemy.Coords.y - 1].tile == TileType.Empty),
                EntityDirection.Down  => (map[enemy.Coords.x - 1, enemy.Coords.y].tile != TileType.Empty && map[enemy.Coords.x + 1, enemy.Coords.y].tile != TileType.Empty && map[enemy.Coords.x, enemy.Coords.y + 1].tile == TileType.Empty),
                EntityDirection.Left  => (map[enemy.Coords.x, enemy.Coords.y - 1].tile != TileType.Empty && map[enemy.Coords.x, enemy.Coords.y + 1].tile != TileType.Empty && map[enemy.Coords.x - 1, enemy.Coords.y].tile == TileType.Empty),
                EntityDirection.Right => (map[enemy.Coords.x, enemy.Coords.y - 1].tile != TileType.Empty && map[enemy.Coords.x, enemy.Coords.y + 1].tile != TileType.Empty && map[enemy.Coords.x + 1, enemy.Coords.y].tile == TileType.Empty),
            }) && random.NextDouble() > 0.8) EntityStep(enemy);
            else {
                (int  x, int  y) absCoordsDiff  = (Math.Abs(pacman.Coords.x - enemy.Coords.x), Math.Abs(pacman.Coords.y - enemy.Coords.y));
                (bool x, bool y) boolCoordsDiff = (pacman.Coords.x < enemy.Coords.x,           pacman.Coords.y < enemy.Coords.y);
                if (absCoordsDiff.x + absCoordsDiff.y <= 1) enemy.Coords = pacman.Coords;
                else if (absCoordsDiff.x > absCoordsDiff.y) {
                    if (      boolCoordsDiff.x && map[enemy.Coords.x - 1, enemy.Coords.y].tile == TileType.Empty) enemy.Coords.x -= 1;
                    else if (!boolCoordsDiff.x && map[enemy.Coords.x + 1, enemy.Coords.y].tile == TileType.Empty) enemy.Coords.x += 1;
                    else if ( boolCoordsDiff.y && map[enemy.Coords.x, enemy.Coords.y - 1].tile == TileType.Empty) enemy.Coords.y -= 1;
                    else if (!boolCoordsDiff.y && map[enemy.Coords.x, enemy.Coords.y + 1].tile == TileType.Empty) enemy.Coords.y += 1;
                    else EntityRandomStep(enemy);
                } else {
                    if (      boolCoordsDiff.y && map[enemy.Coords.x, enemy.Coords.y - 1].tile == TileType.Empty) enemy.Coords.y -= 1;
                    else if (!boolCoordsDiff.y && map[enemy.Coords.x, enemy.Coords.y + 1].tile == TileType.Empty) enemy.Coords.y += 1;
                    else if ( boolCoordsDiff.x && map[enemy.Coords.x - 1, enemy.Coords.y].tile == TileType.Empty) enemy.Coords.x -= 1;
                    else if (!boolCoordsDiff.x && map[enemy.Coords.x + 1, enemy.Coords.y].tile == TileType.Empty) enemy.Coords.x += 1;
                    else EntityRandomStep(enemy);
                }
            }
        }
        private void RenderFullImage(string uri) {
            Image image = new();
            // The grid is rotated for some reason???
            Grid.SetRow(image,        0);
            Grid.SetColumn(image,     0);
            Grid.SetRowSpan(image,    gridHeight);
            Grid.SetColumnSpan(image, gridWidth);
            image.HorizontalAlignment = HorizontalAlignment.Stretch;
            image.VerticalAlignment   = VerticalAlignment.Stretch;
            if (uri != null) {
                BitmapImage bitmap = new();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(uri, UriKind.Relative);
                bitmap.EndInit();
                image.Source = bitmap;
            }
            grid.Children.Add(image);
        }
        private void GameLoop(object sender, EventArgs e) {
            switch (gameState) {
                case GameState.MainMenu:
                    timer.Stop();
                    RenderFullImage(mainMenuUri);
                    musicPlayer.PlayLooping();
                break;
                case GameState.PauseMenu:
                    timer.Stop();
                    RenderFullImage(pauseMenuUri);
                    munchPlayer.Stop();
                    musicPlayer.PlayLooping();
                break;
                case GameState.HitPause:
                    timer.Stop();
                    munchPlayer.Stop();
                    ouchPlayer.PlaySync();
                    ResetEntityCoords();
                    musicPlayer.PlayLooping();
                    // ClearStatsFromGrid();
                    // ClearEntitiesFromGrid();
                    // RenderEntitiesOntoGrid();
                    // RenderStatsOntoGrid();
                break;
                case GameState.KillScreen:
                    timer.Stop();
                    munchPlayer.Stop();
                    ouchPlayer.PlaySync();
                    RenderFullImage(killScreenUri);
                    musicPlayer.PlayLooping();
                break;
                case GameState.WinSceen:
                    timer.Stop();
                    RenderFullImage(winScreenUri);
                    munchPlayer.Stop();
                    musicPlayer.PlayLooping();
                break;
                case GameState.Fun:
                    ClearStatsFromGrid();
                    ClearEntitiesFromGrid();
                    EnemyStep(blinky);
                    EnemyStep(pinky);
                    EnemyStep(inky);
                    EnemyStep(clyde);
                    EntityStep(pacman);
                    // RenderEntitiesOntoGrid();
                    // RenderStatsOntoGrid();
                    if (
                        pacman.Coords == blinky.Coords ||
                        pacman.Coords == pinky.Coords  ||
                        pacman.Coords == inky.Coords   ||
                        pacman.Coords == clyde.Coords
                    ) {
                        if ((lives -= 2) > 0) gameState = GameState.HitPause;
                        else                  gameState = GameState.KillScreen;
                        RenderEntitiesOntoGrid();
                        RenderStatsOntoGrid();
                        return;
                    }
                    if (map[pacman.Coords.x, pacman.Coords.y].tile == TileType.Portal && portalsCoords.Count > 1) {
                        (int, int) newPacmanCoords;
                        while ((newPacmanCoords = SelectRandom(portalsCoords)) == pacman.Coords) {}
                        if (!teleported) pacman.Coords = newPacmanCoords;
                        teleported = true;
                    } else teleported = false;
                    switch (map[pacman.Coords.x, pacman.Coords.y].item) {
                        case ItemType.Dot:   ClearTextureFromGridTileAt(pacman.Coords); if ((dots -= 1) < 80) gameState = GameState.WinSceen; break; // 31 * 31 / 4 ~ 240 and 240 / 3 = 80
                        case ItemType.Coin:  ClearTextureFromGridTileAt(pacman.Coords); coins += 1; break;
                        case ItemType.Heart: ClearTextureFromGridTileAt(pacman.Coords); lives += 1; break;
                    }
                    map[pacman.Coords.x, pacman.Coords.y].item = null;
                    RenderEntitiesOntoGrid();
                    RenderStatsOntoGrid();
                break;
            }
        }
        private void HandleKeyStroke(object sender, KeyEventArgs e) {
            switch (e.Key) {
                case Key.Up:     pacman.NextDirection = EntityDirection.Up;    break;
                case Key.Down:   pacman.NextDirection = EntityDirection.Down;  break;
                case Key.Left:   pacman.NextDirection = EntityDirection.Left;  break;
                case Key.Right:  pacman.NextDirection = EntityDirection.Right; break;
                case Key.Escape: gameState            = GameState.PauseMenu;   return;
                default:                                                       return;
            }
            if ((gameState == GameState.MainMenu || gameState == GameState.PauseMenu) && !timer.IsEnabled) {
                ClearFullImageFromGrid();
                gameState = GameState.Fun;
                timer.Start();
                musicPlayer.Stop();
                munchPlayer.PlayLooping();
            } else if (gameState == GameState.HitPause && !timer.IsEnabled) {
                gameState = GameState.Fun;
                timer.Start();
                musicPlayer.Stop();
                munchPlayer.PlayLooping();
            } else if ((gameState == GameState.KillScreen || gameState == GameState.WinSceen) && !timer.IsEnabled) {
                ClearGrid();
                gameState = GameState.Fun;
                InitGame();
                musicPlayer.Stop();
                munchPlayer.PlayLooping();
            }
        }
        private void ResizeGrid(object sender, SizeChangedEventArgs e) {
            double minDimension = Math.Min(ActualWidth, ActualHeight);
            if (ActualWidth / ActualHeight < gridWidth / gridHeight) {
                grid.Width  = minDimension / 10 * 8;
                grid.Height = minDimension / gridWidth * gridHeight / 10 * 8;
            } else {
                grid.Width  = minDimension / gridHeight * gridWidth / 10 * 8;
                grid.Height = minDimension / 10 * 8;
            }
        }
        private T SelectRandom<T>(T[] array)    => array[random.Next(array.Length)];
        private T SelectRandom<T>(List<T> list) => list[random.Next(list.Count)];
        // Inspired by: https://github.com/professor-l/mazes/blob/master/scripts/binary-tree.js
        private void GenerateMap() {
            for (int y = 1; y < mapHeight - 1; y += 1)
                for (int x = 1; x < mapWidth - 1; x += 1)
                    if (x % 2 == 0 && y % 2 == 0) {
                        if (random.NextDouble() > 0.5) map[x - 1, y].tile = (random.NextDouble() > 0.3) ? TileType.Wall : TileType.Empty;
                        else                           map[x, y - 1].tile = (random.NextDouble() > 0.3) ? TileType.Wall : TileType.Empty;
                    } else                             map[x, y].tile     = TileType.Empty;
            for (int y = 1; y < mapHeight - 1; y += 1)
                for (int x = 1; x < mapWidth - 1; x += 1) {
                    if (x == mapWidth / 2 && y == mapHeight / 2) map[x, y].tile = TileType.Empty;
                    else if (
                        (x == 3            && y == 3)             ||
                        (x == mapWidth - 4 && y == 3)             ||
                        (x == 3            && y == mapHeight - 4) ||
                        (x == mapWidth - 4 && y == mapHeight - 4)
                    ) { map[x, y] = (TileType.Empty, ItemType.Dot); dots += 1; }
                    else if (map[x, y].tile == TileType.Empty)
                        if (
                            ((map[x - 1, y].tile == TileType.Wall) ? 1 : 0) +
                            ((map[x + 1, y].tile == TileType.Wall) ? 1 : 0) +
                            ((map[x, y - 1].tile == TileType.Wall) ? 1 : 0) +
                            ((map[x, y + 1].tile == TileType.Wall) ? 1 : 0) == 3
                        ) {
                            if (random.NextDouble() > 0.8) { map[x, y] = (TileType.Portal, null); portalsCoords.Add((x, y)); }
                            else { map[x, y].item = ItemType.Dot; dots += 1; }
                        } else if (random.NextDouble() > 0.97) map[x, y].item = ItemType.Coin;
                        else if (random.NextDouble() > 0.993)  map[x, y].item = ItemType.Heart;
                        else                                 { map[x, y].item = ItemType.Dot; dots += 1; }
                }
        }
        private void RenderTextureOntoGridTileAt((int x, int y) coords, string? uri) {
            Image tile = new();
            // The grid is rotated for some reason???
            Grid.SetRow(tile,    coords.y);
            Grid.SetColumn(tile, coords.x);
            tile.HorizontalAlignment = HorizontalAlignment.Stretch;
            tile.VerticalAlignment   = VerticalAlignment.Stretch;
            if (uri != null) {
                BitmapImage bitmap = new();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(uri, UriKind.Relative);
                bitmap.EndInit();
                tile.Source = bitmap;
            }
            grid.Children.Add(tile);
        }
        private void RenderTilesOntoGrid() {
            for (int y = 0; y < mapHeight; y += 1)
                for (int x = 0; x < mapWidth; x += 1)
                    RenderTextureOntoGridTileAt((x, y), SelectRandom<string>(map[x, y].tile switch {
                        TileType.Wall   => wallTileUris,
                        TileType.Empty  => emptyTileUris,
                        TileType.Portal => portalTileUris,
                    }));
        }
        private void RenderItemsOntoGrid() {
            for (int y = 0; y < mapHeight; y += 1)
                for (int x = 0; x < mapWidth; x += 1)
                    RenderTextureOntoGridTileAt((x, y), map[x, y].item switch {
                        null           => null,
                        ItemType.Dot   => dotUri,
                        ItemType.Coin  => coinUri,
                        ItemType.Heart => heartFullUri,
                    });
        }
        private void RenderEntitiesOntoGrid() {
            Entity[] entities = { pacman, blinky, pinky, inky, clyde };
            foreach (Entity entity in entities)
                RenderTextureOntoGridTileAt(entity.Coords, entity.Uri);
        }
        private void RenderStatsOntoGrid() {
            for (int x = 0; x < lives / 2; x += 1)
                RenderTextureOntoGridTileAt((x, gridHeight - 1), heartFullUri);
            if (lives % 2 == 1)
                RenderTextureOntoGridTileAt((lives / 2, gridHeight - 1), heartHalfUri);
            for (int x = gridWidth - coins / 3 - coins % 3; x < gridWidth - coins % 3; x += 1)
                RenderTextureOntoGridTileAt((x, gridHeight - 1), moneyUri);
            for (int x = gridWidth - coins % 3; x < gridWidth; x += 1)
                RenderTextureOntoGridTileAt((x, gridHeight - 1), coinUri);
        }
        private void ClearGrid()                                       => grid.Children.Clear();
        private void ClearFullImageFromGrid()                          => grid.Children.RemoveAt(grid.Children.Count - 1);

        private void ClearTextureFromGridTileAt((int x, int y) coords) => (grid.Children[mapWidth * mapHeight + coords.x + coords.y * mapWidth] as Image).Source = null;
        private void ClearEntitiesFromGrid()                           => grid.Children.RemoveRange(grid.Children.Count - 5, 5);
        private void ClearStatsFromGrid()                              => grid.Children.RemoveRange(grid.Children.Count - (lives + 1) / 2 - coins / 3 - coins % 3, (lives + 1) / 2 + coins / 3 + coins % 3);
    }
}
