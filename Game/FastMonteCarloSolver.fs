module FastMonteCarloSolver

#nowarn "9"

open Microsoft.FSharp.NativeInterop
open System

open Common

[<Struct>]
type Run = {
    InitialDirection: Direction
    Score: int
}

[<Struct>]
type RunSummary = {
    mutable TotalScore: int
    mutable NumRuns: int
}

let randomDir context board =
    let random = context.RNG board
    enum<Direction>(random.Next(0, 4))

let runMoves num context board =
    let initialDir = randomDir context board
    let mutable currentBoard = context.TrySwipe board initialDir
    for _ = 2 to num do
        let dir = randomDir context currentBoard
        currentBoard <- context.TrySwipe currentBoard dir
    
    {
        InitialDirection = initialDir
        Score = context.Score currentBoard
    }

let genNextDir numBranches numMoves context board =
    let mem = NativePtr.stackalloc<RunSummary> (4)
    let summaries = Span<RunSummary> (NativePtr.toVoidPtr mem, 4)
    for _ = 1 to numBranches do
        let run = runMoves numMoves context (context.Clone board)
        let direction = int run.InitialDirection
        summaries[direction].NumRuns <- summaries[direction].NumRuns + 1
        summaries[direction].TotalScore <- summaries[direction].TotalScore + run.Score

    let mutable finalDir = 0
    let mutable maxScore = 0.0
    for i = 0 to summaries.Length - 1 do
        let summary = summaries[i]
        let average = (float)summary.TotalScore / (float)summary.NumRuns
        if average > maxScore then
            finalDir <- i
            maxScore <- average
        
    enum<Direction>(finalDir)