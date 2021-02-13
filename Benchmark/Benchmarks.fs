module Benchmark 

open System
open BenchmarkDotNet
open BenchmarkDotNet.Attributes

type Benchmarks () =
    let monteNumBranches = 100
    let monteNumMoves = 100

    let r = Random()

    let board = 
        r.Next()
        |> Game.Board.emptyWithSeed 4 
        |> Game.Board.init

    let rec runLoop dirFactory numRuns board =
        if numRuns = 0 then
            board
        else
            dirFactory board
            |> Game.trySwipe board
            |> runLoop dirFactory (numRuns - 1)

    [<Params(10, 100)>]
    member val public numRuns = 0 with get, set

    [<Benchmark>]
    member this.RunMovesPSeq () =
        let dirFactory = MonteCarloSolver.genNextDirPSeq monteNumBranches monteNumMoves
        runLoop dirFactory this.numRuns board

    [<Benchmark>]
    member this.RunMovesSeq () =
        let dirFactory = MonteCarloSolver.genNextDirSeq monteNumBranches monteNumMoves
        runLoop dirFactory this.numRuns board