module Benchmark 

open System
open BenchmarkDotNet
open BenchmarkDotNet.Attributes

open Common

[<MemoryDiagnoser>]
[<MarkdownExporterAttribute.GitHub>]
type Benchmarks () =

    let numRuns = 1

    let monteNumBranches = 100
    let monteNumMoves = 100

    let size = 4
    let r = Random()
    let seed = r.Next()

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

    //[<Benchmark(Baseline=true)>]
    member this.Functional () =
        runLoop dirFactoryFunctional numRuns GameFunctional.boardContext boardFunctional

    //[<Benchmark>]
    member this.Dict () =
        runLoop dirFactoryDict numRuns GameDict.boardContext boardDict

    //[<Benchmark>]
    member this.Array () =
        runLoop dirFactoryArray numRuns GameArray.boardContext boardArray

    //[<Benchmark>]
    member this.SIMD () =
        runLoop dirFactorySIMD numRuns GameSIMD.boardContext boardSIMD

    [<Benchmark>]
    member this.SIMDPlus () =
        runLoop dirFactorySIMDPlus numRuns GameSIMDPlus.boardContext boardSIMDPlus

    //[<Benchmark>]
    member this.FunctionalPSeq () =
        let board = GameFunctional.boardContext.CreateWithSeed size seed
        let dirFactory = MonteCarloSolver.genNextDirPSeq monteNumBranches monteNumMoves GameFunctional.boardContext
        runLoop dirFactory numRuns GameFunctional.boardContext board

    //[<Benchmark>]
    member this.DictPSeq () =
        let board = GameDict.boardContext.CreateWithSeed size seed
        let dirFactory = MonteCarloSolver.genNextDirPSeq monteNumBranches monteNumMoves GameDict.boardContext
        runLoop dirFactory numRuns GameDict.boardContext board

    //[<Benchmark>]
    member this.ArrayPSeq () =
        let board = GameArray.boardContext.CreateWithSeed size seed
        let dirFactory = MonteCarloSolver.genNextDirPSeq monteNumBranches monteNumMoves GameArray.boardContext
        runLoop dirFactory numRuns GameArray.boardContext board

    //[<Benchmark>]
    member this.SIMDPSeq () =
        let board = GameSIMD.boardContext.CreateWithSeed size seed
        let dirFactory = MonteCarloSolver.genNextDirPSeq monteNumBranches monteNumMoves GameSIMD.boardContext
        runLoop dirFactory numRuns GameSIMD.boardContext board

    //[<Benchmark>]
    member this.SIMDPlusPSeq () =
        let board = GameSIMDPlus.boardContext.CreateWithSeed size seed
        let dirFactory = MonteCarloSolver.genNextDirPSeq monteNumBranches monteNumMoves GameSIMDPlus.boardContext
        runLoop dirFactory numRuns GameSIMDPlus.boardContext board