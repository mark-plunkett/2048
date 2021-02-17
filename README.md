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

|         Method | numRuns |      Mean |     Error |    StdDev |
|--------------- |-------- |----------:|----------:|----------:|
|  FunctionalSeq |       1 | 97.423 ms | 1.8068 ms | 1.6017 ms |
| FunctionalPSeq |       1 | 37.807 ms | 0.4686 ms | 0.4154 ms |
|        DictSeq |       1 | 41.006 ms | 0.6326 ms | 0.5918 ms |
|       DictPSeq |       1 | 17.106 ms | 0.1348 ms | 0.1261 ms |
|       ArraySeq |       1 |  9.583 ms | 0.1644 ms | 0.1538 ms |
|      ArrayPSeq |       1 |  3.358 ms | 0.0334 ms | 0.0313 ms |
|           SIMD |       1 |  4.999 ms | 0.0858 ms | 0.0802 ms |
|       SIMDPSeq |       1 |  1.584 ms | 0.0068 ms | 0.0064 ms |
|         CUDAfy |       - |      TODO |      TODO |      TODO |
