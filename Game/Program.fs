open System

open Common
open MonteCarloSolver

let origin = Console.WindowLeft, Console.WindowTop 

let dumpBoard origin (boardContext:BoardContext<_>) board =
    Console.SetCursorPosition origin
    printfn "%s" (boardContext.ToString board)
    printfn ""
    printfn "Score: %i" board.Score
    printfn ""

let rec getKeyboardDirection board =
    match Console.ReadKey().Key with
    | ConsoleKey.UpArrow -> Up
    | ConsoleKey.DownArrow -> Down
    | ConsoleKey.LeftArrow -> Left
    | ConsoleKey.RightArrow -> Right
    | _ -> getKeyboardDirection board

[<EntryPoint>]
let main argv =
    let argsParser = Argu.ArgumentParser.Create<Args.Args>(programName = "2048.exe")
    let args = argsParser.Parse argv
    let boardContext = {
        TrySwipe = GameDict.trySwipe
        Clone = GameDict.Board.clone
        Create = GameDict.Board.create
        CreateWithSeed = fun i j -> GameDict.Board.create i
        CanSwipe = GameDict.canSwipe
        ToString = GameDict.Board.toString
    }

    let board = boardContext.Create (args.GetResult(Args.Size, defaultValue = 4))
    let directionFactory =
        match args.TryGetResult(Args.MonteCarlo) with
        | Some (branches, depth) -> MonteCarloSolver.genNextDir branches depth boardContext
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