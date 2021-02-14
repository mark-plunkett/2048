open System

open GameArray

[<EntryPoint>]
let main argv =

    let cells = Board.fromList [ 
        [ 2; 0; 2; 0 ]
        [ 2; 2; 0; 2 ]
        [ 2; 2; 0; 0 ]
        [ 2; 0; 0; 0 ] ] 
    //let expected = Board.fromList [[2;8;16;0]]
    let b = Board.create 4
    let b' = { b with Cells = cells }
    swipe b' Common.Direction.Left


    0