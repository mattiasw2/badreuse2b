module badreuse2.TokenizeTests

open Xunit
open badreuse2.tokenize

[<Fact>]
let ``Tokenize simple code`` () =
    let simpleCode = """
    let x = 1
    let y = 2
    printfn "%d" (x + y)
    """

    let actualTokens = tokensOfCode simpleCode
    // printfn "Actual tokens: %A" actualTokens
    let expectedTokens = [|
        "printfn"
    |]

    Assert.Equal<string array>(expectedTokens, actualTokens)

[<Fact>]
let ``Tokenize sample code correctly`` () =
    let sampleCode = """public async Task<bool> WaitForPendingOperationsAsync(string indexName)
{
    Console.WriteLine("Checking for pending operations...");
    
    for (int i = 0; i < 10; i++) // Try for up to 10 seconds
    {
        // Force a refresh to ensure all operations are visible
        var response = await httpClient.PostAsync($"/{indexName}/_refresh", null);
        var responseContent = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            Console.WriteLine($"Failed to refresh index: {response.StatusCode}");
            return false;
        }
    }
}"""

    let actualTokens = tokensOfCode sampleCode
    // printfn "Actual tokens: %A" actualTokens
    let expectedTokens = [|
      "checking"; "console"; "content"; "ensure"; "failed"; "httpclient";
      "indexname"; "issuccessstatuscode"; "operations"; "pending"; "postasync";
      "readasstringasync"; "refresh"; "response"; "responsecontent"; "seconds";
      "statuscode"; "string"; "visible"; "waitforpendingoperationsasync";
      "writeline"
    |]

    Assert.Equal<string array>(expectedTokens, actualTokens)
