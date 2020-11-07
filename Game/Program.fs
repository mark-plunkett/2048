open System

open Game

let origin = Console.WindowLeft, Console.WindowTop 

let dumpBoard origin board =
    Console.SetCursorPosition origin
    printfn "%s" (boardToString board)
    printfn ""

[<EntryPoint>]
let main argv =
    let size = Array.tryHead argv |> Option.defaultValue "4" |> int
    let board = newBoard size
    let trySwipe direction board =
        let newBoard = swipe size direction board
        if board = newBoard then board
        else addRandomCell newBoard

    let rec loop board =
        dumpBoard origin board |> ignore
        match canSwipe board with
        | false -> printfn "Game over :("
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