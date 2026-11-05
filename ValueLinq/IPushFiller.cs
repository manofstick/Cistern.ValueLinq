namespace Cistern.ValueLinq;

/// <summary>
/// What a push hands its source in place of a built chain. Each level of the descent passes on a filler holding its
/// own state and a ref to the filler above; where the descent ends the chain is declared once and filled through
/// them (PushFiller.Fill), so no level copies the chain above it.
/// </summary>
interface IPushFiller<TPushEnumerator>
{
    void Fill(ref TPushEnumerator target);

    // the chain is filled and initialised, so the push running it owns it from here; passed on to the filler after,
    // for a Guard among them
    void Started();
}
