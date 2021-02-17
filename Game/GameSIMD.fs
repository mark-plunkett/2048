module SIMD

    open System.Numerics

    open Common

    let arraysEqual (a:uint16[]) (b:uint16[]) =
        Vector.EqualsAll(Vector(a), Vector(b))

    let swipe (a:uint16[]) =
        let vOrig = Vector(a, 1)
        let vLShift = Vector(a, 2)
        let vMatches = Vector.Equals(vOrig, vLShift)
        let vOrigMatches = Vector.ConditionalSelect(vMatches, vOrig, Vector<uint16>.Zero)
        let vDoubleOrigMatches = Vector.Multiply(Vector(2us), vOrigMatches)
        let vRShift = Vector(a)
        let vRShiftEqualsOrig = Vector.Equals(vOrig, vRShift)
        let vRShiftMatches = Vector.Xor(vRShiftEqualsOrig, Vector<uint16>.One)
        let vZeroedOrig = Vector.Multiply(vOrig, vRShiftMatches)
        let vResult = Vector.Max(vDoubleOrigMatches, vZeroedOrig)
        vResult.CopyTo(a, 1)
        a
    
    let pack (a:uint16[]) =
        a

    let swipeSIMD (board:Board<uint16[]>) direction =
        GameArray.rotateDirection board.Cells direction

        let cells' =
            pack board.Cells
            |> swipe
            |> pack

        { board with Cells = cells' }

