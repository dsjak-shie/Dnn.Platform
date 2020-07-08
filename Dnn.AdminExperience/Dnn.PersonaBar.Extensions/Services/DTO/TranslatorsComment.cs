﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Pages.Services.Dto
{
    using Newtonsoft.Json;

    [JsonObject]
    public class TranslatorsComment
    {
        public int TabId { get; set; }
        public string Text { get; set; }
    }
}
