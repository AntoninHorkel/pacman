# Pac-Man v C# s WPF

Implementace klasick� retro hry Pac-Man s men��mi zm�nami v C# s WFP.

## Dokumentace

### Enumerace

- `GameState` - Definuje r�zn� stavy hry.
- `TileType` - Reprezentuje r�zn� typy dla�dic na map�.
- `ItemType` - Reprezentuje typy p�edm�t�, kter� m��e Pac-Man sb�rat.
- `EntityType` - Reprezentuje r�zn� typy entit ve h�e: Pac-Man a 4 duchy (Blinky, Pinky, Inky, Clyde).
- `EntityDirection` - Ur�uje sm�r pohybu entit: nahoru, dol�, doleva, doprava.

### T��dy

- `Entity` - Reprezentuje entitu ve h�e s jej�mi vlastnostmi.
- `MainWindow` - Hlavn� okno aplikace, obsahuje ve�kerou logiku hry.

### Metody

#### `Entity`

- `Entity(EntityType type, string[] uris)` - Konstruktor t��dy Entity.

#### `MainWindow`

- `MainWindow()` - Konstruktor hlavn�ho okna.
- `InitGame()` - Inicializuje hru, nastavuje mapu, pozice entit a spou�t� �asova�.
- `ResetEntityCoords()` - Nastav� po��te�n� sou�adnice entit.
- `EntityStep(Entity entity)` - Posune entitu o krok na z�klad� jej�ho aktu�ln�ho sm�ru.
- `EntityRandomStep(Entity entity)` - Posune entitu o krok n�hod�m sm�rem.
- `EnemyStep(Entity enemy)` - Posune entitu nep��telskou o krok sm�rem k Pac-Manovi.
- `RenderFullImage(string uri)` - Vykresl� obr�zek p�es celou hern� m��ku.
- `GameLoop(object sender, EventArgs e)` - Hern� smy�ka, kter� aktualizuje stav hry a vykresluje objekty. Volan� ka�d� tik �asova�e (5x za sekundu).
- `HandleKeyStroke(object sender, KeyEventArgs e)` - Zpracov�v� stisk kl�vesy a m�n� sm�r Pacmana nebo stav hry.
- `ResizeGrid(object sender, SizeChangedEventArgs e)` - P�izp�sob� velikost hern� m��ky podle velikosti okna.
- `SelectRandom<T>(T[] array)` - Vybere n�hodn� prvek z pole.
- `SelectRandom<T>(List<T> list)` - Vybere n�hodn� prvek z listu.
- `GenerateMap()` - N�hodn� generuje hern� mapu pomoc� upraven� verze Binary Tree algoritmu, kter� m� �asovou slo�itost O(n) a prostorovou slo�itost O(1) a umis�uje na ni p�edm�ty a entity
- `RenderTextureOntoGridTileAt((int x, int y) coords, string? uri)` - Vykresl� texturu na ur�it� sou�adnice v hern� m��ce.
- `RenderTilesOntoGrid()` - Vykresl� v�echny dla�dice na hern� m��ku.
- `RenderItemsOntoGrid()` - Vykresl� v�echny p�edm�ty na hern� m��ku.
- `RenderEntitiesOntoGrid()` - Vykresl� v�echny entity na hern� m��ku.
- `RenderStatsOntoGrid()` - Vykresl� statistiky (po�et �ivot� a minc�) na hern� m��ku.
- `ClearGrid()` - Vyma�e v�echny prvky z hern� m��ky.
- `ClearFullImageFromGrid()` - Vyma�e obr�zek z hern� m��ky.
- `ClearTextureFromGridTileAt((int x, int y) coords)` - Vyma�e texturu z ur�it� dla�dice na hern� m��ce.
- `ClearEntitiesFromGrid()` - Vyma�e v�echny entity z hern� m��ky.
- `ClearStatsFromGrid()` - Vyma�e statistiky z hern� m��ky.
