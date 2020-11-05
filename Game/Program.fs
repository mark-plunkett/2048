open System

open Game

let test () =
    Game.newBoardFromList [
        [ 0; 0; 2; 4 ]
        [ 2; 0; 2; 4 ]
        [ 2; 0; 2; 0 ]
        [ 0; 2; 8; 16 ]
    ] 

[<EntryPoint>]
let main argv =
    let size = Array.tryHead argv |> Option.defaultValue "4" |> int
    let board = newBoard size
    let swipe = swipe size
    let origin = {
        X = Console.WindowLeft
        Y = Console.WindowTop
    }

    let rec loop board =
        dumpBoard origin board |> ignore
        let newBoard =
            match Console.ReadKey().Key with
            | ConsoleKey.UpArrow -> swipe Up board
            | ConsoleKey.DownArrow -> swipe Down board
            | ConsoleKey.LeftArrow -> swipe Left board
            | ConsoleKey.RightArrow -> swipe Right board
            | _ -> board
        
        newBoard
        |> addRandomCell
        |> loop

    loop board

    0