module GameArrayTests

open System

open Xunit
open FsCheck
open FsCheck.Xunit

open Common
open GameArray

let posToIndexData : obj[] seq =
    seq {
        yield [| 4; 0;0; 0 |]
        yield [| 4; 2;0; 2 |]
        yield [| 4; 2;1; 6 |]
        yield [| 4; 3;3; 15 |]
    }
[<Theory; MemberData(nameof posToIndexData)>]
let ``xyToIndex returns correct index`` size x y expected =
    let i = Board.xyToIndex size x y
    Assert.Equal(expected, i)

[<Fact>]
let ``randomPos always returns in bounds X and Y`` () =
    let board = Board.empty 4
    let randomPositions = List.map (fun _ -> Board.randomPos board) [0..1000]
    Assert.All (randomPositions, (fun p -> Assert.InRange(p.X, 0, 3)))
    Assert.All (randomPositions, (fun p -> Assert.InRange(p.Y, 0, 3)))

[<Fact>]
let ``create returns board with two non zero cells`` () =
    let board = Board.create 4
    let nonZeros = board.Cells |> Array.filter (fun v -> v > 0us)
    Assert.Equal(2, nonZeros.Length)

[<Fact>]
let ``fromList returns 1 dimensional array with expected values`` () =
    let cells = [
        [ 1; 2; 3; 4 ]
        [ 5; 6; 7; 8 ]
        [ 9; 10; 11; 12 ]
        [ 13; 14; 15; 16 ]
    ]

    let cells' = Board.fromList cells
    let expected = Array.map uint16 [|1;2;3;4;5;6;7;8;9;10;11;12;13;14;15;16|]
    Assert.Equal<uint16[]>(expected, cells')

let canSwipeHorizontalData : obj[] seq =
    seq {
        yield [| [
            [ 0; 2; 0; 0 ]
            [ 0; 0; 0; 0 ]
            [ 0; 0; 0; 0 ]
            [ 0; 0; 0; 0 ] ]; 
            true;
        |];
        yield [| [
            [ 2; 0; 0; 0 ]
            [ 0; 0; 0; 0 ]
            [ 0; 0; 0; 0 ]
            [ 0; 0; 0; 0 ] ]; 
            false;
        |]
        yield [| [
            [ 2; 2; 0; 0 ]
            [ 0; 0; 0; 0 ]
            [ 0; 0; 0; 0 ]
            [ 0; 0; 0; 0 ] ]; 
            true;
        |]
        yield [| [
            [ 0; 0; 0; 0 ]
            [ 2; 0; 0; 0 ]
            [ 0; 0; 0; 0 ]
            [ 0; 0; 0; 0 ] ]; 
            false;
        |]
        yield [| [
            [ 0; 0; 0; 0 ]
            [ 2; 2; 0; 0 ]
            [ 0; 0; 0; 0 ]
            [ 0; 0; 0; 0 ] ]; 
            true;
        |]
        yield [| [
            [ 2; 0; 0; 0 ]
            [ 2; 0; 0; 0 ]
            [ 0; 0; 0; 0 ]
            [ 0; 0; 0; 0 ] ]; 
            false;
        |]
        yield [| [
            [ 0; 0; 0; 0 ]
            [ 0; 2; 0; 2 ]
            [ 0; 0; 0; 0 ]
            [ 0; 0; 0; 0 ] ]; 
            true;
        |]
        yield [| [
            [ 0; 0; 0; 0 ]
            [ 0; 0; 0; 0 ]
            [ 0; 0; 0; 0 ]
            [ 0; 0; 0; 0 ] ]; 
            false;
        |]
        yield [| [
            [ 2; 4; 8; 8 ]
            [ 0; 0; 0; 0 ]
            [ 0; 0; 0; 0 ]
            [ 0; 0; 0; 0 ] ]; 
            true;
        |]
        yield [| [
            [ 2; 4; 8; 16 ]
            [ 0; 0; 0; 0 ]
            [ 0; 0; 0; 0 ]
            [ 0; 0; 0; 0 ] ]; 
            false;
        |]
    }
[<Theory; MemberData(nameof canSwipeHorizontalData)>]
let ``canSwipeHorizontal returns expected`` cells expected =
    let board = Board.create 4
    let board' = { board with Cells = cells |> Board.fromList }
    let canSwipe = canSwipe board'
    Assert.Equal(expected, canSwipe)

let flattenRowValues : obj[] seq =
    seq {
        yield [| [2;4;8;16]; [2;4;8;16] |]
        yield [| [0;0;0;0]; [0;0;0;0] |]
        yield [| [2;0;8;16]; [2;8;16;0] |]
        yield [| [2;2;8;16]; [4;8;16;0] |]
        yield [| [2;4;2;4]; [2;4;2;4] |]
        yield [| [2;4;2;0]; [2;4;2;0] |]
        yield [| [2;0;2;2]; [4;2;0;0] |]
    }
[<Theory; MemberData(nameof flattenRowValues)>]
let ``flattenRow works as expected`` row expected =
    let row' = Board.fromList [row]
    let expected' = Board.fromList [expected]
    flattenRow row'.Length 0 row' 
    // row' modified in place
    Assert.Equal<uint16[]>(expected', row')

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
    let result = swipe board' Left
    Assert.Equal<uint16[]>(expected, result.Cells)

//[<Property>]
let ``Game.flatten sum of cells same pre and post`` (cells: int list) =
    List.sum cells = (GameFunctional.flatten cells |> fst |> List.sum)

//[<Property>]
let ``Game.flatten two cells with same value should result in one value with twice the value`` cellValue =
    List.replicate 2 cellValue |> GameFunctional.flatten |> fst = [cellValue * 2]
