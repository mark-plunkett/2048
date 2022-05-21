module BuildIndexBenchmarks

open System
open BenchmarkDotNet
open BenchmarkDotNet.Attributes

open Common

[<MemoryDiagnoser>]
[<MarkdownExporterAttribute.GitHub>]
type BuildIndexBenchmarks () =

    let data = [|
        0s; 0s; 1s; 2s
        0s; 0s; 1s; 2s
        0s; 0s; 1s; 2s
        0s; 0s; 1s; 2s
    |]

    // [<Benchmark>]
    // member this.buildIndex () =
    //     let dataSpan = Span (data)
    //     GameSIMDBranchless.buildIndex dataSpan

    [<Benchmark>]
    member this.buildIndexBranchless () =
        let dataSpan = Span (data)
        GameSIMDBranchless.buildIndexBranchless dataSpan