open System

open Game

let origin = Console.WindowLeft, Console.WindowTop 

let dumpBoard origin board =
    Console.SetCursorPosition origin
    printfn "%s" (boardToString board)
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
    let board = newBoard (args.GetResult(Args.Size, defaultValue = 4))
    let directionFactory =
        match args.TryGetResult(Args.MonteCarlo) with
        | Some (branches, depth) -> MonteCarloSolver.generateNextDirection branches depth
        | None -> getKeyboardDirection

    let rec loop board =
        dumpBoard origin board |> ignore
        match canSwipe board with
        | false -> printfn "Game over :(\n"
        | true -> 
            let direction = directionFactory board
            trySwipe direction board
            |> loop

    loop board

    0