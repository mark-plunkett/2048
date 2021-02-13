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

    let context = {
        TrySwipe = Game.trySwipe
        Clone = Game.Board.clone
        Create = Game.Board.create
        CreateWithSeed = fun size seed -> Game.Board.emptyWithSeed size seed |> Game.Board.init
        CanSwipe = Game.canSwipe
        ToString = Game.Board.toString
    }

    let fastContext = {
        TrySwipe = FastGame.trySwipe
        Clone = FastGame.Board.clone
        Create = FastGame.Board.create
        CreateWithSeed = fun size seed -> FastGame.Board.emptyWithSeed size seed |> FastGame.Board.init
        CanSwipe = FastGame.canSwipe
        ToString = FastGame.Board.toString
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
    member this.RunMovesSeq () =
        let board = context.CreateWithSeed size seed
        let dirFactory = MonteCarloSolver.genNextDir monteNumBranches monteNumMoves context
        runLoop dirFactory this.numRuns context board

    [<Benchmark>]
    member this.RunMovesPSeq () =
        let board = context.CreateWithSeed size seed
        let dirFactory = MonteCarloSolver.genNextDirPSeq monteNumBranches monteNumMoves context
        runLoop dirFactory this.numRuns context board

    [<Benchmark>]
    member this.RunMovesFastSeq () =
        let board = fastContext.CreateWithSeed size seed
        let dirFactory = MonteCarloSolver.genNextDir monteNumBranches monteNumMoves fastContext
        runLoop dirFactory this.numRuns fastContext board

    [<Benchmark>]
    member this.RunMovesFastPSeq () =
        let board = fastContext.CreateWithSeed size seed
        let dirFactory = MonteCarloSolver.genNextDirPSeq monteNumBranches monteNumMoves fastContext
        runLoop dirFactory this.numRuns fastContext board
