using Codexus.OpenSDK.Yggdrasil;
using Codexus.OpenSDK.Entities.Yggdrasil;
using Serilog;

namespace Tex.Type;

internal class Services(
    StandardYggdrasil Yggdrasil
    )
{ 
    public StandardYggdrasil Yggdrasil { get; private set; } = Yggdrasil;
}

