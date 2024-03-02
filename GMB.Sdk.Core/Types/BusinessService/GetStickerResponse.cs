using GMB.Sdk.Core.Types.Models;

namespace GMB.Sdk.Core.Types.Api
{
    public sealed class GetStickerResponse : GenericResponse<GetStickerResponse>
    {
        public GetStickerResponse(Sticker? sticker)
        {
            Sticker = sticker;
        }

        // DO NOT USE THIS CONSTRUCTOR
        public GetStickerResponse() { }

        public Sticker? Sticker { get; set; }
    }
}