open BenchmarkDotNet.Running
open Benchmark

[<EntryPoint>]
let main argv =
    BenchmarkRunner.Run<Benchmarks>() |> ignore
    0