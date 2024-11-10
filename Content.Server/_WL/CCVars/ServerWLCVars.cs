using Robust.Shared.Configuration;

namespace Content.Server._WL.CVars
{
    [CVarDefs]
    public sealed class ServerWLCVars
    {
        /*
          * Chat Gpt
          */
        /// <summary>
        /// Апи-ключ для авторизации запросов к ЭйАй.
        /// </summary>
        public static readonly CVarDef<string> GptApiKey =
            CVarDef.Create("gpt.api_key", "sk-RJOYKygcSQ4497Qlqt8i8ZixlgfmXncV", CVar.SERVERONLY | CVar.CONFIDENTIAL | CVar.SERVER);
    }
}
