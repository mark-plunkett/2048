module Tests

open System

open Xunit
open FsCheck
open FsCheck.Xunit

let canAnyCellsMoveValues : obj[] seq =
    seq {
        yield [| [0;0;0;0]; true |]
        yield [| [2;4;8;16]; false |]
        yield [| [2;0;8;16]; true |]
        yield [| [2;2;8;16]; true |]
        yield [| [2;4;2;4]; false |]
        yield [| [2;4;2;0]; true |]
    }

[<Theory; MemberData("canAnyCellsMoveValues")>]
let ``canAnyCellsMove`` cells expected =
    let result = GameFunctional.canAnyCellsMove cells
    Assert.Equal(expected, result)

[<Property>]
let ``Game.flatten sum of cells same pre and post`` (cells: int list) =
    List.sum cells = (GameFunctional.flatten cells |> fst |> List.sum)

[<Property>]
let ``Game.flatten two cells with same value should result in one value with twice the value`` cellValue =
    List.replicate 2 cellValue |> GameFunctional.flatten |> fst = [cellValue * 2]