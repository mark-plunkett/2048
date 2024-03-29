﻿open System

open Common
open MonteCarloSolver

let initializeConsole () =
    Console.CursorVisible <- false
    Console.Clear ()
    Console.WindowLeft, Console.WindowTop 

let dumpBoard origin (boardContext:BoardContext<_>) board =
    Console.SetCursorPosition origin
    printfn "%s" (boardContext.ToString board)
    printfn ""
    printfn "Score: %i" (boardContext.Score board)
    printfn ""

let rec getKeyboardDirection board =
    match Console.ReadKey().Key with
    | ConsoleKey.UpArrow -> Direction.Up
    | ConsoleKey.DownArrow -> Direction.Down
    | ConsoleKey.LeftArrow -> Direction.Left
    | ConsoleKey.RightArrow -> Direction.Right
    | _ -> getKeyboardDirection board

[<EntryPoint>]
let main argv =
    let argsParser = Argu.ArgumentParser.Create<Args.Args>(programName = "2048.exe")
    let args = argsParser.Parse argv
    let origin = initializeConsole ()
    let boardContext = GameSIMDBranchless.boardContext
    let board = boardContext.Create (args.GetResult(Args.Size, defaultValue = 4))
    let directionFactory =
        match args.TryGetResult(Args.MonteCarlo) with
        | Some (branches, depth) -> FastMonteCarloSolver.genNextDir branches depth boardContext
        | None -> getKeyboardDirection

    let rec loop board =
        dumpBoard origin boardContext board |> ignore
        match boardContext.CanSwipe board with
        | false -> printfn "Game over :(\n"
        | true -> 
            directionFactory board
            |> boardContext.TrySwipe board
            |> loop

    loop board

    0