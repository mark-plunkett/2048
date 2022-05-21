open System
open BenchmarkDotNet.Running
open Benchmark
open BuildIndexBenchmarks

[<EntryPoint>]
let main argv =
    BenchmarkRunner.Run<Benchmarks>() |> ignore
    0 // return an integer exit code