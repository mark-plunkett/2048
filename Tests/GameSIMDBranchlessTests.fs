module GameSIMDBranchlessTests

open System

open Xunit
open FsCheck
open FsCheck.Xunit
open Shouldly

open Common
open GameSIMDBranchless

let rotateData : obj[] seq =
    seq {
        yield 
            [| [ 
                [ 2; 0; 0; 0 ]
                [ 0; 0; 0; 0 ]
                [ 0; 0; 0; 0 ]
                [ 4; 0; 0; 0 ] ];
            [
                [ 0; 0; 0; 0; ]
                [ 0; 0; 0; 0; ]
                [ 0; 0; 0; 0; ]
                [ 2; 0; 0; 4; ]
            ];
            GameSIMDBranchless.Constants.antiClockwiseTransposeMap
        |]
        yield 
            [| [ 
                [ 2; 0; 0; 0 ]
                [ 0; 0; 0; 0 ]
                [ 0; 0; 0; 0 ]
                [ 4; 0; 0; 0 ] ];
            [
                [ 4; 0; 0; 2; ]
                [ 0; 0; 0; 0; ]
                [ 0; 0; 0; 0; ]
                [ 0; 0; 0; 0; ]
            ];
            GameSIMDBranchless.Constants.clockwiseTransposeMap
        |]
        yield 
            [| [ 
                [ 2; 0; 0; 0 ]
                [ 0; 0; 0; 0 ]
                [ 0; 0; 0; 0 ]
                [ 4; 0; 0; 0 ] ];
            [
                [ 0; 0; 0; 2; ]
                [ 0; 0; 0; 0; ]
                [ 0; 0; 0; 0; ]
                [ 0; 0; 0; 4; ]
            ];
            GameSIMDBranchless.Constants.flipTransposeMap
        |]
    }
[<Theory; MemberData(nameof rotateData)>]
let ``rotate`` cells expected map =
    let convert = GameSIMDBranchless.Board.fromList
    let cellsArray = convert cells 
    rotateDirection cellsArray map
    let expectedArray = convert expected
    Assert.Equal<int16[]>(expectedArray, cellsArray)

let swipeData : obj[] seq =
    seq {
        yield 
            [| [ 
                [ 1; 1; 0; 0 ]
                [ 1; 1; 0; 0 ]
                [ 1; 1; 0; 0 ]
                [ 1; 1; 0; 0 ] ];
            [
                [ 2; 0; 0; 0; ]
                [ 2; 0; 0; 0; ]
                [ 2; 0; 0; 0; ]
                [ 2; 0; 0; 0; ]
            ] |]
        yield 
            [| [ 
                [ 1; 0; 1; 0 ]
                [ 1; 1; 0; 1 ]
                [ 1; 1; 0; 0 ]
                [ 1; 0; 0; 0 ] ];
            [
                [ 2; 0; 0; 0; ]
                [ 2; 1; 0; 0; ]
                [ 2; 0; 0; 0; ]
                [ 1; 0; 0; 0; ]
            ] |]
    }
[<Theory; MemberData(nameof swipeData)>]
let ``swipe correctly merges cells`` cells expected =
    let board = GameSIMDBranchless.Board.create 4
    let expected = GameSIMDBranchless.Board.fromList expected
    Array.blit (GameSIMDBranchless.Board.fromList cells) 1 board.Cells 1 16
    swipe &board Left |> ignore
    board.Cells.ShouldBeEquivalentTo(expected)
    // try
        // Assert.Equal<int16[]>(expected, board.Cells)
    // with
    // | e ->
        // printfn "board: %A" board.Cells
        // printfn "expected: %A" expected
        // raise e