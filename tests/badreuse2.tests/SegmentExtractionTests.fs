module SegmentExtractionTests

open Xunit
open badreuse2.SegmentExtraction
open badreuse2

let GetSegmentTexts (segments: SegmentInfo list) : List<string> = 
    segments |> List.map (fun s -> s.SegmentText) 

[<Fact>]
let ``Extract segments from file content with multiple segments`` () =
    let fileContent = "\nmodule Test\n\nlet x = 1\n\nlet y = 2\n"
    let expected = ["module Test";"let x = 1"; "let y = 2\n"]  
    let actual = extractSegments "filename.fs" fileContent
    Assert.Equal<List<string>>(expected, GetSegmentTexts actual)

[<Fact>]
let ``Extract segments from file content with no segments`` () =
    let fileContent = "\n\n\n"
    let expected = [] 
    let actual = extractSegments "filename.fs" fileContent
    Assert.Equal<List<string>>(expected, GetSegmentTexts actual)

[<Fact>]
let ``Extract segments from file content with indented lines`` () =
    let fileContent = "\nmodule Test\n\n    let x = 1\n\n    let y = 2\n"
    let expected = ["module Test\n\n    let x = 1\n\n    let y = 2\n"] 
    let actual = extractSegments "filename.fs" fileContent
    Assert.Equal<List<string>>(expected, GetSegmentTexts actual)

[<Fact>]
let ``Extract segments from file content with multi-line string`` () =
    let fileContent = "\nlet teststring = \"\"\"This is a multi-line string\nspanning multiple lines\"\"\"\n"
    printf "*** Do we see any backslashes in %s" fileContent
    let expected = ["let teststring = \"\"\n"]
    let actual = extractSegments "filename.fs" fileContent
    Assert.Equal<List<string>>(expected, GetSegmentTexts actual)

[<Fact>]
let ``Extract segments from code with embedded multi-line string`` () =
    let fileContent = "\nmodule Test\n\nlet x = \"\"\"This is a multi-line string\nspanning multiple lines\"\"\"\n\nlet y = 2\n"
    let expected = ["module Test"; "let x = \"\""; "let y = 2\n"]
    let actual = extractSegments "filename.fs" fileContent
    Assert.Equal<List<string>>(expected, GetSegmentTexts actual)

[<Fact>]
let ``Extract segments from code with multi-line comment`` () =
    let fileContent = "\nmodule Test\n\n(* This is a multi-line comment\n   spanning multiple lines *)\n\nlet y = 2\n"
    let expected = ["module Test"; "let y = 2\n"]
    let actual = extractSegments "filename.fs" fileContent
    Assert.Equal<List<string>>(expected, GetSegmentTexts actual)
