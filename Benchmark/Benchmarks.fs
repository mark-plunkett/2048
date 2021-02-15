module Benchmark 

open System
open BenchmarkDotNet
open BenchmarkDotNet.Attributes

open Common

[<MarkdownExporterAttribute.GitHub>]
type Benchmarks () =

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

    [<Params(10)>]
    member val public numRuns = 0 with get, set

    [<Benchmark>]
    member this.FunctionalSeq () =
        let board = GameFunctional.boardContext.CreateWithSeed size seed
        let dirFactory = MonteCarloSolver.genNextDir monteNumBranches monteNumMoves GameFunctional.boardContext
        runLoop dirFactory this.numRuns GameFunctional.boardContext board

    [<Benchmark>]
    member this.FunctionalPSeq () =
        let board = GameFunctional.boardContext.CreateWithSeed size seed
        let dirFactory = MonteCarloSolver.genNextDirPSeq monteNumBranches monteNumMoves GameFunctional.boardContext
        runLoop dirFactory this.numRuns GameFunctional.boardContext board

    [<Benchmark>]
    member this.DictSeq () =
        let board = GameDict.boardContext.CreateWithSeed size seed
        let dirFactory = MonteCarloSolver.genNextDir monteNumBranches monteNumMoves GameDict.boardContext
        runLoop dirFactory this.numRuns GameDict.boardContext board

    [<Benchmark>]
    member this.DictPSeq () =
        let board = GameDict.boardContext.CreateWithSeed size seed
        let dirFactory = MonteCarloSolver.genNextDirPSeq monteNumBranches monteNumMoves GameDict.boardContext
        runLoop dirFactory this.numRuns GameDict.boardContext board

    [<Benchmark>]
    member this.ArraySeq () =
        let board = GameArray.boardContext.CreateWithSeed size seed
        let dirFactory = MonteCarloSolver.genNextDir monteNumBranches monteNumMoves GameArray.boardContext
        runLoop dirFactory this.numRuns GameArray.boardContext board

    [<Benchmark>]
    member this.ArrayPSeq () =
        let board = GameArray.boardContext.CreateWithSeed size seed
        let dirFactory = MonteCarloSolver.genNextDirPSeq monteNumBranches monteNumMoves GameArray.boardContext
        runLoop dirFactory this.numRuns GameArray.boardContext board

    [<Benchmark>]
    member this.ArraySIMD () =
        ()

    [<Benchmark>]
    member this.ArrayCUDAfy () =
        ()