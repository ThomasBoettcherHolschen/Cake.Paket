﻿using Cake.Core.Tooling;

namespace Cake.Paket.Addin.Push
{
    /// <summary>
    /// Contains settings used by <see cref="PaketPusher"/>. See <see
    /// href="https://fsprojects.github.io/Paket/paket-push.html">Paket Push</see> for more details.
    /// </summary>
    public sealed class PaketPushSettings : ToolSettings
    {
        /// <summary>
        /// Gets or sets the apikey.
        /// </summary>
        public string ApiKey { get; set; }

        /// <summary>
        /// Gets or sets the endpoint.
        /// </summary>
        public string EndPoint { get; set; }

        /// <summary>
        /// Gets or sets the url.
        /// </summary>
        public string Url { get; set; }
    }
}