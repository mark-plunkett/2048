open System

open Game

let origin = Console.WindowLeft, Console.WindowTop 

let dumpBoard origin board =
    Console.SetCursorPosition origin
    printfn "%s" (boardToString board)
    printfn ""
    printfn "Score: %i" board.Score
    printfn ""

[<EntryPoint>]
let main argv =
    let size = Array.tryHead argv |> Option.defaultValue "4" |> int
    let board = newBoard size
    let rec loop board =
        dumpBoard origin board |> ignore
        match canSwipe board with
        | false -> printfn "Game over :(\n"
        | true -> 
            match Console.ReadKey().Key with
            | ConsoleKey.UpArrow -> trySwipe Up board
            | ConsoleKey.DownArrow -> trySwipe Down board
            | ConsoleKey.LeftArrow -> trySwipe Left board
            | ConsoleKey.RightArrow -> trySwipe Right board
            | _ -> board
            |> loop

    loop board

    0