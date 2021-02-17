module GameSIMDTests

open System

open Xunit
open FsCheck
open FsCheck.Xunit

open Common
open GameSIMD

let packData : obj[] seq =
    seq {
        yield [| 
            [| -1; 2; 0; 0; 0; -1 |]
            [| -1; 2; 0; 0; 0; -1 |]
            |]
        yield [| 
            [| -1; 2; 2; 0; 0; -1 |]
            [| -1; 2; 2; 0; 0; -1 |]
            |]
        yield [| 
            [| -1; 2; 0; 2; 0; -1 |]
            [| -1; 2; 2; 0; 0; -1 |]
            |]
        yield [| 
            [| -1; 2; 0; 0; 2; -1 |]
            [| -1; 2; 2; 0; 0; -1 |]
            |]
        yield [| 
            [| -1; 0; 2; 2; 0; -1 |]
            [| -1; 2; 2; 0; 0; -1 |]
            |]
        yield [| 
            [| -1; 0; 2; 2; 0; 4; 4; 0; 4; -1 |]
            [| -1; 2; 2; 0; 0; 4; 4; 4; 0; -1 |]
            |]
        yield [| 
            [| -1; 0; 2; 2; 0; 0; 0; 0; 4; -1 |]
            [| -1; 2; 2; 0; 0; 4; 0; 0; 0; -1 |]
            |]
        yield [| 
            [| -1; 2; 2; 2; 4; 4; 4; 8; 4; -1 |]
            [| -1; 2; 2; 2; 4; 4; 4; 8; 4; -1 |]
            |]
    }
[<Theory; MemberData(nameof packData)>]
let ``pack tests`` input expected =
    let result =
        input
        |> Array.map int16
        |> pack
    Assert.Equal<int16>(Array.map int16 expected, result)

let swipeSIMDData : obj[] seq =
    seq {
        yield [| 
            [2;4;8;16;0;0;0;0;0;0;0;0;0;0;0;0;]; 
            [2;4;8;16;0;0;0;0;0;0;0;0;0;0;0;0;] |]
        yield [| 
            [0;0;0;0;0;0;0;0;0;0;0;0;0;0;0;0;]; 
            [0;0;0;0;0;0;0;0;0;0;0;0;0;0;0;0;] |]
        yield [| 
            [2;2;8;16;0;0;0;0;0;0;0;0;0;0;0;0;]; 
            [4;0;8;16;0;0;0;0;0;0;0;0;0;0;0;0;] |]
        yield [| 
            [2;4;2;4;0;0;0;0;0;0;0;0;0;0;0;0;]; 
            [2;4;2;4;0;0;0;0;0;0;0;0;0;0;0;0;] |]
        yield [| 
            [2;4;2;0;0;0;0;0;0;0;0;0;0;0;0;0;]; 
            [2;4;2;0;0;0;0;0;0;0;0;0;0;0;0;0;] |]
        yield [| 
            [2;4;8;16;16;0;0;0;0;0;0;0;0;0;0;0;]; 
            [2;4;8;16;16;0;0;0;0;0;0;0;0;0;0;0;] |]
    }
[<Theory; MemberData(nameof swipeSIMDData)>]
let ``swipeSIMD works as expected`` row expected =
    let row' = Board.fromList [row]
    let expected' = Board.fromList [expected]
    // row' modified in place
    swipeSIMD row' |> ignore
    Assert.Equal<int16[]>(expected', row')

let swipeData : obj[] seq =
    seq {
        yield 
            [| [ 
                [ 2; 2; 0; 0 ]
                [ 2; 2; 0; 0 ]
                [ 2; 2; 0; 0 ]
                [ 2; 2; 0; 0 ] ];
            [
                [ 4; 0; 0; 0; ]
                [ 4; 0; 0; 0; ]
                [ 4; 0; 0; 0; ]
                [ 4; 0; 0; 0; ]
            ] |]
        yield 
            [| [ 
                [ 2; 0; 2; 0 ]
                [ 2; 2; 0; 2 ]
                [ 2; 2; 0; 0 ]
                [ 2; 0; 0; 0 ] ];
            [
                [ 4; 0; 0; 0; ]
                [ 4; 2; 0; 0; ]
                [ 4; 0; 0; 0; ]
                [ 2; 0; 0; 0; ]
            ] |]
    }
[<Theory; MemberData(nameof swipeData)>]
let ``swipe correctly merges cells`` cells expected =
    let board = Board.create 4
    let expected = Board.fromList expected
    let board' = { board with Cells = cells |> Board.fromList }
    swipe board' Left |> ignore
    Assert.Equal<int16[]>(expected, board'.Cells)
