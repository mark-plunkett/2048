module Tests

open System
open Xunit

[<Fact>]
let ``My test`` () =
    Assert.True(true)

let cellsCanMoveValues : obj[] seq =
    seq {
        yield [| [0;0;0;0]; true |]
        yield [| [2;4;8;16]; false |]
        yield [| [2;0;8;16]; true |]
        yield [| [2;2;8;16]; true |]
        yield [| [2;4;2;4]; false |]
        yield [| [2;4;2;0]; true |]
    }

[<Theory; MemberData("cellsCanMoveValues")>]
let ``cellsCanMove`` cells expected =
    let result = Game.canAnyCellsMove cells
    Assert.Equal(expected, result)