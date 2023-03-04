module Benchmark 

open System
open BenchmarkDotNet.Diagnosers
open BenchmarkDotNet.Attributes

open Common

[<HardwareCounters(HardwareCounter.BranchMispredictions, HardwareCounter.CacheMisses)>] 
[<MemoryDiagnoser>]
// [<MarkdownExporterAttribute.GitHub>]
type Benchmarks () =

    let numRuns = 1

    let monteNumBranches = 100
    let monteNumMoves = 100

    let size = 4
    let seed = 123

    let rec runLoop dirFactory numRuns context board =
        if numRuns = 0 then
            board
        else
            dirFactory board
            |> context.TrySwipe board
            |> runLoop dirFactory (numRuns - 1) context

    let boardFunctional = GameFunctional.boardContext.CreateWithSeed size seed
    let dirFactoryFunctional = MonteCarloSolver.genNextDir monteNumBranches monteNumMoves GameFunctional.boardContext

    let boardDict = GameDict.boardContext.CreateWithSeed size seed
    let dirFactoryDict = MonteCarloSolver.genNextDir monteNumBranches monteNumMoves GameDict.boardContext

    let boardArray = GameArray.boardContext.CreateWithSeed size seed
    let dirFactoryArray = MonteCarloSolver.genNextDir monteNumBranches monteNumMoves GameArray.boardContext

    let boardSIMD = GameSIMD.boardContext.CreateWithSeed size seed
    let dirFactorySIMD = MonteCarloSolver.genNextDir monteNumBranches monteNumMoves GameSIMD.boardContext

    let boardSIMDPlus = GameSIMDPlus.boardContext.CreateWithSeed size seed
    let dirFactorySIMDPlus = MonteCarloSolver.genNextDir monteNumBranches monteNumMoves GameSIMDPlus.boardContext

    let boardSIMDBranchless = GameSIMDBranchless.boardContext.CreateWithSeed size seed
    let dirFactorySIMDBranchless = MonteCarloSolver.genNextDir monteNumBranches monteNumMoves GameSIMDBranchless.boardContext

    //[<Benchmark(Baseline=true)>]
    member this.Functional () =
        runLoop dirFactoryFunctional numRuns GameFunctional.boardContext boardFunctional

    //[<Benchmark>]
    member this.Dict () =
        runLoop dirFactoryDict numRuns GameDict.boardContext boardDict

    // [<Benchmark>]
    member this.Array () =
        runLoop dirFactoryArray numRuns GameArray.boardContext boardArray

    //[<Benchmark>]
    member this.SIMD () =
        runLoop dirFactorySIMD numRuns GameSIMD.boardContext boardSIMD

    // [<Benchmark>]
    member this.SIMDPlus () =
        runLoop dirFactorySIMDPlus numRuns GameSIMDPlus.boardContext boardSIMDPlus

    [<Benchmark>]
    member this.SIMDBranchless () =
        runLoop dirFactorySIMDBranchless numRuns GameSIMDBranchless.boardContext boardSIMDBranchless