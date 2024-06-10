# Pac-Man v C# s WPF

Implementace klasické retro hry Pac-Man s menšími zmìnami v C# s WFP.

## Dokumentace

### Enumerace

- `GameState` - Definuje rùzné stavy hry.
- `TileType` - Reprezentuje rùzné typy dladic na mapì.
- `ItemType` - Reprezentuje typy pøedmìtù, které mùe Pac-Man sbírat.
- `EntityType` - Reprezentuje rùzné typy entit ve høe: Pac-Man a 4 duchy (Blinky, Pinky, Inky, Clyde).
- `EntityDirection` - Urèuje smìr pohybu entit: nahoru, dolù, doleva, doprava.

### Tøídy

- `Entity` - Reprezentuje entitu ve høe s jejími vlastnostmi.
- `MainWindow` - Hlavní okno aplikace, obsahuje veškerou logiku hry.

### Metody

#### `Entity`

- `Entity(EntityType type, string[] uris)` - Konstruktor tøídy Entity.

#### `MainWindow`

- `MainWindow()` - Konstruktor hlavního okna.
- `InitGame()` - Inicializuje hru, nastavuje mapu, pozice entit a spouští èasovaè.
- `ResetEntityCoords()` - Nastaví poèáteèní souøadnice entit.
- `EntityStep(Entity entity)` - Posune entitu o krok na základì jejího aktuálního smìru.
- `EntityRandomStep(Entity entity)` - Posune entitu o krok náhodım smìrem.
- `EnemyStep(Entity enemy)` - Posune entitu nepøátelskou o krok smìrem k Pac-Manovi.
- `RenderFullImage(string uri)` - Vykreslí obrázek pøes celou herní møíku.
- `GameLoop(object sender, EventArgs e)` - Herní smyèka, která aktualizuje stav hry a vykresluje objekty. Volaná kadı tik èasovaèe (5x za sekundu).
- `HandleKeyStroke(object sender, KeyEventArgs e)` - Zpracovává stisk klávesy a mìní smìr Pacmana nebo stav hry.
- `ResizeGrid(object sender, SizeChangedEventArgs e)` - Pøizpùsobí velikost herní møíky podle velikosti okna.
- `SelectRandom<T>(T[] array)` - Vybere náhodnı prvek z pole.
- `SelectRandom<T>(List<T> list)` - Vybere náhodnı prvek z listu.
- `GenerateMap()` - Náhodnì generuje herní mapu pomocí upravené verze Binary Tree algoritmu, kterı má èasovou sloitost O(n) a prostorovou sloitost O(1) a umisuje na ni pøedmìty a entity
- `RenderTextureOntoGridTileAt((int x, int y) coords, string? uri)` - Vykreslí texturu na urèité souøadnice v herní møíce.
- `RenderTilesOntoGrid()` - Vykreslí všechny dladice na herní møíku.
- `RenderItemsOntoGrid()` - Vykreslí všechny pøedmìty na herní møíku.
- `RenderEntitiesOntoGrid()` - Vykreslí všechny entity na herní møíku.
- `RenderStatsOntoGrid()` - Vykreslí statistiky (poèet ivotù a mincí) na herní møíku.
- `ClearGrid()` - Vymae všechny prvky z herní møíky.
- `ClearFullImageFromGrid()` - Vymae obrázek z herní møíky.
- `ClearTextureFromGridTileAt((int x, int y) coords)` - Vymae texturu z urèité dladice na herní møíce.
- `ClearEntitiesFromGrid()` - Vymae všechny entity z herní møíky.
- `ClearStatsFromGrid()` - Vymae statistiky z herní møíky.
