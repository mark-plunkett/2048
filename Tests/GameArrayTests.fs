module GameArrayTests

open System

open Xunit
open FsCheck
open FsCheck.Xunit

open Common
open GameArray

let posToIndexData : obj[] seq =
    seq {
        yield [| 4; { X = 0; Y = 0 }; 0 |]
        yield [| 4; { X = 2; Y = 0 }; 2 |]
        yield [| 4; { X = 2; Y = 1 }; 6 |]
        yield [| 4; { X = 3; Y = 3 }; 15 |]
    }
[<Theory; MemberData(nameof posToIndexData)>]
let ``posToIndex returns correct index`` size pos expected =
    let i = Board.posToIndex size pos
    Assert.Equal(expected, i)

[<Fact>]
let ``randomPos always returns in bounds X and Y`` () =
    let board = Board.empty 0
    let randomPositions = List.map (fun _ -> Board.randomPos board) [0..1000]
    Assert.All (randomPositions, (fun p -> Assert.InRange(p.X, 0, 3)))
    Assert.All (randomPositions, (fun p -> Assert.InRange(p.Y, 0, 3)))

[<Fact>]
let ``create returns board with two non zero cells`` () =
    let board = Board.create 0
    let nonZeros = board.Cells |> Array.filter (fun v -> v > 0us)
    Assert.Equal(2, nonZeros.Length)

let canAnyCellsMoveValues : obj[] seq =
    seq {
        yield [| [0;0;0;0]; true |]
        yield [| [2;4;8;16]; false |]
        yield [| [2;0;8;16]; true |]
        yield [| [2;2;8;16]; true |]
        yield [| [2;4;2;4]; false |]
        yield [| [2;4;2;0]; true |]
    }

//[<Theory; MemberData("canAnyCellsMoveValues")>]
let ``canAnyCellsMove`` cells expected =
    let result = GameFunctional.canAnyCellsMove cells
    Assert.Equal(expected, result)

//[<Property>]
let ``Game.flatten sum of cells same pre and post`` (cells: int list) =
    List.sum cells = (GameFunctional.flatten cells |> fst |> List.sum)

//[<Property>]
let ``Game.flatten two cells with same value should result in one value with twice the value`` cellValue =
    List.replicate 2 cellValue |> GameFunctional.flatten |> fst = [cellValue * 2]
