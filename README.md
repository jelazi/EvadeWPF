# EvadeWPF

Desktop implementation of the board game **Evade** with a computer opponent driven by the
MiniMax algorithm with alpha-beta pruning. School project, C# / WPF, 2017.

## The game

Evade is played on a 6×6 board. Each player starts with four pawns and two kings on their own
back rank. A player wins by walking one of their kings to the opposite edge of the board; kings
themselves may not capture, so they have to be escorted rather than charged through. Stones can
be frozen, and if all four kings end up frozen the game is a draw.

## What it does

- Human vs. human, human vs. computer, or computer vs. computer
- Search depth configurable per player, so the two engines can be pitted against each other at
  different strengths
- Move history with undo and redo
- Saving and loading a game to XML — board state, whose turn it is, the move history, and both
  players' AI settings
- Running move list in `A : 1` coordinate notation
- Bundled help file (`Evade.chm`)

## Implementation notes

| Piece | File |
|---|---|
| Game loop and turn management | `Manager.cs` |
| Board state, move generation, legality | `Board.cs` |
| MiniMax with alpha-beta pruning, position evaluation, end detection | `Controlor.cs` |
| Player, human or engine, with its own depth | `Player.cs` |
| Rendering and input | `Draw.cs`, `MouseClick.cs`, `MainWindow.xaml` |
| XML persistence | `Files.cs` |

Board squares are a `[Flags]` enum, so colour, stone type, and the transient UI states
(selected, legal target, frozen, best move) all live in one value per field.

The search runs on a second `Manager` instance that replays the whole move history from the
start of the game before exploring — simple, and it keeps the live game state untouched.

## Building

Open `EvadeWPF.sln` in Visual Studio and build. Targets .NET Framework on Windows; the only
external dependency, `Xceed.Wpf.Toolkit.dll`, is checked in under `EvadeWPF/Resources`.

## Status

Archived. Kept as a record of early work — the source comments are in Czech.
