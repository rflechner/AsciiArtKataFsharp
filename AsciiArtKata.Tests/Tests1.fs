#if INTERACTIVE

#r "../packages/System.Console/lib/net46/System.Console.dll"
#I "../packages/Expecto/lib/net461"
#r "Expecto.dll"

#load "../AsciiArtKata/Generator.fs"

#endif

open Expecto
open Generator

let liveTests =
  testList "Transforming typography in blocks" [
    
    test "When computing max line length" {
      Expect.equal maxlineLength 229 "then result should be 229"
    }

    test "When checking if lines contains error of length" {
      Expect.equal linesContainsErrorOfLength false "linesContainsErrorOfLength should be false"
    }

    test "When computing letter's content" {
      Expect.equal numberOfLetters 26 "then number of letters should be 26"
    }

    test "When computing letter's coordinates" {
      let coordinates = computeCoordinates()
      Expect.equal coordinates.Count 26 "then number of letters should be 26"

      for c in ['a' .. 'z'] do
        let message = sprintf "letter '%c' should be mapped" c
        Expect.isTrue (coordinates.ContainsKey c) message
    }

    test "When getting coordinates of letters" {
      let coordinates = computeCoordinates()
      
      let letterA = coordinates |> getLetter 'a'
      Expect.isSome letterA "then 'a' should be found in typography"

      let underscore = coordinates |> getLetter '_'
      Expect.isNone underscore "then '_' should be found in typography"
    }

    test "When computing the average width of the letters" {
      let avg = computeCoordinates() |> getAvgLetterSize
      Expect.equal avg 8 "then it should be 8"
    }

  ]

runTests defaultConfig liveTests

#if !INTERACTIVE

printfn "Press any key to qui ..."
System.Console.ReadKey true

#endif
