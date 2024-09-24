using GMB.Sdk.Core.Types.Api;
using Sdk.Core.Types.Models;

namespace Sdk.Core.Types.Api
{
    public sealed class GetStickerResponse : GenericResponse<GetStickerResponse>
    {
        public GetStickerResponse(int requestId, DbSticker? sticker)
        {
            RequestId = requestId;
            Sticker = sticker;
        }

        // DO NOT USE THIS CONSTRUCTOR
        public GetStickerResponse() { }

        public DbSticker? Sticker { get; set; }
        public int RequestId { get; set; } = -500;
    }
}