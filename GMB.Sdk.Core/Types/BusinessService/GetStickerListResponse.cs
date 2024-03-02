using GMB.Sdk.Core.Types.Models;

namespace GMB.Sdk.Core.Types.Api
{
    public sealed class GetStickerListResponse : GenericResponse<GetStickerListResponse>
    {
        public GetStickerListResponse(List<Sticker> sticker)
        {
            Sticker = sticker;
        }

        // DO NOT USE THIS CONSTRUCTOR
        public GetStickerListResponse() { }

        public List<Sticker> Sticker { get; set; }
    }
}