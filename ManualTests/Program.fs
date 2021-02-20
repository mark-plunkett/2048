open System
open System.Numerics

open Common

[<EntryPoint>]
let main argv =

    let numRuns = 100

    let monteNumBranches = 100
    let monteNumMoves = 100

    let size = 4
    let r = Random()
    let seed = r.Next()

    let context = GameSIMDPlus.boardContext

    let rec runLoop dirFactory numRuns (context:BoardContext<_>) board =
        if numRuns = 0 then
            board
        else
            dirFactory board
            |> context.TrySwipe board
            |> runLoop dirFactory (numRuns - 1) context

    let board = context.CreateWithSeed size seed
    let dirFactory = MonteCarloSolver.genNextDir monteNumBranches monteNumMoves context
    runLoop dirFactory numRuns context board |> ignore

    0