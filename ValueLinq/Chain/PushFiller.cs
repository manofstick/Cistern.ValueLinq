namespace Cistern.ValueLinq.Chain;

static class PushFiller
{
    // the chain initialised once filled, before any push opens its source, and then started. Inline and unprotected:
    // setup built here (Except's set) is what Dynamic PGO optimises the loop over, and neither a handler around it nor
    // building it out of line keeps that. A throw leaves the chain unstarted, holding nothing (an aggregator acquires in
    // its Initialise, the last to run), and each Guard's node releases what it held
    internal static TPushEnumerator Fill<TPushEnumerator, TFiller>(in TFiller filler)
        where TPushEnumerator : IPushEnumerator
        where TFiller : IPushFiller<TPushEnumerator>, allows ref struct
    {
        var chain = default(TPushEnumerator)!;
        filler.Fill(ref chain);
        if (TPushEnumerator.AllowInitialise)
            chain.Initialise();
        filler.Started();
        return chain;
    }
}
