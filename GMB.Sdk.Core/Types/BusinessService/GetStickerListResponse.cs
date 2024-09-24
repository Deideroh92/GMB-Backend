using GMB.Sdk.Core.Types.Api;

namespace Sdk.Core.Types.Api
{
    public sealed class GetStickerListResponse : GenericResponse<GetStickerListResponse>
    {
        public GetStickerListResponse(List<GetStickerResponse>? stickerList)
        {
            StickerList = stickerList;
        }

        // DO NOT USE THIS CONSTRUCTOR
        public GetStickerListResponse() { }

        public List<GetStickerResponse>? StickerList { get; set; }
    }
}