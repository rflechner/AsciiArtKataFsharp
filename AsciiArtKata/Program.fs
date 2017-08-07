
open System
open Generator

[<EntryPoint>]
let main argv =
  let coordinates = computeCoordinates()

  for arg in argv do
    drawString' coordinates arg

  0 // return an integer exit code
