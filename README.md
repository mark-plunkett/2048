# 2048 Performance Testing

## Overview

This project is a console based 2048 implementation that includes a naive (non-heuristic) monte carlo solver with various implementations of the game's rules, from fully "functional" (slow) to more imperative (fast).

Since the solver is CPU bound, it's performance is heavily affected by the choice of game implementation.

The project is an excuse to try various (micro) optimization techniques, and is a work-in-progress.

## Test

Each test run of the solver spawns 100 branches of the game from its current state, and plays 100 moves forward on each. This results in a total of 10,000 moves per run. The Mean time is the average time it takes to execute these 10,000 moves for each implementation.

## Results

``` ini

BenchmarkDotNet=v0.12.1, OS=Windows 10.0.18363.1379 (1909/November2018Update/19H2)
Intel Core i7-6700K CPU 4.00GHz (Skylake), 1 CPU, 8 logical and 4 physical cores
.NET Core SDK=5.0.102
  [Host]     : .NET Core 5.0.2 (CoreCLR 5.0.220.61120, CoreFX 5.0.220.61120), X64 RyuJIT DEBUG
  DefaultJob : .NET Core 5.0.2 (CoreCLR 5.0.220.61120, CoreFX 5.0.220.61120), X64 RyuJIT


```

|     Method |       Mean |     Error |    StdDev | Ratio |      Gen 0 | Gen 1 | Gen 2 | Allocated |
|----------- |-----------:|----------:|----------:|------:|-----------:|------:|------:|----------:|
| Functional | 101.767 ms | 0.5058 ms | 0.4223 ms |  1.00 | 35600.0000 |     - |     - | 142.07 MB |
|       Dict |  43.906 ms | 0.3057 ms | 0.2860 ms |  0.43 | 19500.0000 |     - |     - |  78.06 MB |
|      Array |   9.150 ms | 0.0338 ms | 0.0283 ms |  0.09 |  2828.1250 |     - |     - |  11.32 MB |
|       SIMD |   4.324 ms | 0.0144 ms | 0.0120 ms |  0.04 |   960.9375 |     - |     - |   3.86 MB |
|   SIMDPlus |   2.667 ms | 0.0074 ms | 0.0066 ms |  0.03 |   285.1563 |     - |     - |   1.14 MB |

## Method Detail

### SIMD

The following algorithm was used to vectorize the process of merging adjacent matching cells. The board is rotated and "packed" (non-zero values moved to the start of each row) before the merge, and packed again after the merge.

The board state consists of 16 cells, these are stored in a 1-dimensional array of int16s, which means the board state fits exactly into a 256bit SIMD register (16 values x 16 bits per value = 256) This array is bookended with values of -1 on each side to assist with the left and right "shifting" of the array in the algorithm, resulting in an array of length 18.

Instead of the full board, the steps below use a board with two rows (8 values) bookended with -1s. Values within square brackets indicate values used in each vector operation.

|#| Data | Step |
|-|-|-|
|1| `-1 [ 2 4 4 8 8 0 0 0 ] -1` | Game state bookended with -1s for shift |
|2|  `2 [ 4 4 8 8 0 0 0 -1 ]` | Shift left |
|3| `   [ 0 1 0 0 0 0 0 0 ] ` | Find matches using Vector.BitwiseAnd, ignore any matches at indicies 3, 7, 11, 15 since these would only have matched across row boundaries |
|4| `   [ 0 8 0 0 0 0 0 0 ] ` | Double values at mached indicies using Vector.ConditionalSelect and Vector.Multiply |
|5| `   [ -1 2 4 4 8 8 0 0 ] ` | Shift #1 right |
|6| `   [ 0 0 1 0 0 0 0 0 ] ` | Find matches compared to #1 using Vector.Equals |
|7| `   [ 2 4 0 8 8 0 0 0 ] ` | Use mask to zero matching values of #1, since the values of these matches will be merged to the left and doubled by #4 |
|8| `   [ 2 8 0 8 8 0 0 0 ] ` | Use Vector.Max  to combine #4 and #7 |

### SIMDPlus

Same core algorigthm as the SIMD method, with the following improvements:

- Simple array pooling to reduce allocations in various paths.
- Stack allocated arrays to reduce allocations.
- Loop unrolling.
- Sequential/deterministic generation of new board positions instead of System.Random.
- Function inlining.
