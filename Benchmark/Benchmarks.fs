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

    let contextFunctional = {
        TrySwipe = GameFunctional.trySwipe
        Clone = GameFunctional.Board.clone
        Create = GameFunctional.Board.create
        CreateWithSeed = fun size seed -> GameFunctional.Board.emptyWithSeed size seed |> GameFunctional.Board.init
        CanSwipe = GameFunctional.canSwipe
        ToString = GameFunctional.Board.toString
    }

    let contextDict = {
        TrySwipe = GameDict.trySwipe
        Clone = GameDict.Board.clone
        Create = GameDict.Board.create
        CreateWithSeed = fun size seed -> GameDict.Board.emptyWithSeed size seed |> GameDict.Board.init
        CanSwipe = GameDict.canSwipe
        ToString = GameDict.Board.toString
    }

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
        let board = contextFunctional.CreateWithSeed size seed
        let dirFactory = MonteCarloSolver.genNextDir monteNumBranches monteNumMoves contextFunctional
        runLoop dirFactory this.numRuns contextFunctional board

    [<Benchmark>]
    member this.RunMovesFunctionalPSeq () =
        let board = contextFunctional.CreateWithSeed size seed
        let dirFactory = MonteCarloSolver.genNextDirPSeq monteNumBranches monteNumMoves contextFunctional
        runLoop dirFactory this.numRuns contextFunctional board

    [<Benchmark>]
    member this.RunMovesDictSeq () =
        let board = contextDict.CreateWithSeed size seed
        let dirFactory = MonteCarloSolver.genNextDir monteNumBranches monteNumMoves contextDict
        runLoop dirFactory this.numRuns contextDict board

    [<Benchmark>]
    member this.RunMovesDictPSeq () =
        let board = contextDict.CreateWithSeed size seed
        let dirFactory = MonteCarloSolver.genNextDirPSeq monteNumBranches monteNumMoves contextDict
        runLoop dirFactory this.numRuns contextDict board

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
