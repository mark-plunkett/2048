module Benchmark 

open System
open BenchmarkDotNet
open BenchmarkDotNet.Attributes

open Common

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
    member this.RunMovesFunctionalSeq () =
        let board = GameFunctional.boardContext.CreateWithSeed size seed
        let dirFactory = MonteCarloSolver.genNextDir monteNumBranches monteNumMoves GameFunctional.boardContext
        runLoop dirFactory this.numRuns GameFunctional.boardContext board

    [<Benchmark>]
    member this.RunMovesFunctionalPSeq () =
        let board = GameFunctional.boardContext.CreateWithSeed size seed
        let dirFactory = MonteCarloSolver.genNextDirPSeq monteNumBranches monteNumMoves GameFunctional.boardContext
        runLoop dirFactory this.numRuns GameFunctional.boardContext board

    [<Benchmark>]
    member this.RunMovesDictSeq () =
        let board = GameDict.boardContext.CreateWithSeed size seed
        let dirFactory = MonteCarloSolver.genNextDir monteNumBranches monteNumMoves GameDict.boardContext
        runLoop dirFactory this.numRuns GameDict.boardContext board

    [<Benchmark>]
    member this.RunMovesDictPSeq () =
        let board = GameDict.boardContext.CreateWithSeed size seed
        let dirFactory = MonteCarloSolver.genNextDirPSeq monteNumBranches monteNumMoves GameDict.boardContext
        runLoop dirFactory this.numRuns GameDict.boardContext board

    [<Benchmark>]
    member this.RunMovesArraySeq () =
        let board = GameArray.boardContext.CreateWithSeed size seed
        let dirFactory = MonteCarloSolver.genNextDir monteNumBranches monteNumMoves GameArray.boardContext
        runLoop dirFactory this.numRuns GameArray.boardContext board

    [<Benchmark>]
    member this.RunMovesArrayPSeq () =
        let board = GameArray.boardContext.CreateWithSeed size seed
        let dirFactory = MonteCarloSolver.genNextDirPSeq monteNumBranches monteNumMoves GameArray.boardContext
        runLoop dirFactory this.numRuns GameArray.boardContext board
