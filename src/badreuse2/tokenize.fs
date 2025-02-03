module badreuse2.tokenize

open System.Text.RegularExpressions

// A sample normalization function
let reservedKeywordsOfFsharp = Set.ofList [
    // Basic keywords
    "abstract"; "and"; "as"; "assert"; "base"; "begin"; "class"; "default"; "delegate"
    "do"; "done"; "downcast"; "downto"; "elif"; "else"; "end"; "exception"; "extern"
    "false"; "finally"; "fixed"; "for"; "fun"; "function"; "global"; "if"; "in"; "inherit"
    "inline"; "interface"; "internal"; "lazy"; "let"; "match"; "member"; "module"; "mutable"
    "namespace"; "new"; "not"; "null"; "of"; "open"; "or"; "override"; "private"; "protected"
    "public"; "rec"; "return"; "select"; "static"; "struct"; "then"; "to"; "true"; "try"
    "type"; "upcast"; "use"; "val"; "void"; "when"; "while"; "with"; "yield"
    // Composite keywords
    "and!"; "let!"; "match!"; "return!"; "use!"
    // Special operators as words
    "atomic"; "break"; "checked"; "component"; "const"; "constraint"; "constructor"
    "continue"; "eager"; "event"; "external"; "functor"; "include"; "method"
    "mixin"; "object"; "parallel"; "process"; "protected"; "pure"; "sealed"; "tailcall"
    "trait"; "virtual"; "volatile"
]

// Split the code line into tokens, get rid of short ones, generic ones like reserved keywords and short variable names
let tokensOfCodeLine (token: string) : string array =
    token
    |> fun t -> t.ToLowerInvariant()                          // Lowercase
    |> fun t -> Regex.Replace(t, "[^a-z0-9]", " ")              // Remove punctuation (keeping alphanumerics)
    |> fun t -> t.Split([|' '|], System.StringSplitOptions.RemoveEmptyEntries)
    |> Array.collect (fun part -> 
           // Optionally, split camelCase or snake_case parts further.
           // This is a simplified regex for splitting camelCase:
           Regex.Split(part, "(?<!^)(?=[A-Z])")
       )
    |> Array.map (fun part -> Regex.Replace(part, @"\d+$", ""))  // Remove trailing digits
    |> Array.filter (fun part -> part.Length > 5 && not (Set.contains part reservedKeywordsOfFsharp))
    |> Array.map (fun part -> 
           // Optionally apply a stemmer here.
           // For example, use a Porter stemmer library if available.
           part
       )

// Split the code into tokens, get rid of short ones, generic ones like reserved keywords and short variable names
let tokensOfCode (code: string) : string array =
    // Split code into tokens based on whitespace and common delimiters,
    // then normalize each token.
    code.Split([|' '; '\n'; '\r'; '\t'|], System.StringSplitOptions.RemoveEmptyEntries)
    |> Array.collect tokensOfCodeLine
    |> Array.sort
    |> Array.distinct
