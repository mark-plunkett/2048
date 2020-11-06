open System

open Game

let dumpBoard origin board =
    Console.SetCursorPosition(origin.X, origin.Y)
    printfn "%s" (boardToString board)
    printfn ""

[<EntryPoint>]
let main argv =
    let size = Array.tryHead argv |> Option.defaultValue "4" |> int
    let board = newBoard size
    let trySwipe direction board =
        let canSwipe = 
            match direction with
            | Up | Down -> canSwipeVertical board
            | Left | Right -> canSwipeHorizontal board
        
        match canSwipe with
        | true -> Some (board |> swipe size direction |> addRandomCell)
        | false -> None

    let origin = {
        X = Console.WindowLeft
        Y = Console.WindowTop
    }

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
            | _ -> Some board
            |> function
                | Some b -> loop b
                | None -> loop board

    loop board

    0