module MonteCarloSolver

open System

open FSharp.Collections.ParallelSeq

open Common

type Run = {
    InitialDirection: Direction
    Score: int
}

let randomDir context board =
    let random = context.RNG board
    match random.Next(0, 4) with
    | 0 -> Direction.Up
    | 1 -> Direction.Down
    | 2 -> Direction.Left
    | 3 -> Direction.Right
    | x -> failwithf "Unsupported: %i" x

let runMoves num context board =
    let directions = [ for _ in [1..num] do yield randomDir context board ]
    let finalBoard = List.fold context.TrySwipe board directions
    {
        InitialDirection = directions.Head
        Score = context.Score finalBoard
    }

let genNextDir numBranches numMoves context board =
    [1..numBranches]
    |> Seq.map (fun _ -> runMoves numMoves context (context.Clone board))
    |> Seq.toList
    |> Seq.groupBy (fun run -> run.InitialDirection)
    |> Seq.maxBy (snd >> Seq.averageBy (fun run -> float run.Score))
    |> fst

let genNextDirPSeq numBranches numMoves context board =
    [1..numBranches]
    |> PSeq.map (fun _ -> runMoves numMoves context (context.Clone board))
    |> Seq.toList
    |> Seq.groupBy (fun run -> run.InitialDirection)
    |> Seq.maxBy (snd >> Seq.averageBy (fun run -> float run.Score))
    |> fst