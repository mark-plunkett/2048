open System
open System.Numerics

open Common

let gameSim () =
    let numRuns = 1000
    
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

let vTune_swipeSIMD (cells:int16[]) =
    ()

[<EntryPoint>]
let main argv =

    // let cells = [ 
    //     [ 2; 0; 2; 0 ]
    //     [ 2; 2; 0; 2 ]
    //     [ 2; 2; 0; 0 ]
    //     [ 2; 0; 0; 0 ] ]
    // let cells = [ 
    //     [ 0; 2; 3; 0 ]
    //     [ 0; 0; 0; 0 ]
    //     [ 0; 0; 0; 0 ]
    //     [ 0; 0; 0; 0 ] ]
    let cells = [ 
        [ 3; 2; 0; 0 ]
        [ 0; 0; 0; 0 ]
        [ 0; 0; 0; 0 ]
        [ 0; 0; 0; 0 ] ]
    let expected = [
        [ 3; 0; 0; 0; ]
        [ 3; 2; 0; 0; ]
        [ 3; 0; 0; 0; ]
        [ 2; 0; 0; 0; ]
    ]
    let board = GameSIMDBranchless.Board.create 4
    let expected = GameSIMDBranchless.Board.fromList expected
    let cells' = GameSIMDBranchless.Board.fromList cells 
    Array.iter (fun c -> printf "%i, " c) cells'
    printfn ""
    Array.blit cells' 0 board.Cells 0 18
    GameSIMDBranchless.swipe &board Left |> ignore

    0