module SegmentExtractionRemoveMultilineStringsTests

open Xunit
open badreuse2.SegmentExtraction

[<Fact>]
let ``Remove simple multi-line string`` () =
    let input = "let x = \"\"\"This is a multi-line string\nspanning multiple lines\"\"\""
    let expected = "let x = \"\""
    let actual = removeMultilineStrings input
    Assert.Equal(expected, actual)

[<Fact>]
let ``Remove multiple multi-line strings`` () =
    let input = "let x = \"\"\"First string\nspanning lines\"\"\"\nlet y = \"\"\"Second string\nalso spans lines\"\"\""
    let expected = "let x = \"\"\nlet y = \"\""
    let actual = removeMultilineStrings input
    Assert.Equal(expected, actual)

[<Fact>]
let ``Keep normal strings untouched`` () =
    let input = "let x = \"normal string\""
    let expected = "let x = \"normal string\""
    let actual = removeMultilineStrings input
    Assert.Equal(expected, actual)

[<Fact>]
let ``Handle empty multi-line string`` () =
    let input = "let x = \"\"\"\"\"\""
    let expected = "let x = \"\""
    let actual = removeMultilineStrings input
    Assert.Equal(expected, actual)

[<Fact>]
let ``Remove multi-line string with escaped quotes`` () =
    let input = "let x = \"\"\"This is a multi-line string\nspanning multiple lines\"\"\""
    let expected = "let x = \"\""
    let actual = removeMultilineStrings input
    Assert.Equal(expected, actual)
