module MonteCarloSolver

open System

open Game

type Run = {
    InitialDirection: Direction
    Score: int
}

let random = Random()
let randomDir () =
    match random.Next(0, 4) with
    | 0 -> Up
    | 1 -> Down
    | 2 -> Left
    | 3 -> Right
    | x -> failwithf "Unsupported: %i" x

let runMoves num board =
    let directions = [ for _ in [1..num] do yield randomDir() ]
    let finalBoard = List.fold (fun board direction -> trySwipe direction board) board directions
    {
        InitialDirection = directions.Head
        Score = finalBoard.Score
    }

let generateNextDirection numBranches numMoves board =
    [1..numBranches]
    |> List.map (fun _ -> runMoves numMoves board)
    |> List.groupBy (fun run -> run.InitialDirection)
    |> List.maxBy (snd >> List.averageBy (fun run -> float run.Score))
    |> fst