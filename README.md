# 2048 Performance Testing

## Overview

This project is a console based 2048 implementation that includes a naive (non-heuristic) monte carlo solver with various implementations of the game's rules, from fully "functional" (slow) to more imperative (fast).

Since the solver is CPU bound, it's performance is heavily affected by the choice of game implementation.

The project is an excuse to try various (micro) optimization techniques, and is a work-in-progress.

## Results

``` ini

BenchmarkDotNet=v0.12.1, OS=Windows 10.0.18363.1379 (1909/November2018Update/19H2)
Intel Core i7-6700K CPU 4.00GHz (Skylake), 1 CPU, 8 logical and 4 physical cores
.NET Core SDK=5.0.102
  [Host]     : .NET Core 5.0.2 (CoreCLR 5.0.220.61120, CoreFX 5.0.220.61120), X64 RyuJIT DEBUG
  DefaultJob : .NET Core 5.0.2 (CoreCLR 5.0.220.61120, CoreFX 5.0.220.61120), X64 RyuJIT

```

|           Method | numRuns |      Mean |    Error |    StdDev |
|----------------- |-------- |----------:|---------:|----------:|
|    FunctionalSeq |      10 | 874.01 ms | 3.795 ms |  3.364 ms |
|   FunctionalPSeq |      10 | 369.90 ms | 6.789 ms | 10.962 ms |
|          DictSeq |      10 | 392.13 ms | 2.829 ms |  2.508 ms |
|         DictPSeq |      10 | 166.81 ms | 3.157 ms |  3.100 ms |
|         ArraySeq |      10 |  55.03 ms | 0.372 ms |  0.311 ms |
|        ArrayPSeq |      10 |  17.45 ms | 0.061 ms |  0.051 ms |
|        ArraySIMD |      10 |      TODO |     TODO |      TODO |
|      ArrayCUDAfy |      10 |      TODO |     TODO |      TODO |
